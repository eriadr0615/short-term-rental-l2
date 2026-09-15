using System.Security.Claims;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuario repositorio;
        private readonly IWebHostEnvironment environment;
        private readonly string[] extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

        public UsuariosController(
            IRepositorioUsuario repositorio,
            IWebHostEnvironment environment)
        {
            this.repositorio = repositorio;
            this.environment = environment;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginView { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginView login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }

            string correo = login.Correo.Trim().ToLowerInvariant();
            Usuario? usuario = repositorio.ObtenerPorCorreo(correo);
            if (usuario == null ||
                !usuario.Activo ||
                !ServicioHash.Verificar(login.Clave, usuario.ContraseniaHash))
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos");
                return View(login);
            }

            usuario.UltimaConexion = DateTime.Now;
            repositorio.ActualizarUltimaConexion(usuario.IdUsuario, usuario.UltimaConexion.Value);
            await CrearCookie(usuario, login.Recordarme);

            if (!string.IsNullOrWhiteSpace(login.ReturnUrl) && Url.IsLocalUrl(login.ReturnUrl))
            {
                return LocalRedirect(login.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult Restringido()
        {
            return View();
        }

        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Index()
        {
            return View(repositorio.ObtenerLista());
        }

        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Create()
        {
            return View(new UsuarioCrearView());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Create(UsuarioCrearView modelo)
        {
            if (!RolValido(modelo.RolUsuario))
            {
                ModelState.AddModelError(nameof(modelo.RolUsuario), "El rol seleccionado no es válido");
            }

            if (repositorio.ObtenerPorCorreo(modelo.CorreoUsuario) != null)
            {
                ModelState.AddModelError(nameof(modelo.CorreoUsuario), "Ya existe un usuario con ese correo");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new Usuario
            {
                NombreUsuario = modelo.NombreUsuario.Trim(),
                CorreoUsuario = modelo.CorreoUsuario.Trim().ToLowerInvariant(),
                ContraseniaHash = ServicioHash.Crear(modelo.Clave),
                RolUsuario = modelo.RolUsuario,
                Activo = true
            };

            repositorio.Alta(usuario);
            TempData["Mensaje"] = "Usuario creado correctamente";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Edit(int id)
        {
            Usuario? usuario = repositorio.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(new UsuarioEditarView
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuario = usuario.NombreUsuario,
                CorreoUsuario = usuario.CorreoUsuario,
                RolUsuario = usuario.RolUsuario,
                Activo = usuario.Activo
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Edit(int id, UsuarioEditarView modelo)
        {
            if (id != modelo.IdUsuario)
            {
                return BadRequest();
            }

            Usuario? usuario = repositorio.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            if (!RolValido(modelo.RolUsuario))
            {
                ModelState.AddModelError(nameof(modelo.RolUsuario), "El rol seleccionado no es válido");
            }

            Usuario? usuarioConCorreo = repositorio.ObtenerPorCorreo(modelo.CorreoUsuario);
            if (usuarioConCorreo != null && usuarioConCorreo.IdUsuario != id)
            {
                ModelState.AddModelError(nameof(modelo.CorreoUsuario), "Ya existe un usuario con ese correo");
            }

            int idUsuarioActual = ObtenerIdUsuarioActual();
            if (id == idUsuarioActual && (!modelo.Activo || modelo.RolUsuario != Usuario.RolAdministrador))
            {
                ModelState.AddModelError("", "No puede desactivar ni quitarse el rol de administrador a sí mismo");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            usuario.NombreUsuario = modelo.NombreUsuario.Trim();
            usuario.CorreoUsuario = modelo.CorreoUsuario.Trim().ToLowerInvariant();
            usuario.RolUsuario = modelo.RolUsuario;
            usuario.Activo = modelo.Activo;
            repositorio.Modificacion(usuario);

            if (!string.IsNullOrWhiteSpace(modelo.NuevaClave))
            {
                repositorio.ActualizarClave(usuario.IdUsuario, ServicioHash.Crear(modelo.NuevaClave));
            }

            TempData["Mensaje"] = "Usuario actualizado correctamente";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult Eliminar(int id)
        {
            Usuario? usuario = repositorio.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Usuario.RolAdministrador)]
        public IActionResult EliminarConfirmado(int id)
        {
            if (id == ObtenerIdUsuarioActual())
            {
                TempData["Error"] = "No puede desactivar su propio usuario";
                return RedirectToAction(nameof(Index));
            }

            repositorio.Baja(id);
            TempData["Mensaje"] = "Usuario desactivado correctamente";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Perfil()
        {
            Usuario? usuario = repositorio.ObtenerPorId(ObtenerIdUsuarioActual());
            return usuario == null ? NotFound() : View(usuario);
        }

        public IActionResult EditarPerfil()
        {
            Usuario? usuario = repositorio.ObtenerPorId(ObtenerIdUsuarioActual());
            if (usuario == null)
            {
                return NotFound();
            }

            return View(new UsuarioPerfilView
            {
                NombreUsuario = usuario.NombreUsuario,
                CorreoUsuario = usuario.CorreoUsuario
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(UsuarioPerfilView modelo)
        {
            int id = ObtenerIdUsuarioActual();
            Usuario? usuario = repositorio.ObtenerPorId(id);
            if (usuario == null)
            {
                return NotFound();
            }

            Usuario? usuarioConCorreo = repositorio.ObtenerPorCorreo(modelo.CorreoUsuario);
            if (usuarioConCorreo != null && usuarioConCorreo.IdUsuario != id)
            {
                ModelState.AddModelError(nameof(modelo.CorreoUsuario), "Ya existe un usuario con ese correo");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            usuario.NombreUsuario = modelo.NombreUsuario.Trim();
            usuario.CorreoUsuario = modelo.CorreoUsuario.Trim().ToLowerInvariant();
            repositorio.Modificacion(usuario);

            var autenticacion = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await CrearCookie(usuario, autenticacion.Properties?.IsPersistent == true);

            TempData["Mensaje"] = "Perfil actualizado correctamente";
            return RedirectToAction(nameof(Perfil));
        }

        public IActionResult CambiarClave()
        {
            return View(new CambiarClaveView());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarClave(CambiarClaveView modelo)
        {
            Usuario? usuario = repositorio.ObtenerPorId(ObtenerIdUsuarioActual());
            if (usuario == null)
            {
                return NotFound();
            }

            if (!ServicioHash.Verificar(modelo.ClaveActual, usuario.ContraseniaHash))
            {
                ModelState.AddModelError(nameof(modelo.ClaveActual), "La contraseña actual es incorrecta");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            repositorio.ActualizarClave(usuario.IdUsuario, ServicioHash.Crear(modelo.NuevaClave));
            TempData["Mensaje"] = "Contraseña actualizada correctamente";
            return RedirectToAction(nameof(Perfil));
        }

        public IActionResult CambiarAvatar()
        {
            Usuario? usuario = repositorio.ObtenerPorId(ObtenerIdUsuarioActual());
            if (usuario == null)
            {
                return NotFound();
            }

            return View(new AvatarView { AvatarActual = usuario.Avatar });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarAvatar(AvatarView modelo)
        {
            Usuario? usuario = repositorio.ObtenerPorId(ObtenerIdUsuarioActual());
            if (usuario == null)
            {
                return NotFound();
            }

            modelo.AvatarActual = usuario.Avatar;
            if (modelo.Archivo == null || modelo.Archivo.Length == 0)
            {
                ModelState.AddModelError(nameof(modelo.Archivo), "Debe seleccionar una imagen");
            }
            else
            {
                string extension = Path.GetExtension(modelo.Archivo.FileName).ToLowerInvariant();
                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError(nameof(modelo.Archivo), "El formato de imagen no está permitido");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            string extensionArchivo = Path.GetExtension(modelo.Archivo!.FileName).ToLowerInvariant();
            string carpeta = Path.Combine(environment.WebRootPath, "usuarios", usuario.IdUsuario.ToString());
            Directory.CreateDirectory(carpeta);
            string nombreArchivo = Guid.NewGuid() + extensionArchivo;
            string rutaFisica = Path.Combine(carpeta, nombreArchivo);

            await using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await modelo.Archivo.CopyToAsync(stream);
            }

            EliminarAvatarAnterior(usuario);
            string url = $"/usuarios/{usuario.IdUsuario}/{nombreArchivo}";
            repositorio.ActualizarAvatar(usuario.IdUsuario, url);

            TempData["Mensaje"] = "Avatar actualizado correctamente";
            return RedirectToAction(nameof(Perfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarAvatar()
        {
            Usuario? usuario = repositorio.ObtenerPorId(ObtenerIdUsuarioActual());
            if (usuario == null)
            {
                return NotFound();
            }

            EliminarAvatarAnterior(usuario);
            repositorio.ActualizarAvatar(usuario.IdUsuario, null);
            TempData["Mensaje"] = "Avatar eliminado correctamente";
            return RedirectToAction(nameof(Perfil));
        }

        private int ObtenerIdUsuarioActual()
        {
            string? valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(valor, out int id) ? id : 0;
        }

        private async Task CrearCookie(Usuario usuario, bool persistente)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.CorreoUsuario),
                new Claim(ClaimTypes.Role, usuario.RolUsuario),
                new Claim("FullName", usuario.NombreUsuario)
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var propiedades = new AuthenticationProperties
            {
                IsPersistent = persistente
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad),
                propiedades);
        }

        private static bool RolValido(string rol)
        {
            return rol == Usuario.RolAdministrador || rol == Usuario.RolEmpleado;
        }

        private void EliminarAvatarAnterior(Usuario usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.Avatar) ||
                !usuario.Avatar.StartsWith($"/usuarios/{usuario.IdUsuario}/"))
            {
                return;
            }

            string rutaRelativa = usuario.Avatar.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string rutaFisica = Path.Combine(environment.WebRootPath, rutaRelativa);
            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }
        }
    }
}
