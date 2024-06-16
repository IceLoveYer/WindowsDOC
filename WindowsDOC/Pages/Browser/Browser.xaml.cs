using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WindowsDOC.Control;
using static WindowsDOC.App;

namespace WindowsDOC.Pages.Browser
{
    /// <summary>
    /// Browser.xaml 的交互逻辑
    /// </summary>
    public partial class Browser : Page, ISaveable
    {
        public ObservableCollection<TabItemModel> TabItems { get; set; } = new(); // 浏览器多窗口容器
        public ObservableCollection<DownloadItemModel> DownloadItems { get; set; } = new(); // 下载管理容器
        public ObservableCollection<KeyValuePair<string, string>> UaItems { get; set; } = new() { new("默认", "") }; // Ua管理容器

        public Browser()
        {   
            InitializeComponent();

            // 设置配套项目
            BrowserTabControl.ItemsSource = TabItems;
            ListViewDownload.ItemsSource = DownloadItems;
            ComboBoxUa.ItemsSource = UaItems;
            
            // 读取数据
            Task.Run(async () => { await ReadDataAsync(); });
        }



        // 顶部区域的按钮操作
        private void Button_Click_Back(object sender, RoutedEventArgs e) // 后退
        {
            if (BrowserTabControl.SelectedItem is TabItemModel currentTab && currentTab.Browser.CanGoBack)
            {
                currentTab.Browser.Back();
            }
        }
        private void Button_Click_Refresh(object sender, RoutedEventArgs e) // 刷新
        {
            if (BrowserTabControl.SelectedItem is TabItemModel currentTab)
            {
                TextBoxAddress.Text = currentTab.Browser.Address;
                currentTab.Browser.Reload();
            }
        }
        private void Button_Click_Home(object sender, RoutedEventArgs e) // 主页
        {
            if (BrowserTabControl.SelectedItem is TabItemModel currentTab)
            {
                currentTab.Browser.Load(TextBoxHome.Text);
            }
        }
        private void TextBoxAddress_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) // 地址栏按下回车自动跳转
        {
            if (sender is TextBox tbx) 
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    if (BrowserTabControl.SelectedItem is TabItemModel currentTab && !string.IsNullOrWhiteSpace(TextBoxAddress.Text))
                    {
                        currentTab.Browser.Load(tbx.Text);
                    }
                }
            }
        }
        private void Button_Click_Navigate(object sender, RoutedEventArgs e) // 跳转
        {
            if (BrowserTabControl.SelectedItem is TabItemModel currentTab && !string.IsNullOrWhiteSpace(TextBoxAddress.Text))
            {
                currentTab.Browser.Load(TextBoxAddress.Text);
            }
        }
        private void Button_Click_New(object sender, RoutedEventArgs e) // 新增
        {
            AddNewTab(TextBoxHome.Text);
        }
        private void Button_Click_Close(object sender, RoutedEventArgs e) // 关闭
        {
            // 尝试获取触发事件的按钮
            if (sender is not Button button) return;

            // 获取按钮的DataContext，它应该是TabItemModel的实例
            if (button.DataContext is TabItemModel tabToClose)
            {
                CloseTab(tabToClose);
            }
        }
        public void CloseTab(TabItemModel tabToClose)
        {
            // 如果还剩一个标签页，添加一个新的标签页
            if (TabItems.Count <= 1)
            {
                AddNewTab(TextBoxHome.Text);
            }


            // 尝试选择一个不同的标签页作为当前选中项，否则会报绑定错误
            var currentIndex = TabItems.IndexOf(tabToClose);
            var newIndex = currentIndex - 1 >= 0 ? currentIndex - 1 : currentIndex + 1 < TabItems.Count ? currentIndex + 1 : -1;
            if (newIndex >= 0)
            {
                BrowserTabControl.SelectedItem = TabItems[newIndex];
            }

            TabItems.Remove(tabToClose);
        }
        private void Button_Click_Download(object sender, RoutedEventArgs e) // 下载
        {
            if (DownloadPanel.Width.Value == 0)
            {
                DownloadPanel.Width = new GridLength(1, GridUnitType.Star);

                if (sender is Button button) button.SetResourceReference(BackgroundProperty, "ColorBrushMain1");
            }
            else
            {
                DownloadPanel.Width = new GridLength(0);

                if (sender is Button button) button.SetResourceReference(BackgroundProperty, "ColorBrushMain2");
            }
        }
        private void Button_Click_Setting(object sender, RoutedEventArgs e) // 设置
        {
            if (SettingPanel.Width.Value == 0)
            {
                SettingPanel.Width = new GridLength(1, GridUnitType.Star);

                if (sender is Button button) button.SetResourceReference(BackgroundProperty, "ColorBrushMain1");
            }
            else
            {
                SettingPanel.Width = new GridLength(0);

                if (sender is Button button) button.SetResourceReference(BackgroundProperty, "ColorBrushMain2");
            }
        }



        // 添加新标签
        private void AddNewTab(string url)
        {
            var newTab = new TabItemModel(url); // 使用带URL的构造函数
            newTab.RequestCloseTab += CloseTab; // 订阅空白标签页自动关闭事件

            // 拦截跳转新窗口
            var handler = new CustomLifeSpanHandler();
            handler.PopupRequested += (popupUrl) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AddNewTab(popupUrl); // 在这里添加新的标签页
                });
            };
            newTab.Browser.LifeSpanHandler = handler;

            // 拦截下载
            var downloadHandler = new CustomDownloadHandler(DownloadItems);
            newTab.Browser.DownloadHandler = downloadHandler;


            // 为新标签的浏览器实例添加地址变化监听
            newTab.Browser.AddressChanged += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 如果当前标签页是选中状态，则更新地址栏
                    if (BrowserTabControl.SelectedItem == newTab)
                    {
                        TextBoxAddress.Text = args.NewValue.ToString();
                    }
                });
            };

            TabItems.Add(newTab);
            BrowserTabControl.SelectedItem = newTab; // 选中新标签页
        }

        // 标签页改变
        private void BrowserTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 当选中的标签页改变时，更新地址栏
            if (BrowserTabControl.SelectedItem is TabItemModel currentTab)
            {
                TextBoxAddress.Text = currentTab.Browser.Address;
            }
        }



        // 下载区域的按钮操作
        private void Button_Click_PauseOrResume(object sender, RoutedEventArgs e) // 暂停/继续下载
        {
            if (sender is Button button && button.DataContext is DownloadItemModel downloadItemModel)
            {
                if (!downloadItemModel.IsComplete)
                {
                    // 获取SvgAwesome控件
                    var svgIcon = button.FindName("SvgAwesomePauseOrResum") as FontAwesome5.SvgAwesome;

                    if (downloadItemModel.Downloading)
                    {
                        downloadItemModel.Downloading = false;

                        downloadItemModel.DownloadCallback?.Pause();
                        // 如果下载正在进行，则显示“播放”图标，表示点击后会继续
                        if (svgIcon != null) svgIcon.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Play;
                    }
                    else
                    {
                        downloadItemModel.Downloading = true;

                        downloadItemModel.DownloadCallback?.Resume();
                        // 如果下载已暂停，则显示“暂停”图标，表示点击后会暂停
                        if (svgIcon != null) svgIcon.Icon = FontAwesome5.EFontAwesomeIcon.Solid_Pause;
                    }
                }
                else
                {
                    NotificationControl.Add($"【{downloadItemModel.FileName}】已经下载完成！");
                }
            }
        }
        private void Button_Click_DeleteDownload(object sender, RoutedEventArgs e) // 删除下载
        {
            if (sender is Button button && button.DataContext is DownloadItemModel downloadItemModel)
            {
                // 取消下载
                if (!downloadItemModel.IsComplete) downloadItemModel.DownloadCallback?.Cancel();

                // 从UI集合中移除
                DownloadItems.Remove(downloadItemModel);
            }
        }
        private void Button_Click_OpenFile(object sender, RoutedEventArgs e) // 打开文件
        {
            if (sender is Button button && button.DataContext is DownloadItemModel downloadItemModel && File.Exists(downloadItemModel.FullPath))
            {
                if (downloadItemModel.IsComplete)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(downloadItemModel.FullPath) { UseShellExecute = true });
                }
                else
                {
                    NotificationControl.Add($"【{downloadItemModel.FileName}】还没有下载完哦");
                }
            }
        }
        private void Button_Click_OpenFolder(object sender, RoutedEventArgs e) // 打开文件夹
        {
            if (sender is Button button && button.DataContext is DownloadItemModel downloadItemModel && File.Exists(downloadItemModel.FullPath))
            {
                if (downloadItemModel.IsComplete)
                {
                    string argument = $"/select, \"{downloadItemModel.FullPath}\"";
                    System.Diagnostics.Process.Start("explorer.exe", argument);
                }
                else
                {
                    NotificationControl.Add($"【{downloadItemModel.FileName}】还没有下载完哦");
                }
            }
        }



        // 选择预设UA
        private void ComboBoxUa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxUa.SelectedItem is KeyValuePair<string, string> selectedUa)
            {
                TextBoxUaTitle.Text = selectedUa.Key;
                TextBoxUa.Text = selectedUa.Value;

                ChangeUa(selectedUa.Value);
            }
        }
        // 更改所有浏览器UA
        private void ChangeUa(string newUa)
        {
            // 更新所有浏览器实例的UA，这里选择自定义则获取TextBoxUa内容作为UA
            CustomRequestHandler.SetCustomUserAgent(newUa);

            // 遍历所有标签，强制每个浏览器重新加载以应用新的UA
            foreach (var tab in TabItems)
            {
                // 可以选择只有在浏览器已经加载了内容的情况下才重新加载
                if (!string.IsNullOrEmpty(tab.Browser.Address))
                {
                    tab.Browser.Reload();
                }
            }
        }

        private void Button_Click_UaAdd(object sender, RoutedEventArgs e) // 添加预设UA
        {
            var newKey = TextBoxUaTitle.Text.Trim();
            var newValue = TextBoxUa.Text.Trim();

            // 检查Key是否为空
            if (string.IsNullOrEmpty(newKey))
            {
                NotificationControl.Add($"UA名称不能为空！");
                return;
            }

            // 检查Key是否已存在
            if (UaItems.Any(ua => ua.Key == newKey))
            {
                NotificationControl.Add($"该UA名称已存在！");
                return;
            }

            UaItems.Add(new KeyValuePair<string, string>(newKey, newValue));
            NotificationControl.Add($"添加【{newKey}】成功~");
        }
        private void Button_Click_UaUpdate(object sender, RoutedEventArgs e) // 更新预设UA
        {
            if (ComboBoxUa.SelectedIndex <= 0) // 不允许修改索引为0的项
            {
                NotificationControl.Add($"默认项不能被修改！");
                return;
            }

            var newKey = TextBoxUaTitle.Text.Trim();
            var newValue = TextBoxUa.Text.Trim();

            // 检查Key是否为空
            if (string.IsNullOrEmpty(newKey))
            {
                NotificationControl.Add($"UA名称不能为空！");
                return;
            }

            // 更新项
            UaItems[ComboBoxUa.SelectedIndex] = new KeyValuePair<string, string>(newKey, newValue);
            ChangeUa(newValue);
            NotificationControl.Add($"更新【{newKey}】成功~");
        }
        private void Button_Click_UaDel(object sender, RoutedEventArgs e) // 删除预设UA
        {
            if (ComboBoxUa.SelectedIndex <= 0) // 不允许删除索引为0的项
            {
                NotificationControl.Add($"默认项不能被删除！");
                return;
            }

            UaItems.RemoveAt(ComboBoxUa.SelectedIndex);
            ComboBoxUa.SelectedIndex = 0;
        }



        // 保存数据
        public async Task SaveDataAsync()
        {
            try
            {
                var userAgentsToSave = UaItems
                   .Where((ua, index) => index > 0) // 跳过第一个默认的选项
                   .ToDictionary(p => p.Key, p => p.Value);

                var settings = new Dictionary<string, object>
                {
                    ["HomePageUrl"] = TextBoxHome.Text,
                    ["UserAgents"] = userAgentsToSave,
                    ["SelectedUserAgentIndex"] = ComboBoxUa.SelectedIndex,
                    ["DownloadPanelVisible"] = DownloadPanel.Width != new GridLength(0),
                    ["SettingPanelVisible"] = SettingPanel.Width != new GridLength(0),
                    ["OpenTabItemsUrls"] = TabItems.Select(tab => tab.Browser.Address).ToList(),
                    ["DownloadRecords"] = DownloadItems.Select(item => new
                    {
                        item.FileName,
                        item.FullPath,
                        item.Progress,
                        item.IsComplete,
                        item.DisplayDescription
                    }).ToList()
                };

                string jsonData = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(GlobalDir.BrowserJson, jsonData, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                NotificationControl.Add("SaveData捕获错误：" + ex.Message);
            }
        }

        // 读取数据
        public async Task ReadDataAsync()
        {
            if (File.Exists(GlobalDir.BrowserJson))
            {
                string jsonData = await File.ReadAllTextAsync(GlobalDir.BrowserJson, Encoding.UTF8);
                var settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);

                if (settings != null)
                {
                    var savedUserAgents = settings["UserAgents"].EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString());
                    var downloadPanelVisible = settings["DownloadPanelVisible"].GetBoolean();
                    var settingPanelVisible = settings["SettingPanelVisible"].GetBoolean();
                    var openTabItemsUrls = JsonSerializer.Deserialize<List<string>>(settings["OpenTabItemsUrls"].GetRawText());
                    var downloadRecords = JsonSerializer.Deserialize<ObservableCollection<DownloadItemModel>>(settings["DownloadRecords"].GetRawText());

                    // 使用Dispatcher.BeginInvoke来确保UI更新在UI线程上执行
                    await Dispatcher.BeginInvoke((async () =>
                    {
                        // 恢复首页URL
                        TextBoxHome.Text = settings["HomePageUrl"].GetString();

                        // 清除现有的UA项（除了默认项）并重新加载
                        UaItems.Clear();
                        UaItems.Add(new KeyValuePair<string, string>("默认", "")); // 保留默认项
                        foreach (var ua in savedUserAgents)
                        {
                            UaItems.Add(new KeyValuePair<string, string>(ua.Key, ua.Value ?? string.Empty));
                        }

                        // 设置选中的UA
                        ComboBoxUa.SelectedIndex = settings["SelectedUserAgentIndex"].GetInt32();

                        // 恢复下载面板的可见性
                        DownloadPanel.Width = downloadPanelVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
                        ButtonDownload.SetResourceReference(BackgroundProperty, downloadPanelVisible ? "ColorBrushMain1" : "ColorBrushMain2");

                        // 恢复设置面板的可见性
                        SettingPanel.Width = settingPanelVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
                        ButtonSetting.SetResourceReference(BackgroundProperty, settingPanelVisible ? "ColorBrushMain1" : "ColorBrushMain2");

                        // 恢复打开的标签页和下载记录的操作可能需要保持在外部，特别是如果它们涉及到异步操作
                        if (openTabItemsUrls != null)
                        {
                            foreach (var url in openTabItemsUrls)
                            {
                                AddNewTab(url);
                                await Task.Delay(100); // 等待100毫秒再继续下一个
                            }
                        }

                        // 恢复下载记录
                        if (downloadRecords != null)
                        {
                            foreach (var record in downloadRecords)
                            {
                                DownloadItems.Add(new DownloadItemModel
                                {
                                    FileName = record.FileName,
                                    FullPath = record.FullPath,
                                    Progress = record.Progress,
                                    IsComplete = record.IsComplete,
                                    DisplayDescription = record.DisplayDescription,
                                });
                            }
                        }
                    }));
                }
            }
            else
            {
                // 若无配置文件的初始值
                await Dispatcher.BeginInvoke(() =>
                 {
                     TextBoxHome.Text = "www.baidu.com";
                     AddNewTab(TextBoxHome.Text);
                     ComboBoxUa.SelectedIndex = 0;
                     DownloadPanel.Width = new GridLength(0);
                     SettingPanel.Width = new GridLength(0);
                 });
            }
        }
    }
}
