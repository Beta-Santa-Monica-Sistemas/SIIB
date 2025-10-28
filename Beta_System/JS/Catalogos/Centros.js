function ConsultarCentrosTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CENTROS/ConsultarCentrosTable",
        data: {
        },
        success: function (response) {
            try {
                $("#datatable_centros").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_centros").html(response);
            $('#datatable_centros').DataTable({
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

function NuevoCentro() {
    $("#siglas_centro").val("");
    $("#nombre_centro").val("");
    $("#lbl_add_upd_centro").text("Registrar centro");
    $("#btn_add_upd_centro").attr("onclick", "AgregarNuevoCentro()");
    $("#btn_add_upd_centro").text("Registrar centro");
    $("#m_add_upd_centro").modal("show");
}

function EditarCentro(siglas, centro, id_centro) {
    $("#siglas_centro").val(siglas);
    $("#nombre_centro").val(centro);
    $("#lbl_add_upd_centro").text("Actualizar centro");
    $("#btn_add_upd_centro").text("Actualizar centro");
    $("#btn_add_upd_centro").attr("onclick", "ActualizarCentro(" + id_centro + ")");
    $("#m_add_upd_centro").modal("show");
}

function ActualizarCentro(id_centro) {
    var siglas = $("#siglas_centro").val();
    var nombre = $("#nombre_centro").val();
    var sig_trim = $.trim(siglas);
    var sig_nom = $.trim(nombre);

    if (siglas == "" || sig_trim.length == 0) {
        iziToast.warning({
            title: 'Ingresa las siglas del centro',
            message: '',
        });
    }
    else if (nombre == "" || sig_nom.length == 0) {
        iziToast.warning({
            title: 'Ingresa el nombre del centro',
            message: '',
        });
    }
    else {
        $.ajax({
            type: "POST",
            async: true,
            url: "../CENTROS/ActualizarCentro",
            data: {
                id_centro: id_centro,
                siglas: siglas,
                nombre: nombre
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == "True") {
                    $("#m_add_upd_centro").modal("hide");
                    ConsultarCentrosTable();
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Centro actualizado correctamente!',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema actualizar el centro',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema actualizar el centro',
                });
            }
        });
    }
}

function AgregarNuevoCentro() {
    var siglas = $("#siglas_centro").val();
    var nombre = $("#nombre_centro").val();
    var sig_trim = $.trim(siglas);
    var sig_nom = $.trim(nombre);


    if (siglas == "" || sig_trim.length == 0) {
        iziToast.warning({
            title: 'Ingresa las siglas del centro',
            message: '',
        });
    }
    else if (nombre == "" || sig_nom.length == 0) {
        iziToast.warning({
            title: 'Ingresa el nombre del centro',
            message: '',
        });
    }
    else {
        C_centros_g = {};
        C_centros_g.siglas = siglas;
        C_centros_g.nombre_centro = nombre;

        $.ajax({
            type: "POST",
            async: true,
            url: "../CENTROS/AgregarNuevoCentro",
            data: {
                C_centros_g: C_centros_g
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == "True") {
                    $("#m_add_upd_centro").modal("hide");
                    ConsultarCentrosTable();
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Centro registrado correctamente!',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema registrar el centro',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema registrar el centro',
                });
            }
        });
    }
}

function OnOffCentro(id_centro, modo, nombre_centro) {
    var accion = "ACTIVAR";
    if (modo == false) { accion = "DESACTIVAR"; }
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ADVERTENCIA',
        message: '¿Está seguro de ' + accion + ' el centro: ' + nombre_centro + '?',
        position: 'center',
        buttons: [
            ['<button><b>Si, ejecutar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../CENTROS/OnOffCentro",
                    data: {
                        id_centro: id_centro,
                        modo: modo
                    },
                    success: function (response) {
                        ConsultarCentrosTable();
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                        jsRemoveWindowLoad();
                    }
                });

            }, true],
            ['<button>No, cancelar opreación</button>', function (instance, toast) {
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


