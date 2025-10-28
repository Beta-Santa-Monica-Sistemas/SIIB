using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class HistoricoFacturasLeche
    {
        public List<string> Establo { get; set; }
        public List<string> Destino { get; set; }
        public List<DateTime> Fecha { get; set; }
        public List<string> Folio { get; set; }
        public List<int> Ficha { get; set; }
        public List<string> Tanque { get; set; }
        public List<decimal> Litros_envio { get; set; }
        public List<int> Lote { get; set; }
        public List<decimal> Litros_recibido { get; set; }
        public List<decimal> Precio_litro { get; set; }
        public List<decimal> Precio_flete { get; set; }
        public List<decimal> Importe { get; set; }
        public List<string> Registro { get; set; }
        public HistoricoFacturasLeche()
        {
            Establo = new List<string>();
            Destino = new List<string>();
            Fecha = new List<DateTime>();
            Folio = new List<string>();
            Ficha = new List<int>();
            Tanque = new List<string>();
            Litros_envio = new List<decimal>();
            Lote = new List<int>();
            Litros_recibido = new List<decimal>();
            Precio_litro = new List<decimal>();
            Precio_flete = new List<decimal>();
            Importe = new List<decimal>();
            Registro = new List<string>();
        }
    }
}