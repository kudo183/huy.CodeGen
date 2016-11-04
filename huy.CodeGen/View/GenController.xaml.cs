using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace huy.CodeGen
{
    /// <summary>
    /// Interaction logic for GenController.xaml
    /// </summary>
    public partial class GenController : UserControl
    {
        GenControllerViewModel vm;
        public GenController()
        {
            InitializeComponent();


            vm = new GenControllerViewModel();
            vm.Namespace = "Server.Controllers";
            vm.EnityClassName = "RBaiXe";
            vm.ContextName = "PhuDinhServerContext";
            vm.PropertyList = "int Ma\r\nstring DiaDiemBaiXe";
            vm.DatabaseName = "PhuDinhClientServer";
            vm.SkippedTable = "__EFMigrationsHistory;User";
            vm.OutputPath = @"C:\codegen\Controller";
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
                propertiesList.Add(new EntityProperty() { PropertyType = p[0], PropertyName = p[1] });
            }

            vm.Result = CodeGenerator.GenControllerClass(vm.Namespace, vm.EnityClassName, vm.ContextName, propertiesList);
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
                var dtoClass = CodeGenerator.GenControllerClass(vm.Namespace, table, vm.ContextName, properties);
                var path = vm.OutputPath + "\\" + table + "Controller.cs";
                System.IO.File.WriteAllText(path, dtoClass, Encoding.UTF8);
            }
        }
    }
}
