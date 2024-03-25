using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
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
                    Thread.Sleep(5000); // Atraso de 5 segundos
                }
            });

            // Iniciar o thread
            thread.Start();

            // Aguardar o término do thread (nunca acontecerá neste caso, pois é um loop infinito)
            thread.Join();
        }

        static string GetDefault(Process process)
        {
            string url = string.Empty;
            try
            {
                if (process == null)
                    return "";

                if (process.MainWindowHandle == IntPtr.Zero)
                    return "";

                AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
                if (element == null)
                    return "";


                AutomationElement edit = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                if (edit == null)
                {                 
                    var all = element.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));

                    foreach (AutomationElement tag in all)
                    {
                        tag.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern);
                        if (pattern != null)
                        {
                            url = ((ValuePattern)tag.GetCurrentPattern(ValuePattern.Pattern)).Current.Value;
                            break;
                        }
                    }
                 
                    if (string.IsNullOrEmpty(url))
                        return url;
                }
                else
                {
                    url = ((ValuePattern)edit.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
                }

                return url;
            }catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return url;
            }
            
        }

        static string GetFirefoxUrl(string browsed)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName(browsed);
                var url = string.Empty;

                foreach (Process firefox in processes.Where(x => !x.MainWindowHandle.Equals(IntPtr.Zero)))
                {
                    AutomationElement element = AutomationElement.FromHandle(firefox.MainWindowHandle);
                    if (element == null) return url;

                    AutomationElement customFirstElement = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom));
                    if (customFirstElement == null) return url;

                    AutomationElementCollection customSecundElement = customFirstElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom));

                    foreach (AutomationElement item in customSecundElement)
                    {
                        AutomationElement doc = (item.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom)))
                                                       .FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Document));

                        if (doc != null && !doc.Current.IsOffscreen)
                        {
                            url = ((ValuePattern)doc.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
                            return url;
                        }
                    }
                }
                return url;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        static string GetOperaUrl(string browsed)
        {
            try
            {
                string url = string.Empty;
                Process[] procsOpera = Process.GetProcessesByName(browsed);
                foreach (Process opera in procsOpera.Where(x => !x.MainWindowHandle.Equals(IntPtr.Zero)))
                {
                    AutomationElement elm = AutomationElement.FromHandle(opera.MainWindowHandle);
                    AutomationElement elmUrlBar = elm.FindFirst(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.NameProperty, "Address field"));

                    if (elmUrlBar == null) continue;

                    AutomationPattern pattern = elmUrlBar.GetSupportedPatterns().FirstOrDefault(wr => wr.ProgrammaticName == "ValuePatternIdentifiers.Pattern");

                    if (pattern == null) continue;

                    ValuePattern val = (ValuePattern)elmUrlBar.GetCurrentPattern(pattern);
                    url = val.Current.Value;
                    break;
                }

                return url;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

    }
}
