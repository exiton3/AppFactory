﻿using System;
using System.IO;
using System.Reflection;

namespace AppFactory.Framework.Domain.Infrastructure
{
    public static class AppVersion
    {
        /// <summary>
        /// Gets the current build number
        /// </summary>
        public static string GetAppVersion()
        {
            return GetMainAssemblyVersion().ToString();
        }

        /// <summary>
        /// Gets the currect build date (see http://stackoverflow.com/questions/1600962/displaying-the-build-date)
        /// </summary>
        public static DateTime GetAppBuildDate()
        {
            var linkTimeLocal = Assembly.GetExecutingAssembly().GetLinkerTime();
            return linkTimeLocal;
        }
        

        private static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }

        private static Version GetMainAssemblyVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
    }

}