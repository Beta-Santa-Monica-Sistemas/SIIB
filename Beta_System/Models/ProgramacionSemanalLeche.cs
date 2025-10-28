using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class ProgramacionSemanalLeche
    {
        public List<int> Anio { get; set; }
        public List<int> Mes { get; set; }
        public List<int> NoSemana { get; set; }
        public List<DateTime> FechaInicial { get; set; }
        public List<DateTime> FechaFinal { get; set; }
        public List<decimal> ProgramadoSM { get; set; }
        public List<decimal> ProgramadoSG { get; set; }
        public List<decimal> TotalLitros { get; set; }
        public ProgramacionSemanalLeche()
        {
            Anio = new List<int>();
            Mes = new List<int>();
            NoSemana = new List<int>();
            FechaInicial = new List<DateTime>();
            FechaFinal = new List<DateTime>();
            ProgramadoSM = new List<decimal>();
            ProgramadoSG = new List<decimal>();
            TotalLitros = new List<decimal>();
        }
    }
}