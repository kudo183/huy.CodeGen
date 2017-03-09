using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace huy.CodeGen
{
    /// <summary>
    /// Interaction logic for GenView.xaml
    /// </summary>
    public partial class GenView : UserControl
    {
        GenViewViewModel vm;
        public GenView()
        {
            InitializeComponent();

            vm = new GenViewViewModel();
            vm.Namespace = "Client.View";
            vm.EnityClassName = "TDonHang";
            var sb = new StringBuilder();
            sb.AppendLine("int Ma i");
            sb.AppendLine("int? MaChanh RChanh");
            sb.AppendLine("int MaKhachHang RKhachHang");
            sb.AppendLine("int MaKhoHang RKhoHang");
            sb.AppendLine("System.DateTime Ngay");
            sb.AppendLine("int TongSoLuong");
            sb.Append("bool Xong");
            vm.PropertyList = sb.ToString();
            vm.DatabaseName = "PhuDinhClientServer";
            vm.SkippedTable = "__EFMigrationsHistory";
            vm.OutputPath = @"C:\codegen\View";
            DataContext = vm;

            Loaded += GenView_Loaded;
        }

        private void GenView_Loaded(object sender, RoutedEventArgs e)
        {
            Button_Click(null, null);
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
                    if (p[2] == "i")
                    {
                        property.IsIdentity = true;
                    }
                    else
                    {
                        property.IsForeignKey = true;
                        property.ForeignKeyTableName = p[2];
                    }
                }
                propertiesList.Add(property);
            }

            vm.Result = CodeGenerator.GenViewXamlClass(vm.Namespace, vm.EnityClassName, propertiesList);
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
                var path = vm.OutputPath + "\\" + table + "View.xaml";

                var xamlClass = CodeGenerator.GenViewXamlClass(vm.Namespace, table, properties);
                FileUtils.WriteAllTextInUTF8(path, xamlClass);

                var codeClass = CodeGenerator.GenViewCodeClass(vm.Namespace, table);
                FileUtils.WriteAllTextInUTF8(path + ".cs", codeClass);
            }
        }
    }
}
