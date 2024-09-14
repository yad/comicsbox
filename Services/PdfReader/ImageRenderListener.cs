using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Xobject;

namespace Comicsbox
{
    public class ImageRenderListener : IEventListener
    {
        private byte[]? result = null;

        public byte[]? GetResult()
        {
            return result;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (data is ImageRenderInfo imageData)
            {
                try
                {
                    PdfImageXObject imageObject = imageData.GetImage();
                    if (imageObject == null)
                    {
                        Console.WriteLine("Image could not be read.");
                    }
                    else
                    {
                        this.result = imageObject.GetImageBytes();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Image could not be read: {0}.", ex.Message);
                }
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return null;
        }
    }
}