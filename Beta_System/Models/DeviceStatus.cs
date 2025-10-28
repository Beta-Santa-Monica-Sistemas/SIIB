using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class DeviceStatus
    {
        public int? idAutorizado { get; set; }
        public string NombreDispositivo { get; set; }
        public string Mac { get; set; }
        public string Tipo { get; set; }
        public string Establo { get; set; }
        public DateTime? UltimoLatido { get; set; }
        public bool Online { get; set; }
        public int? MinDesdeUltimo { get; set; }
        public bool? Bloqueo { get; set; }
        public bool? Activo { get; set; }
    }
}