function ConsultarInventarioFecha() {
    var id_almacen_g = $("#id_almacen_g_inv_cons").val();
    var articulo = $("#id_articulo_existencia").val();
    var fecha = $("#fecha_inventario_cons").val();
    if (articulo != '' && articulo != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../UTILERIAS/ExistenciaAlmacenReciente",
            data: {
                id_almacen_g: id_almacen_g,
                articulo: articulo,
                fecha: fecha
            },
            success: function (response) {
                jsRemoveWindowLoad();
                try {
                    $("#tabla_inventario_fecha_cons").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_inventario_fecha_cons").html(response);
                $('#tabla_inventario_fecha_cons').DataTable({
                    keys: false,
                    ordering: true,
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
                    responsive: !0
                });
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar el ID del articulo',
        });
    }
}

function ConsultarArticuloUtilerias() {
    $("#div_tabla_articulo").html('');
    var clave = $("#clave_buscar").val();
    var articulo = $("#nombre_buscar").val();
    if (clave != "" || articulo != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeput: 100000,
            url: "../UTILERIAS/ArticulosUtileriaTable",
            data: {
                clave: clave,
                articulo: articulo
            },
            success: function (response) {
                $("#div_tabla_articulo").html('');
                try {
                    $("#tabla_articulos").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_tabla_articulo").html(response);
                $('#tabla_articulos').DataTable({
                    keys: true,
                    ordering: false,
                    paging: true,
                    dom: "Bfrtip",
                    scrollCollapse: false,
                    buttons: [{}],
                    responsive: !0,
                    pagingType: 'simple_numbers'
                });
                idArticuloAsig = -1;
                jsRemoveWindowLoad();
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar la clave o nombre del articulo',
        });
    }
}

function RegistrarCapturaAlmacen(id_inventario_g, id_articulo) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/RegistrarCapturaAlmacen",
        data: {
            id_inventario_g: id_inventario_g,
            id_articulo: id_articulo
        },
        success: function (response) {
            if (response == 'True' || response == true) {
                iziToast.success({
                    title: 'Exito',
                    message: 'Se capturo correctamente el inventario',
                });
                ConsultarInventarioFecha();
                jsRemoveWindowLoad();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al registrar la captura de inventario',
                });
                jsRemoveWindowLoad();
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ObtenerInfoCapturaAlmacen(id_inventario_d) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/ObtenerInfoCapturaAlmacen",
        data: {
            id_inventario_d: id_inventario_d
        },
        success: function (response) {
            $("#articulo_captura").val('');
            $("#cantidad_captura").val('');
            $("#cantidad_sistema").val('');
            $("#cantidad_diferencia").val('');
            $("#m_captura_d").modal("show");
            $("#btn_captura_accion").attr("onclick", "ModificarCapturaAlmacen(" + id_inventario_d + ");");
            var data = $.parseJSON(response);
            $("#articulo_captura").val(data[0].nombre_articulo);
            $("#cantidad_captura").val(data[0].cantidad_captura);
            $("#cantidad_sistema").val(data[0].cantidad_sistema);
            $("#cantidad_diferencia").val(data[0].diferencia);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModificarCapturaAlmacen(id_inventario_d) {

    var cantidad_captura = $("#cantidad_captura").val();
    var cantidad_sistema = $("#cantidad_sistema").val();
    var diferencia = $("#cantidad_diferencia").val();

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
                        async: false,
                        url: "../UTILERIAS/ModificarCapturaAlmacen",
                        data: {
                            id_inventario_d: id_inventario_d,
                            cantidad_captura: cantidad_captura,
                            cantidad_sistema: cantidad_sistema,
                            diferencia: diferencia
                        },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 'True' || response == true) {
                                iziToast.success({
                                    title: 'Exito',
                                    message: 'Se actualizo correctamente la captura del inventario',
                                });
                                $("#m_captura_d").modal("hide");

                                $("#articulo_captura").val('');
                                $("#cantidad_captura").val('');
                                $("#cantidad_sistema").val('');
                                $("#cantidad_diferencia").val('');
                                ConsultarInventarioFecha();

                            }
                            else {
                                iziToast.error({
                                    title: 'Error',
                                    message: 'Ocurrió un problema al actualizar la captura del inventario',
                                });
                            }
                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                            jsRemoveWindowLoad();
                            iziToast.error({
                                title: 'Error',
                                message: 'Ocurrió un problema al actualizar la captura del inventario',
                            });
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

}


function OnOffArticuloUtileria(id_articulo, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/OnOffArticulos",
        data: {
            id_articulo: id_articulo,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de estatus correctamente',
                });
                ConsultarArticuloUtilerias();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema cambiar el estatus',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function OnOffAlmacenableUtileria(id_articulo, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/OnOffAlmacenables",
        data: {
            id_articulo: id_articulo,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de almacenado correctamente',
                });
                ConsultarArticuloUtilerias();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema cambiar el almacenado',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}