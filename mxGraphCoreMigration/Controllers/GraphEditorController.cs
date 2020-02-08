using com.mxgraph;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Web;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mxGraphCoreMigration.Models;

namespace mxGraphCoreMigration.Controllers
{
    public class GraphEditorController : Controller
    {
        private readonly ILogger<GraphEditorController> _logger;

        public GraphEditorController(ILogger<GraphEditorController> logger)
        {
            _logger = logger;
        }

        //[HttpPost]
        public IActionResult Open()
        {
            Debug.WriteLine("enter /GraphEditorController/Open");
            return Ok();
        }

        [HttpPost]
        public IActionResult Export(ExportPngModel args)
        {
            Debug.WriteLine("enter /GraphEditorController/Export");
            string filename = args.Filename;
            string xml = HttpUtility.UrlDecode(args.Xml);
            string format = args.Format;

            filename = filename != null ? HttpUtility.UrlDecode(filename) : "export";
            if (!filename.ToLower().EndsWith("." + format))
            {
                filename += "." + format;
            }

            if (format == "svg")
            {
                // handled in "public IActionResult Save(ExportModel args)"
            }
            else if (format == "png")
            {
                string width = args.W;
                string height = args.H;
                string bg = args.Bg;
                if (xml != null && width != null && height != null && bg != null
                        && filename != null && format != null)
                {
                    Color? background = (bg != null && !bg.Equals(mxConstants.NONE)) ? ColorTranslator.FromHtml(bg) : (Color?)null;
                    Image image = mxUtils.CreateImage(int.Parse(width), int.Parse(height), background);
                    Graphics g = Graphics.FromImage(image);
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    mxSaxOutputHandler handler = new mxSaxOutputHandler(new mxGdiCanvas2D(g));
                    handler.Read(new XmlTextReader(new StringReader(xml)));

                    if (filename.Length == 0)
                    {
                        filename = "export." + format;
                    }

                    MemoryStream memStream = new MemoryStream();
                    image.Save(memStream, ImageFormat.Png);
                    return File(memStream.GetBuffer(), "image/png", filename);
                }
            }
            return NoContent();
        }

        [HttpPost]
        public IActionResult Save(ExportModel args)
        {
            Debug.WriteLine("enter /GraphEditorController/Save");

            string filename = args.Filename;
            filename = filename != null ? HttpUtility.UrlDecode(filename) : "export";
            string format = args.Format;

            if (format == null) // 默认保存为 xml
            {
                if (args.Xml == null || args.Xml.Length <= 0)
                    return NoContent();
                if (!filename.ToLower().EndsWith(".xml"))
                {
                    filename += ".xml";
                }
                string xml = HttpUtility.UrlDecode(args.Xml);
                using MemoryStream ms = new MemoryStream();
                using var sw = new StreamWriter(ms, new UnicodeEncoding());
                sw.Write(xml);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return File(ms.GetBuffer(), "text/plain", filename);
            }

            if (!filename.ToLower().EndsWith("." + format))
            {
                filename += "." + format;
            }

            if (format == "svg")
            {
                string xml = HttpUtility.UrlDecode(args.Xml);
                if (xml == null || xml.Length <= 0)
                    return NoContent();

                using MemoryStream ms = new MemoryStream();
                using var sw = new StreamWriter(ms, new UnicodeEncoding());
                sw.Write(xml);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                return File(ms.GetBuffer(), "image/svg+xml; charset=utf-8", filename);
            }

            return NoContent();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
