﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace huy.CodeGen.ViewModel
{
    public class DbTable : INotifyPropertyChanged
    {
        public string TableName { get; set; }

        public ObservableCollection<DbTableColumn> Columns { get; set; }
        
        public ObservableCollection<ForeignKey> ForeignKeys { get; set; }

        public ObservableCollection<Index> Indexes { get; set; }

        public ObservableCollection<RequiredMaxLength> RequiredMaxLengths { get; set; }

        public ObservableCollection<DefaultValue> DefaultValues { get; set; }

        public ObservableCollection<HasColumnType> HasColumnTypes { get; set; }

        public ObservableCollection<Reference> ReferencesToThisTable { get; set; }

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
            ForeignKeys = new ObservableCollection<ForeignKey>();
            Indexes = new ObservableCollection<Index>();
            RequiredMaxLengths = new ObservableCollection<RequiredMaxLength>();
            DefaultValues = new ObservableCollection<DefaultValue>();
            HasColumnTypes = new ObservableCollection<HasColumnType>();
            ReferencesToThisTable = new ObservableCollection<Reference>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Reference
    {
        public string PropertyName { get; set; }
        public string ReferenceTableName { get; set; }
    }

    public class HasColumnType
    {
        public string PropertyName { get; set; }
        public string TypeName { get; set; }
    }

    public class RequiredMaxLength
    {
        public string PropertyName { get; set; }
        public bool NeedIsRequired { get; set; }
        public int MaxLength { get; set; }
    }

    public class DefaultValue
    {
        public string PropertyName { get; set; }
        public string Value { get; set; }
    }

    public class Index
    {
        public string PropertyName { get; set; }
        public string IX_Name { get; set; }

        /// <summary>
        /// None = 0, DriPrimaryKey = 1, DriUniqueKey = 2
        /// </summary>
        public int IndexType { get; set; }
    }

    public class ForeignKey
    {
        public string PropertyName { get; set; }
        public string FK_Name { get; set; }

        /// <summary>
        /// Restrict = 0, SetNull = 1, Cascade = 2
        /// </summary>
        public int DeleteAction { get; set; }
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
