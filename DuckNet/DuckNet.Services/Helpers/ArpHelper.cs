using System;
using System.Net;
using System.Runtime.InteropServices;

namespace DuckNet.Services.Helpers
{
    public static class ArpHelper
    {
        // Імпортуємо зовнішню функцію Windows API
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref int PhyAddrLen);

        public static string GetMacAddress(string ipAddress)
        {
            try
            {
                IPAddress dst = IPAddress.Parse(ipAddress);
                byte[] macAddr = new byte[6];
                int macAddrLen = macAddr.Length;

                // Викликаємо системну функцію ARP запиту
                int ret = SendARP(BitConverter.ToInt32(dst.GetAddressBytes(), 0), 0, macAddr, ref macAddrLen);

                if (ret != 0) return "Unknown"; // Якщо помилка або пристрій не відповів

                // Форматуємо байти у вигляд AA:BB:CC...
                string[] str = new string[macAddrLen];
                for (int i = 0; i < macAddrLen; i++)
                    str[i] = macAddr[i].ToString("X2");

                return string.Join(":", str);
            }
            catch
            {
                return "Error";
            }
        }
    }
}