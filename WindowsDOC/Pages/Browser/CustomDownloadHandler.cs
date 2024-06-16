using CefSharp;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WindowsDOC.Control;

namespace WindowsDOC.Pages.Browser
{
    // 定义下载处理
    public class CustomDownloadHandler : IDownloadHandler
    {
        private readonly ObservableCollection<DownloadItemModel> downloadItems;

        public CustomDownloadHandler(ObservableCollection<DownloadItemModel> downloadItems)
        {
            this.downloadItems = downloadItems;
        }

        public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
        {
            // 允许所有下载
            return true;
        }

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (!callback.IsDisposed)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        FileName = downloadItem.SuggestedFileName,
                        DefaultExt = System.IO.Path.GetExtension(downloadItem.SuggestedFileName),
                        Filter = "All files (*.*)|*.*"
                    };

                    // 用户点击确定后才能下载
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        // 使用建议的文件名，不显示下载对话框
                        callback.Continue(saveFileDialog.FileName, showDialog: false);

                        // 添加新的下载项到集合中
                        downloadItems.Add(new DownloadItemModel
                        {
                            Id = downloadItem.Id,
                            FileName = downloadItem.SuggestedFileName,
                            Url = downloadItem.Url,
                            TotalBytes = downloadItem.TotalBytes,
                            Downloading = true,
                        });
                    }
                });
            }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            // 在UI线程上执行，因为我们将修改绑定到UI的集合
            App.Current.Dispatcher.Invoke(() =>
            {
                var item = downloadItems.FirstOrDefault(i => i.Id == downloadItem.Id);
                if (item != null)
                {
                    item.FullPath = downloadItem.FullPath;
                    item.IsComplete = downloadItem.IsComplete;
                    item.DownloadCallback = callback;

                    if (downloadItem.IsInProgress) // 下载进行中
                    {
                        // 更新下载项的状态
                        item.ReceivedBytes = downloadItem.ReceivedBytes;
                        item.Progress = downloadItem.PercentComplete;
                        item.Speed = downloadItem.CurrentSpeed;
                    }
                    else if (downloadItem.IsComplete) // 下载完成
                    {
                        NotificationControl.Add($"【{item.FileName}】下载完成！");
                    }
                }
            });
        }
    }
}
