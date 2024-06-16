using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsDOC.Call;
using WindowsDOC.Control;
using static WindowsDOC.App;

namespace WindowsDOC.Pages.Launcher
{

    /// <summary>
    /// Launcher.xaml 的交互逻辑
    /// </summary>
    public partial class Launcher : Page, ISaveable
    {
        private readonly ObservableCollection<ItemGroupInfo> ItemsGroups = new(); // 这里用ObservableCollection会自动刷新界面
        private ElevatedFileDroper? fileDroper; // 拖放窗口
        private Window Thumbnail = new(); // 缩略图窗口


        public Launcher()
        {
            InitializeComponent();

            // 默认 隐藏编辑界面
            DisplayEditGrid(false, true);
            DisplayEditGrid(false, false);

            // 绑定
            ListViewGroups.ItemsSource = ItemsGroups;

            // 读取数据
            Task.Run(async () => { await ReadDataAsync(); });
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e) // 关闭拖拽窗口
        {
            // 检查窗口是否已经存在且未关闭
            if (fileDroper != null && fileDroper.IsLoaded)
            {
                fileDroper.Close();
            }
        }



        private void ListViewGroups_SelectionChanged(object sender, SelectionChangedEventArgs e) // 选择分组，加载对应项目
        {
            // 根据分组显示选项
            if (ListViewGroups.SelectedIndex >= 0 && ListViewGroups.SelectedIndex < ItemsGroups.Count)
            {
                ListViewItems.ItemsSource = ItemsGroups[ListViewGroups.SelectedIndex].Items;
            }
            else
            {
                ListViewItems.ItemsSource = null;  // 或其他默认值
            }
        }


        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e) // 搜索分组中的项目
        {
            if (ItemsGroups.Count != 0)
            {
                string searchText = TextBoxSearch.Text.ToLower(); // 获取搜索文本并转换为小写以进行不区分大小写的搜索

                // 选择当前分组中的项目
                var items = ItemsGroups[ListViewGroups.SelectedIndex].Items;

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    // 过滤项目：找到包含搜索文本的项目
                    items = new ObservableCollection<ItemInfo>(items.Where(item => item.Text.ToLower().Contains(searchText)));
                }

                // 设置数据源
                ListViewItems.ItemsSource = items;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e) // 分组 扩大/还原
        {
            if (RowDefinitionGroups.Height == GridLength.Auto)
            {
                // 还原第一行的高度为原始值
                RowDefinitionGroups.Height = new GridLength(30);
                SvgAwesomeDraw.Icon = FontAwesome5.EFontAwesomeIcon.Solid_ChevronDown;
            }
            else
            {
                // 设置第一行的高度为自动
                RowDefinitionGroups.Height = GridLength.Auto;
                SvgAwesomeDraw.Icon = FontAwesome5.EFontAwesomeIcon.Solid_ChevronUp;
            }
        }
        private void DisplayEditGrid(bool Display, bool Groups) // 编辑区域 展开/收缩
        {
            if (Groups)
            {
                RowDefinitionGroupsEdit.Height = new(Display ? 30 : 0); // 分组

                RowDefinitionGroupsSearch.Height = new(Display ? 0 : 30); // 搜索
            }
            else
            {
                RowDefinitionItemsEdit.Height = new(Display ? 150 : 0);  // 项目

                RowDefinitionGroupsSearch.Height = new(Display ? 0 : 30); // 隐藏搜索，防止搜索改变索引
                if (fileDroper != null && fileDroper.IsLoaded) { fileDroper.Close(); } // 关闭已有窗口，防止在编辑时添加新文件
            }

            ListViewGroups.IsEnabled = !Display; // 禁止修改分组索引
            ListViewItems.IsEnabled = !Display; // 禁止修改项目索引
        }



        private void ButtonGroupImage_Click(object sender, RoutedEventArgs e) // 更换 分组图标
        {
            ChangeImage(ImageGroup);
        }
        private void ButtonItemImage_Click(object sender, RoutedEventArgs e) // 更换 项目图标
        {
            ChangeImage(ImageItem);
        }
        private static void ChangeImage(Image TempImage)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "选择图片或者从应用程序中提取",
                Filter = "所有文件 (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                try
                {
                    var imageExtensions = new[] { ".jpg", ".png", ".ico", ".bmp", ".gif" };
                    if (imageExtensions.Any(ext => selectedFilePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    {
                        // 创建 BitmapImage 并设置源
                        BitmapImage tempBitmapImage = new();
                        tempBitmapImage.BeginInit();
                        tempBitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        tempBitmapImage.UriSource = new Uri(selectedFilePath, UriKind.Absolute);
                        tempBitmapImage.EndInit();

                        // 将 BitmapImage 设置为图像控件的源
                        TempImage.Source = tempBitmapImage;
                    }
                    else
                    {
                        // 对于其他文件类型，获取并显示图标
                        TempImage.Source = ResourceIcon.GetIconWpfJumbo(selectedFilePath);
                    }
                }
                catch (Exception ex)
                {
                    NotificationControl.Add("ChangeImage：" + ex.Message);
                }
            }
        }
        private void ButtonGroupImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e) // 清除 分组图标
        {
            ImageGroup.Source = null;
        }
        private void ButtonItemImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e) // 清除 项目图标
        {
            ImageItem.Source = null;
        }

        private void ButtonGroupOk_Click(object sender, RoutedEventArgs e) // 分组确定 添加/编辑
        {
            ButtonOk(TextBlockGroup.Text);
        }
        private void ButtonItemOk_Click(object sender, RoutedEventArgs e) // 项目确定 添加/编辑
        {
            ButtonOk(TextBlockItem.Text);
        }
        private void ButtonOk(string Category)
        {
            // 判断当前是添加还是编辑、分组还是项目
            if (Category.Contains("分组"))
            {
                if (Category.Contains("添加"))
                {
                    ItemsGroups.Insert(ListViewGroups.SelectedIndex == -1 ? ListViewGroups.Items.Count : ListViewGroups.SelectedIndex + 1, new ItemGroupInfo // 插入当前分组至索引后面
                    {
                        Text = TextBoxGroup.Text,
                        Image = (BitmapSource)ImageGroup.Source,
                    });

                    ListViewGroups.SelectedIndex++;
                }
                else if (Category.Contains("编辑"))
                {
                    ItemsGroups[ListViewGroups.SelectedIndex].Text = TextBoxGroup.Text;
                    ItemsGroups[ListViewGroups.SelectedIndex].Image = (BitmapSource)ImageGroup.Source;

                    ListViewGroups.Items.Refresh(); // 手动刷新界面
                }

                DisplayEditGrid(false, true);
            }
            else
            {
                if (Category.Contains("添加"))
                {
                    ItemsGroups[ListViewGroups.SelectedIndex].Items.Insert(ListViewItems.SelectedIndex == -1 ? ListViewItems.Items.Count : ListViewItems.SelectedIndex + 1, new ItemInfo // 插入当前项目至索引后面
                    {
                        Text = TextBoxItem.Text,
                        Image = (BitmapSource)ImageItem.Source,

                        IsCommand = CheckBoxCommand.IsChecked ?? false,
                        IsCommandWindowsHide = CheckBoxCommandWindowsHide.IsChecked ?? false,
                        IsRelativePath = CheckBoxRelativePath.IsChecked ?? false,

                        Content = TextBoxEditContent.Text,
                        Content64 = TextBoxEditContent64.Text,
                    });
                }
                else if (Category.Contains("编辑"))
                {
                    ItemInfo TempItemInfo = ItemsGroups[ListViewGroups.SelectedIndex].Items[ListViewItems.SelectedIndex]; // 获取当前项目数据

                    TempItemInfo.Text = TextBoxItem.Text;
                    TempItemInfo.Image = (BitmapSource)ImageItem.Source;

                    TempItemInfo.IsCommand = CheckBoxCommand.IsChecked ?? false;
                    TempItemInfo.IsCommandWindowsHide = CheckBoxCommandWindowsHide.IsChecked ?? false;
                    TempItemInfo.IsRelativePath = CheckBoxRelativePath.IsChecked ?? false;

                    TempItemInfo.Content = TextBoxEditContent.Text;
                    TempItemInfo.Content64 = TextBoxEditContent64.Text;

                    ListViewItems.Items.Refresh(); // 手动刷新界面
                }

                DisplayEditGrid(false, false);
            }
        }

        private void ButtonGroupCancel_Click(object sender, RoutedEventArgs e) // 分组取消 添加/编辑
        {
            DisplayEditGrid(false, true);
        }
        private void ButtonItemCancel_Click(object sender, RoutedEventArgs e) // 项目取消 添加/编辑
        {
            DisplayEditGrid(false, false);
        }



        private void CheckBoxCommand_Checked(object sender, RoutedEventArgs e) // 路径 展开/收缩
        {
            RowDefinitionItemsPath_Relative.Height = new(0);
            RowDefinitionItemsPath_32.Height = new(0);
            RowDefinitionItemsPath_64.Height = new(0);
        }
        private void CheckBoxCommand_Unchecked(object sender, RoutedEventArgs e)
        {
            RowDefinitionItemsPath_Relative.Height = new(1, GridUnitType.Star);
            RowDefinitionItemsPath_32.Height = new(1, GridUnitType.Star);
            RowDefinitionItemsPath_64.Height = new(1, GridUnitType.Star);
        }


        private void ButtonItemImportFile_Click(object sender, RoutedEventArgs e) // 选择文件
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "选择一个文件",
                Filter = "所有文件 (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ParsingFiles(openFileDialog.FileName, TextBoxEditContent);
            }
        }
        private void ButtonItemImportFolder_Click(object sender, RoutedEventArgs e) // 选择文件夹
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new()
            {
                Description = "选择一个文件夹"
            };

            // 显示对话框
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ParsingFiles(folderBrowserDialog.SelectedPath, TextBoxEditContent);
            }
        }
        private void ButtonItemImportFile64_Click(object sender, RoutedEventArgs e) // 选择文件 64位启动程序
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "选择一个64位启动程序",
                Filter = "所有文件 (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ParsingFiles(openFileDialog.FileName, TextBoxEditContent64);
            }
        }
        public void ParsingFiles(string FilePath, TextBox textBox) // 解析文件(夹)
        {
            try
            {
                if (textBox == TextBoxEditContent)
                {
                    TextBoxItem.Text = System.IO.Path.GetFileNameWithoutExtension(FilePath); // 获取文件（夹）名
                    ImageItem.Source = ResourceIcon.GetIconWpfJumbo(FilePath);
                }
                CheckBoxCommand.IsChecked = false; // 类型肯定不是CMD命令
                if (FilePath.StartsWith(Directory.GetCurrentDirectory())) // 如果路径包含了当前运行目录则为相对路径
                {
                    CheckBoxRelativePath.IsChecked = true;
                    textBox.Text = FilePath.Replace(Directory.GetCurrentDirectory(), "").TrimStart('\\');
                }
                else
                {
                    CheckBoxRelativePath.IsChecked = false;
                    textBox.Text = FilePath;
                }
            }
            catch (Exception ex)
            {
                NotificationControl.Add("ParsingFiles捕获错误：" + ex.Message);
            }
        }



        private void ContextMenuGroup_Opened(object sender, RoutedEventArgs e) // 设置分组菜单项状态
        {
            if (ListViewGroups.SelectedIndex == -1) // 检查分组是否选中
            {
                MenuItem_EditGroup.IsEnabled = false;
                MenuItem_DeleteGroup.IsEnabled = false;
            }
            else
            {
                MenuItem_EditGroup.IsEnabled = true;
                MenuItem_DeleteGroup.IsEnabled = true;
            }
        }
        private void MenuItem_AddGroup_Click(object sender, RoutedEventArgs e) // 添加分组
        {
            TextBlockGroup.Text = "添加分组";
            TextBoxGroup.Text = "";
            ImageGroup.Source = null;

            DisplayEditGrid(true, true);
        }
        private void MenuItem_EditGroup_Click(object sender, RoutedEventArgs e) // 编辑分组
        {
            TextBlockGroup.Text = "编辑分组";
            TextBoxGroup.Text = ItemsGroups[ListViewGroups.SelectedIndex].Text;
            ImageGroup.Source = ItemsGroups[ListViewGroups.SelectedIndex].Image ?? null;

            DisplayEditGrid(true, true);
        }
        private void MenuItem_DeleteGroup_Click(object sender, RoutedEventArgs e) // 删除分组
        {
            int Index = ListViewGroups.SelectedIndex;
            ItemsGroups.RemoveAt(Index);
            if (Index > 0) { ListViewGroups.SelectedIndex = Index - 1; } // 删除 当前分组后，跳转前一个分组

            DisplayEditGrid(false, true);
            DisplayEditGrid(false, false);
        }


        private void ContextMenuItem_Opened(object sender, RoutedEventArgs e) // 设置项目菜单项状态
        {
            if (ItemsGroups.Count == 0) // 检查分组为空
            {
                e.Handled = true; // 取消当前控件的上下文菜单打开

                ListViewGroups.ContextMenu.UpdateDefaultStyle(); // 更新菜单背景样式
                ListViewGroups.ContextMenu.IsOpen = true; // 打开分组菜单
            }
            else
            {
                if (ListViewGroups.SelectedIndex == -1) return; // 按理说不会有分组没选中的情况
                if (ListViewItems.SelectedIndex == -1) // 检查项目是否选中
                {
                    MenuItem_EditItem.IsEnabled = false;
                    MenuItem_DeleteItem.IsEnabled = false;
                    MeunItem_MoveItem.IsEnabled = false;
                }
                else
                {
                    MenuItem_EditItem.IsEnabled = true;
                    MenuItem_DeleteItem.IsEnabled = true;
                    MeunItem_MoveItem.IsEnabled = true;
                    SetMobileItemGroups(); // 更新 移动项目 菜单项
                }
            }
        }
        private void MenuItem_AddItem_Click(object sender, RoutedEventArgs e) // 添加项目
        {
            TextBlockItem.Text = "添加项目";
            TextBoxItem.Text = "";
            ImageItem.Source = null;

            CheckBoxCommand.IsChecked = false;
            CheckBoxRelativePath.IsChecked = false;
            TextBoxEditContent.Text = "";
            TextBoxEditContent64.Text = "";

            DisplayEditGrid(true, false);
        }
        private void MenuItem_EditItem_Click(object sender, RoutedEventArgs e) // 编辑项目
        {
            TextBlockItem.Text = "编辑项目";

            ItemInfo TempItemInfo = ItemsGroups[ListViewGroups.SelectedIndex].Items[ListViewItems.SelectedIndex]; // 获取当前项目数据

            TextBoxItem.Text = TempItemInfo.Text;
            ImageItem.Source = TempItemInfo.Image ?? null;

            CheckBoxCommand.IsChecked = TempItemInfo.IsCommand;
            CheckBoxCommandWindowsHide.IsChecked = TempItemInfo.IsCommandWindowsHide;
            CheckBoxRelativePath.IsChecked = TempItemInfo.IsRelativePath;
            TextBoxEditContent.Text = TempItemInfo.Content;
            TextBoxEditContent64.Text = TempItemInfo.Content64;

            DisplayEditGrid(true, false);
        }
        private void MenuItem_DeleteItem_Click(object sender, RoutedEventArgs e) // 删除项目
        {
            ItemsGroups[ListViewGroups.SelectedIndex].Items.RemoveAt(ListViewItems.SelectedIndex);

            DisplayEditGrid(false, false);
        }



        private void SetMobileItemGroups()// 设置移动项目分组；绑定ItemsGroups
        {
            MeunItem_MoveItem.Items.Clear(); // 清除现有的子菜单项

            // 添加子菜单项
            foreach (var groupItem in ItemsGroups)
            {
                var subMenuItem = new MenuItem
                {
                    // 设置子菜单项的样式
                    Style = FindResource("MenuItemStyle") as Style
                };

                // 创建 StackPanel 来容纳图像和文本
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                // 创建图像元素，若空不创建
                if (groupItem.Image != null)
                {
                    var image = new Image
                    {
                        Source = groupItem.Image,
                        Width = 16, // 设置图像的宽度
                        Height = 16 // 设置图像的高度
                    };
                    stackPanel.Children.Add(image); // 将图像添加到 StackPanel
                }

                // 创建文本块
                var textBlock = new TextBlock
                {
                    Text = groupItem.Text,
                    VerticalAlignment = VerticalAlignment.Center
                };
                stackPanel.Children.Add(textBlock); // 将文本块添加到 StackPanel

                // 将 StackPanel 添加到子菜单
                subMenuItem.Header = stackPanel;

                subMenuItem.DataContext = groupItem;
                subMenuItem.Click += MenuItem_MoveSubItem_Click;

                MeunItem_MoveItem.Items.Add(subMenuItem);
            }
        }
        private void MenuItem_MoveSubItem_Click(object sender, RoutedEventArgs e) // 移动项目置新分组
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is ItemGroupInfo targetGroup)
            {
                // 获取选定的项目
                if (ListViewItems.SelectedItem is ItemInfo selectedItem)
                {
                    // 获取当前分组
                    var currentGroup = ItemsGroups[ListViewGroups.SelectedIndex];

                    // 从当前分组中移除选定的项目
                    currentGroup.Items.Remove(selectedItem);

                    // 将选定的项目添加到目标分组
                    targetGroup.Items.Add(selectedItem);

                    DisplayEditGrid(false, true);
                    DisplayEditGrid(false, false);
                }
            }
        }


        private async void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) // 拖动项目判断
        {
            //Console.WriteLine("Sender type: " + sender.GetType().ToString());

            if (sender is ListViewItem Item)
            {
                if (await WaitForLongPressAsync(1000)) // 等待时间
                {
                    ShowThumbnail(Item); // 显示缩略图

                    //  这里区分两个ListView
                    if (Item.Content is ItemGroupInfo tempGroup)
                    {
                        DragDrop.DoDragDrop(Item, tempGroup, DragDropEffects.Move);
                    }
                    else if (Item.Content is ItemInfo TempItemInfo)
                    {
                        DragDrop.DoDragDrop(Item, TempItemInfo, DragDropEffects.Move);
                    }

                    Thumbnail.Close(); // 关闭缩略图
                }
            }
        }
        private static async Task<bool> WaitForLongPressAsync(int milliseconds) // 判断是否为长按
        {
            DateTime startTime = DateTime.Now;
            while (true)
            {
                await Task.Delay(100); // 每100毫秒检查一次

                if ((DateTime.Now - startTime).TotalMilliseconds >= milliseconds)
                {
                    return true; // 如果超过指定的时间，则视为长按
                }

                if (!Mouse.LeftButton.Equals(MouseButtonState.Pressed))
                {
                    return false; // 如果鼠标松开，则视为未长按
                }
            }
        }
        private void ListViewItem_PreviewDrop(object sender, DragEventArgs e) // 拖动项目置新位置
        {
            //Console.WriteLine("Sender type: " + sender.GetType().ToString());

            if (sender is ListViewItem)
            {
                // 获取来源项目
                ListView TempListView;
                int SourceIndex;
                if (e.Data.GetData(typeof(ItemGroupInfo)) is ItemGroupInfo TempItemGroupInfo)
                {
                    TempListView = ListViewGroups;
                    SourceIndex = TempListView.Items.IndexOf(TempItemGroupInfo);
                }
                else if (e.Data.GetData(typeof(ItemInfo)) is ItemInfo TempItemInfo)
                {
                    TempListView = ListViewItems;
                    SourceIndex = TempListView.Items.IndexOf(TempItemInfo);
                }
                else
                {
                    return;
                }

                // 获取目标项目
                var TargetItem = TempListView.InputHitTest(e.GetPosition(TempListView)) as DependencyObject;
                int TargetIndex = -1;
                while (TargetItem != null)
                {
                    if (TargetItem is ListViewItem TempListViewItem)
                    {
                        TargetIndex = TempListView.Items.IndexOf(TempListViewItem.Content);
                        break;
                    }

                    TargetItem = VisualTreeHelper.GetParent(TargetItem);
                }

                // 移动位置
                if (SourceIndex != -1 && TargetIndex != -1 && SourceIndex != TargetIndex) // 不能等于-1，拖动前后位置不变
                {
                    if (TempListView == ListViewGroups) // 判断哪一个ListView
                    {
                        ItemsGroups.Move(SourceIndex, TargetIndex);
                    }
                    else
                    {
                        ItemsGroups[ListViewGroups.SelectedIndex].Items.Move(SourceIndex, TargetIndex);
                    }
                }
            }
        }


        private void ShowThumbnail(ListViewItem Item) // 显示缩略图
        {
            //取项目控件截图
            RenderTargetBitmap TempBitmap = new((int)Item.ActualWidth, (int)Item.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual TempDrawingVisual = new(); // 不用DrawingVisual只能截图第一个项目，后面项目偏移导致截图空白
            using (DrawingContext TempDrawingContext = TempDrawingVisual.RenderOpen())
            {
                VisualBrush brush = new(Item);
                TempDrawingContext.DrawRectangle(brush, null, new Rect(new Point(0, 0), new Size(Item.ActualWidth, Item.ActualHeight)));
            }

            TempBitmap.Render(TempDrawingVisual);

            // 创建图像
            var TempImage = new Image
            {
                Width = Item.ActualWidth / 1.3,
                Height = Item.ActualHeight / 1.3,
                Source = TempBitmap
            };

            // 获取鼠标相对于窗口的位置
            Point MousePosition = PointToScreen(Mouse.GetPosition(this));

            // 创建窗口
            Thumbnail = new Window
            {
                Content = TempImage,
                Width = TempImage.Width,
                Height = TempImage.Height,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = MousePosition.X,
                Top = MousePosition.Y,
            };

            Thumbnail.Show(); // 显示
            _ = MoveThumbnailAsync(); // 鼠标移动事件
        }
        private async Task MoveThumbnailAsync() // 缩略图跟随鼠标位置
        {
            while (Thumbnail.IsVisible)
            {
                Dispatcher.Invoke(() =>
                {
                    Point cursorPosition = CursorHelper.GetCursorPosition();
                    Thumbnail.Left = cursorPosition.X + 5;
                    Thumbnail.Top = cursorPosition.Y + 15;
                });

                await Task.Delay(10); // 等待一小段时间再更新位置
            }
        }
        public static class CursorHelper // 获取鼠标全局位置
        {
            [DllImport("user32.dll")]
            private static extern bool GetCursorPos(out POINT lpPoint);

            private struct POINT
            {
                public int X;
                public int Y;
            }

            public static Point GetCursorPosition()
            {
                GetCursorPos(out POINT p);
                return new Point(p.X, p.Y);
            }
        }



        private void ButtonExternalDrag_Click(object sender, RoutedEventArgs e) // 调用外部文件拖拽功能
        {
            // 检查窗口是否已经存在且未关闭
            if (fileDroper != null && fileDroper.IsLoaded)
            {
                fileDroper.Activate(); // 激活已有窗口
            }
            else
            {
                // 创建新窗口
                fileDroper = new ElevatedFileDroper();
                fileDroper.Show();
                fileDroper.droper.DragDrop += (innerSender, innerE) =>
                {
                    if (ItemsGroups.Count == 0)
                    {
                        NotificationControl.Add("目前还有分组呢，先添加一分组吧~");
                    }
                    else
                    {
                        ParsingExternalFiles(fileDroper.droper.DropFilePaths); // 多个文件或目录进行解析
                    }
                };
            }
        }
        public void ParsingExternalFiles(string[] FilePaths) // 解析外部文件(夹)
        {
            try
            {
                for (int i = 0; i < FilePaths.Length; i++)
                {
                    ItemsGroups[ListViewGroups.SelectedIndex].Items.Insert(ListViewItems.SelectedIndex == -1 ? ListViewItems.Items.Count : ListViewItems.SelectedIndex + 1, new ItemInfo // 插入当前项目至索引后面
                    {
                        Text = System.IO.Path.GetFileNameWithoutExtension(FilePaths[i]),
                        Image = ResourceIcon.GetIconWpfJumbo(FilePaths[i]),

                        IsCommand = false,
                        IsCommandWindowsHide = false,
                        IsRelativePath = FilePaths[i].StartsWith(Directory.GetCurrentDirectory()),

                        Content = FilePaths[i].StartsWith(Directory.GetCurrentDirectory()) ? FilePaths[i].Replace(Directory.GetCurrentDirectory(), "").TrimStart('\\') : FilePaths[i],
                    });
                }
            }
            catch (Exception ex)
            {
                NotificationControl.Add("ParsingExternalFiles捕获错误：" + ex.Message);
            }
        }



        private void ListViewItems_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) // 点击空白处取消选中项
        {

            // 检查是否点击了ListView的项（item）
            if (e.OriginalSource is FrameworkElement element && element.DataContext != null)
            {
                // 如果点击了项，则不执行任何操作
                return;
            }

            // 如果点击的是空白处，则取消选中所有项
            ListViewItems.SelectedItem = null;
        }
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) // 项目双击运行
        {
            // 区分项目 ListViewItems
            if (sender is ListViewItem Item && Item.Content is ItemInfo TempItemInfo)
            {
                if (!TempItemInfo.IsCommand)
                {
                    string TempPath;
                    if (string.IsNullOrEmpty(TempItemInfo.Content64))
                    {
                        TempPath = TempItemInfo.Content;
                    }
                    else
                    {
                        TempPath = Environment.Is64BitOperatingSystem ? TempItemInfo.Content64 : TempItemInfo.Content; // 区分64位
                    }

                    if (TempItemInfo.IsRelativePath) { TempPath = Path.Combine(Directory.GetCurrentDirectory(), TempPath); } // 区分相对路径

                    Process.Start(new ProcessStartInfo("cmd.exe", $"/c start \"\" \"{TempPath}\"")
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        UseShellExecute = true
                    });
                }
                else
                {
                    Process.Start(new ProcessStartInfo("cmd", "/c " + TempItemInfo.Content)
                    {
                        WindowStyle = TempItemInfo.IsCommandWindowsHide ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                        UseShellExecute = true
                    });
                }

                NotificationControl.Add($"运行【{TempItemInfo.Text}】~");
            }
        }



        public async Task SaveDataAsync() // 保存数据
        {
            try
            {
                // 重置数据目录
                if (Directory.Exists(GlobalDir.LauncherDir))
                {
                    Directory.Delete(GlobalDir.LauncherDir, true); // 删除目录及其所有子目录和文件
                    Directory.CreateDirectory(GlobalDir.LauncherDir); // 重新创建目录
                }

                ObservableCollection<ItemGroupInfo> SerializedGroups = new(); // 实例化分组
                for (int i = 0; i < ItemsGroups.Count; i++)
                {
                    var tempGroup = ItemsGroups[i]; // 获取分组

                    if (tempGroup.Image != null) // 保存分组图标
                    {
                        tempGroup.ImagePath = i.ToString() + ".png"; // 分组索引.png
                        SaveImageAsync(tempGroup.Image, Path.Combine(GlobalDir.LauncherDir, tempGroup.ImagePath));
                    }

                    SerializedGroups.Add(new()
                    {
                        Text = tempGroup.Text,
                        ImagePath = tempGroup.ImagePath,
                        Items = new()
                    });

                    for (int ii = 0; ii < tempGroup.Items.Count; ii++)
                    {
                        var tempItem = tempGroup.Items[ii]; // 获取项目

                        if (tempItem.Image != null) // 保存项目图标
                        {
                            tempItem.ImagePath = i.ToString() + "-" + ii.ToString() + ".png"; // 分组索引-项目索引.png
                            SaveImageAsync(tempItem.Image, Path.Combine(GlobalDir.LauncherDir, tempItem.ImagePath));
                        }

                        SerializedGroups[i].Items.Add(new()
                        {
                            Text = tempItem.Text,
                            ImagePath = tempItem.ImagePath,
                            Content = tempItem.Content,
                            Content64 = tempItem.Content64,
                            IsCommand = tempItem.IsCommand,
                            IsCommandWindowsHide = tempItem.IsCommandWindowsHide,
                            IsRelativePath = tempItem.IsRelativePath
                        });
                    }
                }

                // 保存至Json，且每项不能为空并格式化
                string jsonData = JsonSerializer.Serialize(SerializedGroups, App.jsonOptions);
                await File.WriteAllTextAsync(GlobalDir.LauncherJson, jsonData, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                NotificationControl.Add("SaveData捕获错误：" + ex.Message);
            }
        }

        private static void SaveImageAsync(BitmapSource TempImage, string TempImagePath) // 异步保存图像文件
        {
            // 获取图像的编码器
            BitmapEncoder TempEncoder = new PngBitmapEncoder(); // 使用PNG编码器

            // 将图像添加到编码器中
            TempEncoder.Frames.Add(BitmapFrame.Create(TempImage));

            // 创建文件并异步保存图像
            using FileStream fileStream = new(TempImagePath, FileMode.Create);
            TempEncoder.Save(fileStream);
        }

        private async Task ReadDataAsync() // 读取数据
        {
            try
            {
                if (File.Exists(GlobalDir.LauncherJson))
                {
                    string jsonData = await File.ReadAllTextAsync(GlobalDir.LauncherJson, Encoding.UTF8); // 异步读取文件内容
                    var newItemsGroups = JsonSerializer.Deserialize<ObservableCollection<ItemGroupInfo>>(jsonData) ?? new(); // 反实例化

                    // 首先在UI线程之外处理所有数据和异步操作
                    foreach (var tempGroup in newItemsGroups)
                    {
                        tempGroup.Image = await ReadImageAsync(Path.Combine(GlobalDir.LauncherDir, tempGroup.ImagePath));

                        foreach (var tempItem in tempGroup.Items)
                        {
                            tempItem.Image = await ReadImageAsync(Path.Combine(GlobalDir.LauncherDir, tempItem.ImagePath));
                        }
                    }

                    // 然后在UI线程上更新UI
                    Dispatcher.Invoke(() =>
                    {
                        ItemsGroups.Clear(); // 清空原有数据

                        // 添加新读取的数据
                        foreach (var tempGroup in newItemsGroups)
                        {
                            ItemsGroups.Add(tempGroup);
                        }

                        ListViewGroups.SelectedIndex = 0; // 默认选择第一个分组
                    });
                }
            }
            catch (Exception ex)
            {
                // 异常处理：在UI线程上显示错误通知
                NotificationControl.Add("读取配置时发生错误：" + ex.Message);
            }
        }

        private static async Task<BitmapSource?> ReadImageAsync(string TempImagePath) // 异步读取图像文件
        {
            if (File.Exists(TempImagePath))
            {
                BitmapImage? TempImage = null;
                // 读取图像文件的字节到内存中
                byte[] imageBytes = await File.ReadAllBytesAsync(TempImagePath);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    using MemoryStream stream = new(imageBytes);
                    TempImage = new BitmapImage();
                    TempImage.BeginInit();
                    TempImage.CacheOption = BitmapCacheOption.OnLoad;
                    TempImage.StreamSource = stream;
                    TempImage.EndInit();
                    // 强制图像立即加载并缓存，避免在图像使用时延迟加载
                    TempImage.Freeze(); // 使图像在跨线程时不会出错
                });

                return TempImage;
            }
            return null;
        }

    }
}
