using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WindowsDOC.Pages
{
    public partial class NetworkConfig : Page
    {
        public NetworkConfig()
        {
            InitializeComponent();

            LoadNetworkInterfaces();  // 初始化时加载网络接口信息
        }



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



        public void LoadNetworkInterfaces() // 加载网络接口信息并填充下拉框
        {
            try
            {
                List<AdapterInfo> adapterList = new();

                // 查询Win32_NetworkAdapter WMI类以获取所有网络适配器
                ManagementObjectSearcher adapterSearcher = new("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter = true");

                foreach (ManagementObject obj in adapterSearcher.Get().Cast<ManagementObject>())
                {
                    // 获取与物理适配器关联的配置
                    ManagementObjectSearcher configSearcher = new($"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index = {obj["Index"]}");
                    var config = configSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
                    if (config != null)
                    {
                        adapterList.Add(new AdapterInfo
                        {
                            Name = obj["Name"].ToString() ?? "",
                            Description = obj["Description"].ToString() ?? "",
                            NetConnectionID = obj["NetConnectionID"].ToString() ?? "",
                            GUID = obj["GUID"].ToString() ?? "",
                            MACAddress = config["MACAddress"].ToString() ?? "",
                            IPAddresses = config["IPAddress"] as string[] ?? Array.Empty<string>(),
                            Subnets = config["IPSubnet"] as string[] ?? Array.Empty<string>(),
                            Gateways = config["DefaultIPGateway"] as string[] ?? Array.Empty<string>(),
                            DNSServers = config["DNSServerSearchOrder"] as string[] ?? Array.Empty<string>(),
                            IsDhcpEnabled = (bool)config["DHCPEnabled"],
                        });
                    }

                }

                // 填充下拉框
                ComboBoxName.ItemsSource = adapterList;
                ComboBoxName.DisplayMemberPath = "NetConnectionID";

                // 如果网络接口列表不为空，选择第一个网络接口
                if (ComboBoxName.Items.Count > 0)
                {
                    ComboBoxName.SelectedItem = ComboBoxName.Items[0];
                }
            }
            catch (ManagementException e)
            {
                NotificationControl.Add("查询WMI数据时发生错误: " + e.Message);
            }
        }

        private void ComboBoxName_SelectionChanged(object sender, SelectionChangedEventArgs e) // 当选择的网络接口发生变化时触发
        {
            // 清空之前的信息
            IPAddressControlAddress.Clear();
            IPAddressControlMask.Clear();
            IPAddressControlGateway.Clear();
            IPAddressControlDnsPrimary.Clear();
            IPAddressControlDnsSecondary.Clear();

            // 判断选中的项是否为网络适配器信息
            if (ComboBoxName.SelectedItem is AdapterInfo selectedAdapter)
            {
                // 显示网络接口的描述信息
                TextBoxDescription.Text = selectedAdapter.Description;

                // 显示网络接口的MAC地址
                TextBoxMac.Text = selectedAdapter.MACAddress;


                // 获取IPv4的DHCP开启状态
                if (selectedAdapter.IsDhcpEnabled)
                {
                    RadioButtonDhcpOn.IsChecked = true; // 自动获取DHCP

                    // 获取DNS是否自动获取，这里只有开启了DHCP才会检查是否有手动设置的DNS服务器地址
                    string registryPath = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\" + selectedAdapter.GUID;
                    if (Microsoft.Win32.Registry.GetValue(registryPath, "NameServer", null) is string nameServer && string.IsNullOrEmpty(nameServer)) // NameServer不为空说明手动设置了DNS
                    {
                        RadioButtonDhcpDnsOn.IsChecked = true;  // 自动获取DNS
                    }
                    else
                    {
                        RadioButtonDhcpDnsOff.IsChecked = true;  // 手动设置DNS
                    }
                }
                else
                {
                    RadioButtonDhcpOff.IsChecked = true; // 手动设置DHCP

                    RadioButtonDhcpDnsOff.IsChecked = true;  // 手动设置DNS
                }

                // 显示IP地址和子网掩码
                IPAddressControlAddress.IPAddress = selectedAdapter.IPAddresses?.FirstOrDefault() ?? "";
                IPAddressControlMask.IPAddress = selectedAdapter.Subnets?.FirstOrDefault() ?? "";

                // 显示默认网关
                IPAddressControlGateway.IPAddress = selectedAdapter.Gateways?.FirstOrDefault() ?? "";

                // 显示DNS服务器地址
                IPAddressControlDnsPrimary.IPAddress = selectedAdapter.DNSServers?.FirstOrDefault() ?? "";
                IPAddressControlDnsSecondary.IPAddress = selectedAdapter.DNSServers?.Skip(1).FirstOrDefault() ?? "";
            }
        }


        private void RadioButtonDhcpOn_Checked(object sender, RoutedEventArgs e) // 禁用 IP地址、子网掩码、默认网关 编辑状态；开启DNS自动
        {
            IPAddressControlAddress.IsEnabled = false;
            IPAddressControlMask.IsEnabled = false;
            IPAddressControlGateway.IsEnabled = false;

            RadioButtonDhcpDnsOn.IsEnabled = true;
        }
        private void RadioButtonDhcpOn_Unchecked(object sender, RoutedEventArgs e) // 开启 IP地址、子网掩码、默认网关 编辑状态；禁用DNS自动、设置DNS手动
        {
            IPAddressControlAddress.IsEnabled = true;
            IPAddressControlMask.IsEnabled = true;
            IPAddressControlGateway.IsEnabled = true;

            RadioButtonDhcpDnsOn.IsEnabled = false;
            RadioButtonDhcpDnsOff.IsChecked = true;
        }
        private void RadioButtonDhcpOn_Click(object sender, RoutedEventArgs e) // 清空 IP地址、子网掩码、默认网关
        {
            IPAddressControlAddress.Clear();
            IPAddressControlMask.Clear();
            IPAddressControlGateway.Clear();
        }


        private void RadioButtonDhcpDnsOn_Checked(object sender, RoutedEventArgs e) // 禁用 首选、备选DNS服务器 编辑状态
        {
            IPAddressControlDnsPrimary.IsEnabled = false;
            IPAddressControlDnsSecondary.IsEnabled = false;
        }
        private void RadioButtonDhcpDnsOn_Unchecked(object sender, RoutedEventArgs e) // 开启 首选、备选DNS服务器 编辑状态
        {
            IPAddressControlDnsPrimary.IsEnabled = true;
            IPAddressControlDnsSecondary.IsEnabled = true;
        }
        private void RadioButtonDhcpDnsOn_Click(object sender, RoutedEventArgs e) // 清空 DNS服务器地址
        {
            IPAddressControlDnsPrimary.Clear();
            IPAddressControlDnsSecondary.Clear();
        }



        private void Button_Click_1(object sender, RoutedEventArgs e) // 打开 控制面板\所有控制面板项\网络连接
        {
            Process.Start("control", "ncpa.cpl");
        }



        private void Button_Click_2(object sender, RoutedEventArgs e) // 禁用网络适配器
        {
            if (ComboBoxName.SelectedItem is AdapterInfo selectedAdapter)  // 判断选中的项是否为网络接口
            {
                EnableDisableNetworkAdapter(selectedAdapter.GUID, false); // 禁用
            }
        }
        private void Button_Click_3(object sender, RoutedEventArgs e) // 启用网络适配器
        {
            if (ComboBoxName.SelectedItem is AdapterInfo selectedAdapter)  // 判断选中的项是否为网络接口
            {
                EnableDisableNetworkAdapter(selectedAdapter.GUID, true); // 启用
            }
        }
        static void EnableDisableNetworkAdapter(string? adapterGuid, bool enable)
        {
            if (adapterGuid == null) return;

            SelectQuery query = new("Win32_NetworkAdapter", "GUID='" + adapterGuid + "'");
            ManagementObjectSearcher search = new(query);
            foreach (ManagementObject result in search.Get().Cast<ManagementObject>())
            {
                if (enable)
                {
                    result.InvokeMethod("Enable", null);
                }
                else
                {
                    result.InvokeMethod("Disable", null);
                }
            }
        }



        private void Button_Click_4(object sender, RoutedEventArgs e) // 修改MAC
        {
            if (ComboBoxName.SelectedItem is AdapterInfo selectedAdapter)
            {
                // 正则表达式来验证MAC地址格式
                Regex Regex_MAC = new("^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", RegexOptions.Compiled);
                if (Regex_MAC.IsMatch(TextBoxMac.Text))
                {
                    ChangeMacAddressAndReset(TextBoxMac.Text, selectedAdapter.GUID);
                    NotificationControl.Add("MAC地址已更改。请检查网络适配器是否已重新启用。");

                }
                else
                {
                    NotificationControl.Add("无效的MAC地址格式。");
                }
            }
        }
        private static void ChangeMacAddressAndReset(string newMac, string adapterId)
        {
            if (String.IsNullOrEmpty(adapterId)) return;

            string registryPath = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002BE10318}";
            bool success = false;

            using (var rootKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryPath, true))
            {
                if (rootKey != null)
                {
                    foreach (string subkeyName in rootKey.GetSubKeyNames())
                    {
                        using var subkey = rootKey.OpenSubKey(subkeyName, true);
                        if (subkey != null && subkey.GetValue("NetCfgInstanceId")?.ToString() == adapterId)
                        {
                            subkey.SetValue("NetworkAddress", newMac.Replace(":", "").Replace("-", ""), Microsoft.Win32.RegistryValueKind.String);
                            success = true;
                            break;
                        }
                    }
                }
            }

            if (success)
            {
                // 重启网络适配器以应用更改
                ProcessStartInfo psi = new("netsh", $"netsh interface set interface \"{adapterId}\" disable");
                Process.Start(psi)?.WaitForExit();
                psi = new ProcessStartInfo("netsh", $"netsh interface set interface \"{adapterId}\" enable");
                Process.Start(psi)?.WaitForExit();
            }
        }



        private async void Button_Click_5(object sender, RoutedEventArgs e) // 修改IP、掩码、网关、DNS
        {
            // 检查是否有选中的网络适配器
            if (ComboBoxName.SelectedItem is AdapterInfo selectedAdapter)
            {
                // 获取用户在界面上设置的网络配置信息
                string networkAdapterName = selectedAdapter.NetConnectionID;
                bool isDhcpEnabled = RadioButtonDhcpOn.IsChecked == true;
                string ipAddress = IPAddressControlAddress.IPAddress;
                string subnetMask = IPAddressControlMask.IPAddress;
                string gateway = IPAddressControlGateway.IPAddress;
                bool isDnsAuto = RadioButtonDhcpDnsOn.IsChecked == true;
                string[] dnsServers = new string[] { IPAddressControlDnsPrimary.IPAddress, IPAddressControlDnsSecondary.IPAddress };

                try
                {
                    // 异步执行网络配置设置，并等待结果
                    string result = await Task.Run(() => SetNetworkConfiguration(networkAdapterName, isDhcpEnabled, ipAddress, subnetMask, gateway, isDnsAuto, dnsServers));
                    NotificationControl.Add(string.IsNullOrWhiteSpace(result) ? "网络配置设置成功" : result, 5); // 如果输出为空或仅包含空白字符，则也认为配置成功
                }
                catch (Exception ex)
                {
                    NotificationControl.Add("网络配置捕获错误: " + ex.Message, 0); // 显示错误消息
                }
            }
            else
            {
                // 如果没有选中网络适配器，提示用户
                NotificationControl.Add("请选择一个网络适配器！");
            }
        }
        public static string SetNetworkConfiguration(string networkInterfaceName, bool isDhcpEnabled, string ipAddress, string subnetMask, string gateway, bool isDnsAuto, string[] dnsServers)
        {
            // 构建要执行的命令字符串
            StringBuilder commandBuilder = new();

            // 如果启用了DHCP
            if (isDhcpEnabled)
            {
                // 设置网络适配器为自动获取IP地址
                commandBuilder.AppendLine($"netsh interface ip set address name=\"{networkInterfaceName}\" source=dhcp");
            }
            else
            {
                // 如果禁用了DHCP，设置静态IP地址、子网掩码和默认网关
                string command = $"netsh interface ip set address name=\"{networkInterfaceName}\" static {ipAddress} {subnetMask}";
                if (!string.IsNullOrEmpty(gateway))
                {
                    command += $" {gateway}";
                }
                commandBuilder.AppendLine(command);
            }

            // 如果启用了自动DNS
            if (isDnsAuto)
            {
                // 设置网络适配器为自动获取DNS服务器地址
                commandBuilder.AppendLine($"netsh interface ip set dns name=\"{networkInterfaceName}\" source=dhcp");
            }
            else
            {
                // 如果禁用了自动DNS，设置DNS服务器地址
                if (dnsServers != null)
                {
                    if (!string.IsNullOrEmpty(dnsServers[0]))
                    {
                        // 设置主DNS服务器地址
                        commandBuilder.AppendLine($"netsh interface ip set dns name=\"{networkInterfaceName}\" source=static {dnsServers[0]}");

                        // 如果有备用DNS服务器地址，设置备用DNS服务器地址
                        if (dnsServers.Length > 1 && !string.IsNullOrEmpty(dnsServers[1]))
                        {
                            commandBuilder.AppendLine($"netsh interface ip add dns name=\"{networkInterfaceName}\" addr={dnsServers[1]} index=2");
                        }
                    }
                    else if (dnsServers.Length > 1 && !string.IsNullOrEmpty(dnsServers[1]))
                    {
                        // 如果主DNS服务器地址为空，但备用DNS服务器地址不为空，将备用DNS服务器地址设置为主DNS服务器地址
                        commandBuilder.AppendLine($"netsh interface ip set dns name=\"{networkInterfaceName}\" source=static {dnsServers[1]}");
                    }
                    else
                    {
                        // 如果都为空，转为自动获取
                        commandBuilder.AppendLine($"netsh interface ip set dns name=\"{networkInterfaceName}\" source=dhcp");
                    }
                }
            }

            return ToCMD(commandBuilder.ToString()); // 返回输出结果
        }


        // 调用CMD并返回输出结果
        private static string ToCMD(string command)
        {
            try
            {
                // 创建一个新的进程来运行命令
                using Process process = new()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        RedirectStandardInput = true, // 重定向标准输入，允许向进程写入命令
                        RedirectStandardOutput = true, // 重定向标准输出，以读取执行结果
                        UseShellExecute = false, // 设置为 false 以启用输入/输出重定向
                        CreateNoWindow = true, // 不显示命令行窗口
                        Verb = "runas", // 以管理员权限运行
                    }
                };
                process.Start(); // 启动进程

                // 向外部进程发送命令
                using (var streamWriter = process.StandardInput)
                {
                    if (streamWriter.BaseStream.CanWrite) // 检查流是否可写，如果可写向进程的标准输入写入命令
                    {
                        streamWriter.WriteLine("@echo off"); // 清除后面的路径前缀
                        streamWriter.WriteLine(command);
                    }
                }
                process.WaitForExit(); // 等待进程执行完毕

                string output = process.StandardOutput.ReadToEnd();  // 读取执行命令后的输出结果
                int echoOffIndex = output.IndexOf("@echo off"); // 查找 "@echo off" 在输出中的位置
                if (echoOffIndex >= 0) { output = output[(echoOffIndex + "@echo off".Length)..].Trim(); } // 截取 "@echo off" 之后的内容
                //Console.WriteLine("CMD:" + output); // 打印输出结果
                return output;
            }
            catch (Exception ex)
            {
                return ("CMD捕获错误：" + ex.Message);
            }
        }
    }
}