using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace huy.CodeGen.ViewModel
{
    public class DbTable : INotifyPropertyChanged
    {
        public string TableName { get; set; }
        public ObservableCollection<DbTableColumn> Columns { get; set; }

        private bool isSelected = true;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        private bool isExpanded;

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value; OnPropertyChanged();
            }
        }

        public DbTable()
        {
            Columns = new ObservableCollection<DbTableColumn>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DbTableColumn
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsIdentity { get; set; }
        public string ForeignKeyTableName { get; set; }

        public EntityProperty ToEntityProperty()
        {
            return new EntityProperty()
            {
                PropertyType = DataType,
                PropertyName = ColumnName,
                IsIdentity = IsIdentity,
                IsForeignKey = IsForeignKey,
                ForeignKeyTableName = DatabaseUtils.UpperFirstLetter(ForeignKeyTableName)
            };
        }
    }

    public class DatabaseTreeVM : INotifyPropertyChanged
    {
        public string DBName { get; set; }

        private ObservableCollection<DbTable> dbTables;

        public ObservableCollection<DbTable> DbTables
        {
            get { return dbTables; }
            set
            {
                dbTables = value;
                OnPropertyChanged();
            }
        }

        public DatabaseTreeVM()
        {
            DbTables = new ObservableCollection<DbTable>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
