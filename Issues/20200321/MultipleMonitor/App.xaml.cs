using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MultipleMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var allScreens = System.Windows.Forms.Screen.AllScreens;
            var info=allScreens.Aggregate(new StringBuilder(), (sb, screen) =>
                    sb.AppendLine(
                        $@"{screen.DeviceName}, IsPrimary: {(screen.Primary ? "True" : "False")}, Bounds: {screen.Bounds}, Working Area {screen.WorkingArea}")
                ,sb=> sb.ToString()
            );
            Console.WriteLine(@"{0}", info);
            var win = new MainWindow
            {
                Height = 500,
                Width = 500,
                Left = 0,
                Top = 0,
                SysInfo = {Text = info}
            };
            win.ShowDialog();

            Shutdown();
        }
    }
}
