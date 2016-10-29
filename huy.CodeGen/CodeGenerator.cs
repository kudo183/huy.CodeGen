using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huy.CodeGen
{
    public static class CodeGenerator
    {
        private static readonly string LineEnding = "\r\n";

        public static string GenDtoClassImplementINotifyPropertyChanged(string nameSpace, string interfaceName, string className, List<EntityProperty> properties)
        {
            var sb = new StringBuilder();
            var tab = "    ";
            var tab2 = tab + tab;
            var tab3 = tab2 + tab;
            sb.AppendLine("using System.ComponentModel;");
            sb.AppendLine();
            sb.AppendFormat("namespace {0}\r\n", nameSpace);
            sb.AppendLine("{");
            sb.AppendFormat("{0}[ProtoBuf.ProtoContract]\r\n", tab);
            sb.AppendFormat("{0}public partial class {1} : {2}, INotifyPropertyChanged\r\n", tab, className, interfaceName);
            sb.AppendLine(tab + "{");
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}{1} o{2};\r\n", tab2, item.PropertyType, item.PropertyName);
            }
            sb.AppendLine();
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}{1} _{2};\r\n", tab2, item.PropertyType, item.PropertyName);
            }
            sb.AppendLine();
            for (var i = 0; i < properties.Count; i++)
            {
                var item = properties[i];
                sb.AppendFormat("{0}[ProtoBuf.ProtoMember({1})]\r\n", tab2, i + 1);
                sb.AppendLine(GenProperty(tab2, item.PropertyType, item.PropertyName));
            }
            sb.AppendLine();
            sb.AppendFormat("{0}public void SetCurrentValueAsOriginalValue()\r\n", tab2);
            sb.AppendLine(tab2 + "{");
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}o{1} = {2};\r\n", tab3, item.PropertyName, item.PropertyName);
            }
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            sb.AppendFormat("{0}public bool HasChange()\r\n", tab2);
            sb.AppendLine(tab2 + "{");
            sb.AppendFormat("{0}return (o{1} != {2})\r\n", tab3, properties[0].PropertyName, properties[0].PropertyName);
            for (var i = 1; i < properties.Count; i++)
            {
                var item = properties[i];
                sb.AppendFormat("{0}|| (o{1} != {2})\r\n", tab3, item.PropertyName, item.PropertyName);
            }
            sb.AppendLine(";");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            foreach (var item in properties.Where(p => p.IsForeignKey))
            {
                sb.AppendFormat("{0}{1} _{2}Sources;\r\n", tab2, "object", item.PropertyName);
            }
            sb.AppendLine();
            foreach (var item in properties.Where(p => p.IsForeignKey))
            {
                sb.AppendFormat("{0}[Newtonsoft.Json.JsonIgnore]\r\n", tab2);
                sb.AppendLine(GenProperty(tab2, "object", item.PropertyName + "Sources"));
            }
            sb.AppendLine();
            sb.AppendFormat("{0}public event PropertyChangedEventHandler PropertyChanged;\r\n", tab2);
            sb.AppendFormat("{0}public virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)\r\n", tab2);
            sb.AppendLine(tab2 + "{");
            sb.AppendFormat("{0}PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));\r\n", tab3);
            sb.AppendLine(tab2 + "}");
            sb.AppendLine(tab + "}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string GenDtoClass(string nameSpace, string interfaceName, string className, List<EntityProperty> properties)
        {
            var sb = new StringBuilder();
            var tab = "    ";
            var tab2 = tab + tab;
            var tab3 = tab2 + tab;
            sb.AppendLine("namespace " + nameSpace);
            sb.AppendLine("{");
            sb.AppendLine(tab + "[ProtoBuf.ProtoContract]");
            sb.AppendLine(tab + "public partial class " + className + " : " + interfaceName);
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

        public static string GenControllerClass(string nameSpace, string enityClassName, string contextName, List<EntityProperty> properties)
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
            sb.AppendLine(string.Format("{0}public partial class {1}Controller : SwaEntityBaseController<{2}, {1}, {1}Dto>", tab, enityClassName, contextName));
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

        public static string GenTextManagerClass(string nameSpace, string defaultLanguage, List<GenTextManagerViewModel.TextData> textDataList)
        {
            var sb = new StringBuilder();
            var tab = "    ";
            var tab2 = tab + tab;
            var tab3 = tab2 + tab;
            var tab4 = tab3 + tab;
            var tab5 = tab4 + tab;
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine("using System.Windows;");
            sb.AppendLine();
            sb.AppendLine("namespace " + nameSpace);
            sb.AppendLine("{");
            sb.AppendLine(tab + "public static class TextManager");
            sb.AppendLine(tab + "{");
            sb.AppendLine(tab2 + "static readonly Dictionary<string, string> _dic = new Dictionary<string, string>();");
            sb.AppendFormat("{0}static readonly string DefaultLanguage = \"{1}\";\r\n", tab2, defaultLanguage);
            sb.AppendLine();
            sb.AppendLine(tab2 + "static TextManager()");
            sb.AppendLine(tab2 + "{");
            sb.AppendLine(tab3 + "if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()) == true)");
            sb.AppendLine(tab3 + "{");
            sb.AppendLine(tab4 + "InitDefaultLanguageData();");
            sb.AppendLine(tab4 + "return;");
            sb.AppendLine(tab3 + "}");
            sb.AppendLine();
            sb.AppendLine(tab3 + "var language = Thread.CurrentThread.CurrentUICulture.Name.ToLower();");
            sb.AppendLine(tab3 + "if (language == DefaultLanguage)");
            sb.AppendLine(tab3 + "{");
            sb.AppendLine(tab4 + "InitDefaultLanguageData();");
            sb.AppendLine(tab4 + "return;");
            sb.AppendLine(tab3 + "}");
            sb.AppendLine();
            sb.AppendLine(tab3 + "var fileName = language + \".txt\";");
            sb.AppendLine(tab3 + "var di = new System.IO.DirectoryInfo(\"text\");");
            sb.AppendLine(tab3 + "var fi = di.GetFiles(fileName);");
            sb.AppendLine(tab3 + "if (fi.Length == 1)");
            sb.AppendLine(tab3 + "{");
            sb.AppendLine(tab4 + "var sr = fi[0].OpenText();");
            sb.AppendLine(tab4 + "var line = sr.ReadLine();");
            sb.AppendLine(tab4 + "while (string.IsNullOrEmpty(line) == false)");
            sb.AppendLine(tab4 + "{");
            sb.AppendLine(tab5 + "var texts = line.Split(new[] { \"\\t\\t\" }, System.StringSplitOptions.None);");
            sb.AppendLine(tab5 + "_dic.Add(texts[0], texts[1]);");
            sb.AppendLine(tab5 + "line = sr.ReadLine();");
            sb.AppendLine(tab4 + "}");
            sb.AppendLine(tab4 + "sr.Close();");
            sb.AppendLine(tab3 + "}");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            foreach (var item in textDataList)
            {
                sb.AppendFormat("{0}public static string {1} {{ get {{ return GetText(); }} }}\r\n", tab2, item.TextKey);
            }
            sb.AppendLine();
            sb.AppendLine(tab2 + "public static string GetText([CallerMemberName] string textKey = null)");
            sb.AppendLine(tab2 + "{");
            sb.AppendLine(tab3 + "string text;");
            sb.AppendLine(tab3 + "if (_dic.TryGetValue(textKey, out text) == true)");
            sb.AppendLine(tab3 + "{");
            sb.AppendLine(tab4 + "return text;");
            sb.AppendLine(tab3 + "}");
            sb.AppendLine(tab3 + "return \"[no text]\";");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            sb.AppendLine(tab2 + "private static void InitDefaultLanguageData()");
            sb.AppendLine(tab2 + "{");
            foreach (var item in textDataList)
            {
                sb.AppendFormat("{0}_dic.Add(\"{1}\", \"{2}\");\r\n", tab3, item.TextKey, item.TextValue);
            }
            sb.AppendLine(tab2 + "}");
            sb.AppendLine(tab + "}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string GenViewModelClass(string nameSpace, string entityClassName, List<EntityProperty> properties)
        {
            var sb = new StringBuilder();
            var dtoClassName = entityClassName + "Dto";
            var className = entityClassName + "ViewModel";
            var hasForeignKey = properties.Any(p => p.IsForeignKey == true);
            var tab = "    ";
            var tab2 = tab + tab;
            var tab3 = tab2 + tab;
            var tab4 = tab3 + tab;
            sb.AppendLine("using Client.Abstraction;");
            sb.AppendLine("using DTO;");
            sb.AppendLine("using SimpleDataGrid;");
            sb.AppendLine("using SimpleDataGrid.ViewModel;");
            sb.AppendLine();
            sb.AppendLine("namespace " + nameSpace);
            sb.AppendLine("{");
            sb.AppendFormat("{0}public partial class {1} : BaseViewModel<{2}>{3}", tab, className, dtoClassName, LineEnding);
            sb.AppendLine(tab + "{");
            sb.AppendLine(tab2 + "partial void InitFilterPartial();");
            if (hasForeignKey == true)
            {
                sb.AppendLine(tab2 + "partial void LoadReferenceDataPartial();");
            }
            sb.AppendFormat("{0}partial void ProcessDtoBeforeAddToEntitiesPartial({1} dto);{2}", tab2, dtoClassName, LineEnding);
            sb.AppendFormat("{0}partial void ProcessNewAddedDtoPartial({1} dto);{2}", tab2, dtoClassName, LineEnding);
            sb.AppendLine();
            foreach (var item in properties)
            {
                var filterType = GetFilterTypeFromProperty(item);
                sb.AppendFormat("{0}{1} _{2}Filter;{3}",
                    tab2, filterType, item.PropertyName, LineEnding);
            }
            sb.AppendLine();
            sb.AppendFormat("{0}public {1}() : base(){2}", tab2, className, LineEnding);
            sb.AppendLine(tab2 + "{");
            foreach (var item in properties)
            {
                if (item.IsForeignKey == true)
                {
                    sb.AppendFormat("{0}_{1}Filter = new HeaderComboBoxFilterModel({2}", tab3, item.PropertyName, LineEnding);
                    sb.AppendFormat("{0}TextManager.{1}_{2}, HeaderComboBoxFilterModel.ComboBoxFilter,{3}", tab4, entityClassName, item.PropertyName, LineEnding);
                    sb.AppendFormat("{0}nameof({1}.{2}),{3}", tab4, dtoClassName, item.PropertyName, LineEnding);
                    sb.AppendFormat("{0}typeof({1}),{2}", tab4, item.PropertyType, LineEnding);
                    sb.AppendFormat("{0}nameof({1}Dto.TenHienThi),{2}", tab4, item.ForeignKeyTableName, LineEnding);
                    sb.AppendFormat("{0}nameof({1}Dto.Ma));{2}", tab4, item.ForeignKeyTableName, LineEnding);
                    sb.AppendFormat("{0}_{1}Filter.AddCommand = new SimpleCommand(\"{1}AddCommand\",{2}", tab3, item.PropertyName, LineEnding);
                    sb.AppendLine(tab4 + "() => base.ProccessHeaderAddCommand(");
                    sb.AppendFormat("{0}new View.{1}View(), \"{1}\", ReferenceDataManager<{1}Dto>.Instance.Load){2}", tab4, item.ForeignKeyTableName, LineEnding);
                    sb.AppendLine(tab3 + ");");
                    sb.AppendFormat("{0}_{1}Filter.ItemSource = ReferenceDataManager<{2}Dto>.Instance.Get();{3}", tab3, item.PropertyName, item.ForeignKeyTableName, LineEnding);
                }
                else
                {
                    var filterType = GetFilterTypeFromProperty(item);
                    sb.AppendFormat("{0}_{1}Filter = new {2}(TextManager.{3}_{4}, nameof({5}.{4}), typeof({6}));{7}",
                        tab3, item.PropertyName, filterType, entityClassName, item.PropertyName, dtoClassName, item.PropertyType, LineEnding);
                }
                sb.AppendLine();
            }
            sb.AppendLine(tab3 + "InitFilterPartial();");
            sb.AppendLine();
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}AddHeaderFilter(_{1}Filter);{2}", tab3, item.PropertyName, LineEnding);
            }
            sb.AppendLine(tab2 + "}");
            if (hasForeignKey == true)
            {
                sb.AppendLine();
                sb.AppendLine(tab2 + "public override void LoadReferenceData()");
                sb.AppendLine(tab2 + "{");
                foreach (var item in properties.Where(p => p.IsForeignKey == true))
                {
                    sb.AppendFormat("{0}ReferenceDataManager<{1}Dto>.Instance.Load();{2}", tab3, item.ForeignKeyTableName, LineEnding);
                }
                sb.AppendLine();
                sb.AppendLine(tab3 + "LoadReferenceDataPartial();");
                sb.AppendLine(tab2 + "}");
            }
            sb.AppendLine();
            sb.AppendFormat("{0}protected override void ProcessDtoBeforeAddToEntities({1} dto){2}", tab2, dtoClassName, LineEnding);
            sb.AppendLine(tab2 + "{");
            foreach (var item in properties.Where(p => p.IsForeignKey == true))
            {
                sb.AppendFormat("{0}dto.{1}Sources = ReferenceDataManager<{2}Dto>.Instance.Get();{3}",
                    tab3, item.PropertyName, item.ForeignKeyTableName, LineEnding);
            }
            sb.AppendLine();
            sb.AppendLine(tab3 + "ProcessDtoBeforeAddToEntitiesPartial(dto);");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            sb.AppendFormat("{0}protected override void ProcessNewAddedDto({1} dto){2}", tab2, dtoClassName, LineEnding);
            sb.AppendLine(tab2 + "{");
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}if (_{1}Filter.FilterValue != null){2}", tab3, item.PropertyName, LineEnding);
                sb.AppendLine(tab3 + "{");
                sb.AppendFormat("{0}dto.{1} = ({2})_{1}Filter.FilterValue;{3}",
                    tab4, item.PropertyName, item.PropertyType, LineEnding);
                sb.AppendLine(tab3 + "}");
            }
            sb.AppendLine();
            sb.AppendLine(tab3 + "ProcessNewAddedDtoPartial(dto);");
            sb.AppendLine(tab3 + "ProcessDtoBeforeAddToEntities(dto);");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine(tab + "}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string GenViewClass(string nameSpace, string entityClassName, List<EntityProperty> properties)
        {
            var sb = new StringBuilder();
            var dtoClassName = entityClassName + "Dto";
            var className = entityClassName + "View";
            var hasForeignKey = properties.Any(p => p.IsForeignKey == true);
            var tab = "    ";
            var tab2 = tab + tab;
            var tab3 = tab2 + tab;
            var tab4 = tab3 + tab;
            sb.AppendFormat("<Abstraction:BaseView x:TypeArguments=\"Dto:{0}\"{1}", dtoClassName, LineEnding);
            sb.AppendFormat("{0}x:Class=\"{1}.{2}\"{3}", tab, nameSpace, className, LineEnding);
            sb.AppendLine(tab + "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            sb.AppendLine(tab + "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
            sb.AppendLine(tab + "xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\"");
            sb.AppendLine(tab + "xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"");
            sb.AppendLine(tab + "xmlns:SimpleDataGrid=\"clr-namespace:SimpleDataGrid;assembly=SimpleDataGrid\"");
            sb.AppendLine(tab + "xmlns:Dto=\"clr-namespace:DTO;assembly=DTO\"");
            sb.AppendLine(tab + "xmlns:Abstraction=\"clr-namespace:Client.Abstraction;assembly=Client.Abstraction\">");
            sb.AppendLine(tab + "<SimpleDataGrid:EditableGridView>");
            sb.AppendLine(tab2 + "<SimpleDataGrid:EditableGridView.Columns>");
            foreach (var item in properties)
            {
                if (item.IsIdentity == true)
                {
                    sb.AppendFormat("{0}<SimpleDataGrid:DataGridTextColumnExt Width=\"80\" Header=\"{1}\" IsReadOnly=\"True\" Binding=\"{{Binding {1}, Mode=OneWay}}\"/>{2}", tab3, item.PropertyName, LineEnding);
                }
                else if (item.IsForeignKey == true)
                {
                    sb.AppendFormat("{0}<SimpleDataGrid:DataGridComboBoxColumnExt Header=\"{1}\"{2}", tab3, item.PropertyName, LineEnding);
                    sb.AppendLine(tab4 + "SelectedValuePath=\"Ma\"");
                    sb.AppendLine(tab4 + "DisplayMemberPath=\"TenHienThi\"");
                    sb.AppendFormat("{0}SelectedValueBinding=\"{{Binding {1}, UpdateSourceTrigger=PropertyChanged}}\"{2}", tab4, item.PropertyName, LineEnding);
                    sb.AppendFormat("{0}ItemsSource=\"{{Binding {1}Sources}}\"/>{2}", tab4, item.PropertyName, LineEnding);
                }
                else
                {
                    var columnType = GetDataGridColumnTypeFromProperty(item);
                    sb.AppendFormat("{0}<SimpleDataGrid:{1} Header=\"{2}\" Binding=\"{{Binding {2}, UpdateSourceTrigger=PropertyChanged}}\"/>{3}", tab3, columnType, item.PropertyName, LineEnding);
                }
            }
            sb.AppendLine(tab2 + "</SimpleDataGrid:EditableGridView.Columns>");
            sb.AppendLine(tab + "</SimpleDataGrid:EditableGridView>");
            sb.AppendLine("</Abstraction:BaseView>");
            return sb.ToString();
        }

        private static string GenProperty(string tab, string propertyType, string propertyName)
        {
            return string.Format("{0}public {1} {2} {{ get {{ return _{2}; }} set {{ _{2} = value; OnPropertyChanged(); }} }}",
                    tab, propertyType, propertyName);
        }

        private static string GetFilterTypeFromProperty(EntityProperty property)
        {
            if (property.IsForeignKey == true)
            {
                return "HeaderComboBoxFilterModel";
            }
            var propertyType = property.PropertyType;
            if (propertyType == "string" || propertyType == "int" || propertyType == "int?")
            {
                return "HeaderTextFilterModel";
            }
            else if (propertyType == "bool" || propertyType == "bool?")
            {
                return "HeaderCheckFilterModel";
            }
            else if (propertyType == "System.DateTime" || propertyType == "System.DateTime?")
            {
                return "HeaderDateFilterModel";
            }

            return "HeaderTextFilterModel";
        }

        private static string GetDataGridColumnTypeFromProperty(EntityProperty property)
        {
            if (property.IsIdentity == true)
            {
                return "DataGridTextColumnExt";
            }
            if (property.IsForeignKey == true)
            {
                return "DataGridComboBoxColumnExt";
            }
            var propertyType = property.PropertyType;
            if (propertyType == "string" || propertyType == "int" || propertyType == "int?")
            {
                return "DataGridTextColumnExt";
            }
            else if (propertyType == "bool" || propertyType == "bool?")
            {
                return "DataGridCheckBoxColumnExt";
            }
            else if (propertyType == "System.DateTime" || propertyType == "System.DateTime?")
            {
                return "DataGridDateColumn";
            }

            return "DataGridTextColumnExt";
        }
    }
}
