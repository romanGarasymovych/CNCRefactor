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
    /// Interaction logic for RemovalsControl.xaml
    /// </summary>
    public partial class RemovalsControl : UserControl, INotifyPropertyChanged
    {
        private readonly DatabaseContext _context = new DatabaseContext();
        public ObservableCollection<RemovalVM> Removals { get; set; } = [];
        public RemovalVM? Removal { get; set; }

        private MachineVM? machine;
        public MachineVM? Machine { get => machine; set { machine = value; OnPropertyChanged(); } }
        public RemovalsControl()
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
            LoadRemovals();
            DataContext = this;
        }

        private void LoadRemovals()
        {
            if (Machine != null)
            {
                Removals = [.. _context.Removals.Include(r => r.Machine)
                    .Where(r => r.MachineId == Machine.MachineId).Select(m => new RemovalVM(m)).ToList()];
                RemovalsListView.ItemsSource = Removals;
                RemovalsLabel.Content = $"Removals for: {Machine.Name}";
            }
        }

        private void CreateNewRemovalButton_Click(object sender, RoutedEventArgs e)
        {
            if (Machine != null)
            {
                var newRemoval = new Removal { Text = "Text", MachineId = Machine.MachineId };
                newRemoval = _context.Removals.Add(newRemoval).Entity;
                _context.SaveChanges();
                Removals.Add(new RemovalVM(newRemoval));
            }
        }

        private void SaveRemovalButton_Click(object sender, RoutedEventArgs e)
        {
            if (Removal != null)
            {
                _context.Removals.Where(r => r.RemovalId == Removal.RemovalId)
                    .ExecuteUpdate(r => r.SetProperty(p => p.Text, Removal.Text));
                _context.ChangeTracker.Clear();
                LoadRemovals();
            }
        }

        private void CancelRemovalButton_Click(object sender, RoutedEventArgs e)
        {
            if (Removal != null)
            {
                Removal.IsEditing = false;
            }
        }

        private void EditRemovalButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListViewItem)RemovalsListView.ContainerFromElement((Button)sender))?.Content;
            if (curItem != null && curItem is RemovalVM removal)
            {
                if (Removal != null)
                {
                    Removal.IsEditing = false;
                }
                Removal = removal;
                Removal.IsEditing = true;
            }
        }

        private void DeleteRemovalButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListViewItem)RemovalsListView.ContainerFromElement((Button)sender))?.Content;
            if (curItem != null && curItem is RemovalVM Removal)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete '{Removal.Text}'?", "Are you sure?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    _context.Removals.Where(m => m.RemovalId == Removal.RemovalId).ExecuteDelete();
                    LoadRemovals();
                }
            }
        }
    }
}
