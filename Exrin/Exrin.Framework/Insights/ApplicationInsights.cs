﻿namespace Exrin.Insights
{
    using Abstraction;
    using Common;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class ApplicationInsights : IApplicationInsights
    {

        private readonly INavigationReadOnlyState _navigationState = null;
        private readonly IDeviceInfo _deviceInfo = null;
        private string _userId = null;
        private string _fullName = null;
        private static string _sessionId = Guid.NewGuid().ToString(); // Once per application load
        private readonly IBlockingQueue<IInsightData> queue = new BlockingQueue<IInsightData>();

        public ApplicationInsights(IDeviceInfo deviceInfo, INavigationReadOnlyState navigationState)
        {
            _deviceInfo = deviceInfo;
            _navigationState = navigationState;
        }

        public Task<IBlockingQueue<IInsightData>> GetQueue()
        {
            return Task.FromResult(queue);
        }

        public void SetIdentity(string userId, string fullName)
        {
            _userId = userId;
            _fullName = fullName;
        }

        /// <summary>
        /// Used to fill in the extra details into the insights data before storage.
        /// </summary>
        /// <param name="data"></param>
        private async Task FillData(IInsightData data)
        {
            data.Created = DateTime.UtcNow;
            data.AppVersion = CleanVersion(DefaultHelper.GetOrDefault(_deviceInfo.GetAppVersion, new Version("0.0.0.0")));
            data.Battery = await DefaultHelper.GetOrDefaultAsync(_deviceInfo.GetBattery, null);
            data.ConnectionStrength = await DefaultHelper.GetOrDefaultAsync(_deviceInfo.GetConnectionStrength, null);
            data.ConnectionType = DefaultHelper.GetOrDefault(_deviceInfo.GetConnectionType, ConnectionType.Unknown);
            data.DeviceIdentifier = DefaultHelper.GetOrDefault(_deviceInfo.GetUniqueId, "");
            data.FullName = _fullName;
            data.Id = Guid.NewGuid();
            data.IPAddress = await DefaultHelper.GetOrDefaultAsync(_deviceInfo.GetIPAddress, "");
            data.Model = await DefaultHelper.GetOrDefaultAsync(_deviceInfo.GetModel, "");
            data.OSVersion = CleanVersion(await DefaultHelper.GetOrDefaultAsync(_deviceInfo.GetOSVersion, new Version("0.0.0.0")));
            data.SessionId = _sessionId;
            data.UserId = _userId;
        }

        private Version CleanVersion(Version version)
        {
            return new Version($"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}".Replace("-1", "0"));
        }

        /// <summary>
        /// Used to fill out the Insight Data with variables that must be obtained
        /// before the execution of the app continues.
        /// </summary>
        /// <param name="data"></param>
        private Task FillInThreadData(IInsightData data)
        {
            data.ViewName = _navigationState.ViewName;

            return Task.FromResult(true);
        }


        public async Task TrackMetric(string category, object value, [CallerMemberName] string callerName = "")
        {
            try
            {
                var data = new InsightData()
                {
                    Category = InsightCategory.Metric,
                    CustomMarker = category,
                    CustomValue = value,
                    CallerName = callerName
                };

                await FillInThreadData(data);

                Finalize(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void Finalize(IInsightData data)
        {
            // Finalize in background thread, no need to keep the app waiting.
            Task.Run(async () =>
            {
                await FillData(data);

                Store(data);
            });
        }

        public async Task TrackException(Exception exception, [CallerMemberName] string callerName = "")
        {
            try
            {
                var data = new InsightData()
                {
                    Category = InsightCategory.Exception,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    CallerName = callerName
                };

                await FillInThreadData(data);

                Finalize(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task TrackEvent(string eventName, string message, [CallerMemberName] string callerName = "")
        {
            try
            {
                var data = new InsightData()
                {
                    Category = InsightCategory.Event,
                    Message = message,
                    CustomMarker = eventName,
                    CallerName = callerName
                };

                await FillInThreadData(data);

                Finalize(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Store event in file for transmission
        /// </summary>
        /// <param name="data"></param>
        private void Store(IInsightData data)
        {
            try
            {
                queue.Enqueue(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public Task TrackRaw(IInsightData data)
        {
            try
            {
                Store(data);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Task.FromResult(false);
            }
        }
    }
}
