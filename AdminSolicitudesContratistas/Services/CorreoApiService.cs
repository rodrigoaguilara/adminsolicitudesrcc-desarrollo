using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;

namespace AdminSolicitudesContratistas.Services
{
    public class CorreoApiService
    {
        private static readonly HttpClient cliente = new HttpClient();

        public async Task<string> EnviarCorreoAsync(
            string emisor,
            string receptor,
            string asunto,
            string cuerpo,
            string copia)
        {
            string url = ConfigurationManager.AppSettings["CorreoApi_Url"];

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ConfigurationErrorsException(
                    "No se encuentra configurada la URL de la API de correo."
                );
            }

            using (var formulario = new MultipartFormDataContent())
            {
                formulario.Add(new StringContent(emisor), "emisor");
                formulario.Add(new StringContent(receptor), "receptor");
                formulario.Add(new StringContent(asunto), "asunto");
                formulario.Add(new StringContent(copia ?? ""), "copiaa");
                formulario.Add(new StringContent(cuerpo), "cuerpo");

                using (var respuesta = await cliente.PostAsync(url, formulario))
                {
                    respuesta.EnsureSuccessStatusCode();
                    return await respuesta.Content.ReadAsStringAsync();
                }
            }
        }
    }
}