using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminSolicitudesContratistas.Models.GesintelModel
{
    public class GetDocumentByProviderPayload
    {
        [JsonProperty("providerCode")]
        public string ProviderCode { get; set; }
    }
}