using System;
using System.Diagnostics;
using System.Text;

namespace WindowsDOC.Pages.NetworkConfig
{
    public class NetworkUtilities
    {
        //更改Mac地址并重置
        public static void ChangeMacAddressAndReset(string newMac, string adapterId)
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



        // 设置网络配置
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
