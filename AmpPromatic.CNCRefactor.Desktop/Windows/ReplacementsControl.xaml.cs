using AmpPromatic.CNCRefactor.Desktop.Data;
using AmpPromatic.CNCRefactor.Desktop.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AmpPromatic.CNCRefactor.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for ReplacementsControl.xaml
    /// </summary>
    public partial class ReplacementsControl : UserControl, INotifyPropertyChanged
    {
        private readonly DatabaseContext _context = new DatabaseContext();
        public ObservableCollection<ReplacementVM> Replacements { get; set; } = [];
        public ReplacementVM? Replacement { get; set; }

        private MachineVM? machine;
        public MachineVM? Machine { get => machine; set { machine = value; OnPropertyChanged(); } }
        public ReplacementsControl()
        {
            InitializeComponent();
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public void SetMachine(MachineVM? machine)
        {
            Machine = machine;
            LoadReplacements();
            DataContext = this;
        }

        private void LoadReplacements()
        {
            if (Machine != null)
            {
                Replacements = [.. _context.Replacements.Include(r => r.Machine)
                    .Where(r => r.MachineId == Machine.MachineId).Select(m => new ReplacementVM(m)).ToList()];
                ReplacementsListView.ItemsSource = Replacements;
                ReplacementsLabel.Content = $"Replacements for: {Machine.Name}";
            }
        }

        private void CreateNewReplacementButton_Click(object sender, RoutedEventArgs e)
        {
            if (Machine != null)
            {
                var newReplacement = new Replacement { Text = "Text", TextToReplace = "Text to replace", MachineId = Machine.MachineId };
                newReplacement = _context.Replacements.Add(newReplacement).Entity;
                _context.SaveChanges();
                Replacements.Add(new ReplacementVM(newReplacement));
            }
        }

        private void SaveReplacementButton_Click(object sender, RoutedEventArgs e)
        {
            if (Replacement != null)
            {
                _context.Replacements.Where(r => r.ReplacementId == Replacement.ReplacementId)
                    .ExecuteUpdate(r => r.SetProperty(p => p.Text, Replacement.Text).SetProperty(p => p.TextToReplace, Replacement.TextToReplace));
                _context.ChangeTracker.Clear();
                LoadReplacements();
            }
        }

        private void CancelReplacementButton_Click(object sender, RoutedEventArgs e)
        {
            if (Replacement != null)
            {
                Replacement.IsEditing = false;
            }
        }

        private void EditReplacementButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListViewItem)ReplacementsListView.ContainerFromElement((Button)sender))?.Content;
            if (curItem != null && curItem is ReplacementVM replacement)
            {
                if (Replacement != null)
                {
                    Replacement.IsEditing = false;
                }
                Replacement = replacement;
                Replacement.IsEditing = true;
            }
        }

        private void DeleteReplacementButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListViewItem)ReplacementsListView.ContainerFromElement((Button)sender))?.Content;
            if (curItem != null && curItem is ReplacementVM replacement)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete '{replacement.Text}'?", "Are you sure?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    _context.Replacements.Where(m => m.ReplacementId == replacement.ReplacementId).ExecuteDelete();
                    LoadReplacements();
                }
            }
        }
    }
}

