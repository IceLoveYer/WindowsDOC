using System.ComponentModel;

namespace WindowsDOC.Pages.SystemConfig
{
    // 系统占用率 数据绑定
    public class UsageItemModel : INotifyPropertyChanged
    {
        private int? _progress;
        public string Name { get; set; }
        public string? ProgressText { get; set; }

        public int? Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = (int?)value;
                    OnPropertyChanged(nameof(Progress));

                    ProgressText = $"{Name}\n{Progress}%";
                    OnPropertyChanged(nameof(ProgressText));
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public UsageItemModel(string name, int value)
        {
            Name = name;
            Progress = value;
        }
    }
}
