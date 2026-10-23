using AdminSolicitudesContratistas.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

using System.Web.Hosting;

namespace AdminSolicitudesContratistas.Models
{
    public class EnvioCorreosMOP
    {
        static string staticEmisorProveedores = ConfigurationManager.AppSettings.Get("NotificacionesContratistas_CorreoOrigenProveedores").ToString();
        static string staticEmisorMOP = ConfigurationManager.AppSettings.Get("NotificacionesContratistas_CorreoOrigenMOP").ToString();
        //static string staticEmisorErroresMOP = ConfigurationManager.AppSettings.Get("NotificacionesWebPrivado_CorreoErroresMOPOrigen").ToString();
        static string staticReceptorMOPTMP = ConfigurationManager.AppSettings.Get("NotificacionesContratistas_CorreoDestinoMOPTMP").ToString();
        static string staticReceptorProveedorTMP = ConfigurationManager.AppSettings.Get("NotificacionesContratistas_CorreoDestinoProveedorTMP").ToString();
        static string staticReceptorErroresMOP = ConfigurationManager.AppSettings.Get("NotificacionesContratistas_CorreoDestinoErroresMOP").ToString();
        static string staticAsunto = "";
        static string staticCuerpo = "";
        static string staticConCopia; // = "";//CORREO DE DESPALDO MOP
        static string staticURLCondicionesDeUsoMP = ConfigurationManager.AppSettings.Get("URLCondicionesDeUsoMP").ToString();
        static string staticBaseDatos = ConfigurationManager.AppSettings.Get("BaseDatosOrigen").ToString();


        //EnvioAnalisis
        public static string EnviarNotificacion_EnvioAnalisis(int idSolicitud, string rutProveedor)
        {
            ProveedorView prov = new ProveedorView();
            prov = SQLMethods.TraerDatosProveedor(rutProveedor);
            string descripcionTipoSolicitud = "";
            string descripcionTipoRegistro = "";
            string codTipoReg = "";
            string fechaCreacion = "";
            string estadoSolicitud = "";
            string fechaEnvioAnalisis = "";
            string respWS = "";
            int codRegion = 0;

            using (dsSolicitudesMOP db = new dsSolicitudesMOP())
            {
                var list = db.SP_MOP_TRAER_SOLICITUD_POR_ID(idSolicitud).ToList();
                foreach (var s in list)
                {
                    descripcionTipoSolicitud = s.TipoSol;
                    descripcionTipoRegistro = s.TipoReg;
                    codTipoReg = s.CodTipoReg;
                    fechaCreacion = s.FechaCreacion.ToString();
                    estadoSolicitud = s.Estado.ToString();
                    fechaEnvioAnalisis = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss tt");
                    codRegion = s.IdRegionSol;

                }

                //Obtiene todos los documentos enviados a anlisis
                var documentos = db.SP_MOP_DETALLE_SOLICITUD_POR_TIPO(idSolicitud, "ADJT", "").ToList();

                //Filtra los documentos por area
                var documentosFiltrados = documentos
                    .Where(d =>
                        d.CodAreaAnalisis == "CON" ||
                        d.CodAreaAnalisis == "LEG" ||
                        d.CodAreaAnalisis == "TEC")
                    .ToList();

                // Agrupa los documentos por area
                var documentosPorArea = documentosFiltrados
                    .GroupBy(d => d.CodAreaAnalisis)
                    .ToList();

                var destinatarios = db.Database
                .SqlQuery<DestinatarioCorreoArea>(
                    "EXEC SP_MOP_TRAER_NOMBRE_MAIL_POR_REG_TIPOREG @p0, @p1",
                    codRegion,
                    "CME"
                )
                .ToList();

                //envio el correo al analistas definidos por area
                //se agrega el correo de registro
                string CorreoRegistro = ConfigurationManager.AppSettings["CorreoRegistro"];
                // Asunto y Mensaje
                string swAsunto = ConfigurationManager.AppSettings["asuntoI-01"];
                string swCuerpo = ConfigurationManager.AppSettings["I-01"];

                // Destinatarios temporales por área
                string correoLEG = "rodrigo.aguilar@mop.gov.cl;";
                //string correoCON = "exequiel.gallardo.a@mop.gov.cl";
                string correoCON = "rodrigo.aguilar@mop.gov.cl";
                //string correoTEC = "miguel.acuna.r@mop.gov.cl";
                string correoTEC = "rodrigo.aguilar@mop.gov.cl";

                foreach (var area in documentosPorArea)
                {
                    string codigoArea = area.Key;
                    string nombreArea = area.First().AreaAnalisis;

                    //Seleccionamos el correo de envio
                    string destinatarioArea = "";

                    switch (codigoArea)
                    {
                        case "LEG":
                            destinatarioArea = correoLEG;
                            break;

                        case "CON":
                            destinatarioArea = correoCON;
                            break;

                        case "TEC":
                            destinatarioArea = correoTEC;
                            break;
                    }

                    // Todos los documentos de esta área en una sola lista
                    string listadoDocumentos = string.Join(
                        Environment.NewLine,
                        area.Select(doc =>
                            System.Text.RegularExpressions.Regex.Replace(
                                doc.NomDoc ?? "",
                                @"^\s*\d+\s*[\.\-]+\s*",
                                ""
                            )
                        )
                    );

                    // Generamos UN correo para esta área
                    string cuerpoHtml = PlantillaCorreoService.PlantillaCorreos(
                        idSolicitud.ToString(),
                        prov.RazonSocial,
                        rutProveedor,
                        descripcionTipoRegistro,
                        descripcionTipoSolicitud,
                        estadoSolicitud,
                        fechaEnvioAnalisis,
                        nombreArea,
                        listadoDocumentos,
                        swAsunto,
                        swCuerpo
                    );

                    // Aquí enviaremos UN SOLO correo al destinatario del área.
                    string asunto = swAsunto.Replace(
                                        "{{NroSolicitud}}",
                                        idSolicitud.ToString()
                                    );


                    try
                    {
                        // Copias locales de los datos del correo
                        string emisorCorreo = CorreoRegistro;
                        string destinatarioCorreo = destinatarioArea;
                        string asuntoCorreo = asunto;
                        string cuerpoCorreo = cuerpoHtml;

                        // Programar el envío sin esperar a la API
                        HostingEnvironment.QueueBackgroundWorkItem(async cancellationToken =>
                        {
                            try
                            {
                                var servicioCorreo = new CorreoApiService();

                                await servicioCorreo.EnviarCorreoAsync(
                                    emisorCorreo,
                                    destinatarioCorreo,
                                    asuntoCorreo,
                                    cuerpoCorreo,
                                    ""
                                );
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Trace.TraceError(
                                    "Error enviando correo I-01: " + ex.ToString()
                                );
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        // Una falla al programar el correo no detiene el proceso principal
                        System.Diagnostics.Trace.TraceError(
                            "Error programando correo I-01: " + ex.ToString()
                        );
                    }

                }

                return respWS="nop";
            }


            ////RESTAURAR DESPUES de aca
            //string saludo = "";
            //string saludoAM = "Buenos días,";
            //string saludoPM = "Buenas tardes,";
            //DateTime date = DateTime.Now;
            //string bloque = date.ToString("tt"); //TRAE SI ES AM o PM
            //switch (bloque)
            //{
            //    case "AM":
            //        saludo = saludoAM;
            //        break;
            //    case "PM":
            //        saludo = saludoPM;
            //        break;
            //}



            //staticAsunto = "Solicitud enviada a análisis N°: " + idSolicitud.ToString();
            //staticCuerpo = "<html>";
            //staticCuerpo += "<h2> SOLICITUD ENVIADA A ANÁLISIS N° " + idSolicitud.ToString() + "</h2>";
            //staticCuerpo += "<p>" + saludo + "</p>";
            //staticCuerpo += "<p> Se ha asignado a Análisis una nueva solicitud, con los siguientes datos</p>";
            //staticCuerpo += "<table>";
            //staticCuerpo += "<tr><td> Solicitud N° " + idSolicitud.ToString() + "</td></tr>";
            //staticCuerpo += "<tr><td> Fecha Solicitud:  " + fechaCreacion + "</td></tr> ";
            //staticCuerpo += "<tr><td> Proveedor:        " + prov.RazonSocial + "</td></tr> ";
            //staticCuerpo += "<tr><td> RUT Proveedor:    " + rutProveedor + "</td></tr> ";
            //staticCuerpo += "<tr><td> Tipo de Solicitud: " + descripcionTipoSolicitud + " </td></tr> ";
            //staticCuerpo += "<tr><td> Tipo de Registro: " + descripcionTipoRegistro + " </td></tr> ";
            //staticCuerpo += "</table>";
            //staticCuerpo += "<p>Saluda atentamente, </p>";
            //staticCuerpo += "<p>Equipo MOP.</p>";
            //// pie de pagina email
            //staticCuerpo += "<table>";
            //staticCuerpo += "<tr><td>";
            //staticCuerpo += "Este mensaje ha sido generado automáticamente por Mercado Público y no puede ser considerado spam según nuestras ";            
            //staticCuerpo += "<a target='_blank'  href='"+ staticURLCondicionesDeUsoMP + "'>condiciones de uso.</a>";
            //staticCuerpo += "</td></tr>";
            //staticCuerpo += "<tr><td>";
            //staticCuerpo += "<p style='font-size: 13px; text-align: center;'>";
            //staticCuerpo += "Ministerio de Obras Públicas<br>";
            //staticCuerpo += "Morandé 59, Santiago de Chile<br>";
            //staticCuerpo += "Atención Ciudadana (56 2) 2449 4000";
            //staticCuerpo += "  </p>";
            //staticCuerpo += "</td></tr>";
            //staticCuerpo += "</table>";
            //staticCuerpo += "</html>";

            //string respWS = "";

            //////////////////////////////////////////////////////
            //////////////////////////////////////////////////////
            ////REVISAR DESTINATARIOS MOP

            //try
            //{
            //    WS_EnvioMail.EnvioCorreosSoapClient wsMail = new WS_EnvioMail.EnvioCorreosSoapClient();
            //    //respWS = wsMail.enviarCorreoHTML(staticEmisorMOP, staticReceptorMOPTMP, staticAsunto, staticCuerpo, staticConCopia);
            //    respWS = wsMail.enviarCorreoHTML(staticEmisorMOP, "rodrigo.aguilar@mop.gov.cl", staticAsunto, staticCuerpo, "");
            //}
            //catch (Exception x)
            //{
            //    respWS = x.Message.ToString();
            //    //throw;
            //}

            ////RESTAURAR DESPUES Hasta aca
            return respWS;

        }

        //ResponderAnalisis
        public static string EnviarNotificacion_ResponderAnalisis(int idSolicitud, string areaAnalisis)
        {
            
            string descripcionTipoSolicitud = "";
            string descripcionTipoRegistro = "";
            string fechaCreacion = "";
            string rutProveedor = "";

            using (dsSolicitudesMOP db = new dsSolicitudesMOP())
            {
                var list = db.SP_MOP_TRAER_SOLICITUD_POR_ID(idSolicitud).ToList();
                foreach (var s in list)
                {
                    descripcionTipoSolicitud = s.TipoSol;
                    descripcionTipoRegistro = s.TipoReg;
                    fechaCreacion = s.FechaCreacion.ToString();
                    rutProveedor = s.RutProveedor;
                }
            }

            ProveedorView prov = new ProveedorView();
            prov = SQLMethods.TraerDatosProveedor(rutProveedor);

            string saludo = "";
            string saludoAM = "Buenos días,";
            string saludoPM = "Buenas tardes,";
            DateTime date = DateTime.Now;
            string bloque = date.ToString("tt"); //TRAE SI ES AM o PM
            switch (bloque)
            {
                case "AM":
                    saludo = saludoAM;
                    break;
                case "PM":
                    saludo = saludoPM;
                    break;
            }



            staticAsunto = "Respuesta del área de análisis a solicitud N°: " + idSolicitud.ToString();
            staticCuerpo = "<html>";
            staticCuerpo += "<h2> RESPUESTA DEL ÁREA DE ANÁLISIS A SOLICITUD N° " + idSolicitud.ToString() + "</h2>";
            staticCuerpo += "<p>" + saludo + "</p>";
            staticCuerpo += "<p> El área " + areaAnalisis + " ha dado respuesta a la solicitud asignada</p>";
            staticCuerpo += "<table>";
            staticCuerpo += "<tr><td> Solicitud N° " + idSolicitud.ToString() + "</td></tr>";
            staticCuerpo += "<tr><td> Fecha Solicitud:  " + fechaCreacion + "</td></tr> ";
            staticCuerpo += "<tr><td> Proveedor:        " + prov.RazonSocial + "</td></tr> ";
            staticCuerpo += "<tr><td> RUT Proveedor:    " + rutProveedor + "</td></tr> ";
            staticCuerpo += "<tr><td> Tipo de Solicitud: " + descripcionTipoSolicitud + " </td></tr> ";
            staticCuerpo += "<tr><td> Tipo de Registro: " + descripcionTipoRegistro + " </td></tr> ";
            staticCuerpo += "</table>";
            staticCuerpo += "<p>Saluda atentamente, </p>";
            staticCuerpo += "<p>Equipo MOP.</p>";
            // pie de pagina email
            staticCuerpo += "<table>";
            staticCuerpo += "<tr><td>";
            staticCuerpo += "Este mensaje ha sido generado automáticamente por Mercado Público y no puede ser considerado spam según nuestras ";
            staticCuerpo += "<a target='_blank'  href='"+ staticURLCondicionesDeUsoMP + "'>condiciones de uso.</a>";
            staticCuerpo += "</td></tr>";
            staticCuerpo += "<tr><td>";
            staticCuerpo += "<p style='font-size: 13px; text-align: center;'>";
            staticCuerpo += "Ministerio de Obras Públicas<br>";
            staticCuerpo += "Morandé 59, Santiago de Chile<br>";
            staticCuerpo += "Atención Ciudadana (56 2) 2449 4000";
            staticCuerpo += "  </p>";
            staticCuerpo += "</td></tr>";
            staticCuerpo += "</table>";
            staticCuerpo += "</html>";

            string respWS = "";

            ////////////////////////////////////////////////////
            ////////////////////////////////////////////////////
            //REVISAR DESTINATARIOS MOP

            try
            {
                WS_EnvioMail.EnvioCorreosSoapClient wsMail = new WS_EnvioMail.EnvioCorreosSoapClient();
                //respWS = wsMail.enviarCorreoHTML(staticEmisorMOP, staticReceptorMOPTMP, staticAsunto, staticCuerpo, staticConCopia);
                respWS = wsMail.enviarCorreoHTML(staticEmisorMOP, "rodrigo.aguilar@mop.gov.cl", staticAsunto, staticCuerpo, "");
            }
            catch (Exception x)
            {
                respWS = x.Message.ToString();
                //throw;
            }


            return respWS;

        }


        //SolicitarCorrecciones
        public static string EnviarNotificacion_SolicitarCorreccionesProveedor(int idSolicitud, string emailProveedor, string funcionarioMOP)
        {
            
            string descripcionTipoSolicitud = "";
            string descripcionTipoRegistro = "";
            string fechaCreacion = "";
            string rutProveedor = "";

            using (dsSolicitudesMOP db = new dsSolicitudesMOP())
            {
                var list = db.SP_MOP_TRAER_SOLICITUD_POR_ID(idSolicitud).ToList();
                foreach (var s in list)
                {
                    descripcionTipoSolicitud = s.TipoSol;
                    descripcionTipoRegistro = s.TipoReg;
                    fechaCreacion = s.FechaCreacion.ToString();
                    rutProveedor = s.RutProveedor;
                }
            }

            ProveedorView prov = new ProveedorView();
            prov = SQLMethods.TraerDatosProveedor(rutProveedor);


            string saludo = "";
            string saludoAM = "Buenos días,";
            string saludoPM = "Buenas tardes,";
            DateTime date = DateTime.Now;
            string bloque = date.ToString("tt"); //TRAE SI ES AM o PM
            switch (bloque)
            {
                case "AM":
                    saludo = saludoAM;
                    break;
                case "PM":
                    saludo = saludoPM;
                    break;
            }

            string asunto = "";
            string cuerpo = "";

            asunto = "MOP solicita correcciones a su solicitud N°: " + idSolicitud.ToString();
            cuerpo = "<html>";
            cuerpo += "<h2> MOP SOLICITA CORRECCIONES A SU SOLICITUD N° " + idSolicitud.ToString() + "</h2>";
            cuerpo += "<p>" + saludo + "</p>";
            cuerpo += "<p> Junto con saludar, le informamos que se la solicitud N°: <b>" + idSolicitud.ToString() + " ha sido devuelta, </p>";
            cuerpo += "<p> por favor verificar en la opción <b>Ver mi ficha registro MOP</b>, ingresar a la solicitud y ver las correcciones ahí mencionadas.</p>";            
            cuerpo += "<p>Saluda atentamente, </p>";
            cuerpo += "<p>Equipo MOP.</p>";
            // pie de pagina email
            cuerpo += "<table>";
            cuerpo += "<tr><td>";
            cuerpo += "Este mensaje ha sido generado automáticamente por Mercado Público y no puede ser considerado spam según nuestras ";
            cuerpo += "<a target='_blank'  href='"+ staticURLCondicionesDeUsoMP + "'>condiciones de uso.</a>";
            cuerpo += "</td></tr>";
            cuerpo += "<tr><td>";
            cuerpo += "<p style='font-size: 13px; text-align: center;'>";
            cuerpo += "Ministerio de Obras Públicas<br>";
            cuerpo += "Morandé 59, Santiago de Chile<br>";
            cuerpo += "Atención Ciudadana (56 2) 2449 4000";
            cuerpo += "  </p>";
            cuerpo += "</td></tr>";
            cuerpo += "</table>";
            cuerpo += "</html>";

            string respWS = "";

            

            try
            {
                WS_EnvioMail.EnvioCorreosSoapClient wsMail = new WS_EnvioMail.EnvioCorreosSoapClient();
                if (staticBaseDatos.Equals("DESA")) {
                    emailProveedor = staticReceptorProveedorTMP;
                }

                //respWS = wsMail.enviarCorreoHTML(staticEmisorProveedores, emailProveedor, asunto, cuerpo, staticConCopia);
                respWS = wsMail.enviarCorreoHTML(staticEmisorProveedores, "rodrigo.aguilar@mop.gov.cl", asunto, cuerpo, "");

                //log mail
                //string funcionarioMOP = Session["SessionNombreUsuario"].ToString();                
                int r = GuardarMOPLogEnvioCorreoProveedores(rutProveedor, idSolicitud, asunto, cuerpo, emailProveedor, funcionarioMOP, respWS);
            }
            catch (Exception x)
            {
                respWS = x.Message.ToString();
                //throw;
            }


            return respWS;

        }




        //ResolucionFinalProve
        public static string EnviarNotificacion_ResolucionFinalProveedor(int idSolicitud, string emailProveedor, string funcionarioMOP)
        {

            string descripcionTipoSolicitud = "";
            string descripcionTipoRegistro = "";
            string fechaCreacion = "";
            string rutProveedor = "";

            using (dsSolicitudesMOP db = new dsSolicitudesMOP())
            {
                var list = db.SP_MOP_TRAER_SOLICITUD_POR_ID(idSolicitud).ToList();
                foreach (var s in list)
                {
                    descripcionTipoSolicitud = s.TipoSol;
                    descripcionTipoRegistro = s.TipoReg;
                    fechaCreacion = s.FechaCreacion.ToString();
                    rutProveedor = s.RutProveedor;
                }
            }

            ProveedorView prov = new ProveedorView();
            prov = SQLMethods.TraerDatosProveedor(rutProveedor);


            string saludo = "";
            string saludoAM = "Buenos días,";
            string saludoPM = "Buenas tardes,";
            DateTime date = DateTime.Now;
            string bloque = date.ToString("tt"); //TRAE SI ES AM o PM
            switch (bloque)
            {
                case "AM":
                    saludo = saludoAM;
                    break;
                case "PM":
                    saludo = saludoPM;
                    break;
            }

            string asunto, cuerpo;


            asunto = "MOP informa la resolución final de solicitud N°:  " + idSolicitud.ToString();
            cuerpo = "<html>";
            cuerpo += "<h2> MOP INFORMA LA RESOLUCIÓN FINAL DE SOLICITUD N° " + idSolicitud.ToString() + "</h2>";
            cuerpo += "<p>" + saludo + "</p>";
            cuerpo += "<p> Junto con saludar, tenemos a bien enviar a usted el oficio que informa la resolución de su solicitud N°: <b>" + idSolicitud.ToString() + "</b>. ";
            cuerpo += "<p> Dicho documento es enviado en forma electrónica a través del portal de Mercado Público, ingresando a <b>Ver mi ficha Registro MOP</b></p>";
            cuerpo += "<p>Saluda atentamente, </p>";
            cuerpo += "<p>Equipo MOP.</p>";
            // pie de pagina email
            cuerpo += "<table>";
            cuerpo += "<tr><td>";
            cuerpo += "Este mensaje ha sido generado automáticamente por Mercado Público y no puede ser considerado spam según nuestras ";
            cuerpo += "<a target='_blank'  href='" + staticURLCondicionesDeUsoMP + "'>condiciones de uso.</a>";
            cuerpo += "</td></tr>";
            cuerpo += "<tr><td>";
            cuerpo += "<p style='font-size: 13px; text-align: center;'>";
            cuerpo += "Ministerio de Obras Públicas<br>";
            cuerpo += "Morandé 59, Santiago de Chile<br>";
            cuerpo += "Atención Ciudadana (56 2) 2449 4000";
            cuerpo += "  </p>";
            cuerpo += "</td></tr>";
            cuerpo += "</table>";
            cuerpo += "</html>";

            string respWS = "";

            ////////////////////////////////////////////////////
            ////////////////////////////////////////////////////
            //REVISAR DESTINATARIOS MOP

            try
            {
                WS_EnvioMail.EnvioCorreosSoapClient wsMail = new WS_EnvioMail.EnvioCorreosSoapClient();
                if (staticBaseDatos.Equals("DESA"))
                {
                    emailProveedor = staticReceptorProveedorTMP;
                }                    
                //respWS = wsMail.enviarCorreoHTML(staticEmisorProveedores, emailProveedor, asunto, cuerpo, staticConCopia);
                respWS = wsMail.enviarCorreoHTML(staticEmisorProveedores, "rodrigo.aguilar@mop.gov.cl", asunto, cuerpo, "");
                
                int r = GuardarMOPLogEnvioCorreoProveedores(rutProveedor, idSolicitud, asunto, cuerpo, emailProveedor, funcionarioMOP, respWS);
            }
            catch (Exception x)
            {
                respWS = x.Message.ToString();
                //throw;
            }


            return respWS;

        }


        private static int GuardarMOPLogEnvioCorreoProveedores(string rutProveedor, int numSolicitud, string motivoEmail, string cuerpoEmail,
            string emailDestinatario,string funcionarioResponsable, string respuestaWS) {
            try
            {
                using (dsSolicitudesMOP db = new dsSolicitudesMOP())
                {
                    int OK = db.SP_MOP_INSERTAR_MOP_LOG_CORREO_PROVEEDORES(rutProveedor, numSolicitud, motivoEmail, cuerpoEmail, emailDestinatario, funcionarioResponsable, respuestaWS);
                    return OK;
                }
            }
            catch (Exception x)
            {
                throw x;
            }

        }

        //

    }
}