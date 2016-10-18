using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    /// Interaction logic for GenDto.xaml
    /// </summary>
    public partial class GenDto : UserControl
    {
        GenDtoViewModel vm;
        public GenDto()
        {
            InitializeComponent();

            vm = new GenDtoViewModel();
            vm.Namespace = "DTO";
            vm.InterfaceName = "IDto";
            vm.ClassName = "RBaiXeDto";
            vm.PropertyList = "int Ma\r\nstring DiaDiemBaiXe";
            vm.DatabaseName = "PhuDinhClientServer";
            vm.OutputPath = @"C:\codegen\DTO";
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

            vm.Result = GenDtoClass(vm.Namespace, vm.InterfaceName, vm.ClassName, propertiesList);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var table in DatabaseUtils.ListTables(vm.DatabaseName))
            {
                var properties = DatabaseUtils.ListColumnsOfTable(vm.DatabaseName, table);
                var dtoClass = GenDtoClass(vm.Namespace, vm.InterfaceName, table + "Dto", properties);
                var path = vm.OutputPath + "\\" + table + "Dto.cs";
                System.IO.File.WriteAllText(path, dtoClass, Encoding.UTF8);
            }
        }

        public string GenDtoClass(string nameSpace, string interfaceName, string className, List<EntityProperty> properties)
        {
            var sb = new StringBuilder();
            var tab = "    ";
            var tab2 = tab + tab;
            var tab3 = tab2 + tab;
            sb.AppendLine("namespace " + nameSpace);
            sb.AppendLine("{");
            sb.AppendLine(tab + "[ProtoBuf.ProtoContract]");
            sb.AppendLine(tab + "public class " + className + " : " + interfaceName);
            sb.AppendLine(tab + "{");
            foreach (var item in properties)
            {
                sb.AppendLine(string.Format("{0}{1} _{2};", tab2, item.PropertyType, item.PropertyName));
            }
            sb.AppendLine();
            for (var i = 0; i < properties.Count; i++)
            {
                var item = properties[i];
                sb.AppendLine(string.Format("{0}[ProtoBuf.ProtoMember({1})]", tab2, i + 1));
                sb.AppendLine(string.Format("{0}public {1} {2} {{ get; set; }}", tab2, item.PropertyType, item.PropertyName));
            }
            sb.AppendLine();
            sb.AppendLine(tab2 + "public void SetCurrentValueAsOriginalValue()");
            sb.AppendLine(tab2 + "{");
            foreach (var item in properties)
            {
                sb.AppendLine(string.Format("{0}_{1} = {2};", tab3, item.PropertyName, item.PropertyName));
            }
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            sb.AppendLine(tab2 + "public bool HasChange()");
            sb.AppendLine(tab2 + "{");
            sb.Append(string.Format("{0}return (_{1} != {2})", tab3, properties[0].PropertyName, properties[0].PropertyName));
            for (var i = 1; i < properties.Count; i++)
            {
                var item = properties[i];
                sb.AppendLine();
                sb.Append(string.Format("{0}|| (_{1} != {2})", tab3, item.PropertyName, item.PropertyName));
            }
            sb.AppendLine(";");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            foreach (var item in properties.Where(p => p.IsForeignKey))
            {
                sb.AppendLine(string.Format("{0}[Newtonsoft.Json.JsonIgnore]", tab2));
                sb.AppendLine(string.Format("{0}public object {1}Sources {{ get; set; }}", tab2, item.PropertyName));
            }
            sb.AppendLine(tab + "}");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
