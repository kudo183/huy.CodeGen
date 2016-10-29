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
            vm.SkippedTable = "__EFMigrationsHistory;User";
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

            vm.Result = CodeGenerator.GenViewClass(vm.Namespace, vm.EnityClassName, propertiesList);
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
                var dtoClass = CodeGenerator.GenViewClass(vm.Namespace, table, properties);
                var path = vm.OutputPath + "\\" + table + "View.xaml";
                System.IO.File.WriteAllText(path, dtoClass, Encoding.UTF8);

                var sb = new StringBuilder();
                sb.AppendLine("using Client.Abstraction;");
                sb.AppendLine("namespace " + vm.Namespace);
                sb.AppendLine("{");
                sb.AppendLine(string.Format("    public partial class {0}View : BaseView<DTO.{0}Dto>", table));
                sb.AppendLine("    {");
                sb.AppendLine(string.Format("        public {0}View() : base()", table));
                sb.AppendLine("        {");
                sb.AppendLine("            InitializeComponent();");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
                sb.AppendLine("}");
                System.IO.File.WriteAllText(path + ".cs", sb.ToString(), Encoding.UTF8);
            }
        }
    }
}
