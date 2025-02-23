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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AmpPromatic.CNCRefactor.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for InsertionsControl.xaml
    /// </summary>
    public partial class InsertionsControl : UserControl
    {
        private readonly DatabaseContext _context = new();
        public MachineVM? Machine { get; set; }
        public ObservableCollection<InsertionVM> Insertions { get; set; } = [];
        public InsertionVM? Insertion { get; set; }
        private List<InsertionType> InsertionTypes { get; set; } = [];
        public InsertionsControl()
        {
            InitializeComponent();
            InsertionTypes = Enum.GetValues(typeof(InsertionType)).Cast<InsertionType>().ToList();
            DataContext = this;
        }

        public void SetMachine(MachineVM? machine)
        {
            Machine = machine;
            SetInsertionsLabel();
            LoadInsertions();
            DataContext = this;
        }

        private void SetInsertionsLabel()
        {
            if (Machine != null)
            {
                InsertionsLabel.Content = $"Insertions for {Machine.Name}";
            }
        }

        private void LoadInsertions()
        {
            if (Machine != null)
            {
                Insertions = new(_context.Insertions.Where(i => i.MachineId == Machine.MachineId).Select(i => new InsertionVM(i, InsertionTypes)).ToList());
                InsertionsListView.ItemsSource = Insertions;
            }
        }

        private void SaveInsertionButton_Click(object sender, RoutedEventArgs e)
        {
            if (Insertion != null)
            {
                var editedInsertion = _context.Insertions.First(m => m.InsertionId == Insertion.InsertionId);
                editedInsertion.Text = Insertion.Text;
                editedInsertion.Qualifier = Insertion.Qualifier;
                editedInsertion.InsertionType = Insertion.InsertionType;
                _context.SaveChanges();
                LoadInsertions();
            }
        }

        private void CancelInsertionButton_Click(object sender, RoutedEventArgs e)
        {
            if (Insertion != null)
            {
                Insertion.IsEditing = false;
            }
        }

        private void CreateNewInsertionButton_Click(object sender, RoutedEventArgs e)
        {
            if (Machine != null)
            {
                var newInsertion = new Insertion { Text = "Insertion Text", Qualifier = "Insert after text", MachineId = Machine.MachineId };
                newInsertion = _context.Insertions.Add(newInsertion).Entity;
                _context.SaveChanges();
                Insertions.Add(new InsertionVM(newInsertion, InsertionTypes));
            }
        }
        private void EditInsertionButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListViewItem)InsertionsListView.ContainerFromElement((Button)sender))?.Content;
            if (curItem is InsertionVM insertion)
            {
                if (Insertion != null)
                {
                    Insertion.IsEditing = false;
                }
                Insertion = insertion;
                Insertion.IsEditing = true;
            }
        }

        private void DeleteInsertionButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListViewItem)InsertionsListView.ContainerFromElement((Button)sender))?.Content;
            if (curItem is InsertionVM insertion)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete '{insertion.Text}'?", "Are you sure?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    _context.Insertions.Where(i => i.InsertionId == insertion.InsertionId).ExecuteDelete();
                    LoadInsertions();
                }
            }
        }

    }
}
