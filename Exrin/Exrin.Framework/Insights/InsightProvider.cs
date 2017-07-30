﻿namespace Exrin.Insights
{
    using Abstraction;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class InsightProvider : IInsightsProvider
	{

		private readonly IInsightStorage _storage = null;
		private readonly int _tick = 0;
		public InsightProvider(IInsightStorage storage, int tick = 300000) // Default 5 minutes
		{
			_storage = storage;
			_tick = tick;

			// Immediate Process and Start Timer
			if (tick > 0)
				Task.Run(Process);
		}

		private static object _lock = new object();
		private bool _isRunning = false;

		private Task Process()
		{
			var state = new object();
			new Timer(async (s) =>
			{
				lock (_lock)
				{
					if (_isRunning)
						return;

					_isRunning = true;
				}
				try
				{
					var insights = await _storage.ReadAllData();

                    var deleteList = new List<IInsightData>();
                    var list = await Send(insights);
                    foreach (var data in list.ToList())
                        deleteList.Add(data);
                    
                    foreach (var item in deleteList)
                        await _storage.Delete(item);
                }
				catch (Exception ex) { Debug.WriteLine(ex.Message); }
				finally
				{
					_isRunning = false;
				}
			}, state, 0, _tick, true); // Instant first run

			return Task.FromResult(true);
		}


		/// <summary>
		/// Will send the insight data to the insight provider
		/// </summary>
		/// <param name="data"></param>
		/// <returns>The successfully sent insight data</returns>
		protected abstract Task<IList<IInsightData>> Send(IList<IInsightData> insights);

		/// <summary>
		/// Saves to storage for later processing.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public virtual async Task Record(IInsightData data)
		{
			await _storage.Write(data);
		}

	}
}
