function ConsultarPTTO() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/ConsultarPTTO",
        data: {

        },
        success: function (response) {
            try {
                $("#table_ptto").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_presupuesto_sab").html(response);
            $('#table_ptto').DataTable({
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

function CuentasContables() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/CuentasContables",
        data: {

        },
        success: function (response) {
            try {
                $("#tabla_cuentas").dataTable().fnDestroy();
            } catch (e) { }
            $("#div_cuenta_sab").html(response);
            $('#tabla_cuentas').DataTable({
                keys: true,
                ordering: true,
                paging: true,
                dom: "Bfrtip",
                buttons: [{ }],
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

function VinculoIntegracion() {
    jsShowWindowLoad();
    $.ajax({
        type: "POST",
        async: false,
        url: "../UTILERIAS/CalcularIntegracionGeneral",
        data: {

        },
        success: function (response) {
            if (response == "True" || response == true) {
                iziToast.success({
                    title: 'Aviso',
                    message: 'Exito'
                });
            }
            else {
                iziToast.warning({
                    title: 'Aviso',
                    message: response
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

function UpdateSiibPtto(m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12) {

    var cuenta = $("#id_cuenta_contable").val();

    var script1 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m1 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '1';";
    var script2 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m2 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '2';";
    var script3 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m3 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '3';";
    var script4 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m4 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '4';";
    var script5 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m5 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '5';";
    var script6 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m6 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '6';";
    var script7 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m7 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '7';";
    var script8 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m8 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '8';";
    var script9 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m9 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '9';";
    var script10 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m10 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '10';";
    var script11 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m11 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '11';";
    var script12 = "update [BETA_CORP].[dbo].[C_presupuestos_cuentas_meses_anios] set valor_presupuesto = '" + m12 + "' where id_cuenta_contable_g = '" + cuenta + "' and id_mes = '12';";

    // Concatenate all scripts into a single string, separated by newlines
    var allScripts = script1 + "\n" + script2 + "\n" + script3 + "\n" + script4 + "\n" + script5 + "\n" + script6 + "\n" + script7 + "\n" + script8 + "\n" + script9 + "\n" + script10 + "\n" + script11 + "\n" + script12;

    // Set the concatenated scripts as the value of the textarea
    $("#vinculo_script").val(allScripts);


}