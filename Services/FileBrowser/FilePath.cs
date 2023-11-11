using System.IO;
using System.Linq;

namespace Comicsbox.FileBrowser
{
    public class FilePath
    {
        public string RelativePath { get; private set; }

        public string AbsolutePath { get; private set; }

        private readonly string _lastPath;

        public FilePath(string rootPath, params string[] relativePaths)
        {
            RelativePath = Path.Combine(relativePaths);
            AbsolutePath = Path.Combine(rootPath, RelativePath);
            _lastPath = relativePaths.LastOrDefault();
        }

        public string FileName
        {
            get
            {
                return _lastPath != null && Path.HasExtension(_lastPath) ? _lastPath : string.Empty;
            }
        }
    }
}
