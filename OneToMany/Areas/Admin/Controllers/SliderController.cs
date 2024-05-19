using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneToMany.Data;
using OneToMany.Helpers.Extension;
using OneToMany.Models;
using OneToMany.ViewModels.Categories;
using OneToMany.ViewModels.Sliders;

namespace OneToMany.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SliderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;  

        public SliderController(AppDbContext context ,
                                IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Slider> sLiders = await _context.Sliders.ToListAsync();

            List<SliderVM> result = sLiders.Select(m => new SliderVM { Id = m.Id, Image = m.Image }).ToList();

            return View(result);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(SliderCreateVM request)
        {
            if (!ModelState.IsValid) return View();

            foreach (var item in request.Image)
            {
                if (!item.ChekFileType("image/"))
                {
                    ModelState.AddModelError("Image", "File must be only image format");
                    return View();
                };

                if (!item.CheckFileSize(200))
                {
                    ModelState.AddModelError("Image", "Image size must be max 200kb");
                    return View();
                }
            }

            foreach (var item in request.Image)
            {
                string fileName = Guid.NewGuid().ToString() + "-" + item.FileName;

                string path = Path.Combine(_env.WebRootPath, "img", fileName);

                await item.SaveFileToLocalAsync(path);

                await _context.Sliders.AddAsync(new Slider { Image = fileName });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");

        }



        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return BadRequest();
            Slider slider = await _context.Sliders.Where(m => m.Id == id).FirstOrDefaultAsync();
            if (slider is null) return NotFound();
            _context.Sliders.Remove(slider);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id is null) return BadRequest();

            Slider slider = await _context.Sliders.Where(m => m.Id == id)
                                                  .FirstOrDefaultAsync();

            if (slider is null) return NotFound();

            SliderDetailVM model = new()
            {
                Id = slider.Id,
                Image = slider.Image,
                SoftDeleted = slider.SoftDeleted
            };

            return View(model);



        }
    }
}