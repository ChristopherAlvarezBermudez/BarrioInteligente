using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using BarrioInteligenteWeb.Models;
using System.Collections.Generic;

namespace BarrioInteligenteWeb.Controllers
{
    public class ReportesController : Controller
    {
        // Leemos la conexión directamente del archivo de configuración
        private string connectionString = "Server=localhost;Port=3306;Database=BarrioInteligenteDb;User=root;Password=;";

        public IActionResult Index()
        {
            List<Reporte> lista = new List<Reporte>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Reportes";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Reporte
                            {
                                Id = reader.GetInt32("Id"),
                                Titulo = reader.GetString("Titulo"),
                                Descripcion = reader.GetString("Descripcion"),
                                Categoria = reader.GetString("Categoria"),
                                Ubicacion = reader.GetString("Ubicacion"),
                                Fecha = reader.GetDateTime("Fecha")
                            });
                        }
                    }
                }
            }
            return View(lista);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Reporte reporte)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Reportes (Titulo, Descripcion, Categoria, Ubicacion) VALUES (@Titulo, @Descripcion, @Categoria, @Ubicacion)";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Titulo", reporte.Titulo);
                    cmd.Parameters.AddWithValue("@Descripcion", reporte.Descripcion);
                    cmd.Parameters.AddWithValue("@Categoria", reporte.Categoria);
                    cmd.Parameters.AddWithValue("@Ubicacion", reporte.Ubicacion);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
    }
}