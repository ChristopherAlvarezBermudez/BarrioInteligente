using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BarrioInteligenteWeb.Data;
using BarrioInteligenteWeb.Models;
using BarrioInteligenteWeb.Services;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using BarrioInteligenteWeb.Hubs;

namespace BarrioInteligenteWeb.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ReportesController> _logger;
        private readonly IHubContext<ReportesHub> _hubContext;
        private readonly IProfanityService _profanityService;

        public ReportesController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            ILogger<ReportesController> logger,
            IHubContext<ReportesHub> hubContext,
            IProfanityService profanityService)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _hubContext = hubContext;
            _profanityService = profanityService;
        }

        private int UsuarioActualId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public IActionResult Index()
        {
            try
            {
                var lista = _context.Reportes
                    .AsNoTracking()
                    .OrderByDescending(r => r.Fecha)
                    .ToList();
                ViewBag.UsuarioId = UsuarioActualId;
                ViewBag.FotoPerfil = _context.Usuarios
                    .AsNoTracking()
                    .Where(u => u.Id == UsuarioActualId)
                    .Select(u => u.FotoPerfil)
                    .FirstOrDefault();
                return View(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el índice de reportes");
                return View(new List<Reporte>());
            }
        }

        public IActionResult MisReportes()
        {
            try
            {
                var lista = _context.Reportes
                    .AsNoTracking()
                    .Where(r => r.UsuarioId == UsuarioActualId)
                    .OrderByDescending(r => r.Fecha)
                    .ToList();
                ViewBag.FotoPerfil = _context.Usuarios
                    .AsNoTracking()
                    .Where(u => u.Id == UsuarioActualId)
                    .Select(u => u.FotoPerfil)
                    .FirstOrDefault();
                return View("MisReportes", lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar Mis Reportes para usuario {UsuarioId}", UsuarioActualId);
                return View("MisReportes", new List<Reporte>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var reportesCategorias = await _context.Reportes
                    .AsNoTracking()
                    .GroupBy(r => r.Categoria)
                    .Select(g => new { Categoria = g.Key, Count = g.Count() })
                    .ToListAsync();

                var reportesEstados = await _context.Reportes
                    .AsNoTracking()
                    .GroupBy(r => r.Estado)
                    .Select(g => new { Estado = g.Key, Count = g.Count() })
                    .ToListAsync();

                var topUsuarios = await _context.Usuarios
                    .AsNoTracking()
                    .OrderBy(u => u.Reputacion)
                    .ThenByDescending(u => u.PuntosReputacion)
                    .Take(3)
                    .ToListAsync();

                ViewBag.DataCategorias = JsonSerializer.Serialize(reportesCategorias);
                ViewBag.DataEstados = JsonSerializer.Serialize(reportesEstados);
                ViewBag.TopCiudadanos = topUsuarios;
                
                ViewBag.FotoPerfil = await _context.Usuarios
                    .AsNoTracking()
                    .Where(u => u.Id == UsuarioActualId)
                    .Select(u => u.FotoPerfil)
                    .FirstOrDefaultAsync();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el dashboard");
                return View();
            }
        }

        public IActionResult Comunidad(string filtro = "recientes", double? lat = null, double? lon = null, string tipo = "Reporte")
        {
            try
            {
                var query = _context.Reportes
                    .AsNoTracking()
                    .Include(r => r.Usuario)
                    .Where(r => r.TipoPost == tipo || (tipo == "Reporte" && string.IsNullOrEmpty(r.TipoPost)))
                    .AsEnumerable(); // Haversine

                List<Reporte> lista;

                if (filtro == "cercanos" && lat.HasValue && lon.HasValue)
                {
                    const double R = 6371; // km
                    double latR = lat.Value * Math.PI / 180;
                    double lonR = lon.Value * Math.PI / 180;

                    lista = query
                        .Where(r => r.Latitud != 0 || r.Longitud != 0)
                        .OrderBy(r =>
                        {
                            double dLat = (r.Latitud * Math.PI / 180) - latR;
                            double dLon = (r.Longitud * Math.PI / 180) - lonR;
                            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                                     + Math.Cos(latR) * Math.Cos(r.Latitud * Math.PI / 180)
                                     * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                            return 2 * R * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                        })
                        .ToList();
                }
                else
                {
                    lista = query
                        .OrderByDescending(r => r.Fecha)
                        .ToList();
                }

                ViewBag.FiltroActivo = filtro;
                ViewBag.TipoActual = tipo;
                ViewBag.FotoPerfil = _context.Usuarios
                    .AsNoTracking()
                    .Where(u => u.Id == UsuarioActualId)
                    .Select(u => u.FotoPerfil)
                    .FirstOrDefault();

                var usuarioLocal = _context.Usuarios.AsNoTracking().FirstOrDefault(u => u.Id == UsuarioActualId);
                ViewBag.EsAdmin = usuarioLocal != null && usuarioLocal.EsAdmin;

                return View(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar Comunidad. Filtro={Filtro}", filtro);
                return View(new List<Reporte>());
            }
        }

        public IActionResult Crear()
        {
            ViewBag.FotoPerfil = _context.Usuarios
                .Where(u => u.Id == UsuarioActualId)
                .Select(u => u.FotoPerfil)
                .FirstOrDefault();
            return View();
        }

        private static readonly (double lat, double lon)[] PolyTerritorioRD = new[]
        {
            (19.75, -71.74), (19.92, -71.65), (19.95, -71.00), (19.85, -70.65),
            (19.68, -69.95), (19.40, -69.80), (19.35, -69.25), (19.23, -69.10),
            (19.16, -69.25), (19.05, -69.45), (19.03, -68.90), (18.75, -68.40),
            (18.55, -68.30), (18.30, -68.40), (18.10, -68.65), (18.33, -68.85),
            (18.40, -69.00), (18.42, -69.35), (18.42, -69.70), (18.44, -70.00),
            (18.20, -70.25), (18.18, -70.55), (18.35, -70.65), (18.25, -71.05),
            (18.20, -71.08), (18.05, -71.10), (17.85, -71.25), (17.75, -71.40),
            (17.55, -71.52), (17.85, -71.70), (18.03, -71.75), (18.35, -71.72),
            (18.50, -71.86), (18.68, -71.75), (18.88, -71.71), (19.12, -71.71),
            (19.32, -71.70), (19.55, -71.72), (19.75, -71.74)
        };

        private static bool EsUbicacionValida(double lat, double lng)
        {
            if (lat < 17.50 || lat > 20.00 || lng < -71.86 || lng > -68.25)
                return false;

            bool inside = false;
            for (int i = 0, j = PolyTerritorioRD.Length - 1; i < PolyTerritorioRD.Length; j = i++)
            {
                var pi = PolyTerritorioRD[i];
                var pj = PolyTerritorioRD[j];
                bool intersect = ((pi.lon > lng) != (pj.lon > lng))
                    && (lat < (pj.lat - pi.lat) * (lng - pi.lon) / (pj.lon - pi.lon) + pi.lat);
                if (intersect) inside = !inside;
            }
            return inside;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Reporte reporte, IFormFile? imagen)
        {
            try
            {
                if (!EsUbicacionValida(reporte.Latitud, reporte.Longitud))
                {
                    ModelState.AddModelError(string.Empty, "La ubicación debe estar dentro del territorio terrestre de la República Dominicana (no en el mar ni en territorio extranjero).");
                    ViewBag.ErrorGuardado = "La ubicación seleccionada está en el mar o fuera de la República Dominicana.";
                    ViewBag.FotoPerfil = _context.Usuarios
                        .Where(u => u.Id == UsuarioActualId)
                        .Select(u => u.FotoPerfil)
                        .FirstOrDefault();
                    return View(reporte);
                }

                reporte.UsuarioId = UsuarioActualId;
                reporte.Fecha = DateTime.Now;

                var direccionParaTitulo = !string.IsNullOrEmpty(reporte.DireccionFisica) 
                    ? reporte.DireccionFisica 
                    : "zona reportada";
                reporte.Titulo = $"{reporte.Categoria} en {direccionParaTitulo}";

                if (imagen != null && imagen.Length > 0)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsDir);

                    var ext = Path.GetExtension(imagen.FileName).ToLowerInvariant();
                    if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
                    {
                        _logger.LogWarning("Tipo de archivo no permitido: {Extension} por usuario {UsuarioId}", ext, UsuarioActualId);
                        ModelState.AddModelError("imagen", "Solo se permiten imágenes (.jpg, .png, .webp).");
                        return View(reporte);
                    }

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await imagen.CopyToAsync(stream);

                    reporte.ImagenUrl = $"/uploads/{fileName}";
                    _logger.LogInformation("Imagen guardada: {Path}", reporte.ImagenUrl);
                }

                ModelState.Remove("Usuario");
                if (ModelState.IsValid)
                {
                    // ── Filtro de profanidades en descripción ANTES de guardar ──
                    if (!string.IsNullOrWhiteSpace(reporte.Descripcion))
                    {
                        var filtroDesc = await _profanityService.ValidarYCensurarAsync(
                            reporte.Descripcion, UsuarioActualId);
                        reporte.Descripcion = filtroDesc.TextoCensurado;
                    }

                    _context.Reportes.Add(reporte);
                    await _context.SaveChangesAsync();

                    await _hubContext.Clients.All.SendAsync("RecibirNuevoReporte", reporte.Latitud, reporte.Longitud, reporte.Titulo, reporte.Categoria);

                    _logger.LogInformation("Reporte creado ID={Id} por usuario {UsuarioId}", reporte.Id, UsuarioActualId);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al crear reporte. UsuarioId={UsuarioId}, Titulo={Titulo}", UsuarioActualId, reporte.Titulo);
                ViewBag.ErrorGuardado = "Ocurrió un error al guardar el reporte. Intenta de nuevo.";
            }

            return View(reporte);
        }

        public IActionResult Detalles(int id)
        {
            try
            {
                var usuarioLocal = _context.Usuarios.AsNoTracking().FirstOrDefault(u => u.Id == UsuarioActualId);
                ViewBag.EsAdmin = usuarioLocal != null && usuarioLocal.EsAdmin;
                ViewBag.UsuarioId = UsuarioActualId;
                var reporte = _context.Reportes
                    .AsNoTracking()
                    .Include(r => r.Usuario)
                    .FirstOrDefault(r => r.Id == id);

                if (reporte == null)
                {
                    _logger.LogWarning("Reporte no encontrado: ID={Id}", id);
                    return NotFound();
                }

                var comentarios = _context.Comentarios
                    .AsNoTracking()
                    .Include(c => c.Usuario)
                    .Include(c => c.Likes) // Estado Like
                    .Where(c => c.ReporteId == id)
                    .OrderBy(c => c.Fecha)
                    .ToList();

                var yaVoto = _context.Validaciones
                    .AsNoTracking()
                    .Any(v => v.ReporteId == id && v.UsuarioId == UsuarioActualId);

                return View(new ReporteDetalleViewModel
                {
                    Reporte = reporte,
                    Comentarios = comentarios,
                    YaVoto = yaVoto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles del reporte ID={Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLikeComentario(int id)
        {
            try
            {
                var like = await _context.ComentariosLikes
                    .FirstOrDefaultAsync(l => l.ComentarioId == id && l.UsuarioId == UsuarioActualId);

                bool estaLikeado = false;

                if (like != null)
                {
                    // Quitar Like
                    _context.ComentariosLikes.Remove(like);
                }
                else
                {
                    // Dar Like
                    like = new ComentarioLike
                    {
                        ComentarioId = id,
                        UsuarioId = UsuarioActualId
                    };
                    _context.ComentariosLikes.Add(like);
                    estaLikeado = true;
                }

                await _context.SaveChangesAsync();

                var totalLikes = await _context.ComentariosLikes.CountAsync(l => l.ComentarioId == id);

                return Json(new { success = true, estaLikeado, totalLikes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el like del comentario ID={Id} por el usuario {UsuarioId}", id, UsuarioActualId);
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarReporte(int id)
        {
            try
            {
                var reporte = await _context.Reportes.FirstOrDefaultAsync(r => r.Id == id);
                if (reporte == null) return Json(new { success = false, message = "Reporte no encontrado." });

                var validacion = await _context.Validaciones
                    .FirstOrDefaultAsync(v => v.ReporteId == id && v.UsuarioId == UsuarioActualId);

                bool estaConfirmado = false;

                if (validacion != null)
                {
                    _context.Validaciones.Remove(validacion);
                    reporte.Upvotes = Math.Max(0, reporte.Upvotes - 1);
                }
                else
                {
                    _context.Validaciones.Add(new Validacion
                    {
                        ReporteId = id,
                        UsuarioId = UsuarioActualId,
                        FechaVoto = DateTime.Now
                    });
                    reporte.Upvotes++;
                    estaConfirmado = true;
                }

                await _context.SaveChangesAsync();
                
                return Json(new { success = true, estaConfirmado = estaConfirmado, nuevoConteo = reporte.Upvotes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar reporte. ReporteId={Id}, UsuarioId={UsuarioId}", id, UsuarioActualId);
                return Json(new { success = false, message = "Error interno del servidor." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarComentario(int reporteId, string texto)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    // ── Filtro de profanidades ANTES de persistir ──
                    var filtro = await _profanityService.ValidarYCensurarAsync(
                        texto.Trim(), UsuarioActualId);
                    var textoFinal = filtro.TextoCensurado;

                    _context.Comentarios.Add(new Comentario
                    {
                        ReporteId = reporteId,
                        UsuarioId = UsuarioActualId,
                        Texto = textoFinal,
                        Fecha = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar comentario. ReporteId={Id}, UsuarioId={UsuarioId}", reporteId, UsuarioActualId);
            }

            return RedirectToAction(nameof(Detalles), new { id = reporteId });
        }

        /// <summary>
        /// Endpoint JSON que retorna objetos enriquecidos de incidencias
        /// filtradas por categoría y estado, para el mapa de calor interactivo.
        /// Parámetros: ?categoria=Basura&amp;estadoFiltro=Activos
        /// estadoFiltro: "Activos" (Pendiente + En Proceso) | "Resueltos"
        /// Formato: [{ lat, lng, intensidad, id, titulo }, ...]
        /// </summary>
        [HttpGet]
        public IActionResult ObtenerCoordenadasCalor(string? categoria, string estadoFiltro = "Activos")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoria))
                    return Json(Array.Empty<object>());

                var query = _context.Reportes
                    .Where(r => r.Categoria == categoria && (r.Latitud != 0 || r.Longitud != 0));

                // Filtrar por estado
                if (estadoFiltro == "Activos")
                    query = query.Where(r => r.Estado == "Pendiente" || r.Estado == "En Proceso");
                else if (estadoFiltro == "Resueltos")
                    query = query.Where(r => r.Estado == "Resuelto");

                var datos = query
                    .Select(r => new
                    {
                        lat = r.Latitud,
                        lng = r.Longitud,
                        intensidad = 1.0, // Peso uniforme estilo Snap Map: la intensidad la define la densidad geográfica
                        id = r.Id,
                        titulo = r.Titulo,
                        direccion = r.DireccionFisica,
                        categoria = r.Categoria,
                        estado = r.Estado,
                        foto = string.IsNullOrWhiteSpace(r.ImagenUrl) ? "" : r.ImagenUrl
                    })
                    .ToList();

                return Json(datos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener coordenadas para el mapa de calor. Categoría={Categoria}, Estado={Estado}", categoria, estadoFiltro);
                return Json(Array.Empty<object>());
            }
        }
    }
}