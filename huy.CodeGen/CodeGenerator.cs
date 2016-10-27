using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huy.CodeGen
{
    public static class CodeGenerator
    {
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
            sb.AppendLine(string.Format("{0}static readonly string DefaultLanguage = \"{1}\";", tab2, defaultLanguage));
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
            sb.AppendLine(tab5 + "var texts = line.Split(new[] { \"\\t\\t\" }, System.StringSplitOptions.RemoveEmptyEntries);");
            sb.AppendLine(tab5 + "_dic.Add(texts[0], texts[1]);");
            sb.AppendLine(tab5 + "line = sr.ReadLine();");
            sb.AppendLine(tab4 + "}");
            sb.AppendLine(tab4 + "sr.Close();");
            sb.AppendLine(tab3 + "}");
            sb.AppendLine(tab2 + "}");
            sb.AppendLine();
            foreach (var item in textDataList)
            {
                sb.AppendLine(string.Format("{0}public static string {1} {{ get {{ return GetText(); }} }}", tab2, item.TextKey));
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
                sb.AppendLine(string.Format("{0}_dic.Add(\"{1}\", \"{2}\");", tab3, item.TextKey, item.TextValue));
            }
            sb.AppendLine(tab2 + "}");
            sb.AppendLine(tab + "}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string GenProperty(string tab, string propertyType, string propertyName)
        {
            return string.Format("{0}public {1} {2} {{ get {{ return _{2}; }} set {{ _{2} = value; OnPropertyChanged(); }} }}",
                    tab, propertyType, propertyName);
        }
    }
}
