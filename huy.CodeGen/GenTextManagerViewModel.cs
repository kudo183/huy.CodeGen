using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace huy.CodeGen
{
    public class GenTextManagerViewModel : INotifyPropertyChanged
    {
        public CultureInfo[] AllCultures;

        public class TextData
        {
            public string TextKey { get; set; }
            public string TextValue { get; set; }
        }

        private string _namespace;
        public string Namespace
        {
            get { return _namespace; }
            set
            {
                _namespace = value;
                OnPropertyChanged();
            }
        }

        private string _languageName;
        public string LanguageName
        {
            get { return _languageName; }
            set
            {
                _languageName = value;
                OnPropertyChanged();
                if (_languageName != null)
                {
                    LanguageDisplayName = AllCultures.First(p => string.Compare(p.Name, _languageName, true) == 0).DisplayName;
                }
            }
        }

        private string _defaultLanguageName;
        public string DefaultLanguageName
        {
            get { return _defaultLanguageName; }
            set
            {
                _defaultLanguageName = value;
                OnPropertyChanged();
                if (_defaultLanguageName != null)
                {
                    DefaultLanguageDisplayName = AllCultures.First(p => string.Compare(p.Name, _defaultLanguageName, true) == 0).DisplayName;
                }
            }
        }

        private string _defaultLanguageDisplayName;
        public string DefaultLanguageDisplayName
        {
            get { return _defaultLanguageDisplayName; }
            set
            {
                _defaultLanguageDisplayName = value;
                OnPropertyChanged();
            }
        }

        private string _languageDisplayName;
        public string LanguageDisplayName
        {
            get { return _languageDisplayName; }
            set
            {
                _languageDisplayName = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<string> _languageNameList;
        public ObservableCollection<string> LanguageNameList
        {
            get { return _languageNameList; }
            set
            {
                _languageNameList = value;
                OnPropertyChanged();
            }
        }

        private List<TextData> _textDataList;
        public List<TextData> TextDataList
        {
            get { return _textDataList; }
            set
            {
                _textDataList = value;
                OnPropertyChanged();
            }
        }
        
        private string _outputPath;
        public string OutputPath
        {
            get { return _outputPath; }
            set
            {
                _outputPath = value;
                OnPropertyChanged();
            }
        }

        private string _result;
        public string Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
