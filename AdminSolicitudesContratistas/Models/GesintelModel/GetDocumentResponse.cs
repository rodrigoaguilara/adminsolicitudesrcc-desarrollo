using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace AdminSolicitudesContratistas.Models.GesintelModel
{
    public class GetDocumentResponse
    {
        [JsonProperty("document")]
        public Document DocumentProp { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }

        public class Document
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("creationDate")]
            public long CreationDate { get; set; }
            [JsonProperty("folio")]
            public int Folio { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("provider")]
            public Provider ProviderProp { get; set; }
            [JsonProperty("stage")]
            public object Stage { get; set; }
            [JsonProperty("origin")]
            public string Origin { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("size")]
            public int Size { get; set; }
            [JsonProperty("revision")]
            public object Revision { get; set; }
        }

        public class Provider
        {
            [JsonProperty("rut")]
            public string Rut { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("category")]
            public string Category { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("code")]
            public string Code { get; set; }
        }

    }
}