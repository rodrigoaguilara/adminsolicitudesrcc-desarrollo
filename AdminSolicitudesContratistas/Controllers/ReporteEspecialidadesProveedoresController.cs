using AdminSolicitudesContratistas.Models;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminSolicitudesContratistas.Controllers
{
    public class ReporteEspecialidadesProveedoresController : Controller
    {
        // GET: ReporteEspecialidadesProveedores
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }



        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonBuscarReporteEspecialidadesProveedores(string codTipoRegistro)
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    var list = db.SP_MOP_REP_ESPECIALIDADES_PROVEEDORES(codTipoRegistro).ToList();
                    var jsonResult = new JsonResult();
                    int cant = list.Count;

                    if (cant > 0)
                    {
                        jsonResult.Data = list;
                    }
                    else
                    {
                        jsonResult.Data = "EMPTY";
                    }

                    jsonResult.MaxJsonLength = Int32.MaxValue;
                    jsonResult.ContentType = "application/json";
                    return jsonResult;
                }
            }
            catch (Exception)
            {

                throw;
            }

        }




        [HttpPost]
        public ActionResult ReporteEspecialidadesProveedores(string data)
        {
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            var file = map["file"];
            var stream = (MemoryStream)downloadExcelEspecialidadesProveedores(data);


            string today = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString();
            //var filename = string.Format("{0}.{1}.csv", "Listado de Solicitudes", today);
            var filename = string.Format("{0} {1}.xlsx", file, today);
            stream.Position = 0;

            return File(stream.ToArray(), "application/octet-stream", filename);
        }


        public Object downloadExcelEspecialidadesProveedores(string data)
        {
            MemoryStream stream = new MemoryStream();
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            //var json = map.ContainsKey("base64") ? FuncionesGenerales.base64_to_str(map["base64"]) : "";
            var json = map.ContainsKey("stringDatos") ? map["stringDatos"] : "";
            var name = map["file"];
            //var allRegs = miscelaneos.ListadoRegiones();
            //var allServ = miscelaneos.ListadoServicios();

            using (XLWorkbook wb = new XLWorkbook())
            {
                var lista = JsonConvert.DeserializeObject<List<ReporteEspecialidadesProveedoresExcel>>(json);
                int i = 2;
                var ws = wb.Worksheets.Add(name);
                ws.Cell("A1").Value = "RUT";
                ws.Cell("B1").Value = "Razón Social";
                ws.Cell("C1").Value = "Tipo Registro";
                ws.Cell("D1").Value = "Cod Especialidad";
                ws.Cell("E1").Value = "Especialidad";
                ws.Cell("F1").Value = "Categoría";
                ws.Cell("G1").Value = "Capacidad Económica";
                ws.Cell("H1").Value = "Estado Proveedor";
                ws.Cell("I1").Value = "Fecha Ingreso";
                ws.Cell("J1").Value = "Fecha Renovación";
                ws.Cell("K1").Value = "Mes renovación";
                ws.Cell("L1").Value = "Fecha estado financiero";
                ws.Cell("M1").Value = "Región";
                //ws.Cell("Q1").Value = "Días desde Ingreso";
                ws.Range("A1:M1").Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.5);
                ws.Range("A1:M1").Style.Font.Bold = true;

                try
                {
                    foreach (var item in lista)
                    {
                        ws.Cell("A" + i).Value = item.Rut;
                        ws.Cell("B" + i).Value = item.RazonSocialProveedor;
                        ws.Cell("C" + i).Value = item.TipoRegistro;
                        ws.Cell("D" + i).Value = item.CodTipoEspecialidad;
                        ws.Cell("E" + i).Value = item.TipoEspecialidad;
                        ws.Cell("F" + i).Value = item.TipoCategoria;
                        ws.Cell("G" + i).Value = item.CapacidadEconomica;
                        ws.Cell("H" + i).Value = item.EstadoProveedor;
                        ws.Cell("I" + i).Value = item.FechaIngresoReg;
                        ws.Cell("J" + i).Value = item.FechaSuspension;
                        ws.Cell("K" + i).Value = item.NombreMesRenovacion;
                        ws.Cell("L" + i).Value = item.FechaEstadoFinanciero;
                        ws.Cell("M" + i).Value = item.Region;
                        i++;
                    }

                    //17 columnas
                    IXLRange range = ws.Range(ws.Cell(1, 1).Address, ws.Cell(i, 13).Address);
                    range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    wb.SaveAs(stream);
                }
                catch (Exception e)
                {
                    ws.Cell("A" + i).Value = "ERROR: Ocurrió un problema en la generacion del Excel";
                    ws.Cell("A" + i + 1).Value = e.Message;
                }
            }
            return (Object)stream;
        }






    }
}