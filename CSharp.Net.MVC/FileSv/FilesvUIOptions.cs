using System.IO;
using System.Reflection;

namespace CSharp.Net.Mvc.FileSv
{
    public class FilesvUIOptions
    {
        public string Title { get; set; } = "Filesv";
        public string RoutePrefix { get; set; } = "Filesv";

        public Func<Stream> IndexStream { get; set; } = () =>
            typeof(FilesvUIOptions).GetTypeInfo().Assembly.GetManifestResourceStream(Assembly.GetExecutingAssembly().GetName().Name + ".FileSv.FileSvUI.index.html");

    }
}

