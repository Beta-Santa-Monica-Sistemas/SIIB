//#region CATALOGO ARTICULOS
function LimpiarCatalogoArticulo() {
    $("#articulo_nombre").val('');
    $("#articulo_medida").prop('selectedIndex', 0);
    $("#articulo_status").prop("checked", false);
}

function ConsultarArticulosCatalogoBascula() {
    var estatus = $("#id_activo_articulo").val();
    jsShowWindowLoad();
    $("#div_tabla_articulos").html('');
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarArticulosCatalogoBascula",
        data: { estatus: estatus },
        success: function (response) {
            try {
                $("#tabla_bascula_articulos").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_articulos").html(response);
            $('#tabla_bascula_articulos').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ModalCatalogoArticulo() {
    LimpiarCatalogoArticulo();
    $("#m_bascula_articulos").modal("show");
    $("#articulo_medida").prop('selectedIndex', 0);
    $("#articulo_status").prop('disabled', true);
    $("#articulo_status").prop('checked', true);

    $("#m_bascula_articulos_lbl").text("Registrar articulos");
    $("#bascula_articulos_btn").attr("onclick", "RegistraArticuloCatalogoBascula()");
}

function RegistraArticuloCatalogoBascula() {
    var nombre = $('#articulo_nombre').val().toUpperCase();
    var medida = $('#articulo_medida').val();
    if ($('#articulo_nombre').val().trim() != "") {
        $.ajax({
            type: "POST",
            async: false,
            url: "../CONFIGURACION/RegistrarArticulosCatalogoBascula",
            data: {
                nombre: nombre,
                medida: medida
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Se registro el articulo correctamente',
                    });
                    LimpiarCatalogoArticulo();
                    $("#articulo_status").prop("checked", true);
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al registrar el articulo',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar un nombre del articulo'
        });
    }
}

function AsignarInformacionArticuloCatalogoBascula(id_bascula_articulo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/AsignarInformacionArticuloCatalogoBascula",
        data: { id_bascula_articulo: id_bascula_articulo },
        success: function (response) {
            if (response != "[]") {
                $("#m_bascula_articulos_lbl").text("Modificar articulo");
                $("#bascula_articulos_btn").attr("onclick", "ModificarArticuloCatalogoBascula(" + id_bascula_articulo + ")");

                LimpiarCatalogoArticulo();
                $('#articulo_status').prop('disabled', false);
                $('#m_bascula_articulos').modal("show");
                var data = $.parseJSON(response);
                $("#articulo_nombre").val(data[0].nombre_articulo);
                $("#articulo_medida").val(data[0].id_unidad_medida);
                var activo = data[0].activo;
                if (activo == true) { $("#articulo_status").prop('checked', true) }
                else { $("#articulo_status").prop('checked', false) }
            }
            else {
                iziToast.warning({
                    title: '',
                    message: 'No se encontró informacion',
                });
            }

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModificarArticuloCatalogoBascula(id_bascula_articulo) {
    var nombre = $('#articulo_nombre').val().toUpperCase();
    var medida = $('#articulo_medida').val();
    var activo = false;
    if ($("#articulo_status").is(':checked')) {
        activo = true;
    }

    if ($('#articulo_nombre').val().trim() != "") {
        $.ajax({
            type: "POST",
            async: false,
            url: "../CONFIGURACION/ModificarArticuloCatalogoBascula",
            data: {
                id_bascula_articulo: id_bascula_articulo,
                nombre: nombre,
                medida: medida,
                activo: activo
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Se modifico el articulo correctamente',
                    });
                    LimpiarCatalogoArticulo();
                    $('#m_bascula_articulos').modal("hide");
                    $("#bascula_articulos_btn").attr("onclick", "");
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema al modificar el articulo',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar un nombre del articulo'
        });
    }
}

function OnOffArticuloCatalogoBascula(id_articulo, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/OnOffArticuloCatalogoBascula",
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
                ConsultarArticulosCatalogoBascula();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al cambiar el estatus',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
//#endregion

//#region CATALOGO PROVEEDORES
function LimpiarCatalogoProveedor() {
    $("#proveedor_clave").val('');
    $("#proveedor_nombre").val('');
    $("#proveedor_status").prop("checked", false);
}

function ConsultarProveedoresCatalogoBascula() {
    var estatus = $("#id_activo_proveedor").val();
    jsShowWindowLoad();
    $("#div_tabla_proveedores").html('');
    $.ajax({
        type: "POST",
        async: true,
        url: "../CONFIGURACION/ConsultarProveedoresCatalogoBascula",
        data: { estatus: estatus },
        success: function (response) {
            try {
                $("#tabla_bascula_proveedores").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_proveedores").html(response);
            $('#tabla_bascula_proveedores').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ModalCatalogoProveedores() {
    LimpiarCatalogoProveedor();
    $("#m_bascula_proveedores").modal("show");
    $("#proveedor_status").prop('disabled', true);
    $("#proveedor_status").prop('checked', true);

    $("#m_bascula_proveedores_lbl").text("Registrar proveedores");
    $("#bascula_proveedor_btn").attr("onclick", "RegistrarProveedoresCatalogoBascula()");
}

function ValidarProveedoresCatalogoBascula() {
    var clave = $('#proveedor_clave').val();
    return new Promise(function (resolve, reject) {
        $.ajax({
            type: "POST",
            url: "../CONFIGURACION/ValidarProveedoresCatalogoBascula",
            data: { clave: clave },
            success: function (response) {
                if (response == "True") {
                    iziToast.warning({
                        title: 'Aviso',
                        message: 'La clave no está disponible, favor de ingresar una diferente',
                    });
                    resolve(false);  // La clave no está disponible
                } else {
                    resolve(true);  // La clave está disponible
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
                reject(error);  // Error en la llamada AJAX
            }
        });
    });
}

function RegistrarProveedoresCatalogoBascula() {
    var nombre = $('#proveedor_nombre').val().toUpperCase();
    var clave = $('#proveedor_clave').val();

    if ($('#proveedor_nombre').val().trim() != "" && $('#proveedor_clave').val().trim() != "") {
        ValidarProveedoresCatalogoBascula().then(function (isValid) {
            if (isValid) {
                $.ajax({
                    type: "POST",
                    url: "../CONFIGURACION/RegistrarProveedoresCatalogoBascula",
                    data: {
                        nombre: nombre,
                        clave: clave
                    },
                    success: function (response) {
                        if (response == "True") {
                            iziToast.success({
                                title: 'Éxito',
                                message: 'Se registró al proveedor correctamente',
                            });
                            LimpiarCatalogoProveedor();
                            $("#proveedor_status").prop("checked", true);
                        } else {
                            iziToast.error({
                                title: 'Error',
                                message: 'Ocurrió un problema al registrar al proveedor',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
            }
        }).catch(function (error) {
            console.error('Error en la validación: ', error);
        });
    } else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar un nombre y clave del proveedor'
        });
    }
}


function AsignarInformacionProveedoresCatalogoBascula(id_bascula_proveedor, clave) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/AsignarInformacionProveedoresCatalogoBascula",
        data: { id_bascula_proveedor: id_bascula_proveedor },
        success: function (response) {
            if (response != "[]") {
                $("#m_bascula_proveedores_lbl").text("Modificar proveedor");
                $("#bascula_proveedor_btn").attr("onclick", "ModificarProveedoresCatalogoBascula(" + id_bascula_proveedor + ",'" + clave + "')");

                LimpiarCatalogoArticulo();
                $('#proveedor_status').prop('disabled', false);
                $('#m_bascula_proveedores').modal("show");
                var data = $.parseJSON(response);
                $("#proveedor_nombre").val(data[0].nombre_prov);
                $("#proveedor_clave").val(data[0].clave_prov);
                var activo = data[0].activo;
                if (activo == true) { $("#proveedor_status").prop('checked', true) }
                else { $("#proveedor_status").prop('checked', false) }
            }
            else {
                iziToast.warning({
                    title: '',
                    message: 'No se encontró informacion',
                });
            }

        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ModificarProveedoresCatalogoBascula(id_bascula_proveedor, clave_anterior) {
    var nombre = $('#proveedor_nombre').val().toUpperCase();
    var clave = $('#proveedor_clave').val();
    var activo = false;
    if ($("#proveedor_status").is(':checked')) {
        activo = true;
    }

    if ($('#proveedor_nombre').val().trim() != "") {
        if (clave_anterior == clave || ValidarProveedoresCatalogoBascula() == 1) {
            $.ajax({
                type: "POST",
                async: false,
                url: "../CONFIGURACION/ModificarProveedoresCatalogoBascula",
                data: {
                    id_bascula_proveedor: id_bascula_proveedor,
                    nombre: nombre,
                    clave: clave,
                    activo: activo
                },
                success: function (response) {
                    if (response == "True") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se modifico el proveedor correctamente',
                        });
                        LimpiarCatalogoArticulo();
                        $('#m_bascula_proveedores').modal("hide");
                        $("#bascula_proveedor_btn").attr("onclick", "");
                    }
                    else {
                        iziToast.error({
                            title: 'Error',
                            message: 'Ocurrió un problema al modificar al proveedor',
                        });
                    }
                },
                error: function (xhr, status, error) {
                    console.error(error);
                }
            });
        }
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar un nombre del proveedor'
        });
    }
}

function OnOffProveedoresCatalogoBascula(id_bascula_proveedor, modo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/OnOffProveedoresCatalogoBascula",
        data: {
            id_bascula_proveedor: id_bascula_proveedor,
            modo: modo
        },
        success: function (response) {
            if (response == "True") {
                iziToast.success({
                    title: 'Exito',
                    message: 'Cambio de estatus correctamente',
                });
                ConsultarProveedoresCatalogoBascula();
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al cambiar el estatus',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}
//#endregion


//#region ASOCIACION PROVEEDORES BASCULA - PROVEEDORES COMPRAS

function ConsultarProveedoresAsociadosBasculaCompras() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        timeput: 900000,
        url: "../CONFIGURACION/ConsultarProveedoresAsociadosBasculaCompras",
        data: {},
        success: function (response) {
            try {
                $("#datatable_proveedores_asociados").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_proveedores_asociados").html(response);
            $('#datatable_proveedores_asociados').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                select: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarProveedoresSinAsociacionBascula() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/ConsultarProveedoresSinAsociacionBascula",
        data: { },
        success: function (response) {
            try {
                $("#datatable_proveedores_bascula").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_proveedores_bascula").html(response);
            $('#datatable_proveedores_bascula').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                select: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarProveedoresSinAsociacionCompras() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../CONFIGURACION/ConsultarProveedoresSinAsociacionCompras",
        data: {},
        success: function (response) {
            try {
                $("#datatable_proveedores_compras").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_proveedores_compras").html(response);
            $('#datatable_proveedores_compras').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                select: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                pagingType: 'simple_numbers'
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function AsociarProveedorBasculaProveedorCompras() {
    var id_proveedor_compras = $('input[name="id_proveedor_compras_asociacion"]:checked').val();
    var id_proveedor_bascula = $('input[name="id_proveedor_bascula_asociacion"]:checked').val();
    if (id_proveedor_bascula == undefined || id_proveedor_compras == undefined) {
        iziToast.warning({
            title: 'Seleccione un proveedor de cada listado',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../CONFIGURACION/AsociarProveedorBasculaProveedorCompras",
            data: {
                id_proveedor_bascula: id_proveedor_bascula,
                id_proveedor_compras: id_proveedor_compras
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    iziToast.success({
                        title: 'Asociacion generada correctamente',
                        message: '',
                    });
                    ConsultarProveedoresSinAsociacionBascula();
                    ConsultarProveedoresSinAsociacionCompras();
                }
                else if (response == 1) {
                    iziToast.error({
                        title: 'Ya existe una asociación para estos dos proveedores',
                        message: '',
                    });
                }
                else {
                    iziToast.error({
                        title: 'Ocurrió un error al asociar los proveedores',
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
}

function EliminarAsociacionProveedoresBasculaCompras(id_proveedor_bascula_compras) {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        timeput: 900000,
        url: "../CONFIGURACION/EliminarAsociacionProveedoresBasculaCompras",
        data: { id_proveedor_bascula_compras: id_proveedor_bascula_compras },
        success: function (response) {
            if (response == 0) {
                iziToast.success({
                    title: 'Asociacion eliminada correctamente',
                    message: '',
                });
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
//#endregion