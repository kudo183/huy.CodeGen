using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            vm.Result = GenDtoClass(vm.Namespace, vm.EnityClassName, vm.ContextName, propertiesList);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var table in DatabaseUtils.ListTables(vm.DatabaseName))
            {
                var properties = DatabaseUtils.ListColumnsOfTable(vm.DatabaseName, table);
                var dtoClass = GenDtoClass(vm.Namespace, table, vm.ContextName, properties);
                var path = vm.OutputPath + "\\" + table + "Controller.cs";
                System.IO.File.WriteAllText(path, dtoClass, Encoding.UTF8);
            }
        }

        public string GenDtoClass(string nameSpace, string enityClassName, string contextName, List<EntityProperty> properties)
        {
            var sb = new StringBuilder();
            var tab = "    ";
            var tab2 = tab + tab;
            var tab3 = tab2 + tab;
            sb.AppendLine("using huypq.SwaMiddleware;");
            sb.AppendLine("using Server.Entities;");
            sb.AppendLine("using DTO;");
            sb.AppendLine();
            sb.AppendLine("namespace " + nameSpace);
            sb.AppendLine("{");
            sb.AppendLine(string.Format("{0}public class {1}Controller : SwaEntityBaseController<{2}, {1}, {1}Dto>", tab, enityClassName, contextName));
            sb.AppendLine(tab + "{");
            sb.AppendLine(string.Format("{0}public override {1}Dto ConvertToDto({1} entity)", tab2, enityClassName));
            sb.AppendLine(tab2 + "{");
            sb.AppendLine(string.Format("{0}var dto = new {1}Dto();", tab3, enityClassName));
            foreach (var item in properties)
            {
                sb.AppendLine(string.Format("{0}dto.{1} = entity.{1};", tab3, item.PropertyName));
            }
            sb.AppendLine(string.Format("{0}return dto;", tab3, enityClassName));
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            sb.AppendLine(string.Format("{0}public override {1} ConvertToEntity({1}Dto dto)", tab2, enityClassName));
            sb.AppendLine(tab2 + "{");
            sb.AppendLine(string.Format("{0}var entity = new {1}();", tab3, enityClassName));
            foreach (var item in properties)
            {
                sb.AppendLine(string.Format("{0}entity.{1} = dto.{1};", tab3, item.PropertyName));
            }
            sb.AppendLine(string.Format("{0}return entity;", tab3, enityClassName));
            sb.AppendLine(tab2 + "}");
            sb.AppendLine(tab + "}");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
