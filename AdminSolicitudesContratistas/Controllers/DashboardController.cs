using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


using AdminSolicitudesContratistas.Models;

namespace AdminSolicitudesContratistas.Controllers
{
    public class DashboardController : Controller
    {
        [HttpPost]
        public JsonResult GetProveedoresVigentesPorTipo()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    // Llamada al SP a través de EF
                    var data = db.SP_MOP_DASH_PROVEEDORES_VIG_POR_TIPO().ToList();

                    if (data.Any())
                    {
                        return Json(new { success = true, data = data });
                    }
                    else
                    {
                        return Json(new { success = false, message = "EMPTY" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSolicitudesPorEstado()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var data = db.SP_MOP_DASH_SOLICITUDES_POR_ESTADO().ToList();
                    return Json(new { success = true, data = data });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetProveedoresPorRegion()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var data = db.SP_MOP_DASH_PROVEEDORES_POR_REGION().ToList();
                    return Json(new { success = true, data = data });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSolicitudesPorRegion()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var data = db.SP_MOP_DASH_SOLICITUDES_POR_REGION().ToList();
                    return Json(new { success = true, data = data });
                }
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }








    }
}