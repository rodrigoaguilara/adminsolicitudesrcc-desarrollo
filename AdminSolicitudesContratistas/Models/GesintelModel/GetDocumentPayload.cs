using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminSolicitudesContratistas.Models.GesintelModel
{
    public class GetDocumentPayload
    {
        [JsonProperty("folio")]
        public int Folio { get; set; }
    }
}