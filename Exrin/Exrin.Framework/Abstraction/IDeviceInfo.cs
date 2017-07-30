﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exrin.Abstraction
{
    public interface IDeviceInfo
    {
        /// <summary>
        /// A string representing the version of the app in Major.Minor.Build
        /// </summary>
        /// <returns></returns>
        Version GetAppVersion();

        /// <summary>
        /// A string representing the model of the device
        /// </summary>
        /// <returns></returns>
        Task<string> GetModel();

        /// <summary>
        /// A string representing the OS and version
        /// </summary>
        /// <returns></returns>
        string GetOS();

        Task<Version> GetOSVersion();

        /// <summary>
        /// Percentage of how charged the battery is
        /// </summary>
        /// <returns></returns>
        Task<double?> GetBattery();

        Size GetScreenSize();

        ConnectionType GetConnectionType();

        Task<double?> GetConnectionStrength();

        /// <summary>
        /// Gets a unique Id of the application installation
        /// </summary>
        /// <returns></returns>
        string GetUniqueId();

        Task<string> GetIPAddress();
    }
}
