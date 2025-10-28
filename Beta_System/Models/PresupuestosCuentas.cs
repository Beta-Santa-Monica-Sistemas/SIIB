using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class PresupuestosCuentas
    {
        public virtual C_cuentas_contables_g c_cuentas { get; set; }
        public virtual C_presupuestos_cuentas_meses_anios c_presupuestos { get; set; }
    }
}