using ImageMagick;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {

        IHostingEnvironment _hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormFile image)
        {
            if (image == null) return View();
            if (image.Length < 0) return View();

            string[] allowedImageTypes = new string[] { "image/jpeg", "image/png" };
            if (!allowedImageTypes.Contains(image.ContentType.ToLower())) return View();

            var imagesPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");
            var webPFileName = Path.GetFileNameWithoutExtension(image.FileName) + ".webp";
            var normalImagePath = Path.Combine(imagesPath, image.FileName);
            var webPImagePath = Path.Combine(imagesPath, webPFileName);


            using (var fileStream = new FileStream(normalImagePath, FileMode.Create))
            {
                //Save original file to local path.
                image.CopyTo(fileStream);

                //Convert webp from local path, and save it same path with diffrent extension.
                SaveWebp(50, webPImagePath, normalImagePath);
            }

            Images viewModel = new Images();
            viewModel.NormalImage = "/images/" + image.FileName;
            viewModel.WebPImage = "/images/" + webPFileName;


            return View(viewModel);
        }
        private void SaveWebp(int quality, string webpImagePath, string orginalImagePath)
        {
            byte[] file;
            using (MagickImage magickImage = new MagickImage(orginalImagePath))
            {
                magickImage.Format = MagickFormat.WebP;
                magickImage.Quality = quality;
                magickImage.AutoOrient();
                //In some cases image can be rotated, we block to rotating with AutoOrient (optional).
                file = magickImage.ToByteArray();

            }
            System.IO.File.WriteAllBytes(webpImagePath, file);
        }

    }
}
