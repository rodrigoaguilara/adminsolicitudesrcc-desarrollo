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
    public class SolicitudesEliminadasController : Controller
    {
        // GET: SolicitudesEliminadas
        [CheckAuthorization]
        public ActionResult Index()
        {
            return View();
        }


        [ValidateAntiForgeryToken]
        [CheckAuthorization]
        [HttpPost]
        public JsonResult JsonBuscarSolicitudesEliminadas()
        {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {

                    var list = db.SP_MOP_LISTAR_SOLICITUDES_ELIMINADAS().ToList();
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
        public ActionResult ReporteSolicitudesEliminadas(string data)
        {
            var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            var file = map["file"];
            var stream = (MemoryStream)downloadExcelStream(data);


            string today = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString();
            //var filename = string.Format("{0}.{1}.csv", "Listado de Solicitudes", today);
            var filename = string.Format("{0} {1}.xlsx", file, today);
            stream.Position = 0;

            return File(stream.ToArray(), "application/octet-stream", filename);
        }


        public Object downloadExcelStream(string data)
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
                var lista = JsonConvert.DeserializeObject<List<SolicitudesEliminadasExcel>>(json);
                int i = 2;
                var ws = wb.Worksheets.Add(name);
                ws.Cell("A1").Value = "N° Solicitud";
                ws.Cell("B1").Value = "Solicitud padre";
                ws.Cell("C1").Value = "RUT";
                ws.Cell("D1").Value = "Razón Social";
                ws.Cell("E1").Value = "Estado";
                ws.Cell("F1").Value = "Tipo de Solicitud";
                ws.Cell("G1").Value = "Tipo de Registro";
                ws.Cell("H1").Value = "Región";
                ws.Cell("I1").Value = "Fecha Creación";
                ws.Cell("J1").Value = "Fecha Eliminación";
                ws.Cell("K1").Value = "Funcionario Responsable";
                ws.Cell("L1").Value = "Motivo Eliminación";
                //ws.Cell("Q1").Value = "Días desde Ingreso";
                ws.Range("A1:L1").Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.5);
                ws.Range("A1:L1").Style.Font.Bold = true;

                try
                {
                    foreach (var item in lista)
                    {
                        ws.Cell("A" + i).Value = item.IdSol;
                        ws.Cell("B" + i).Value = item.IdSolPadre;
                        ws.Cell("C" + i).Value = item.RutProveedor;
                        ws.Cell("D" + i).Value = item.RazonSocial;
                        ws.Cell("E" + i).Value = item.Estado;
                        ws.Cell("F" + i).Value = item.TipoSol;
                        ws.Cell("G" + i).Value = item.TipoReg;
                        ws.Cell("H" + i).Value = item.Region;
                        ws.Cell("I" + i).Value = item.FechaCreacion;
                        ws.Cell("J" + i).Value = item.FechaEliminacion;
                        ws.Cell("K" + i).Value = item.FuncionarioResponsable;
                        ws.Cell("L" + i).Value = item.MotivoEliminacion;
                        i++;
                    }

                    //17 columnas
                    IXLRange range = ws.Range(ws.Cell(1, 1).Address, ws.Cell(i, 12).Address);
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