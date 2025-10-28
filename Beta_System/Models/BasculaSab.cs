using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class BasculaSab
    {
        private int _id_establo;
        private int _id_ficha;        
        private string _folio;

        private string _fecha1;
        private string _fecha2;
        private string _placas;

        private string _chofer;
        private string _producto;
        private string _cliente;

        private decimal _pesada1;
        private decimal _pesada2;
        private decimal _pesadat;

        private string _pesador;
        private string _observaciones;
        

        public BasculaSab()
        {

        }

        public BasculaSab(int id_establo, int id_ficha, string folio, string fecha1, string fecha2, string placas, string chofer,
            string producto, decimal pesada1, decimal pesada2, decimal pesadat, string pesador, string observaciones, string cliente)
        {
            this._id_establo = id_establo;
            this._id_ficha = id_ficha;
            this._folio = folio;
            this._fecha1 = fecha1;
            this._fecha2 = fecha2;
            this._placas = placas;
            this._chofer = chofer;
            this._producto = producto;
            this._pesada1 = pesada1;
            this._pesada2 = pesada2;
            this._pesadat = pesadat;
            this._pesador = pesador;
            this._observaciones = observaciones;
            this._cliente = cliente;
        }
        public int Id_establo { get => _id_establo; set => _id_establo = value; }
        public int Id_ficha { get => _id_ficha; set => _id_ficha = value; }
        public string Folio { get => _folio; set => _folio = value; }
        public string Fecha1 { get => _fecha1; set => _fecha1 = value; }
        public string Fecha2 { get => _fecha2; set => _fecha2 = value; }
        public string Placas { get => _placas; set => _placas = value; }
        public string Chofer { get => _chofer; set => _chofer = value; }
        public string Producto { get => _producto; set => _producto = value; }
        public decimal Pesada1 { get => _pesada1; set => _pesada1 = value; }
        public decimal Pesada2 { get => _pesada2; set => _pesada2 = value; }
        public decimal Pesadat { get => _pesadat; set => _pesadat = value; }

        public string Pesador { get => _pesador; set => _pesador = value; }
        public string Observaciones { get => _observaciones; set => _observaciones = value; }
        public string Cliente { get=>_cliente; set => _cliente = value; }

    }
}