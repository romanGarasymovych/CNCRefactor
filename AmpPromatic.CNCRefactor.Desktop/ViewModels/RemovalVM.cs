using AmpPromatic.CNCRefactor.Desktop.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AmpPromatic.CNCRefactor.Desktop.ViewModels
{
    public class RemovalVM: Removal, INotifyPropertyChanged
    {
        public RemovalVM(Removal removal)
        {
            RemovalId = removal.RemovalId;
            MachineId = removal.MachineId;
            Machine = removal.Machine;
            Text = removal.Text;
        }

        public RemovalVM()
        {
        }

        public Removal ToReplacement()
        {
            return new Removal
            {
                RemovalId = RemovalId,
                MachineId = MachineId,
                Text = Text,
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
