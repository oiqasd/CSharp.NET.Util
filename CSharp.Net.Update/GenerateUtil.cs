using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace CSharp.Net.Update
{
    class GenerateUtil
    {
        public static string GetXmlInfo(string startApp, string packageUrl)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.AppendLine("<apps>");
            sb.AppendLine($"\t<app id=\"{AES.Encrypt(Process.GetCurrentProcess().ProcessName)}\">");
            sb.AppendLine($"\t\t<start-app>{AES.Encrypt(startApp)}</start-app>");
            sb.AppendLine($"\t\t<version>{Assembly.GetEntryAssembly()?.GetName().Version?.ToString()}</version>");
            sb.AppendLine($"\t\t<package-url>{AES.Encrypt(packageUrl)}</package-url>");
            sb.AppendLine("\t\t<package-type>exe</package-type>");
            sb.AppendLine("\t\t<update-time></update-time>");
            sb.AppendLine("\t</app>");
            sb.AppendLine("</apps>");

            return sb.ToString();
        }
    }
}
