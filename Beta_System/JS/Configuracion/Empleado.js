//----------- EMPLEADOS
validaciones = 0;
function AddEditEmpleado(modo) {
    $("#mensaje_telefono").text('');
    $("#mensaje_correo").text('');
    campos_validar.forEach(function (campo_id) {
        var $campo = $("#" + campo_id);
        $campo.css("border-color", "");
    });
    $("#mensaje_correo").text();
    //EDITAR
    if (modo == 1) {
        $("#lbl_add_edit_empleado").text("EDITAR EMPLEADO");
        $("#btn_add_edit_emplado").text("Actualizar");
    }

    //AGREGAR
    else {
        $("#lbl_add_edit_empleado").text("REGISTRAR EMPLEADO");
        $("#btn_add_edit_emplado").text("Registrar");
        $("#btn_add_edit_emplado").attr("onclick", "RegistrarEmpleado();");
        $(".input_clear_emp").val('');
    }

    $("#m_add_edit_empleado").modal("show");
}

function ConsultarEmpleadosSistema() {
    $.ajax({
        type: "POST",
        async: true,
        url: "../EMPLEADOS/ConsultarEmpleadosSistema",
        data: {
            modo: 0
        },
        success: function (response) {
            try {
                $("#table_empleados").dataTable().fnDestroy();
            } catch (e) { }
            $("#table_empleados_admin").html(response);
            $('#table_empleados').DataTable({
                keys: true,
                search: true,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}]
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

//OBTENER DATOS EMPLEADO
function EditarEmpleado(id_empleado) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../EMPLEADOS/EditarEmpleado",
        data: {
            id_empleado: id_empleado
        },
        success: function (response) {
            jsRemoveWindowLoad();
            $("#btn_add_edit_emplado").attr("onclick", "ActualizarEmpleado(" + id_empleado + ");");

            var data = $.parseJSON(response);
            $("#nombre_empleado").val(data[0].nombres);
            $("#ap_paterno").val(data[0].apellido_paterno);
            $("#ap_materno").val(data[0].apellido_materno);
            $("#correo").val(data[0].correo);
            $("#telefono").val(data[0].telefono_celular);

        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
//MODIFICAR DATOS EMPLEADO
function ActualizarEmpleado(id_empleado) {
    ValidarTelefonoEmpleado();
    ValidarCorreosEmpleado();
    var nombres = $("#nombre_empleado").val();
    var ap_pat = $("#ap_paterno").val();
    var ap_mat = $("#ap_materno").val();
    var correo = $("#correo").val();
    var telefono_celular = $("#telefono").val();
    jsShowWindowLoad();
    var campos_revisados = campos_validar.every(function (campos_id) {
        return $("#" + campos_id).val() !== '';
    });
    if (campos_revisados && validaciones == 0) {
        $.ajax({
            type: "POST",
            async: true,
            url: "../EMPLEADOS/ActualizarEmpleado",
            data: {
                id_empleado: id_empleado,
                nombres: nombres,
                ap_pat: ap_pat,
                ap_mat: ap_mat,
                correo: correo,
                telefono_celular: telefono_celular
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Empleado actualizado corretamente!',
                    });
                    $("#m_add_edit_empleado").modal("hide");
                    ConsultarEmpleadosSistema();
                    LimpiarCamposEmpleado();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al registrar el empleado',
                    });
                }
                LimpiarCampos();
            },
            error: function (xhr, status, error) {
                console.error(error);
                jsRemoveWindowLoad();
            }
        });
    }
    else {
        campos_validar.forEach(function (campo_id) {
            var $campo = $("#" + campo_id);
            if ($campo.val() == '') {
                $campo.css("border-color", "red");
            }
        });
        iziToast.warning({
            title: 'Aviso',
            message: '¡Campos obligatorios!',
        });
        jsRemoveWindowLoad();
    }
}
var campos_validar = ["nombre_empleado", "ap_paterno", "ap_materno", "correo", "telefono"];
//REGISTRAR DATOS EMPLEADO
function RegistrarEmpleado() {
    ValidarTelefonoEmpleado();
    ValidarCorreosEmpleado();
    var c_empleados = {};
    c_empleados.nombres = $("#nombre_empleado").val();
    c_empleados.apellido_paterno = $("#ap_paterno").val();
    c_empleados.apellido_materno = $("#ap_materno").val();
    c_empleados.correo = $("#correo").val();
    c_empleados.telefono_celular = $("#telefono").val();
    jsShowWindowLoad();

    var campos_revisados = campos_validar.every(function (campos_id) {
        return $("#" + campos_id).val() !== '';
    });

    if (campos_revisados && validaciones == 0) {
        $.ajax({
            type: "POST",
            async: true,
            url: "../EMPLEADOS/RegistrarEmpleado",
            data: {
                c_empleados: c_empleados
            },
            success: function (response) {     
                LimpiarCampos();
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Empleado registrado corretamente!',
                    });
                    ConsultarEmpleadosSistema();
                    LimpiarCamposEmpleado();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al registrar el empleado',
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
    else {
        jsRemoveWindowLoad();
        campos_validar.forEach(function (campo_id) {
            var $campo = $("#" + campo_id);
            if ($campo.val() == '') {
                $campo.css("border-color", "red");
            }
        });
        iziToast.warning({
            title: 'Aviso',
            message: '¡Campos obligatorios!',
        });
    }
}
//LIMPIAR BORDERS DE LOS CAMPOS
function LimpiarKeyPressEMP() {
    campos_validar.forEach(function (campo_id) {
        var $campo = $("#" + campo_id);
        if ($campo.val() != '') {
            $campo.css("border-color", "");
        }
    });
}
//VALIDAR CAMPOS Y ASIGNAR BORDES
function ValidarCorreosEmpleado() {
    var correo = $("#correo").val();
    var emailRegex = /^[a-zA-Z0-9]([a-zA-Z0-9_.]*[a-zA-Z0-9])?@[a-zA-Z0-9]+(\.[a-zA-Z]{2,})+$/;

    if (!emailRegex.test(correo)) {
        $("#mensaje_correo").text('¡Correo inválido!');
        $("#mensaje_correo").css("color", "red");
        $("#mensaje_correo").css("border-color", "red");
        validaciones = 1;
    } else {
        $("#mensaje_correo").text('¡Correo válido!');
        $("#mensaje_correo").css("color", "green");
        validaciones = 0;
    }

}
//LIMPIAR CAMPOS
function LimpiarCamposEmpleado() {
    campos_validar.forEach(function (campo_id) {
        var $campo = $("#" + campo_id);
        if ($campo.val() != '') {
            $campo.val();
        }
    });
}

function ValidarTelefonoEmpleado() {
    var telefono = $("#telefono").val();
    if (telefono.length == 7 || telefono.length == 10) {
        $("#mensaje_telefono").text('¡Telefono valido!');
        $("#mensaje_telefono").css("color", "green");
    }
    else {
        $("#mensaje_telefono").text('¡Telefono invalido!');
        $("#mensaje_telefono").css("color", "red");
        $("#telefono").css("border-color", "red");
        validaciones = 1;
    }
}
//LIMPIAR CAMPOS
function LimpiarCampos() {
    campos_validar.forEach(function (campo_id) {
        var $campo = $("#" + campo_id);
        $campo.val('');
    });
    $("#mensaje_telefono").text('');
    $("#mensaje_correo").text('');
}