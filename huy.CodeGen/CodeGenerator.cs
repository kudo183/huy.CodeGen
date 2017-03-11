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
            sb.AppendFormat("namespace {0}{1}", nameSpace, LineEnding);
            sb.AppendLine("{");
            sb.AppendFormat("{0}[ProtoBuf.ProtoContract]{1}", tab, LineEnding);
            sb.AppendFormat("{0}public partial class {1} : {2}, INotifyPropertyChanged{3}", tab, className, interfaceName, LineEnding);
            sb.AppendLine(tab + "{");
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}{1} o{2};{3}", tab2, item.PropertyType, item.PropertyName, LineEnding);
            }
            sb.AppendLine();
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}{1} _{2};{3}", tab2, item.PropertyType, item.PropertyName, LineEnding);
            }
            sb.AppendLine();
            for (var i = 0; i < properties.Count; i++)
            {
                var item = properties[i];
                sb.AppendFormat("{0}[ProtoBuf.ProtoMember({1})]{2}", tab2, i + 1, LineEnding);
                sb.AppendLine(GenProperty(tab2, item.PropertyType, item.PropertyName));
            }
            sb.AppendLine();
            sb.AppendFormat("{0}public void SetCurrentValueAsOriginalValue(){1}", tab2, LineEnding);
            sb.AppendLine(tab2 + "{");
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}o{1} = {2};{3}", tab3, item.PropertyName, item.PropertyName, LineEnding);
            }
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            sb.AppendFormat("{0}public bool HasChange(){1}", tab2, LineEnding);
            sb.AppendLine(tab2 + "{");
            sb.AppendFormat("{0}return (o{1} != {2})", tab3, properties[0].PropertyName, properties[0].PropertyName);
            for (var i = 1; i < properties.Count; i++)
            {
                var item = properties[i];
                sb.AppendLine();
                sb.AppendFormat("{0}|| (o{1} != {2})", tab3, item.PropertyName, item.PropertyName);
            }
            sb.AppendLine(";");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            foreach (var item in properties.Where(p => p.IsForeignKey))
            {
                sb.AppendFormat("{0}{1} _{2}Sources;{3}", tab2, "object", item.PropertyName, LineEnding);
            }
            sb.AppendLine();
            foreach (var item in properties.Where(p => p.IsForeignKey))
            {
                sb.AppendFormat("{0}[Newtonsoft.Json.JsonIgnore]{1}", tab2, LineEnding);
                sb.AppendLine(GenProperty(tab2, "object", item.PropertyName + "Sources"));
            }
            sb.AppendLine();
            sb.AppendFormat("{0}public event PropertyChangedEventHandler PropertyChanged;{1}", tab2, LineEnding);
            sb.AppendFormat("{0}public virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)\r\n", tab2);
            sb.AppendLine(tab2 + "{");
            sb.AppendFormat("{0}PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));{1}", tab3, LineEnding);
            sb.AppendLine(tab2 + "}");

            var pkName = properties.First(p => p.IsIdentity).PropertyName;
            if (pkName != "ID")
            {
                sb.AppendLine();
                sb.AppendLine(tab2 + "[Newtonsoft.Json.JsonIgnore]");
                sb.AppendFormat("{0}public int ID {{ get {{ return {1}; }} set {{ {1} = value;}} }}{2}", tab2, pkName, LineEnding);
            }

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

            var pkName = properties.First(p => p.IsIdentity).PropertyName;
            if (pkName != "ID")
            {
                sb.AppendLine();
                sb.AppendLine(tab2 + "[Newtonsoft.Json.JsonIgnore]");
                sb.AppendFormat("{0}public int ID {{ get {{ return {1}; }} set {{ {1} = value;}} }}{2}", tab2, pkName, LineEnding);
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
            sb.AppendFormat("{0}public partial class {1}Controller : SwaEntityBaseController<{2}, {1}, {1}Dto>{3}", tab, enityClassName, contextName, LineEnding);
            sb.AppendLine(tab + "{");
            sb.AppendFormat("{0}partial void ConvertToDtoPartial(ref {1}Dto dto, {1} entity);{2}", tab2, enityClassName, LineEnding);
            sb.AppendFormat("{0}partial void ConvertToEntityPartial(ref {1} entity, {1}Dto dto);{2}", tab2, enityClassName, LineEnding);
            sb.AppendLine();
            sb.AppendFormat("{0}public override {1}Dto ConvertToDto({1} entity){2}", tab2, enityClassName, LineEnding);
            sb.AppendLine(tab2 + "{");
            sb.AppendFormat("{0}var dto = new {1}Dto();{2}", tab3, enityClassName, LineEnding);
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}dto.{1} = entity.{1};{2}", tab3, item.PropertyName, LineEnding);
            }
            sb.AppendLine();
            sb.AppendLine(tab3 + "ConvertToDtoPartial(ref dto, entity);");
            sb.AppendLine();
            sb.AppendLine(tab3 + "return dto;");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            sb.AppendFormat("{0}public override {1} ConvertToEntity({1}Dto dto){2}", tab2, enityClassName, LineEnding);
            sb.AppendLine(tab2 + "{");
            sb.AppendFormat("{0}var entity = new {1}();{2}", tab3, enityClassName, LineEnding);
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}entity.{1} = dto.{1};{2}", tab3, item.PropertyName, LineEnding);
            }
            sb.AppendLine();
            sb.AppendLine(tab3 + "ConvertToEntityPartial(ref entity, dto);");
            sb.AppendLine();
            sb.AppendLine(tab3 + "return entity;");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine(tab + "}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string GenTextManagerClass(string nameSpace, List<GenTextManagerViewModel.TextData> textDataList)
        {
            var sb = new StringBuilder();
            var tab = "    ";
            var tab2 = tab + tab;
            var tab3 = tab2 + tab;
            var tab4 = tab3 + tab;
            var tab5 = tab4 + tab;
            var tab6 = tab5 + tab;
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine("using System.Windows;");
            sb.AppendLine();
            sb.AppendLine("namespace " + nameSpace);
            sb.AppendLine("{");
            sb.AppendLine(tab + "public static partial class TextManager");
            sb.AppendLine(tab + "{");
            sb.AppendLine(tab2 + "static partial void InitDefaultLanguageDataPartial();");
            sb.AppendLine();
            sb.AppendLine(tab2 + "static readonly Dictionary<string, string> _dic = new Dictionary<string, string>();");
            sb.AppendLine(tab2 + "public static string Language;");
            sb.AppendLine();
            sb.AppendLine(tab2 + "static TextManager()");
            sb.AppendLine(tab2 + "{");
            sb.AppendLine(tab3 + "if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()) == true)");
            sb.AppendLine(tab3 + "{");
            sb.AppendLine(tab4 + "InitDefaultLanguageData();");
            sb.AppendLine(tab4 + "return;");
            sb.AppendLine(tab3 + "}");
            sb.AppendLine();
            sb.AppendLine(tab3 + "var language = Language;");
            sb.AppendLine(tab3 + "if (string.IsNullOrEmpty(language) == true)");
            sb.AppendLine(tab3 + "{");
            sb.AppendLine(tab4 + "language = Thread.CurrentThread.CurrentUICulture.Name.ToLower();");
            sb.AppendLine(tab3 + "}");
            sb.AppendLine();
            sb.AppendLine(tab3 + "var fileName = language + \".txt\";");
            sb.AppendLine(tab3 + "try");
            sb.AppendLine(tab3 + "{");
            sb.AppendLine(tab4 + "var di = new System.IO.DirectoryInfo(\"text\");");
            sb.AppendLine(tab4 + "var fi = di.GetFiles(fileName);");
            sb.AppendLine(tab4 + "if (fi.Length == 1)");
            sb.AppendLine(tab4 + "{");
            sb.AppendLine(tab5 + "var sr = fi[0].OpenText();");
            sb.AppendLine(tab5 + "var line = sr.ReadLine();");
            sb.AppendLine(tab5 + "while (string.IsNullOrEmpty(line) == false)");
            sb.AppendLine(tab5 + "{");
            sb.AppendLine(tab6 + "var texts = line.Split(new[] { \"\\t\\t\" }, System.StringSplitOptions.None);");
            sb.AppendLine(tab6 + "_dic.Add(texts[0], texts[1]);");
            sb.AppendLine(tab6 + "line = sr.ReadLine();");
            sb.AppendLine(tab5 + "}");
            sb.AppendLine(tab5 + "sr.Close();");
            sb.AppendLine(tab4 + "}");
            sb.AppendLine(tab4 + "else");
            sb.AppendLine(tab4 + "{");
            sb.AppendLine(tab5 + "InitDefaultLanguageData();");
            sb.AppendLine(tab4 + "}");
            sb.AppendLine(tab3 + "}");
            sb.AppendLine(tab3 + "catch");
            sb.AppendLine(tab3 + "{");
            sb.AppendLine(tab4 + "InitDefaultLanguageData();");
            sb.AppendLine(tab3 + "}");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            foreach (var item in textDataList)
            {
                sb.AppendFormat("{0}public static string {1} {{ get {{ return GetText(); }} }}{2}", tab2, item.TextKey, LineEnding);
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
                sb.AppendFormat("{0}_dic.Add(\"{1}\", \"{2}\");{3}", tab3, item.TextKey, item.TextValue, LineEnding);
            }
            sb.AppendLine();
            sb.AppendLine(tab3 + "InitDefaultLanguageDataPartial();");
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
            var tab5 = tab4 + tab;
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
                sb.AppendFormat("{0}HeaderFilterBaseModel _{1}Filter;{2}",
                    tab2, item.PropertyName, LineEnding);
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
                    sb.AppendFormat("{0}nameof({1}Dto.ID)){2}", tab4, item.ForeignKeyTableName, LineEnding);
                    sb.AppendLine(tab3 + "{");
                    sb.AppendFormat("{0}AddCommand = new SimpleCommand(\"{1}AddCommand\",{2}", tab4, item.PropertyName, LineEnding);
                    sb.AppendLine(tab5 + "() => base.ProccessHeaderAddCommand(");
                    sb.AppendFormat("{0}new View.{1}View(), \"{1}\", ReferenceDataManager<{1}Dto>.Instance.Load)),{2}", tab5, item.ForeignKeyTableName, LineEnding);
                    sb.AppendFormat("{0}ItemSource = ReferenceDataManager<{1}Dto>.Instance.Get(){2}", tab4, item.ForeignKeyTableName, LineEnding);
                    sb.AppendLine(tab3 + "};");
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

        public static string GenViewXamlClass(string nameSpace, string entityClassName, List<EntityProperty> properties)
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
                    sb.AppendLine(tab4 + "SelectedValuePath=\"ID\"");
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

        public static string GenViewCodeClass(string nameSpace, string entityClassName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Client.Abstraction;");
            sb.AppendLine("namespace " + nameSpace);
            sb.AppendLine("{");
            sb.AppendLine(string.Format("    public partial class {0}View : BaseView<DTO.{0}Dto>", entityClassName));
            sb.AppendLine("    {");
            sb.AppendLine(string.Format("        partial void InitUIPartial();"));
            sb.AppendLine();
            sb.AppendLine(string.Format("        public {0}View() : base()", entityClassName));
            sb.AppendLine("        {");
            sb.AppendLine("            InitializeComponent();");
            sb.AppendLine();
            sb.AppendLine("            InitUIPartial();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string GenDbContextClass(string nameSpace, string contextName, IEnumerable<ViewModel.DbTable> tables)
        {
            var sb = new StringBuilder();
            var tab = "    ";
            var tab2 = tab + tab;
            var tab3 = tab2 + tab;
            var tab4 = tab3 + tab;
            var tab5 = tab4 + tab;

            sb.AppendLine("using System;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata;");
            sb.AppendLine("using huypq.SwaMiddleware;");
            sb.AppendLine();
            sb.AppendLine("namespace " + nameSpace);
            sb.AppendLine("{");
            sb.AppendFormat("{0}public partial class {1} : DbContext, SwaIDbContext<SwaUser, SwaGroup, SwaUserGroup>{2}", tab, contextName, LineEnding);
            sb.AppendLine(tab + "{");
            //sb.AppendFormat("{0}public {1}(DbContextOptions<{1}> options) : base(options){2}", tab2, contextName, LineEnding);
            //sb.AppendLine(tab2 + "{");
            //sb.AppendLine(tab3 + "ChangeTracker.AutoDetectChangesEnabled = false;");
            //sb.AppendLine(tab3 + "ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;");
            //sb.AppendLine(tab2 + "}");
            //sb.AppendLine();
            sb.AppendLine(tab2 + "protected override void OnModelCreating(ModelBuilder modelBuilder)");
            sb.AppendLine(tab2 + "{");
            foreach (var table in tables)
            {
                var UpperedTableName = DatabaseUtils.UpperFirstLetter(table.TableName);
                sb.AppendFormat("{0}modelBuilder.Entity<{1}>(entity =>{2}", tab3, UpperedTableName, LineEnding);
                sb.AppendLine(tab3 + "{");
                foreach (var index in table.Indexes)
                {
                    switch (index.IndexType)
                    {
                        case 0:
                            sb.AppendFormat("{0}entity.HasIndex(e => e.{1}){2}", tab4, index.PropertyName, LineEnding);
                            sb.AppendFormat("{0}.HasName(\"{1}\");{2}", tab5, index.IX_Name, LineEnding);
                            break;
                        case 1:
                            sb.AppendFormat("{0}entity.HasKey(e => e.{1}){2}", tab4, index.PropertyName, LineEnding);
                            sb.AppendFormat("{0}.HasName(\"{1}\");{2}", tab5, index.IX_Name, LineEnding);
                            if (table.TableName != UpperedTableName)
                            {
                                sb.AppendLine();
                                sb.AppendFormat("{0}entity.ToTable(\"{1}\");{2}", tab4, table.TableName, LineEnding);
                            }
                            break;
                        case 2:
                            sb.AppendFormat("{0}entity.HasIndex(e => e.{1}){2}", tab4, index.PropertyName, LineEnding);
                            sb.AppendFormat("{0}.HasName(\"{1}\"){2}", tab5, index.IX_Name, LineEnding);
                            sb.AppendLine(tab5 + ".IsUnique();");
                            break;
                    }
                    sb.AppendLine();
                }
                foreach (var defaultValue in table.DefaultValues)
                {
                    var value = defaultValue.Value.Substring(1, defaultValue.Value.Length - 2);
                    if (string.IsNullOrEmpty(value) == true)
                    {
                        value = "''";
                    }
                    sb.AppendFormat("{0}entity.Property(e => e.{1}).HasDefaultValueSql(\"{2}\");{3}", tab4, defaultValue.PropertyName, value, LineEnding);
                    sb.AppendLine();
                }
                foreach (var hasColumnType in table.HasColumnTypes)
                {
                    sb.AppendFormat("{0}entity.Property(e => e.{1}).HasColumnType(\"{2}\");{3}", tab4, hasColumnType.PropertyName, hasColumnType.TypeName, LineEnding);
                }
                foreach (var requiredMaxLength in table.RequiredMaxLengths)
                {
                    sb.AppendFormat("{0}entity.Property(e => e.{1})", tab4, requiredMaxLength.PropertyName);
                    if (requiredMaxLength.NeedIsRequired == true)
                    {
                        sb.AppendLine();
                        sb.Append(tab5 + ".IsRequired()");
                    }
                    if (requiredMaxLength.MaxLength > 0)
                    {
                        sb.AppendLine();
                        sb.AppendFormat("{0}.HasMaxLength({1})", tab5, requiredMaxLength.MaxLength);
                    }
                    sb.AppendLine(";");
                }
                sb.AppendLine();
                foreach (var foreignKey in table.ForeignKeys)
                {
                    sb.AppendFormat("{0}entity.HasOne(d => d.{1}Navigation){2}", tab4, foreignKey.PropertyName, LineEnding);
                    sb.AppendFormat("{0}.WithMany(p => p.{1}{2}Navigation){3}", tab5, UpperedTableName, foreignKey.PropertyName, LineEnding);
                    sb.AppendFormat("{0}.HasForeignKey(d => d.{1}){2}", tab5, foreignKey.PropertyName, LineEnding);
                    if (foreignKey.DeleteAction == 0)
                    {
                        sb.AppendLine(tab5 + ".OnDelete(DeleteBehavior.Restrict)");
                    }
                    else if (foreignKey.DeleteAction == 1)
                    {
                        sb.AppendLine(tab5 + ".OnDelete(DeleteBehavior.SetNull)");
                    }
                    else if (foreignKey.DeleteAction == 2)
                    {
                        sb.AppendLine(tab5 + ".OnDelete(DeleteBehavior.Cascade)");
                    }
                    sb.AppendFormat("{0}.HasConstraintName(\"{1}\");{2}", tab5, foreignKey.FK_Name, LineEnding);
                    sb.AppendLine();
                }
                sb.AppendLine(tab3 + "});");
            }
            sb.AppendLine(tab2 + "}");
            foreach (var table in tables)
            {
                sb.AppendFormat("{0}public DbSet<{1}> {1} {{ get; set; }}{2}",
                    tab2, DatabaseUtils.UpperFirstLetter(table.TableName), LineEnding);
            }
            sb.AppendLine(tab + "}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string GenEntityClass(string nameSpace, string entityClassName, IEnumerable<EntityProperty> properties, IEnumerable<ViewModel.Reference> references)
        {
            var sb = new StringBuilder();
            var tab = "    ";
            var tab2 = tab + tab;
            var tab3 = tab2 + tab;
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace " + nameSpace);
            sb.AppendLine("{");
            var interfaceName = "SwaIEntity";
            if (entityClassName == "SwaUser")
            {
                interfaceName = "SwaIUser";
            }
            else if (entityClassName == "SwaGroup")
            {
                interfaceName = "SwaIGroup";
            }
            else if (entityClassName == "SwaUserGroup")
            {
                interfaceName = "SwaIUserGroup";
            }
            sb.AppendFormat("{0}public partial class {1} : huypq.SwaMiddleware.{2}{3}", tab, entityClassName, interfaceName, LineEnding);
            sb.AppendLine(tab + "{");
            sb.AppendFormat("{0}public {1}(){2}", tab2, entityClassName, LineEnding);
            sb.AppendLine(tab2 + "{");
            foreach (var item in references)
            {
                sb.AppendFormat("{0}{1}Navigation = new HashSet<{2}>();{3}", tab3, DatabaseUtils.UpperFirstLetter(item.PropertyName), DatabaseUtils.UpperFirstLetter(item.ReferenceTableName), LineEnding);
            }
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            foreach (var item in properties)
            {
                sb.AppendFormat("{0}public {1} {2} {{ get; set; }}{3}", tab2, item.PropertyType, item.PropertyName, LineEnding);
            }
            sb.AppendLine();
            foreach (var item in references)
            {
                sb.AppendFormat("{0}public ICollection<{1}> {2}Navigation {{ get; set; }}{3}", tab2, DatabaseUtils.UpperFirstLetter(item.ReferenceTableName), DatabaseUtils.UpperFirstLetter(item.PropertyName), LineEnding);
            }
            sb.AppendLine();
            foreach (var item in properties.Where(p => p.IsForeignKey == true))
            {
                sb.AppendFormat("{0}public {1} {2}Navigation {{ get; set; }}{3}", tab2, DatabaseUtils.UpperFirstLetter(item.ForeignKeyTableName), item.PropertyName, LineEnding);
            }

            var pkName = properties.First(p => p.IsIdentity).PropertyName;
            if (pkName != "ID")
            {
                sb.AppendLine();
                sb.AppendLine(tab2 + "[System.ComponentModel.DataAnnotations.Schema.NotMapped]");
                sb.AppendFormat("{0}public int ID {{ get {{ return {1}; }} set {{ {1} = value;}} }}{2}", tab2, pkName, LineEnding);
            }

            sb.AppendLine(tab + "}");
            sb.AppendLine("}");
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
