using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models.Tracker
{
    public class KardexIngredientes
    {
        public string Fecha { get; set; }
        public int id_articulo { get; set; }
        public int id_ing { get; set; }
        public string nombre_articulo { get; set; }

        public string concepto { get; set; }
        public string tipo { get; set; }
        public string hra_entrada { get; set; }


        public decimal Compra { get; set; }
        public decimal Transferencia  { get; set; }
        public decimal Ajuste { get; set; }
        public decimal Consumo { get; set; }
        public decimal Saldo { get; set; }

    }
}