using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
	public class ReporteConceptosNomina
	{
        public int id_nomina_g { get; set; }
        public string nombre_nomina { get; set; }
        public List<int> id_conceptos { get; set; }
        public List<string>  nombres_conceptos { get; set; }
        public List<decimal> valores_conceptos { get; set; }



    }
}