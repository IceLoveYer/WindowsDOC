using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WindowsDOC.Control;

namespace WindowsDOC.Pages.NetworkConfig
{
    public partial class NetworkConfig : Page
    {
        public NetworkConfig()
        {
            InitializeComponent();

            // 读取网络接口信息
            Task.Run(async () => { await LoadNetworkInterfacesAsync(); });
        }



        public async Task LoadNetworkInterfacesAsync() // 异步加载网络接口信息并填充下拉框
        {
            try
            {
                var adapterList = await Task.Run(() =>
                 {
                     var list = new List<AdapterInfo>();

                     // 查询Win32_NetworkAdapter WMI类以获取所有网络适配器
                     using (var adapterSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter = true"))
                     {
                         foreach (ManagementObject obj in adapterSearcher.Get().Cast<ManagementObject>())
                         {
                             // 获取与物理适配器关联的配置
                             using var configSearcher = new ManagementObjectSearcher($"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index = {obj["Index"]}");
                             var config = configSearcher.Get().Cast<ManagementObject>().FirstOrDefault();
                             if (config != null)
                             {
                                 list.Add(new AdapterInfo
                                 {
                                     Name = obj["Name"]?.ToString() ?? "",
                                     Description = obj["Description"]?.ToString() ?? "",
                                     NetConnectionID = obj["NetConnectionID"]?.ToString() ?? "",
                                     GUID = obj["GUID"]?.ToString() ?? "",
                                     MACAddress = config["MACAddress"]?.ToString() ?? "",
                                     IPAddresses = config["IPAddress"] as string[] ?? Array.Empty<string>(),
                                     Subnets = config["IPSubnet"] as string[] ?? Array.Empty<string>(),
                                     Gateways = config["DefaultIPGateway"] as string[] ?? Array.Empty<string>(),
                                     DNSServers = config["DNSServerSearchOrder"] as string[] ?? Array.Empty<string>(),
                                     IsDhcpEnabled = (bool)config["DHCPEnabled"],
                                 });
                             }
                         }
                     }
                     return list;
                 });

                // 更新UI，这需要在UI线程上执行
                Dispatcher.Invoke(() =>
                {
                    // 填充下拉框
                    ComboBoxName.ItemsSource = adapterList;
                    ComboBoxName.DisplayMemberPath = "NetConnectionID";

                    // 如果网络接口列表不为空，选择第一个网络接口
                    if (ComboBoxName.Items.Count > 0)
                    {
                        ComboBoxName.SelectedItem = ComboBoxName.Items[0];
                    }
                });
            }
            catch (Exception ex)
            {
                NotificationControl.Add("查询WMI数据时发生错误: " + ex.Message);
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
                    NetworkUtilities.ChangeMacAddressAndReset(TextBoxMac.Text, selectedAdapter.GUID);
                    NotificationControl.Add("MAC地址已更改。请检查网络适配器是否已重新启用。");

                }
                else
                {
                    NotificationControl.Add("无效的MAC地址格式。");
                }
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
                    string result = await Task.Run(() => NetworkUtilities.SetNetworkConfiguration(networkAdapterName, isDhcpEnabled, ipAddress, subnetMask, gateway, isDnsAuto, dnsServers));
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
    }
}