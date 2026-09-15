using Inmobiliaria.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    public class ImagenInmuebleController : Controller
    {
        private readonly IRepositorioImagenInmueble repositorio;
        private readonly IWebHostEnvironment environment;
        private readonly string[] extensionesPermitidas =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };
        public ImagenInmuebleController(
            IRepositorioImagenInmueble repositorio,
            IWebHostEnvironment environment)
        {
            this.repositorio = repositorio;
            this.environment = environment;
        }

        [HttpPost]
        public IActionResult Alta(int idInmueble, List<IFormFile> imagenes, bool esPrincipal = false)
        {
            if (imagenes == null || imagenes.Count == 0)
            {
                return BadRequest("No se cargo ninguna imagen. Reintenta nuevamente");
            }

            string carpeta = Path.Combine(
                environment.WebRootPath,
                "inmuebles",
                idInmueble.ToString()
            );

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }
            bool primeraImagen = true;

            foreach (var imagen in imagenes)
            {
                if (imagen == null || imagen.Length == 0)
                {
                    continue;
                }
                string extension =
                    Path.GetExtension(imagen.FileName).ToLower();

                if (!extensionesPermitidas.Contains(extension))
                {
                    continue;
                }
                string nombreArchivo =
                    Guid.NewGuid().ToString() + extension;
                string rutaFisica =
                    Path.Combine(carpeta, nombreArchivo);

                using (var stream =
                    new FileStream(rutaFisica, FileMode.Create))
                {
                    imagen.CopyTo(stream);
                }

                string url =
                    $"/inmuebles/{idInmueble}/{nombreArchivo}";

                bool principalEstaImagen =
                    esPrincipal && primeraImagen;
                if (principalEstaImagen)
                {
                    repositorio.QuitarPrincipal(idInmueble);
                }
                ImagenInmueble nuevaImagen =
                    new ImagenInmueble
                    {
                        IdInmueble = idInmueble,
                        UrlImg = url,
                        EsPrincipal = principalEstaImagen
                    };
                repositorio.Alta(nuevaImagen);
                primeraImagen = false;
            }

            return RedirectToAction(
                "Details",
                "Inmueble",
                new { id = idInmueble }
            );
        }

        [HttpGet]
        public IActionResult ObtenerPorInmueble(int idInmueble)
        {
            var imagenes =
                repositorio.ObtenerPorInmueble(idInmueble);

            return Ok(imagenes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var imagen = repositorio.ObtenerPorId(id);

            if (imagen == null)
            {
                return NotFound();
            }

            int idInmueble = imagen.IdInmueble;

            string rutaRelativa =
                imagen.UrlImg.TrimStart('/');

            string rutaFisica = Path.Combine(
                environment.WebRootPath,
                rutaRelativa.Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString())
            );

            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }

            repositorio.Baja(id);

            return RedirectToAction(
                "Details",
                "Inmueble",
                new { id = idInmueble }
            );
        }
    }
}