using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models.Tracker
{
    public class TiradasDiarias
    {
        public int id_ing { get; set; }
        public string clave { get; set; }
        public string ingrediente { get; set; }
        public string tipo { get; set; }
        public decimal ppto { get; set; }
        public decimal real { get; set; }



    }
}