using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models.ContabilidadMS
{
    public class SolicitudPagoDetalleMS
    {
        public string id_docto_ba { get; set; }
        public string id_concepto_ba { get; set; }
        public string nombre_concepto { get; set; }
        public string fecha_ba { get; set; }
        public string referencia { get; set; }

        public string importe { get; set; }
        public string moneda_id { get; set; }
        public string moneda_nombre { get; set; }
        public string moneda_clave_fiscal { get; set; }
        public string descripcion { get; set; }
        public string beneficiario_id { get; set; }
        public string beneficiario_nombre { get; set; }
        public string prov_cuenta_bancaria { get; set; }
        public string prov_tipo_persona { get; set; }
        public string prov_nombre_pf { get; set; }
        public string id_docto_cp { get; set; }
    }
}