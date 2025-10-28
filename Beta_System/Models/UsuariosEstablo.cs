using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class UsuariosEstablo
    {
        public virtual C_usuarios_corporativo c_usuarios { get; set; }
        //public virtual C_establos C_rstablos { get; set; }
        public virtual C_usuarios_establos c_usuarios_establos { get; set; }
    }
}