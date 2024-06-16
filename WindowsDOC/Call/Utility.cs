using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Windows.Media;

namespace WindowsDOC.Call
{
    public static class Utility
    {
        // 格式化字节并加入合适的单位，若oneThousand为真则转换厂家1000，默认系统定义的1024
        public static string FormatBytes(long bytes, bool oneThousand = false)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            long divisor = oneThousand ? 1000 : 1024;

            while (len >= divisor && order < sizes.Length - 1)
            {
                order++;
                len /= divisor;
            }
            return $"{len:0.##} {sizes[order]}";
        }



        // 注册表通用转长整数
        public static long ConvertToLong(object value)
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
                    throw new InvalidOperationException($"不支持的字节数组长度：{bytes.Length}");
                }
            }
            else if (value is UInt64 || value is UInt32 || value is Int64 || value is Int32)
            {
                return Convert.ToInt64(value);
            }
            else
            {
                throw new InvalidOperationException($"不支持的数据类型：{value.GetType().FullName}");
            }
        }
    }
}
