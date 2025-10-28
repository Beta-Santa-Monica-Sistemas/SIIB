using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class CuentasUsuario
    {
        public C_cuentas_contables_g Cuentas { get; set; }
        public C_usuarios_cuentas_contables Cta_Usuario { get; set; }
    }
}