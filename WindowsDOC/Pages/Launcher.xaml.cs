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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsDOC.Call;

namespace WindowsDOC.Pages
{

    /// <summary>
    /// Launcher.xaml 的交互逻辑
    /// </summary>
    public partial class Launcher : Page
    {
        // 定义项目为全局
        private readonly string DataDirectory; // 当前工作目录
        private Window Thumbnail = new(); // 缩略图窗口

        private ObservableCollection<MyItemGroup> ItemsGroups = new(); // 这里用ObservableCollection会自动刷新界面
        private readonly ElevatedFileDroper FileDroper = new(); // 拖放窗口


        public Launcher()
        {
            // 设置数据目录
            DataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data\\Launcher");
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }
            //Console.WriteLine("当前数据目录：" + DataDirectory);

            InitializeComponent();


            // 读取数据
            ReadData();

            ListViewGroups.ItemsSource = ItemsGroups;
            ListViewGroups.SelectedIndex = 0;

            DisplayEdit(false, true); // 默认 隐藏编辑界面
            DisplayEdit(false, false);
        }



        private class MyItemGroup // 定义分组类型
        {
            public BitmapSource? Image { get; set; } // 图像
            public string ImagePath { get; set; } = ""; // 图像文件路径
            public string Text { get; set; } = ""; // 标题
            public ObservableCollection<MyItem> Items { get; set; } = new();// 项目
        }
        private class MyItem // 定义项目类型
        {
            public BitmapSource? Image { get; set; } // 图像
            public string ImagePath { get; set; } = ""; // 图像文件路径
            public string Text { get; set; } = ""; // 标题
            public string Content { get; set; } = ""; // 运行路径或命令
            public MyItemType ItemType { get; set; } = MyItemType.Path; // 项目类型
            public MyItemPathType ItemPathType { get; set; } = MyItemPathType.Absolute; // 类型

        }
        private enum MyItemType
        {
            Path, // 路径
            Command // 命令
        }
        private enum MyItemPathType
        {
            Absolute, // 绝对
            Relative // 相对
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
                    items = new ObservableCollection<MyItem>(items.Where(item => item.Text.ToLower().Contains(searchText)));
                }

                // 设置数据源
                ListViewItems.ItemsSource = items;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e) // 展开收缩分组
        {
            if (GridLayout.RowDefinitions[0].Height == GridLength.Auto)
            {
                // 还原第一行的高度为原始值
                GridLayout.RowDefinitions[0].Height = new GridLength(30);
                SvgAwesomeDraw.Icon = FontAwesome5.EFontAwesomeIcon.Solid_ChevronDown;
            }
            else
            {
                // 设置第一行的高度为自动
                GridLayout.RowDefinitions[0].Height = GridLength.Auto;
                SvgAwesomeDraw.Icon = FontAwesome5.EFontAwesomeIcon.Solid_ChevronUp;
            }
        }
        private void DisplayEdit(bool Display, bool Groups) // 展开收缩编辑区域
        {
            if (Groups)
            {
                GridLayout.RowDefinitions[1].Height = new(Display ? 30 : 0); // 分组

                GridLayout.RowDefinitions[2].Height = new(Display ? 0 : 30); // 搜索
            }
            else
            {
                GridLayout.RowDefinitions[4].Height = new(Display ? 130 : 0);  // 项目
            }
        }



        private void ButtonGroupImage_Click(object sender, RoutedEventArgs e) // 更换图标
        {
            ChangeImage(ImageGroup);
        }
        private void ButtonItemImage_Click(object sender, RoutedEventArgs e)
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



        private void ButtonGroupOk_Click(object sender, RoutedEventArgs e) // 确定添加或编辑
        {
            ButtonOk(TextBlockGroup.Text);
        }
        private void ButtonItemOk_Click(object sender, RoutedEventArgs e)
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
                    ItemsGroups.Insert(ListViewGroups.SelectedIndex == -1 ? ListViewGroups.Items.Count : ListViewGroups.SelectedIndex + 1, new MyItemGroup // 插入当前分组至索引后面
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

                DisplayEdit(false, true);
            }
            else
            {
                if (Category.Contains("添加"))
                {
                    ItemsGroups[ListViewGroups.SelectedIndex].Items.Insert(ListViewItems.SelectedIndex == -1 ? ListViewItems.Items.Count : ListViewItems.SelectedIndex + 1, new MyItem // 插入当前项目至索引后面
                    {
                        Text = TextBoxItem.Text,
                        Image = (BitmapSource)ImageItem.Source,

                        ItemType = RadioButtonPath.IsChecked == true ? MyItemType.Path : MyItemType.Command,
                        ItemPathType = RadioButtoAbsolutePath.IsChecked == true ? MyItemPathType.Absolute : MyItemPathType.Relative,

                        Content = TextBoxEditContent.Text,
                    });
                }
                else if (Category.Contains("编辑"))
                {
                    MyItem TempMyItem = ItemsGroups[ListViewGroups.SelectedIndex].Items[ListViewItems.SelectedIndex]; // 获取当前项目数据

                    TempMyItem.Text = TextBoxItem.Text;
                    TempMyItem.Image = (BitmapSource)ImageItem.Source;

                    TempMyItem.ItemType = RadioButtonPath.IsChecked == true ? MyItemType.Path : MyItemType.Command;
                    TempMyItem.ItemPathType = RadioButtoAbsolutePath.IsChecked == true ? MyItemPathType.Absolute : MyItemPathType.Relative;

                    TempMyItem.Content = TextBoxEditContent.Text;

                    ListViewItems.Items.Refresh(); // 手动刷新界面
                }

                DisplayEdit(false, false);
            }
        }

        private void ButtonGroupCancel_Click(object sender, RoutedEventArgs e) // 取消添加或编辑
        {
            DisplayEdit(false, true);
        }
        private void ButtonItemCancel_Click(object sender, RoutedEventArgs e)
        {
            DisplayEdit(false, false);
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
                // 只取第一个文件或目录
                ParsingFiles(openFileDialog.FileName);
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
                // 获取选择的文件夹路径
                ParsingFiles(folderBrowserDialog.SelectedPath);
            }
        }


        private void ContextMenuGroup_Opened(object sender, RoutedEventArgs e) // 设置分组菜单项状态
        {
            if (ItemsGroups.Count == 0) // 检查分组为空
            {
                MenuItem_EditGroup.IsEnabled = false;
                MenuItem_DeleteGroup.IsEnabled = false;
            }
        }
        private void MenuItem_AddGroup_Click(object sender, RoutedEventArgs e) // 添加分组
        {
            TextBlockGroup.Text = "添加分组";
            TextBoxGroup.Text = "";
            ImageGroup.Source = null;

            DisplayEdit(true, true);
        }
        private void MenuItem_EditGroup_Click(object sender, RoutedEventArgs e) // 编辑分组
        {
            TextBlockGroup.Text = "编辑分组";
            TextBoxGroup.Text = ItemsGroups[ListViewGroups.SelectedIndex].Text;
            ImageGroup.Source = ItemsGroups[ListViewGroups.SelectedIndex].Image ?? null;

            DisplayEdit(true, true);
        }
        private void MenuItem_DeleteGroup_Click(object sender, RoutedEventArgs e) // 删除分组
        {
            ItemsGroups.RemoveAt(ListViewGroups.SelectedIndex);

            DisplayEdit(false, true);
            DisplayEdit(false, false);
        }



        private void ContextMenuItem_Opened(object sender, RoutedEventArgs e) // 设置项目菜单项状态
        {
            if (ItemsGroups.Count == 0) // 检查分组为空
            {
                e.Handled = true; // 取消当前控件的上下文菜单打开
                ListViewGroups.ContextMenu.IsOpen = true; // 打开分组菜单
            }
            else
            {
                if (ItemsGroups[ListViewGroups.SelectedIndex].Items.Count == 0) // 检查项目为空
                {
                    MenuItem_EditItem.IsEnabled = false;
                    MenuItem_DeleteItem.IsEnabled = false;
                    MeunItem_MoveItem.IsEnabled = false;
                }
                else
                {
                    SetMobileItemGroups(); // 更新 移动项目 菜单项
                }
            }
        }
        private void MenuItem_AddItem_Click(object sender, RoutedEventArgs e) // 添加项目
        {
            TextBlockItem.Text = "添加项目";
            TextBoxItem.Text = "";
            ImageItem.Source = null;

            RadioButtonPath.IsChecked = true;
            RadioButtoAbsolutePath.IsChecked = true;
            TextBoxEditContent.Text = "";

            DisplayEdit(true, false);
        }
        private void MenuItem_EditItem_Click(object sender, RoutedEventArgs e) // 编辑项目
        {
            TextBlockItem.Text = "编辑项目";

            MyItem TempMyItem = ItemsGroups[ListViewGroups.SelectedIndex].Items[ListViewItems.SelectedIndex]; // 获取当前项目数据

            TextBoxItem.Text = TempMyItem.Text;
            ImageItem.Source = TempMyItem.Image ?? null;

            if (TempMyItem.ItemType == MyItemType.Path)
            {
                RadioButtonPath.IsChecked = true;

                if (TempMyItem.ItemPathType == MyItemPathType.Absolute)
                {
                    RadioButtoAbsolutePath.IsChecked = true;
                }
                else
                {
                    RadioButtonRelativePath.IsChecked = true;
                }
            }
            else
            {
                RadioButtonCommand.IsChecked = true;
            }
            TextBoxEditContent.Text = TempMyItem.Content;

            DisplayEdit(true, false);
        }
        private void MenuItem_DeleteItem_Click(object sender, RoutedEventArgs e) // 删除项目
        {
            ItemsGroups[ListViewGroups.SelectedIndex].Items.RemoveAt(ListViewItems.SelectedIndex);

            DisplayEdit(false, false);
        }



        private async void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) // 拖动项目判断
        {
            //Console.WriteLine("Sender type: " + sender.GetType().ToString());

            if (sender is ListViewItem Item)
            {
                if (await WaitForLongPressAsync(300)) // 等待时间
                {
                    ShowThumbnail(Item); // 显示缩略图

                    //  这里区分两个ListView
                    if (Item.Content is MyItemGroup TempGroup)
                    {
                        DragDrop.DoDragDrop(Item, TempGroup, DragDropEffects.Move);
                    }
                    else if (Item.Content is MyItem TempMyItem)
                    {
                        DragDrop.DoDragDrop(Item, TempMyItem, DragDropEffects.Move);
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
                if (e.Data.GetData(typeof(MyItemGroup)) is MyItemGroup TempMyItemGroup)
                {
                    TempListView = ListViewGroups;
                    SourceIndex = TempListView.Items.IndexOf(TempMyItemGroup);
                }
                else if (e.Data.GetData(typeof(MyItem)) is MyItem TempMyItem)
                {
                    TempListView = ListViewItems;
                    SourceIndex = TempListView.Items.IndexOf(TempMyItem);
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

                //Console.WriteLine("来源：" + SourceIndex + " 目标：" + TargetIndex);

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



        private void CheckBoxExternalDragDrop_Checked(object sender, RoutedEventArgs e)// 启用外部文件拖拽功能
        {
            // 启用拖拽功能
            FileDroper.HwndSource = (HwndSource)PresentationSource.FromVisual(ListViewGroups);
            FileDroper.AddHook();
            FileDroper.DragDrop += (sender, e) =>
            {
                ParsingFiles(FileDroper.DropFilePaths[0]); // 只取第一个文件或目录进行解析
            };
        }
        private void CheckBoxExternalDragDrop_Unchecked(object sender, RoutedEventArgs e)// 禁用外部文件拖拽功能
        {
            FileDroper.RemoveHook();
        }
        public void ParsingFiles(string FilePath) // 解析外部文件(夹)
        {
            if (ItemsGroups.Count == 0)
            {
                NotificationControl.Add("目前还有分组呢，先添加一分组吧~");
            }
            else
            {
                try
                {
                    TextBlockItem.Text = "添加项目";

                    TextBoxItem.Text = System.IO.Path.GetFileName(FilePath); // 获取文件名或文件夹名

                    RadioButtonPath.IsChecked = true; // 类型肯定是路径
                    if (FilePath.StartsWith(Directory.GetCurrentDirectory())) // 如果路径包含了当前运行目录则为相对路径
                    {
                        RadioButtonRelativePath.IsChecked = true;
                        TextBoxEditContent.Text = FilePath.Replace(Directory.GetCurrentDirectory(), "").TrimStart('\\');
                    }
                    else
                    {
                        RadioButtoAbsolutePath.IsChecked = true;
                        TextBoxEditContent.Text = FilePath;
                    }

                    ImageItem.Source = ResourceIcon.GetIconWpfJumbo(FilePath); // 获取高清图标
                }
                catch (Exception ex)
                {
                    NotificationControl.Add("ParsingFiles：" + ex.Message);
                }

                DisplayEdit(true, false);
            }
        }


        private void ListViewItems_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) // 点击空白处取消选中项
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
                await Dispatcher.InvokeAsync(() =>
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
            if (sender is MenuItem menuItem && menuItem.DataContext is MyItemGroup targetGroup)
            {
                // 获取选定的项目
                if (ListViewItems.SelectedItem is MyItem selectedItem)
                {
                    // 获取当前分组
                    var currentGroup = ItemsGroups[ListViewGroups.SelectedIndex];

                    // 从当前分组中移除选定的项目
                    currentGroup.Items.Remove(selectedItem);

                    // 将选定的项目添加到目标分组
                    targetGroup.Items.Add(selectedItem);

                    DisplayEdit(false, true);
                    DisplayEdit(false, false);
                }
            }
        }



        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) // 项目双击运行
        {
            // 区分项目 ListViewItems
            if (sender is ListViewItem Item && Item.Content is MyItem TempMyItem)
            {
                if (TempMyItem.ItemType == MyItemType.Path)
                {
                    string TempPath = TempMyItem.ItemPathType == MyItemPathType.Absolute ? TempMyItem.Content : Path.Combine(Directory.GetCurrentDirectory(), TempMyItem.Content);
                    Process.Start(new ProcessStartInfo("cmd.exe", $"/c start \"\" \"{TempPath}\"") { WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, UseShellExecute = true });
                }
                else
                {
                    Process.Start(new ProcessStartInfo("cmd", "/c " + TempMyItem.Content));
                }
            }
        }



        public void SaveData() // 保存
        {
            // 重置数据目录
            if (Directory.Exists(DataDirectory))
            {
                Directory.Delete(DataDirectory, true); // 删除目录及其所有子目录和文件
                Directory.CreateDirectory(DataDirectory); // 重新创建目录
            }

            // 分组不得为0
            if (ItemsGroups.Count > 0)
            {
                ObservableCollection<MyItemGroup> SerializedGroups = new(); // 实例化分组
                for (int i = 0; i < ItemsGroups.Count; i++)
                {
                    var TempGroup = ItemsGroups[i]; // 获取分组

                    ObservableCollection<MyItem> SerializedItems = new(); // 实例化项目
                    if (TempGroup.Items != null)
                    {
                        for (int ii = 0; ii < TempGroup.Items.Count; ii++)
                        {
                            var TempItem = TempGroup.Items[ii]; // 获取分组中的项目

                            if (TempItem.Image != null) // 保存项目图标
                            {
                                TempItem.ImagePath = i.ToString() + "-" + ii.ToString() + ".png"; // 分组索引-项目索引.png
                                SaveImage(TempItem.Image, Path.Combine(DataDirectory, TempItem.ImagePath));
                            }

                            SerializedItems.Add(new MyItem
                            {
                                ImagePath = TempItem.ImagePath,
                                Text = TempItem.Text,
                                Content = TempItem.Content,
                                ItemType = TempItem.ItemType,
                                ItemPathType = TempItem.ItemPathType
                            });
                        }
                    }

                    if (TempGroup.Image != null) // 保存分组图标
                    {
                        TempGroup.ImagePath = i.ToString() + ".png"; // 分组索引.png
                        SaveImage(TempGroup.Image, Path.Combine(DataDirectory, TempGroup.ImagePath));
                    }

                    SerializedGroups.Add(new MyItemGroup
                    {
                        ImagePath = TempGroup.ImagePath,
                        Text = TempGroup.Text,
                        Items = SerializedItems
                    });
                }

                // 保存至Json，且每项不能为空并格式化
                string JsonFilePath = Path.Combine(DataDirectory, "Data.json");
                string JsonData = JsonSerializer.Serialize(SerializedGroups, App.jsonOptions);
                File.WriteAllText(JsonFilePath, JsonData, Encoding.UTF8);
            }
        }
        private static void SaveImage(BitmapSource TempImage, string TempImagePath) // 保存图像文件
        {
            // 获取图像的编码器
            BitmapEncoder TempEncoder = new PngBitmapEncoder(); // 使用PNG编码器

            // 将图像添加到编码器中
            TempEncoder.Frames.Add(BitmapFrame.Create(TempImage));

            // 创建文件并保存图像
            using FileStream fileStream = new(TempImagePath, FileMode.Create);
            TempEncoder.Save(fileStream);
        }

        private void ReadData() // 读取
        {
            string JsonFilePath = Path.Combine(DataDirectory, "Data.json");
            if (File.Exists(JsonFilePath))
            {
                string JsonData = File.ReadAllText(JsonFilePath, Encoding.UTF8);
                ItemsGroups = JsonSerializer.Deserialize<ObservableCollection<MyItemGroup>>(JsonData) ?? new(); // 反实例化
                if (ItemsGroups.Count > 0)
                {
                    // 读取图像
                    foreach (var TempGroup in ItemsGroups)
                    {
                        TempGroup.Image = ReadImage(Path.Combine(DataDirectory, TempGroup.ImagePath));

                        if (TempGroup.Items != null)
                        {
                            foreach (var TempItem in TempGroup.Items)
                            {
                                TempItem.Image = ReadImage(Path.Combine(DataDirectory, TempItem.ImagePath));
                            }
                        }
                    }
                }
            }
        }
        private static BitmapSource? ReadImage(string TempImagePath) // 读取图像文件
        {
            if (File.Exists(TempImagePath))
            {
                using FileStream stream = new(TempImagePath, FileMode.Open, FileAccess.Read);
                BitmapImage TempImage = new();
                TempImage.BeginInit();
                TempImage.CacheOption = BitmapCacheOption.OnLoad;
                TempImage.StreamSource = stream;
                TempImage.EndInit();
                return TempImage;
            }
            return null;
        }
    }
}
