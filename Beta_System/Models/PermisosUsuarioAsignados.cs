using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class PermisosUsuarioAsignados
    {
        private int _permiso;
        private string _nombre_permiso;
        private bool _asignado;

        public PermisosUsuarioAsignados()
        {

        }

        public PermisosUsuarioAsignados(int permiso, string nombre_permiso, bool asignado)
        {
            this._permiso = permiso;
            this._nombre_permiso = nombre_permiso;
            this._asignado = asignado;

        }
        public int Permiso { get => _permiso; set => _permiso = value; }
        public string Nombre_Permiso { get => _nombre_permiso; set => _nombre_permiso = value; }
        public bool Asignado { get => _asignado; set => _asignado = value; }
    }
}