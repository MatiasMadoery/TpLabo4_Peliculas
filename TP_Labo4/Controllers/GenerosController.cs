using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SpreadsheetLight;
using TP_Labo4.Models;

namespace TP_Labo4.Controllers
{
    public class GenerosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        public GenerosController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Generos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Generos.ToListAsync());
        }

        // GET: Generos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genero = await _context.Generos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (genero == null)
            {
                return NotFound();
            }

            return View(genero);
        }

        // GET: Generos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Generos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Descripcion")] Genero genero)
        {
            if (ModelState.IsValid)
            {
                _context.Add(genero);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(genero);
        }

        // GET: Generos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genero = await _context.Generos.FindAsync(id);
            if (genero == null)
            {
                return NotFound();
            }
            return View(genero);
        }

        // POST: Generos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Descripcion")] Genero genero)
        {
            if (id != genero.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(genero);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeneroExists(genero.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(genero);
        }

        // GET: Generos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genero = await _context.Generos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (genero == null)
            {
                return NotFound();
            }

            return View(genero);
        }

        // POST: Generos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genero = await _context.Generos.FindAsync(id);
            if (genero != null)
            {
                _context.Generos.Remove(genero);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GeneroExists(int id)
        {
            return _context.Generos.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ImportarExcel()
        {
            var archivos = HttpContext.Request.Form.Files;
            if (archivos != null && archivos.Count > 0)
            {
                var archivoExcel = archivos[0];
                if (archivoExcel.Length > 0)
                {
                    var rutaDestino = Path.Combine(_env.WebRootPath, "importaciones");
                    var extArch = Path.GetExtension(archivoExcel.FileName);
                    if (extArch == ".xlsx" || extArch == ".xls")
                    {
                        var archivoDestino = Guid.NewGuid().ToString().Replace("-", "") + extArch;
                        var rutaCompleta = Path.Combine(rutaDestino, archivoDestino);
                        using (var filestream = new FileStream(rutaCompleta, FileMode.Create))
                        {
                            archivoExcel.CopyTo(filestream);
                        }
                        SLDocument archXls = new SLDocument(rutaCompleta);
                        if (archXls != null)
                        {
                            List<Genero> ListaGeneros = new List<Genero>();
                            int fila = 1; 
                            while (!string.IsNullOrEmpty(archXls.GetCellValueAsString(fila, 1)))
                            {
                                try
                                {
                                    Genero genero = new Genero
                                    {
                                        Descripcion = archXls.GetCellValueAsString(fila, 1)
                                      
                                    };
                                    ListaGeneros.Add(genero);
                                }
                                catch (Exception ex)
                                {
                                    ViewData["Error"] = $"Error al procesar la fila {fila}: {ex.Message}";
                                    return View("Error");
                                }
                                fila++;
                            }
                            if (ListaGeneros.Count > 0)
                            {
                                _context.AddRange(ListaGeneros);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                ViewData["Error"] = "No se encontraron datos válidos en el archivo Excel.";
                            }
                        }
                    }
                }
            }
            return RedirectToAction("Index", "Generos");
        }


    }
}
