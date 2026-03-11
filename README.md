# **Barrio Inteligente**

## Descripcion
Plataforma de roportaje urbano colaborativa.
---
## Problema
Falta de atención y priorización de problemas urbanos cotidianos que afectan la calidad de vida.
---
## Objetivo
Desarrollar un ecosistema tecnológico que permita a los residentes de la República Dominicana reportar incidencias urbanas en tiempo real mediante un mapa colaborativo, facilitando a las autoridades y líderes comunitarios la visualización, análisis y resolución de estos problemas mediante un panel de control centralizado.
---
## Integrantes
* Steven Guiseppe Rodríguez-A00116539
* Brian Almonte Sosa-A00116791
* Kiara Evianny Medina Corporan-A00116054
* Christopher Alvarez Bermudez-A00116310
* Elian Emmanuel De Jesús Guzmán A00115958

---

## Tecnologias
### GIS
**Geolocalización en tiempo real** 
Geolocalización en tiempo real para registrar con precisión las coordenadas exactas de cada reporte y visualizar la distribución espacial de los problemas urbanos.


### App movil
**Reporte ciudadano**  
Interfaz intuitiva para que los usuarios capturen fotos de las incidencias las describan brevemente y reciban notificaciones sobre el estado de la solución.

### Dashboard Web
**Gestión y análisis con ASP.NET Core y MySQL**
Panel de control desarrollado en **ASP.NET Core MVC (.NET 8)**. Permite a las autoridades visualizar y gestionar los reportes.
* **Backend:** C# / .NET 8
* **Base de Datos:** MySQL (MariaDB/XAMPP)
* **Conector:** MySql.Data
* **Funcionalidad:** CRUD completo (Crear, Leer, Listar) de incidencias.

## Configuración del Proyecto

### 1. Base de Datos (Script SQL)
Para ejecutar este proyecto localmente, es necesario crear la base de datos en MySQL utilizando el siguiente script:

```sql
CREATE DATABASE BarrioInteligenteDb;
USE BarrioInteligenteDb;

CREATE TABLE Reportes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Titulo VARCHAR(100) NOT NULL,
    Descripcion TEXT,
    Categoria VARCHAR(50) NOT NULL,
    Ubicacion VARCHAR(200) NOT NULL,
    Fecha DATETIME DEFAULT CURRENT_TIMESTAMP
);