using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using FA5Icon = FontAwesome5.EFontAwesomeIcon;

namespace WindowsDOC.Pages
{
    public partial class SystemConfig : Page
    {
        public SystemConfig()
        {
            InitializeComponent();


            // 读取系统占用率
            ReadSystemUsage();

            // 创建定时
            DispatcherTimer timer = new()
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            timer.Tick += (s, e) => UpdateSystemUsage();
            timer.Start();


            // 读取系统配置
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                await ReadSystemConfigurationAsync();
            }));
        }



        // 系统占用率 数据绑定
        public class SystemUsage
        {
            // 属性声明
            public string Name { get; set; }
            public double Progress { get; set; }
            public string ProgressText => $"{Name} {Progress}%";

            // 构造函数
            public SystemUsage(string name, double value)
            {
                Name = name;
                Progress = value;
            }
        }

        public void ReadSystemUsage()
        {
            // 初始化系统占用率列表并直接绑定到ListView
            ListViewUsage.ItemsSource = new List<SystemUsage>
            {
                new ("CPU", 0),
                new ("内存", 0)
            }.Concat(DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Fixed)
                .Select(d => new SystemUsage(d.Name, 0)))
                .ToList();

            // 这里可以先获取一次，否则要等计时器时间才能触发
            UpdateSystemUsage();
        }


        // 更新系统占用率
        private readonly PerformanceCounter cpuCounter = new("Processor", "% Processor Time", "_Total");
        private readonly PerformanceCounter ramCounter = new("Memory", "Available MBytes");

        public void UpdateSystemUsage()
        {
            if (ListViewUsage.ItemsSource is List<SystemUsage> systemUsages)
            {
                // 更新CPU占用率
                var cpuUsage = systemUsages.FirstOrDefault(su => su.ProgressText.StartsWith("CPU"));
                if (cpuUsage != null)
                {
                    cpuUsage.Progress = Math.Round(cpuCounter.NextValue());
                }

                // 更新内存占用率
                var ramUsage = systemUsages.FirstOrDefault(su => su.ProgressText.StartsWith("内存"));
                if (ramUsage != null)
                {
                    var totalMemory = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
                    var availableMemory = ramCounter.NextValue() * 1024 * 1024; // 转换为字节
                    var usedMemory = totalMemory - availableMemory;
                    var memoryUsage = (usedMemory / totalMemory) * 100;
                    ramUsage.Progress = Math.Round(memoryUsage);
                }

                // 更新磁盘分区占用率
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
                {
                    var driveUsage = systemUsages.FirstOrDefault(su => su.ProgressText.StartsWith(drive.Name));
                    if (driveUsage != null)
                    {
                        driveUsage.Name = $"用{FormatSize(drive.TotalSize - drive.AvailableFreeSpace, false)}\n共{FormatSize(drive.TotalSize, false)}\n{drive.Name} ";
                        driveUsage.Progress = Math.Round(100 * (1 - (double)drive.AvailableFreeSpace / drive.TotalSize));
                    }
                }

                // 刷新ListView
                ListViewUsage.Items.Refresh();
            }
        }



        // 系统信息 数据绑定
        public class SystemInfo
        {
            // 属性声明
            public FA5Icon Icon { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }

            // 构造函数
            public SystemInfo(FA5Icon icon, string name, string value)
            {
                Icon = icon;
                Name = name;
                Value = value;
            }
        }


        // 读取系统配置
        public async Task ReadSystemConfigurationAsync()
        {
            var systemInfos = await Task.Run(() =>
            {
                // 在后台线程上构建 systemInfos 列表
                return new List<SystemInfo>
                {
                    new (FA5Icon.Solid_Portrait, "电脑名称", Environment.MachineName),

                    new (FA5Icon.Brands_Modx, "电脑型号", FetchWMIInfo("select * from Win32_ComputerSystem", obj =>
                    {
                        string chassisType = FetchWMIInfo("select * from Win32_SystemEnclosure", enclosureObj =>
                        {
                            if (enclosureObj["ChassisTypes"] is ushort[] types && types.Length > 0)
                                return types[0].ToString();
                            return "";
                        });

                        string computerType = (chassisType == "9" || chassisType == "10") ? "笔记本" : "台式机";
                        string model = obj["Model"]?.ToString() ?? "";
                        return $"{computerType} {model}";
                    })),

                    new (FA5Icon.Brands_Windows, "操作系统", FetchWMIInfo("select * from Win32_OperatingSystem", obj =>
                    {
                        string caption = obj["Caption"]?.ToString() ?? "";
                        string architecture = obj["OSArchitecture"]?.ToString() ?? "";

                        RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                        if (key != null)
                        {
                            string? displayVersion = key.GetValue("DisplayVersion", "")?.ToString();
                            string? currentBuild = key.GetValue("CurrentBuild", "")?.ToString();
                            int? ubr = Convert.ToInt32(key.GetValue("UBR", 0));
                            return $"{caption} {architecture} {displayVersion} ({currentBuild}.{ubr})";
                        }
                        else
                        {
                            return $"{caption} {architecture}";
                        }
                    })),

                    new (FA5Icon.Brands_Elementor, "主板", FetchWMIInfo("select * from Win32_BaseBoard", obj => obj["Manufacturer"]?.ToString() + " " + obj["Product"]?.ToString())),

                    new (FA5Icon.Solid_Microchip, "CPU", FetchWMIInfo("select * from Win32_Processor", obj =>
                    {
                        string cpuName = obj["Name"]?.ToString() ?? "";
                        string cores = obj["NumberOfCores"]?.ToString() + "核" ?? "";
                        string threads = obj["NumberOfLogicalProcessors"]?.ToString() + "线程" ?? "";
                        return $"{cpuName} {cores}{threads}";
                    })),

                    new (FA5Icon.Solid_Memory, "内存", FetchWMIInfo("select * from Win32_PhysicalMemory", obj =>
                    {
                        string manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                        string speed = obj["Speed"]?.ToString() ?? "";
                        string partNumber = obj["PartNumber"]?.ToString() ?? "";
                        string serialNumber = obj["SerialNumber"]?.ToString() ?? "";
                        string capacity = $"{Convert.ToInt64(obj["Capacity"]) / (1024 * 1024 * 1024)}G";

                        string memoryType = string.Empty;
                        try
                        {
                            int type = Convert.ToInt32(obj["SMBIOSMemoryType"]);
                            memoryType = type switch
                            {
                                0x0 => "Unknown",
                                0x1 => "Other",
                                0x2 => "DRAM",
                                0x3 => "Synchronous DRAM",
                                0x4 => "Cache DRAM",
                                0x5 => "EDO",
                                0x6 => "EDRAM",
                                0x7 => "VRAM",
                                0x8 => "SRAM",
                                0x9 => "RAM",
                                0xa => "ROM",
                                0xb => "Flash",
                                0xc => "EEPROM",
                                0xd => "FEPROM",
                                0xe => "EPROM",
                                0xf => "CDRAM",
                                0x10 => "3DRAM",
                                0x11 => "SDRAM",
                                0x12 => "SGRAM",
                                0x13 => "RDRAM",
                                0x14 => "DDR",
                                0x15 => "DDR2",
                                0x16 => "DDR2 FB-DIMM",
                                0x17 => "Undefined 23",
                                0x18 => "DDR3",
                                0x19 => "FBD2",
                                0x1a => "DDR4",
                                0x22 => "DDR5",
                                _ => "Undefined"
                            };

                            return $"{manufacturer} {partNumber} {memoryType} {speed}MHz {capacity} ({serialNumber})";
                        }
                        catch (Exception)
                        {
                            return $"{manufacturer} {partNumber} {speed}MHz {capacity} ({serialNumber})";
                        }

                    })),

                    new (FA5Icon.Solid_Hdd, "磁盘", FetchWMIInfo("select * from Win32_DiskDrive", obj =>
                    {
                        string model = obj["Model"]?.ToString() ?? "";
                        string size = FormatSize(Convert.ToInt64(obj["Size"]), false);
                        string serialNumber = obj["SerialNumber"]?.ToString() ?? "";

                        return $"{model} {size} ({serialNumber})";
                    })),

                    new (FA5Icon.Solid_FileVideo, "显卡", FetchWMIInfo("select * from Win32_VideoController", obj =>
                    {
                        string cardName = obj["Name"]?.ToString() ?? "";

                        // 从注册表提取显存大小，obj["AdapterRAM"]返回的类型是UInt32，不能超过4G
                        string size = "";
                        string? pnpDeviceID = obj["PNPDeviceID"]?.ToString();
                        if (!string.IsNullOrEmpty(pnpDeviceID))
                        {
                            // 这个表里包含了所有显卡信息，从0000开始
                            RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
                            if (key != null)
                            {
                                foreach (string subKeyName in key.GetSubKeyNames())
                                {
                                    try
                                    {
                                        RegistryKey? subKey = key.OpenSubKey(subKeyName);
                                        if (subKey != null)
                                        {
                                            // 这里判断前面PNPDeviceID中开头是否包含MatchingDeviceId的路径
                                            string? matchingDeviceId = subKey.GetValue("MatchingDeviceId")?.ToString();
                                            if (!string.IsNullOrEmpty(matchingDeviceId) && pnpDeviceID.Contains(matchingDeviceId, StringComparison.OrdinalIgnoreCase))
                                            {
                                                // 优先返回qwMemorySize，其次MemorySize
                                                var memorySizeObj = subKey.GetValue("HardwareInformation.qwMemorySize") ?? subKey.GetValue("HardwareInformation.MemorySize");
                                                if (memorySizeObj != null)
                                                {
                                                    size = FormatSize(ConvertToLong(memorySizeObj), false);
                                                }
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                        }

                        string driverVersion = obj["DriverVersion"]?.ToString() ?? "";

                        return $"{cardName} {size} ({driverVersion})";
                    })),

                    new (FA5Icon.Solid_Desktop, "显示器", GetRegistryMonitorInfo() ?? FetchWMIInfo("select * from Win32_DesktopMonitor", obj =>
                    {
                        // 如果注册表没有提取显示器信息，继续从WMI提取信息，内容少得可怜..
                        string name = obj["Name"]?.ToString() ?? "";
                        string screenHeight = obj["ScreenHeight"]?.ToString() ?? "";
                        string screenWidth = obj["ScreenWidth"]?.ToString() ?? "";

                        return $"{name} {screenHeight}×{screenWidth}";
                    })),

                    new (FA5Icon.Solid_FileAudio, "声卡", FetchWMIInfo("select * from Win32_SoundDevice", obj => obj["Name"]?.ToString() ?? "")),

                    new (FA5Icon.Solid_Ethernet, "网卡", FetchWMIInfo("select * from Win32_NetworkAdapter WHERE PhysicalAdapter = true", obj =>
                    {
                        string name = obj["Name"]?.ToString() ?? "";
                        string macAddress = obj["MACAddress"]?.ToString() ?? "";

                        return $"{name} ({macAddress})";
                    }))
                };
            });

            // 在主线程上更新 ListViewItems 的 ItemsSource
            Dispatcher.Invoke(() =>
            {
                ListViewItems.ItemsSource = systemInfos;
            });
        }



        // 使用WMI查询获取系统信息
        private static string FetchWMIInfo(string wmiQuery, Func<ManagementObject, string> processFunc)
        {
            try
            {
                ManagementObjectSearcher searcher = new(wmiQuery); // 创建一个新的WMI查询搜索器

                List<string> resultList = new(); // 用于存储处理后的查询结果

                // 遍历WMI查询返回的每个对象
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    string result = processFunc(obj); // 使用提供的函数处理对象，并获取结果

                    Regex MyRegex = new("\\s+", RegexOptions.Compiled);
                    result = MyRegex.Replace(result, " "); // 使用正则表达式替换多个连续的空白字符为一个空白字符

                    resultList.Add(result); // 将处理后的结果添加到结果列表中
                }

                return string.Join("\n", resultList); // 使用换行符连接处理后的所有结果，并返回
            }
            catch (Exception ex)
            {
                // 使用 MessageBox 显示错误信息
                return $"发生错误：{ex.Message}";
            }
        }





        // 格式化大小，用来转换厂家或系统定义的容量大小，并加入合适的单位
        public static string FormatSize(long sizeInBytes, bool useBinaryPrefix)
        {
            string[] units = new string[] { "B", "KB", "MB", "GB", "TB" };
            int unitIndex = 0;
            long divisor = useBinaryPrefix ? 1024 : 1000;

            while (sizeInBytes >= divisor && unitIndex < units.Length - 1)
            {
                sizeInBytes /= divisor;
                unitIndex++;
            }

            return $"{sizeInBytes}{units[unitIndex]}";
        }

        // 注册表通用转长整数
        static long ConvertToLong(object value)
        {
            // 尝试从byte[]转换为long
            if (value is byte[] bytes)
            {
                if (bytes.Length == 8) // UInt64 or Int64
                {
                    return BitConverter.ToInt64(bytes, 0);
                }
                else if (bytes.Length == 4) // UInt32 or Int32
                {
                    return BitConverter.ToInt32(bytes, 0);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported byte array length: {bytes.Length}");
                }
            }
            else if (value is UInt64 || value is UInt32 || value is Int64 || value is Int32)
            {
                return Convert.ToInt64(value);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported data type: {value.GetType().FullName}");
            }
        }

        // 从注册表获取显示器信息 *这是精华*
        static string? GetRegistryMonitorInfo()
        {
            // 计算机\HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Enum\DISPLAY\CMN15F4\4&39f8f762&0&UID265988\Device Parameters
            if (Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Enum\DISPLAY") is RegistryKey baseKey)
            {
                string info = string.Empty; // 定义返回值

                foreach (string subKeyName in baseKey.GetSubKeyNames()) // CMN15F4
                {
                    if (baseKey.OpenSubKey(subKeyName) is RegistryKey subKey)
                    {
                        foreach (string monitorKey in subKey.GetSubKeyNames()) // 4&39f8f762&0&UID265988
                        {
                            // 获取EDID数据并解析
                            if ((baseKey.OpenSubKey(subKeyName + @"\" + monitorKey + @"\Device Parameters")?.GetValue("EDID") as byte[]) is byte[] edid && edid.Length > 0)
                            {
                                // 制造商ID，0x08-0x09，都会简写三个字母
                                string manufacturer = Encoding.ASCII.GetString(new byte[]
                                {
                                        (byte)(64 + ((edid[0x08] & 124) >> 2)), // 0x08中的第1-5位，第0位是符号
                                        (byte)(64 + ((edid[0x08] & 3) << 3) + ((edid[0x09] & 224) >> 5)), // 0x08中的第6-7位，0x09中的第0-2位
                                        (byte)(64 + (edid[0x09] & 31)) // 0x09中的第3-7位
                                });

                                // 水平和垂直图像尺寸，0x15-0x16，A²+B²=C²，转英寸÷2.54
                                double horizontalImageSize = edid[0x15]; // 34cm
                                double verticalImageSize = edid[0x16]; // 19cm
                                double diagonalInch = Math.Sqrt(Math.Pow(horizontalImageSize, 2) + Math.Pow(verticalImageSize, 2)); // 获取对角线长度
                                string screenSize = $"{Math.Round(diagonalInch / 2.54, 1)}in"; // 15.3

                                // 像素时钟
                                int pixelClock = (edid[0x37] << 8 | edid[0x36]); // 28505

                                // 水平和垂直活动像素，0x38与0x3A前四位拼接，0x3B与0x3D前四位拼接
                                int horizontalActivePixels = (edid[0x3A] >> 4) << 8 | edid[0x38]; // 1920
                                int verticalActivePixels = (edid[0x3D] >> 4) << 8 | edid[0x3B]; // 1080
                                string preferredResolution = $"{horizontalActivePixels}×{verticalActivePixels}";

                                // 计算宽高比
                                int gcdValue = GCD(horizontalActivePixels, verticalActivePixels);
                                string aspectRatio = $"{horizontalActivePixels / gcdValue}:{verticalActivePixels / gcdValue}";
                                // 辅助函数：计算最大公约数
                                static int GCD(int a, int b)
                                {
                                    while (b != 0)
                                    {
                                        int temp = b;
                                        b = a % b;
                                        a = temp;
                                    }
                                    return a;
                                }

                                // 水平和垂直blanking，0x39与0x3A后四位拼接，0x3C与0x3D后四位拼接
                                int horizontalBlanking = ((edid[0x3A] & 0x0F) << 8) | edid[0x39]; // 160
                                int verticalBlanking = ((edid[0x3D] & 0x0F) << 8) | edid[0x3C]; // 62
                                double calculatedRefreshRate = (pixelClock * 10000) / ((horizontalActivePixels + horizontalBlanking) * (verticalActivePixels + verticalBlanking));
                                int refreshRate = Convert.ToInt32(Math.Round(calculatedRefreshRate)); // 28505*10000/[(1920+160)*(1080+62)]≈120Hz

                                // 型号，70-7D
                                string modelDescription = Encoding.ASCII.GetString(edid, 0x70, 14).Trim();

                                // 拼接信息
                                info += $"{manufacturer} {modelDescription} {screenSize} {preferredResolution} {aspectRatio} {refreshRate}Hz\n";
                            }
                        }
                    }
                }

                if (info.EndsWith('\n')) { info = info.Remove(info.Length - 1); } // 删除最后一个换行符
                return info;
            }

            return null; // 如果没有从注册表中获取到有效的显示器信息，返回null
        }
    }
}
