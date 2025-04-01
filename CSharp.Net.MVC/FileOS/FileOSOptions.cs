using System.IO;
using System.Reflection;

namespace CSharp.Net.Mvc.FileOS
{
    public class FileOSOptions
    {
        public string Title { get; set; } = "FileOS";
        public string RoutePrefix { get; set; } = "FileOS";

        public Func<Stream> IndexStream { get; set; } = () =>
            typeof(FileOSOptions).GetTypeInfo().Assembly.GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".FileOS.FileOSUI.index.html");

    }
}

