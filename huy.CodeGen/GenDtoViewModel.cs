using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huy.CodeGen
{
    public class GenDtoViewModel : INotifyPropertyChanged
    {
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string InterfaceName { get; set; }
        public string PropertyList { get; set; }
        public string DatabaseName { get; set; }
        public string OutputPath { get; set; }

        private string result;

        public string Result
        {
            get { return result; }
            set
            {
                result = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Result)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
