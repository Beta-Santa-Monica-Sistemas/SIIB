using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class Doctos_CP_MS
    {
        public string Docto_CP_Id { get; set; }
        public string Folio { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; }
        public string Importe { get; set; }
        public string Impuesto { get; set; }
        public string IVA_Retenido { get; set; }
        public string ISR_Retenido { get; set; }
    }
}