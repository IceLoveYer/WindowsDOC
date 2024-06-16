using System;

namespace WindowsDOC.Pages.NetworkConfig
{
    // 定义一个类来保存网络适配器信息
    public class AdapterInfo
    {
        // 属性声明
        public string Name { get; set; } // 网络适配器名称
        public string Description { get; set; } // 网络适配器描述
        public string NetConnectionID { get; set; } // 网络连接ID
        public string GUID { get; set; } // GUID
        public string MACAddress { get; set; } // MAC地址
        public bool IsDhcpEnabled { get; set; } // DHCP状态
        public string[] IPAddresses { get; set; } // IP地址数组
        public string[] Subnets { get; set; } // 子网掩码数组
        public string[] Gateways { get; set; } // 默认网关数组
        public string[] DNSServers { get; set; } // DNS服务器地址数组

        // 构造函数
        public AdapterInfo()
        {
            Name = string.Empty;
            Description = string.Empty;
            NetConnectionID = string.Empty;
            GUID = string.Empty;
            MACAddress = string.Empty;
            IPAddresses = Array.Empty<string>();
            Subnets = Array.Empty<string>();
            Gateways = Array.Empty<string>();
            DNSServers = Array.Empty<string>();
        }
    }
}
