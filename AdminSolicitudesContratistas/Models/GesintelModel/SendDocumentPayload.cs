using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminSolicitudesContratistas.Models.GesintelModel
{
    public class SendDocumentPayload
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("rut")]
        public string Rut { get; set; }
        [JsonProperty("providerType")]
        public string ProviderType { get; set; }
        [JsonProperty("providerName")]
        public string ProviderName { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}