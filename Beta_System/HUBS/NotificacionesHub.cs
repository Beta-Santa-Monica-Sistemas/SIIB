using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;


namespace Beta_System.HUBS
{
    public class NotificacionesHub: Hub
    {
        private static string MensajeGlobal = null;

        public void EstablecerAnuncio(string mensaje)
        {
            MensajeGlobal = mensaje;
            Clients.All.RecibirAnuncioGlobal(mensaje);
        }

        public void QuitarAnuncio()
        {
            MensajeGlobal = null;
            Clients.All.QuitarAnuncioGlobal();
        }

        public string ObtenerAnuncio()
        {
            return MensajeGlobal;
        }




    }
}