using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdminSolicitudesContratistas.Models;
using AdminSolicitudesContratistas.Models.ViewModels;
using Newtonsoft.Json;


namespace AdminSolicitudesContratistas.Controllers
{
    public class PerfilesController : Controller
    {
        // GET: Perfiles
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }

        // Carga los datos para la tabla principal de perfiles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult JsonListarPerfiles()
        {
            using (var db = new dsSolicitudesMOP())
            {
                var perfiles = db.SP_MOP_ADM_LISTAR_PERFILES()
                    .Select(p => new PerfilInfoViewModel { CodPerfil = p.CodPerfil, DescPerfil = p.DescPerfil })
                    .ToList();
                return Json(perfiles);
            }
        }

        // Carga la lista de permisos (opciones de menú) para un perfil específico
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult JsonListarOpcionesPorPerfil(string codPerfil)
        {
            using (var db = new dsSolicitudesMOP())
            {
                var opciones = db.SP_MOP_ADM_LISTAR_OPCIONES_PERFIL(codPerfil)
                    .Select(o => new OpcionMenuPerfilViewModel
                    {
                        IdOpcionMenuPerfil = o.IdOpcionMenuPerfil,
                        OpcionMenuID = o.OpcionMenuID,
                        DescOpcion = o.DescOpcion,
                        DescMenu = o.DescMenu,
                        Asignado = o.Asignado == 1
                    }).ToList();
                return Json(opciones);
            }
        }

        // Guarda los cambios de los permisos para un perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult JsonGuardarPermisos(string codPerfil, string permisosJson)
        {
            try
            {
                // 1. Deserializamos el string JSON.
                var permisos = JsonConvert.DeserializeObject<List<OpcionMenuPerfilViewModel>>(permisosJson);

                // 2. Filtramos solo los permisos asignados y extraemos sus IDs.
                var idsAsignados = permisos
                    .Where(p => p.Asignado)
                    .Select(p => p.OpcionMenuID);

                // 3. Unimos los IDs en un solo string separado por comas.
                var opcionesMenuIdCsv = string.Join(",", idsAsignados);

                // 4. Llamamos al Stored Procedure pasando el string.
                using (var db = new dsSolicitudesMOP())
                {
                    // 1. La llamada al SP ahora devuelve un 'int?' directamente.
                    int? resultado = db.SP_MOP_ADM_GUARDAR_PERMISOS_PERFIL(codPerfil, opcionesMenuIdCsv).FirstOrDefault();

                    // 2. Comparamos directamente la variable 'resultado'.
                    if (resultado.HasValue && resultado.Value == 1)
                    {
                        return Json(new { success = true, message = "Permisos guardados correctamente." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Ocurrió un error al guardar los permisos en la base de datos." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error inesperado en el servidor: " + ex.Message.ToString() });
            }
        }


    }
}