using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AdminSolicitudesContratistas.Models;
using ClosedXML.Excel;
using Newtonsoft.Json;

namespace AdminSolicitudesContratistas.Controllers
{
    public class ReporteContactosProveedorController : Controller
    {
        // GET: ReporteContactosProveedor
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }


        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonBuscarReporteContactos()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var list = db.SP_MOP_REP_CONTACTO_PROVEEDORES().ToList();
                    var jsonResult = new JsonResult();

                    if (list.Any())
                    {
                        jsonResult.Data = list;
                    }
                    else
                    {
                        jsonResult.Data = "EMPTY";
                    }

                    jsonResult.MaxJsonLength = Int32.MaxValue;
                    return jsonResult;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult ExportarExcel(string data)
        {
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            var file = map["file"];
            var stream = (MemoryStream)downloadExcelStream(data);

            string today = DateTime.Now.ToString("dd-MM-yyyy");
            var filename = string.Format("{0} {1}.xlsx", file, today);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
        }

        public Object downloadExcelStream(string data)
        {
            MemoryStream stream = new MemoryStream();
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            var json = map["stringDatos"];
            var name = map["file"];

            using (var wb = new XLWorkbook())
            {
                var lista = JsonConvert.DeserializeObject<List<ReporteContactosProveedores>>(json);
                var ws = wb.Worksheets.Add(name);

                // Definir cabeceras
                ws.Cell("A1").Value = "RUT";
                ws.Cell("B1").Value = "Razón Social";
                ws.Cell("C1").Value = "Estado Proveedor";
                ws.Cell("D1").Value = "Tipo Registro";
                ws.Cell("E1").Value = "Email";
                ws.Cell("F1").Value = "Teléfono Contacto";
                ws.Cell("G1").Value = "Celular Contacto";
                ws.Cell("H1").Value = "Dirección";
                ws.Cell("I1").Value = "Comuna";
                ws.Cell("J1").Value = "Ciudad";
                ws.Cell("K1").Value = "Región";

                // Estilo para la cabecera
                var headerRange = ws.Range("A1:K1");
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD"); // Un azul corporativo
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontColor = XLColor.White;

                // Insertar datos
                ws.Cell("A2").InsertData(lista);

                // Ajustar columnas al contenido
                ws.Columns().AdjustToContents();

                wb.SaveAs(stream);
            }
            return stream;
        }



    }
}