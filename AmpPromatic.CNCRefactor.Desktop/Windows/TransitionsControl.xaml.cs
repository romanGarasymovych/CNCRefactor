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
    /// Interaction logic for TransitionsControl.xaml
    /// </summary>
    public partial class TransitionsControl : UserControl, INotifyPropertyChanged
    {
        private readonly DatabaseContext _context = new DatabaseContext();
        public ObservableCollection<TransitionVM> Transitions { get; set; } = [];
        public TransitionVM? Transition { get; set; }

        private MachineVM? machine;
        public MachineVM? Machine { get => machine; set { machine = value; OnPropertyChanged(); } }
        public TransitionsControl()
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
            LoadTransitions();
            DataContext = this;
        }

        private void LoadTransitions()
        {
            if (Machine != null)
            {
                Transitions = [.. _context.Transitions.Include(r => r.Machine)
                    .Where(r => r.MachineId == Machine.MachineId).Select(m => new TransitionVM(m)).ToList()];
                TransitionsListView.ItemsSource = Transitions;
                TransitionsLabel.Content = $"Transitions for: {Machine.Name}";
            }
        }

        private void CreateNewTransitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (Machine != null)
            {
                var newTransition = new Transition { OldText = "Text", NewText = "Text to replace", MachineId = Machine.MachineId };
                newTransition = _context.Transitions.Add(newTransition).Entity;
                _context.SaveChanges();
                Transitions.Add(new TransitionVM(newTransition));
            }
        }

        private void SaveTransitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (Transition != null)
            {
                _context.Transitions.Where(r => r.TransitionId == Transition.TransitionId)
                    .ExecuteUpdate(r => r.SetProperty(p => p.OldText, Transition.OldText).SetProperty(p => p.NewText, Transition.NewText));
                _context.ChangeTracker.Clear();
                LoadTransitions();
            }
        }

        private void CancelTransitionButton_Click(object sender, RoutedEventArgs e)
        {
            if (Transition != null)
            {
                Transition.IsEditing = false;
            }
        }

        private void EditTransitionButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListViewItem)TransitionsListView.ContainerFromElement((Button)sender))?.Content;
            if (curItem != null && curItem is TransitionVM replacement)
            {
                if (Transition != null)
                {
                    Transition.IsEditing = false;
                }
                Transition = replacement;
                Transition.IsEditing = true;
            }
        }

        private void DeleteTransitionButton_Click(object sender, RoutedEventArgs e)
        {
            var curItem = ((ListViewItem)TransitionsListView.ContainerFromElement((Button)sender))?.Content;
            if (curItem != null && curItem is TransitionVM replacement)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to delete '{replacement.OldText}'?", "Are you sure?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    _context.Transitions.Where(m => m.TransitionId == replacement.TransitionId).ExecuteDelete();
                    LoadTransitions();
                }
            }
        }
    }
}

