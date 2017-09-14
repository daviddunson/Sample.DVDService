// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="GSD Logic">
//   Copyright © 2017 GSD Logic. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ExperimentalDVR
{
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;

    internal class Program
    {
        private static void Main(string[] args)
        {
            ////var providers = "http://mobilelistings.tvguide.com/Listingsweb/ws/rest/serviceproviders/zipcode/35749?formattype=json";
            ////var otaThurSep14_1500 = "http://mobilelistings.tvguide.com/Listingsweb/ws/rest/schedules/20471.268435456/start/1505419200/duration/120?ChannelFields=Name%2CFullName%2CNumber%2CSourceId&ScheduleFields=ProgramId%2CEndTime%2CStartTime%2CTitle%2CAiringAttrib%2CCatId&formattype=json&disableChannels=music%2Cppv%2C24hr";
            ////var otaFriSep15_1500 = "http://mobilelistings.tvguide.com/Listingsweb/ws/rest/schedules/20471.268435456/start/1505505600/duration/120?ChannelFields=Name%2CFullName%2CNumber%2CSourceId&ScheduleFields=ProgramId%2CEndTime%2CStartTime%2CTitle%2CAiringAttrib%2CCatId&formattype=json&disableChannels=music%2Cppv%2C24hr";
            ////var otaFriSep15_1900 = "http://mobilelistings.tvguide.com/Listingsweb/ws/rest/schedules/20471.268435456/start/1505520000/duration/120?ChannelFields=Name%2CFullName%2CNumber%2CSourceId&ScheduleFields=ProgramId%2CEndTime%2CStartTime%2CTitle%2CAiringAttrib%2CCatId&formattype=json&disableChannels=music%2Cppv%2C24hr";
            ////var wowFriSep15_1900 = "http://mobilelistings.tvguide.com/Listingsweb/ws/rest/schedules/63127.1/start/1505520000/duration/120?ChannelFields=Name%2CFullName%2CNumber%2CSourceId&ScheduleFields=ProgramId%2CEndTime%2CStartTime%2CTitle%2CAiringAttrib%2CCatId&formattype=json&disableChannels=music%2Cppv%2C24hr";
            ////return;

            using (var client = new HttpClient())
            {
                using (var sourceStream = client.GetStreamAsync("http://192.168.1.126:5004/auto/v48.1").Result)
                {
                    using (var targetStream = new FileStream("Test.mpeg", FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        var stopwatch = Stopwatch.StartNew();
                        var buffer = new byte[1024];
                        var bytesRead = 0;

                        while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            targetStream.Write(buffer, 0, bytesRead);

                            if (stopwatch.ElapsedMilliseconds > 10000)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}