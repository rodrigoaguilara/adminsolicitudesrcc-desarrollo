using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{

    public class FiltroItemSolViewModel
    {
        public string DESCRIPCION { get; set; }
        public string CODIGO { get; set; }
        public int PADRE { get; set; }
    }

    public class FiltrosSolicitudViewModel
    {
        public List<FiltroItemSolViewModel> TiposRegistro { get; set; }
        public List<FiltroItemSolViewModel> TiposSolicitud { get; set; }
        public List<FiltroItemSolViewModel> EstadosSolicitud { get; set; }

        public FiltrosSolicitudViewModel()
        {
            TiposRegistro = new List<FiltroItemSolViewModel>();
            TiposSolicitud = new List<FiltroItemSolViewModel>();
            EstadosSolicitud = new List<FiltroItemSolViewModel>();
        }
    }
}