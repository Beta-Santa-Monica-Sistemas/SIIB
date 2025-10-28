using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class SubmodulosUsuario
    {
        private int _id_modulo;
        private int _id_submodulo;
        private string _nombre_submodulo;
        private string _funcion;
        private string _controlador;

        private bool _aplica_submenu;
        private int _id_submodulo_item;

        public SubmodulosUsuario()
        {

        }

        public SubmodulosUsuario(int id_modulo, int id_submodulo, string nombre_submodulo, string funcion, string controlador, bool _aplica_submenu, int id_submenu_item)
        {
            this._id_modulo = id_modulo;
            this._id_submodulo = id_submodulo;
            this._nombre_submodulo = nombre_submodulo;
            this._funcion = funcion;
            this._controlador = controlador;
            this._aplica_submenu = _aplica_submenu;
            this._id_submodulo_item = id_submenu_item;
        }
        public int Id_modulo { get => _id_modulo; set => _id_modulo = value; }
        public int Id_submodulo { get => _id_submodulo; set => _id_submodulo = value; }
        public string Nombre_submodulo { get => _nombre_submodulo; set => _nombre_submodulo = value; }
        public string Funcion { get => _funcion; set => _funcion = value; }
        public string Controlador { get => _controlador; set => _controlador = value; }
        public bool Aplica_submenu { get => _aplica_submenu; set => _aplica_submenu = value; }
        public int Id_submodulo_item { get => _id_submodulo_item; set => _id_submodulo_item = value; }


    }
}