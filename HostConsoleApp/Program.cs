using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WcfServiceLibrary;

namespace HostConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = Assembly.GetExecutingAssembly().FullName;
            Maximize();
            try
            {
                ServiceHost host = new ServiceHost(typeof(ObservableService));
                host.Open();
                Console.WriteLine($"Host has been started and running on the following endpoints:");
                foreach (var item in host.Description.Endpoints)
                {
                    Console.WriteLine(item.Address);
                }
                Console.WriteLine("Press any to exit...");
                Console.ReadLine();
                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                //TODO handle the exception
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        private static void Maximize()
        {
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
        }
    }
}
