using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
                    GenView(vm.ViewPath);
                    break;
                case "ViewModel":
                    GenViewModel(vm.ViewModelPath);
                    break;
                case "Text":
                    GenText(vm.TextPath);
                    break;
                case "Controller":
                    GenController(vm.ControllerPath);
                    break;
                case "Dto":
                    GenDto(vm.DtoPath);
                    break;
                case "Entity":
                    GenEntity(vm.EntityPath);
                    break;
                case "All":
                    GenAllCode();
                    break;
            }

            vm.Messages.Add(string.Format("{0} | Done.", DateTime.Now));
        }

        private void GenAllCode()
        {
            GenView(System.IO.Path.Combine(vm.ProjectPath, @"Client\Client\View\Gen"));
            GenViewModel(System.IO.Path.Combine(vm.ProjectPath, @"Client\Client\ViewModel\Gen"));
            GenText(System.IO.Path.Combine(vm.ProjectPath, @"Client\Client\Text"));
            GenController(System.IO.Path.Combine(vm.ProjectPath, @"Server\src\Server\Controllers\Gen"));
            GenDto(System.IO.Path.Combine(vm.ProjectPath, @"DTO\src\DTO\Gen"));
            GenEntity(System.IO.Path.Combine(vm.ProjectPath, @"Server\src\Server\Entities\Gen"));
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
                case "All":
                    OpenPath(vm.ProjectPath);
                    break;
            }
        }

        private void OpenPath(string path)
        {
            System.Diagnostics.Process.Start(path);
        }

        private void GenView(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating View ...", DateTime.Now));

            var viewNamespace = vm.ClientNamespace + ".View";
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                var tempPath = System.IO.Path.Combine(path, entityClassName + "View.xaml");

                var properties = table.Columns.Where(p => p.ColumnName != "GroupID").Select(p => p.ToEntityProperty())
                    .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase).ToList();

                var xamlClass = CodeGenerator.GenViewXamlClass(viewNamespace, entityClassName, properties);
                FileUtils.WriteAllTextInUTF8(tempPath, xamlClass);

                var codeClass = CodeGenerator.GenViewCodeClass(viewNamespace, entityClassName);
                FileUtils.WriteAllTextInUTF8(tempPath + ".cs", codeClass);
            }
        }

        private void GenViewModel(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating ViewModel ...", DateTime.Now));

            var viewNamespace = vm.ClientNamespace + ".ViewModel";
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                var properties = table.Columns.Where(p => p.ColumnName != "GroupID").Select(p => p.ToEntityProperty())
                   .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase).ToList();

                var viewModelClass = CodeGenerator.GenViewModelClass(viewNamespace, entityClassName, properties);

                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(path, entityClassName + "ViewModel.cs"), viewModelClass);
            }
        }

        private void GenText(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Text ...", DateTime.Now));

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
            FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(path, "TextManager.cs"), textManagerClass);
        }

        private void GenController(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Controller ...", DateTime.Now));

            var viewNamespace = vm.ServerNamespace + ".Controllers";
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true && p.TableName.StartsWith("Swa") == false))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                var properties = table.Columns.Select(p => p.ToEntityProperty())
                   .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase).ToList();

                var controllerClass = CodeGenerator.GenControllerClass(viewNamespace, entityClassName, vm.DbContextName, properties);

                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(path, entityClassName + "Controller.cs"), controllerClass);
            }
        }

        private void GenDto(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Dto ...", DateTime.Now));

            var viewNamespace = vm.DtoNamespace;
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                var properties = table.Columns.Select(p => p.ToEntityProperty())
                   .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase).ToList();

                var dtoClass = CodeGenerator.GenDtoClassImplementINotifyPropertyChanged(viewNamespace, "IDto", entityClassName + "Dto", properties);

                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(path, entityClassName + "Dto.cs"), dtoClass);
            }
        }

        private void GenEntity(string path)
        {
            vm.Messages.Add(string.Format("{0} | Generating Entity ...", DateTime.Now));

            var nameSpace = vm.ServerNamespace + ".Entities";
            foreach (var table in vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true))
            {
                var entityClassName = DatabaseUtils.UpperFirstLetter(table.TableName);
                var properties = table.Columns.Select(p => p.ToEntityProperty())
                   .OrderBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase).ToList();

                var dtoClass = CodeGenerator.GenEntityClass(nameSpace, entityClassName, properties, table.ReferencesToThisTable);

                FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(path, entityClassName + ".cs"), dtoClass);
            }

            var contextClass = CodeGenerator.GenDbContextClass(nameSpace, vm.DbContextName, vm.DatabaseTreeVM.DbTables.Where(p => p.IsSelected == true));

            FileUtils.WriteAllTextInUTF8(System.IO.Path.Combine(path, vm.DbContextName + ".cs"), contextClass);
        }
    }
}
