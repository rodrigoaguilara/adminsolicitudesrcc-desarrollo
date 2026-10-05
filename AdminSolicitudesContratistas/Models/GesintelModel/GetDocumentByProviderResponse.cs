using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminSolicitudesContratistas.Models.GesintelModel
{
    public class GetDocumentByProviderResponse
    {
        [JsonProperty("documents")]
        public List<Document> Documents { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }

        public class Document
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("creationDate")]
            public long CreationDate { get; set; }
            [JsonProperty("clientId")]
            public string ClientId { get; set; }
            [JsonProperty("folio")]
            public int Folio { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("record")]
            public object Record { get; set; }
            [JsonProperty("stage")]
            public string Stage { get; set; }
            [JsonProperty("statusAsig")]
            public string StatusAsig { get; set; }
            [JsonProperty("origin")]
            public string Origin { get; set; }
            [JsonProperty("asignedTo")]
            public string AsignedTo { get; set; }
            [JsonProperty("asignedToId")]
            public object AsignedToId { get; set; }
            [JsonProperty("dateAsigned")]
            public object DateAsigned { get; set; }
            [JsonProperty("daysAsigned")]
            public object DaysAsigned { get; set; }
            [JsonProperty("dateLastReq")]
            public object DateLastReq { get; set; }
            [JsonProperty("nroReqs")]
            public int NroReqs { get; set; }
            [JsonProperty("prevStage")]
            public string PrevStage { get; set; }
            [JsonProperty("datePrevStage")]
            public long? datePrevStage { get; set; }
            [JsonProperty("daysStage")]
            public int? DaysStage { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("size")]
            public int Size { get; set; }
            [JsonProperty("validity")]
            public string Validity { get; set; }
        }
    }
}