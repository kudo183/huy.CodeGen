using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace huy.CodeGen
{
    public static class CodeGenerator
    {
        public static  string GenDtoClass(string nameSpace, string interfaceName, string className, List<EntityProperty> properties)
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
    }
}
