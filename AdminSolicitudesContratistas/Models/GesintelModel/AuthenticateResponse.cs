using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminSolicitudesContratistas.Models.GesintelModel
{
    public class AuthenticateResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("expirationDate")]
        public int? ExpirationDate { get; set; }
        [JsonProperty("msg")]
        public string Message { get; set; }
        [JsonProperty("timeout")]
        public int Timeout { get; set; }
    }
}