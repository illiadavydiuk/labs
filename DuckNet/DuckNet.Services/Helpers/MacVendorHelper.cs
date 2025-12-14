using System.Collections.Generic;

namespace DuckNet.Services.Helpers
{
    public static class MacVendorHelper
    {
        // Це розширена база популярних OUI
        private static readonly Dictionary<string, string> Vendors = new Dictionary<string, string>
        {
            // Apple
            { "00:03:93", "Apple" }, { "00:05:02", "Apple" }, { "00:0A:27", "Apple" }, { "00:0A:95", "Apple" },
            { "00:0D:93", "Apple" }, { "00:11:24", "Apple" }, { "00:14:51", "Apple" }, { "00:16:CB", "Apple" },
            { "00:17:F2", "Apple" }, { "00:19:E3", "Apple" }, { "00:1B:63", "Apple" }, { "00:1C:27", "Apple" },
            { "00:1C:B3", "Apple" }, { "00:1D:4F", "Apple" }, { "00:1E:52", "Apple" }, { "00:1E:C2", "Apple" },
            { "88:AE:DD", "Apple" }, { "F4:5C:89", "Apple" }, { "AC:BC:32", "Apple" }, { "DC:2B:2A", "Apple" },
            
            // Samsung
            { "00:02:78", "Samsung" }, { "00:07:AB", "Samsung" }, { "00:09:18", "Samsung" }, { "00:0D:AE", "Samsung" },
            { "00:12:47", "Samsung" }, { "00:13:77", "Samsung" }, { "00:15:99", "Samsung" }, { "00:15:B9", "Samsung" },
            { "50:E5:49", "Samsung" }, { "24:F5:AA", "Samsung" }, { "FC:C2:DE", "Samsung" }, { "8C:71:F8", "Samsung" },

            // Xiaomi
            { "F0:D5:BF", "Xiaomi" }, { "AC:F7:F3", "Xiaomi" }, { "64:CC:2E", "Xiaomi" }, { "14:F6:5A", "Xiaomi" },
            { "00:EC:0A", "Xiaomi" }, { "04:CF:8C", "Xiaomi" }, { "0C:1D:AF", "Xiaomi" }, { "10:2A:B3", "Xiaomi" },

            // TP-Link
            { "BC:D0:74", "TP-Link" }, { "50:C7:BF", "TP-Link" }, { "AC:84:C6", "TP-Link" }, { "14:CC:20", "TP-Link" },
            { "18:A6:F7", "TP-Link" }, { "18:D6:C7", "TP-Link" }, { "1C:FA:68", "TP-Link" }, { "00:0A:EB", "TP-Link" },

            // Huawei
            { "00:18:82", "Huawei" }, { "00:19:E0", "Huawei" }, { "00:1E:10", "Huawei" }, { "00:22:A1", "Huawei" },
            { "00:25:9E", "Huawei" }, { "00:46:4B", "Huawei" }, { "00:66:4B", "Huawei" }, { "00:E0:FC", "Huawei" },

            // Intel (мережеві карти в ПК)
            { "00:02:B3", "Intel" }, { "00:03:47", "Intel" }, { "00:04:23", "Intel" }, { "00:07:E9", "Intel" },
            { "00:0C:F1", "Intel" }, { "00:0E:35", "Intel" }, { "00:11:11", "Intel" }, { "00:12:F0", "Intel" },

            // Cisco
            { "00:00:0C", "Cisco" }, { "00:01:42", "Cisco" }, { "00:01:43", "Cisco" }, { "00:01:63", "Cisco" },
            
            // Virtual Machines
            { "00:0C:29", "VMware" }, { "00:50:56", "VMware" }, { "00:1C:14", "VMware" },
            { "00:15:5D", "Microsoft Hyper-V" }, { "08:00:27", "VirtualBox" },

            // Raspberry Pi
            { "B8:27:EB", "Raspberry Pi" }, { "DC:A6:32", "Raspberry Pi" }, { "E4:5F:01", "Raspberry Pi" },

            // Generic / Common PC brands
            { "FC:AA:14", "GigaByte" }, { "18:31:BF", "ASUS" }, { "04:D9:F5", "ASUS" },
            { "00:14:22", "Dell" }, { "F0:4D:A2", "Dell" }, { "D8:9E:F3", "HP" }, { "3C:D9:2B", "HP" }
        };

        public static string GetVendor(string mac)
        {
            if (string.IsNullOrEmpty(mac) || mac.Length < 8) return "Unknown";
            // Беремо префікс (перші 3 байти)
            string prefix = mac.Substring(0, 8).ToUpper();
            return Vendors.TryGetValue(prefix, out string vendor) ? vendor : "Unknown Vendor";
        }
    }
}