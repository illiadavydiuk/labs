using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DuckNet.Services.Helpers
{
    public static class TracerouteHelper
    {
        public static async Task<IEnumerable<string>> TraceRoute(string hostNameOrAddress)
        {
            var results = new List<string>();
            using (var pinger = new Ping())
            {
                PingOptions options = new PingOptions(1, true);
                int timeout = 1000;
                byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

                // Максимум 30 стрибків
                for (int ttl = 1; ttl <= 30; ttl++)
                {
                    options.Ttl = ttl;
                    try
                    {
                        PingReply reply = await pinger.SendPingAsync(hostNameOrAddress, timeout, buffer, options);

                        string route = $"{ttl}\t{reply.Status}\t{reply.RoundtripTime}ms\t{reply.Address}";
                        results.Add(route);

                        if (reply.Status == IPStatus.Success)
                            break;
                    }
                    catch
                    {
                        results.Add($"{ttl}\tTimeout\t*");
                    }
                }
            }
            return results;
        }
    }
}