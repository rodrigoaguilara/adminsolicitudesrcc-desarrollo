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
    public class ReporteCapacidadEconomicaHistoricaController : Controller
    {
        // GET: ReporteCapacidadEconomicaHistorica
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }


        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonBuscarReporteCapacidadEconomicaHistorica()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {

                    var list = db.SP_MOP_REP_CAPACIDAD_ECONOMICA_HISTORICA().ToList();
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
        public ActionResult ReporteCapacidadEconomicaHistorica(string data)
        {
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            var file = map["file"];
            var stream = (MemoryStream)downloadExcelCapacidadEconomicaHistorica(data);


            string today = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString();
            //var filename = string.Format("{0}.{1}.csv", "Listado de Solicitudes", today);
            var filename = string.Format("{0} {1}.xlsx", file, today);
            stream.Position = 0;

            return File(stream.ToArray(), "application/octet-stream", filename);
        }


        public Object downloadExcelCapacidadEconomicaHistorica(string data)
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
                var lista = JsonConvert.DeserializeObject<List<ReporteCapEconomicaHistoricaExcel>>(json);
                int i = 2;
                var ws = wb.Worksheets.Add(name);
                ws.Cell("A1").Value = "RUT";
                ws.Cell("B1").Value = "Razón Social";
                ws.Cell("C1").Value = "Cap. Económica";
                ws.Cell("D1").Value = "Fecha Estado Financiero";
                ws.Cell("E1").Value = "Estado Proveedor";
                ws.Cell("F1").Value = "Fecha Renovación";
                ws.Cell("G1").Value = "Vigencia Estado Financiero";
                ws.Cell("H1").Value = "Tipo de Registro";
                ws.Range("A1:H1").Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.5);
                ws.Range("A1:H1").Style.Font.Bold = true;

                try
                {
                    foreach (var item in lista)
                    {
                        ws.Cell("A" + i).Value = item.RutProveedor;
                        ws.Cell("B" + i).Value = item.RazonSocial;
                        ws.Cell("C" + i).Value = item.CapacidadEconomica;
                        ws.Cell("D" + i).Value = item.FechaEstadoFinanciero;
                        ws.Cell("E" + i).Value = item.EstadoProveedor;
                        ws.Cell("F" + i).Value = item.FechaRenovacion;
                        ws.Cell("G" + i).Value = item.Vigencia;
                        ws.Cell("H" + i).Value = item.TipoRegistro;
                        i++;
                    }

                    //17 columnas
                    IXLRange range = ws.Range(ws.Cell(1, 1).Address, ws.Cell(i, 8).Address);
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