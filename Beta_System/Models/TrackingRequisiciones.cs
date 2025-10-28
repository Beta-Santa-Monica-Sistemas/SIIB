using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class TrackingRequisiciones
    {
        public bool Registrada { get; set; }
        public DateTime fecha_registro { get; set; }

        public bool Firma_1 { get; set; }
        public DateTime fecha_firma_1 { get; set; }

        public List<C_compras_cotizaciones_requisiciones> cotizaciones { get; set; }

        public List<C_compras_cotizaciones_requisiciones> confirmaciones { get; set; }

        public List<C_compras_cotizaciones_confirmadas_g> parcialidades { get; set; }


        public bool Firma_2 { get; set; }
        public DateTime fecha_firma_2 { get; set; }

        public int no_ordenes_compra { get; set; }
        public List<C_compras_ordenes_g> ordenes_compra { get; set; }
        


    }
}