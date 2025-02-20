using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace WpfApp1
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _folderName = string.Empty;
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            SetDefaultFolderName();
            SetDefaultDestinationPath();
        }

        public string FolderName
        {
            get => _folderName;
            set
            {
                _folderName = value;
                OnPropertyChanged(nameof(FolderName));
            }
        }

        private void SetDefaultFolderName()
        {
            fromDatePicker.SelectedDate = DateTime.Today.AddDays(-1);
            toDatePicker.SelectedDate = DateTime.Today;
            fromDatePicker.SelectedDateChanged += (s, e) => UpdateFolderName();
            toDatePicker.SelectedDateChanged += (s, e) => UpdateFolderName();
            UpdateFolderName();
        }

        private void UpdateFolderName()
        {
            string fromDate = fromDatePicker.SelectedDate?.ToString("yyyyMMdd") ?? "From";
            string toDate = toDatePicker.SelectedDate?.ToString("yyyyMMdd") ?? "To";
            FolderName = $"{fromDate}-{toDate}";
        }

        private void SetDefaultDestinationPath()
        {
            destinationTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Multiselect = false;
            dialog.Title = "コピー先フォルダを選択";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (dialog.ShowDialog() == true)
            {
                destinationTextBox.Text = System.IO.Path.GetDirectoryName(dialog.FolderName);
            }
        }

        private void CopyLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (fromDatePicker.SelectedDate == null || toDatePicker.SelectedDate == null)
            {
                MessageBox.Show("日付を指定してください。");
                return;
            }
            DateTime fromDate = (DateTime)fromDatePicker.SelectedDate;
            DateTime toDate = (DateTime)toDatePicker.SelectedDate;
            string destinationPath = destinationTextBox.Text;
            string folderName = folderNameTextBox.Text;

            if (string.IsNullOrEmpty(destinationPath))
            {
                MessageBox.Show("コピー先を指定してください。");
                return;
            }
            // すでにフォルダが存在する場合は上書きするか選択する
            if (Directory.Exists(Path.Combine(destinationPath, folderName)))
            {
                MessageBoxResult result = MessageBox.Show("フォルダがすでに存在します。上書きしますか？", "確認", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            // Directory.GetDirectoriesでYYYYMMDDのフォルダ名のみを取得する
            string[] allDirectories = Directory.GetDirectories(@"C:\Users\uts19\デスクトップ\App1\Log", "*", SearchOption.AllDirectories);
            Regex dateRegex = new Regex(@"^\d{8}$");
            string[] logDirectories = Array.FindAll(allDirectories, dir => dateRegex.IsMatch(new DirectoryInfo(dir).Name));
            MessageBox.Show(logDirectories[0]);
            // yyyyMMddにパースでき、fromDateとtoDateの間にあるフォルダのみを抽出し、フォルダ名の一覧をstring[]で取得する
            logDirectories = Array.FindAll(logDirectories, dir =>
            {
                string folderName = new DirectoryInfo(dir).Name;
                DateTime folderDate;
                if (DateTime.TryParseExact(folderName, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out folderDate))
                {
                    return folderDate >= fromDate;
                }
                return false;
            });
            MessageBox.Show(logDirectories[0]);
            return;
            if (logDirectories.Length == 0)
            {
                MessageBox.Show("ログフォルダが見つかりません。");
                return;
            }

            string targetPath = Path.Combine(destinationPath, folderName);
            Directory.CreateDirectory(targetPath);


            foreach (string logDir in logDirectories)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(logDir);
                if (dirInfo.CreationTime >= fromDate && dirInfo.CreationTime <= toDate)
                {
                    string destDir = Path.Combine(targetPath, dirInfo.Name);
                    CopyDirectory(logDir, destDir);
                }
            }
            MessageBox.Show("ログのコピーが完了しました。");
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                // フォルダ名がYYYYMMDDにパースできるか判定する
                if  (!DateTime.TryParse(Path.GetDirectoryName(directory), out DateTime _))
                {
                    continue;
                }
                string folderName = DateTime.Parse(Path.GetFileName(directory)).ToString("yyyyMMdd");
                // folderNameがfromDateとtoDateの間にあるか判定する
                if (DateTime.Parse(folderName) <= fromDatePicker.SelectedDate || DateTime.Parse(folderName) >= toDatePicker.SelectedDate)
                {
                    continue;
                }
                string destPath = Path.Combine(destDir, Path.GetFileName(directory));
                Directory.CreateDirectory(destPath);
                CopyDirectory(directory, destPath);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                MessageBox.Show($"Button {clickedButton} was clicked.");
                MessageBox.Show($"Event {e}.");
            }
        }
    }
}
