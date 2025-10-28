using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class RequiServicio
    {
        private int _IdProveedores { get; set; }
        private int _IdArticulos { get; set; }
        private int _Id_Requi_d { get; set; }
        public RequiServicio()
        {

        }
        public RequiServicio(
            int _IdProveedores,
            int _IdArticulos,
            int _Id_Requi_d
            )
        {
            this._IdProveedores = _IdProveedores;
            this._IdArticulos = _IdArticulos;
            this._Id_Requi_d = _Id_Requi_d;
        }
        public int IdProveedores { get => _IdProveedores; set => _IdProveedores = value; }
        public int IdArticulos { get => _IdArticulos; set => _IdArticulos = value; }
        public int Id_Requi_d { get => _Id_Requi_d; set => _Id_Requi_d = value; }
    }
}