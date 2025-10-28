using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class FichasPendienteSab
    {
        public List<string> Fichas { get; set; }
        public List<string> Folios { get; set; }
        public List<string> Articulos { get; set; }
        public List<string> Cliente { get; set; }
        public List<string> LineaTransportista { get; set; }
        public List<string> Chofer { get; set; }
        public List<string> Placas { get; set; }
        public List<string> Peso1 { get; set; }
        public List<string> Fecha1 { get; set; }
        public List<string> Maquilador { get; set; }
        public List<string> Pozo { get; set; }
        public List<string> Observacion { get; set; }
        public List<string> Movimiento { get; set; }
        public List<string> TipoMovimiento { get; set; }

        public FichasPendienteSab()
        {
            Fichas = new List<string>();
            Folios = new List<string>();
            Articulos = new List<string>();
            Cliente = new List<string>();
            LineaTransportista = new List<string>();
            Chofer = new List<string>();
            Placas = new List<string>();
            Peso1 = new List<string>();
            Fecha1 = new List<string>();
            Maquilador = new List<string>();
            Pozo = new List<string>();
            Observacion = new List<string>();
            Movimiento = new List<string>();
            TipoMovimiento = new List<string>();
        }
    }
}