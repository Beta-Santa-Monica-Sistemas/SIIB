using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Beta_System.Models
{
    public class REPORTE_ALMACEN_AnalisisConsumos
    {
        public int id_articulo;
        public string Clave_articulo;
        public string Nombre_articulo;
        public string Unidad_Medida;
        public decimal Precio;
        public decimal Importe;

        public decimal Inventario_inicio;
        public decimal Entradas;
        public decimal Salidas;
        public decimal Inventario_final;
        public decimal Consumo;


        public decimal Inventario_real;

        public string Ubicacion;
        public string Fecha_compra;
        public int Clave_Proveedor;
        public string Proveedor;
        public decimal Cantidad_Compra;
        public string Tipo_Articulo;
    }
}