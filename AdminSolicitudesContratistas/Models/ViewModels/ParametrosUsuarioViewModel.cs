using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models.ViewModels
{
    public class PerfilViewModel
    {
        public string CodPerfil { get; set; }
        public string DescPerfil { get; set; }
    }

    public class ParametrosUsuarioViewModel
    {
        public List<FiltroItemViewModel> TiposSolicitud { get; set; }
        public List<FiltroItemViewModel> TiposRegistro { get; set; }
        public List<PerfilViewModel> Perfiles { get; set; }
        public List<RegionFiltroViewModel> Regiones { get; set; }

        public ParametrosUsuarioViewModel()
        {
            TiposSolicitud = new List<FiltroItemViewModel>();
            TiposRegistro = new List<FiltroItemViewModel>();
            Perfiles = new List<PerfilViewModel>();
            Regiones = new List<RegionFiltroViewModel>();
        }
    }
}