using AmpPromatic.CNCRefactor.Desktop.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AmpPromatic.CNCRefactor.Desktop.ViewModels
{
    public class TransitionVM : Transition, INotifyPropertyChanged
    {
        public TransitionVM(Transition replacement)
        {
            TransitionId = replacement.TransitionId;
            MachineId = replacement.MachineId;
            Machine = replacement.Machine;
            MachineVM = new MachineVM(replacement.Machine);
            OldText = replacement.OldText;
            NewText = replacement.NewText;
        }

        public TransitionVM()
        {
        }

        public Transition ToTransition()
        {
            return new Transition
            {
                TransitionId = TransitionId,
                MachineId = MachineId,
                OldText = OldText,
                NewText = NewText
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private MachineVM? machineVM;

        public MachineVM? MachineVM
        {
            get { return machineVM; }
            set
            {
                machineVM = value;
                OnPropertyChanged();
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
    }
}
