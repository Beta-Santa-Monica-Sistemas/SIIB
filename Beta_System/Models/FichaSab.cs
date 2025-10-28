using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class FichaSab
    {
        public List<string> Fichas { get; set; }
        public List<string> Folios { get; set; }
        public List<string> Articulos { get; set; }
        public List<string> Cliente { get; set; }
        public List<string> Fecha1 { get; set; }
        public List<string> Fecha2 { get; set; }
        public List<string> LineaTransportista { get; set; }
        public List<string> Chofer { get; set; }
        public List<string> Placas { get; set; }
        public List<string> Peso1 { get; set; }
        public List<string> Peso2 { get; set; }
        public List<string> PesoT { get; set; }
        public List<string> MatSeca { get; set; }
        public List<string> Maquilador { get; set; }
        public List<string> Pozo { get; set; }
        public List<string> Observacion { get; set; }
        public List<string> Movimiento { get; set; }
        public List<string> TipoMovimiento { get; set; }
        public List<string> PesoOrigen { get; set; }

        public FichaSab()
        {
            Fichas = new List<string>();
            Folios = new List<string>();
            Articulos = new List<string>();
            Cliente = new List<string>();
            Fecha1 = new List<string>();
            Fecha2 = new List<string>();
            LineaTransportista = new List<string>();
            Chofer = new List<string>();
            Placas = new List<string>();
            Peso1 = new List<string>();
            Peso2 = new List<string>();
            PesoT = new List<string>();
            MatSeca = new List<string>();
            Maquilador = new List<string>();
            Pozo = new List<string>();
            Observacion = new List<string>();
            Movimiento = new List<string>();
            TipoMovimiento = new List<string>();
            PesoOrigen = new List<string>();
        }
    }
}