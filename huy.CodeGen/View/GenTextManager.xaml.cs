using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace huy.CodeGen
{
    /// <summary>
    /// Interaction logic for GenTextManager.xaml
    /// </summary>
    public partial class GenTextManager : UserControl
    {
        Dictionary<string, List<GenTextManagerViewModel.TextData>> _dic =
            new Dictionary<string, List<GenTextManagerViewModel.TextData>>();

        GenTextManagerViewModel vm;
        string _defaultLanguage = "en-us";

        public GenTextManager()
        {
            InitializeComponent();
            vm = new GenTextManagerViewModel();
            vm.AllCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            vm.LanguageNameList = new ObservableCollection<string>();
            vm.DatabaseName = "PhuDinhClientServer";
            vm.SkippedTable = "__EFMigrationsHistory";
            DataContext = vm;
            CreateNewButton_Click(null, null);
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            vm.Result = CodeGenerator.GenTextManagerClass(vm.Namespace, vm.DefaultLanguageName, _dic[vm.DefaultLanguageName]);
        }

        private void AddLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.0, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.0, GridUnitType.Auto) });
            var dataGrid = new DataGrid();
            dataGrid.ItemsSource = vm.AllCultures.Where(p => vm.LanguageNameList.Contains(p.Name) == false)
                .OrderBy(p => p.DisplayName)
                .Select(p => new { p.DisplayName, Code = p.Name });
            var btn = new Button() { Content = "Ok", Width = 80, Margin = new Thickness(10) };
            Grid.SetRow(btn, 1);
            grid.Children.Add(dataGrid);
            grid.Children.Add(btn);
            var w = new Window()
            {
                Width = 450,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = "Choose Language",
                Content = grid
            };
            btn.Click += (s, a) =>
            {
                foreach (dynamic item in dataGrid.SelectedItems)
                {
                    dynamic code = item.Code.ToLower();
                    vm.LanguageNameList.Add(code);
                    var data = new List<GenTextManagerViewModel.TextData>();
                    foreach (var key in _dic[vm.DefaultLanguageName])
                    {
                        data.Add(new GenTextManagerViewModel.TextData() { TextKey = key.TextKey });
                    }
                    _dic.Add(code, data);
                }

                w.Close();
            };
            w.ShowDialog();
        }

        private void CreateNewButton_Click(object sender, RoutedEventArgs e)
        {
            vm.Namespace = "Client";
            _dic.Clear();
            vm.TextDataList = new List<GenTextManagerViewModel.TextData>();
            _dic.Add(_defaultLanguage, vm.TextDataList);
            vm.LanguageNameList.Clear();
            vm.LanguageNameList.Add(_defaultLanguage);
            vm.LanguageName = _defaultLanguage;
            vm.DefaultLanguageName = _defaultLanguage;
            vm.OutputPath = @"C:\codegen\Text";
        }

        private void LoadFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Select TextManager.cs";
            ofd.Filter = "C# code (.cs)|*.cs";
            if (ofd.ShowDialog() == true)
            {
                vm.OutputPath = System.IO.Path.GetDirectoryName(ofd.FileName);
                vm.Result = File.ReadAllText(ofd.FileName);
                LoadLanguageFromTxtFile();
                LoadTextData(vm.Result);
                vm.LanguageName = vm.DefaultLanguageName;
            }
        }

        private void FromDBButton_Click(object sender, RoutedEventArgs e)
        {
            var data = new List<GenTextManagerViewModel.TextData>();
            
            var skippedTable = new List<string>(
                vm.SkippedTable.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            foreach (var table in DatabaseUtils.ListTables(vm.DatabaseName))
            {
                if (skippedTable.Count > 0 && skippedTable.Contains(table))
                    continue;

                var properties = DatabaseUtils.ListColumnsOfTable(vm.DatabaseName, table);
                foreach (var prop in properties)
                {
                    data.Add(new GenTextManagerViewModel.TextData()
                    {
                        TextKey = string.Format("{0}_{1}", table, prop.PropertyName),
                        TextValue = prop.PropertyName
                    });
                }
            }

            var l = vm.DefaultLanguageName;
            _dic.Clear();
            _dic.Add(l, data);
            vm.LanguageNameList.Clear();
            vm.LanguageNameList.Add(l);
            vm.TextDataList = data;
            vm.LanguageName = l;
            vm.DefaultLanguageName = l;

            GenerateButton_Click(null, null);
        }

        private void SaveToOutputPathButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(System.IO.Path.Combine(vm.OutputPath, "TextManager.cs"), vm.Result);
            foreach (var data in _dic)
            {
                using (var fs = File.Create(System.IO.Path.Combine(vm.OutputPath, data.Key + ".txt")))
                using (var sw = new StreamWriter(fs))
                {
                    foreach (var line in data.Value)
                    {
                        sw.WriteLine(string.Format("{0}\t\t{1}", line.TextKey, line.TextValue));
                    }
                }
            }
        }

        private void LoadLanguageFromTxtFile()
        {
            var di = new DirectoryInfo(vm.OutputPath);
            _dic.Clear();
            vm.LanguageNameList.Clear();
            foreach (var txtFile in di.EnumerateFiles("*.txt"))
            {
                var data = new List<GenTextManagerViewModel.TextData>();
                foreach (var line in File.ReadAllLines(txtFile.FullName))
                {
                    if (string.IsNullOrEmpty(line) == true)
                        break;

                    var texts = line.Split(new[] { "\t\t" }, StringSplitOptions.None);
                    data.Add(new GenTextManagerViewModel.TextData()
                    {
                        TextKey = texts[0],
                        TextValue = texts[1]
                    });
                }
                var languageName = System.IO.Path.GetFileNameWithoutExtension(txtFile.Name);
                vm.LanguageNameList.Add(languageName);
                _dic.Add(languageName, data);
            }
        }

        private void LoadTextData(string text)
        {
            var sr = new StringReader(text);
            for (int i = 0; i < 5; i++)//skip 5 line
            {
                sr.ReadLine();
            }

            vm.Namespace = sr.ReadLine().Substring("namespace ".Length);

            for (int i = 0; i < 4; i++)//skip 4 line
            {
                sr.ReadLine();
            }

            var current = sr.ReadLine();
            var index = current.IndexOf('"') + 1;
            var end = current.IndexOf('"', index);

            vm.DefaultLanguageName = current.Substring(index, end - index);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(vm.LanguageName) == true)
                return;

            List<GenTextManagerViewModel.TextData> textData;
            if (_dic.TryGetValue(vm.LanguageName, out textData) == false)
            {
                textData = new List<GenTextManagerViewModel.TextData>();
                _dic.Add(vm.LanguageName, textData);
            }

            vm.TextDataList = textData;
        }
    }
}
