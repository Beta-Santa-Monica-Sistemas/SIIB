//#region CONFIGURACIÓN DE ARTICULOS / INGREDIENTES
function ConsultarArticulosIngredientesEstablo() {
    var id_establo = $("#id_establo").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../CATALOGOS/ConsultarArticulosIngredientesEstablo",
        data: { id_establo: id_establo },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#select2-id_articulo_ing-container").text("Selecciona un ingrediente");
            $(".select_multiple").val('');
            $('.select_multiple').multiselect('destroy');
            $(".select_ingredientes").html(response);
            $('.select_multiple').multiselect({
                nonSelectedText: 'Selecciona una opción',
                includeSelectAllOption: true,
                buttonWidth: '100%',
                selectAllText: 'Todas'
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function SeleccionarArticuloAsociacion(id_articulo) {
    id_articulo_asociacion = id_articulo;

    $(".rows_articulos").css("background-color", "");
    $("#row_art_" + id_articulo + "").css("background-color", "rgb(153, 255, 204");
}

function SeleccionarForrajesAsociacion(id_ing) {
    var check = $("#ing_check_" + id_ing + "");
    var row = $("#row_ing_" + id_ing + "");
    if (check.is(":checked")) {
        row.css("background-color", "rgb(153, 255, 204");
    }
    else {
        row.css("background-color", "");
    }
}

function ConsultarArticulosForrajesTable() {
    var id_establo = $("#id_establo_ing").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../TRACKER/ConsultarArticulosForrajesTable",
        data: { id_establo: id_establo },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_articulos_forraje").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarArticulosTrackerTable() {
    jsShowWindowLoad();
    var id_establo = $("#id_establo_ing").val();
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../TRACKER/ConsultarArticulosTrackerTable",
        data: { id_establo: id_establo },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_ingredientes_tracker").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarArticulosIngredientesTrackerTable() {
    jsShowWindowLoad();
    var id_establo = $("#id_establo_ing").val();
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../TRACKER/ConsultarArticulosIngredientesTrackerTable",
        data: { id_establo: id_establo },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_ingredientes_articulos").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function EliminarAsociacionArticuloIngrediente(id_ingrediente_articulo) {
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../TRACKER/EliminarAsociacionArticuloIngrediente",
        data: { id_ingrediente_articulo: id_ingrediente_articulo },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Asociación eliminada correctamente',
                    message: '',
                });
                ConsultarArticulosTrackerTable();
                ConsultarArticulosIngredientesTrackerTable();
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un error al eliminar la asociación',
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

function AsociarArticuloIngrediente() {
    if (id_articulo_asociacion == 0 || isNaN(id_articulo_asociacion) || id_articulo_asociacion == undefined) {
        iziToast.error({
            title: 'Selecciona un articulo para asociar',
            message: '',
        });
        return;
    }

    var id_ingredientes = [];
    var count = 0;
    $(".checks_ingredientes:checked").each(function () {
        id_ingredientes[count] = $(this).val();
        count++;
    });

    if (id_ingredientes.length <= 0) {
        iziToast.error({
            title: 'Selecciona al menos un ingrediente para asociar',
            message: '',
        });
        return;
    }

    jsShowWindowLoad();
    var id_establo = $("#id_establo_ing").val();
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../TRACKER/AsociarArticuloIngrediente",
        data: {
            id_articulo: id_articulo_asociacion,
            id_ingredientes: id_ingredientes,
            id_establo: id_establo
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == 0) {
                iziToast.success({
                    title: 'Ingredientes y articulo asociados correctamente',
                    message: '',
                });
                ConsultarArticulosTrackerTable();
                ConsultarArticulosIngredientesTrackerTable();
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un error al asociar',
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

//#endregion

//#region  CENTRALIZADOR
function simulateProgress() {
    let width = 0;
    const bar = document.getElementById("progress-bar");
    const interval = setInterval(() => {
        if (width >= 70) {
            clearInterval(interval); // Detén cuando esté al 90%
        } else {
            width += 2;
            bar.style.width = width + "%";
        }
    }, 100);
    return interval;
}
function CentralizarInformacionTrackerEstablo() {
    var id_establo = $("#id_establo").val();
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 10000000); // 5 seg

    const datos = {
        id_establo: id_establo,
        fecha_inicio: fecha_inicio,
        fecha_fin: fecha_fin
    };

    jsShowWindowLoad();
    const progressInterval = simulateProgress();
    document.getElementById("progress-bar").style.background = "#1ef134";
    fetch('/TRACKER/CentralizarInformacionTrackerEstablo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(datos),
        signal: controller.signal
    })
    .then(res => res.json())
    .then(data => {
        jsRemoveWindowLoad();
        if (data == 0) {
            clearTimeout(timeoutId);
            clearInterval(progressInterval);
            document.getElementById("progress-bar").style.width = "100%";
            iziToast.success({
                title: 'Información centralizada correctamente',
                message: '',
            });
        } else {
            clearInterval(progressInterval);
            document.getElementById("progress-bar").style.background = "red";
            iziToast.error({
                title: 'Ocurrió un error al centralizar la información',
                message: '',
            });
        }
    })
    .catch(err => {
        jsRemoveWindowLoad();
        clearInterval(progressInterval);
        document.getElementById("progress-bar").style.background = "red";

        if (err.name === 'AbortError') {
            iziToast.error({
                title: 'Tiempo de espera agotado',
                message: '',
            });
        } else {
            iziToast.error({
                title: 'Ocurrió un error al centralizar la información',
                message: '',
            });
        }
    });

    //jsShowWindowLoad();
    //$.ajax({
    //    type: "POST",
    //    async: false,
    //    timeout: 90000000,
    //    url: "../TRACKER/CentralizarInformacionTrackerEstablo",
    //    data: {
    //        id_establo: id_establo,
    //        fecha_inicio: fecha_inicio,
    //        fecha_fin: fecha_fin
    //    },
    //    success: function (response) {
    //        jsRemoveWindowLoad();
    //        if (response == 0) {
    //            iziToast.success({
    //                title: 'Información centralizada correctamente',
    //                message: '',
    //            });
    //        }
    //        else {
    //            iziToast.error({
    //                title: 'Ocurrió un error al centralizar la información',
    //                message: '',
    //            });
    //        }
    //    },
    //    error: function (xhr, status, error) {
    //        console.error(error);
    //        jsRemoveWindowLoad();
    //    }
    //});

}

//#endregion


// #region  MOVIMIENTOS TRACKER
function ConsultarMovimientosTrackerTable() {
    jsShowWindowLoad();
    var id_establo = $("#id_establo").val();
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();
    var id_tipo_mov = $("#id_tipo_mov").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../TRACKER/ConsultarMovimientosTrackerTable",
        data: {
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin,
            id_establo: id_establo,
            id_tipo_mov: id_tipo_mov

        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#dataable_movimientos_tracker").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_movimientos_tracker").html(response);
            $('#dataable_movimientos_tracker').DataTable({
                ordering: true,
                searching: true,
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
                dom: "Bfrtip",
                select: true,
                responsive: false,
                pagingType: 'simple_numbers',
                pageLength: 20
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}



//#endregion


//#region KARDEX INGREDENTES

function ShowHideIngredientes() {
    var tipo_consulta = $('input[name="tipo_consulta"]:checked').val();
    $("#div_articulo_ing_multiple").css("display", "none");
    $("#div_articulo_ing").css("display", "none");
    if (tipo_consulta == 3) {
        $("#div_articulo_ing_multiple").css("display", "block");
    }
    else {
        $("#div_articulo_ing").css("display", "block");
    }
}

function ConsultarKardexIngredientes() {
    var tipo_consulta = $('input[name="tipo_consulta"]:checked').val();
    var id_establo = $("#id_establo").val();
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();
    var id_articulo = $("#id_articulo_ing").val();
    if (tipo_consulta == 3) {
        id_articulo = $("#id_articulo_ing_multiple").val();
    }

    if (id_articulo == undefined || id_articulo == "" || fecha_fin == "" || fecha_fin == "" || id_establo == undefined || tipo_consulta == undefined) {
        iziToast.error({
            title: 'Ingrese todos los parámetros',
            message: '',
        });
        return;
    }
    jsShowWindowLoad();
    if (tipo_consulta == 1) {
        $.ajax({
            type: "POST",
            async: true,
            timeout: 900000,
            url: "../TRACKER/ConsultarKardexIngredientesFecha",
            data: {
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin,
                id_establo: id_establo,
                id_articulo: id_articulo

            },
            success: function (response) {
                jsRemoveWindowLoad();
                try {
                    $("#datatable_kardex_ingredientes").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_kardex_ingredientes").html(response);
                $('#datatable_kardex_ingredientes').DataTable({
                    ordering: false,
                    searching: true,
                    paging: false,
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
                    dom: "Bfrtip",
                    select: true,
                    responsive: false,
                    pagingType: 'simple_numbers',
                    pageLength: 20
                });

            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }

    else if (tipo_consulta == 2) {
        $.ajax({
            type: "POST",
            async: true,
            timeout: 900000,
            url: "../TRACKER/ConsultarKardexIngredientesDetallado",
            data: {
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin,
                id_establo: id_establo,
                id_articulo: id_articulo

            },
            success: function (response) {
                jsRemoveWindowLoad();
                try {
                    $("#datatable_kardex_detalle").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_kardex_ingredientes").html(response);
                $('#datatable_kardex_detalle').DataTable({
                    ordering: false,
                    searching: true,
                    paging: false,
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
                    dom: "Bfrtip",
                    select: true,
                    responsive: false
                });

            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }

    else if (tipo_consulta == 3) {
        $.ajax({
            type: "POST",
            async: true,
            timeout: 900000,
            url: "../TRACKER/ConsultarKardexIngredientesResumen",
            data: {
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin,
                id_establo: id_establo,
                id_articulo: id_articulo

            },
            success: function (response) {
                jsRemoveWindowLoad();
                try {
                    $("#datatable_kardex_ingredientes_resumen").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_kardex_ingredientes").html(response);
                $('#datatable_kardex_ingredientes_resumen').DataTable({
                    ordering: false,
                    searching: true,
                    paging: false,
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
                    dom: "Bfrtip",
                    select: true,
                    responsive: false
                });

            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }

}

//#endregion


//#region TIRADAS DIARIAS
function ConsultarTiradasDiarias() {
    jsShowWindowLoad();
    var id_establo = $("#id_establo").val();
    var fecha = $("#fecha_tirada").val();
    $.ajax({
        type: "POST",
        async: false,
        timeout: 900000,
        url: "../TRACKER/ConsultarTiradasDiarias",
        data: {
            id_establo: id_establo,
            fecha: fecha
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_tiradas_diarias").dataTable().fnDestroy();
                $("#datatable_prepaacion_mezclas").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tiradas_diarias").html(response);
            $('#datatable_tiradas_diarias').DataTable({
                ordering: true,
                searching: true,
                paging: false,
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
                dom: "Bfrtip",
                select: true,
                responsive: false
            });
            $('#datatable_prepaacion_mezclas').DataTable({
                ordering: true,
                searching: true,
                paging: false,
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
                dom: "Bfrtip",
                select: true,
                responsive: false
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


//#endregion



//#region CONSUMOS X CORRAL
function ConsultarConsumosXCorralTracker() {
    jsShowWindowLoad();
    var id_establo = $("#id_establo").val();
    var fecha_inicio = $("#fecha_inicio").val();
    var fecha_fin = $("#fecha_fin").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../TRACKER/ConsultarConsumosXCorralTracker",
        data: {
            id_establo: id_establo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin

        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_consumos_corral").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_datatable_consumos_corral").html(response);
            $('#datatable_consumos_corral').DataTable({
                ordering: true,
                searching: true,
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
                dom: "Bfrtip",
                select: true,
                responsive: false,
                pagingType: 'simple_numbers',
                pageLength: 20
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


//#endregion
