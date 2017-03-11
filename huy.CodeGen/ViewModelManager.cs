using huy.CodeGen.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huy.CodeGen
{
    public sealed class ViewModelManager
    {
        private static readonly ViewModelManager _instance = new ViewModelManager();

        public static ViewModelManager Instance
        {
            get { return _instance; }
        }

        public void LoadSettings()
        {
            genAllVM = new ViewModel.GenAllVM();

            genAllVM.DatabaseTreeVM.DBName = Properties.Settings.Default.GA_DbName;
            genAllVM.ClientNamespace = Properties.Settings.Default.GA_ClientNamespace;
            genAllVM.ServerNamespace = Properties.Settings.Default.GA_ServerNamespace;
            genAllVM.DtoNamespace = Properties.Settings.Default.GA_DtoNamespace;
            genAllVM.DefaultLanguage = Properties.Settings.Default.GA_DefaultLanguage;
            genAllVM.DbContextName = Properties.Settings.Default.GA_DbContextName;
            genAllVM.ViewPath = Properties.Settings.Default.GA_ViewPath;
            genAllVM.ViewModelPath = Properties.Settings.Default.GA_ViewModelPath;
            genAllVM.TextPath = Properties.Settings.Default.GA_TextPath;
            genAllVM.ControllerPath = Properties.Settings.Default.GA_ControllerPath;
            genAllVM.DtoPath = Properties.Settings.Default.GA_DtoPath;
            genAllVM.EntityPath = Properties.Settings.Default.GA_EntityPath;
            genAllVM.ProjectPath = Properties.Settings.Default.GA_ProjectPath;
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.GA_DbName = genAllVM.DatabaseTreeVM.DBName;
            Properties.Settings.Default.GA_ClientNamespace = genAllVM.ClientNamespace;
            Properties.Settings.Default.GA_ServerNamespace = genAllVM.ServerNamespace;
            Properties.Settings.Default.GA_DtoNamespace = genAllVM.DtoNamespace;
            Properties.Settings.Default.GA_DefaultLanguage = genAllVM.DefaultLanguage;
            Properties.Settings.Default.GA_DbContextName = genAllVM.DbContextName;
            Properties.Settings.Default.GA_ViewPath = genAllVM.ViewPath;
            Properties.Settings.Default.GA_ViewModelPath = genAllVM.ViewModelPath;
            Properties.Settings.Default.GA_TextPath = genAllVM.TextPath;
            Properties.Settings.Default.GA_ControllerPath = genAllVM.ControllerPath;
            Properties.Settings.Default.GA_DtoPath = genAllVM.DtoPath;
            Properties.Settings.Default.GA_EntityPath = genAllVM.EntityPath;
            Properties.Settings.Default.GA_ProjectPath = genAllVM.ProjectPath;

            Properties.Settings.Default.Save();
        }

        private GenAllVM genAllVM;
        public GenAllVM GenAllVM { get { return genAllVM; } }
    }
}
