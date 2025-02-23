using AmpPromatic.CNCRefactor.Desktop.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace AmpPromatic.CNCRefactor.Desktop.ViewModels
{
    public record class InsertionTypeVM(string Name, InsertionType Type)
    {
        public static string CreateName(InsertionType type)
        {
            if (string.IsNullOrWhiteSpace(type.ToString()))
                return string.Empty;

            var stringBuilder = new StringBuilder();
            foreach (char c in type.ToString())
            {
                if (char.IsUpper(c) && stringBuilder.Length > 0)
                {
                    stringBuilder.Append(' ');
                }
                stringBuilder.Append(c);
            }
            return stringBuilder.ToString();
        }
    }
    public class InsertionVM : Insertion, INotifyPropertyChanged
    {
        public List<InsertionTypeVM> InsertionTypes { get; set; } = new();
        public InsertionVM(Insertion insertion, List<InsertionType> insertionTypes)
        {
            InsertionId = insertion.InsertionId;
            MachineId = insertion.MachineId;
            Machine = insertion.Machine;
            Qualifier = insertion.Qualifier;
            Text = insertion.Text;
            InsertionType = insertion.InsertionType;
            InsertionTypes = insertionTypes.Select(i => new InsertionTypeVM(InsertionTypeVM.CreateName(i), i)).ToList();
        }
        public InsertionVM()
        {
        }
        public Insertion ToInsertion()
        {
            return new Insertion
            {
                InsertionId = InsertionId,
                MachineId = MachineId,
                Qualifier = Qualifier,
                InsertionType = InsertionType,
                Text = Text
            };
        }
        public event PropertyChangedEventHandler? PropertyChanged;
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
