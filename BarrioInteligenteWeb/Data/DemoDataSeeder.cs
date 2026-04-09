using System.Security.Cryptography;
using System.Text;
using BarrioInteligenteWeb.Models;

namespace BarrioInteligenteWeb.Data
{
    public static class DemoDataSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (context.Usuarios.Any()) return; // Ya tiene datos

            // ── Usuarios ──
            var usuarios = new List<Usuario>
            {
                new Usuario
                {
                    Id = 1,
                    NombreCompleto = "Steven G. Rodríguez",
                    Correo = "steven.rodriguez@unapec.edu.do",
                    Password = HashPassword("demo1234"),
                    FechaRegistro = new DateTime(2026, 1, 15),
                    EmailConfirmado = true,
                    PuntosReputacion = 450,
                    Reputacion = NivelReputacion.Excelente,
                    MotivoReputacion = "Gracias por respetar las normas comunitarias y contribuir al bienestar del barrio."
                },
                new Usuario
                {
                    Id = 2,
                    NombreCompleto = "Brian Almonte",
                    Correo = "brian.almonte@unapec.edu.do",
                    Password = HashPassword("demo1234"),
                    FechaRegistro = new DateTime(2026, 1, 20),
                    EmailConfirmado = true,
                    PuntosReputacion = 380,
                    Reputacion = NivelReputacion.Excelente,
                    MotivoReputacion = "Participante activo en la comunidad."
                },
                new Usuario
                {
                    Id = 3,
                    NombreCompleto = "Kiara Medina",
                    Correo = "kiara.medina@unapec.edu.do",
                    Password = HashPassword("demo1234"),
                    FechaRegistro = new DateTime(2026, 2, 1),
                    EmailConfirmado = true,
                    PuntosReputacion = 320,
                    Reputacion = NivelReputacion.Buena,
                    MotivoReputacion = "Buen historial de reportes verificados."
                },
                new Usuario
                {
                    Id = 4,
                    NombreCompleto = "Christopher Álvarez",
                    Correo = "christopher.alvarez@unapec.edu.do",
                    Password = HashPassword("demo1234"),
                    FechaRegistro = new DateTime(2026, 2, 10),
                    EmailConfirmado = true,
                    PuntosReputacion = 280,
                    Reputacion = NivelReputacion.Buena,
                    MotivoReputacion = "Contribuciones regulares."
                },
                new Usuario
                {
                    Id = 5,
                    NombreCompleto = "Elian De Jesús",
                    Correo = "elian.dejesus@unapec.edu.do",
                    Password = HashPassword("demo1234"),
                    FechaRegistro = new DateTime(2026, 2, 15),
                    EmailConfirmado = true,
                    PuntosReputacion = 210,
                    Reputacion = NivelReputacion.Regular,
                    MotivoReputacion = "Nuevo miembro de la comunidad."
                }
            };

            context.Usuarios.AddRange(usuarios);
            context.SaveChanges();

            // ── Reportes ──
            var reportes = new List<Reporte>
            {
                new Reporte
                {
                    Id = 1,
                    Titulo = "Bache en Av. Máximo Gómez",
                    TipoPost = "Reporte",
                    Descripcion = "Bache de gran tamaño en el carril derecho. Varios vehículos han sufrido daños en sus neumáticos al pasar por esta zona. Se necesita reparación urgente.",
                    Categoria = "Bache",
                    Ubicacion = "18.4735,-69.9230",
                    DireccionFisica = "Av. Máximo Gómez, Gazcue, Santo Domingo",
                    Latitud = 18.4735,
                    Longitud = -69.9230,
                    Fecha = DateTime.Now.AddHours(-2),
                    Estado = "Pendiente",
                    Upvotes = 8,
                    UsuarioId = 1
                },
                new Reporte
                {
                    Id = 2,
                    Titulo = "Basura en Calle Santiago",
                    TipoPost = "Reporte",
                    Descripcion = "Acumulación de basura desde hace 3 días en la esquina. Los camiones recolectores no han pasado por la zona. Genera mal olor y atrae plagas.",
                    Categoria = "Basura",
                    Ubicacion = "18.4780,-69.9180",
                    DireccionFisica = "Calle Santiago, Zona Universitaria, Santo Domingo",
                    Latitud = 18.4780,
                    Longitud = -69.9180,
                    Fecha = DateTime.Now.AddHours(-5),
                    Estado = "En Proceso",
                    Upvotes = 12,
                    UsuarioId = 2
                },
                new Reporte
                {
                    Id = 3,
                    Titulo = "Luminaria en Parque Independencia",
                    TipoPost = "Reporte",
                    Descripcion = "Farola apagada frente al parque. La zona queda completamente oscura durante la noche, representando un riesgo para los peatones.",
                    Categoria = "Luminaria",
                    Ubicacion = "18.4850,-69.9290",
                    DireccionFisica = "Parque Independencia, Centro Histórico",
                    Latitud = 18.4850,
                    Longitud = -69.9290,
                    Fecha = DateTime.Now.AddDays(-1),
                    Estado = "Pendiente",
                    Upvotes = 5,
                    UsuarioId = 3
                },
                new Reporte
                {
                    Id = 4,
                    Titulo = "Bache en Calle El Conde",
                    TipoPost = "Reporte",
                    Descripcion = "Bache reparado satisfactoriamente por el ayuntamiento después de múltiples reportes de la comunidad.",
                    Categoria = "Bache",
                    Ubicacion = "18.4690,-69.9350",
                    DireccionFisica = "Calle El Conde, Zona Colonial",
                    Latitud = 18.4690,
                    Longitud = -69.9350,
                    Fecha = DateTime.Now.AddDays(-3),
                    Estado = "Resuelto",
                    Upvotes = 15,
                    UsuarioId = 4
                },
                new Reporte
                {
                    Id = 5,
                    Titulo = "Basura en Av. Abraham Lincoln",
                    TipoPost = "Reporte",
                    Descripcion = "Contenedor completamente lleno y desbordado. Residuos caen al suelo y generan mal olor en toda la cuadra.",
                    Categoria = "Basura",
                    Ubicacion = "18.4920,-69.9150",
                    DireccionFisica = "Av. Abraham Lincoln, Piantini",
                    Latitud = 18.4920,
                    Longitud = -69.9150,
                    Fecha = DateTime.Now.AddHours(-8),
                    Estado = "Pendiente",
                    Upvotes = 7,
                    UsuarioId = 5
                },
                new Reporte
                {
                    Id = 6,
                    Titulo = "Luminaria en Av. George Washington",
                    TipoPost = "Reporte",
                    Descripcion = "Poste de luz inclinado peligrosamente después de la tormenta tropical. Riesgo alto de caída sobre vehículos estacionados.",
                    Categoria = "Luminaria",
                    Ubicacion = "18.4620,-69.9400",
                    DireccionFisica = "Av. George Washington, Malecón",
                    Latitud = 18.4620,
                    Longitud = -69.9400,
                    Fecha = DateTime.Now.AddDays(-2),
                    Estado = "En Proceso",
                    Upvotes = 20,
                    UsuarioId = 1
                },
                new Reporte
                {
                    Id = 7,
                    Titulo = "Bache en Av. 27 de Febrero",
                    TipoPost = "Reporte",
                    Descripcion = "Hundimiento considerable en la calle principal. El tránsito se ha vuelto lento y peligroso en la zona, especialmente durante las lluvias.",
                    Categoria = "Bache",
                    Ubicacion = "18.4760,-69.9100",
                    DireccionFisica = "Av. 27 de Febrero, Naco",
                    Latitud = 18.4760,
                    Longitud = -69.9100,
                    Fecha = DateTime.Now.AddHours(-12),
                    Estado = "Pendiente",
                    Upvotes = 3,
                    UsuarioId = 2
                },
                new Reporte
                {
                    Id = 8,
                    Titulo = "Basura en Parque Mirador Sur",
                    TipoPost = "Reporte",
                    Descripcion = "Se realizó jornada de limpieza comunitaria exitosa. El parque quedó completamente limpio gracias a voluntarios del barrio.",
                    Categoria = "Basura",
                    Ubicacion = "18.4500,-69.9250",
                    DireccionFisica = "Parque Mirador Sur, Santo Domingo",
                    Latitud = 18.4500,
                    Longitud = -69.9250,
                    Fecha = DateTime.Now.AddDays(-5),
                    Estado = "Resuelto",
                    Upvotes = 25,
                    UsuarioId = 3
                }
            };

            context.Reportes.AddRange(reportes);
            context.SaveChanges();

            // ── Comentarios ──
            var comentarios = new List<Comentario>
            {
                new Comentario { Id = 1, Texto = "Yo pasé por ahí ayer y casi daño mi carro. ¡Esto necesita atención urgente!", ReporteId = 1, UsuarioId = 2, Fecha = DateTime.Now.AddHours(-1) },
                new Comentario { Id = 2, Texto = "Confirmo, la situación está terrible. Ya van 3 días así.", ReporteId = 2, UsuarioId = 3, Fecha = DateTime.Now.AddHours(-4) },
                new Comentario { Id = 3, Texto = "¡Excelente! Gracias al ayuntamiento por responder rápido.", ReporteId = 4, UsuarioId = 1, Fecha = DateTime.Now.AddDays(-2) },
                new Comentario { Id = 4, Texto = "Cuidado al pasar por esa zona de noche, no se ve nada.", ReporteId = 3, UsuarioId = 4, Fecha = DateTime.Now.AddHours(-20) },
                new Comentario { Id = 5, Texto = "Ya contacté al ayuntamiento y dijeron que esta semana lo reparan.", ReporteId = 6, UsuarioId = 5, Fecha = DateTime.Now.AddDays(-1) }
            };

            context.Comentarios.AddRange(comentarios);
            context.SaveChanges();

            // ── Validaciones (Upvotes) ──
            var validaciones = new List<Validacion>
            {
                new Validacion { Id = 1, ReporteId = 1, UsuarioId = 2, FechaVoto = DateTime.Now.AddHours(-1) },
                new Validacion { Id = 2, ReporteId = 1, UsuarioId = 3, FechaVoto = DateTime.Now.AddHours(-1) },
                new Validacion { Id = 3, ReporteId = 2, UsuarioId = 1, FechaVoto = DateTime.Now.AddHours(-4) },
                new Validacion { Id = 4, ReporteId = 2, UsuarioId = 4, FechaVoto = DateTime.Now.AddHours(-3) },
                new Validacion { Id = 5, ReporteId = 6, UsuarioId = 2, FechaVoto = DateTime.Now.AddDays(-1) },
                new Validacion { Id = 6, ReporteId = 6, UsuarioId = 3, FechaVoto = DateTime.Now.AddDays(-1) },
                new Validacion { Id = 7, ReporteId = 4, UsuarioId = 5, FechaVoto = DateTime.Now.AddDays(-2) },
                new Validacion { Id = 8, ReporteId = 8, UsuarioId = 1, FechaVoto = DateTime.Now.AddDays(-4) }
            };

            context.Validaciones.AddRange(validaciones);
            context.SaveChanges();
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + "BarrioInteligenteSalt2026");
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }
    }
}
