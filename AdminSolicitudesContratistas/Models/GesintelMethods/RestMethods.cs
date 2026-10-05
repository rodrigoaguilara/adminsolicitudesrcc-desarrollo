using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;

using AdminSolicitudesContratistas.Models.GesintelModel;
using AdminSolicitudesContratistas.Models.GesintelMethods;


namespace AdminSolicitudesContratistas.Models.GesintelMethods
{
    public class RestMethods
    {
        private static string _baseUrl = System.Configuration.ConfigurationManager.AppSettings["GesintelBaseURL"].ToString();
        private static string _sendDocument = System.Configuration.ConfigurationManager.AppSettings["SendDocumentMethod"].ToString();
        private static string _getDocument = System.Configuration.ConfigurationManager.AppSettings["GetDocumentMethod"].ToString();
        private static string _getFileDocument = System.Configuration.ConfigurationManager.AppSettings["GetFileDocumentMethod"].ToString();
        private static string _getDocumentsByProvider = System.Configuration.ConfigurationManager.AppSettings["GetDocumentsByProviderMethod"].ToString();
        private static string _getDocumentsByType = System.Configuration.ConfigurationManager.AppSettings["GetDocumentsByType"].ToString();

        static int staticGesintelClientTimeOutInMinutes = System.Convert.ToInt32(ConfigurationManager.AppSettings.Get("GesintelClientTimeoutInMinutes"));

        private static readonly HttpClient _httpClient;

        static RestMethods()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(staticGesintelClientTimeOutInMinutes)
            };

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        }

        public static GetDocumentResponse GetDocument(string token, GetDocumentPayload payload)
        {
            //_httpClient.DefaultRequestHeaders.Add("Autorization", $"Bearer {token}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string payl = JsonConvert.SerializeObject(payload, Formatting.None);

            StringContent sc = new StringContent(payl, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync(_baseUrl + _getDocument, sc);


            if (response.Result.IsSuccessStatusCode)
            {
                var result = response.Result.Content.ReadAsStringAsync();
                string resultString = result.Result;

                return JsonConvert.DeserializeObject<GetDocumentResponse>(resultString);
            }
            else
            {
                GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " GetDocument RestMethod: " + response.Result.StatusCode + " " + response.Result.Content);
                return null;
            }

        }

        public static byte[] GetFileDocument(string token, GetFileDocumentPayload payload)
        {
            //_httpClient.DefaultRequestHeaders.Add("Autorization", $"Bearer {token}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string payl = JsonConvert.SerializeObject(payload, Formatting.None);

            StringContent sc = new StringContent(payl, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync(_baseUrl + _getFileDocument, sc);

            if (response.Result.IsSuccessStatusCode)
            {
                var result = response.Result.Content.ReadAsByteArrayAsync();

                return result.Result;
            }
            else
            {
                GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " GetFileDocument RestMethod: " + response.Result.StatusCode + " " + response.Result.Content);
                return null;
            }

        }

        public static GetDocumentByProviderResponse GetDocumentsByProvider(string token, GetDocumentByProviderPayload payload)
        {
            //_httpClient.DefaultRequestHeaders.Add("Autorization", $"Bearer {token}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string payl = JsonConvert.SerializeObject(payload, Formatting.None);

            StringContent sc = new StringContent(payl, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync(_baseUrl + _getDocumentsByProvider, sc);

            if (response.Result.IsSuccessStatusCode)
            {
                var result = response.Result.Content.ReadAsStringAsync();
                string resultString = result.Result;

                return JsonConvert.DeserializeObject<GetDocumentByProviderResponse>(resultString);
            }
            else
            {
                GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " GetDocumentsByProvider RestMethod: " + response.Result.StatusCode + " " + response.Result.Content);
                return null;
            }

        }

        public static SendDocumentResponse SendDocument(string token, SendDocumentPayload payload)
        {
            //_httpClient.DefaultRequestHeaders.Add("Autorization", $"Bearer {token}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string payl = JsonConvert.SerializeObject(payload, Formatting.None);

            StringContent sc = new StringContent(payl, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync(_baseUrl + _sendDocument, sc);

            if (response.Result.IsSuccessStatusCode)
            {
                var result = response.Result.Content.ReadAsStringAsync();
                string resultString = result.Result;

                return JsonConvert.DeserializeObject<SendDocumentResponse>(resultString);
            }
            else
            {
                GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Error, DateTime.Now.ToString() + " SendDocument RestMethod: " + response.Result.StatusCode + " " + response.Result.Content);
                return null;
            }

        }

        public static GetDocumentsByTypeResponse.Root GetDocumentByType(string token, GetDocumentsByTypePayload payload)
        //public static GetDocumentsByTypeResponse GetDocumentByType(string token, GetDocumentsByTypePayload payload)
        {
            //_httpClient.DefaultRequestHeaders.Add("Autorization", $"Bearer {token}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string payl = JsonConvert.SerializeObject(payload, Formatting.None);

            StringContent sc = new StringContent(payl, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync(_baseUrl + _getDocumentsByType, sc);


            if (response.Result.IsSuccessStatusCode)
            {
                var result = response.Result.Content.ReadAsStringAsync();
                string resultString = result.Result;

                return JsonConvert.DeserializeObject<GetDocumentsByTypeResponse.Root>(resultString);
            }
            else
            {
                GesintelMethods.Log.EscribeLogEventViewer(System.Diagnostics.TraceLevel.Info, DateTime.Now.ToString() + " GetDocumentByType RestMethod: " + response.Result.StatusCode + " " + response.Result.Content);
                return null;
            }

        }
    }
}