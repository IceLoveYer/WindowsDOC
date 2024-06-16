using CefSharp;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static WindowsDOC.Call.Utility;

namespace WindowsDOC.Pages.Browser
{
    // 定义下载项目模型
    public class DownloadItemModel : INotifyPropertyChanged
    {
        private int id;
        private string? fileName;
        private string? url;
        private string? fullPath;
        private long receivedBytes;
        private long totalBytes;
        private int progress;
        private long speed;
        private bool isComplete;
        private bool downloading;
        private string? displayDescription;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        public string? FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                OnPropertyChanged();
            }
        }

        public string? Url
        {
            get => url;
            set
            {
                url = value;
                OnPropertyChanged();
            }
        }

        public string? FullPath
        {
            get => fullPath;
            set
            {
                fullPath = value;
                OnPropertyChanged();
            }
        }

        public long ReceivedBytes
        {
            get => receivedBytes;
            set
            {
                receivedBytes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayDescription));
            }
        }

        public long TotalBytes
        {
            get => totalBytes;
            set
            {
                totalBytes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayDescription));
            }
        }

        public int Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged();
            }
        }

        public long Speed
        {
            get => speed;
            set
            {
                speed = value;
                OnPropertyChanged();

                string currentSpeed = FormatBytes(Speed);
                string sizeDownloaded = FormatBytes(ReceivedBytes);
                string totalSize = FormatBytes(TotalBytes);
                string remainingTime = CalculateRemainingTime();
                displayDescription = $"{sizeDownloaded}/{totalSize}-{currentSpeed}/s {remainingTime}";
            }
        }

        public bool IsComplete
        {
            get => isComplete;
            set
            {
                isComplete = value;
                OnPropertyChanged();
            }
        }

        public bool Downloading
        {
            get => downloading;
            set
            {
                downloading = value;
                OnPropertyChanged();
            }
        }

        public string? DisplayDescription
        {
            get => displayDescription;
            set
            {
                displayDescription = value;
                OnPropertyChanged();
            }
        }

        public string CalculateRemainingTime()
        {
            if (Speed <= 0 || TotalBytes <= ReceivedBytes)
                return "";

            long bytesRemaining = TotalBytes - ReceivedBytes;
            double secondsRemaining = bytesRemaining / (double)Speed;
            TimeSpan time = TimeSpan.FromSeconds(secondsRemaining);

            if (time.TotalHours >= 1)
                return $"{time.Hours}小时 {time.Minutes}分钟";
            else if (time.TotalMinutes >= 1)
                return $"{time.Minutes}分钟 {time.Seconds}秒";
            else
                return $"{time.Seconds}秒";
        }



        private IDownloadItemCallback? downloadCallback;
        public IDownloadItemCallback? DownloadCallback
        {
            get => downloadCallback;
            set
            {
                downloadCallback = value;
                OnPropertyChanged();
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
