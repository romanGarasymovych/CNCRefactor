using AmpPromatic.CNCRefactor.Desktop.Data;
using AmpPromatic.CNCRefactor.Desktop.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AmpPromatic.CNCRefactor.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for MachineSettings.xaml
    /// </summary>
    public partial class MachineSettings : Window
    {
        private readonly DatabaseContext _context = new();
        public ObservableCollection<MachineVM> Machines { get; set; } = [];
        public MachineVM? Machine { get; set; }
        public ObservableCollection<InsertionVM> Insertions { get; set; } = [];
        public InsertionVM? Insertion { get; set; }
        public MachineSettings()
        {
            LoadMachines();
            InitializeComponent();
            DataContext = this;
        }

        private void LoadMachines()
        {
            Machines = [.. _context.Machines.Select(m => new MachineVM(m))];
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Machine != null)
            {
                var editedMachine = _context.Machines.First(m => m.MachineId == Machine.MachineId);
                editedMachine.Name = Machine.Name;
                editedMachine.Extension = Machine.Extension;
                _context.SaveChanges();
                LoadMachines();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListViewItem)MachinesListView.ContainerFromElement((Button)sender))?.Content;
            if (curItem != null && curItem is Machine machine)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete '{machine.Name}'? This will remove any settings associated with it", "Are you sure?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    _context.Machines.Where(m => m.MachineId == machine.MachineId).ExecuteDelete();
                    LoadMachines();
                }
            }

        }

        private void CreateNewButton_Click(object sender, RoutedEventArgs e)
        {
            var newMachine = new Machine { Name = "New Machine", Extension = ".nc" };
            newMachine = _context.Machines.Add(newMachine).Entity;
            _context.SaveChanges();
            Machines.Add(new MachineVM(newMachine));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListViewItem)MachinesListView.ContainerFromElement((Button)sender))?.Content;
            if (curItem != null && curItem is MachineVM machine)
            {
                if (Machine != null)
                {
                    Machine.IsEditing = false;
                }
                Machine = machine;
                Machine.IsEditing = true;

                InsertionsControl.SetMachine(Machine);
                ReplacementsControl.SetMachine(Machine);
                RemovalsControl.SetMachine(Machine);
                TransitionsControl.SetMachine(Machine);
            }
        }

        

        private void CancelMachineEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (Machine != null)
            {
                Machine.IsEditing = false;
            }
        }

        private void RestoreDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to restore default settings? This will undo any customizations", "Are you sure?", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (Machine != null)
                {
                    Machine.IsEditing = false;
                    Machine = null;
                    InsertionsControl.SetMachine(null);
                    ReplacementsControl.SetMachine(null);
                    RemovalsControl.SetMachine(null);
                    TransitionsControl.SetMachine(null);
                }
                _context.RestoreDefaults();
                LoadMachines();
            }
        }
    }
}
