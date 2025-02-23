using AmpPromatic.CNCRefactor.Desktop.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AmpPromatic.CNCRefactor.Desktop.ViewModels
{
    public class FileVM : INotifyPropertyChanged
    {
        public ObservableCollection<Transition> Transitions { get; set; } = [];

        private int transitionId;
        private Transition transition;
        private string filePath;


        public string FilePath
        {
            get => filePath; set
            {
                filePath = value;
                OnPropertyChanged();
            }
        }

        public int TransitionId
        {
            get => transitionId; set
            {
                transitionId = value;
                Transition = Transitions.First(t => t.TransitionId == transitionId);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Transition));
            }
        }

        public Transition Transition
        {
            get => transition; set
            {
                transition = value;
                OnPropertyChanged();
            }
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public override string ToString()
        {
            return $"{TransitionId} - {FilePath}";
        }
    }
}
