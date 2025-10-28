using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class FacturaLecheVenta_MS
    {
        public int DOCTO_VE_ID { get; set; }
        public string FOLIO { get; set; }
        public string FECHA { get; set; }
        public string CLAVE_CLIENTE { get; set; }
        public int CLIENTE_ID { get; set; }
        public string NOMBRE_CLIENTE { get; set; }
        public string USUARIO_CREADOR { get; set; }
        public string CLAVE_FISCAL { get; set; }
        public decimal UNIDADES { get; set; }
        public decimal PRECIO_UNITARIO { get; set; }
        public decimal PRECIO_TOTAL_NETO { get; set; }
        public string ARTICULO { get; set; }
        public int ARTICULO_ID { get; set; }
    }
}