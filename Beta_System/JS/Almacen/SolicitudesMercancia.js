$('#fecha_inicio_admin').val(last_week);
$('#fecha_fin_admin').val(today);


function ConsultarCuentasCargosUsuarioSelect() {
    var id_cargo_contable = $("#select_cargo").val();
    if (id_cargo_contable == "0" || id_cargo_contable == "select_cargo") {
        $("#select_cuenta").html('');
        return;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/ConsultarCuentasCargosUsuarioSelect",
        data: {
            id_cargo_contable: id_cargo_contable
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#select_cuenta").html(response);

            ConsultarCuentasContableSelect();
            $("#select_cargo").focus();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarCuentasContablesCargosSelect() {
    var id_cargo_contable = $("#select_cargo").val();
    if (id_cargo_contable == "0" || id_cargo_contable == "select_cargo") { return; }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/ConsultarCuentasContablesCargosSelect",
        data: {
            id_cargo_contable: id_cargo_contable
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#select_cuenta").html(response);
            ConsultarCuentasContableSelect();
            $("#select_cargo").focus();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarCuentasContableSelect() {
    var id_cuenta = $("#select_cuenta").val();
    if ($("#select_cuenta option").length == 0) {
        return;
    }

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/ConsultarCuentasContableSelects",
        data: {
            id_cuentas_contable: id_cuenta
        },
        success: function (response) {
            var data = $.parseJSON(response);
            $("#cuentas").text(data[0].cuenta);
            $("#id_cuentas").val(data[0].cuenta);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarUbicacionesAlmacen() {
    var id_almacen = $("#id_almacen").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/AlmacenUbicacionArticuloSelectCargado",
        data: {
            id_almacen: id_almacen
        },
        success: function (response) {
            var data = $.parseJSON(response);
            var ubicacionSelect = $("#id_ubicacion");
            ubicacionSelect.empty();
            data.forEach(function (item) {
                var option = $('<option></option>').attr("value", item.id_ubicacion_almacen).text(item.nombre_ubicacion);
                ubicacionSelect.append(option);
            });

            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function SwitchPrecio() {
    var input_precio = $("#precio_articulo");
    var check = $("#check_nuevo_precio");
    if (check.prop("checked") == true) {
        input_precio.removeAttr("readonly");
    }
    else {
        input_precio.attr("readonly", "true");
    }
}

function CalcularImporteArticulo() {
    var precio = $("#precio_articulo").val().replace(",","");
    var cantidad = $("#cantidad_articulo").val();
    var importe = precio * cantidad;
    var importeMX = importe.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' });
    //$("#importe_articulo").val(importe.toFixed(2));
    $("#importe_articulo").val(importeMX);
    $("#cantidad_articulo").css("border-color", "");
}

function CalcularTotalImportes() {
    var importes = $(".importes");
    var total = 0;
    importes.each(function () {
        total = total + parseFloat($(this).text().replace('$', '').replace(/,/g, ''));
    });

    var total_final = total.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' });
    $("#total_importe").val(total_final);
}



function EliminarArticuloSolicitud(row) {
    $("#row_" + row + "").remove();
    CalcularTotalImportes();
}


function GenerarSolicitudMercancia() {
    $(".valid_input").css("border-color", "#ced4da");

    var id_empleado_remitente = $("#select_solicitante").val();

    var id_productor = $("#select_productor").val();
    if ($("#concepto").val() == "") {
        iziToast.warning({
            title: 'Ingresa el concepto',
            message: '',
        });
        $("#concepto").css("border-color", "red");
    }
    else if ($('#articulos_table_solicitud tr').length == 1) {
        iziToast.warning({
            title: 'Agrega articulos a la solicitud',
            message: '',
        });
    }
    else if (id_empleado_remitente == "" || id_empleado_remitente == undefined) {
        iziToast.warning({
            title: 'Ingrese el solicitante del vale',
            message: '',
        });
    }
    else {
        var c_solicitud_g = {};
        c_solicitud_g.id_centro_g = $("#select_centro").val();
        c_solicitud_g.id_departamento_g = $("#select_departamento").val();
        c_solicitud_g.id_almacen_g = $("#select_almacen").val();
        //c_solicitud_g.id_empleado_solicita = $("#select_solicitante").val();
        c_solicitud_g.id_establo_equipo = $("#select_equipo").val();
        c_solicitud_g.cargo_desc = $("#concepto").val();
        c_solicitud_g.id_empleado_remitente = id_empleado_remitente;

        var total_importe_g = $("#total_importe").val();
        var total_importe_d = parseFloat(total_importe_g.replace('$', '').replace(/,/g, ''));
        c_solicitud_g.total = total_importe_d;

        var count = 0;
        var cargos_contables = [];
        var id_articulos = [];
        var id_unidades = [];
        var cantidades = [];
        var precios = [];
        var importes = [];
        var cargos = [];
        var cuentas = [];
        var obs = [];

        $(".cargos_contables").each(function () {
            cargos_contables[count] = $(this).text(); count++;
        });
        count = 0;
        $(".id_articulo_merca").each(function () {
            id_articulos[count] = $(this).text(); count++;
        });
        count = 0;
        $(".unidades_medida").each(function () {
            id_unidades[count] = $(this).text(); count++;
        });
        count = 0;
        $(".cantidades").each(function () {
            cantidades[count] = $(this).text(); count++;
        });
        count = 0;
        $(".precios").each(function () {
            var precios_s = $(this).text();
            var precio = parseFloat(precios_s.replace('$', '').replace(/,/g, ''));
            precios[count] = precio; count++;
        });
        count = 0;
        $(".importes").each(function () {
            var importes_g = $(this).text();
            var importes_d = parseFloat(importes_g.replace('$', '').replace(/,/g, ''));
            importes[count] = importes_d; count++;
        });
        count = 0;
        $(".id_cuentas").each(function () {
            cuentas[count] = $(this).text(); count++;
        });
        count = 0;
        $(".id_cargos").each(function () {
            cargos[count] = $(this).text(); count++;
        });
        count = 0;
        $(".observaciones").each(function () {
            obs[count] = $(this).text(); count++;
        });

        $.ajax({
            type: "POST",
            async: false,
            url: "../ALMACEN/GenerarSolicitudMercancia",
            data: {
                c_solicitud_g: c_solicitud_g,
                id_productor: id_productor
            },
            success: function (response) {
                var id_solicitud_g = response;
                if (id_solicitud_g > 0) {
                    $.ajax({
                        type: "POST",
                        async: false,
                        url: "../ALMACEN/GenerarSolicitudMercanciaDetalle",
                        data: {
                            id_solicitud_g: id_solicitud_g,
                            id_articulos: id_articulos,
                            id_unidades: id_unidades,
                            cantidades: cantidades,
                            precios: precios,
                            importes: importes,
                            cargos: cargos,
                            cuentas: cuentas,
                            obs: obs,
                            cargos_contables: cargos_contables
                        },
                        success: function (response) {
                            if (response == "True") {
                                iziToast.success({
                                    title: "Solicitud generada correctamente con el folio: #" + id_solicitud_g + " !",
                                    message: '',
                                    timeout: false
                                });
                                $("#observaciones_articulo").val('');
                                $("#cantidad_articulo").val('1');
                                $("#concepto").val('');
                                $("#tbody_solicitud_articulos").html('');
                                $("#total_importe").val('');
                                ConsultarSolicitudesTable();
                                $("#tab_solicitudes").click();

                                $("#id_articulos").val('');
                                $("#precio_articulo").val('0.00');
                                $("#cantidad_articulo").val('1');
                                $("#importe_articulo").val('0.00');
                            }
                            else {
                                iziToast.error({
                                    title: 'Error',
                                    message: 'Ocurrió un problema al registrar el detalle de la solicitud',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                        }
                    });
                }
                else if (id_solicitud_g == -1) {
                    iziToast.info({
                        title: 'Presiona las teclas:',
                        message: 'Ctrl + Shift + R',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al registrar el encabezado de la solicitud',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
}

function ConsultarSolicitudesTable() {
    var id_estatus = $("#id_status_solicitudes_admin").val();
    var fecha_inicio = $("#fecha_inicio_admin").val();
    var fecha_fin = $("#fecha_fin_admin").val();
    var folio = $("#folio_admin").val();
    var id_almacen = $("#id_almacen_admin").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarSolicitudesTable",
        data: {
            id_estatus: id_estatus,
            fecha_fin: fecha_fin,
            fecha_inicio: fecha_inicio,
            folio: folio,
            id_almacen: id_almacen
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_solicitudes_admin").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_solicitudes_admin").html(response);
            $('#tabla_solicitudes_admin').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-MX.json',
                },
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

function ConsultarSolicitudDetalle(id_solicitud_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarSolicitudDetalle",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#tabla_solicitudes_detalle").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_detalle_solicitud").html(response);
            $('#tabla_solicitudes_detalle').DataTable({
                keys: true,
                ordering: true,
                searching: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{
                    extend: "",
                }],
                responsive: !0
            });

            ConsultarCambiosArticuloSolicitud(id_solicitud_g);
            $("#m_detalle_solicitud").modal("show");
            $("#btn_entregar_vale").css("display", "block");

            $(".divEditable").keypress(function (e) {
                if (isNaN(String.fromCharCode(e.which))) e.preventDefault();
            });

            $("#m_detalle_solicitud").modal("show");
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarCambiosArticuloSolicitud(id_solicitud_g) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ALMACEN/ConsultarCambiosArticuloSolicitud",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (response) {
            try {
                $("#tabla_cambios_articulos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_cambios_solicitud").html(response);
            $('#tabla_cambios_articulos').DataTable({
                keys: true,
                ordering: true,
                searching: false,
                paging: false,
                dom: "Bfrtip",
                buttons: [{
                    extend: "",
                }],
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function AutorizarSolicitud(id_solicitud_g) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/AutorizarSolicitud",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == "True") {
                iziToast.success({
                    title: 'Solicitud autorizada correctamente',
                    message: '',
                });
                ConsultarSolicitudesTable();
                $("#m_detalle_solicitud").modal("hide");
            }
            else {
                iziToast.error({
                    title: 'No se pudo autorizar la solicitud',
                    message: '',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function GenerarValePDF(id_solicitud_g, id_div) {
    jsShowWindowLoad();
    $("#" + id_div +"").css("display", "block");
    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/GenerarValePDF",
        data: {
            id_solicitud_g: id_solicitud_g
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#" + id_div +"").html(response);
            var HTML_Width = $("#" + id_div +"").width();
            var HTML_Height = $("#" + id_div +"").height();
            var top_left_margin = 15;
            var PDF_Width = HTML_Width + (top_left_margin * 2);
            var PDF_Height = (PDF_Width * 1.5) + (top_left_margin * 2);
            var canvas_image_width = HTML_Width;
            var canvas_image_height = HTML_Height;

            var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;

            html2canvas($("#" + id_div +"")[0]).then(function (canvas) {
                var imgData = canvas.toDataURL("image/png", 1.0);
                var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
                pdf.addImage(imgData, 'PNG', top_left_margin, top_left_margin, canvas_image_width, canvas_image_height);
                for (var i = 1; i <= totalPDFPages; i++) {
                    pdf.addPage(PDF_Width, PDF_Height);
                    pdf.addImage(imgData, 'PNG', top_left_margin, -(PDF_Height * i) + (top_left_margin * 4), canvas_image_width, canvas_image_height);
                }
                pdf.save("Solicitud Mercancia.pdf");
            });
            $("#" + id_div +"").css("display", "none");
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarExistenciaArticuloAlmacen() {
    var id_almacen_g = $("#select_almacen").val();
    var id_articulo = $("#codigo_articulo").text();

    $.ajax({
        type: "POST",
        async: true,
        url: "../ALMACEN/ConsultarExistenciaArticuloAlmacen",
        data: {
            id_almacen_g: id_almacen_g,
            id_articulo: id_articulo
        },
        success: function (response) {
            $("#existencia_articulo").val(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
