using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace huy.CodeGen.ViewModel
{
    public class GenAllVM
    {
        public DatabaseTreeVM DatabaseTreeVM { get; set; }

        public string ClientNamespace { get; set; }
        public string ServerNamespace { get; set; }
        public string DtoNamespace { get; set; }
        public string DefaultLanguage { get; set; }
        public string DbContextName { get; set; }
        public string ViewPath { get; set; }
        public string ViewModelPath { get; set; }
        public string TextPath { get; set; }
        public string ControllerPath { get; set; }
        public string DtoPath { get; set; }
        public string EntityPath { get; set; }
        public string ProjectPath { get; set; }

        public IEnumerable<CultureInfo> LanguageNameList { get; set; }

        public ObservableCollection<string> Messages { get; set; }

        public GenAllVM()
        {
            DatabaseTreeVM = new DatabaseTreeVM();
            LanguageNameList = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(p => p.DisplayName, System.StringComparer.OrdinalIgnoreCase);
            Messages = new ObservableCollection<string>();
        }
    }
}
