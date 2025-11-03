using elFinder.NetCore.Drivers.FileSystem;
using elFinder.NetCore;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Harmic.Areas.Admin.Controllers
{
    [Area("Admin")]

    [Route("Admin/el-finder-file-system/harmic")]
    public class FileSystemController : Controller
    {
        IWebHostEnvironment _env;
        public FileSystemController(IWebHostEnvironment env) => _env = env;

        [HttpGet("connector")]
        [HttpPost("connector")]
        public async Task<IActionResult> Connector()
        {
            var connector = GetConnector();
            var result = await connector.ProcessAsync(Request);
            if (result is JsonResult json) return Content(JsonSerializer.Serialize(json.Value), json.ContentType);
            return Json(result);
        }

        [HttpGet("thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, hash);
        }

        private Connector GetConnector()
        {
            string pathroot = "files";
            var driver = new FileSystemDriver();
            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            string rootDirectory = Path.Combine(_env.WebRootPath, pathroot);
            string url = $"/{pathroot}/";
            string urlthumb = $"{absoluteUrl}/Admin/el-finder-file-system/harmic/thumb/";

            var root = new RootVolume(rootDirectory, url, urlthumb)
            {
                IsReadOnly = false,
                IsLocked = false,
                Alias = "Files",
                ThumbnailSize = 100,
            };

            driver.AddRoot(root);

            return new Connector(driver) { MimeDetect = MimeDetectOption.Internal };
        }
    }
}
