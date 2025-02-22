﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using RochaBlogs.Data;
using RochaBlogs.Models;
using RochaBlogs.Services.Interfaces;
using RochaBlogs.ViewModels;
using X.PagedList;

namespace RochaBlogs.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IBlogEmailSender emailSender, ApplicationDbContext context, IImageService imageService, IConfiguration configuration)
        {
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _imageService = imageService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var defaultImage = await _imageService.EncodeImageAsync(_configuration["DefaultHomePageImage"]);
            var defaultContentType = _configuration["DefaultHomePageImage"].Split(".")[1];

            // Sharing with others one line at a time
            ViewData["HeaderImage"] = _imageService.DecodeImage(defaultImage, defaultContentType);
            ViewData["MainText"] = "";
            ViewData["SubText"] = "";

            var pageNumber = page ?? 1;
            var pageSize = 5;
            var blogs = _context.Blogs.Include(b => b.BlogUser).Where(b => b.Posts
            .Any(p => p.ReadyStatus == Enums.ReadyStatus.ProductionReady))
            .OrderByDescending(b => b.Created)
            .ToPagedListAsync(pageNumber, pageSize);

            return View(await blogs);
        }

        public async Task<IActionResult> About()
        {

            var defaultImage = await _imageService.EncodeImageAsync(_configuration["DefaultAboutPageImage"]);
            var defaultContentType = _configuration["DefaultAboutPageImage"].Split(".")[1];

            ViewData["HeaderImage"] = _imageService.DecodeImage(defaultImage, defaultContentType);
            ViewData["MainText"] = "About me";
            ViewData["SubText"] = "Get to know the developer";

            return View();
        }

        public async Task<IActionResult> Contact()
        {
            var defaultImage = await _imageService.EncodeImageAsync(_configuration["DefaultContactPageImage"]);
            var defaultContentType = _configuration["DefaultContactPageImage"].Split(".")[1];

            ViewData["HeaderImage"] = _imageService.DecodeImage(defaultImage, defaultContentType);
            ViewData["MainText"] = "Contact me";
            ViewData["SubText"] = "Share your ideas with me";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMe model)
        {
            model.Message = $"{model.Message} <hr/> Phone: {model.Phone}";
            await _emailSender.SendContactEmailAsync(model.Email, model.Name, model.Subject, model.Message);
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}