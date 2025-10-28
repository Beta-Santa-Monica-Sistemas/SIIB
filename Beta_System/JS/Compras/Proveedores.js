//#region  ADMINISTRACIÓN DEL PROVEEDOR
function AccionProveedor(accion) {
    $("#id_banco_1").css("border-color", "");
    $("#tipo_moneda_1").css("border-color", "");
    $("#giro_proveedor").css("border-color", "");
    $("#mensaje_correo").text('');
    $("#mensaje_telefono").text('');
    $("#mensaje_clabe").text('');
    //EDITAR
    if (accion == 1) {
        $("#lbl_titulo_proveedor").text("EDITAR PROVEEDOR");
        $("#btn_accion_realizar").text("Actualizar");
        $(".ms_edicion").each(function () {
            $(this).prop("readonly", true);
        });
        $("#btn_accion_realizar").css("display", "block");
    }
    //MOSTRAR
    else if (accion == 3) {
        $("#lbl_titulo_proveedor").text("INFORMACION PROVEEDOR");
        $("#btn_accion_realizar").css("display", "none");
        $(".solo_lectura").each(function () {
            $(this).prop("disabled", true);
        });
        $("#bloqueo_proveedor").val('');
        $("#bloqueo_proveedor").css("display", "none");
    }
    //BLOQUEAR
    else if (accion == 4) {
        $("#lbl_titulo_proveedor_bloqueo").text("INFORMACION DE BLOQUEOS");
        $("#m_proveedor_bloqueo").modal("show");
        ConsultarProveedoresBloqueos();
    }
    else {
        LimpiarSeleccion();
        $("#bloqueo_proveedor").css("display", "none");
        $("#cve").prop("readonly", false);
        $("#btn_accion_realizar").css("display", "block");
        $("#lbl_titulo_proveedor").text("REGISTRAR PROVEEDOR");
        $("#btn_accion_realizar").text("Registrar");
        $("#btn_accion_realizar").attr("onclick", "RegistrarProveedor();");
        $(".solo_lectura").each(function () {
            $(this).prop("disabled", false);
        });

        $("#id_estatus").prop("disabled", true);
    }

    $("#m_proveedor").modal("show");
    $(".limpiar").each(function () {
        $(this).css("border", "");
    });



}

function ProveedorBloqueo() {
    if ($("#id_estatus").val() == 3) {
        $("#bloqueo_proveedor").css("display", "block");
    }
    else {
        $("#bloqueo_proveedor").val('');
        $("#bloqueo_proveedor").css("display", "none");
    }
}

var id_proveedor = -1;
function ConsultarProveedoresBloqueos() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../PROVEEDORES/ConsultarProveedoresBloq",
        data: {
            id_proveedor: id_proveedor
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_proveedores_bloqueo").html(response);
            $('#datatable_proveedores_bloqueo').DataTable({
                keys: true,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [],
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarProveedores() {
    jsShowWindowLoad();
    var id_estatus = $("#estatus_search").val();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../PROVEEDORES/ConsultarProveedores",
        data: {
            id_estatus: id_estatus
        },
        success: function (response) {
            jsRemoveWindowLoad();
            try {
                $("#datatable_proveedores").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_proveedores").html(response);
            $('#datatable_proveedores').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
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
                scrollX: true,
                pagingType: 'simple_numbers',
                pageLength: 20,
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });

}

var clave_original = '';
function ObtenerInfProveedor(id_compras_proveedor) {
    jsShowWindowLoad();
    $("#motivo_bloqueo").val('');
    $.ajax({
        type: "POST",
        async: false,
        url: "../PROVEEDORES/ObtenerInfProveedor",
        data: {
            id_compras_proveedor: id_compras_proveedor
        },
        success: function (response) {
            $("#btn_accion_realizar").attr("onclick", "ModificarProveedor(" + id_compras_proveedor + ");");
            var data = $.parseJSON(response);
            id_proveedor = id_compras_proveedor;
            $("#cve").val(data[0].cve);

            clave_original = $("#cve").val();

            $("#nombre_prov").val(data[0].nombre_prov);
            $("#razon_social").val(data[0].razon_social);
            $("#RFC").val(data[0].RFC);
            $("#direccion1").val(data[0].direccion_1_prov);
            $("#tel_prov").val(data[0].tel_prov);
            $("#correo_prov").val(data[0].correo_prov);
            $("#dias_pago").val(data[0].dias_pago);
            $("#contacto1").val(data[0].contacto_nombre_1);
            $("#correo1").val(data[0].contacto_correo_1);
            $("#telefono1").val(data[0].contacto_tel_1);
            $("#id_estatus").val(data[0].id_proveedor_status);
            $("#giro_descripcion").val(data[0].giro_desciprcion);
            $("#clabe1").val(data[0].cta_banco_1);
            $("#alias_bascula").val(data[0].alias_bascula);

            //BANCO
            $("#id_banco_1").val(data[0].banco);
            //MONEDA
            $("#tipo_moneda_1").val(data[0].nombre + " - " + data[0].clave_fiscal);
            //GIRO PROVEEDOR
            if (data[0].id_giro_proveedor == null) {
                $("#giro_proveedor").val('ninguno_3');
            }
            else {
                $("#giro_proveedor").val(data[0].id_giro_proveedor);
            }

            var bascula = data[0].disponible_bascula;
            if (bascula == true) {
                $("#prov_bascula").prop('checked', true);
            }
            else {
                $("#prov_bascula").prop('checked', false);
            }


            var alimentacion = data[0].prov_alimentacion;
            if (alimentacion == true) {
                $("#prov_alimentacion").prop('checked', true);
            }
            else {
                $("#prov_alimentacion").prop('checked', false);
            }

            $("#id_cuenta").val(data[0].cuenta_cxp);
            $("#id_banco_1").prop("disabled", true);

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    ObtenerObsProveedor(id_compras_proveedor);
    jsRemoveWindowLoad();
}

function ObtenerObsProveedor(id_compras_proveedor) {
    jsShowWindowLoad();
    if ($("#id_estatus").val() == 3) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../PROVEEDORES/ObtenerObsProveedor",
            data: {
                id_compras_proveedor: id_compras_proveedor
            },
            success: function (response) {
                $("#bloqueo_proveedor").val('');
                $("#bloqueo_proveedor").css("display", "block");
                var data = $.parseJSON(response);
                $("#motivo_bloqueo").val(data[0].motivo);
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        $("#bloqueo_proveedor").val('');
        $("#bloqueo_proveedor").css("display", "none");
    }
    jsRemoveWindowLoad();
}

function ModificarProveedor(id_compras_proveedor) {
    $("#giro_proveedor").css("border", "");
    var candadoActualizacion = 1;

    /*ValidacionTotal();*/

    var c_proveedor = {};
    c_proveedor.id_proveedor_status = $("#id_estatus").val();
    c_proveedor.contacto_nombre_1 = $("#contacto1").val();
    c_proveedor.contacto_correo_1 = $("#correo1").val();
    c_proveedor.contacto_tel_1 = $("#telefono1").val();
    c_proveedor.giro_desciprcion = $("#giro_descripcion").val();
    c_proveedor.id_giro_proveedor = $("#giro_proveedor").val();
    c_proveedor.alias_bascula = $("#alias_bascula").val();

    if ($("#prov_alimentacion").is(':checked')) {
        c_proveedor.prov_alimentacion = true;
    }
    else {
        c_proveedor.prov_alimentacion = false;
    }

    if ($("#prov_bascula").is(':checked')) {
        c_proveedor.disponible_bascula = true;
    }
    else {
        c_proveedor.disponible_bascula = false;
    }

    jsShowWindowLoad();
    //BLOQUEAR PROVEEDOR
    if ($("#id_estatus").val() == 3) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../PROVEEDORES/BloqueoLogProveedor",
            data: {
                id_compras_proveedor: id_compras_proveedor,
                motivo: motivo
            },
            success: function (response) {
                if (response == "True") {
                    jsRemoveWindowLoad();
                    LimpiarSeleccion();
                    ConsultarProveedores();
                    iziToast.success({
                        title: 'Exico',
                        message: '¡Proveedor bloqueado correctamente!',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al registrar el bloqueo',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    //ACTUALIZACION
    else {
        //SI EL PROVEEDOR ES DE BASCULA
        if (c_proveedor.disponible_bascula == true) {
            if (c_proveedor.alias_bascula.trim() == "") {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Favor de ingresar un alias al proveedor',
                });
                $("#alias_bascula").css("border-color", "red");
                candadoActualizacion = 0;
                jsRemoveWindowLoad();
                return;
            }
        }
        else {
            c_proveedor.alias_bascula = " ";
        }

        //VALIDACION DE CAMPOS OBLIGATORIOS
        $(".obligatorio").each(function () {
            if ($(this).val().trim() == "") {
                $(this).css("border-color", "red");
                candadoActualizacion = 0;
            }
        });

        //VALIDACION DEL GIRO DEL PROVEEDOR
        if ($("#giro_proveedor").val() == "ninguno_3") {
            iziToast.warning({
                title: 'Aviso',
                message: '¡Seleccione el giro de la empresa al proveedor!',
            });
            $("#giro_proveedor").css("border", "1px solid red");
            candadoActualizacion = 0;
        }

        //MENSAJE DE AVISO
        if (candadoActualizacion == 0) {
            iziToast.warning({
                title: 'Aviso',
                message: '¡Es necesario rellenar los campos obligatorios!',
            });
            jsRemoveWindowLoad();
            return;
        }

        //ACTUALIZACION DEL PROOVEDOR
        $.ajax({
            type: "POST",
            async: true,
            url: "../PROVEEDORES/ModificarProveedor",
            data: {
                id_compras_proveedor: id_compras_proveedor,
                c_proveedor: c_proveedor
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Proveedor actualizado corretamente!',
                    });
                    $("#alias_bascula").css("border-color", "");
                    $("#m_proveedor").modal("hide");
                    LimpiarSeleccion();
                    ConsultarProveedores();
                    jsRemoveWindowLoad();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al actualizar el proveedor',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    jsRemoveWindowLoad();
}
var campos_validar = ["contacto1", "correo1"];

function LimpiarSeleccion() {

    campos_validar.forEach(function (campo_id) {
        var $campo = $("#" + campo_id);
        if ($campo.val() != '') {
            $campo.val('');
            $campo.css("border-color", "");
        }
    });
    $("#clabe2").val('');
    $("#id_estatus").val(1);
    $("#contacto2").val('');
    $("#correo2").val('');
    $("#telefono2").val('');
    $("#prov_alimentacion").prop('checked', false);
    $("#prov_bascula").prop('checked', false);
    $("#id_banco_1").css("border-color", "");
    $("#tipo_moneda_1").css("border-color", "");
    $("#giro_proveedor").css("border-color", "");
    $(".limpiar_banco").val('17');
    $(".limpiar_seleccion1").val('ninguno_1');
    $(".limpiar_seleccion2").val('ninguno_2');
    $(".limpiar_seleccion3").val('ninguno_3');
}

function LimpiarKeyPress() {
    campos_validar.forEach(function (campo_id) {
        var $campo = $("#" + campo_id);
        if ($campo.val() != '') {
            $campo.css("border-color", "");
        }
    });
    if ($("#motivo_bloqueo").val() != '') {
        $("#motivo_bloqueo").css("border-color", "");
    }
}

function LimpiarSelect() {
    if ($("#giro_proveedor").val() !== 'ninguno_3') {
        $("#giro_proveedor").css("border-color", "");
    }
}


var claves_proveedor = 0;
function ValidarClaveProveedor() {
    jsShowWindowLoad();
    var clave = $("#cve").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../PROVEEDORES/ValidaClaveProveedor",
        data: {
            clave: clave
        },
        success: function (response) {
            jsRemoveWindowLoad();
            if (response == 0) {
                if (clave_original == $("#cve").val()) {
                    claves_proveedor = 1;
                    $("#cve").css("border-color", "");
                }
                else {
                    $("#cve").css("border-color", "red");
                    iziToast.warning({
                        title: 'Aviso',
                        message: '¡La clave ingresada ya existe!',
                    });
                    claves_proveedor = 0;

                }
            }
            else {
                $("#cve").css("border-color", "");
                claves_proveedor = 1;

            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarInformacionProveedorNomb(modo) {
    var id_proveedor;
    if (modo == 1) { id_proveedor = $("#clave_proveedor").val(); }
    else { id_proveedor = $("#clave_proveedor_serv").val(); }
    $.ajax({
        type: "POST",
        async: false,
        url: "../PROVEEDORES/ConsultarInformacionNomProv",
        data: {
            id_proveedor: id_proveedor
        },
        success: function (response) {
            if (response.length > 3) {
                var data = $.parseJSON(response);
                $("#proveedor").val(data[0].id_compras_proveedor);
                $("#id_proveedor").val(data[0].razon_social);
                $("#nombre_comercial").text(data[0].nombre_prov);
                $("#razon_social").text(data[0].razon_social);
                $("#direccion").text(data[0].direccion_1_prov);
                $("#nombre_contacto").text(data[0].contacto_nombre_1);
                $("#telef_oficina").text(data[0].tel_prov);
                $("#telef_celular").text(data[0].contacto_tel_1);
                $("#correo").text(data[0].correo_prov);
                $("#credito").val(data[0].dias_pago);
                $("#clave_fiscal_moneda").text(data[0].clave_fiscal);
                $("#div_info_prov").css("display", "block");

                $("#clave_fiscal_moneda_new_cotizacion").text(data[0].clave_fiscal);
                $("#moneda_new_cotizacion").text(data[0].id_tipo_moneda);
                $("#tipo_moneda").val(data[0].id_tipo_moneda);


                $("#id_proveedor_serv").val(data[0].razon_social);
                $("#clave_fiscal_moneda_new_cotizacion_serv").text(data[0].clave_fiscal);
            }
            else {
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'No se encontro al proveedor',
                });
                $("#clave_proveedor").focus();
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function AccionesProveedor(accion) {
    //if (accion == 1) {
    //    //CLAVE PROVEEDOR
    //    $('#clave_proveedor').keydown(function (e) {
    //        if (e.keyCode === 13 || e.keyCode === 9) {
    //            ConsultarInformacionProveedorNomb();
    //            $("#presentacion").focus();
    //        }
    //    });
    //}
    //else {
    //    $('#id_proveedor').keydown(function (e) {
    //        if (e.keyCode === 13 || e.keyCode === 9) {
    //            ConsultarClaveProveedor()                
    //            $("#precio_unitario").focus();
    //        }
    //    });
    //}
}

function ConsultarClaveProveedor(modo) {
    var id_proveedor;
    if (modo == 1) { id_proveedor = $("#id_proveedor").val(); }
    else { id_proveedor = $("#id_proveedor_serv").val(); }
    $.ajax({
        type: "POST",
        async: false,
        url: "../PROVEEDORES/ConsultarInfoClave",
        data: {
            id_proveedor: id_proveedor
        },
        success: function (response) {
            if (response != "[]" || response != null) {
                var data = $.parseJSON(response);
                if (modo == 1) {
                    $("#clave_proveedor").val(data[0].cve);
                    ConsultarInformacionProveedorNomb(1);
                    $("#clave_fiscal_moneda_new_cotizacion").text(data[0].clave_fiscal);
                    $("#moneda_new_cotizacion").text(data[0].id_tipo_moneda);
                }
                else {
                    $("#clave_proveedor_serv").val(data[0].cve);
                    ConsultarInformacionProveedorNomb(2);

                    $("#clave_fiscal_moneda_new_cotizacion_serv").text(data[0].clave_fiscal);
                    $("#moneda_new_cotizacion_serv").text(data[0].id_tipo_moneda);
                }
            }
            else {
                iziToast.warning({
                    title: '¡El centro es un campo obligatorio!',
                    message: '',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}


validaciones = 0;
function ValidarCorreos() {
    var correo = $("#correo1").val();
    var emailRegex = /^[a-zA-Z0-9]([a-zA-Z0-9_.]*[a-zA-Z0-9])?@[a-zA-Z0-9]+(\.[a-zA-Z]{2,})+$/;

    if (!emailRegex.test(correo)) {
        $("#mensaje_correo").text('¡Correo inválido!');
        $("#mensaje_correo").css("color", "red");
        $("#mensaje_correo").css("border-color", "red");
        validaciones = 1;
    } else {
        $("#mensaje_correo").text('¡Correo válido!');
        $("#mensaje_correo").css("color", "green");
    }

}
//function ValidarClabeInterbancaria() {
//    var clabe = $("#clabe1").val();
//    if (clabe.length == 18) {
//        $("#mensaje_clabe").text('¡Clabe valida!');
//        $("#mensaje_clabe").css("color", "green");
//    }
//    else {
//        $("#mensaje_clabe").text('¡Clabe invalida!');
//        $("#mensaje_clabe").css("color", "red");
//        $("#mensaje_clabe").css("border-color", "red");
//        validaciones = 1;
//    }
//}
function ValidacionTotal() {
    ValidarCorreos();
    //ValidarClabeInterbancaria();
    var clabe = $("#clabe1").val();
    var correo = $("#correo1").val();
    var telefono = $("#telefono1").val();
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    var isValid = emailRegex.test(correo);
    if (!isValid && telefono.length == 7 && telefono.length == 10) {
        validaciones = 1;
    }
    else {
        validaciones = 0;
    }
}
function ValidarTelefono() {
    var telefono = $("#telefono1").val();
    if (telefono.length == 7 || telefono.length == 10) {
        $("#mensaje_telefono").text('¡Telefono valido!');
        $("#mensaje_telefono").css("color", "green");
    }
    else {
        $("#mensaje_telefono").text('¡Telefono invalido!');
        $("#mensaje_telefono").css("color", "red");
        $("#mensaje_telefono").css("border-color", "red");
        validaciones = 1;
    }
}

function LupaProveedores(accion) {
    $("#m_lupa_proveedor").modal("show");

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../PROVEEDORES/LupaProveedor",
        data: {
            accion: accion
        },
        success: function (response) {
            try {
                $("#tabla_articulos_lupa").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_lupa_proveedor").html(response);
            $('#tabla_proveedor_lupa').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 6
            });
            jsRemoveWindowLoad();

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });

}
function InformacionLupaProveedor(proveedor, accion, nombre) {
    if (accion != 3) {
        $("#m_lupa_proveedor").modal("hide");
        $("#clave_proveedor").val(proveedor);
        ConsultarInformacionProveedorNomb(1);
    }
    else {
        $("#m_lupa_proveedor").modal("hide");
        $("#id_proveedor").text(proveedor);
        $("#proveedor_nombre").val(nombre);
        $("#id_compras_proveedor").text(proveedor);
    }
}


function AccionModalProveedor(accion) {
    if (accion == 1) {
        $("#m_ingresar_proveedor").modal("show");
        $("#txt_usuario_rfc").val("");
        $("#id_compras_proveedor").text("");
        $("#proveedor_nombre").val("");
        $("#txt_password").val("");

    }
    else if (accion == 2) {
        $("#m_ingresar_proveedor").modal("hide");
        $("#txt_usuario_rfc").val("");
        $("#id_compras_proveedor").text("");
        $("#proveedor_nombre").val("");
        $("#txt_password").val("")
    }
    else {
        $("#txt_usuario_rfc").val("");
        $("#id_compras_proveedor").text("");
        $("#proveedor_nombre").val("");
        $("#txt_password").val("")
    }
}

function RegistrarProveedorCuenta() {
    var usuario = $("#txt_usuario_rfc").val();
    var contrasena = $("#txt_password").val();
    var id_proveedor = $("#id_compras_proveedor").text();
    if (usuario.length > 0 && contrasena.length > 0 && id_proveedor.length > 0) {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: false,
            url: "../PROVEEDORES/RegistrarProveedorCuenta",
            data: {
                usuario: usuario,
                contrasena: contrasena,
                id_proveedor: id_proveedor
            },
            success: function (response) {
                if (response == 1) {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Se registro la cuenta del proveedor!',
                    });
                    AccionModalProveedor(2);
                }
                else if (response == 3) {
                    iziToast.warning({
                        title: 'Aviso',
                        message: '¡Usuario o proveedor en uso, favor de utilizar otro!',
                    });
                    AccionModalProveedor(3);
                }
                else {
                    iziToast.warning({
                        title: 'Aviso',
                        message: '¡No se realizo el regisro!',
                    });
                    AccionModalProveedor(2);
                }

                MostrarProveedoresTable();
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
            message: '¡Favor de rellenar todos los campos!',
        });
    }
}

function MostrarProveedoresTable() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../PROVEEDORES/MostrarProveedoresTable",
        data: {},
        success: function (response) {
            try {
                $("#proveedores_cuentas").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_cuentas_proveedor").html(response);
            $('#proveedores_cuentas').DataTable({
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
                responsive: !0
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function OnOffProveedores(modo, id_proveedor) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../PROVEEDORES/OnOffProveedor",
        data: {
            id_proveedor: id_proveedor,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                MostrarProveedoresTable();
                iziToast.success({
                    title: 'Exito',
                    message: '¡Actualizacion realizada!',
                });
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Ocurrió un problema al actualizar el proveedor',
                });
            }
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarProvNombre() {
    var nombre = $("#proveedor_nombre").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../PROVEEDORES/ConsultarProvNombre",
        data: {
            nombre: nombre
        },
        success: function (response) {
            $("#id_compras_proveedor").text(response);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
            $("#proveedor_nombre").val("");
        }
    });
}
function MostrarProveedoresFacturas(idproveedor) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../PROVEEDORES/MostrarProveedoresFacturas",
        data: {
            idproveedor: idproveedor
        },
        success: function (response) {
            try {
                $("#proveedores_facturas_hist").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_factura_proveedor").html(response);
            $('#proveedores_facturas_hist').DataTable({
                keys: false,
                ordering: true,
                select: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [],
                responsive: !0
            });
            $("#m_factura_proveedor").modal("show");
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
            $("#m_factura_proveedor").modal("hide");
        }
    });
}

function SYNC_EMPLEADO(id_prov_ms) {
    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 999,
        title: 'ATENCIÓN',
        message: '¿Está seguro de sincronizar el proveedor? Se sincronizará su información de Microsip',
        position: 'center',
        buttons: [
            ['<button><b>Si, sincronizar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                jsShowWindowLoad();
                $.ajax({
                    type: "POST",
                    async: false,
                    timeout: 90000,
                    url: "../PROVEEDORES/SYNC_EMPLEADO",
                    data: { id_prov_ms: id_prov_ms },
                    success: function (response) {
                        jsRemoveWindowLoad();
                        var data = $.parseJSON(response);
                        if (data.Activo == 1) {
                            iziToast.success({
                                title: data.Message,
                                message: '',
                            });
                            ConsultarProveedores();
                        }
                        else {
                            iziToast.error({
                                title: data.Message,
                                message: '',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                        jsRemoveWindowLoad();
                        $("#m_factura_proveedor").modal("hide");
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

//#endregion


//#regiion------------- SOLICITUDES DE PROVEEDORES
function ConsultarSolicitudesRegistroProveedor(id_status) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../PROVEEDORES/ConsultarSolicitudesRegistroProveedor",
        data: {
            id_status: id_status
        },
        success: function (response) {
            var div = "";
            var table = "";
            jsRemoveWindowLoad();
            if (id_status == 1) {  //REGISTRADA
                div = "div_solicitudes_pend";
                tabla = "table_solicitudes_pend";
            }
            else if (id_status == 4) {
                div = "div_solicitudes_aut";
                tabla = "table_solicitudes_aut";
            }
            else if (id_status == 3) {
                div = "div_solicitudes_rech";
                tabla = "table_solicitudes_rech";
            }
            $("#" + div + "").html(response);
            $("#" + tabla + "").DataTable({
                keys: false,
                select: true,
                ordering: false,
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
                pageLength: 20
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarDetalleSolicitud(id_solicitud) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../PROVEEDORES/ConsultarDetalleSolicitud",
        data: { id_solicitud: id_solicitud },
        success: function (response) {
            $("#div_detalle_solicitud").html(response);
            $("#m_detalle_solicitud").modal("show");
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
            $("#m_detalle_solicitud").modal("hide");
        }
    });
}

function AutorizarRechazarSolicitudRegistroProveedor(id_solicitud, id_status, razon_social) {
    var msj = "";
    if (id_status == 2) { msj = "Si, autorizar la solicitud"; }
    else if (id_status == 3) { msj = "Si, rechazar la solicitud"; }

    else { msj = "Si, ejecutar la acción seleccionada"; }

    if (id_status == 2) {  //AUTORIZAR Y SINCRONIZAR A MICROSIP
        ConsultarConsecutivoCxP(razon_social, id_solicitud);
        $("#m_detalle_solicitud").modal("hide");
        iziToast.question({
            timeout: null,
            titleSize: '20px',
            messageSize: '18px',
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 9999999,
            title: 'ADVERTENCIA LOS DATOS DE LA SOLICITUD SE REGISTRARÁN EN MICROSIP',
            message: '¿Desea realizar esta operación?',
            position: 'center',

            color: "blue",
            maxWidth: 1000,
            icon: 'icon-contacts',
            displayMode: 2,
            progressBarColor: 'rgb(0, 255, 184)',
            image: '/Content/img_layout/Logo_beta_icono.png',
            imageWidth: 70,
            layout: 2,

            drag: false,
            inputs: [
                ["<strong>Cuenta CxP: </strong><input type='text' id='no_cuenta_cxp' style='width:300px;'>", "keyup", function (instance, toast, input, e) {
                    console.info(input.value);
                }, true]
            ],
            buttons: [
                ['<button><b>' + msj + '</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    var cta_cxp = $("#no_cuenta_cxp").val();
                    if (cta_cxp == "" || cta_cxp == undefined) {
                        iziToast.warning({
                            title: 'Ingrese la cuenta CxP',
                            message: '',
                        });
                        return;
                    }
                    else {
                        jsShowWindowLoad();
                        $.ajax({
                            type: "POST",
                            async: true,
                            url: "../PROVEEDORES/AutorizarRechazarSolicitudRegistroProveedor",
                            data: { id_solicitud: id_solicitud, id_status: id_status, cta_cxp: cta_cxp },
                            success: function (response) {
                                jsRemoveWindowLoad();
                                if (response == 0) {
                                    $("#m_detalle_solicitud").modal("hide");
                                    ConsultarSolicitudesRegistroProveedor(1);
                                    ConsultarSolicitudesRegistroProveedor(3);
                                    ConsultarSolicitudesRegistroProveedor(4);
                                    if (id_status == 2) {
                                        iziToast.success({
                                            title: 'Solicitud autorizada correctamente',
                                            message: '',
                                        });
                                    }
                                    else {
                                        iziToast.success({
                                            title: 'Acción ejecutada correctamente',
                                            message: '',
                                        });
                                    }
                                }

                                else if (response == -1) {
                                    iziToast.error({
                                        title: 'ERROR AL CONSULTAR LA SOLICITUD',
                                        message: '',
                                    });
                                }
                                else if (response == -2) {
                                    iziToast.warning({
                                        title: 'LA SOLICITUD YA FUE AUTORIZADA',
                                        message: '',
                                    });
                                }
                                else if (response == -3) {
                                    iziToast.warning({
                                        title: 'LA SOLICITUD ESTÁ RECHAZADA',
                                        message: '',
                                    });
                                }
                                else if (response == -4) {
                                    iziToast.warning({
                                        title: 'LA SOLICITUD YA FUE SINCRONIZADA',
                                        message: '',
                                    });
                                }

                                else if (response == -5) {
                                    iziToast.error({
                                        title: 'ERROR LA CUENTA CxP YA ESTÁ REGISTRADA EN MICROSIP',
                                        message: '',
                                    });
                                }
                                else if (response == -6) {
                                    iziToast.error({
                                        title: 'OCURRIÓ UN ERROR AL GUARDAR LA INFORMACIÓN EN MICROSIP (información repetida)',
                                        message: '',
                                    });
                                }
                            },
                            error: function (xhr, status, error) {
                                console.error(error);
                                jsRemoveWindowLoad();
                                $("#m_detalle_solicitud").modal("hide");
                            }
                        });
                    }
                }, true],
                ['<button>Regresar</button>', function (instance, toast) {

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
    else {
        iziToast.question({
            timeout: 20000,
            close: false,
            overlay: true,
            displayMode: 'once',
            id: 'question',
            zindex: 9999,
            title: 'ADVERTENCIA',
            message: '¿Desea realizar esta opción?',
            position: 'center',
            buttons: [
                ['<button><b>' + msj + '</b></button>', function (instance, toast) {
                    instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                    jsShowWindowLoad();
                    $.ajax({
                        type: "POST",
                        async: true,
                        url: "../PROVEEDORES/AutorizarRechazarSolicitudRegistroProveedor",
                        data: { id_solicitud: id_solicitud, id_status: id_status, cta_cxp: "" },
                        success: function (response) {
                            jsRemoveWindowLoad();
                            if (response == 0) {
                                $("#m_detalle_solicitud").modal("hide");
                                ConsultarSolicitudesRegistroProveedor(1);
                                ConsultarSolicitudesRegistroProveedor(3);
                                ConsultarSolicitudesRegistroProveedor(4);
                                if (id_status == 2) {
                                    iziToast.success({
                                        title: 'Solicitud autorizada correctamente',
                                        message: '',
                                    });
                                }
                                else if (id_status == 3) {
                                    iziToast.success({
                                        title: 'Solicitud rechazada correctamente',
                                        message: '',
                                    });
                                }
                                else {
                                    iziToast.success({
                                        title: 'Acción ejecutada correctamente',
                                        message: '',
                                    });
                                }
                            }
                            else if (response == -1) {
                                iziToast.error({
                                    title: 'ERROR AL CONSULTAR LA SOLICITUD',
                                    message: '',
                                });
                            }
                            else if (response == -2) {
                                iziToast.warning({
                                    title: 'LA SOLICITUD YA FUE AUTORIZADA',
                                    message: '',
                                });
                            }
                            else if (response == -3) {
                                iziToast.warning({
                                    title: 'LA SOLICITUD ESTÁ RECHAZADA',
                                    message: '',
                                });
                            }
                            else if (response == -4) {
                                iziToast.warning({
                                    title: 'LA SOLICITUD YA FUE SINCRONIZADA',
                                    message: '',
                                });
                            }

                            else if (response == -5) {
                                iziToast.error({
                                    title: 'ERROR LA CUENTA CxP YA ESTÁ REGISTRADA EN MICROSIP',
                                    message: '',
                                });
                            }

                        },
                        error: function (xhr, status, error) {
                            console.error(error);
                            jsRemoveWindowLoad();
                            $("#m_detalle_solicitud").modal("hide");
                        }
                    });

                }, true],
                ['<button>Regresar</button>', function (instance, toast) {

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

function EditarSolicitudRegistroProveedor(id_solicitud) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../PROVEEDORES/EditarSolicitudRegistroProveedor",
        data: { id_solicitud: id_solicitud },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#div_detalle_solicitud").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ActualizarSolicitudRegistroProveedor(id_solicitud) {
    var nombre = $("#txt_nombre_prov").val();
    var razon_social = $("#txt_razon_social").val();
    var rfc = $("#txt_rfc").val();
    var correo = $("#txt_correo").val();
    var contacto = $("#txt_contacto").val();
    var telefono = $("#txt_telefono").val();
    var clave_inter = $("#txt_clave_inter").val();

    if (nombre == "" || razon_social == "" || rfc == "" || correo == "" || contacto == "" || telefono == "" || clave_inter == "") {
        iziToast.error({
            title: 'Ingrese todos los datos',
            message: '',
        });
        return;
    }

    iziToast.question({
        timeout: 20000,
        close: false,
        overlay: true,
        displayMode: 'once',
        id: 'question',
        zindex: 9999999,
        title: 'ATENCIÓN',
        message: '¿Está seguro de modificar la solicitud?',
        position: 'center',
        buttons: [
            ['<button><b>Si, actualizar</b></button>', function (instance, toast) {
                instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                $.ajax({
                    type: "POST",
                    async: true,
                    url: "../PROVEEDORES/ActualizarSolicitudRegistroProveedor",
                    data: {
                        id_solicitud: id_solicitud,
                        nombre: nombre,
                        razon_social: razon_social,
                        rfc: rfc,
                        correo: correo,
                        contacto: contacto,
                        telefono: telefono,
                        clave_inter: clave_inter
                    },
                    success: function (response) {
                        if (response == 0) {
                            iziToast.success({
                                title: 'Solicitud aztualizada correctamente',
                                message: '',
                            });
                            ConsultarDetalleSolicitud(id_solicitud);
                        }
                        else {
                            iziToast.error({
                                title: 'Ocurrió un error al actualizar la solicitud',
                                message: '',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                        jsRemoveWindowLoad();
                        $("#m_factura_proveedor").modal("hide");
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


function ConsultarConsecutivoCxP(razon_social, id_solicitud) {
    $.ajax({
        type: "POST",
        async: true,
        url: "../PROVEEDORES/ConsultarConsecutivoCxP",
        data: {
            razon_social: razon_social,
            id_solicitud: id_solicitud
        },
        success: function (response) {
            $("#no_cuenta_cxp").val(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
            $("#m_factura_proveedor").modal("hide");
        }
    });
}

function GenerarTokenRegistro() {
    var nombre_token = $("#nombre_token").val();
    $.ajax({
        type: "POST",
        async: true,
        url: "../PROVEEDORES/GenerarTokenRegistro",
        data: {
            nombre_token: nombre_token
        },
        success: function (response) {
            if (response == 0) {
                $("#m_registrar_token").modal("hide");
                ConsultarTokensGenerados();
                iziToast.success({
                    title: 'Exito',
                    message: '¡Token generado correctamente!',
                });
            }
            else if (response == -1) {
                iziToast.warning({
                    title: 'Demasiados intentos para generar un Token',
                    message: 'Intente mas tarde',
                });
            }
            else {
                iziToast.error({
                    title: 'Ocurrió un error al generar el token',
                    message: '',
                });
            }
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
            $("#m_factura_proveedor").modal("hide");
        }
    });
}

function ConsultarTokensGenerados() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../PROVEEDORES/ConsultarTokensGenerados",
        data: {
        },
        success: function (response) {
            $("#div_tokens_generados").html(response);
            jsRemoveWindowLoad();

            $("#div_tokens_generados").html(response);
            $("#datatable_tokens_generados").DataTable({
                keys: false,
                select: true,
                ordering: false,
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
                pageLength: 20
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function CopiarTokenPortapapeles(textToCopy) {
    // Create a temporary textarea element to hold the text
    var tempTextArea = document.createElement("textarea");
    tempTextArea.value = textToCopy;
    document.body.appendChild(tempTextArea);

    // Select the text in the textarea
    tempTextArea.select();

    // Copy the selected text
    document.execCommand("copy");

    // Remove the textarea element
    document.body.removeChild(tempTextArea);
    iziToast.success({
        title: 'Codigo copiado en portapapeles',
        message: '',
    });
}



//#endregion




