using Beta_System.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text.pdf;

namespace Beta_System.Controllers
{
    public class PORTALController : Controller
    {
        private BETA_CORPEntities db = new BETA_CORPEntities();

        public ActionResult OrdenCompraPDF(int id_orden_g, string token_orden)
        {
            var orden = db.C_compras_ordenes_g.Where(x => x.id_compras_orden_g == id_orden_g && x.token_orden == token_orden).FirstOrDefault();
            if (orden != null)
            {
                ViewBag.id_orden_g = orden.id_compras_orden_g;
                ViewBag.token_orden = orden.token_orden;
                return View("FormatoOrdenCompra/Index", orden);
            }
            else
            {
                return View("../Shared/Error", null);
            }
        }

        public void GenerarOrdenCompraPDF(int id_orden_g, string token_orden)
        {
            var orden = db.C_compras_ordenes_g.Where(x => x.id_compras_orden_g == id_orden_g && x.token_orden == token_orden).FirstOrDefault();

            // Configuración para el documento PDF
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10, 10, 10, 10);
            MemoryStream ms = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            document.Open();

            // Fuentes y colores
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE);
            var headerFont2 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var cellFontMayorNegra = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var cellFontMayor = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            var cellFontsm = FontFactory.GetFont(FontFactory.HELVETICA, 7);
            var cellFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            var cellFontUnderline = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, iTextSharp.text.Font.UNDERLINE);

            var cellError = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 40);
            cellError.Color = BaseColor.RED;

            BaseColor headerBackgroundColor = new BaseColor(52, 58, 64);


            int[] id_parametros = new int[] { 1, 7, 8, 9, 10, 11, 12 };
            var parametros = db.C_parametros_configuracion.Where(x => id_parametros.Contains(x.id_parametro_configuracion));

            string logo_empresa = parametros.Where(x => x.id_parametro_configuracion == 1).FirstOrDefault().valor_texto;
            string nombre_empresa = parametros.Where(x => x.id_parametro_configuracion == 7).FirstOrDefault().valor_texto;
            string direccion_1 = parametros.Where(x => x.id_parametro_configuracion == 8).FirstOrDefault().valor_texto;
            string direccion_2 = parametros.Where(x => x.id_parametro_configuracion == 9).FirstOrDefault().valor_texto;
            string telefono = parametros.Where(x => x.id_parametro_configuracion == 10).FirstOrDefault().valor_texto;
            string rfc = parametros.Where(x => x.id_parametro_configuracion == 11).FirstOrDefault().valor_texto;
            string link_proveedores = parametros.Where(x => x.id_parametro_configuracion == 12).FirstOrDefault().valor_texto;

            PdfPTable headerTable = new PdfPTable(3) { WidthPercentage = 100 };
            headerTable.SetWidths(new float[] { 0.5f, 0.1f, 0.4f });
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            // Agregar logo en la primera celda
            var imagePath = Server.MapPath("~/Content/img_layout/logo_beta_new.png");
            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var logo = iTextSharp.text.Image.GetInstance(System.Drawing.Image.FromStream(stream), System.Drawing.Imaging.ImageFormat.Png);
                logo.ScaleToFit(100f, 100f);
                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                headerTable.AddCell(logoCell);
            }

            PdfPTable tSalto = new PdfPTable(1) { WidthPercentage = 100 };
            tSalto.AddCell(new PdfPCell(new Phrase("\n", cellFont)) { Border = Rectangle.NO_BORDER });
            tSalto.AddCell(new PdfPCell(new Phrase("\n", cellFont)) { Border = Rectangle.NO_BORDER });
            headerTable.AddCell(tSalto);

            // Fechas a la derecha
            PdfPTable dateTable = new PdfPTable(1);
            dateTable.AddCell(new PdfPCell(new Phrase("Fecha y Hora de Impresión", cellFontMayorNegra))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            dateTable.AddCell(new PdfPCell(new Phrase(String.Format(DateTime.Now.ToString(), "dd/mm/yyyy hh:mm"), cellFontMayorNegra))
            {
                Border = iTextSharp.text.Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            headerTable.AddCell(dateTable);


            var cellFontMayorUnderline = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, iTextSharp.text.Font.UNDERLINE);

            //ERROR DEL DOCUMENTO
            if (orden == null)
            {
                document.Add(headerTable);

                // Calcular el espacio restante en la página después del headerTable
                float espacioDisponible = document.PageSize.Height - document.TopMargin - document.BottomMargin - headerTable.TotalHeight;

                // Crear una tabla para centrar el mensaje
                PdfPTable informacion = new PdfPTable(1)
                {
                    WidthPercentage = 100
                };

                // Crear la celda centrada dentro del espacio restante
                PdfPCell cell1 = new PdfPCell(new Phrase("¡ORDEN DE COMPRA NO ENCONTRADA!", cellError))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    FixedHeight = espacioDisponible // Asegurarte de que no exceda el espacio disponible
                };

                // Agregar la celda a la tabla
                informacion.AddCell(cell1);

                // Agregar la tabla al documento
                document.Add(informacion);




                document.Close();
                byte[] PdfBytes = ms.ToArray();
                ms.Close();
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", $"attachment;filename=Orden_Compra_No_Encontrada.pdf");
                Response.Buffer = true;
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(PdfBytes);
                Response.End();
            }
            //DOCUMENTO VALIDO
            else
            {
                // Tabla de información de la empresa
                PdfPTable empresaTable = new PdfPTable(1) { WidthPercentage = 100 };
                empresaTable.AddCell(new PdfPCell(new Phrase("FACTURAR A", cellFontMayorUnderline)) { Border = Rectangle.NO_BORDER });
                empresaTable.AddCell(new PdfPCell(new Phrase(nombre_empresa, cellFontMayorNegra)) { Border = Rectangle.NO_BORDER });
                empresaTable.AddCell(new PdfPCell(new Phrase(direccion_1 + " " + direccion_2, cellFontMayor)) { Border = Rectangle.NO_BORDER });
                empresaTable.AddCell(new PdfPCell(new Phrase("TEL. " + telefono, cellFontMayor)) { Border = Rectangle.NO_BORDER });
                empresaTable.AddCell(new PdfPCell(new Phrase("RFC: " + rfc, cellFontMayor)) { Border = Rectangle.NO_BORDER });
                empresaTable.AddCell(new PdfPCell(new Phrase("Para recepción de facturas a pago ingresar:", cellFontMayor)) { Border = Rectangle.NO_BORDER });

                empresaTable.AddCell(new PdfPCell(new Phrase(link_proveedores, cellFontMayorUnderline)) { Border = Rectangle.NO_BORDER });
                empresaTable.AddCell(new PdfPCell(new Phrase("\n", cellFontMayorNegra)) { Border = Rectangle.NO_BORDER });

                // Tabla de orden de compra 1
                PdfPTable ordenCompraTable1 = new PdfPTable(1) { WidthPercentage = 100 };
                ordenCompraTable1.AddCell(new PdfPCell(new Phrase("ORDEN DE COMPRA      " + orden.id_compras_orden_g, cellFontUnderline)) { Border = Rectangle.NO_BORDER });
                // Tabla de orden de compra 2
                PdfPTable ordenCompraTable2 = new PdfPTable(1) { WidthPercentage = 100 };
                ordenCompraTable2.AddCell(new PdfPCell(new Phrase("Comprador", cellFontMayor)) { Border = Rectangle.NO_BORDER });
                ordenCompraTable2.AddCell(new PdfPCell(new Phrase("Requisición", cellFontMayor)) { Border = Rectangle.NO_BORDER });
                ordenCompraTable2.AddCell(new PdfPCell(new Phrase("Fecha", cellFontMayor)) { Border = Rectangle.NO_BORDER });
                ordenCompraTable2.AddCell(new PdfPCell(new Phrase("Tiempo entrega", cellFontMayor)) { Border = Rectangle.NO_BORDER });

                // Tabla de orden de compra 3
                PdfPTable ordenCompraTable3 = new PdfPTable(1) { WidthPercentage = 100 };
                ordenCompraTable3.AddCell(new PdfPCell(new Phrase(orden.C_compras_cotizaciones_confirmadas_g.C_usuarios_corporativo.C_empleados.nombres + " " + orden.C_compras_cotizaciones_confirmadas_g.C_usuarios_corporativo.C_empleados.apellido_paterno, cellFontMayor)) { Border = Rectangle.NO_BORDER });
                ordenCompraTable3.AddCell(new PdfPCell(new Phrase(orden.C_centros_g.siglas + " " + orden.C_compras_cotizaciones_confirmadas_g.C_compras_requi_g.id_requisicion_articulo_g, cellFontMayor)) { Border = Rectangle.NO_BORDER });
                ordenCompraTable3.AddCell(new PdfPCell(new Phrase(orden.fecha_registro.Value.ToShortDateString(), cellFontMayor)) { Border = Rectangle.NO_BORDER });
                ordenCompraTable3.AddCell(new PdfPCell(new Phrase(orden.C_compras_cotizaciones_confirmadas_g.C_compras_cotizaciones_confirmadas_d.Select(x => x.C_compras_cotizaciones_requisiciones.dias_entrega).Max() + " días", cellFontMayor)) { Border = Rectangle.NO_BORDER });

                //Tabla de orden Union1
                PdfPTable tablaUnion1 = new PdfPTable(2) { WidthPercentage = 100 };
                tablaUnion1.SetWidths(new float[] { 0.5f, 0.5f });
                tablaUnion1.DefaultCell.Border = Rectangle.NO_BORDER;
                tablaUnion1.AddCell(ordenCompraTable2);
                tablaUnion1.AddCell(ordenCompraTable3);

                //Tabla de orden Union2
                PdfPTable tablaUnion2 = new PdfPTable(1) { WidthPercentage = 100 };
                tablaUnion2.DefaultCell.Border = Rectangle.NO_BORDER;
                tablaUnion2.AddCell(ordenCompraTable1);
                tablaUnion2.AddCell(tablaUnion1);



                //
                PdfPTable peticion = new PdfPTable(1) { WidthPercentage = 100 };
                peticion.AddCell(new PdfPCell(new Phrase("ENVIAR A: " + orden.C_compras_ordenes_ubicaciones_entrega.nombre_ubicacion.ToUpper(), cellFontMayorNegra)) { Border = Rectangle.NO_BORDER });
                peticion.AddCell(new PdfPCell(new Phrase("SOLICITA: " + orden.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.nombres.ToUpper() + " " + orden.C_compras_requi_g.C_usuarios_corporativo5.C_empleados.apellido_paterno.ToUpper(), cellFontMayorNegra)) { Border = Rectangle.NO_BORDER });



                //Tabla datos del proveedor
                PdfPTable proveedorTable = new PdfPTable(1) { WidthPercentage = 100 };
                proveedorTable.AddCell(new PdfPCell(new Phrase("DATOS DEL PROVEEDOR", cellFontUnderline)) { Border = Rectangle.NO_BORDER });
                proveedorTable.AddCell(new PdfPCell(new Phrase(orden.C_compras_proveedores.cve + " " + orden.C_compras_proveedores.razon_social, cellFontMayorNegra)) { Border = Rectangle.NO_BORDER });
                proveedorTable.AddCell(new PdfPCell(new Phrase("Moneda " + orden.C_compras_proveedores.C_tipos_moneda.clave_fiscal, cellFontMayor)) { Border = Rectangle.NO_BORDER });
                proveedorTable.AddCell(new PdfPCell(new Phrase("Dirección " + orden.C_compras_proveedores.direccion_1_prov, cellFontMayor)) { Border = Rectangle.NO_BORDER });
                proveedorTable.AddCell(new PdfPCell(new Phrase("Teléfono " + orden.C_compras_proveedores.tel_prov, cellFontMayor)) { Border = Rectangle.NO_BORDER });
                proveedorTable.AddCell(new PdfPCell(new Phrase("Condiciones de pago " + orden.C_compras_proveedores.dias_pago + " DIAS", cellFontMayor)) { Border = Rectangle.NO_BORDER });                


                //Salto de hoja Medio
                PdfPTable tablaMedioSalto = new PdfPTable(1) { WidthPercentage = 70 };
                tablaMedioSalto.AddCell(new PdfPCell(new Phrase("\n", cellFont)) { Border = Rectangle.NO_BORDER });

                // Tabla combinada
                PdfPTable tablaOficial = new PdfPTable(3) { WidthPercentage = 100 };
                tablaOficial.SetWidths(new float[] { 0.45f, 0.1f, 0.45f });
                tablaOficial.AddCell(new PdfPCell(empresaTable) { Border = Rectangle.NO_BORDER });
                tablaOficial.AddCell(new PdfPCell(tablaMedioSalto) { Border = Rectangle.NO_BORDER });
                tablaOficial.AddCell(new PdfPCell(tablaUnion2) { Border = Rectangle.NO_BORDER });
                tablaOficial.AddCell(new PdfPCell(peticion) { Border = Rectangle.NO_BORDER });
                tablaOficial.AddCell(new PdfPCell(tablaMedioSalto) { Border = Rectangle.NO_BORDER });
                tablaOficial.AddCell(new PdfPCell(proveedorTable) { Border = Rectangle.NO_BORDER });
                //document.Add(tablaOficial);


                PdfPTable ENCABEZADO = new PdfPTable(1) { WidthPercentage = 100 };
                ENCABEZADO.DefaultCell.Border = Rectangle.NO_BORDER;
                ENCABEZADO.AddCell(headerTable);
                ENCABEZADO.AddCell(tablaOficial);
                document.Add(ENCABEZADO);

                //Salto de hoja Completo
                PdfPTable tablaSalto = new PdfPTable(1) { WidthPercentage = 100 };
                tablaSalto.AddCell(new PdfPCell(new Phrase("\n", cellFont)) { Border = Rectangle.NO_BORDER });
                tablaSalto.AddCell(new PdfPCell(new Phrase("\n", cellFont)) { Border = Rectangle.NO_BORDER });
                document.Add(tablaSalto);

                // Tabla de detalles de la orden
                PdfPTable detallesTable = new PdfPTable(6) { WidthPercentage = 100 };
                detallesTable.SetWidths(new float[] { 0.07f, 0.55f, 0.07f, 0.12f, 0.07f, 0.12f });
                string[] headers = { "Articulo", "Descripcion", "Unidad", "Precio", "Cant.", "Importe $" };
                foreach (var header in headers)
                {
                    PdfPCell headerCell = new PdfPCell(new Phrase(header, headerFont2)) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER };
                    detallesTable.AddCell(headerCell);
                }

                decimal subTotal = 0;
                string monedaOrden = "";
                foreach (var item in orden.C_compras_ordenes_d.Where(x => x.activo == true).GroupBy(x => x.id_articulo))
                {
                    detallesTable.AddCell(new PdfPCell(new Phrase(item.FirstOrDefault().C_articulos_catalogo.clave, cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                    PdfPTable articulo = new PdfPTable(1) { WidthPercentage = 100 };
                    articulo.AddCell(new PdfPCell(new Phrase(item.FirstOrDefault().C_articulos_catalogo.nombre_articulo, cellFontBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                    //-----VALIDACION PARA COMPARAR REQUI_D CON ORDEN_D (EN CASO DE QUE SE HAYA MODIFICADO EL ARTICULO DE ORDEN_D)
                    var observa_test = item.FirstOrDefault().C_compras_ordenes_g.C_compras_requi_g.C_compras_requi_d.FirstOrDefault(x => x.id_articulo == item.Key);
                    if (observa_test != null)
                    {
                        string observacion = item.FirstOrDefault()?.C_compras_ordenes_g.C_compras_requi_g.C_compras_requi_d.FirstOrDefault(x => x.id_articulo == item.Key)?.observacion?.Trim() ?? "";

                        if (observa_test.observacion.Length < 80)
                        {
                            if (observacion.Length > 0)
                            {
                                articulo.AddCell(new PdfPCell(new Phrase("(" + observacion + ")", cellFontsm))
                                {
                                    Border = Rectangle.NO_BORDER,
                                    HorizontalAlignment = Element.ALIGN_CENTER
                                });
                            }
                        }
                        else
                        {
                            if (observacion.Length > 0)
                            {
                                string cortada = observacion.Substring(0, Math.Min(observacion.Length, 140));
                                articulo.AddCell(new PdfPCell(new Phrase("(" + cortada + ")", cellFontsm))
                                {
                                    Border = Rectangle.NO_BORDER,
                                    HorizontalAlignment = Element.ALIGN_CENTER
                                });
                            }
                        }
                    }
                    else
                    {
                        articulo.AddCell(new PdfPCell(new Phrase(" ", cellFontsm)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                    }
                    var a = item.FirstOrDefault()?.C_compras_ordenes_g.C_compras_requi_g.C_compras_requi_d.FirstOrDefault(x => x.id_articulo == item.Key)?.observacion.Length;
                    detallesTable.AddCell(new PdfPCell(articulo) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                    detallesTable.AddCell(new PdfPCell(new Phrase(item.FirstOrDefault().C_unidades_medidas.unidad_medida, cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                    detallesTable.AddCell(new PdfPCell(new Phrase("$" + item.FirstOrDefault().precio_unitario.Value.ToString("N2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                    detallesTable.AddCell(new PdfPCell(new Phrase(Convert.ToDouble(item.Sum(x => x.cantidad_compra)).ToString(), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                    detallesTable.AddCell(new PdfPCell(new Phrase("$" + item.Sum(x => x.precio_unitario * x.cantidad_compra).Value.ToString("N2"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });

                    subTotal += item.Sum(x => x.precio_unitario * x.cantidad_compra).Value;
                    monedaOrden = item.FirstOrDefault().C_tipos_moneda.clave_fiscal;
                }
                document.Add(detallesTable);

                // Tabla de cantidad en letras y comentarios
                PdfPTable cantidadTable = new PdfPTable(1) { WidthPercentage = 50 };
                COMPRASController comprasController = new COMPRASController();
                string totalEnLetras = comprasController.ConvertirNumeroALetras((decimal)subTotal, monedaOrden);
                cantidadTable.AddCell(new PdfPCell(new Phrase("\n", cellFontMayorNegra)) { Border = Rectangle.NO_BORDER });
                cantidadTable.AddCell(new PdfPCell(new Phrase("Comentarios: " + orden.C_compras_requi_g.concepto, cellFontMayorNegra)) { Border = Rectangle.NO_BORDER });

                // Tabla de subtotal
                PdfPTable totalTable = new PdfPTable(1) { WidthPercentage = 50 };
                totalTable.AddCell(new PdfPCell(new Phrase("\n", cellFontMayorNegra)) { Border = Rectangle.NO_BORDER });
                totalTable.AddCell(new PdfPCell(new Phrase("Subtotal $" + subTotal.ToString("N2") + " " + monedaOrden, cellFontMayorNegra)) { Border = Rectangle.NO_BORDER });

                //Tabla sub y cantidad
                PdfPTable subtotalTable = new PdfPTable(3) { WidthPercentage = 100 };
                subtotalTable.SetWidths(new float[] { 0.45f, 0.1f, 0.45f });
                subtotalTable.DefaultCell.Border = Rectangle.NO_BORDER;
                subtotalTable.AddCell(cantidadTable);
                subtotalTable.AddCell(tablaSalto);
                subtotalTable.AddCell(totalTable);
                document.Add(subtotalTable);

                //Resumen de cargos
                PdfPTable resumenCargosTable = new PdfPTable(2) { WidthPercentage = 5 };
                resumenCargosTable.DefaultCell.Border = Rectangle.NO_BORDER;
                resumenCargosTable.AddCell(new PdfPCell(new Phrase("Importe", headerFont2)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER });
                resumenCargosTable.AddCell(new PdfPCell(new Phrase("Cuenta - Cargo", headerFont2)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER });
                foreach (var item in orden.C_compras_ordenes_d.Where(x => x.activo == true).GroupBy(x => x.id_cuenta_contable))
                {
                    decimal total_cuenta = 0;
                    foreach (var cuenta in item)
                    {
                        total_cuenta += (decimal)cuenta.cantidad_compra * (decimal)cuenta.precio_unitario;
                    }
                    resumenCargosTable.AddCell(new PdfPCell(new Phrase("$" + total_cuenta.ToString("N2"), cellFont)) { Border = Rectangle.NO_BORDER });
                    resumenCargosTable.AddCell(new PdfPCell(new Phrase(item.FirstOrDefault().C_cuentas_contables_g.cuenta + " " + item.FirstOrDefault().C_cargos_contables_g.nombre_cargo, cellFont)) { Border = Rectangle.NO_BORDER });

                }


                // Notas finales
                PdfPTable notasTable = new PdfPTable(1) { WidthPercentage = 5 };
                string[] notas = {
            "* Documentación necesaria al momento de entrega: factura original, 2 copias de factura y orden de compra",
    "* La factura debe indicar el número de orden de compra",
    "* El subtotal de la factura debe coincidir con la orden de compra",
    "* Si no tiene acceso al portal o desconoce, solicite su acceso",
    "* Entrega oficina: Lunes a viernes de 08:30 a 13:00.",
    "* Entrega establos: Lunes a viernes de 08:00 a 15:00."
};

                notasTable.AddCell(new PdfPCell(new Phrase(notas[0], cellFontMayorNegra)) { Border = Rectangle.NO_BORDER });
                notasTable.AddCell(new PdfPCell(new Phrase(notas[1], cellFontMayor)) { Border = Rectangle.NO_BORDER });
                notasTable.AddCell(new PdfPCell(new Phrase(notas[2], cellFontMayor)) { Border = Rectangle.NO_BORDER });
                notasTable.AddCell(new PdfPCell(new Phrase(notas[3], cellFontMayor)) { Border = Rectangle.NO_BORDER });
                notasTable.AddCell(new PdfPCell(new Phrase(notas[4], cellFontMayor)) { Border = Rectangle.NO_BORDER });
                notasTable.AddCell(new PdfPCell(new Phrase(notas[5], cellFontMayor)) { Border = Rectangle.NO_BORDER });


                PdfPTable tablaSaltoFinal = new PdfPTable(1) { WidthPercentage = 100 };
                tablaSaltoFinal.DefaultCell.Border = Rectangle.NO_BORDER;
                tablaSaltoFinal.AddCell(new PdfPCell(new Phrase("\n", cellFont)) { Border = Rectangle.NO_BORDER });
                tablaSaltoFinal.AddCell(new PdfPCell(new Phrase("\n", cellFont)) { Border = Rectangle.NO_BORDER });


                PdfPTable saltos = new PdfPTable(1) { WidthPercentage = 100 };
                saltos.DefaultCell.Border = Rectangle.NO_BORDER;
                saltos.AddCell(new PdfPCell(new Phrase("\n", cellFontMayorNegra)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                document.Add(saltos);

                //Encabezado final
                PdfPTable tituloFinal = new PdfPTable(3) { WidthPercentage = 100 };
                tituloFinal.SetWidths(new float[] { 0.45f, 0.1f, 0.45f });
                tituloFinal.DefaultCell.Border = Rectangle.NO_BORDER;
                tituloFinal.AddCell(new PdfPCell(new Phrase("RESUMEN DE CARGOS", cellFontMayorNegra)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                tituloFinal.AddCell(new PdfPCell(new Phrase("\n", cellFont)) { Border = Rectangle.NO_BORDER });
                tituloFinal.AddCell(new PdfPCell(new Phrase("INDICACIONES", cellFontMayorNegra)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER });
                document.Add(tituloFinal);

                // Tabla final sin bordes
                PdfPTable finalTable = new PdfPTable(3) { WidthPercentage = 100 };
                finalTable.SetWidths(new float[] { 0.45f, 0.1f, 0.45f });
                finalTable.DefaultCell.Border = Rectangle.NO_BORDER;
                finalTable.AddCell(resumenCargosTable);
                finalTable.AddCell(tablaSaltoFinal);
                finalTable.AddCell(notasTable);

                document.Add(finalTable);


                document.Close();
                byte[] pdfBytes = ms.ToArray();
                ms.Close();
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", $"attachment;filename=Orden_Compra_{orden.id_compras_orden_g}.pdf");
                Response.Buffer = true;
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(pdfBytes);
                Response.End();
            }
        }


    }
}