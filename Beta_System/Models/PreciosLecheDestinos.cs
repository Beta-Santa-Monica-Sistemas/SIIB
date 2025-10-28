using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class PreciosLecheDestinos
    {
        public C_envios_leche_destinos Destinos { get; set; }
        public C_envios_leche_precios Precios { get; set; }
        public C_envios_leche_cliente_destinos Clientes_destinos { get; set; }
    }
}