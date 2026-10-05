using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace AdminSolicitudesContratistas.Models
{
    public class FuncionesGenerales
    {


        public static string str_to_base64(string str)
        {
            if (str == null)
                return "";

            var plainTextBytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string base64_to_str(string base64)
        {
            if (base64 == null)
                return "";

            var base64EncodedBytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        //public static void LogErrorProduccion(string contexto, int numSolicitud, Exception ex)
        //{
        //    try
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        sb.AppendLine($"--- ERROR EN PRODUCCIÓN ---");
        //        sb.AppendLine($"Contexto: {contexto}");
        //        sb.AppendLine($"Solicitud N°: {numSolicitud}");
        //        sb.AppendLine($"Mensaje: {ex.Message}");
        //        if (ex.InnerException != null)
        //            sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
        //        sb.AppendLine($"Stack Trace: {ex.StackTrace}");
        //        sb.AppendLine($"---------------------------");

        //        using (EventLog eventLog = new EventLog("Application"))
        //        {
        //            eventLog.Source = "Log_AdminRCC"; // Asegúrate de usar el mismo origen que ya funciona en tu código
        //            eventLog.WriteEntry(sb.ToString(), EventLogEntryType.Error, 999, 1);
        //        }
        //    }
        //    catch { /* No dejar que el log rompa la app */ }
        //}

        //public static void LogTraceProduccion(string mensaje)
        //{
        //    try
        //    {
        //        using (EventLog eventLog = new EventLog("Application"))
        //        {
        //            eventLog.Source = "Log_AdminRCC";
        //            eventLog.WriteEntry("TRACE SOLICITUDES: " + mensaje, EventLogEntryType.Information, 888, 1);
        //        }
        //    }
        //    catch { }
        //}


        private static readonly object _lock = new object();

        private static void EscribirEnArchivo(string contenido, string tipo)
        {
            try
            {
                string folderPath = HostingEnvironment.MapPath("~/LogsRCC");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = $"Log_AdminRCC_{DateTime.Now:yyyyMMdd}.txt";
                string fullPath = Path.Combine(folderPath, fileName);

                // --- CAPTURA DE USUARIO ---
                string usuario = "SISTEMA";
                try
                {
                    if (HttpContext.Current?.Session != null && HttpContext.Current.Session["SessionNombreUsuario"] != null)
                        usuario = HttpContext.Current.Session["SessionNombreUsuario"].ToString();
                    else if (HttpContext.Current?.User?.Identity != null)
                        usuario = HttpContext.Current.User.Identity.Name;
                }
                catch { }

                // Incluimos el usuario en la línea de log
                string linea = $"[{DateTime.Now:HH:mm:ss}] [{tipo}] [{usuario}] {contenido}{Environment.NewLine}";

                lock (_lock)
                {
                    File.AppendAllText(fullPath, linea, Encoding.UTF8);
                }
            }
            catch { }
        }

        public static void LogErrorProduccion(string contexto, int numSolicitud, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"ERROR - Solicitud: {numSolicitud} - Contexto: {contexto}");
            sb.AppendLine($"Mensaje: {ex.Message}");
            if (ex.InnerException != null)
                sb.AppendLine($"Inner: {ex.InnerException.Message}");
            sb.AppendLine($"Stack: {ex.StackTrace}");
            sb.AppendLine("----------------------------------------------------------------");

            EscribirEnArchivo(sb.ToString(), "EXCEPTION");
        }

        public static void LogTraceProduccion(string mensaje)
        {
            EscribirEnArchivo(mensaje, "TRACE");
        }

    }
}