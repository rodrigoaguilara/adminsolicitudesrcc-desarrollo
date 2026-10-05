using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminSolicitudesContratistas.Models.GesintelModel
{
    public class GetDocumentsByTypePayload
    {
        [JsonProperty("folio")]
        public int Folio { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}