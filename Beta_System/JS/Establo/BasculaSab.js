$('input[type="date"]').val(today);



//#region BASCULA_SAB
function fakebascula() {
    peso_real = "100kg";
    peso_real1 = "100kg";
    peso_real2 = "100kg";
    pesoT = 1000;
    speso_real = 100;
    peso_bascula = 10;

    $("#peso_bascula").val(peso_real);
    $("#peso_bascula_2").val(100);
    CalcularPesoTotal();
}


//function MostrarTodoReimp() {
//    jsShowWindowLoad();
//    $("#div_pdf_primera").css("display", "block");
//    var ficha = $("#id_ficha_g_seg").val();
//    var establo = $("#id_establo_seg").val();
//    var folio = $("#folio_seg").val();
//    var placas = $("#placas_seg").val();
//    var chofer = $("#conductor_seg").val();
//    var producto = $("#articulo_seg").val();
//    var pesada1 = parseFloat($("#peso_bascula_seg").val());
//    var pesada2 = parseFloat(speso_real);
//    var pesadat = parseFloat(pesoT);
//    var observaciones = $("#obs_seg").val();

//    $.ajax({
//        type: "POST",
//        async: false,
//        url: "../BASCULASAB/GenerarFichaPDF",
//        data: {
//            ficha: ficha,
//            establo: establo,
//            folio: folio,
//            placas: placas,
//            chofer: chofer,
//            producto: producto,
//            pesada1: pesada1,
//            pesada2: pesada2,
//            pesadat: pesadat,
//            observaciones: observaciones
//        },
//        success: function (response) {
//            jsRemoveWindowLoad();
//            $("#div_pdf_directo").html(response);
//        },
//        error: function (xhr, status, error) {
//            console.error(error);
//            jsRemoveWindowLoad();
//        }
//    });
//}
function MostrarTodoReimp() {
    jsShowWindowLoad();
    var ficha = $("#id_ficha_g_seg").val();
    var establo = $("#id_establo_seg").val();
    var folio = $("#folio_seg").val();
    var placas = $("#placas_seg").val();
    var chofer = $("#conductor_seg").val();
    var producto = $("#articulo_seg").val();
    var pesada1 = parseFloat($("#peso_bascula_seg").val());
    var pesada2 = parseFloat(speso_real);
    var pesadat = parseFloat(pesoT);
    var observaciones = $("#obs_seg").val();
    var cliente = $("#proveedor_seg").val();

    var newTab = window.open('', '_blank');

    $.ajax({
        type: "POST",
        url: "../BASCULASAB/GenerarFichaPDF",
        data: {
            ficha: ficha,
            establo: establo,
            folio: folio,
            placas: placas,
            chofer: chofer,
            producto: producto,
            pesada1: pesada1,
            pesada2: pesada2,
            pesadat: pesadat,
            observaciones: observaciones,
            cliente: cliente
        },
        success: function (response) {
            jsRemoveWindowLoad();

            // Set the content of the new tab with the partial view HTML
            newTab.document.open();
            newTab.document.write(response);
            newTab.document.close();
            newTab.onload = function () {
                newTab.print();
                newTab.close();
            };
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ImpresionSAB() {
    jsShowWindowLoad();

    // Assuming your partial view content is already loaded in the div_pdf_directo element
    var contentToPrint = $("#div_pdf_directo").html();

    var printWindow = window.open('', '_blank');
    printWindow.document.open();
    printWindow.document.write('<html><head><title>Print</title></head><body>');
    printWindow.document.write(contentToPrint);
    printWindow.document.write('</body></html>');
    printWindow.document.close();

    // Ensure the content is fully loaded before printing
    $(printWindow).on('load', function () {
        printWindow.print();
        printWindow.onafterprint = function () {
            jsRemoveWindowLoad();
            printWindow.close();
        };
    });
}






function FichaPendiente() {
    $("#m_pendientes").modal("show");
    var id_establo = $("#id_establo_seg").val();
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULASAB/FichaPendiente",
        data: {
            id_establo: id_establo
        },
        success: function (response) {
            $("#div_ficha_pendiente").html(response);
            $('#table_fichas_pendientes').DataTable({
                ordering: false,
                paging: true,
                searching: true,
                dom: "Bfrtip",
                buttons: [{}],
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
function ConsultarInfoFichaSabDirecto(ficha) {
    jsShowWindowLoad();
    $("#m_pendientes").modal("hide");
    $("#id_ficha_g_seg").val(ficha);
    var id_establo = $("#id_establo_seg").val();
    if (ficha != "") {
        $.ajax({
            type: "POST",
            async: false,
            url: "../BASCULASAB/ConsultarInfoFichaSab",
            data: {
                fichas: ficha,
                id_establo: id_establo
            },
            success: function (response) {
                if (response != "No se encontro informacion de la ficha") {
                    var data = $.parseJSON(response);

                    $("#no_pozo_seg").val(data.Pozo);
                    $("#proveedor_seg").val(data.ProvCli);
                    $("#articulo_seg").val(data.Producto);
                    if (data.TMov == "E") {
                        $("#tmov_seg").val("Entrada");
                    } else if (data.TMov == "S") {
                        $("#tmov_seg").val("Salida");
                    } else {
                        $("#tmov_seg").val("Inventario");
                    }



                    if (data.CveMov == "001") {
                        $("#cvemov_seg").val('COMPRA');
                    }
                    if (data.CveMov == "002") {
                        $("#cvemov_seg").val('TRANSFERENCIA ENTRE ESTABLOS');
                    }
                    if (data.CveMov == "003") {
                        $("#cvemov_seg").val('TRANSFERENCIA FORRAJE/AGRICOLA');
                    }
                    if (data.CveMov == "004") {
                        $("#cvemov_seg").val('VENTA (SALIDA DE LECHE)');
                    }
                    if (data.CveMov == "005") {
                        $("#cvemov_seg").val('VENTA (SALIDA DE GANADO)');
                    }
                    if (data.CveMov == "006") {
                        $("#cvemov_seg").val('SALIDAS DESECHO');
                    }
                    if (data.CveMov == "007") {
                        $("#cvemov_seg").val('INVENTARIO');
                    }



                    $("#folio_seg").val(data.Folio);
                    $("#sucursal_seg").val(data.Sucursal);
                    $("#destino_seg").val(data.Destino);
                    $("#linea_seg").val(data.PropietarioCamion);
                    $("#conductor_seg").val(data.Chofer);
                    $("#placas_seg").val(data.Placas);
                    $("#maquilador_seg").val(data.Maquilador);
                    $("#predio_cliente_seg").val(data.PredioCliente);
                    $("#obs_seg").val(data.ObsP1);
                    $("#peso_origen_seg").val(data.PesoOri);
                    $("#mat_seca_seg").val(data.PMatSec);
                    $("#peso_bascula_seg").val(data.Peso1);
                    peso_real = data.Peso1;

                    var agrupada = data.agrupada;
                    if (agrupada == true) { $("#agrupada").prop('checked', true) }
                    else { $("#agrupada").prop('checked', false) }
                    fichaencontrada = 1;
                    jsRemoveWindowLoad();
                }
                else {
                    iziToast.warning({
                        title: '',
                        message: 'No se encontró la ficha o ya se registro la segunda pesada',
                    });
                    $(".input_clear").val('');
                    fichaencontrada = 0;
                    jsRemoveWindowLoad();
                }

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
            message: 'Favor de ingresar una ficha',
        });
        $("#id_ficha_g_seg").focus();
        jsRemoveWindowLoad();
    }
}
function MostrarReimp() {
    $("#m_reimpresion").modal("show");
}
function ReimprecionFicha() {
    var ficha = $("#reimp_ficha").val();
    var id_establo = $("#id_establo_re").val();
    jsShowWindowLoad();
    if (ficha != "" || ficha != null) {
        $.ajax({
            type: "POST",
            async: false,
            url: "../BASCULASAB/ReimprecionFichaPDF",
            data: {
                fichas: ficha,
                id_establo: id_establo
            },
            success: function (response) {
                if (response != "") {
                    var pdf = new jsPDF('landscape', 'pt', 'letter');

                    // Convert the HTML content to PDF using html2pdf
                    html2pdf(response, {
                        margin: 10,
                        filename: 'Ficha_Bascula.pdf',
                        image: { type: 'jpeg', quality: 0.98 },
                        html2canvas: { scale: 2 },
                        jsPDF: { unit: 'pt', format: 'letter', orientation: 'landscape' }
                    }).then(function () {
                        // Hide the loading window or perform any other necessary actions
                        jsRemoveWindowLoad();
                    });
                }
                else {
                    iziToast.warning({
                        title: 'Aviso',
                        message: 'No se logro generar el pdf',
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
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar la ficha',
        });
        jsRemoveWindowLoad();
    }
}


function GenerarFichaPDF(ficha) {
    jsShowWindowLoad();
    $("#div_pdf_primera").css("display", "block");
    var establo = $("#id_establo_seg").val();
    var folio = $("#folio_seg").val();
    var placas = $("#placas_seg").val();
    var chofer = $("#conductor_seg").val();
    var producto = $("#articulo_seg").val();
    var pesada1 = parseFloat($("#peso_bascula_seg").val());
    var pesada2 = parseFloat(speso_real);
    var pesadat = parseFloat(pesoT);
    var observaciones = $("#obs_seg").val();

    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULASAB/GenerarFichaPDF",
        data: {
            ficha: ficha,
            establo: establo,
            folio: folio,
            placas: placas,
            chofer: chofer,
            producto: producto,
            pesada1: pesada1,
            pesada2: pesada2,
            pesadat: pesadat,
            observaciones: observaciones
        },
        success: function (response) {
            var pdf = new jsPDF('landscape', 'pt', 'letter');

            // Convert the HTML content to PDF using html2pdf
            html2pdf(response, {
                margin: 10,
                filename: 'Ficha_Bascula.pdf',
                image: { type: 'jpeg', quality: 0.98 },
                html2canvas: { scale: 2 },
                jsPDF: { unit: 'pt', format: 'letter', orientation: 'landscape' }
            }).then(function () {
                // Hide the loading window or perform any other necessary actions
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ConsultarPozosEstabloSab() {
    var establo = $("#id_establo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULASAB/ObtenerPozos",
        data: { establo: establo },
        success: function (response) {
            $("#no_pozo").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarProveedorEstabloSab() {
    var proveedor = $("#id_establo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULASAB/ObtenerProveedor",
        data: { proveedor: proveedor },
        success: function (response) {
            $("#select_proveedor").append(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarArticuloSab() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULASAB/ObtenerArticulo",
        data: {},
        success: function (response) {
            $("#select_articulo").append(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarFichaSab() {
    var establo = $("#id_establo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULASAB/ObtenerFichaSab",
        data: { establo: establo },
        success: function (response) {
            $("#id_ficha_g").val(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function GuardarFichaSab() {
    jsShowWindowLoad();
    var peso_bascula = peso_real1;
    if (peso_bascula != 0) {
        ConsultarFichaSab();
        var id_ficha_g = $("#id_ficha_g").val();
        var id_establo = $("#id_establo").val();
        var no_pozo = $("#no_pozo").val();
        var select_proveedor = $("#select_proveedor").val();
        var select_articulo = $("#select_articulo").val();
        var cvemov = $("#cvemov option:selected").text().split('-')[0].trim();
        var tmov = $("#tmov").val();
        tmov = (tmov == 1) ? "E" : (tmov == 2) ? "S" : "I";
        var folio = $("#folio").val();
        var sucursal = $("#sucursal").val();
        var destino = $("#destino").val();
        var linea_trasp = $("#linea_trasp").val();
        var chofer = $("#conductor").val();
        var placas = $("#placas").val();
        var maquilador = $("#maquilador").val();
        var predio_cliente = $("#predio_cliente").val();
        var obs = $("#obs").val();
        var peso_origen = $("#peso_origen").val();
        var mat_seca = $("#mat_seca").val();
        var tabla = $("#tabla_p1").val().trim();
        var variedad = $("#variedad_p1").val();
        var corte = $("#corte_p1").val();
        var pacas = $("#pacas_p1").val();
        var ensilador = $("#ensilador_p1").val();
        //SI ES ENVIO DE LECHE
        if (cvemov == "004") {
            if (linea_trasp == "NO APLICA") {
                iziToast.warning({
                    title: 'Error',
                    message: 'Los envios de leche deben tener linea transportista',
                });
                jsRemoveWindowLoad();
                return;
            }
        }
        if (no_pozo != null && cvemov != "00null" && tmov != "" && chofer != "" && placas != "" && tabla != undefined && tabla != "" && tabla != " ") {
            folio = folio.trim() === "" ? " " : folio;
            sucursal = sucursal.trim() === "" ? " " : sucursal;
            destino = destino.trim() === "" ? " " : destino;
            linea_trasp = linea_trasp.trim() === "" ? " " : linea_trasp;
            chofer = chofer.trim() === "" ? " " : chofer;
            placas = placas.trim() === "" ? " " : placas;
            maquilador = maquilador.trim() === "" ? " " : maquilador;
            predio_cliente = predio_cliente.trim() === "" ? " " : predio_cliente;
            obs = obs.trim() === "" ? " " : obs;
            peso_origen = peso_origen.trim() === "" ? "0" : peso_origen;
            mat_seca = mat_seca.trim() === "" ? "0" : mat_seca;
            variedad = variedad.trim() === "" ? " " : variedad;
            corte = corte.trim() === "" ? "0" : corte;
            pacas = pacas.trim() === "" ? "0" : pacas;
            ensilador = ensilador.trim() === "" ? " " : ensilador;
            //tabla = tabla.trim() === "" ? " " : tabla;
            var agrupada = false;
            if ($("#agrupada").is(':checked')) { agrupada = true; }
            if (maquilador == null || maquilador == "") {
                maquilador = " ";
            }
            if (predio_cliente == null || predio_cliente == "") {
                predio_cliente = " ";
            }
            if (peso_origen == null || peso_origen == "") {
                peso_origen = "0";
            }
            if (mat_seca == null || mat_seca == "") {
                mat_seca = "0";
            }
            if (sucursal == null || sucursal == "") {
                sucursal = " ";
            }
            if (destino == null || destino == "") {
                destino = " ";
            }
            //if (tabla == undefined || tabla == "" || tabla == " ") {
            //    tabla = " ";
            //}
            if (variedad == null || variedad == "") {
                variedad = " ";
            }
            if (corte == null || corte == "") {
                corte = "0";
            }
            if (pacas == null || pacas == "") {
                pacas = "0";
            }
            if (ensilador == null || ensilador == "") {
                ensilador = " ";
            }
            var primera_pesada = [id_establo, id_ficha_g, no_pozo, select_proveedor, select_articulo, cvemov, tmov, folio, sucursal, destino, linea_trasp, chofer, placas, maquilador, predio_cliente, obs, peso_origen, mat_seca, agrupada, peso_bascula, tabla, variedad, ensilador, corte, pacas];
            if (select_proveedor != 0 && select_articulo != 0) {
                $.ajax({
                    type: "POST",
                    async: false,
                    url: "../BASCULASAB/GuardarFichaSab",
                    data: {
                        primera_pesada: primera_pesada
                    },
                    success: function (response) {
                        if (response == "True") {
                            iziToast.success({
                                title: 'Exito',
                                message: 'Ficha registrada correctamente!',
                            });
                            ConsultarFichaSab();
                            $("#peso_bascula").val("0");
                            peso_real1 = 0;
                        }
                        else {
                            iziToast.error({
                                title: 'Error',
                                message: 'Ocurrió un problema registrar la ficha',
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
                iziToast.warning({
                    title: 'Aviso',
                    message: 'Es necesario seleccionar un articulo y proveedor',
                });
                jsRemoveWindowLoad();
            }
        }
        else {
            iziToast.warning({
                title: 'Aviso',
                message: 'Es necesario rellenar todos los campos',
            });
            $(".borders").css("border-color", "red");
            jsRemoveWindowLoad();
        }
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario el peso para registrar la ficha',
        });
        jsRemoveWindowLoad();
    }
}


function LimpiarCampos() {
    $("#agrupada").prop('checked', false)
    $(".input_clear").val('');
    $(".borders").css("border-color", "");
    peso_real1 = 0;
    $("#tmov").val(1);
    $("#cvemov").val(1);
    $("#linea_trasp").prop("selectedIndex", 0);
    $("#peso_bascula").val("0");

    $("#select_proveedor").prop("selectedIndex", 0);
    $("#select_proveedor").trigger("change");

    $("#select_articulo").prop("selectedIndex", 0);
    $("#select_articulo").trigger("change");

    var pozo = $("#no_pozo option:first").val();
    $("#no_pozo").prop("selectedIndex", pozo);
    $("#no_pozo").trigger("change");
}


var artisel = 0;
var provsel = 0;
function selectProveedor() {
    artisel = 1;
}
function selectArticulo() {
    provsel = 1;
}

var fichaencontrada = 0;
function ConsultarInfoFichaSab() {
    jsShowWindowLoad();
    var ficha = $("#id_ficha_g_seg").val();
    var id_establo = $("#id_establo_seg").val();
    if (ficha != "") {
        $.ajax({
            type: "POST",
            async: false,
            url: "../BASCULASAB/ConsultarInfoFichaSab",
            data: {
                fichas: ficha,
                id_establo: id_establo
            },
            success: function (response) {
                if (response != "No se encontro informacion de la ficha") {
                    var data = $.parseJSON(response);

                    $("#no_pozo_seg").val(data.Pozo);
                    $("#proveedor_seg").val(data.ProvCli);
                    $("#articulo_seg").val(data.Producto);
                    if (data.TMov == "E") {
                        $("#tmov_seg").val("Entrada");
                    } else if (data.TMov == "S") {
                        $("#tmov_seg").val("Salida");
                    } else {
                        $("#tmov_seg").val("Inventario");
                    }



                    if (data.CveMov == "001") {
                        $("#cvemov_seg").val('001-COMPRA');
                    }
                    if (data.CveMov == "002") {
                        $("#cvemov_seg").val('002-TRANSFERENCIA ENTRE ESTABLOS');
                    }
                    if (data.CveMov == "003") {
                        $("#cvemov_seg").val('003-TRANSFERENCIA FORRAJE/AGRICOLA');
                    }
                    if (data.CveMov == "004") {
                        $("#cvemov_seg").val('004-VENTA (SALIDA DE LECHE)');
                    }
                    if (data.CveMov == "005") {
                        $("#cvemov_seg").val('005-VENTA (SALIDA DE GANADO)');
                    }
                    if (data.CveMov == "006") {
                        $("#cvemov_seg").val('006-SALIDAS DESECHO');
                    }
                    if (data.CveMov == "007") {
                        $("#cvemov_seg").val('007-INVENTARIO');
                    }



                    $("#folio_seg").val(data.Folio);
                    $("#sucursal_seg").val(data.Sucursal);
                    $("#destino_seg").val(data.Destino);
                    $("#linea_seg").val(data.PropietarioCamion);
                    $("#conductor_seg").val(data.Chofer);
                    $("#placas_seg").val(data.Placas);
                    $("#maquilador_seg").val(data.Maquilador);
                    $("#predio_cliente_seg").val(data.PredioCliente);
                    $("#obs_seg").val(data.ObsP1);
                    $("#peso_origen_seg").val(data.PesoOri);
                    $("#mat_seca_seg").val(data.PMatSec);
                    $("#peso_bascula_seg").val(data.Peso1);
                    peso_real = data.Peso1;



                    $("#tabla_p2").val(data.Tabla);
                    $("#variedad_p2").val(data.Variedad);
                    $("#corte_p2").val(data.Corte);
                    $("#pacas_p2").val(data.Pacas);
                    $("#ensilador_p2").val(data.Ensilador);




                    var agrupada = data.agrupada;
                    if (agrupada == true) { $("#agrupada").prop('checked', true) }
                    else { $("#agrupada").prop('checked', false) }
                    fichaencontrada = 1;
                    jsRemoveWindowLoad();
                }
                else {
                    iziToast.warning({
                        title: '',
                        message: 'No se encontró la ficha o ya se registro la segunda pesada',
                    });
                    $(".input_clear").val('');
                    fichaencontrada = 0;
                    jsRemoveWindowLoad();
                }

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
            message: 'Favor de ingresar una ficha',
        });
        $("#id_ficha_g_seg").focus();
        jsRemoveWindowLoad();
    }
}

function GuardarSegundaPesadaSab() {

    var id_establo = $("#id_establo_seg").val();
    var ficha = $("#id_ficha_g_seg").val();
    var peso_2 = speso_real;
    var peso1 = parseFloat($("#peso_bascula_seg").val());
    var peso2 = parseFloat($("#peso_bascula_2").val());
    pesoT = Math.abs(peso1 - peso2);
    if (peso2 == 0) {
        iziToast.warning({
            title: 'Aviso',
            message: 'La segunda pesada no puede ser 0',
        });
        return;
    }
    $("#peso_bascula_t").val(pesoT);
    var peso_t = pesoT;
    if (fichaencontrada == 1) {
        if (peso_2 != "" && !isNaN(pesoT) && ficha != "") {
            jsShowWindowLoad();
            $.ajax({
                type: "POST",
                async: false,
                url: "../BASCULASAB/GuardarSegundaPesadaSab",
                data: {
                    fichas: ficha,
                    peso_2: peso_2,
                    peso_t: pesoT,
                    id_establo: id_establo
                },
                success: function (response) {
                    if (response == "Exito") {
                        iziToast.success({
                            title: 'Exito',
                            message: 'Segunda pesada generada correctamente',
                        });
                        ImpresionFichaDirecto(ficha);
                        $(".inputs_clear").val('');
                        $("#agrupada").prop('checked', false);
                        fichaencontrada = 0;
                        speso_real = 0;
                        pesoT = 0;
                    }
                    else {
                        iziToast.warning({
                            title: 'Aviso',
                            message: 'No se encontró la ficha. Asegurese de no modificar el campo una vez se haya buscado',
                        });
                        fichaencontrada = 0;
                    }
                    jsRemoveWindowLoad();
                },
                error: function (xhr, status, error) {
                    console.error(error);
                    jsRemoveWindowLoad();
                }
            });
            jsRemoveWindowLoad();
        }
        else {
            iziToast.error({
                title: 'Asegurate de que el PESO 2 tenga un valor',
                message: '',
            });
            jsRemoveWindowLoad();
        }
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de buscar la ficha',
        });
        jsRemoveWindowLoad();
    }
}


function ImpresionFichaDirecto(ficha) {
    jsShowWindowLoad();
    var establo = $("#id_establo_seg").val();
    var folio = $("#folio_seg").val();
    var placas = $("#placas_seg").val();
    var chofer = $("#conductor_seg").val();
    var producto = $("#articulo_seg").val();
    var pesada1 = parseFloat($("#peso_bascula_seg").val());
    var pesada2 = parseFloat(speso_real);
    var pesadat = parseFloat(pesoT);
    var observaciones = $("#obs_seg").val();
    var cliente = $("#proveedor_seg").val();

    var newTab = window.open('', '_blank');

    $.ajax({
        type: "POST",
        url: "../BASCULASAB/GenerarFichaPDF",
        data: {
            ficha: ficha,
            establo: establo,
            folio: folio,
            placas: placas,
            chofer: chofer,
            producto: producto,
            pesada1: pesada1,
            pesada2: pesada2,
            pesadat: pesadat,
            observaciones: observaciones,
            cliente: cliente
        },
        success: function (response) {
            jsRemoveWindowLoad();
            newTab.document.open();
            newTab.document.write(response);
            newTab.document.close();
            newTab.onload = function () {
                newTab.print();
                newTab.close();
            };
        },
        error: function (xhr, status, error) {
            console.error(error);
            jsRemoveWindowLoad();
        }
    });
}

function ReImpresionFichaDirecto() {
    jsShowWindowLoad();
    var ficha = $("#reimp_ficha").val();
    var establo = $("#id_establo_re").val();
    if (ficha.length > 5) {
        $.ajax({
            type: "POST",
            url: "../BASCULASAB/ReimprecionFichaPDF",
            data: {
                fichas: ficha,
                id_establo: establo
            },
            success: function (response) {
                jsRemoveWindowLoad();
                if (response != "") {
                    var newTab = window.open('', '_blank');
                    newTab.document.open();
                    newTab.document.write(response);
                    newTab.document.close();
                    newTab.onload = function () {
                        newTab.print();
                        newTab.close();
                    };
                }
                else {
                    //newTab.close();
                    iziToast.warning({
                        title: 'Aviso',
                        message: 'No se encontró la ficha',
                    });
                    $("#reimp_ficha").val("");
                }
            },
            error: function (xhr, status, error) {
                //newTab.close();
                console.error(error);
                jsRemoveWindowLoad();
                iziToast.warning({
                    title: 'Aviso',
                    message: 'No se encontró la ficha',
                });
            }
        });
    }
    else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de ingresar una ficha valida',
        });
        jsRemoveWindowLoad();
    }
}


var pesoT = 0;
function CalcularPesoTotal() {
    var peso1 = parseFloat($("#peso_bascula_seg").val());
    var pesoStr = $("#peso_bascula_2").val().trim();
    pesoStr = pesoStr.replace(/[^0-9.,]/g, '');
    var peso2 = parseFloat(pesoStr.replace(',', '.'));
    pesoT = Math.abs(peso1 - peso2);
    $("#peso_bascula_t").val(pesoT);
    $("#peso_bascula_2").val(peso2);
}

function ConsultarFichasReporteSAB() {
    jsShowWindowLoad();
    var id_establo = $("#id_establo_reporte").val();
    var fecha_inicio = $("#fecha_i").val();
    var fecha_fin = $("#fecha_f").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULASAB/ConsultarFichasReporteSAB",
        data: {
            id_establo: id_establo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            try {
                $("#table_fichas_reporte").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_fichas_reporte").html(response);
            $('#table_fichas_reporte').DataTable({
                keys: true,
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
                language: {
                    encoding: "UTF-8"
                },
                responsive: false
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
var peso_real1 = 0;
var peso_real2 = 0;
var speso_real = 0;
function ObtenerPesoBascula(modo) {
    var id_establo = $("#id_establo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULASAB/ObtenerPesoBascula",
        data: { id_establo: id_establo },
        success: function (response) {
            if (response != "") {
                //PRIMERA PESADA
                if (modo == 1) {
                    var pesoStr = response;
                    pesoStr = pesoStr.replace(/[^0-9.,]/g, '');
                    var peso = parseFloat(pesoStr.replace(',', '.'));
                    $("#peso_bascula").val(peso);
                    peso_real1 = peso;

                }
                else {
                    //SEGUNDA PESADA
                    $("#peso_bascula_2").val(response);
                    peso_real2 = $("#peso_bascula_1").val();
                    speso_real = response;
                    CalcularPesoTotal();
                }


            }
            else {
                alert("No se pudo abrir el puerto");
                if (modo == 1) {
                    $("#peso_bascula").val(response);
                    peso_real1 = 0;
                }
                else {
                    $("#peso_bascula_2").val(response);
                    peso_real2 = 0;
                    speso_real = 0;
                }
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarPozosEstablo() {
    var id_establo = $("#id_establo").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/ConsultarPozos",
        data: { id_establo: id_establo },
        success: function (response) {
            $("#no_pozo").html(response);
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function ConsultarUltimaFicha() {
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/ConsultarUltimaFicha",
        data: {
        },
        success: function (response) {
            if (response != 0) {
                $("#id_ficha_g").val(response);
            }
            else {
                iziToast.error({
                    title: 'Error',
                    message: 'Ocurrió un problema al consultar la ultima ficha',
                });
            }
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

function GuardarFicha() {
    var peso_1 = $("#peso_bascula").val();
    var no_pozo = $("#no_pozo").val();

    if (peso_1 == "") {
        iziToast.error({
            title: 'Asegurate de que el PESO tenga un valor',
            message: '',
        });
    }
    else if (peso_1 == "No se pudo abrir el puerto") {
        iziToast.warning({
            title: 'Aviso',
            message: 'Favor de verificar el puerto',
        });
    }
    else if (no_pozo == undefined || isNaN(no_pozo)) {
        iziToast.warning({
            title: 'Aviso',
            message: 'Asegurese de seleccionar un nùmero de pozo',
        });
    }

    else {

        var id_establo = $("#id_establo").val();

        var select_proveedor = $("#select_proveedor").val();
        var select_articulo = $("#select_articulo").val();

        var cvemov = $("#cvemov").val();
        var tmov = $("#tmov").val();

        var folio = $("#folio").val();
        var sucursal = $("#sucursal").val();
        var destino = $("#destino").val();
        var linea_trasp = $("#linea_trasp").val();

        var chofer = $("#conductor").val();
        var placas = $("#placas").val();
        var maquilador = $("#maquilador").val();
        var predio_cliente = $("#predio_cliente").val();

        var obs = $("#obs").val();
        var peso_origen = $("#peso_origen").val();
        var mat_seca = $("#mat_seca").val();

        var agrupada = false;
        if ($("#agrupada").is(':checked')) { agrupada = true; }

        var c_ficha = {};
        c_ficha.id_establo = id_establo;
        c_ficha.id_no_pozo = no_pozo;
        c_ficha.id_bascula_proveedor = select_proveedor;
        c_ficha.id_bascula_articulo = select_articulo;
        c_ficha.id_codigo_movimiento = cvemov;
        c_ficha.id_tipo_movimiento = tmov;
        c_ficha.folio = folio;
        c_ficha.sucursal = sucursal;
        c_ficha.destino = destino;
        c_ficha.id_linea_transportista = linea_trasp;
        c_ficha.chofer = chofer;
        c_ficha.placas = placas;
        c_ficha.maquilador = maquilador;
        c_ficha.predio_cliente = predio_cliente;
        c_ficha.observaciones = obs;
        c_ficha.peso_origen = peso_origen;
        c_ficha.mat_seca = mat_seca;
        c_ficha.agrupada = agrupada;
        c_ficha.termina = false;

        $.ajax({
            type: "POST",
            async: false,
            url: "../BASCULA/GuardarFicha",
            data: {
                c_ficha: c_ficha,
                peso_1: peso_1
            },
            success: function (response) {
                if (response == "True") {
                    iziToast.success({
                        title: 'Exito',
                        message: 'Ficha registrada correctamente!',
                    });
                    $(".input_clear").val('');
                    ConsultarUltimaFicha();
                }
                else {
                    iziToast.error({
                        title: 'Error',
                        message: 'Ocurrió un problema registrar la ficha',
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error(error);
            }
        });
    }
}

function ConsultarInfoFicha() {
    var ficha = $("#id_ficha_g_seg").val();
    if (ficha != "") {
        $.ajax({
            type: "POST",
            async: false,
            url: "../BASCULA/ConsultarInfoFicha",
            data: { ficha: ficha },
            success: function (response) {
                if (response != "[]") {
                    var data = $.parseJSON(response);

                    $("#no_pozo_seg").val(data[0].no_pozo);
                    $("#proveedor_seg").val(data[0].nombre_prov);
                    $("#articulo_seg").val(data[0].nombre_articulo);
                    $("#tmov_seg").val(data[0].tmov);
                    $("#cvemov_seg").val(data[0].cvemo);
                    $("#folio_seg").val(data[0].folio);
                    $("#sucursal_seg").val(data[0].sucursal);
                    $("#destino_seg").val(data[0].destino);
                    $("#linea_seg").val(data[0].linea_trasp);
                    $("#conductor_seg").val(data[0].conductor);
                    $("#placas_seg").val(data[0].placas);
                    $("#maquilador_seg").val(data[0].maquilador);
                    $("#predio_cliente_seg").val(data[0].predio_cliente);
                    $("#obs_seg").val(data[0].observaciones);
                    $("#peso_origen_seg").val(data[0].peso_origen);
                    $("#mat_seca_seg").val(data[0].peso_materia_seca);
                    $("#peso_bascula_seg").val(data[0].peso_1);

                    var agrupada = data[0].agrupada;
                    if (agrupada == true) { $("#agrupada").prop('checked', true) }
                    else { $("#agrupada").prop('checked', false) }

                }
                else {
                    iziToast.warning({
                        title: '',
                        message: 'No se encontró ninguna ficha con ese numero',
                    });
                    $(".input_clear").val('');
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
            message: 'Favor de ingresar una ficha',
        });
        $("#id_ficha_g_seg").focus();
    }
}

function GuardarSegundaPesada() {
    var ficha = $("#id_ficha_g_seg").val();
    var peso_2 = $("#peso_bascula_2").val();
    if (peso_2 != "" || peso_2 != undefined || !isNaN(peso_2) || ficha != "") {
        $.ajax({
            type: "POST",
            async: false,
            url: "../BASCULA/GuardarSegundaPesada",
            data: {
                ficha: ficha,
                peso_2: peso_2
            },
            success: function (response) {
                if (response.length > 10) {
                    iziToast.error({
                        title: response,
                        message: '',
                    });
                }
                else {
                    $("#peso_bascula_t").val(response);
                    iziToast.success({
                        title: 'Segunda pesada generada correctamente',
                        message: '',
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
            title: 'Asegurate de que el PESO 2 tenga un valor',
            message: '',
        });
    }

}

function ConsultarFichasReporte() {
    var id_establo = $("#id_establo_reporte").val();
    var fecha_inicio = $("#fecha_i").val();
    var fecha_fin = $("#fecha_f").val();
    $.ajax({
        type: "POST",
        async: false,
        url: "../BASCULA/ConsultarFichasReporte",
        data: {
            id_establo: id_establo,
            fecha_inicio: fecha_inicio,
            fecha_fin: fecha_fin
        },
        success: function (response) {
            try {
                $("#table_fichas_reporte").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_fichas_reporte").html(response);
            $('#table_fichas_reporte').DataTable({
                keys: true,
                ordering: true,
                paging: false,
                dom: "Bfrtip",
                buttons: [{}],
                responsive: !0
            });
        },
        error: function (xhr, status, error) {
            console.error(error);
        }
    });
}

var campos_validar = ["folio", "sucursal", "destino", "conductor", "placas", "maquilador", "predio_cliente", "obs", "peso_origen", "mat_seca"];

function ExportarFichasBasculaExcel() {
    var id_establo = $("#id_establo_reporte").val();
    var fecha_inicio = $("#fecha_i").val();
    var fecha_fin = $("#fecha_f").val();

    if (fecha_inicio !== "" && fecha_fin !== "") {
        jsShowWindowLoad();

        fetch('../BASCULASAB/ExportarFichasBasculaExcel', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: new URLSearchParams({
                id_establo: id_establo,
                fecha_inicio: fecha_inicio,
                fecha_fin: fecha_fin
            })
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok.');
                }
                return response.blob().then(blob => {
                    var disposition = response.headers.get('Content-Disposition');
                    var filename = 'Reporte_Fuera_de_Rango.xlsx';

                    if (disposition && disposition.indexOf('attachment') !== -1) {
                        var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                        var matches = filenameRegex.exec(disposition);
                        if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                    }

                    var link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = filename;
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                });
            })
            .catch(error => {
                console.error('Error:', error);
            })
            .finally(() => {
                jsRemoveWindowLoad();
            });
    } else {
        iziToast.warning({
            title: 'Aviso',
            message: 'Es necesario ingresar la fecha de inicio y fecha final.'
        });
    }
}