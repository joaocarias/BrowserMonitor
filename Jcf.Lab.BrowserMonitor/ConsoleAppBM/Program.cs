using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Automation;

namespace ConsoleAppBM
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Iniciar a função de busca das URLs das abas do Firefox em um novo thread
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    GetFirefoxTabURLs("firefox");
                    Thread.Sleep(5000); // Atraso de 5 segundos
                }
            });

            // Iniciar o thread
            thread.Start();

            // Aguardar o término do thread (nunca acontecerá neste caso, pois é um loop infinito)
            thread.Join();
        }

        static void GetFirefoxTabURLs(string processName)
        {
            var firefoxProcesses = Process.GetProcessesByName(processName);

            if (firefoxProcesses.Length == 0)
            {
                Console.WriteLine("Firefox não está em execução.");
                return;
            }

            var firefoxProcess = firefoxProcesses.FirstOrDefault(x => !x.MainWindowTitle.Equals(""));

            if (firefoxProcess.MainWindowHandle == IntPtr.Zero)
            {
                Console.WriteLine("IntPtr.Zero");
                return;
            }

            AutomationElement firefoxWindow = AutomationElement.FromHandle(firefoxProcess.MainWindowHandle);

            if (firefoxWindow == null)
            {
                Console.WriteLine("Não foi possível encontrar a janela do Firefox.");
                return;
            }

            var tabStrip = firefoxWindow.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.ClassNameProperty, "TabWindowClass"));

            if (tabStrip == null)
            {
                Console.WriteLine("Não foi possível encontrar a barra de guias (abas).");
                return;
            }

            var tabItems = tabStrip.FindAll(TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));

            if (tabItems == null || tabItems.Count == 0)
            {
                Console.WriteLine("Não foi possível encontrar nenhuma guia (aba).");
                return;
            }

            foreach (AutomationElement tabItem in tabItems)
            {
                string pageTitle = tabItem.Current.Name;

                var urlElement = tabItem.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

                if (urlElement != null)
                {
                    string url = ((ValuePattern)urlElement.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;

                    Console.WriteLine("Título da página: " + pageTitle);
                    Console.WriteLine("URL: " + url);
                    Console.WriteLine();
                }
            }
        }
    }
}
