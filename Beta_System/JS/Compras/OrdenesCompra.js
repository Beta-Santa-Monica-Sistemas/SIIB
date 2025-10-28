$('input[type="date"]').val(today);
$('#orden_fecha_inicial_search').val(last_3_day);

function CancelarOrdenCompra(id_orden_g) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 99999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de cancelar la orden de compra?',
        position: 'center',
        buttons: [
            ['<button><b>Si, cancelar orden de compra</b></button>', function (instance, toast) {

                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');


                iziToast.question({
                    timeout: 20000,
                    close: false,
                    overlay: true,
                    displayMode: 'once',
                    id: 'questionSoporte',
                    zindex: 999,
                    title: 'Justificacion',
                    drag: false,
                    message: `
                <div style="margin-top:10px; width: 100%;">
                    <input type="text" id="justificacionInput"
                           style="width: 100%; padding: 10px; font-size: 16px; box-sizing: border-box;"
                           placeholder="Escribe tu justificación del soporte...">
                </div>
                <div style="margin-top: 20px; display: flex; justify-content: center; gap: 20px;">                                 
                    <button id="cancelBtn" style="padding: 10px 20px;" class = "btn btn_beta_danger">Cancelar</button>
                    <button id="confirmBtn" style="padding: 10px 20px;" class = "btn btn_beta"><b>Confirmar</b></button>
                </div>
            `,
                    position: 'center',
                    buttons: [],
                    onOpening: function (instance, toast) {
                        toast.style.width = '60%';
                        $(".iziToast-body").removeAttr("style");
                        $(".iziToast-capsule").css("width", "100%");
                        $(".iziToast-icon").css("display", "none");
                        $("#questionSoporte").css("width", "80%");
                        $(".iziToast-texts").css("width", "100%");
                        $(".iziToast-message").css("width", "100%");

                        setTimeout(() => {
                            document.getElementById('confirmBtn').addEventListener('click', function () {


                                var justificacion_soporte = $("#justificacionInput").val();
                                if (justificacion_soporte.length < 10) {
                                    iziToast.warning({
                                        title: 'Aviso',
                                        message: 'El tamaño minimo de la justificacion es de 10 caracteres',
                                    });
                                    return;
                                }
                                iziToast.hide({}, toast);
                                jsShowWindowLoad();
                                $.ajax({
                                    type: "POST",
                                    async: true,
                                    url: "../ORDENES_COMPRA/CancelarOrdenCompra",
                                    data: {
                                        id_orden_g: id_orden_g
                                    },
                                    success: function (response) {
                                        jsRemoveWindowLoad();
                                        if (response == 1) {
                                            iziToast.warning({
                                                title: 'Aviso',
                                                message: "No se puede cancelar la orden de compra, ya se recepciono un articulo",
                                            });
                                        }
                                        else if (response == 2) {
                                            iziToast.success({
                                                title: 'Exito',
                                                message: "Se cancelo correctamente la orden de compra",
                                            });
                                            //$("#m_orden_compra_detalle").modal("hide");
                                            BuscarInformacionRequisicionUtileria();
                                        }
                                        else if (response == 3) {
                                            iziToast.error({
                                                title: 'Aviso',
                                                message: "Ocurrio un fallo al cancelar la orden de compra, favor de intentarlo en otro momento",
                                            });
                                        }
                                    },
                                    error: function (xhr, status, error) {
                                        console.error(error);
                                        jsRemoveWindowLoad();
                                    }
                                });



                            });

                            document.getElementById('cancelBtn').addEventListener('click', function () {
                                iziToast.warning({
                                    title: 'Aviso',
                                    message: 'Soporte no realizado',
                                });
                                iziToast.hide({}, toast);
                            });
                        }, 100);
                    },
                    onClosing: function (instance, toast, closedBy) { },
                    onClosed: function (instance, toast, closedBy) { }
                });



            }, true],
            ['<button>No, cancelar operación</button>', function (instance, toast) {

                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');

            }],
        ],
        onClosing: function (instance, toast, closedBy) {
            console.info('Closing | closedBy: ' + closedBy);
        },
        onClosed: function (instance, toast, closedBy) {
            console.info('Closed | closedBy: ' + closedBy);
        }
    });










    //var estatus = $("#status_orden").text();
    //if (estatus == 1) {


    //}
    //else if (estatus == 2) {
    //    iziToast.warning({
    //        title: 'Aviso',
    //        message: "No se puede cancelar una orden de compra importada",
    //    });
    //}
    //else {
    //    iziToast.warning({
    //        title: 'Aviso',
    //        message: "No se puede cancelar una orden de compra cancelada",
    //    });
    //}
}
function ConsultarOrdenesCompra() {
    var fecha_inicio = $("#orden_fecha_inicial_search").val();
    var fecha_fin = $("#orden_fecha_final_search").val();
    var centro = $("#id_centro_search").val();
    var tipo_orden = $("#id_tipo_orden_search").val();
    var proveedor = $("#id_proveedor_search").val();

    var recepcionada = $('input[name="radio_recepcion_orden"]:checked').val();

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ORDENES_COMPRA/ConsultarOrdenesCompra",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            centro: centro,
            tipo_orden: tipo_orden,
            proveedor: proveedor,
            recepcionada: recepcionada
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_ordenes_compra_general").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_ordenes_compra_general").html(response);
            $('#datatable_ordenes_compra_general').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{
                    extend: "copy",
                    className: "btn-sm",
                    columns: ':visible'
                }, {
                    extend: "csv",
                    className: "btn-sm"
                }, {
                    extend: "excel",
                    className: "btn-sm"
                }, {
                    extend: "pdf",
                    className: "btn-sm"
                }, {
                    extend: "print",
                    className: "btn-sm"
                }],
                responsive: false,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarOrdenCompraDetalle(id_orden_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ORDENES_COMPRA/ConsultarOrdenCompraDetalle",
        data: { id_orden_g: id_orden_g },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#m_orden_compra_detalle").modal("show");

            try {
                $("#datatable_orden_compra_detalle").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_ordenes_compra_detalle").html(response);
            $('#datatable_orden_compra_detalle').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function GenerarOrdenCompraPDF(id_orden_g, token_orden) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../PORTAL/GenerarOrdenCompraPDF",
        data: {
            id_orden_g: id_orden_g,
            token_orden: token_orden
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_pdf_orden_compra").css("display", "block");
            $("#div_pdf_orden_compra").html(response);
            var HTML_Width = $("#div_pdf_orden_compra").width();
            var HTML_Height = $("#div_pdf_orden_compra").height();
            var top_left_margin = 15;
            var PDF_Width = HTML_Width + (top_left_margin * 2);
            var PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
            var canvas_image_width = HTML_Width;
            var canvas_image_height = HTML_Height;

            var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;

            html2canvas($("#div_pdf_orden_compra")[0]).then(function (canvas) {
                var imgData = canvas.toDataURL("image/jpeg", 1.0);
                var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
                pdf.addImage(imgData, 'JPG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
                for (var i = 1; i <= totalPDFPages; i++) {
                    pdf.addPage(PDF_Width, PDF_Height);
                    pdf.addImage(imgData, 'JPG', top_left_margin, -(PDF_Height * i) + (top_left_margin * 4), canvas_image_width, canvas_image_height);
                }
                pdf.save("Orden de compra: " + id_orden_g + ".pdf");
            });
            $("#div_pdf_orden_compra").css("display", "none");

            //html2pdf()
            //    .from(response)
            //    .set({
            //        margin: [15, 15, 15, 15],
            //        filename: "Prenomina.pdf",
            //        image: { type: 'png', quality: 2 },
            //        html2canvas: { scale: 1.1 },
            //        jsPDF: { unit: 'pt', format: [612, 936], orientation: 'A4' }, // Oficio: 8.5 x 13 pulgadas
            //        pagebreak: { mode: ['avoid-all', 'css', 'legacy'] }
            //    })
            //    .save()
            //    .then(function () {
            //        jsRemoveWindowLoad();
            //    });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function MostrarFormularioReenviarOrdenCompra(id_orden_g, token_orden, correo_default, folio_orden) {
    $("#correo_reenviar_orden").val(correo_default);
    $("#btn_reenviar_orden").attr("onclick", "ReenviarOrdenCompra(" + id_orden_g + ",'" + token_orden + "','" + folio_orden + "')");
    $("#m_reenviar_orden_compra").modal("show");
}

function ReenviarOrdenCompra(id_orden_g, token_orden, folio_orden) {
    var correo_destino = $("#correo_reenviar_orden").val();
    if (correo_destino == "") {
        iziToast.warning({
            title: 'Ingrese el correo de destino',
            message: '',
        });
        return false;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ORDENES_COMPRA/ReenviarOrdenCompra",
        data: {
            id_orden_g: id_orden_g,
            token_orden: token_orden,
            correo_destino: correo_destino,
            folio_orden: folio_orden
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "True") {
                iziToast.success({
                    title: 'Orden de compra reenviada correctamente!',
                    message: "Destino: " + correo_destino + "",
                });
                $("#m_reenviar_orden_compra").modal("hide");
                $("#correo_reenviar_orden").val("");
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al reenviar la orden de compra',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}



//--------------------- RECEPCION DE ORDENES DE COMPRA DE OFICNA
function ConsultarOrdenesPendientesRecepcion() {
    var id_centros = $("#id_centro_recepcion").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../COMPRAS/ConsultarOrdenesPendientesRecepcion",
        data: { id_centros: id_centros },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_ordenes_pendientes_recepcion").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_ordenes_pendientes_recepcion").html(response);
            $('#datatable_ordenes_pendientes_recepcion').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                select: true,
                pageLength: 20,
                scrollY: false,
                scrollX: true
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarDetalleOrdenPendienteRecepcion(id_orden_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../COMPRAS/ConsultarDetalleOrdenPendienteRecepcion",
        data: { id_orden_g: id_orden_g },
        success: function (response) {
            $("#btn_recepcionar_orden").attr("onclick", "RecepcionarOrdenCompra(" + id_orden_g + ")");
            $("#div_detalle_orden_recepcion").html(response);
            $("#m_detalle_orden_recepcion").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function HabilitarRecepcionArticulo(id_orden_d, pendiente) {
    var check = $("#art_" + id_orden_d + "");
    if (check.is(':checked')) {
        var input = $("#art_cantidad_" + id_orden_d + "");
        input.val(pendiente);
        input.removeAttr("disabled");
    }
    else {
        var input = $("#art_cantidad_" + id_orden_d + "");
        input.val('');
        input.attr("disabled", "true");
    }
}

function RecepcionarOrdenCompra(id_orden_g) {
    var folio_factura_remision = $("#factura_folio").val();

    var id_orden_d = [];
    var cantidades = [];
    var count = 0;

    var valid_cantidad = true;
    var valid_pendiente = true;
    $(".articulos_recepcion").each(function () {
        if ($(this).is(':checked')) {
            var id_articulo_d = $(this).attr("id").split('_')[1];
            var cantidad = $("#art_cantidad_" + id_articulo_d + "").val();
            var pendiente = parseFloat($("#pendiente_" + id_articulo_d + "").text());
            if (cantidad == undefined || cantidad == "" || isNaN(cantidad) || cantidad <= 0) { valid_cantidad = false; }
            else if (pendiente < cantidad) { valid_pendiente = false; }
            else {
                cantidades[count] = cantidad;
                id_orden_d[count] = id_articulo_d;
                count++;
            }
        }
    });

    if (valid_cantidad == false) {
        iziToast.warning({
            title: '',
            message: 'Ingrese cantidades validas',
        });
    }
    else if (valid_pendiente == false) {
        iziToast.warning({
            title: '',
            message: 'Ingresa cantidades menores a lo pendiente de entregar',
        });
    }
    else if (id_orden_d.length <= 0) {
        iziToast.warning({
            title: 'Seleccione al menos 1 articulo para recepcionar',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../COMPRAS/RecepcionarOrdenCompra",
            data: {
                id_orden_g: id_orden_g,
                id_orden_d: id_orden_d,
                cantidad: cantidades,
                folio_factura_remision: folio_factura_remision
            },
            success: function (response) {
                jsRemoveWindowLoad();
                $("#m_detalle_orden_recepcion").modal("hide");
                ConsultarOrdenesPendientesRecepcion();
                if (response == 0) {
                    iziToast.success({
                        title: 'Recepción realizada correctamente',
                        message: '',
                    });
                }
                else if (response == 1) {
                    iziToast.error({
                        title: 'Error',
                        message: 'Orden ya recepcionada',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
}






