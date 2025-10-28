function ConsultarEquiposEstabloTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EQUIPOS_ESTABLOS/ConsultarEquiposEstabloTable",
        data: {
        },
        success: function (response) {
            try {
                $("#datatable_equipos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_equipos").html(response);
            $('#datatable_equipos').DataTable({
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

function NuevoEquipos() {
    $(".input_clean").val("");
    $("#clave_equipo").prop('disabled', false);

    $("#lbl_valid_equipo").text("");
    $("#lbl_add_upd_equipo").text("Registrar equipo");
    $("#btn_add_upd_equipo").attr("onclick", "RegistrarEquipoEstablo()");
    $("#btn_add_upd_equipo").text("Registrar equipo");
    $("#m_add_upd_equipo").modal("show");
}

function EditarEquipo(clave, nombre, obs, id_equipo) {
    $("#clave_equipo").prop('disabled', true);

    $("#clave_equipo").val(clave);
    $("#nombre_equipo").val(nombre);
    $("#obs_equipo").val(obs);

    $("#lbl_valid_equipo").text("");

    $("#lbl_add_upd_equipo").text("Actualizar equipo");
    $("#btn_add_upd_equipo").text("Actualizar equipo");
    $("#btn_add_upd_equipo").attr("onclick", "ActualizarEquipoEstablo(" + id_equipo + ")");
    $("#m_add_upd_equipo").modal("show");
}

function ValidarClaveUnicaEquipo() {
    var lbl_valid = $("#lbl_valid_equipo");
    $.ajax({
        type: "POST",
        async: true,
        url: "../EQUIPOS_ESTABLOS/ValidarClaveUnicaEquipo",
        data: { clave: $("#clave_equipo").val() },
        success: function (response) {
            if (response == "True") {
                lbl_valid.css("color", "green");
                lbl_valid.text("Clave valida");
            }
            else {
                lbl_valid.css("color", "red");
                lbl_valid.text("Clave existente. Ingrese otra clave");
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function RegistrarEquipoEstablo() {
    var clave = $("#clave_equipo").val();
    var nombre = $("#nombre_equipo").val();
    var obs = $("#obs_equipo").val();

    var clave_trim = $.trim(clave);
    var nom_trim = $.trim(nombre);


    if (clave == "" || clave_trim.length == 0) {
        iziToast.warning({
            title: 'Ingresa la clave del equipo',
            message: '',
        });
    }
    else if (nombre == "" || nom_trim.length == 0) {
        iziToast.warning({
            title: 'Ingresa el nombre del equipo',
            message: '',
        });
    }
    else if ($("#lbl_valid_equipo").css('color') == 'rgb(255, 0, 0)') {
        iziToast.warning({
            title: 'Ingresa una clave diferente',
            message: '',
        });
    }
    else {
        C_establos_equipos = {};
        C_establos_equipos.clave_equipo = clave;
        C_establos_equipos.nombre_equipo = nombre;
        C_establos_equipos.observaciones = obs;

        $.ajax({
            type: "POST",
            async: true,
            url: "../EQUIPOS_ESTABLOS/RegistrarEquipoEstablo",
            data: {
                C_establos_equipos: C_establos_equipos
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == "True") {
                    $("#m_add_upd_equipo").modal("hide");
                    ConsultarEquiposEstabloTable();
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Equipo registrado correctamente!',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema registrar el equipo',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema registrar el equipo',
                });
            }
        });
    }
}


function ActualizarEquipoEstablo(id_equipo) {
    //var clave = $("#clave_equipo").val();
    var nombre = $("#nombre_equipo").val();
    var obs = $("#obs_equipo").val();

    //var clave_trim = $.trim(clave);
    var nom_trim = $.trim(nombre);

    if (nombre == "" || nom_trim.length == 0) {
        iziToast.warning({
            title: 'Ingresa el nombre del equipo',
            message: '',
        });
    }
    else {
        $.ajax({
            type: "POST",
            async: true,
            url: "../EQUIPOS_ESTABLOS/ActualizarEquipoEstablo",
            data: {
                id_equipo: id_equipo,
                nombre_equipo: nombre,
                obs: obs
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == "True") {
                    $("#m_add_upd_equipo").modal("hide");
                    ConsultarEquiposEstabloTable();
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Equipo actualizado correctamente!',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema actualizar el equipo',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema actualizar el equipo',
                });
            }
        });
    }
}


function OnOffEquipoEstablo(id_equipo, modo, nombre_equipo) {
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
        message: '¿Está seguro de ' + accion + ' el equipo: ' + nombre_equipo + '?',
        position: 'center',
        buttons: [
            ['<button><b>Si, ejecutar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../EQUIPOS_ESTABLOS/OnOffEquipoEstablo",
                    data: {
                        id_equipo: id_equipo,
                        modo: modo
                    },
                    success: function (response) {
                        ConsultarEquiposEstabloTable();
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


