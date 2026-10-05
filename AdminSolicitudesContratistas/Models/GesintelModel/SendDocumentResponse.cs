using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminSolicitudesContratistas.Models.GesintelModel
{
    public class SendDocumentResponse
    {
        [JsonProperty("folio")]
        public string Folio { get; set; }
        [JsonProperty("status")]
        public string status { get; set; }
    }
}