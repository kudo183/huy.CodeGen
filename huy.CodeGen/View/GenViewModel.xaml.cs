using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace huy.CodeGen
{
    /// <summary>
    /// Interaction logic for GenViewModel.xaml
    /// </summary>
    public partial class GenViewModel : UserControl
    {
        GenViewModelViewModel vm;
        public GenViewModel()
        {
            InitializeComponent();

            vm = new GenViewModelViewModel();
            vm.Namespace = "Client.ViewModel";
            vm.EnityClassName = "TDonHang";
            var sb = new StringBuilder();
            sb.AppendLine("int Ma");
            sb.AppendLine("int? MaChanh RChanh");
            sb.AppendLine("int MaKhachHang RKhachHang");
            sb.AppendLine("int MaKhoHang RKhoHang");
            sb.AppendLine("System.DateTime Ngay");
            sb.AppendLine("int TongSoLuong");
            sb.Append("bool Xong");
            vm.PropertyList = sb.ToString();
            vm.DatabaseName = "PhuDinhClientServer";
            vm.SkippedTable = "__EFMigrationsHistory";
            vm.OutputPath = @"C:\codegen\ViewModel";
            DataContext = vm;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tr = new System.IO.StringReader(vm.PropertyList);
            var line = "";
            var propertiesList = new List<EntityProperty>();
            while (true)
            {
                line = tr.ReadLine();
                if (string.IsNullOrEmpty(line) == true)
                    break;

                var p = line.Split(' ');
                var property = new EntityProperty() { PropertyType = p[0], PropertyName = p[1] };
                if (p.Length == 3)
                {
                    property.IsForeignKey = true;
                    property.ForeignKeyTableName = p[2];
                }
                propertiesList.Add(property);
            }

            vm.Result = CodeGenerator.GenViewModelClass(vm.Namespace, vm.EnityClassName, propertiesList);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var skippedTable = new List<string>(
                vm.SkippedTable.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            foreach (var table in DatabaseUtils.ListTables(vm.DatabaseName))
            {
                if (skippedTable.Count > 0 && skippedTable.Contains(table))
                    continue;

                var properties = DatabaseUtils.ListColumnsOfTable(vm.DatabaseName, table);
                var dtoClass = CodeGenerator.GenViewModelClass(vm.Namespace, table, properties);
                var path = vm.OutputPath + "\\" + table + "ViewModel.cs";
                FileUtils.WriteAllTextInUTF8(path, dtoClass);
            }
        }
    }
}
