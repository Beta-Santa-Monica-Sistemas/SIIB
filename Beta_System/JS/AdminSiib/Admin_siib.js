

function ConsultarMenuSIIBAmin() {
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ADMIN_SIIB/ConsultarMenuSIIBAmin",
        data: { },
        success: function (response) {
            $("#div_menu_admin").html(response);
        },
        failure: function (response) {
        },
        error: function (response) {
        }
    });
}

function AddEditModulo() {
    $("#txt_nombre_modulo").val('');
    $("#txt_icono_modulo").val('');
    $("#btn_add_edit_modulo").attr("onclick", "AgregarActualizarModuloSIIB(0);");
    $("#btn_add_edit_modulo").text("Registrar módulo");
    $("#m_info_modulo").modal("show");
}

function EditarInfoModulo(nombre, icono, id_modulo) {
    $("#txt_nombre_modulo").val(nombre);
    $("#txt_icono_modulo").val(icono);
    $("#btn_add_edit_modulo").attr("onclick", "AgregarActualizarModuloSIIB(" + id_modulo + ");");
    $("#btn_add_edit_modulo").text("Actualizar módulo");
    $("#m_info_modulo").modal("show");
}

function AgregarActualizarModuloSIIB(id_modulo) {
    var nombre = $("#txt_nombre_modulo").val();
    var icono = $("#txt_icono_modulo").val();
    if (nombre.trim().length == 0) {
        iziToast.warning({
            title: 'Ingrese un nombre válido para el módulo',
            message: '',
        });
        return;
    }
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ADMIN_SIIB/AgregarActualizarModuloSIIB",
        data: {
            id_modulo: id_modulo,
            nombre: nombre,
            icono: icono
        },
        success: function (response) {
            if (response == 0) {
                $("#txt_nombre_modulo").val('');
                $("#txt_icono_modulo").val('');
                ConsultarMenuSIIBAmin();
                iziToast.success({
                    title: "Acción ejecutada correctamente",
                    message: '',
                });
                $("#m_info_modulo").modal("hide");
            }
            else {
                var accion = "actualizar";
                if (id_modulo == 0) { accion = "registrar"; }
                iziToast.error({
                    title: "Ocurrió un problema al " + accion + " el módulo",
                    message: '',
                });
            }
        },
        failure: function (response) {
        },
        error: function (response) {
        }
    });
}


function ConsultarFormularioComportamiento() {
    $("#div_sub_modulo_g").css("display", "none");
    var modo = $('input[name="check_aplica_subsubmenu"]:checked').val();
    if (modo == 1) {  //SUBMODULO NORMAL
        $("#txt_controlador_submodulo").prop("readonly", false);
        $("#txt_funcion_submodulo").prop("readonly", false);
    }
    else if (modo == 2) {  //SUBMODULO GENERAL
        $("#txt_controlador_submodulo").prop("readonly", true);
        $("#txt_funcion_submodulo").prop("readonly", true);
    }
    else if (modo == 3) {  //ITEM SUBMODULO GENERAL
        ConsultarSubModulosPrincipalesSelect();
        $("#txt_controlador_submodulo").prop("readonly", false);
        $("#txt_funcion_submodulo").prop("readonly", false);

    }
}
function ConsultarSubModulosPrincipalesSelect() {
    var id_modulo = $("#id_modulo_submenu").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../MODULOS/ConsultarSubModulosPrincipalesSelect",
        data: { id_modulo: id_modulo },
        success: function (response) {
            $("#div_sub_modulo_g").css("display", "block");
            $("#id_submodulo_principal").html(response);
        },
        failure: function (response) {
        },
        error: function (response) {
        }
    });
}

function EditarInfoSubModulo(id_submodulo, id_modulo, nombre, funcion, controlador, aplica_submenu, id_sub_submenu) {
    $("#id_modulo_submenu").val(id_modulo);
    $("#txt_nombre_submodulo").val(nombre);
    $("#txt_funcion_submodulo").val(funcion);
    $("#txt_controlador_submodulo").val(controlador);
    $(".check_aplica_subsubmenu").prop("cheked", false);

    $("#radio_submenu").prop("checked", true);
    if (aplica_submenu == 1) {
        $("#radio_aplica_secundario").prop("checked", true);
    }
    if (id_sub_submenu > 0) {
        $("#radio_item_secundario").prop("checked", true);
    }
    ConsultarFormularioComportamiento();

    $("#btn_add_edit_submodulo").attr("onclick", "ActualizarSubModulo(" + id_submodulo + ")");
    $("#btn_add_edit_submodulo").text("Actualizar");
    $("#id_modulo_submenu").attr("readonly", false);
    if (id_submodulo == 0) {
        $("#btn_add_edit_submodulo").text("Agregar");
        $("#id_modulo_submenu").attr("readonly", true);
    }
    $("#m_info_submodulo").modal("show");
}

function ActualizarSubModulo(id_submodulo) {
    var id_modulo = $("#id_modulo_submenu").val();
    var nombre = $("#txt_nombre_submodulo").val();
    var controlador = $("#txt_controlador_submodulo").val();
    var funcion = $("#txt_funcion_submodulo").val();
    var comportamiento = $('input[name="check_aplica_subsubmenu"]:checked').val();
    var id_submenu_modulo_sub = 0;
    if (comportamiento == 3) {
        id_submenu_modulo_sub = $("#id_submodulo_principal").val();
    }

    if (nombre.trim().length == 0) {
        iziToast.warning({
            title: 'Ingrese un nombre válido para el módulo',
            message: '',
        });
        return;
    }
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ADMIN_SIIB/ActualizarSubModulo",
        data: {
            id_submodulo: id_submodulo,
            id_modulo: id_modulo,
            nombre: nombre,
            controlador: controlador,
            funcion: funcion,
            comportamiento: comportamiento,
            id_submenu_modulo_sub: id_submenu_modulo_sub
        },
        success: function (response) {
            if (response == 0) {
                $("#txt_nombre_modulo").val('');
                $("#txt_icono_modulo").val('');
                ConsultarMenuSIIBAmin();
                iziToast.success({
                    title: "Acción ejecutada correctamente",
                    message: '',
                });
                $("#m_info_submodulo").modal("hide");
            }
            else {
                var accion = "actualizar";
                if (id_modulo == 0) { accion = "registrar"; }
                iziToast.error({
                    title: "Ocurrió un problema al " + accion + " el módulo",
                    message: '',
                });
            }
        },
        failure: function (response) {
        },
        error: function (response) {
        }
    });
}





