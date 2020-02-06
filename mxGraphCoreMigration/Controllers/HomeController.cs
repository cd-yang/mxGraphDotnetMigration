using com.mxgraph;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Web;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mxGraphCoreMigration.Models;

namespace mxGraphCoreMigration.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ExportSvg(ExportModel args)
        {
            string filename = args.Filename;
            string xml = args.Xml;
            string format = args.Format;

            if (filename != null)
            {
                filename = HttpUtility.UrlDecode(filename);
            }
            else
            {
                filename = "export";
            }

            if (xml != null && xml.Length > 0)
            {
                if (format == null)
                {
                    format = "xml";
                }

                if (!filename.ToLower().EndsWith("." + format))
                {
                    filename += "." + format;
                }
                return Content(xml, "image/svg+xml; charset=utf-8");

                //context.Response.ContentType = "application/xml";
                //context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + filename + "\"");
                //context.Response.StatusCode = 200; /* OK */

                //context.Response.Write(xml);
            }
            else
            {
                return null;
                //context.Response.StatusCode = 400; /* Bad Request */
            }
        }

        [HttpPost]
        public IActionResult ExportPng(ExportPngModel args)
        {
            string filename = args.Filename;
            string xml = args.Xml;

            string width = args.W;
            string height = args.H;
            string bg = args.Bg;
            string format = args.Format;

            if (filename != null)
            {
                filename = HttpUtility.UrlDecode(filename);
            }

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

                //context.Response.ContentType = "image/" + format;
                //context.Response.AddHeader("Content-Disposition",
                //        "attachment; filename=" + filename);

                MemoryStream memStream = new MemoryStream();
                image.Save(memStream, ImageFormat.Png);
                //memStream.WriteTo(context.Response.OutputStream);

                //context.Response.StatusCode = 200; /* OK */

                return File(memStream, "image/png");
            }
            else
            {
                return null;
                //context.Response.StatusCode = 400; /* Bad Request */
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
