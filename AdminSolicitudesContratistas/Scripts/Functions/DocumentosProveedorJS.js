
$(document).ready(function () {

    IniciarDocumentosDelProveedor();

});


function IniciarDocumentosDelProveedor() {
    //$("#txtAñoDocSubirDocumentoProveedor").val('');

    $("#cardSubirDocumentosProveedor").css('display', 'none');
    $("#cardListadoDocumentosProveedor").css('display', 'none');

    let currentDate = new Date();
    let año = currentDate.getFullYear();
    console.log('año ' + año);
    $("#txtAñoDocSubirDocumentoProveedor").val(año);
    //Setear máximo de año como el año actual
    $("#lblMsgSubirDocumentosProveedor").text('');
    $("#alertMsgSubirDocumentosProveedor").css('display', 'none');


    //LIMPIAR TXT
    $("#fileSubirDocumentoProveedor").val('');
    $("#txtDescripcionSubirDocumentoProveedor").val();
    $("#txtPalabrasClaveDocumentoProveedor").val();
    $('#cmbAreaSubirDocumentoProveedor').val(0);
    $('#cmbTipoDocSubirDocumentoProveedor').empty();
    $("#cmbTipoDocSubirDocumentoProveedor").append($("<option value='' selected > (Seleccionar...) </option>"));

    $("#lblRazonSocialProveedor").css('display', 'none');
    $("#hiddenRutProveedorSubirDoc").val('');
}




function CargarDocumentosDelProveedor(rutProveedor) {
    IniciarDocumentosDelProveedor();

    $('#btnBuscarProveedor').css('display', 'none');
    $('#btnProcesandoBuscarProveedor').css('display', 'block');

    $.ajax({
        type: 'POST',
        url: "/DocumentosDelProveedor/JsonTraerDocumentosDelProveedor",
        dataType: "json",
        data: { rutProveedor: rutProveedor, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
        success: function (datos) {
            if (datos === "EMPTY") {
                $("#cardSubirDocumentosProveedor").css('display', 'none');
                $("#cardListadoDocumentosProveedor").css('display', 'none');
                $("#lblRazonSocialProveedor").css('display', 'block');
                $("#lblRazonSocialProveedor").text('');
                $("#hiddenRutProveedorSubirDoc").val('');
                alert('Proveedor no encontrado');

            } else {
                CargarDatosProveedor(rutProveedor);
                $("#cardSubirDocumentosProveedor").css('display', 'block');
                $("#cardListadoDocumentosProveedor").css('display', 'block');
                $('#collapseListadoDocumentosProveedor').collapse('show');
                $('#collapseSubirDocumentosProveedor').collapse('hide');
                
                bootstrapTableDocumentosDelProveedor(datos, rutProveedor);
            }

            $('#btnBuscarProveedor').css('display', 'block');
            $('#btnProcesandoBuscarProveedor').css('display', 'none');

        },
        error: function (jqXHR, textStatus, errorThrown) { }
    });
}


function CargarDatosProveedor(rutProveedor) {

    $.ajax({
        type: 'POST',
        url: "/Proveedores/JsonTraerDetalleProveedor",
        dataType: "json",
        data: { rutProveedor: rutProveedor, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
        success: function (datos) {
            if (datos === "EMPTY") {
            } else {
                $.each(datos, function (key, val) {
                    $("#lblRazonSocialProveedor").css('display', 'block');
                    $("#lblRazonSocialProveedor").text(val.RazonSocial);
                    $("#hiddenRutProveedorSubirDoc").val(val.Rut);
                });

            }
        },
        error: function (jqXHR, textStatus, errorThrown) { }
    });

}


function bootstrapTableDocumentosDelProveedor(datos, rutProveedor) {

    $('#tablaDocumentosDelProveedor').bootstrapTable("destroy");
    $('#tablaDocumentosDelProveedor').bootstrapTable({
        search: true, pagination: true, searchAlign: "center" /*searchAlign: "left"*/,
        pageSize: 200,
        showPaginationSwitch: true,
        filterControl: true, //formats: ['xlsx', 'csv', 'txt'],
        //showExport: true,
        //exportDataType: 'all',
        columns: [{
            field: 'NOMBRE_ARCHIVO',
            title: 'Nombre Archivo',
            visible: true,
            filterControl: 'input',
            sortable: true
        }, {
            field: 'DESCRIPCION',
            title: 'Descripción',
            filterControl: 'input',
            sortable: true
        }, {
            field: 'ID_SOL',
            title: 'N° Solicitud',
            filterControl: 'input',
            sortable: true
        }, {
            field: 'ID_SOL_REL',
            title: 'Sol. rel.',
            filterControl: 'input',
            sortable: true
        }, {
            field: 'ESTADO_SOLICITUD',
            title: 'Estado Sol.',
            filterControl: 'input',
            sortable: true
        }, {
            field: 'FECHA_SOLICITUD',
            title: 'Fecha Sol.',
            filterControl: 'input',
            sortable: true
        }, {
            field: 'TIPO_SOLICITUD',
            title: 'Tipo Sol.',
            filterControl: 'input',
            sortable: true
        },

            {
            field: 'TIPO_REGISTRO',
            title: 'Tipo Registro',
            filterControl: 'input',
            sortable: true
        },{
            field: 'AREA_ANALISIS',
            title: 'Área Análisis',
            filterControl: 'input',
            sortable: true
        }, {
            field: 'TIPO_DOCUMENTO',
            title: 'Tipo Documento',
            filterControl: 'select',
            visible: false,
            sortable: true
        },{
            field: 'TIPO',
            title: 'Tipo archivo',
            filterControl: 'select',
            sortable: true
        }, {
            field: 'AÑO',
            title: 'Año',
            filterControl: 'input',
            sortable: true
        }, {
            field: 'PALABRAS_CLAVE',
            title: 'Palabras Clave',
            filterControl: 'input',
            visible: false,
            sortable: true
        }, {
            field: 'VISIBLE_AL_PROVEEDOR',
            title: 'Visible al proveedor',
            filterControl: 'select',
            sortable: true,
            formatter: function (value, row, index) {
                if (value == 'S') {
                    //return '<input type="checkbox" checked disabled/>';
                    return 'Sí';
                } else {
                    //return '<input type="checkbox" disabled />';
                    return 'No';
                }
            }
        }, {
            field: '',
            title: 'Descargar',
            formatter: function (value, row, index) {
                var iconClass = "bi bi-search";
                var btnClass = "btn btn-primary";
                //var est = row.CodEstadoCHP;
                //if (est == 'SCO') {
                //    //SOLICITAN CORRECCIONES
                //    iconClass = "bi bi-exclamation-circle";
                //    btnClass = "btn btn-warning";
                //} 
                //console.log(value);
                //console.log(row);
                //console.log(index);

                return '<div class="container">' +
                    ' <input type="hidden" id="hiddenIdArchivoGesintel' + index + '" value="' + row.ID_ARCHIVO_GESINTEL + '" />  ' +
                    ' <input type="hidden" id="hiddenIndex' + index + '" value="' + index  + '" />  ' +
                    ' <button id="btnProcesandoBajarArchivo'+index+'" class="btn btn-primary" type="button" style="display:none;" disabled> ' +
                        ' <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ' +                        
                    ' </button>' + 
                    ' <a href="/Solicitudes/BajarArchivoGesintel?idArchivoGesintel=' + row.ID_ARCHIVO_GESINTEL + '&rutProveedor=' + rutProveedor + '" id="btnBajarArchivo'+index+'" class= "btn btn-primary download-link"> <i class="bi bi-download"></i></a> ' +
                    '</div> ';
            }
        }


        ],
        data: datos
    });

}


$('#tablaDocumentosDelProveedor').on('click', 'a.download-link', function (event) {
    // Prevent the default behavior (i.e., navigating to the file download URL)
    event.preventDefault();

    var index = $(this).closest('tr').find('input[type="hidden"][id^="hiddenIndex"]').val();
    $('#btnBajarArchivo' + index).css('display', 'none');
    $('#btnProcesandoBajarArchivo' + index).css('display', 'block');

    //var idArchivoGesintel = $(this).closest('tr').find('input[type="hidden"]').val();
    var idArchivoGesintel = $(this).closest('tr').find('input[type="hidden"][id^="hiddenIdArchivoGesintel"]').val();
    


    //var rutProveedor = $("#lblRutProveedor").text();
    var rutProveedor = $('#txtBuscarRutProveedor').val();

    //console.log('idArchivoGesintel ' + idArchivoGesintel);
    //console.log('rutProveedor ' + rutProveedor);

    $.get("/Solicitudes/BajarArchivoGesintel", { idArchivoGesintel: idArchivoGesintel, rutProveedor: rutProveedor })
        .done(function (data) {
            if (data.success === undefined) {
                
                window.location.href = "/Solicitudes/BajarArchivoGesintel?idArchivoGesintel=" + idArchivoGesintel + "&rutProveedor=" + rutProveedor;
            } else {
                alert(data.message);
            }

            $('#btnBajarArchivo' + index).css('display', 'block');
            $('#btnProcesandoBajarArchivo' + index).css('display', 'none');

        }).fail(function () {
            // Handle AJAX error
            alert("Error al intentar descargar archivo: Intente más tarde");
        });
});




function CargarTipoDocSubirDoc() {
    var areaDocumento = $("#cmbAreaSubirDocumentoProveedor").val();
    //console.log('areaDocumento ' + areaDocumento);
    if (areaDocumento != '0') {
        $.ajax({
            type: 'POST',
            url: "/DocumentosDelProveedor/JsonTraerTipoDocumento",
            dataType: "json",
            data: { areaDocumento: areaDocumento, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (datos) {
                if (datos === "EMPTY") {
                    $('#cmbTipoDocSubirDocumentoProveedor').empty();
                } else {
                    $('#cmbTipoDocSubirDocumentoProveedor').empty();
                    $("#cmbTipoDocSubirDocumentoProveedor").append($("<option value='' selected > (Seleccionar...) </option>"));
                    $(datos).each(function () {
                        $("#cmbTipoDocSubirDocumentoProveedor").append($("<option></option>").val(this.CODIGO_DOC).html(this.NOMBRE_DOC));
                    });

                }
            },
            error: function (jqXHR, textStatus, errorThrown) { }
        });
    } else {
        $('#cmbTipoDocSubirDocumentoProveedor').empty();
        $("#cmbTipoDocSubirDocumentoProveedor").append($("<option value='' selected > (Seleccionar...) </option>"));
    }


}



$("#btnSubirDocumentoProveedor").on("click", function () {
    $('#btnSubirDocumentoProveedor').css('display', 'none');
    $('#btnProcesandoSubirDocumentoProveedor').css('display', 'block');

    //Variables para el Log
    var rutProveedor = $("#hiddenRutProveedorSubirDoc").val();
    var nombreArchivo = $('#fileSubirDocumentoProveedor').val().replace(/C:\\fakepath\\/i, '');
    var descripcion = $('#txtDescripcionSubirDocumentoProveedor').val();
    var area = $('#cmbAreaSubirDocumentoProveedor option:selected').html();
    var tipoDoc = $('#cmbTipoDocSubirDocumentoProveedor option:selected').html();
    var año = $('#txtAñoDocSubirDocumentoProveedor').val();
    //console.log('descripcion: ' + descripcion);
    //console.log('area: ' + area);
    //console.log('tipoDoc: ' + tipoDoc);
    //console.log('año: ' + año);
    var formData = new FormData($("#formArchivoProveedor")[0]);
    $.ajax({
        url: $("#formArchivoProveedor").attr("action"),
        type: "POST",
        data: formData,
        processData: false, // Important: tell jQuery not to process the data
        contentType: false, // Important: tell jQuery not to set contentType
        success: function (result) {
            $("#lblMsgSubirDocumentosProveedor").text(result.message);
            if (result.success) {
                //TRUE                
                $("#alertMsgSubirDocumentosProveedor").css('display', 'block');
                setTimeout(function () {
                    $("#alertMsgSubirDocumentosProveedor").css('display', 'none');
                }, 3000);
            } else {
                //FALSE
                $("#alertMsgSubirDocumentosProveedor").css('display', 'block');
            }
            $('#btnSubirDocumentoProveedor').css('display', 'block');
            $('#btnProcesandoSubirDocumentoProveedor').css('display', 'none');

            //GUARDAR LOG
            var codTipoOperacion = 'ARCHP'; //
            var codEstado = ''; // ???????
            var comentario1 = 'Subida de Archivos del Proveedor por Equipo MOP';
            var comentario2 = 'Nombre del archivo: ' + nombreArchivo;
            var comentario3 = 'Descripción: ' + descripcion + ' | Area: ' + area + ' | Tipo Documento: ' + tipoDoc + ' | Año: ' + año;
            GuardarMOPLogProveedor(rutProveedor, codTipoOperacion, codEstado, comentario1, comentario2, comentario3);

            CargarDocumentosDelProveedor(rutProveedor);
        },
        error: function (error) {
            console.log(error);
        }
    });


});







$('#cmbAreaSubirDocumentoProveedor').on('change', function () {
    CargarTipoDocSubirDoc();
});


$('#btnVerListadoDocumentosProveedor').on('click', function () {
    $('#collapseListadoDocumentosProveedor').collapse('toggle');
});

$('#btnVerSubirDocumentosProveedor').on('click', function () {
    $('#collapseSubirDocumentosProveedor').collapse('toggle');
});



$('#btnBuscarProveedor').on('click', function () {

    var rutProveedor = $('#txtBuscarRutProveedor').val();
    CargarDocumentosDelProveedor(rutProveedor);
});