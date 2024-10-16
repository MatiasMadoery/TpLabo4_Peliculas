using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TP_Labo4.Models;

namespace TP_Labo4.Controllers
{
    public class PeliculasController : Controller
    {
        private readonly AppDbContext _context;

        private readonly IWebHostEnvironment _env;
        public PeliculasController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
       
        // GET: Peliculas
        public async Task<IActionResult> Index()
        {           
            return View(await _context.Peliculas.ToListAsync());
        }

        // GET: Peliculas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var pelicula = await _context.Peliculas
              .Include(p => p.Genero) //Incluir genero
              .Include(p => p.PeliculaActores!)  //Relación con actores
              .ThenInclude(pa => pa.Actor)
              .FirstOrDefaultAsync(m => m.Id == id);

            if (pelicula == null)
            {
                return NotFound();
            }

            return View(pelicula);
        }

        // GET: Peliculas/Create
        public IActionResult Create()
        {
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Descripcion");

            // Cargar los actores disponibles para asociarlos con la película
            ViewBag.Actores = _context.Actores.ToList();
            return View();
        }

        // POST: Peliculas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GeneroId,Titulo,FechaEstreno,Portada,Trailer,Resumen")] Pelicula pelicula, List<int> actoresIds)
        {
            if (ModelState.IsValid)
            {
                var archivos = HttpContext.Request.Form.Files;
                if (archivos != null && archivos.Count > 0)
                {
                    var archivoFoto = archivos[0];
                    if (archivoFoto.Length > 0)
                    {
                        var pathDestino = Path.Combine(_env.WebRootPath, "img\\images");

                        var archivoDestino = Guid.NewGuid().ToString().Replace("-", "");
                        var extension = Path.GetExtension(archivoFoto.FileName);
                        archivoDestino += extension;

                        using (var filestream = new FileStream(Path.Combine(pathDestino, archivoDestino), FileMode.Create))
                        {
                            archivoFoto.CopyTo(filestream);
                            pelicula.Portada = archivoDestino;
                        }

                    }
                }
                _context.Add(pelicula);
                await _context.SaveChangesAsync();


                // Asociar actores seleccionados a la película
                if (actoresIds != null)
                {
                    foreach (var actorId in actoresIds)
                    {
                        var peliculaActor = new PeliculaActores
                        {
                            PeliculaId = pelicula.Id,
                            ActorId = actorId
                        };
                        _context.PeliculaActores.Add(peliculaActor);
                        _context.SaveChanges();  // Guardar los cambios
                        return RedirectToAction("Index");
                    }
                }

            }
            ViewBag.Actores = _context.Actores.ToList();
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Descripcion", pelicula.GeneroId);
            return View(pelicula);
        }

        // GET: Peliculas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {           
            if (id == null)
            {
                return NotFound();
            }

            var pelicula = await _context.Peliculas
              .Include(p => p.PeliculaActores!)  //Relación con actores
              .ThenInclude(pa => pa.Actor)
              .FirstOrDefaultAsync(m => m.Id == id);

            if (pelicula == null)
            {
                return NotFound();
            }

            // Cargar los actores disponibles para asociarlos con la película
            ViewBag.Actores = _context.Actores.ToList();

            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Descripcion", pelicula.GeneroId);
            return View(pelicula);
        }

        // POST: Peliculas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GeneroId,Titulo,FechaEstreno,Portada,Trailer,Resumen")] Pelicula pelicula, List<int> actoresIds)
        {
            if (id != pelicula.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Si no se selecciona ningún archivo, mantener la portada existente
                var archivos = HttpContext.Request.Form.Files;
                if (archivos != null || archivos?.Count == 0)
                {
                    // Mantener la portada actual (no la sobrescribas)
                    pelicula.Portada = _context.Peliculas
                        .AsNoTracking()
                        .Where(p => p.Id == pelicula.Id)
                        .Select(p => p.Portada)
                        .FirstOrDefault();
                }
                else
                {
                    var archivoFoto = archivos?[0];
                    if (archivoFoto?.Length > 0)
                    {
                        var pathDestino = Path.Combine(_env.WebRootPath, "img\\images");

                        var archivoDestino = Guid.NewGuid().ToString().Replace("-", "");
                        var extension = Path.GetExtension(archivoFoto.FileName);
                        archivoDestino += extension;

                        using (var filestream = new FileStream(Path.Combine(pathDestino, archivoDestino), FileMode.Create))
                        {
                            archivoFoto.CopyTo(filestream);
                            if (pelicula.Portada != null)
                            {
                                var archivoViejo = Path.Combine(pathDestino, pelicula.Portada!);
                                if (System.IO.File.Exists(archivoViejo))
                                {
                                    System.IO.File.Delete(archivoViejo);
                                }
                            }
                            pelicula.Portada = archivoDestino;
                        }
                    }
                }

                //Guardar y validar actores en las peliculas
                if (actoresIds != null)
                {
                    // Crear una lista para almacenar las nuevas relaciones de actores
                    var nuevaRelaciones = new List<PeliculaActores>();

                    // Cargar la película existente para verificar relaciones
                    var peliculaExistente = _context.Peliculas
                        .Include(p => p.PeliculaActores)
                        .FirstOrDefault(p => p.Id == pelicula.Id);

                    // Obtener los IDs de los actores ya relacionados
                    var actoresYaRelacionados = peliculaExistente.PeliculaActores.Select(pa => pa.ActorId).ToList();

                    foreach (var actorId in actoresIds)
                    {
                        // Verificar si el actor ya está relacionado
                        if (!actoresYaRelacionados.Contains(actorId))
                        {
                            // Agregar la nueva relación si no está ya relacionada
                            var peliculaActor = new PeliculaActores
                            {
                                PeliculaId = pelicula.Id,
                                ActorId = actorId
                            };
                            nuevaRelaciones.Add(peliculaActor);
                        }
                        else
                        {
                            // Aquí puedes agregar un mensaje si lo deseas
                            ModelState.AddModelError("Actores", $"El actor con ID {actorId} ya está asociado con esta película.");
                        }
                    }

                    // Agregar todas las nuevas relaciones de actores a la base de datos
                    if (nuevaRelaciones.Any())
                    {
                        _context.PeliculaActores.AddRange(nuevaRelaciones);
                        _context.SaveChanges();  // Guardar los cambios
                    }

                    return RedirectToAction("Index");
                }

                //if (actoresIds != null)
                //{
                //    foreach (var actorId in actoresIds)
                //    {
                //        var peliculaActor = new PeliculaActores
                //        {
                //            PeliculaId = pelicula.Id,
                //            ActorId = actorId
                //        };
                //        _context.PeliculaActores.Add(peliculaActor);
                //        _context.SaveChanges();  // Guardar los cambios
                //        return RedirectToAction("Index");
                //    }
                //}                
            
                try
                {
                    _context.Update(pelicula);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PeliculaExists(pelicula.Id))
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
            ViewBag.Actores = _context.Actores.ToList();
            ViewData["GeneroId"] = new SelectList(_context.Generos, "Id", "Descripcion", pelicula.GeneroId);
            return View(pelicula);
        }

        // GET: Peliculas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pelicula = await _context.Peliculas
                .Include(p => p.Genero)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pelicula == null)
            {
                return NotFound();
            }

            return View(pelicula);
        }

        // POST: Peliculas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pelicula = await _context.Peliculas.FindAsync(id);
            if (pelicula != null)
            {
                _context.Peliculas.Remove(pelicula);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PeliculaExists(int id)
        {
            return _context.Peliculas.Any(e => e.Id == id);
        }
    }
}
