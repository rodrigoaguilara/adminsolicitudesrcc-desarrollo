using System;
using System.Net;

namespace AdminSolicitudesContratistas.Services
{
    public static class PlantillaCorreoService
    {
        public static string PlantillaCorreos(
            string RazonSocial,
            string RutProveedor,
            string TipoRegistro,
            string NombreDocumento,
            String FechaPublicacion,
            string CorreoRegistro,
            string Asunto,
            string Mensaje)
        {

            string cuerpo = @"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Solicitud Enviada</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f4f4; font-family: 'Arial Narrow', 'Helvetica Condensed', Arial, sans-serif;"">

    <!-- Contenedor Principal con borde gris -->
    <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""max-width: 600px; background-color: #ffffff;font-family: 'Arial Narrow', 'Helvetica Condensed', Arial, sans-serif; border: 1px solid #dcdcdc; margin-top: 20px; margin-bottom: 20px;"">
        
        <!-- Línea superior dividida: Mitad Azul y Mitad Roja -->
        <tr>
            <td width=""50%"" height=""4"" style=""background-color: #0E5A94; font-size: 0; line-height: 0;"">&nbsp;</td>
            <td width=""50%"" height=""4"" style=""background-color: #E63B4D; font-size: 0; line-height: 0;"">&nbsp;</td>
        </tr>

        <!-- Encabezado / Logos -->
        <tr>
            <td colspan=""2"" style=""padding: 25px 30px 20px 30px; border-bottom: 1px solid #ededed;"">
                <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                    <tr>
                        <td>
                            <!-- Texto superior secundario en azul institucional -->
                            <div style=""font-size: 11px; font-weight: bold; color: #0E5A94; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 4px;"">
                                Dirección General de Obras Públicas
                            </div>
                            <!-- Título Principal -->
                            <div style=""font-size: 21px; font-weight: bold; color: #0A3555; line-height: 1.2; margin-bottom: 5px;"">
                                Registro de Contratistas y Consultores
                            </div>
                            <!-- Subtítulo inferior -->
                            <div style=""font-size: 12px; color: #666666; font-weight: normal;"">
                                Ministerio de Obras Públicas · Gobierno de Chile
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>

        <!-- Cuerpo del mensaje -->
        <tr>
            <td colspan=""2"" style=""padding: 30px 30px 20px 30px;"">
                
                <!-- Cuadro de Título Destacado (Borde izquierdo en azul principal) -->
                <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""border-left: 4px solid #0C5B96; background-color: #f4f8fb; margin-bottom: 25px;"">
                    <tr>
                        <td style=""padding: 15px 15px;"">
                            <p style=""margin: 0; font-size: 19px; color: #0A3555; font-weight: bold;"">{{Asunto}}</p>
                        </td>
                    </tr>
                </table>

                <!-- Texto de Introducción -->
                <p style=""margin: 10px 0 20px 0; font-size: 17xpx; color: #333333; line-height: 1.6; text-align: justify;"">
                    {{Mensaje}}
                </p>
                <!-- Nota informativa de seguridad -->
                <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""margin-top: 25px; background-color: #f9f9f9; border: 1px dashed #dddddd;"">
                    <tr>
                        <td style=""padding: 12px 15px; font-size: 11px; color: #666666; line-height: 1.4;"">
                            Este mensaje fue generado automáticamente por el Sistema de Solicitudes del Registro de Contratistas y Consultores. Por seguridad, nunca se solicitarán contraseñas ni credenciales mediante este correo.
                        </td>
                    </tr>
                </table>

            </td>
        </tr>

        <!-- Footer / Pie de página institucional -->
        <tr>
            <td colspan=""2"" style=""background-color: #0A3555; padding: 20px 30px; color: #ffffff;"">
                <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                    <tr>
                        <td style=""font-size: 12px; line-height: 1.4; color: #ffffff;"">
                            <strong>Dirección General de Obras Públicas</strong><br>
                            Ministerio de Obras Públicas - Gobierno de Chile
                        </td>
                        <td align=""right"" style=""font-size: 12px; line-height: 1.4; color: #ffffff; vertical-align: middle;"">
                            Registro de Contratistas y Consultores<br>
                            Sistema de Solicitudes
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>

</body>
</html>";


            string mensajeFinal = (Mensaje ?? "")
                .Replace("{{RazonSocial}}", RazonSocial ?? "")
                .Replace("{{RutProveedor}}", RutProveedor ?? "")
                .Replace("{{TipoRegistro}}", TipoRegistro ?? "")
                .Replace("{{NombreDocumento}}", NombreDocumento ?? "")
                .Replace("{{FechaPublicacion}}", FechaPublicacion ?? "")
                .Replace("{{CorreoRegistro}}", CorreoRegistro ?? "");

            // Convertir el mensaje a HTML seguro y respetar saltos de línea
            mensajeFinal = WebUtility.HtmlEncode(mensajeFinal)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", "<br />");

            // Incorporar el mensaje procesado a la plantilla
            return cuerpo
                .Replace("{{Mensaje}}", mensajeFinal)
                .Replace("{{Asunto}}", WebUtility.HtmlEncode(Asunto ?? ""));

        }
    }
}