﻿namespace Exrin.Insights
{
    using Abstraction;
    using System;

    public class InsightData : IInsightData
    {
        
        public DateTime Created { get; set; }

        public string CallerName { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public string DeviceIdentifier { get; set; }

        public string FullName { get; set; }

        public Guid Id { get; set; }

        public string IPAddress { get; set; }

        public string Message { get; set; }

        public string Model { get; set; }

        public Version AppVersion { get; set; }

        public Version OSVersion { get; set; }

        public string StackTrace { get; set; } = "";

        public string UserId { get; set; }

        public double? Battery { get; set; }

        public double? ConnectionStrength { get; set; }

        public string SessionId { get; set; }

        public InsightCategory Category { get; set; }

        public string CustomMarker { get; set; }

        public object CustomValue { get; set; }

        public string ViewName { get; set; }
    }
}
