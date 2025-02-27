using AmpPromatic.CNCRefactor.Common;
using AmpPromatic.CNCRefactor.Desktop.Data;
using AmpPromatic.CNCRefactor.Desktop.Utilities;
using AmpPromatic.CNCRefactor.Desktop.ViewModels;
using AmpPromatic.CNCRefactor.Desktop.Windows;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Formats.Tar;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace AmpPromatic.CNCRefactor.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly DatabaseContext _context = new();
        public Transition DefaultTransition { get; set; }
        private Machine? toMachine;
        private Transition? transition;
        private Transition? nonTransition;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public ObservableCollection<Machine> Machines { get; set; } = [];
        public ObservableCollection<Transition> Transitions { get; set; } = [];
        public ObservableCollection<FileVM> SelectedFiles { get; set; } = [];
        public ObservableCollection<FileVM> SelectedTransitionFiles { get; set; } = [];

        public Machine? ToMachine { get => toMachine; set { toMachine = value; OnPropertyChanged(); } }
        public Transition? Transition
        {
            get => transition; set
            {
                transition = value;
                if (value is not null)
                    foreach (var transitionFile in SelectedTransitionFiles)
                    {
                        transitionFile.TransitionId = value.TransitionId;
                    }
                OnPropertyChanged();
            }
        }
        public Transition? NonTransition
        {
            get => nonTransition; set
            {
                nonTransition = value;
                if (value is not null)
                    foreach (var transitionFile in SelectedFiles)
                    {
                        transitionFile.TransitionId = value.TransitionId;
                    }
                OnPropertyChanged();
            }
        }
        public string TransitionFilesString => string.Join(", ", TransitionFiles);
        private string[] TransitionFiles
        {
            get
            {
                var val = SettingsHelper.GetSetting(nameof(TransitionFiles));
                return JsonSerializer.Deserialize<string[]>(val ?? "[]")!;
            }
            set
            {
                SettingsHelper.SetSetting(nameof(TransitionFiles), JsonSerializer.Serialize(value));
                OnPropertyChanged();
                OnPropertyChanged(nameof(TransitionFilesString));
            }
        }

        private string DefaultDirectory
        {
            get
            {
                return SettingsHelper.GetSetting(nameof(DefaultDirectory));
            }
            set
            {
                SettingsHelper.SetSetting(nameof(DefaultDirectory), value);
            }
        }
        private string DefaultFromMachine
        {
            get
            {
                return SettingsHelper.GetSetting(nameof(DefaultFromMachine));
            }
            set
            {
                SettingsHelper.SetSetting(nameof(DefaultFromMachine), value);
            }
        }
        private string DefaultToMachine
        {
            get
            {
                return SettingsHelper.GetSetting(nameof(DefaultToMachine));
            }
            set
            {
                SettingsHelper.SetSetting(nameof(DefaultToMachine), value);
            }
        }
        private bool isEditing;

        public bool IsEditing
        {
            get { return isEditing; }
            set
            {
                isEditing = value;
                OnPropertyChanged();
                IsEditingVis = IsEditing ? Visibility.Visible : Visibility.Collapsed;
                IsViewOnlyVis = !IsEditing ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private Visibility isEditingVis = Visibility.Collapsed;
        public Visibility IsEditingVis
        {
            get => isEditingVis;
            private set
            {
                isEditingVis = value;
                OnPropertyChanged();
            }
        }

        private Visibility isViewOnlyVis = Visibility.Visible;
        public Visibility IsViewOnlyVis
        {
            get => isViewOnlyVis;
            private set
            {
                isViewOnlyVis = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {

            _context.Initialize();
            IsEditing = false;
            LoadData();
            DefaultTransition = Transitions.First(t => t.NewText == "227");
            Transition = DefaultTransition;
            NonTransition = DefaultTransition;
            InitializeComponent();
            DataContext = this;
            FilesListView.DataContext = this;
            FilesListView.ItemsSource = SelectedFiles;
        }

        private void LoadData()
        {
            if (string.IsNullOrWhiteSpace(DefaultFromMachine))
            {
                DefaultFromMachine = "PEGA 345";
            }
            if (string.IsNullOrWhiteSpace(DefaultToMachine))
            {
                DefaultToMachine = "VIPROS 255";
            }
            if (TransitionFiles == null || TransitionFiles.Length == 0)
            {
                TransitionFiles = ["BCT", "TCT", "LTT", "RTT"];
            }
            Machines = [.. _context.Machines];
            ToMachine = Machines.First(m => m.Name == DefaultToMachine);

            Transitions = [.. _context.Transitions];
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(DefaultDirectory))
            {
                fileDialog.DefaultDirectory = DefaultDirectory;
            }
            else
            {
                DefaultDirectory = fileDialog.DefaultDirectory;
            }
            fileDialog.Multiselect = true;
            fileDialog.Filter = BuildFilterString();
            var browseDialogResult = fileDialog.ShowDialog();
            if (browseDialogResult != null && browseDialogResult == true)
            {
                SelectedFiles = [.. fileDialog.FileNames
                    .Where(f => !TransitionFiles.Any(tf => Path.GetFileName(f).Contains(tf)))
                    .Select(f => new FileVM() { FilePath = f, Transitions = Transitions, TransitionId = DefaultTransition.TransitionId })];
                SelectedTransitionFiles = [.. fileDialog.FileNames
                    .Where(f => TransitionFiles.Any(tf => Path.GetFileName(f).Contains(tf)))
                    .Select(f => new FileVM() { FilePath = f, Transitions = Transitions, TransitionId = DefaultTransition.TransitionId })];
                FilesListView.ItemsSource = SelectedFiles;
                TransitionFilesListView.ItemsSource = SelectedTransitionFiles;
                DefaultDirectory = Path.GetDirectoryName(fileDialog.FileNames.FirstOrDefault()) ?? string.Empty;
            }
        }

        /// <summary>
        /// Builds the filter string for the OpenFileDialog that includes all extensions of the machines in the database
        /// </summary>
        /// <returns>Returns the filter string</returns>
        private string BuildFilterString()
        {
            var filterString = "Machine Files (";
            foreach (var machine in Machines)
            {
                filterString += $"*{machine.Extension};";
            }
            filterString = filterString.TrimEnd(';');
            filterString += ")|";
            foreach (var machine in Machines)
            {
                filterString += $"*{machine.Extension};";
            }
            filterString = filterString.TrimEnd(';');
            return filterString;
        }

        #region Refactor

        private void RefactorButtonClick(object sender, RoutedEventArgs e)
        {
            if (OvewriteFile.IsChecked == true)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to overwrite the old file(s)? This action cannot be undone", "Overwrite Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    Refactor(SelectedFiles.Concat(SelectedTransitionFiles));
                }
            }
            else
            {
                Refactor(SelectedFiles.Concat(SelectedTransitionFiles));
            }
        }

        private void Refactor(IEnumerable<FileVM> files)
        {
            var results = ReadAndRefactorFiles(files);
            if (results.All(r => r.ResultType == ResultType.Success))
            {
                MessageBox.Show("Finished!", "Success", MessageBoxButton.OK, MessageBoxImage.None);
            }
            else
            {
                string message = "Finished with errors: \n";
                foreach (var errorResult in results.Where(r => r.ResultType != ResultType.Success))
                {
                    message += errorResult.ErrorMessage + " - " + errorResult.FileName + "\n";
                }
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<RefactorResult> ReadAndRefactorFiles(IEnumerable<FileVM> files)
        {
            var refactors = new Dictionary<FileInfo, string[]>();
            var results = new List<RefactorResult>();
            foreach (var file in files)
            {
                try
                {
                    var allowedExtensions = Machines.Select(m => m.Extension).ToArray();
                    var fileInfo = new FileInfo(file.FilePath);

                    if (!allowedExtensions.Contains(fileInfo.Extension.ToLowerInvariant()))
                    {
                        results.Add(RefactorResult.Failure(ResultType.FileNotSupported, "File is not supported", file.FilePath));
                    }

                    var lines = TryReadFile(file.FilePath);
                    if (lines.Count == 0)
                    {
                        results.Add(RefactorResult.Failure(ResultType.FileEmpty, "File is empty", file.FilePath));
                    }
                    ReplaceValues(lines);
                    TransitionValues(lines, file.Transition);
                    InsertLines(lines);
                    refactors.Add(fileInfo, [.. lines]);
                }
                catch (FileNotFoundException ex)
                {
                    Debug.WriteLine(ex.Message);
                    results.Add(RefactorResult.Failure(ResultType.FileNotFound, ex.Message, file.FilePath));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    results.Add(RefactorResult.Failure(ResultType.UnknownError, ex.Message, file.FilePath));
                }
            }

            if (OvewriteFile.IsChecked == true)
            {
                foreach (var result in refactors)
                {
                    var fileInfo = result.Key;
                    var newLines = result.Value;
                    var newPath = Path.Combine(fileInfo.DirectoryName!, $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}{ToMachine!.Extension}");
                    File.Delete(fileInfo.FullName);
                    WriteFile(newPath, newLines);
                }
            }
            else
            {
                var saveFolderPath = OpenSaveFileDialog(refactors.First().Key);

                if (!string.IsNullOrEmpty(saveFolderPath))
                {
                    foreach (var result in refactors)
                    {
                        var fileInfo = result.Key;
                        var newLines = result.Value;
                        WriteFile(Path.Combine(saveFolderPath, Path.GetFileNameWithoutExtension(fileInfo.FullName) + ToMachine!.Extension), newLines);
                        results.Add(RefactorResult.Success(null));
                    }
                }
                else
                {
                    return [RefactorResult.Failure(ResultType.OperationCancelled, "Operation cancelled", default)];
                }
            }
            return results;
        }

        private void ReplaceValues(List<string> lines)
        {
            var replacements = _context.Replacements.Where(r => r.MachineId == ToMachine!.MachineId).ToList();
            foreach (var replacement in replacements)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    if (line.Contains(replacement.TextToReplace))
                    {
                        lines[i] = line
                            .Replace(replacement.TextToReplace, replacement.Text);
                    }
                }
            }
        }

        private void TransitionValues(List<string> lines, Transition transition)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.Contains(transition.OldText))
                {
                    lines[i] = line
                        .Replace(transition.OldText, transition.NewText);
                }
            }

        }

        private void InsertLines(List<string> newLines)
        {
            var insertions = _context.Insertions.Where(i => i.MachineId == ToMachine!.MachineId).ToList();
            foreach (var insertion in insertions)
            {
                for (int i = 0; i < newLines.Count; i++)
                {
                    var line = newLines[i];
                    bool hasAnotherLine = i + 1 < newLines.Count;
                    if (line.Contains(insertion.Qualifier))
                    {
                        // check if the line already exists to not insert duplicate
                        if (hasAnotherLine && newLines[i + 1] != insertion.Text)
                        {
                            newLines.Insert(i + 1, insertion.Text);
                        }
                    }
                }
            }
        }

        private static List<string> TryReadFile(string filePath)
        {
            List<string> lines = [];
            using (StreamReader reader = new(filePath))
            {
                string? currentLine;
                while ((currentLine = reader.ReadLine()) != null)
                {
                    lines.Add(currentLine);
                }
            }
            return lines;
        }

        private static void WriteFile(string filePath, string[] lines)
        {
            using StreamWriter writer = new(filePath);
            foreach (var line in lines)
            {
                writer.WriteLine(line);
            }
        }

        #endregion

        private static string OpenSaveFileDialog(FileInfo fileInfo)
        {
            var dg = new OpenFolderDialog
            {
                DefaultDirectory = fileInfo.DirectoryName
            };
            Nullable<bool> result = dg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                return dg.FolderName;
            }

            return string.Empty;
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MachineSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var machineSettingsMenu = new MachineSettings
            {
                Owner = this
            };
            machineSettingsMenu.ShowDialog();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow()
            {
                Owner = this
            };
            about.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IsEditing = !IsEditing;

            if (IsEditing)
            {
                TransitionFilesTextBox.Text = string.Join(", ", TransitionFiles);
            }
            else
            {
                TransitionFiles = TransitionFilesTextBox.Text.Replace(" ", "").Split(',');
            }
        }

        private void TransitionFilesHeaderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.SelectedItem is Transition transition)
                {

                }
            }
        }

        private void FilesHeaderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.SelectedItem is Transition transition)
                {

                }
            }
        }
    }
}