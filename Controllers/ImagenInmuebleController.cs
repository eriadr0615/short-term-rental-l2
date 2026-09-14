//imagen es creada sin imagen al comiezo
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
        public IActionResult Alta(int idInmueble, IFormFile imagen, bool esPrincipal = false)
        {
            if (imagen == null || imagen.Length == 0)
            {
                return BadRequest("No se registro niguna actualizacion sobre una imagen");
            }
            string extension = Path.GetExtension(imagen.FileName).ToLower();
            if (!extensionesPermitidas.Contains(extension))
            {
                return BadRequest("El formato de imagen no está permitido.");
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

            string nombreArchivo = Guid.NewGuid().ToString() + extension;
            string rutaFisica = Path.Combine(carpeta, nombreArchivo);
            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                imagen.CopyTo(stream);
            }
            string url = $"/inmuebles/{idInmueble}/{nombreArchivo}";
            ImagenInmueble nuevaImagen = new ImagenInmueble
            {
                IdInmueble = idInmueble,
                UrlImg = url,
                EsPrincipal = esPrincipal
            };

            repositorio.Alta(nuevaImagen);
            return RedirectToAction(
                    "Details",
                    "Inmueble",
                    new { id = idInmueble }
                );
            
        }

        [HttpGet]
        public IActionResult ObtenerPorInmueble(int idInmueble)
        {
            var imagenes = repositorio.ObtenerPorInmueble(idInmueble);
            return Ok(imagenes);
        }
    }
}