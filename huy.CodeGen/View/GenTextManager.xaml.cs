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
        List<string> _textKeys = new List<string>();
        Dictionary<string, List<GenTextManagerViewModel.TextData>> _dic =
            new Dictionary<string, List<GenTextManagerViewModel.TextData>>();

        GenTextManagerViewModel vm;

        public GenTextManager()
        {
            InitializeComponent();
            vm = new GenTextManagerViewModel();
            vm.AllCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            vm.LanguageNameList = new ObservableCollection<string>();
            vm.Namespace = "Client";
            vm.DatabaseName = "PhuDinh";
            vm.SkippedTable = "__EFMigrationsHistory";
            vm.SkippedColumn = "GroupID";
            DataContext = vm;
            CreateNewButton_Click(null, null);
        }

        #region TextManager

        private void CreateNewButton_Click(object sender, RoutedEventArgs e)
        {
            vm.Result = CodeGenerator.GenTextManagerClass(vm.Namespace, new List<CodeGen.GenTextManagerViewModel.TextData>());
        }

        private void FromDBButton_Click(object sender, RoutedEventArgs e)
        {
            var data = new List<GenTextManagerViewModel.TextData>();

            var skippedTables = new List<string>(
                vm.SkippedTable.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            var skippedColumns = new List<string>(
                vm.SkippedColumn.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            foreach (var table in DatabaseUtils.ListTables(vm.DatabaseName))
            {
                if (skippedTables.Count > 0 && skippedTables.Contains(table))
                    continue;

                var properties = DatabaseUtils.ListColumnsOfTable(vm.DatabaseName, table);
                foreach (var prop in properties)
                {
                    if (skippedColumns.Count > 0 && skippedColumns.Contains(prop.PropertyName))
                        continue;

                    data.Add(new GenTextManagerViewModel.TextData()
                    {
                        TextKey = string.Format("{0}_{1}", table, prop.PropertyName),
                        TextValue = prop.PropertyName
                    });
                }
            }

            vm.Result = CodeGenerator.GenTextManagerClass(vm.Namespace, data);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.DefaultExt = "cs";
            sfd.Filter = "c# code file(*.cs)|*.cs";
            sfd.FileName = "TextManager.cs";
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, vm.Result);
            }
        }

        #endregion

        #region TextEditor

        private void LanguageNameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void TextEditor_Load_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Select TextManager.cs";
            ofd.Filter = "c# code file(*.cs)|*.cs";
            if (ofd.ShowDialog() == true)
            {
                vm.OutputPath = System.IO.Path.GetDirectoryName(ofd.FileName) + "\\";
                _textKeys = GetTextKeysFromTextManagerFile(vm.OutputPath);

                var di = new DirectoryInfo(vm.OutputPath);
                _dic.Clear();
                vm.LanguageNameList.Clear();
                foreach (var txtFile in di.EnumerateFiles("*.txt"))
                {
                    var data = new Dictionary<string, GenTextManagerViewModel.TextData>();
                    foreach (var key in _textKeys)
                    {
                        data.Add(key, new GenTextManagerViewModel.TextData() { TextKey = key });
                    }
                    foreach (var line in File.ReadAllLines(txtFile.FullName))
                    {
                        if (string.IsNullOrEmpty(line) == true)
                            break;

                        var texts = line.Split(new[] { "\t\t" }, StringSplitOptions.None);
                        data[texts[0]].TextValue = texts[1];
                    }

                    var languageName = System.IO.Path.GetFileNameWithoutExtension(txtFile.Name);
                    vm.LanguageNameList.Add(languageName);
                    _dic.Add(languageName, data.Values.ToList());
                }
            }
        }

        private void TextEditor_AddLanguage_Click(object sender, RoutedEventArgs e)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.0, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.0, GridUnitType.Auto) });
            var dataGrid = new DataGrid();
            dataGrid.ItemsSource = vm.AllCultures.Where(p => vm.LanguageNameList.Contains(p.Name.ToLower()) == false)
                .OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(p => new { p.DisplayName, Code = p.Name.ToLower() });
            var btn = new Button() { Content = "Ok", Width = 80, Margin = new Thickness(10) };
            Grid.SetRow(btn, 1);
            grid.Children.Add(dataGrid);
            grid.Children.Add(btn);
            var w = new Window()
            {
                Width = 450,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = "Choose Language",
                Content = grid,
                FontSize = 14
            };
            btn.Click += (s, a) =>
            {
                foreach (dynamic item in dataGrid.SelectedItems)
                {
                    dynamic code = item.Code.ToLower();
                    vm.LanguageNameList.Add(code);
                    var data = new List<GenTextManagerViewModel.TextData>();
                    foreach (var key in _textKeys)
                    {
                        data.Add(new GenTextManagerViewModel.TextData() { TextKey = key });
                    }
                    _dic.Add(code, data);
                }

                w.Close();
            };
            w.ShowDialog();
        }

        private void TextEditor_Save_Click(object sender, RoutedEventArgs e)
        {
            var data = _dic[vm.LanguageName];
            using (var fs = File.Create(System.IO.Path.Combine(vm.OutputPath, vm.LanguageName + ".txt")))
            using (var sw = new StreamWriter(fs))
            {
                foreach (var line in data)
                {
                    sw.WriteLine(string.Format("{0}\t\t{1}", line.TextKey, line.TextValue));
                }
            }
        }

        private List<string> GetTextKeysFromTextManagerFile(string path)
        {
            var result = new List<string>();

            var textManagerFilePath = System.IO.Path.Combine(path, "TextManager.cs");
            var line = File.ReadLines(textManagerFilePath).GetEnumerator();
            line.MoveNext();
            while (line.Current.Contains("private static void InitDefaultLanguageData()") == false)
            {
                line.MoveNext();
            }
            line.MoveNext();//"{"
            line.MoveNext();
            while (line.Current.Contains("_dic.Add") == true)
            {
                var text = line.Current;
                var index1 = text.IndexOf("\"") + 1;
                var index2 = text.IndexOf("\"", index1);
                result.Add(text.Substring(index1, index2 - index1));
                line.MoveNext();
            }

            var textManagerPartFilePath = System.IO.Path.Combine(path, "TextManager.part.cs");

            if (File.Exists(textManagerPartFilePath) == true)
            {
                line = File.ReadLines(textManagerPartFilePath).GetEnumerator();
                line.MoveNext();
                while (line.Current.Contains("InitDefaultLanguageDataPartial()") == false)
                {
                    line.MoveNext();
                }
                line.MoveNext();//"{"
                line.MoveNext();
                while (line.Current.Contains("_dic.Add") == true)
                {
                    var text = line.Current;
                    var index1 = text.IndexOf("\"") + 1;
                    var index2 = text.IndexOf("\"", index1);
                    result.Add(text.Substring(index1, index2 - index1));
                    line.MoveNext();
                }
            }
            return result;
        }

        #endregion
    }
}
