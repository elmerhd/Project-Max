using System.Threading;
using System.Windows;

namespace Max
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static MaxEngine MaxEngine;
        private bool showUI = false;
        private static MaxUI MaxUI;
        public App()
        {
            MaxEngine = new MaxEngine();
            Thread thread = new Thread(new ThreadStart(MaxEngine.Load));
            thread.Start();
        }


        public static MaxEngine GetEngine()
        {
            return MaxEngine;
        }

        public static MaxUI GetUI()
        {
            return MaxUI;
        }

        void Application_Startup(object sender, StartupEventArgs e)
        {
            foreach (string args in e.Args)
            {

                string placeholder = args.Split(':')[0];
                string value = args.Split(':')[1];
                if (placeholder == "showUI")
                {
                    showUI = bool.Parse(value);
                }
            }
            if (showUI)
            {
                MaxUI = new MaxUI();
                MaxUI.Show();
            }
        }
    }
}
