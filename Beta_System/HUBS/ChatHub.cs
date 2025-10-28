using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beta_System.Models;


namespace Beta_System.HUBS
{
    public class Mensaje
    {
        public string Origen { get; set; }
        public string Destino { get; set; }
        public string Texto { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class ChatHub : Hub
    {
        //private static ConcurrentDictionary<string, HashSet<string>> UsuariosConectados = new ConcurrentDictionary<string, HashSet<string>>();
        //private static ConcurrentDictionary<string, bool> UsuariosActivos = new ConcurrentDictionary<string, bool>();
        //private static BETA_CORPEntities db = new BETA_CORPEntities();
        //private int dias_historial = Convert.ToInt32(db.C_parametros_configuracion.Find(1026).valor_numerico);


        //private static List<Mensaje> Mensajes = new List<Mensaje>();

        //private string ObtenerNombreUsuario()
        //{
        //    return Context.QueryString["usuario"] ?? "Invitado";
        //}


        //#region -------------------- Conexiones --------------------
        //public override Task OnConnected()
        //{
        //    string connectionId = Context.ConnectionId;
        //    string nombre = ObtenerNombreUsuario();

        //    // Agregar a conexiones
        //    if (!UsuariosConectados.ContainsKey(nombre))
        //        UsuariosConectados[nombre] = new HashSet<string>();

        //    lock (UsuariosConectados[nombre])
        //    {
        //        UsuariosConectados[nombre].Add(Context.ConnectionId);
        //    }

        //    // Por defecto está activo
        //    UsuariosActivos[nombre] = true;

        //    ActualizarListaUsuarios();
        //    return base.OnConnected();
        //}

        //public override Task OnDisconnected(bool stopCalled)
        //{
        //    string connectionId = Context.ConnectionId;
        //    var entry = UsuariosConectados.FirstOrDefault(u => u.Value.Contains(connectionId));
        //    if (!string.IsNullOrEmpty(entry.Key))
        //    {
        //        var nombre = entry.Key;
        //        lock (UsuariosConectados[nombre])
        //        {
        //            UsuariosConectados[nombre].Remove(connectionId);
        //            if (UsuariosConectados[nombre].Count == 0)
        //                UsuariosConectados.TryRemove(nombre, out _);
        //        }
        //    }

        //    ActualizarListaUsuarios();
        //    return base.OnDisconnected(stopCalled);
        //}

        //private void ActualizarListaUsuarios()
        //{
        //    var listaNombres = UsuariosConectados.Keys.OrderBy(n => n).ToList();
        //    Clients.All.ActualizarUsuarios(listaNombres);
        //}

        //#endregion




        //#region -------------------- Mensajes --------------------
        //public void EnviarMensaje(string destinoNombre, string mensaje)
        //{
        //    string origenNombre = ObtenerNombreUsuario();

        //    // Guardar mensaje temporal
        //    Mensajes.Add(new Mensaje { Origen = origenNombre, Destino = destinoNombre, Texto = mensaje, Fecha = DateTime.Now });
        //    Mensajes.RemoveAll(m => m.Fecha < DateTime.Now.AddDays(-dias_historial));

        //    // Enviar solo si el destinatario está activo
        //    if (UsuariosConectados.TryGetValue(destinoNombre, out var destSet) && UsuariosActivos.TryGetValue(destinoNombre, out bool estaActivo) && estaActivo)
        //    {
        //        lock (destSet)
        //        {
        //            foreach (var connId in destSet)
        //                Clients.Client(connId).RecibirMensaje(origenNombre, mensaje);
        //        }
        //    }

        //    // También enviar a mis propias conexiones
        //    if (UsuariosConectados.TryGetValue(origenNombre, out var origSet))
        //    {
        //        lock (origSet)
        //        {
        //            foreach (var connId in origSet)
        //                Clients.Client(connId).RecibirMensaje(origenNombre, mensaje);
        //        }
        //    }
        //}

        //public List<Mensaje> ObtenerHistorial(string otroUsuario)
        //{
        //    string miNombre = ObtenerNombreUsuario();
        //    return Mensajes
        //        .Where(m =>
        //            (m.Origen == miNombre && m.Destino == otroUsuario) ||  // mis mensajes
        //            (m.Origen == otroUsuario && m.Destino == miNombre)     // mensajes del otro
        //        )
        //        .OrderBy(m => m.Fecha)
        //        .ToList();
        //}

        //#endregion




        //#region -------------------- Typing --------------------
        //public void Escribiendo(string destinoNombre, bool escribiendo)
        //{
        //    string origenNombre = ObtenerNombreUsuario();

        //    if (UsuariosConectados.TryGetValue(destinoNombre, out var destSet))
        //    {
        //        lock (destSet)
        //        {
        //            foreach (var connId in destSet)
        //                Clients.Client(connId).RecibirTyping(origenNombre, escribiendo);
        //        }
        //    }
        //}

        //#endregion




        //#region  -------------------- Estatus del chat --------------------
        //public void CambiarEstado(bool activo)
        //{
        //    string nombre = ObtenerNombreUsuario();
        //    UsuariosActivos[nombre] = activo;
        //    ActualizarListaUsuarios();
        //}

        //public bool ObtenerEstado()
        //{
        //    string nombre = ObtenerNombreUsuario();
        //    if (UsuariosActivos.TryGetValue(nombre, out bool estado))
        //        return estado;

        //    // Por defecto activo si no existe
        //    return true;
        //}

        //#endregion





    }

}