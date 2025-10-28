function ConsultarPreciosArticulosForrajesTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../CONFIGURACION/ConsultarPreciosArticulosForrajesTable",
        data: {},
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_precios_forraje").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_precios_forraje").html(response);
            $('#datatable_precios_forraje').DataTable({
                keys: false,
                ordering: true,
                searching: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [],
                responsive: false,
                select: false
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function GuardarPreciosArticulosForrajes() {
    var id_articulos = [];
    var precios = [];
    var count = 0;
    var valid_negativos = false;
    $(".id_articulos_forrajes_precios").css("border", "");
    $(".id_articulos_forrajes_precios").each(function () {
        var id_articulo = $(this).attr("id").split("_")[1];
        id_articulos[count] = id_articulo;

        var precio = $("#ArtForrajePrecio_" + id_articulo + "").val();
        if (precio == "" || precio == undefined) { precio = 0; }
        if (precio <= 0) { valid_negativos = true; $("#ArtForrajePrecio_" + id_articulo + "").css("border", "1px solid red"); }
        precios[count] = precio;
        count++;
    });
    if (id_articulos.length == 0 || precios.length == 0) {
        iziToast.warning({
            title: 'No se detectaron ingredientes a actualizar',
            message: '',
        });
    }
    //else if (valid_negativos == true) {
    //    iziToast.warning({
    //        title: 'Asegurese de ingresar solo valores positivos',
    //        message: '',
    //    });
    //}
    else {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 999,
            title: 'ADVERTENCIA',
            message: '¿Está seguro de actualizar los precios?',
            position: 'center',
            buttons: [
                ['<button><b>Si, actualizar ahora</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        timeout: 900000,
                        url: "../CONFIGURACION/GuardarPreciosArticulosForrajes",
                        data: {
                            id_articulos: id_articulos,
                            precios: precios
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                iziToast.success({
                                    title: 'Precios actualizados correctamente',
                                    message: '',
                                });
                                ConsultarPreciosArticulosForrajesTable();
                            }
                            else if (response == -1) {
                                iziToast.error({
                                    title: 'Ocurrió un error al actualizar los precios',
                                    message: '',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                            jsRemoveWindowLoad();
                        }
                    });
                }, true],
                ['<button>No, regresar</button>', function (instance, toast) {
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
    }
}

