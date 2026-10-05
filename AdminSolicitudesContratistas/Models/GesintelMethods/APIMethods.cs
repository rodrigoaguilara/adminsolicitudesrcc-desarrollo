using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using System.Net.Http;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;


using AdminSolicitudesContratistas.Models.GesintelModel;
using AdminSolicitudesContratistas.Models.GesintelMethods;
using System.Net;

namespace AdminSolicitudesContratistas.Models.GesintelMethods
{
    public class APIMethods
    {
        private string _baseUrl = System.Configuration.ConfigurationManager.AppSettings["GesintelBaseURL"].ToString();
        private string _authenticate = System.Configuration.ConfigurationManager.AppSettings["AuthenticateMethod"].ToString();
        static int staticGesintelClientTimeOutInMinutes = System.Convert.ToInt32(ConfigurationManager.AppSettings.Get("GesintelClientTimeoutInMinutes"));

        //private static HttpClient _httpClient;
        private static readonly HttpClient _httpClient;

        static APIMethods()
        {
            // Configuramos el ServicePointManager una sola vez para toda la aplicación
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;

            _httpClient = new HttpClient();
            // El timeout debe ser razonable (ej. 30 segundos), no minutos.
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public AuthenticateResponse Authenticate(AuthenticatePayload payload)
        {
            // Creamos un contexto para identificar este paso en el log
            string contextoLog = $"API_Auth(User:{payload.Username})";

            try
            {
                FuncionesGenerales.LogTraceProduccion($"{contextoLog} - Iniciando llamado POST a Gesintel");

                string payl = JsonConvert.SerializeObject(payload, Formatting.None);
                StringContent sc = new StringContent(payl, Encoding.UTF8, "application/json");
                string url = _baseUrl + _authenticate;

                // Ejecutamos la petición
                var responseTask = _httpClient.PostAsync(url, sc);
                var response = responseTask.GetAwaiter().GetResult();

                FuncionesGenerales.LogTraceProduccion($"{contextoLog} - Respuesta recibida. Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var resultString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                    // Logueamos una parte del resultado para traza (sin exponer la clave completa si es posible)
                    FuncionesGenerales.LogTraceProduccion($"{contextoLog} - Deserializando respuesta OK");

                    return JsonConvert.DeserializeObject<AuthenticateResponse>(resultString);
                }
                else
                {
                    string errorDetail = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    FuncionesGenerales.LogTraceProduccion($"{contextoLog} - ERROR API: {response.StatusCode} - Detalle: {errorDetail}");

                    return new AuthenticateResponse { Status = "ERROR", Token = "" };
                }
            }
            catch (Exception ex)
            {
                // LOG DE ERROR EN TXT: 
                // Usamos 0 como numSolicitud ya que aquí no lo tenemos directamente
                FuncionesGenerales.LogErrorProduccion(contextoLog, 0, ex);

                return new AuthenticateResponse { Status = "ERROR", Token = "" };
            }
        }
    }
}