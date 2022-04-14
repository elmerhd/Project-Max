using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Max
{
    public class AppEngine
    {
        private MaxEngine MaxEngine;
        private string ApplicationsFile = "applications.aiml";

        public AppEngine(MaxEngine maxEngine)
        {
            this.MaxEngine = maxEngine;
            maxEngine.BrainEngine.Log($"Loading Engine: {nameof(AppEngine)}");
            GetAllApps();
        }
        public void GetAllApps()
        {
            // GUID taken from https://docs.microsoft.com/en-us/windows/win32/shell/knownfolderid
            var FOLDERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");

            ShellObject appsFolder = (ShellObject)KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);
            ShellObjectWatcher sow = new ShellObjectWatcher(appsFolder, false);
            sow.AllEvents += Sow_AllEvents;
            //sow.Start();

            List<Category> appsCategories = new List<Category>();
            foreach (var app in (IKnownFolder)appsFolder)
            {
                // The friendly app name
                string name = app.Name;
                // The ParsingName property is the AppUserModelID
                string appUserModelID = app.ParsingName; // or app.Properties.System.AppUserModel.ID

                AppResponse appResponse = new AppResponse(name, appUserModelID);
                appsCategories.Add(new Category($"open {name}".ToLower(), appResponse));
            }
            MaxBrain maxBrain = new MaxBrain(appsCategories.ToArray());
            var file = $"{MaxEngine.BrainFolder}/{ApplicationsFile}";
            maxBrain.save(file);
            this.MaxEngine.BrainEngine.Bot.loadAIML(file);
        }

        private void Sow_AllEvents(object sender, ShellObjectNotificationEventArgs e)
        {
            Console.WriteLine("Shell Watcher ***");
        }

        public void OpenApp(string id)
        {
            Process proc =  Process.Start("explorer.exe", @" shell:appsFolder\" + id);
        }
    }
}
