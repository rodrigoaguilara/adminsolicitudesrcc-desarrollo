using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{
    public class DocumentoProveedorViewModel
    {
        public string NOMBRE_ARCHIVO { get; set; }
        public string DESCRIPCION { get; set; }
        public int? ID_SOL { get; set; }
        public string AREA_ANALISIS { get; set; }
        public string TIPO_DOCUMENTO { get; set; }
        public string TIPO { get; set; }
        public int? AÑO { get; set; }
        public string PALABRAS_CLAVE { get; set; }
        public string VISIBLE_AL_PROVEEDOR { get; set; }
        public long? ID_ARCHIVO_GESINTEL { get; set; }
    }
}