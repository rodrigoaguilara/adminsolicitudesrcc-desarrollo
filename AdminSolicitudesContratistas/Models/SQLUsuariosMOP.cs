using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;

namespace AdminSolicitudesContratistas.Models
{
    public class SQLUsuariosMOP
    {
        
        
        public static int UsuarioMOPExistenteDB(string username)
        {
            int ret = 0;
            using (dsSolicitudesMOP db = new dsSolicitudesMOP())
            {
                var outputParameter = new ObjectParameter("EXISTE", typeof(Int32));
                db.SP_MOP_VALIDAR_USUARIO_MOP_EXISTE(username, outputParameter);
                ret = (int)outputParameter.Value;

            }


            return ret;
        }

    }
}