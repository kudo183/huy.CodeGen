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

namespace huy.CodeGen.View
{
    /// <summary>
    /// Interaction logic for GenAll.xaml
    /// </summary>
    public partial class GenAll : UserControl
    {
        ViewModel.GenAllVM vm = ViewModelManager.Instance.GenAllVM;

        public GenAll()
        {
            InitializeComponent();

            DataContext = vm;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
                return;

            switch (btn.Tag.ToString())
            {
                case "View":
                    GenView();
                    break;
                case "ViewModel":
                    GenViewModel();
                    break;
                case "Text":
                    GenText();
                    break;
                case "Controller":
                    GenController();
                    break;
                case "Dto":
                    GenDto();
                    break;
                case "Entity":
                    GenEntity();
                    break;
                case "All":
                    GenAllCode();
                    break;
            }

            vm.Messages.Add(string.Format("{0} | Done.", DateTime.Now));
        }

        private void GenAllCode()
        {
            GenView();
            GenViewModel();
            GenText();
            GenController();
            GenDto();
            GenEntity();
        }
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null)
                return;

            switch (btn.Tag.ToString())
            {
                case "View":
                    OpenPath(vm.ViewPath);
                    break;
                case "ViewModel":
                    OpenPath(vm.ViewModelPath);
                    break;
                case "Text":
                    OpenPath(vm.TextPath);
                    break;
                case "Controller":
                    OpenPath(vm.ControllerPath);
                    break;
                case "Dto":
                    OpenPath(vm.DtoPath);
                    break;
                case "Entity":
                    OpenPath(vm.EntityPath);
                    break;
            }
        }

        private void OpenPath(string path)
        {
            System.Diagnostics.Process.Start(path);
        }

        private void GenView()
        {
            vm.Messages.Add(string.Format("{0} | Generating View ...", DateTime.Now));

            var viewNamespace = vm.ClientNamespace + ".View";
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                var path = vm.ViewPath + "\\" + entityClassName + "View.xaml";

                var properties = table.Columns.Where(p => p.ColumnName != "GroupID").Select(p => p.ToEntityProperty())
                    .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase).ToList();

                var xamlClass = CodeGenerator.GenViewXamlClass(viewNamespace, entityClassName, properties);
                System.IO.File.WriteAllText(path, xamlClass, Encoding.UTF8);

                var codeClass = CodeGenerator.GenViewCodeClass(viewNamespace, entityClassName);
                System.IO.File.WriteAllText(path + ".cs", codeClass, Encoding.UTF8);
            }
        }

        private void GenViewModel()
        {
            vm.Messages.Add(string.Format("{0} | Generating ViewModel ...", DateTime.Now));

            var viewNamespace = vm.ClientNamespace + ".ViewModel";
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                var properties = table.Columns.Where(p => p.ColumnName != "GroupID").Select(p => p.ToEntityProperty())
                   .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase).ToList();

                var viewModelClass = CodeGenerator.GenViewModelClass(viewNamespace, entityClassName, properties);
                var path = vm.ViewModelPath + "\\" + entityClassName + "ViewModel.cs";
                System.IO.File.WriteAllText(path, viewModelClass, Encoding.UTF8);
            }
        }

        private void GenText()
        {
            vm.Messages.Add(string.Format("{0} | Generating Text ...", DateTime.Now));

            var defaultLanguage = vm.DefaultLanguage.ToLower();
            var textData = new List<GenTextManagerViewModel.TextData>();
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                foreach (var column in table.Columns)
                {
                    if (column.ColumnName == "GroupID")
                        continue;

                    textData.Add(new GenTextManagerViewModel.TextData()
                    {
                        TextKey = string.Format("{0}_{1}", entityClassName, column.ColumnName),
                        TextValue = column.ColumnName
                    });
                }
            }

            var viewNamespace = vm.ClientNamespace;

            var textManagerClass = CodeGenerator.GenTextManagerClass(viewNamespace, textData);
            System.IO.File.WriteAllText(System.IO.Path.Combine(vm.TextPath, "TextManager.cs"), textManagerClass);

            using (var fs = System.IO.File.Create(System.IO.Path.Combine(vm.TextPath, defaultLanguage + ".txt")))
            using (var sw = new System.IO.StreamWriter(fs))
            {
                foreach (var line in textData)
                {
                    sw.WriteLine(string.Format("{0}\t\t{1}", line.TextKey, line.TextValue));
                }
            }
        }

        private void GenController()
        {
            vm.Messages.Add(string.Format("{0} | Generating Controller ...", DateTime.Now));

            var viewNamespace = vm.ServerNamespace + ".Controllers";
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true && p.TableName.StartsWith("Swa") == false))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                var properties = table.Columns.Select(p => p.ToEntityProperty())
                   .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase).ToList();

                var controllerClass = CodeGenerator.GenControllerClass(viewNamespace, entityClassName, vm.DbContextName, properties);
                var path = vm.ControllerPath + "\\" + entityClassName + "Controller.cs";
                System.IO.File.WriteAllText(path, controllerClass, Encoding.UTF8);
            }
        }

        private void GenDto()
        {
            vm.Messages.Add(string.Format("{0} | Generating Dto ...", DateTime.Now));

            var viewNamespace = vm.DtoNamespace;
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                var properties = table.Columns.Select(p => p.ToEntityProperty())
                   .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase).ToList();

                var dtoClass = CodeGenerator.GenDtoClassImplementINotifyPropertyChanged(viewNamespace, "IDto", entityClassName + "Dto", properties);
                var path = vm.DtoPath + "\\" + entityClassName + "Dto.cs";
                System.IO.File.WriteAllText(path, dtoClass, Encoding.UTF8);
            }
        }

        private void GenEntity()
        {
            vm.Messages.Add(string.Format("{0} | Generating Entity ...", DateTime.Now));

            var nameSpace = vm.ServerNamespace + ".Entities";
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                var properties = table.Columns.Select(p => p.ToEntityProperty())
                   .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase).ToList();

                var dtoClass = CodeGenerator.GenEntityClass(nameSpace, entityClassName, properties, table.ReferencesToThisTable);
                var path = vm.EntityPath + "\\" + entityClassName + ".cs";
                System.IO.File.WriteAllText(path, dtoClass, Encoding.UTF8);
            }

            var contextClass = CodeGenerator.GenDbContextClass(nameSpace, vm.DbContextName, vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true));
            var contextClassPath = vm.EntityPath + "\\" + vm.DbContextName + ".cs";
            System.IO.File.WriteAllText(contextClassPath, contextClass, Encoding.UTF8);
        }
    }
}
