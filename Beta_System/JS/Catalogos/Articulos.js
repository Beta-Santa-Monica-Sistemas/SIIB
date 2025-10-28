//#region CRUD ARTICULOS
function RegistrarArticulo() {
    var nombreValue = $.trim($("#nombre").val());
    var c_articulo = {};
    c_articulo.clave = $("#codigo_articulo").val();
    c_articulo.nombre_articulo = $("#nombre").val();
    c_articulo.descripcion_articulo = $("#descrip").val();
    c_articulo.id_unidad_medida = $("#id_medida").val();
    c_articulo.id_articulo_tipo = $("#id_tipo").val();
    c_articulo.id_articulo_clasificacion = $("#id_clasificacion").val();
    c_articulo.id_articulo_linea = null;
    c_articulo.id_marca = $("#id_marca_articulo").val();
    if ($("#codigo_articulo").val() != "") {
        if ($("#almacenable").is(':checked')) {
            c_articulo.almacenable = true;
        }
        else {
            c_articulo.almacenable = false;
        }

        if ($("#chk_disponible_bascula").is(':checked')) {
            c_articulo.disponible_bascula = true;
        }
        else {
            c_articulo.disponible_bascula = false;
        }
        c_articulo.id_articulo_tipo_requisicion = $("#id_tipo_requisicion").val();
        if ($("#nombre").val() != '' && nombreValue.length > 0) {
            $.ajax({
                type: "POST",
                async: false,
                url: "../ARTICULOS/RegistarArticulo",
                data: {
                    c_articulo: c_articulo
                },
                success: function (response) {
                    if (response == 0) {
                        //Obtener ID REGISTRO
                        iziToast.success({
                            title: 'Exito',
                            message: 'Se registro correctamente el articulo',
                        });
                        $(".input_valid").each(function () {
                            $(this).css("border", "");
                        });
                        $("#almacenable").prop("checked", false);
                        $(".input_clear_art").val('');
                        $("#id_clasificacion").val(1);
                        $("#id_medida").val(11);

                        $("#id_tipo").val(12);
                        $("#id_tipo_requisicion").val(1);
                        //ConsultarArticuloCompletos(0);
                        $("#m_agregar_editar_articulo").modal("hide");
                    }
                    else if (response == -1) {
                        iziToast.warning({
                            title: 'Alguien mas ya registró el articulo con esa clave.',
                            message: 'Intente de nuevo',
                        });
                        ConsultarConsecutivoArticulo();
                    }
                    else if (response == 1) {
                        //Mensaje Fallo
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
                title: 'Alerta',
                message: 'Campo Obligatorio',
            });
            $(".input_valid").each(function () {
                if ($(this).val() == "") { $(this).css("border-color", "red"); }
                else { $(this).css("border", ""); }
            });
            $("#nombre").css("border-color", "red");
            $("#nombre").val('');
        }
    }
    else {
        iziToast.warning({
            title: 'Alerta',
            message: 'La clave es un campo obligatorio',
        });
    }
}

function ModificarArticulo(id_articulo) {
    var nombre_articulo = $("#nombre").val();
    var descripcion = $("#descrip").val();
    var unidad_medida = $("#id_medida").val();
    var articulo_tipo = $("#id_tipo").val();
    var artic_clasif = $("#id_clasificacion").val();
    //var clave = $("#codigo_articulo").val();
    var artic_linea = null;
    var rubro = $("#id_tipo_requisicion").val();

    if ($("#almacenable").is(':checked')) {
        var almaenable = true;
    }
    else {
        var almaenable = false;
    }
    if ($("#id_estatus").is(':checked')) {
        var estatus = true;
    }
    else {
        var estatus = false;
    }
    var disponible_bascula = false;
    if ($("#chk_disponible_bascula").is(':checked')) {
        disponible_bascula = true;
    }
    if ($("#nombre").val() != '') {
        $.ajax({
            type: "POST",
            async: false,
            url: "../ARTICULOS/ModificarArticulos",
            data: {
                id_articulo: id_articulo,
                nombre_articulo: nombre_articulo,
                descripcion: descripcion,
                unidad_medida: unidad_medida,
                articulo_tipo: articulo_tipo,
                artic_clasif: artic_clasif,
                almaenable: almaenable,
                estatus: estatus,
                rubro: rubro,
                disponible_bascula: disponible_bascula
                //clave: clave
            },
            success: function (response) {
                if (response == "True") {

                    $("#m_agregar_editar_articulo").modal("hide");


                    if ($("#busqueda_especifica").hasClass('active')) {
                        ConsultarArticuloCompletosFiltrado();

                    } else if ($("#busqueda_filtro").hasClass('active')) {
                        ConsultarArticuloCompletos(1);
                    }
                    iziToast.success({
                        title: 'Exito',
                        message: 'Articulo actualizado corretamente!',
                    });
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
        $(".input_clear_art").val('');
    }
    else {
        iziToast.warning({
            title: 'Alerta',
            message: 'Campo Obligatorio',
        });
        $(".input_valid").each(function () {
            if ($(this).val() == "") { $(this).css("border-color", "red"); }
            else { $(this).css("border", ""); }
        });
    }
}


var articulo_activo;
function ObtenerInfArticulo(id_articulo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ObtenerInfoArticulo",
        data: {
            id_articulo: id_articulo
        },
        success: function (response) {
            $("#btn_accion_realizar").attr("onclick", "ModificarArticulo(" + id_articulo + ");");
            var data = $.parseJSON(response);
            $("#nombre").val(data[0].nombre_articulo);
            $("#descrip").val(data[0].descripcion_articulo);
            $("#id_medida").val(data[0].id_unidad_medida);
            $("#id_tipo").val(data[0].id_articulo_tipo);
            $("#id_clasificacion").val(data[0].id_articulo_clasificacion);
            $("#id_tipo_requisicion").val(data[0].id_articulo_tipo_requisicion);
            var almacenable = data[0].almacenable;
            if (almacenable == true) {
                $("#almacenable").prop('checked', true);
            }
            else {
                $("#almacenable").prop('checked', false);
            }
            var estatus = data[0].activo;
            if (estatus == true) {
                $("#id_estatus").prop('checked', true);
            }
            else {
                $("#id_estatus").prop('checked', false);
            }


            var disponible_bascula = data[0].disponible_bascula;
            if (disponible_bascula == true) {
                $("#chk_disponible_bascula").prop('checked', true);
            }
            else {
                $("#chk_disponible_bascula").prop('checked', false);
            }
            $("#id_marca_articulo").val(data[0].id_marca);
            $("#codigo_articulo").val(data[0].clave);
            TipoArticuloRequi();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ObtenerPrecArticulo(id_articulo) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarPrecioArticulo",
        data: {
            id_articulo: id_articulo
        },
        success: function (response) {
            $("#precio_articulo").val(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function OnOffArticulo(id_articulo, modo) {
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

function OnOffAlmacenable(id_articulo, modo) {
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

function LimpiarNombre() {
    $("#nombre").css("border-color", "");
}

function Almacenable(accion) {
    if (accion == 1) {
        $("#almacenable").prop("checked", false);
    }
}
function TipoArticuloRequi() {
    var umedida = $("#id_medida").val();
    var clasificacion = $("#id_clasificacion").val();
    var tipo = $("#id_tipo").val();
    var rubro = $("#id_tipo_requisicion").val();
    if (rubro == 2) {
        $("#id_medida").val(10);
        $("#id_clasificacion").val(52);
        $("#id_tipo").val(16);

        $("#almacenable").prop("checked", false);
        $("#almacenable").attr("onclick", "Almacenable(1);");
    }
    else {
        $("#almacenable").attr("onclick", "Almacenable(2);");
    }
}


//#endregion


//#region ARTICULOS PRECIO
function CargarInfoArticuloSinPrecio(id_articulo) {
    $("#m_articulo_precio").modal("show");
    $("#btn_accion_realizar_precio").attr("onclick", "RegistrarPrecioArticulo();");
    $("#articulo_precio").val('')
    $("#div_lupa_precio").css("display", "block");
    $("#nombre_articulo_precio").val('');
    $("#codigo_articulo_precio").val('');
    $("#id_articulo_registro_con_precio").text('');
    $("#id_articulo_registro_sin_precio").text('');
    $("#codigo_articulo_precio").prop("disabled", false);
    $("#nombre_articulo_precio").prop("disabled", false);
    $("#articulo_precio").prop("disabled", false);
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/CargarInfoArticuloSinPrecio",
        data: {
            id_articulo_precio: id_articulo
        },
        success: function (response) {
            if (response != null) {
                var data = $.parseJSON(response);
                $("#nombre_articulo_precio").val(data[0].nombre_articulo);
                $("#codigo_articulo_precio").val(data[0].clave);
                $("#id_articulo_registro_sin_precio").text(data[0].id_articulo);
            }
            else {
                $("#nombre_articulo_precio").val('');
                $("#codigo_articulo_precio").val('');
                $("#id_articulo_registro_sin_precio").text('');
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'No se encontro la informacion del articulo',
                });
            }
            $("#m_lupa_articulos_pendiente").modal("hide");
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
            iziToast.warning({
                title: '¡Aviso!',
                message: 'Ocurrio un problema al buscar el articulo',
            });
        }
    });
}
function CargarInfoArticuloSinPrecioClave() {
    var clave = $("#codigo_articulo_precio").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/CargarInfoArticuloSinPrecioClave",
        data: {
            clave: clave
        },
        success: function (response) {
            if (response != null) {
                var data = $.parseJSON(response);
                $("#nombre_articulo_precio").val(data[0].nombre_articulo);
                $("#codigo_articulo_precio").val(data[0].clave);
                $("#id_articulo_registro_sin_precio").text(data[0].id_articulo);
            }
        },
        error: function (xhr, status, error) { }
    });
}
function CargarInfoArticuloSinPrecioNombre() {
    var nombre = $("#nombre_articulo_precio").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/CargarInfoArticuloSinPrecioNombre",
        data: {
            nombre: nombre
        },
        success: function (response) {
            if (response != null) {
                var data = $.parseJSON(response);
                $("#nombre_articulo_precio").val(data[0].nombre_articulo);
                $("#codigo_articulo_precio").val(data[0].clave);
                $("#id_articulo_registro_sin_precio").text(data[0].id_articulo);
            }
        },
        error: function (xhr, status, error) { }
    });
}
function CargarInfoArticuloConPrecio(id_articulo_precio, clave, articulo, precio) {
    $("#m_articulo_precio").modal("show");
    $("#div_lupa_precio").css("display", "none");
    $("#id_articulo_registro_sin_precio").text('');
    $("#id_articulo_registro_con_precio").text(id_articulo_precio);
    $("#codigo_articulo_precio").val(clave);
    $("#nombre_articulo_precio").val(articulo);
    $("#articulo_precio").val(precio);

    $("#codigo_articulo_precio").prop("disabled", true);
    $("#nombre_articulo_precio").prop("disabled", true);
    $("#btn_accion_realizar_precio").attr("onclick", "ModificarPrecioArticulo();");
}
function ModalArticuloPrecio(valor) {
    if (valor == 1) {
        //REGISTRAR
        CaracteresControl();
        $("#m_articulo_precio").modal("show");
        $("#btn_accion_realizar_precio").attr("onclick", "RegistrarPrecioArticulo();");
        $("#articulo_precio").val('')
        $("#div_lupa_precio").css("display", "block");
        $("#nombre_articulo_precio").val('');
        $("#codigo_articulo_precio").val('');
        $("#id_articulo_registro_con_precio").text('');
        $("#id_articulo_registro_sin_precio").text('');
        $("#codigo_articulo_precio").prop("disabled", false);
        $("#nombre_articulo_precio").prop("disabled", false);
        $("#articulo_precio").prop("disabled", false);
    }
    else if (valor == 2) {
        //LUPA PENDIENTE
        $("#m_lupa_articulos_pendiente").modal("show");
    }
}
function ArticulosPendientesPrecio() {
    var id_clasif_filtro = $("#id_clasificaciones_precio").val();
    $.ajax({
        type: "POST",
        async: true,
        timeput: 100000,
        url: "../ARTICULOS/ArticulosPendientesPrecio",
        data: {
            id_clasif_filtro: id_clasif_filtro
        },
        success: function (response) {
            try {
                $("#tabla_articulos_precio").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_lupa_articulos_pendiente").html(response);
            $('#tabla_articulos_precio').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
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
function ArticulosConPrecio() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ArticulosConPrecio",
        data: {},
        success: function (response) {
            try {
                $("#tabla_articulos_con_precio").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_articulo_con_precio").html(response);
            $('#tabla_articulos_con_precio').DataTable({
                keys: false,
                select: true,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
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
function RegistrarPrecioArticulo() {
    var id_articulo = $("#id_articulo_registro_sin_precio").text();
    var precio = parseFloat($("#articulo_precio").val());
    if (id_articulo != "") {
        if (precio > 0) {
            $.ajax({
                type: "POST",
                async: false,
                url: "../ARTICULOS/RegistrarPrecioArticulo",
                data: {
                    id_articulo: id_articulo,
                    precio: precio
                },
                success: function (response) {
                    if (response == "True" || response == true) {
                        iziToast.success({
                            title: '¡Exito!',
                            message: 'Se registro el precio correctamente',
                        });
                    }
                    else {
                        iziToast.warning({
                            title: '¡Aviso!',
                            message: 'No registro el precio del articulo',
                        });
                    }

                    $("#nombre_articulo_precio").val('');
                    $("#codigo_articulo_precio").val('');
                    $("#articulo_precio").val('');
                    $("#id_articulo_registro_con_precio").text('');
                    $("#id_articulo_registro_sin_precio").text('');
                    $("#m_lupa_articulos_pendiente").modal("hide");
                    jsRemoveWindowLoad();
                },
                error: function (xhr, status, error) {
                    console.error(error);
                    jsRemoveWindowLoad();
                    iziToast.warning({
                        title: '¡Aviso!',
                        message: 'Ocurrio un problema al registrar el precio del articulo',
                    });
                }
            });
        }
        else {
            iziToast.warning({
                title: '¡Aviso!',
                message: 'Favor de ingresar un precio mayor a 0',
            });
        }
    }
    else {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Favor de indicar el articulo a registrar su precio',
        });
    }
}
function ModificarPrecioArticulo() {
    var id_articulo = $("#id_articulo_registro_con_precio").text();
    var precio = parseFloat($("#articulo_precio").val());
    if (id_articulo != "") {
        if (precio > 0) {
            $.ajax({
                type: "POST",
                async: false,
                url: "../ARTICULOS/ModificarPrecioArticulo",
                data: {
                    id_articulo_precio: id_articulo,
                    precio: precio
                },
                success: function (response) {
                    if (response == "True" || response == true) {
                        iziToast.success({
                            title: '¡Exito!',
                            message: 'Se modifico el precio correctamente',
                        });
                        ArticulosConPrecio();
                        $("#nombre_articulo_precio").val('');
                        $("#articulo_precio").val('');
                        $("#codigo_articulo_precio").val('');
                        $("#id_articulo_registro_con_precio").text('');
                        $("#id_articulo_registro_sin_precio").text('');
                        $("#m_articulo_precio").modal("hide");
                    }
                    else {
                        iziToast.warning({
                            title: '¡Aviso!',
                            message: 'No se modifico el precio del articulo',
                        });
                    }
                    jsRemoveWindowLoad();
                },
                error: function (xhr, status, error) {
                    console.error(error);
                    iziToast.warning({
                        title: '¡Aviso!',
                        message: 'Ocurrio un problema al modificar el precio del articulo',
                    });
                    jsRemoveWindowLoad();
                }
            });
        }
        else {
            iziToast.warning({
                title: '¡Aviso!',
                message: 'Favor de ingresar un precio mayor a 0',
            });
        }
    }
    else {
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Favor de indicar el articulo a editar su precio',
        });
    }
}
function ArticulosPrecioCompleto() {
    var id_clasif_filtro = $("#id_clasificaciones_precio").val();
    $.ajax({
        type: "POST",
        async: true,
        timeput: 100000,
        url: "../ARTICULOS/ArticulosPendientesPrecio",
        data: {
            id_clasif_filtro: id_clasif_filtro
        },
        success: function (response) {
            try {
                $("#tabla_articulos_precio").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_lupa_articulos_pendiente").html(response);
            $('#tabla_articulos_precio').DataTable({
                keys: false,
                ordering: true,
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
//#endregion

//#region METODO AUXILIARES
function AccionArticulo(accion) {
    //Modificar Articulo
    if (accion == 1) {
        $("#lbl_titulo_articulo").text("EDITAR ARTÍCULO");
        $("#btn_accion_realizar").text("Actualizar");

        $("#id_estatus").attr("onclick", "Parametro(2);");
    }
    //Agregar Articulo
    else {
        $("#id_clasificacion").val(1);
        $("#id_medida").val(11);
        $("#lbl_titulo_articulo").text("REGISTRAR ARTÍCULO");
        $("#btn_accion_realizar").text("Registrar");
        $("#btn_accion_realizar").attr("onclick", "RegistrarArticulo();");
        $(".input_clear_art").val('');
        $("#almacenable").prop("checked", false);
        $("#chk_disponible_bascula").prop("checked", false);
        $("#id_estatus").prop("checked", true);
        $("#id_estatus").attr("onclick", "Parametro(1);");
        $("#id_tipo").val(12);
        $("#id_tipo_requisicion").val(1);
    }

    $("#m_agregar_editar_articulo").modal("show");
    $(".input_valid").each(function () {
        $(this).css("border", "");
    });

}

function ConsultarExistenciaArticuloAlmacenTable() {
    $("#div_tabla_articulo_existencia").html('');
    var requi_tipo = $("#id_tipo_requisicion").val();
    if (requi_tipo == 1) {
        var clave = $("#codigo_articulo").text();
        if (clave.trim() != '') {
            $.ajax({
                type: "POST",
                async: false,
                url: "../ALMACEN/ConsultarExistenciaPorAlmacenArticulo",
                data: { clave: clave },
                success: function (response) {
                    try {
                        $("#tabla_almacen_articulo_existencia").dataTable().fnDestroy();
                    } catch (e) { }
                    $("#div_tabla_articulo_existencia").html(response);
                    $('#tabla_almacen_articulo_existencia').DataTable({
                        keys: false,
                        searching: false,
                        select: false,
                        ordering: false,
                        paging: false,
                        dom: "Bfrtip",
                        scrollCollapse: false,
                        buttons: [{}],
                        responsive: !0,
                        info: false
                    });
                    jsRemoveWindowLoad();
                },
                error: function (xhr, status, error) {
                    console.error(error);
                    jsRemoveWindowLoad();
                }
            });
        }
    }
}

function ConsultaArticuloTransitoTable() {
    var clave = $("#clave_buscar").val();
    var tipo = $("#id_tipo").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeput: 100000,
        url: "../COMPRAS/ConsultaArticuloTransitoTable",
        data: {
            clave: clave,
            tipo: tipo
        },
        success: function (response) {
            try {
                $("#tabla_articulos_transito").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_articulo_transito").html(response);
            $('#tabla_articulos_transito').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{
                    extend: "copy",
                    className: "btn-sm",
                    columns: ':visible'
                }, {
                    extend: "csv",
                    className: "btn-sm",
                    bom: true, // Añadir BOM para UTF-8
                    charset: 'utf-8', // Especificar la codificación
                    customize: function (csv) {
                        // Añadir el BOM manualmente y devolver el CSV modificado
                        return "\uFEFF" + csv;
                    }
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
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            iziToast.warning({
                title: '¡Aviso!',
                message: 'No se encontro la informacion del articulo',
            });
            jsRemoveWindowLoad();
        }
    });
}

function Parametro(accion) {
    //ACTIVO ARTICULO
    if (accion == 1) {
        $("#id_estatus").prop("checked", true);
    }
    //ACTIVO MARCA
    else if (accion == 2) {
        $("#id_estatus_marca").prop("checked", true);


    }
    //ACTIVO CLASIFICACION
    else if (accion == 3) {
        $("#id_estatus_clasificacion").prop("checked", true);
    }
    else {

    }
}

function Articulo_Servicio() {
    var valor = $("#almacenable").val();
    if (valor == 1) {
        $("#almacenable").prop("checked", false);
        $("#almacenable").prop("disabled", true);
    }
    else {
        $("#almacenable").prop("checked", false);
        $("#almacenable").prop("enabled", true);
    }
}

function mostrarfiltro(accion) {
    if (accion == 1) {
        $("#filtros_div").css("display", "block");
        acciones = 1;
    }
    else {
        $("#filtros_div").css("display", "none");
        acciones = 0;
    }
}

function ConsultarPrecioArticulo() {
    var id_articulo = $("#id_articulos").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarPrecioArticulo",
        data: {
            id_articulo: id_articulo
        },
        success: function (response) {
            $("#precio_articulo").val(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarInformacionArticulo() {
    var id_articulo = $("#id_articulo").val().split('|')[0];
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarInformacionArticulo",
        data: {
            id_articulo: id_articulo
        },
        success: function (response) {
            var data = $.parseJSON(response);
            $("#unidad_medida").text(data[0].unidad_medida);
            $("#id_unidad_medida").val(data[0].id_unidad_medida);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarInformacionArticuloID(modo) {
    var tipo = '';
    if (modo == 1) {
        tipo = $("#id_tipo_requisicion").val();
    }
    var id_articulo = $("#id_articulos").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarInformacionArticulo",
        data: {
            id_articulo: id_articulo,
            tipo: tipo
        },
        success: function (response) {
            if (response != "") {
                $("#id_articulos").css("border-color", "");
                var data = $.parseJSON(response);
                $("#unidad_medida").text(data[0].unidad_medida);



                /*$("#id_unidad_medida").val(data[0].id_unidad_medida);*/
                ConsultarDecimalArticulo(data[0].id_unidad_medida);


                $("#id_articulo").val(data[0].nombre_articulo);
                $("#codigo_articulo").text(data[0].id_articulo);
            }
            else {
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'No se encontro el articulo',
                });

                $("#unidad_medida").text('');
                $("#id_unidad_medida").val('');
                $("#id_articulo").val('');
                $("#precio").val('0.00');
                $("#codigo_articulo").text('');
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarInformacionArticuloNomb(modo) {
    var tipo = '';
    if (modo == 1) {
        tipo = $("#id_tipo_requisicion").val();
    }
    else {
        tipo = 2;
    }
    var id_articulo;
    if (modo == 1) { id_articulo = $("#id_articulo").val(); }
    else { id_articulo = $("#id_articulo_serv").val(); }
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarInformacionNomArticulo",
        data: {
            nombre_articulo: id_articulo,
            tipo: tipo
        },
        success: function (response) {
            $("#id_articulos").css("border-color", "");
            $("#id_articulos_serv").css("border-color", "");
            if (response.length > 3) {
                var data = $.parseJSON(response);
                if (modo == 1) {
                    $("#unidad_medida").text(data[0].unidad_medida);
                    ConsultarDecimalArticulo(data[0].id_unidad_medida);
                    $("#id_articulo").val(data[0].nombre_articulo);
                    $("#id_articulos").val(data[0].clave);
                    $("#codigo_articulo").text(data[0].id_articulo);
                }
                else {
                    $("#id_articulo_serv").val(data[0].nombre_articulo);
                    $("#id_articulos_serv").val(data[0].clave);
                    $("#codigo_articulo_serv").text(data[0].id_articulo);
                }
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}



function ConsultarDecimalArticulo(id_unidad_medida) {
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarDecimalArticulo",
        data: {
            id_unidad_medida: id_unidad_medida
        },
        success: function (response) {
            var data = $.parseJSON(response);
            var idfinal = id_unidad_medida + "|" + data[0].acepta_decimal;
            $("#id_unidad_medida").val(idfinal);
            //ConsultarPrecioArticulo(); CalcularImporteArticulo();
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

var idArticuloAsig = "";
var $elemSeleTipo = $('');
function ConsultarArticulos() {
    jsShowWindowLoad();
    var id_articulo_tipo = $("#id_articulo_tipo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarArticulo",
        data: {
            id_articulo_tipo: id_articulo_tipo
        },
        success: function (response) {

            $("#div_tabla_articulo").html(response);

            $('#tabla_articulos').bootstrapTable({
                columns: [{
                    field: 'id_articulo',
                    title: 'Clave'
                }, {
                    field: 'nombre_articulo',
                    title: 'Articulo'
                }]
            });
            $('#tabla_articulos').on('click-row.bs.table', function (e, row, $element) {
                $elemSeleTipo.css("background-color", "#ffffff");
                $element.css("background-color", "#99FFCC");
                $elemSeleTipo = $element;
                idArticuloAsig = row.id_articulo;
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

var acciones = 0;
function ConsultarArticuloCompletos(accion) {
    acciones = 1;
    jsShowWindowLoad();
    var id_clasif_filtro = $("#id_clasificaciones").val();
    var art_tipo_requisicion = $("#art_tipo_requisicion").val();
    var id_almacenables = $("#id_almacenables").val();
    var id_activo = $("#id_activos").val();
    var marcas_art = $("#art_marca").val();


    var almacen_p = [];
    var activo_p = [];
    if (id_almacenables == 0) {
        almacen_p = [false, true];
    }
    else { almacen_p = [id_almacenables] }
    if (id_activo == 0) {
        activo_p = [false, true];
    }
    else { activo_p = [id_activo] }
    if (acciones == 1) {
        $.ajax({
            type: "POST",
            async: true,
            timeput: 100000,
            url: "../ARTICULOS/ConsultarArticuloCompleto",
            data: {
                id_clasif_filtro: id_clasif_filtro,
                almacen: almacen_p,
                activo: activo_p,
                tipo: art_tipo_requisicion,
                marcas_art: marcas_art
            },
            success: function (response) {
                try {
                    $("#tabla_articulos").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_tabla_articulo").html(response);
                $('#tabla_articulos').DataTable({
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
}

function ConsultarArticuloCompletosFiltrado() {
    $("#div_tabla_articulo").html('');
    var clave = $("#clave_buscar").val();
    var articulo = $("#nombre_buscar").val();
    if (clave != "" || articulo != "") {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            timeput: 100000,
            url: "../ARTICULOS/ConsultarArticuloCompletosFiltrado",
            data: {
                clave: clave,
                articulo: articulo
            },
            success: function (response) {
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
}

var idArtRemov = "";
var $elemSeleAlma = $('');
function ConsultarArticuloAlmacen() {
    $("#div_tabla_articulo").html('');
    var id_almacen = $("#id_almacen").val();
    var id_tipo = $("#id_articulo_tipo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarArticuloAlmacen",
        data: {
            id_almacen: id_almacen,
            id_tipo: id_tipo
        },
        success: function (response) {
            $("#div_tabla_articulo_almacen").html(response);

            $('#tabla_articulos_alma').bootstrapTable({
                columns: [{
                    field: 'id_articulo',
                    title: 'Clave'
                }, {
                    field: 'nombre_articulo',
                    title: 'Articulo'
                }]
            });
            $('#tabla_articulos_alma').on('click-row.bs.table', function (e, row, $element) {
                $elemSeleAlma.css("background-color", "#ffffff");
                $element.css("background-color", "#F1A095");
                $elemSeleAlma = $element;
                idArtRemov = row.id_articulo;
            });
            idArtRemov = -1;
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function AsignarArticulosAlmacen() {
    var count = 0;
    var id_almacen = $("#id_almacen").val();
    var ubicacion = $("#id_ubicacion").val();
    var id_articulos_pendientes = [];
    var checks_pendientes = $("#tabla_articulos_pendientes input[type='checkbox']:checked");
    checks_pendientes.each(function (index) {
        id_articulos_pendientes[index] = $(this).attr('id');
        count = 1;
    });

    if (count > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../ARTICULOS/AsignarArticuloAlmacen",
            data: {
                id_articulos_pendientes: id_articulos_pendientes,
                id_almacen: id_almacen,
                ubicacion: ubicacion
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Ubicacion asignado correctamente!',
                    });
                    ConsultarArticulosUbicaionAlmacen();
                    ConsultarArticulosNoAlmacenable();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: '¡Ocurrió un problema al asignar el articulo!',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        iziToast.error({
            title: 'Aviso',
            message: '¡Favor de seleccionar un articulo!'
        })
    }
}
function RemoverArticulosAlmacen() {
    var count = 0;
    var id_almacen = $("#id_almacen").val();
    var id_articulos_ubicacion = [];
    var checks_ubicacion = $("#tabla_articulos_ubicacion input[type='checkbox']:checked");
    checks_ubicacion.each(function (index) {
        id_articulos_ubicacion[index] = $(this).attr('id');
        count = 1;
    });

    if (count > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../ARTICULOS/RemoverArticulosAlmacen",
            data: {
                id_articulos_ubicacion: id_articulos_ubicacion,
                id_almacen: id_almacen
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: '¡Ubicacion asignado correctamente!',
                    });
                    ConsultarArticulosUbicaionAlmacen();
                    ConsultarArticulosNoAlmacenable();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: '¡Ocurrió un problema al asignar el articulo!',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
    else {
        iziToast.error({
            title: 'Aviso',
            message: '¡Favor de seleccionar un articulo!'
        })
    }
}


function ConsultarArticulosNoAlmacenable() {
    jsShowWindowLoad();
    var id_articulo_tipo = $("#id_articulo_tipo").val();
    var id_almacen = $("#id_almacen").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarArticulosNoAlmacenableSistema",
        data: {
            id_almacen: id_almacen,
            id_articulo_tipo: id_articulo_tipo
        },
        success: function (response) {
            try {
                $("#tabla_articulos_pendientes").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_articulo_pendientes").html(response);
            $('#tabla_articulos_pendientes').DataTable({
                keys: false,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: false,
                searching: true
                //pagingType: 'simple_numbers',
                //pageLength: 10
            });
            jsRemoveWindowLoad();
            ConsultarArticulosUbicaionAlmacen();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
function ConsultarArticulosUbicaionAlmacen() {
    jsShowWindowLoad();
    var id_articulo_tipo = $("#id_articulo_tipo").val();
    var id_almacen = $("#id_almacen").val();
    var ubicacion = $("#id_ubicacion").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ConsultarArticulosUbicaionAlmacen",
        data: {
            id_almacen: id_almacen,
            id_articulo_tipo: id_articulo_tipo,
            ubicacion: ubicacion
        },
        success: function (response) {
            try {
                $("#tabla_articulos_ubicacion").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_articulo_almacen").html(response);
            $('#tabla_articulos_ubicacion').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
                pagingType: 'simple_numbers',
                pageLength: 10
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function LupaArticulo(modo) {
    var requi_tipo = $("#id_tipo_requisicion").val();
    if (requi_tipo < 0 || requi_tipo == undefined) {
        requi_tipo = 1;
    }
    $("#id_clasificaciones_art").val(-1);
    $("#id_clasificaciones_art").trigger("change");
    $("#m_lupa_articulos").modal("show");
}
function InformacionLupaArticulo(articulo, modo) {
    $("#m_lupa_articulos").modal("hide");
    $("#id_articulos").val(articulo);
    if (modo == 0) {
        //ESTE MODO NO HACE NADA SOLO SETEA LA CLAVE (articulo) a algún input en el front
        $("#clave_buscar").val(articulo);
    }
    else if (modo == 1) {
        ConsultarInformacionArticuloID(1);
        BuscarPrecioArticulo();
    }
    else if (modo == 3) {
        $("#id_articulo_nuevo_cambio").val(articulo);
    }
    else {
        ConsultarInformacionArticuloID();

        BuscarPrecioArticulo();
        CalcularImporteArticulo();
        ConsultarExistenciaArticuloAlmacen();
    }
}
function LupaArticuloParametro(modo) {
    var parametro = $("#id_clasificaciones_art").val();
    var requi_tipo = $("#id_tipo_requisicion").val();
    if (requi_tipo < 0 || requi_tipo == undefined) {
        requi_tipo = 1;
    }
    $("#m_lupa_articulos").modal("show");

    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/LupaArticuloParametro",
        data: {
            requi_tipo: requi_tipo,
            modo: modo,
            parametro: parametro
        },
        success: function (response) {
            try {
                $("#tabla_articulos_lupa").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_lupa_articulos").html(response);
            $('#tabla_articulos_lupa').DataTable({
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


        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function BuscarPrecioArticulo() {
    var id_articulo = parseFloat($("#codigo_articulo").text());
    if (id_articulo != '') {
        $.ajax({
            type: "POST",
            async: false,
            url: "../ARTICULOS/BuscarPrecioArticulo",
            data: {
                id_articulo: id_articulo
            },
            success: function (response) {
                $("#precio").val(response);
                $("#precio_articulo").val(response);
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
}

//#endregion


//#region CLASIFICACIONES
function ModalArticuloClasificacion(accion) {
    if (accion == 1) {
        $("#m_clasificacion_articulo").modal("show");
        $("#confirmar_clasificacion").attr("onclick", "AccionClasificacion(1)");
        MostrarTablaClasificacion();

        $("#id_estatus_clasificacion").attr("onclick", "Parametro(9);");
    }
    else if (accion == 2) {
        $("#agregar_clasificacion").css("display", "none");
        $("#div_tabla_clasificacion").css("display", "none");
        $("#div_agregar_clasificacion").css("display", "block");
        $("#confirmar_clasificacion").attr("onclick", "AccionClasificacion(1)");



        $("#id_estatus_clasificacion").prop("checked", true);
        $("#id_estatus_clasificacion").attr("onclick", "Parametro(3);");
    }
}

function MostrarTablaClasificacion() {
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/MostrarTablaClasificacion",
        data: {
        },
        success: function (response) {
            try {
                $("#tabla_clasificaciones").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_clasificacion").html(response);
            $('#tabla_clasificaciones').DataTable({
                keys: true,
                ordering: false,
                paging: true,
                dom: "Bfrtip",
                scrollCollapse: false,
                buttons: [{}],
                responsive: !0,
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

function MostrarInformacionClasificacion(id_clasificacion) {
    $("#id_estatus_clasificacion").attr("onclick", "Parametro(9);");
    //jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/MostrarInformacionClasificacion",
        data: {
            id_clasificacion: id_clasificacion
        },
        success: function (response) {
            $("#agregar_clasificacion").css("display", "none");
            $("#div_tabla_clasificacion").css("display", "none");
            $("#div_agregar_clasificacion").css("display", "block");


            var data = $.parseJSON(response);
            $("#nombre_clasificacion").val(data[0].nombre_clasificacion);
            if (data[0].activo === true) {
                $("#id_estatus_clasificacion").prop("checked", true);
            } else {
                $("#id_estatus_clasificacion").prop("checked", false);
            }
            $("#id_clasificacion").text(data[0].id_articulo_clasificacion);

            $("#confirmar_clasificacion").attr("onclick", "AccionClasificacion(2)");
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function AccionClasificacion(modo) {
    if (modo == 3) {
        $("#agregar_clasificacion").css("display", "block");
        $("#div_tabla_clasificacion").css("display", "block");
        $("#div_agregar_clasificacion").css("display", "none");
        LimpiarClasificacion();
        MostrarTablaClasificacion();
    }
    else {
        var clasificacion = $("#nombre_clasificacion").val();
        var estatus_clasificacion = false;
        if ($("#id_estatus_clasificacion").is(':checked')) {
            estatus_clasificacion = true;
        }
        else {
            estatus_clasificacion = false;
        }
        if (clasificacion.trim() != '' && clasificacion.length > 0) {
            //REGISTRAR CLASIFICACION
            if (modo == 1) {
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../CATALOGOS/RegistrarClasificacion",
                    data: {
                        clasificacion: clasificacion,
                        estatus_clasificacion: estatus_clasificacion
                    },
                    success: function (response) {
                        if (response == "True") {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se registro correctamente',
                            });
                            LimpiarClasificacion();
                        }
                        else {
                            iziToast.error({
                                title: 'Error',
                                message: 'Ocurrió un problema al realizar el registro',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
            }
            //MODIFICAR CLASIFICACION
            else {
                id_clasificacion = $("#id_clasificacion").text();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../CATALOGOS/ModificarClasificacion",
                    data: {
                        id_clasificacion: id_clasificacion,
                        clasificacion: clasificacion,
                        estatus_clasificacion: estatus_clasificacion
                    },
                    success: function (response) {
                        if (response == "True") {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se modifico correctamente',
                            });
                            $("#agregar_clasificacion").css("display", "block");
                            $("#div_tabla_clasificacion").css("display", "block");
                            $("#div_agregar_clasificacion").css("display", "none");
                            LimpiarClasificacion();
                            MostrarTablaClasificacion();
                        }
                        else {
                            iziToast.error({
                                title: 'Error',
                                message: 'Ocurrió un problema al realizar el cambio',
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
                title: 'Alerta',
                message: 'Es obligatorio ingresar la clasificacion',
            });
        }
    }
}

function LimpiarClasificacion() {
    $("#nombre_clasificacion").val('');
    $("#id_estatus_clasificacion").prop("checked", false);
    $("#id_clasificacion").text("0");
}

function MostrarClasificacionSelect() {
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/MostrarClasificacionSelect",
        data: {
        },
        success: function (response) {
            var response2 = response;
            response = '<option value="0">TODAS</option>' + response;
            $("#id_clasificacion").html(response2);
            $("#id_clasificaciones").html(response);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}
//#endregion

//#region MARCAS

function ModalArticuloMarca(accion) {

    if (accion == 1) {
        $("#m_marca_articulo").modal("show");
        $("#marca_confirmar").attr("onclick", "AccionMarca(1)");
        MostrarTablaMarcas();

        $("#id_estatus_marca").attr("onclick", "Parametro(9);");
    }
    else if (accion == 2) {
        $("#nueva_marca").css("display", "none");
        $("#div_tabla_marcas").css("display", "none");
        $("#div_agregar_marca").css("display", "block");
        $("#marca_confirmar").attr("onclick", "AccionMarca(1)");

        $("#id_estatus_marca").prop("checked", true);
        $("#id_estatus_marca").attr("onclick", "Parametro(2);");


    }
}
function MostrarTablaMarcas() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/MostrarTablaMarcas",
        data: {
        },
        success: function (response) {
            try {
                $("#tabla_marcas").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_tabla_marcas").html(response);
            $('#tabla_marcas').DataTable({
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

function MostrarInformacionMarca(id_marca) {
    $("#id_estatus_marca").attr("onclick", "Parametro(9);");
    //jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/MostrarInformacionMarca",
        data: {
            id_marca: id_marca
        },
        success: function (response) {
            $("#nueva_marca").css("display", "none");
            $("#div_tabla_marcas").css("display", "none");
            $("#div_agregar_marca").css("display", "block");


            var data = $.parseJSON(response);
            $("#nombre_marca").val(data[0].marca);
            if (data[0].activo === true) {
                $("#id_estatus_marca").prop("checked", true);
            } else {
                $("#id_estatus_marca").prop("checked", false);
            }
            $("#id_marca_articulo").text(data[0].id_marca);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function AccionMarca(modo) {
    if (modo == 3) {
        $("#nueva_marca").css("display", "block");
        $("#div_tabla_marcas").css("display", "block");
        $("#div_agregar_marca").css("display", "none");
        LimpiarMarcas(2);
        MostrarTablaMarcas();
    }
    else {
        var marca = $("#nombre_marca").val();
        var estatus_marca = false;
        if ($("#id_estatus_marca").is(':checked')) {
            estatus_marca = true;
        }
        else {
            estatus_marca = false;
        }
        if (marca.trim() != '' && marca.length > 0) {
            //REGISTRAR MARCA
            if (modo == 1) {
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../CATALOGOS/RegistrarMarca",
                    data: {
                        marca: marca,
                        estatus_marca: estatus_marca
                    },
                    success: function (response) {
                        if (response == "True") {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se registro correctamente',
                            });
                            LimpiarMarcas();
                        }
                        else {
                            iziToast.error({
                                title: 'Error',
                                message: 'Ocurrió un problema al realizar el registro',
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error(error);
                    }
                });
            }
            //MODIFICAR MARCA
            else {
                id_marca = $("#id_marca_articulo").text();
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../CATALOGOS/ModificarMarca",
                    data: {
                        id_marca: id_marca,
                        marca: marca,
                        estatus_marca: estatus_marca
                    },
                    success: function (response) {
                        if (response == "True") {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Se modifico correctamente',
                            });
                            $("#nueva_marca").css("display", "block");
                            $("#div_tabla_marcas").css("display", "block");
                            $("#div_agregar_marca").css("display", "none");
                            LimpiarMarcas();
                            MostrarTablaMarcas();
                        }
                        else {
                            iziToast.error({
                                title: 'Error',
                                message: 'Ocurrió un problema al realizar el cambio',
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
                title: 'Alerta',
                message: 'Es obligatorio ingresar la marca',
            });
        }
    }
}

function LimpiarMarcas(modo) {
    $("#nombre_marca").val('');
    $("#id_marca_articulo").text("0");
    if (modo == 2) {
        $("#id_estatus_marca").prop("checked", false);
    }
}

function MostrarMarcasSelect() {
    $.ajax({
        type: "POST",
        async: true,
        url: "../CATALOGOS/MostrarMarcasSelect",
        data: {
        },
        success: function (response) {
            var response2 = response;
            response = '<option value="0">TODAS</option>' + response;
            $("#id_marca_articulo").html(response2);
            $("#art_marca").html(response);
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


function ConsultarConsecutivoArticulo() {
    $.ajax({
        type: "POST",
        async: true,
        timeout: 90000,
        url: "../ARTICULOS/ConsultarConsecutivoArticulo",
        data: {
        },
        success: function (response) {
            $("#codigo_articulo").val(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

//#endregion



//#region ARTICULOS BASCULA

function CheckAllArticulosBascula() {
    if ($("#check_all_articulos_bascula").is(':checked')) {
        $(".articulo_bascula_check").prop("checked", true);
    }
    else {
        $(".articulo_bascula_check").prop("checked", false);
    }
}

function PreAsociacionArticuloBascula() {
    var id_articulos = [];
    var clave_articulos = [];
    var nombre_articulos = [];
    $(".articulo_bascula_check").each(function () {
        if ($(this).is(':checked')) {
            var id_articulo = $(this).attr('id').split('_').pop();
            var clave_articulo = $(this).closest('td').next('td').text();
            var nombre_articulo = $(this).closest('td').next('td').next('td').text();
            clave_articulos.push(clave_articulo);
            id_articulos.push(id_articulo);
            nombre_articulos.push(nombre_articulo);
        }
    });

    if (id_articulos.length > 0) {

        $("#m_articulo_asociacion").modal('show');
        //try {
        //    $("#tabla_pre_asociacion").dataTable().fnDestroy();
        //} catch (e) { }
        $('#tabla_pre_asociacion tbody').empty();
        for (var i = 0; i < id_articulos.length; i++) {
            var nuevaFila = '<tr>' +
                '<td>' + clave_articulos[i] + '</td>' +
                '<td id="articulo_bascula_' + id_articulos[i] + '" class ="articulos_asociacion_confirmados">' + nombre_articulos[i] + '</td>' +
                '<td><button class="btn btn_beta_danger" onclick="EliminarPreAsociacion(this)"><i class="fa fa-remove"></i></button></td>' +
                '</tr>';
            $('#tabla_pre_asociacion tbody').append(nuevaFila);
        }

        //$('#tabla_pre_asociacion').DataTable({
        //    ordering: false,
        //    paging: false,
        //    dom: "Bfrtip",
        //    buttons: [{}],
        //    responsive: false,
        //    scrollY: false,
        //    select: false,
        //    keys: false,
        //    scrollX: true,
        //    searching: false,
        //    info: false
        //});

        TipoArticuloBasculaDiv();
    }
    else {
        iziToast.warning({
            title: 'Aviso.',
            message: 'Favor de seleccionar 1 articulo',
        });
    }
}

function EliminarPreAsociacion(preAsociacion) {
    $(preAsociacion).closest('tr').remove();
}

function PreAsociacionArticuloBasculaCerrar() {
    $("#m_articulo_asociacion").modal('hide');
}

function TipoArticuloBasculaDiv() {
    $("#div_tipos_articulo_almacen").html('');
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/TipoArticuloBasculaDiv",
        data: {
        },
        success: function (response) {
            $("#div_tipos_articulo_bascula").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}

function ArticulosBasculaTable() {
    $("#div_articulo_bascula").html('');
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../ARTICULOS/ArticulosBasculaTable",
        data: {},
        success: function (response) {
            $("#div_articulo_bascula").html(response);
            try {
                $("#tabla_articulos_bascula").dataTable().fnDestroy();
            } catch (e) { }
            var table = $('#tabla_articulos_bascula').DataTable({
                keys: false,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                searching: true,
                fixedHeader: true
            });

            $('#tabla_articulos_bascula').DataTable().columns.adjust().draw();
            $(window).resize(function () {
                table.columns.adjust().draw();
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
    jsRemoveWindowLoad();
}


function AsociacionArticuloBascula() {
    var tipos_ficha = [];
    $(".tipo_ficha_articulo:checked").each(function () {
        var tipo_ficha = $(this).attr('id').split('_');
        tipos_ficha.push(tipo_ficha[3]);
    });
    var id_articulos = [];
    $(".articulos_asociacion_confirmados").each(function () {
        var id_articulo = $(this).attr('id').split('_');
        id_articulos.push(id_articulo[2]);
    });

    if (tipos_ficha.length == 0) { tipos_ficha.push(0); }

    if (id_articulos.length > 0) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../ARTICULOS/AsociacionArticuloBascula",
            data: {
                tipos_ficha: tipos_ficha,
                id_articulos: id_articulos
            },
            success: function (response) {
                if (response == "True" || response == true) {
                    $("#m_articulo_asociacion").modal('hide');
                    ArticulosBasculaTable();
                }
                else {
                    iziToast.warning({
                        title: 'Aviso.',
                        message: 'Ocurrio un problema al asociar el articulo con el tipo',
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
            title: 'Aviso.',
            message: 'Favor de seleccionar 1 articulo',
        });
    }
}
//#endregion


//#region HISTORIAL DE COTIZACIONES

function ConsultaHistorialCotizacionTable() {
    var clave = $("#clave_buscar").val();
    jsShowWindowLoad();
    if (clave.trim() != '' && clave.length > 0) {
        var fecha_inicio = $("#fechas_inicio_historial").val();
        var fecha_fin = $("#fechas_fin_historial").val();

        if (fecha_inicio == undefined) { fecha_inicio = ""; }
        if (fecha_fin == undefined) { fecha_fin = ""; }

        $.ajax({
            type: "POST",
            async: true,
            timeput: 100000,
            url: "../COMPRAS/ConsultaHistorialCotizacionTable",
            data: {
                clave: clave,
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin
            },
            success: function (response) {
                try {
                    $("#tabla_articulos_transito").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_tabla_articulo_transito").html(response);
                $('#tabla_articulos_transito').DataTable({
                    keys: false,
                    ordering: true,
                    paging: true,
                    dom: "Bfrtip",
                    buttons: [{
                        extend: "copy",
                        className: "btn-sm",
                        columns: ':visible'
                    }, {
                        extend: "csv",
                        className: "btn-sm",
                        bom: true, // Añadir BOM para UTF-8
                        charset: 'utf-8', // Especificar la codificación
                        customize: function (csv) {
                            // Añadir el BOM manualmente y devolver el CSV modificado
                            return "\uFEFF" + csv;
                        }
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
                jsRemoveWindowLoad();
            },
            error: function (xhr, status, error) {
                console.error(error);
                iziToast.warning({
                    title: '¡Aviso!',
                    message: 'No se encontró la información del articulo o no tiene historial de compras',
                });
                jsRemoveWindowLoad();
            }
        });
    }
    else {
        jsRemoveWindowLoad();
        iziToast.warning({
            title: '¡Aviso!',
            message: 'Favor de ingresar la clave del articulo',
        });
    }
}

function ConsultarHistorialCotizacionesGeneralTable() {
    var clave = $("#clave_buscar").val();
    jsShowWindowLoad();
    if (clave.trim() != '' && clave.length > 0) {
        $.ajax({
            type: "POST",
            async: true,
            timeput: 100000,
            url: "../COMPRAS/ConsultarHistorialCotizacionesGeneralTable",
            data: {
                clave: clave
            },
            success: function (response) {
                try {
                    $("#datatable_cotizaciones_general").dataTable().fnDestroy();
                } catch (e) { }
                $("#div_tabla_cotizaciones_general").html(response);
                $('#datatable_cotizaciones_general').DataTable({
                    keys: false,
                    ordering: true,
                    searching: false,
                    paging: true,
                    dom: "Bfrtip",
                    buttons: [{
                        extend: "copy",
                        className: "btn-sm",
                        columns: ':visible'
                    }, {
                        extend: "csv",
                        className: "btn-sm",
                        bom: true, // Añadir BOM para UTF-8
                        charset: 'utf-8', // Especificar la codificación
                        customize: function (csv) {
                            // Añadir el BOM manualmente y devolver el CSV modificado
                            return "\uFEFF" + csv;
                        }
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
                jsRemoveWindowLoad();
            },
            error: function (xhr, status, error) {
                console.error(error);
                //iziToast.warning({
                //    title: '¡Aviso!',
                //    message: 'No se encontró la información del articulo o no tiene historial de compras',
                //});
                jsRemoveWindowLoad();
            }
        });
    }
    else {
        jsRemoveWindowLoad();
        //iziToast.warning({
        //    title: '¡Aviso!',
        //    message: 'Favor de ingresar la clave del articulo',
        //});
    }
}

function EditarVigenciaCotizacion(id_compra_articulo_cotizacion) {
    $("#id_cotizacion_editar").val(id_compra_articulo_cotizacion);
    $("#lbl_id_cotizacion").text("Cotización #" + id_compra_articulo_cotizacion + "");
    $("#m_editar_vigencia").modal("show");
}

function ActualizarVigenciaCotizacion() {
    var id_compra_articulo_cotizacion = $("#id_cotizacion_editar").val();
    var fecha_vigencia = $("#fecha_vigencia_edit").val();
    if (fecha_vigencia == "" || fecha_vigencia == undefined) {
        iziToast.warning({
            title: 'Selecciona una fecha de vigencia',
            message: '',
        });
    }
    else if (id_compra_articulo_cotizacion == "" || id_compra_articulo_cotizacion == undefined) {
        iziToast.warning({
            title: 'Cotización no encontrada, cierre el recuadro e intente nuevamente',
            message: '',
        });
    }
    else {
        jsShowWindowLoad();
        $.ajax({
            type: "POST",
            async: true,
            url: "../COMPRAS/ActualizarVigenciaCotizacion",
            data: {
                id_compra_articulo_cotizacion: id_compra_articulo_cotizacion,
                fecha_vigencia: fecha_vigencia
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response == 0) {
                    $("#m_editar_vigencia").modal('hide');
                    ConsultarHistorialCotizacionesGeneralTable();
                    ConsultaHistorialCotizacionTable();
                    iziToast.success({
                        title: 'Vigencia actualizada correctamente',
                        message: '',
                    });
                }
                else if (response == -2) {
                    iziToast.warning({
                        title: 'Seleccione una fecha mayor al día de hoy',
                        message: '',
                    });
                }
                else {
                    iziToast.warning({
                        title: 'Aviso.',
                        message: 'Ocurrio un problema al asociar el articulo con el tipo',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
}

function RegistrarCotizacionProveedor() {
    var id_articulo = $("#id_articulo_add_coti").text();
    var fecha_vigencia = $("#fecha_vigencia").val();
    if (fecha_vigencia == '' || fecha_vigencia == "") {
        iziToast.warning({
            title: 'Ingresa una fecha de vigencia',
            message: '',
        });
    }
    else if (id_articulo == '' || id_articulo == "") {
        iziToast.warning({
            title: 'Articulo no detectado, cierre este recuadro y vuelvalo a intentar',
            message: '',
        });
    }
    else {
        var descuentos = $("#descuento").val();
        if (descuentos <= 0 || descuentos == "") {
            descuentos = 0;
        }

        var id_compras_proveedor = $("#proveedor").val();
        var precio_unitario = $("#precio_unitario").val();
        var porcentaje_descuento = descuentos;

        var precio_unit = $("#precio_unitario_iva").val();
        var precio = parseFloat(precio_unit.replace('$', '').replace(/,/g, ''));

        var dias_entrega = $("#tiempo_entrega").val();
        if (dias_entrega <= 0 || dias_entrega == "") {
            dias_entrega = 0;
        }

        var precio_final = precio;
        var id_unidad_medida = $("#id_unidad_medida_add_coti").text();
        var marca = $("#marca").find(":selected").text();
        var presentacion = $("#presentacion").val();
        var id_tipo_moneda = $("#tipo_moneda").val();
        ConsultarInformacionProveedorNomb(1);
        ValidarTotal();

        if (validacion) {
            iziToast.question({
                timeout: 20000,
                close: false,
                overlay: true,
                displayMode: 'once',
                id: 'question',
                zindex: 999999,
                title: 'ATENCIÓN',
                message: '¿Está seguro de registrar esta cotización?',
                position: 'center',
                buttons: [
                    ['<button><b>Si, registrar</b></button>', function (instance, toast) {
                        instance.hide({ transitionOut: 'fadeOut' }, toast, 'button');
                        jsShowWindowLoad();
                        $.ajax({
                            type: "POST",
                            async: false,
                            url: "../COMPRAS/RegistrarCotizacionProveedor",
                            data: {
                                id_articulo: id_articulo,
                                id_compras_proveedor: id_compras_proveedor,
                                precio_unitario: precio_unitario,
                                porcentaje_descuento: porcentaje_descuento,
                                precio_final: precio_final,
                                id_unidad_medida: id_unidad_medida,
                                marca: marca,
                                presentacion: presentacion,
                                fecha_vigencia: fecha_vigencia,
                                dias_entrega: dias_entrega,
                                id_tipo_moneda: id_tipo_moneda
                            },
                            success: function (result) {
                                jsRemoveWindowLoad();
                                if (result == 0) {
                                    $("#m_add_cotizacion").modal("hide");
                                    iziToast.success({
                                        title: 'Exito',
                                        message: '¡Cotizacion registrada!',
                                    });
                                    ConsultarHistorialCotizacionesGeneralTable();
                                    ConsultaHistorialCotizacionTable();
                                }
                                else {
                                    iziToast.error({
                                        title: 'Ocurrió un error al registrar la cotización',
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
        else {
            iziToast.warning({
                title: 'Aviso',
                message: 'Campos obligatorios',
            });
        }
    }
}

//#endregion


//#region CRITICIDAD
function ConsultarArticulosCriticidadAlmacen() {
    var id_almacen = $("#id_almacen").val();
    var id_articulo_tipo = $("#id_articulo_tipo").val();
    var tipo_consulta = $('input[name="tipo_vista"]:checked').val();

    if (id_articulo_tipo == undefined || id_articulo_tipo == null || id_articulo_tipo == "" ||
        id_almacen == undefined || id_almacen == null || id_almacen == "" ||
        tipo_consulta == undefined || tipo_consulta == null || tipo_consulta == "") {
        iziToast.warning({
            title: 'Ingrese todos los parámetros',
            message: '',
        });
        return;
    }
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: true,
        timeout: 900000,
        url: "../ALMACEN/ConsultarArticulosCriticidadAlmacen",
        data: {
            id_almacen: id_almacen,
            id_articulo_tipo: id_articulo_tipo
        },
        success: function (response) {
            $("#div_criticos").html(response);
            try {
                $("#datatabletabla_criticos").dataTable().fnDestroy();
            } catch (e) { }
            $('#datatabletabla_criticos').DataTable({
                keys: false,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: false,
                scrollY: false,
                scrollX: true
            });
            jsRemoveWindowLoad();
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}


//#endregion


