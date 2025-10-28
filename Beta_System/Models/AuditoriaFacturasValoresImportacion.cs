using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class AuditoriaFacturasValoresImportacion
    {
        public string id_docto_cp_ms { get; set; }
        public string concepto_ms { get; set; }
        public string folio_ms { get; set; }
        public string estatus { get; set; }
        public string color { get; set; }

        public decimal importe_ms { get; set; }
        public decimal impuesto_ms { get; set; }
        public decimal iva_retenido_ms { get; set; }
        public decimal isr_retenido_ms { get; set; }
        public decimal total_ms { get; set; }
        public decimal total_co_ms { get; set; }


        public DateTime fecha_registro { get; set; }
        public string no_orden { get; set; }
        public decimal subtotal{ get; set; }
        public decimal iva_monto { get; set; }
        public decimal iva_retenido_monto { get; set; }
        public decimal isr_monto { get; set; }
        public decimal ieps_monto { get; set; }
        public decimal descuento { get; set; }
        public string proveedor { get; set; }
        public string cve_proveedor { get; set; }
        public string moneda { get; set; }
        public decimal total_sib { get; set; }


    }
}