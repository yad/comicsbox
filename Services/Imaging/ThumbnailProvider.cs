using Comicsbox.FileBrowser;
using Microsoft.Extensions.FileProviders;

namespace Comicsbox.Imaging
{
    public class ThumbnailProvider
    {
        private readonly IFilePathFinder _pathFinder;

        public ThumbnailProvider(IFilePathFinder pathFinder)
        {
            _pathFinder = pathFinder;
        }

        public byte[] GetThumbnailContent(string name)
        {
            var fileInfo = GetThumbnail(name);
            return fileInfo.ToByteArray();
        }

        public IFileInfo GetThumbnail(string name)
        {
            string defaultFileContainerExtension = BookInfoService.DefaultFileContainerExtension;
            var filePath = name.Contains(defaultFileContainerExtension) ? _pathFinder.LocateFile(name) : _pathFinder.LocateFirstFile(defaultFileContainerExtension);
            string thumbnailFileName = string.Format("{0}.jpg", filePath.FileName);
            var thumbnailFile = _pathFinder.LocateFile(thumbnailFileName);
            return _pathFinder.GetThumbnailFileInfoForFile(thumbnailFile);
        }
    }
}
