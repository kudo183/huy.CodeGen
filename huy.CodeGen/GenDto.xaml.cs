using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

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

            vm.Result = CodeGenerator.GenDtoClass(vm.Namespace, vm.InterfaceName, vm.ClassName, propertiesList);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var table in DatabaseUtils.ListTables(vm.DatabaseName))
            {
                var properties = DatabaseUtils.ListColumnsOfTable(vm.DatabaseName, table);
                var dtoClass = CodeGenerator.GenDtoClass(vm.Namespace, vm.InterfaceName, table + "Dto", properties);
                var path = vm.OutputPath + "\\" + table + "Dto.cs";
                System.IO.File.WriteAllText(path, dtoClass, Encoding.UTF8);
            }
        }
    }
}
