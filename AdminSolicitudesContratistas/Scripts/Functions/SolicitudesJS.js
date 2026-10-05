
(function ($) {
    'use strict'; // Habilita el modo estricto de JS.

    var isLoadingSolicitud = false;

    // =========================================================================
    // 1. CÓDIGO DE INICIALIZACIÓN (SE EJECUTA CUANDO EL DOM ESTÁ LISTO)
    // =========================================================================
    $(document).ready(function () {
        // Asignación de eventos globales
        $(document).ajaxError(function (e, xhr) {
            if (xhr.status == 401) window.location = "../Home/Login";
            else if (xhr.status == 403) alert("No tiene permisos para acceder a este recurso.");
        });

        // Asignación de todos los eventos de la página en un solo lugar
        asignarEventos();

        // Carga inicial de datos de la página
        IniciarForm();
        CargarFiltros();
    });

    // =========================================================================
    // 2. ASIGNACIÓN DE EVENTOS (ON CLICK, ON CHANGE, ETC.)
    // =========================================================================
    function asignarEventos() {
        // Botones de Filtros
        $('#btnLimpiarFiltros').on('click', LimpiarFiltros);

        // Botones y Selects del Modal
        $('#btnGuardarSolicitud').on('click', handleGuardarSolicitud);
        $('#btnDescargaMasivaDocumentos').on('click', DescargaMasivaDocumentos);
        $('#cmbAccionesSolicitud').on('change', function () {
            SeleccionarAccionSolicitud($(this).val());
        });

        // Eventos de los Collapse (Acordeones)
        $('#btnVerComentarios').on('click', function () { $('#collapseComentarios').collapse('toggle'); });
        $('#btnVerDocumentosAdjuntos').on('click', function () { $('#collapseDocumentosAdjuntos').collapse('toggle'); });
        $('#btnVerLogSolicitud').on('click', function () { $('#collapseLog').collapse('toggle'); });
        $('#btnVerObservacionesSolicitud').on('click', function () { $('#collapseObservacionesSolicitud').collapse('toggle'); });
        $('#btnVerDocumentoResolucion').on('click', function () { $('#collapseDocumentoResolucion').collapse('toggle'); });
        $('#btnVerSolicitudesRelacionadas').on('click', function () { $('#collapseSolicitudesRelacionadas').collapse('toggle'); });

        // Delegación de eventos para elementos dinámicos (tablas)
        $('#tablaDocumentosAdjuntos').on('click', 'a.download-link', handleDownloadLinkClick);
        $('#tablaDocumentoResolucion').on('click', 'a.download-link', handleDownloadResolucionLinkClick);
        // El evento para .btnVerSolicitudRelacionada se asigna dinámicamente en CargarSolicitudesRelacionadasEnTabla
    }

    // =========================================================================
    // 3. DEFINICIONES DE FUNCIONES
    // =========================================================================

    // #region INICIALIZACIÓN Y FILTROS DE PÁGINA

    function IniciarForm() {
        $('#alertMensajes').css('display', 'none');
        $('#alertInformacion').css('display', 'none');

        $('#btnDownloadExcel').css('display', 'none');

    }

    //function CargarFiltros() {
    //    CargarFiltroTipoRegistroMOP();
    //    CargarTiposSolicitud();
    //    CargarEstadosSolicitud();
    //    var userRegion = $("#hiddenUserRegion").val();
    //    var lblUserRegion = 'TODAS';
    //    if (userRegion != 0) {
    //        lblUserRegion = userRegion;
    //    }

    //    $("#lblUserRegion").text(lblUserRegion);
    //}

    function CargarFiltros() {
        // Definimos los selectores de nuestros dropdowns
        var $cmbEstado = $('#cmbBuscarEstadoMOP');
        var $cmbTipoRegistro = $('#cmbBuscarTipoRegistro');
        var $cmbTipoSolicitud = $('#cmbBuscarTipoSolicitud');

        // 1. Poner los dropdowns en estado "Cargando..." ANTES de la llamada AJAX.
        var dropdowns = $cmbEstado.add($cmbTipoRegistro).add($cmbTipoSolicitud);
        dropdowns.empty().append($('<option disabled selected>Cargando...</option>')).prop('disabled', true);

        // Obtenemos los parámetros necesarios para la llamada
        var userTipoRegistro = $("#hiddenUserTipoRegistro").val();
        var userTipoSolicitud = $("#hiddenUserTipoSolicitud").val();

        // 2. Realizamos la ÚNICA llamada AJAX
        $.ajax({
            type: 'POST',
            url: "/Solicitudes/JsonTraerFiltrosBusquedaSolicitudes", 
            dataType: "json",
            data: {
                userTipoRegistro: userTipoRegistro,
                userTipoSolicitud: userTipoSolicitud,
                __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
            },
            success: function (data) {
                // 3. En caso de éxito, poblamos los dropdowns con los datos reales
                PoblarDropdown($cmbEstado, data.EstadosSolicitud, 'CODIGO', 'DESCRIPCION');
                PoblarDropdown($cmbTipoRegistro, data.TiposRegistro, 'CODIGO', 'DESCRIPCION');
                PoblarDropdown($cmbTipoSolicitud, data.TiposSolicitud, 'CODIGO', 'DESCRIPCION');
            },
            error: function () {
                // 4. En caso de error, mostramos un mensaje de error en los dropdowns
                dropdowns.empty().append($('<option disabled selected>Error al cargar</option>')).prop('disabled', true);
                alert('No se pudieron cargar los filtros de búsqueda.');
            }
        });

        // Lógica restante de la función original
        var userRegion = $("#hiddenUserRegion").val();
        $("#lblUserRegion").text(userRegion != 0 ? userRegion : 'TODAS');
    }

    function PoblarDropdown($selector, datos, valorProp, textoProp) {
        $selector.empty();
        $selector.append($("<option value='' selected> (Seleccionar...) </option>"));

        if (datos && datos.length > 0) {
            $.each(datos, function () {
                $selector.append($("<option></option>").val(this[valorProp]).html(this[textoProp]));
            });
        } else {
            $selector.append($("<option value='' disabled> (Sin datos) </option>"));
        }

        //se reactiva dropdown
        $selector.prop('disabled', false);
    }

    //function CargarFiltroTipoRegistroMOP() {
    //    //var codigoPadre = 2237;//TIPO DE REGISTRO
    //    var userTipoRegistro = $("#hiddenUserTipoRegistro").val();
    //    $.ajax({
    //        type: 'POST',
    //        url: "/Solicitudes/JsonListarFiltroTipoRegistro",
    //        dataType: "json",
    //        data: { codTipoRegistro: userTipoRegistro, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
    //        success: function (datos) {
    //            if (datos === "EMPTY") {
    //                $('#cmbBuscarTipoRegistro').empty();
    //                $("#cmbBuscarTipoRegistro").append($("<option value='' selected > (SIN DATOS) </option>"));
    //            } else {
    //                $('#cmbBuscarTipoRegistro').empty();
    //                $("#cmbBuscarTipoRegistro").append($("<option value='' selected > (Seleccionar...) </option>"));
    //                $(datos).each(function () {
    //                    $("#cmbBuscarTipoRegistro").append($("<option></option>").val(this.CODIGO).html(this.DESCRIPCION));
    //                });
    //            }
    //        },
    //        error: function (jqXHR, textStatus, errorThrown) { }
    //    });
    //}

    //function CargarTiposSolicitud() {
    //    //var codigoPadre = 2241;//TIPO DE SOLICITUD
    //    var userTipoSolicitud = $("#hiddenUserTipoSolicitud").val();
    //    $.ajax({
    //        type: 'POST',
    //        url: "/Solicitudes/JsonListarFiltroTipoSolicitud",
    //        dataType: "json",
    //        data: { codTipoSolicitud: userTipoSolicitud, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
    //        success: function (datos) {
    //            if (datos === "EMPTY") {
    //                $('#cmbBuscarTipoSolicitud').empty();
    //                $("#cmbBuscarTipoSolicitud").append($("<option value='' selected > (SIN DATOS) </option>"));
    //            } else {
    //                $('#cmbBuscarTipoSolicitud').empty();
    //                $("#cmbBuscarTipoSolicitud").append($("<option value='' selected > (Seleccionar...) </option>"));
    //                $(datos).each(function () {
    //                    $("#cmbBuscarTipoSolicitud").append($("<option></option>").val(this.CODIGO).html(this.DESCRIPCION));
    //                });

    //            }
    //        },
    //        error: function (jqXHR, textStatus, errorThrown) { }
    //    });
    //}

    //function CargarEstadosSolicitud() {
    //    var codigoPadre = 2230;//ESTADO SOLICITUD
    //    $.ajax({
    //        type: 'POST',
    //        url: "/Solicitudes/JsonListarParametrosPorTipo",
    //        dataType: "json",
    //        data: { codigoPadre: codigoPadre, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
    //        success: function (datos) {
    //            if (datos === "EMPTY") {

    //            } else {
    //                $('#cmbBuscarEstadoMOP').empty();
    //                $("#cmbBuscarEstadoMOP").append($("<option value='' selected > (Seleccionar...) </option>"));
    //                $(datos).each(function () {
    //                    $("#cmbBuscarEstadoMOP").append($("<option></option>").val(this.CODIGO).html(this.DESCRIPCION));
    //                });

    //            }
    //        },
    //        error: function (jqXHR, textStatus, errorThrown) { }
    //    });
    //}


    function LimpiarFiltros() {
        $("#txtBuscarRutProveedor").val('');
        $("#txtBuscarRazonSocial").val('');
        $("#txtBuscarNumeroSolicitud").val('');

        $("#cmbBuscarEstadoMOP option:selected").prop("selected", false);
        $("#cmbBuscarTipoRegistro option:selected").prop("selected", false);
        $("#cmbBuscarTipoSolicitud option:selected").prop("selected", false);

        $('#alertMensajes').css('display', 'none');
    }

    // #endregion

    // #region LÓGICA PRINCIPAL DEL MODAL (CARGA Y MANEJO)

    function CargarDatosSolicitud(numSolicitud, refresh) {

        if (isLoadingSolicitud) {
            console.log("Carga de solicitud bloqueada, ya hay una en curso.");
            return;
        }

        isLoadingSolicitud = true;
        console.log("Iniciando carga para solicitud: " + numSolicitud);

        IniciarModal();
        TituloModal('Detalle de Solicitud N° ' + numSolicitud);
        //showWaitingModal();
        $('#loadingSpinnerSolicitud').show(); 
        $('#contenidoSolicitud').hide();      
        $('#modalSolicitud').modal('show');   

        $.ajax({
            type: 'POST',
            url: "/Solicitudes/JsonTraerDetalleCompletoSolicitud",
            dataType: "json",
            data: { numSolicitud: numSolicitud, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (data) {
                console.log("Success callback recibido para solicitud: " + numSolicitud);
                if (data && data.Principal) {
                    var v = data.Principal;
                    // Poblar cabecera y datos principales
                    $("#hiddenIdSolicitud").val(numSolicitud);
                    $("#hiddenIdSolPadre").val(v.IdSolPadre);
                    $("#lblRazonSocialProveedor").text(v.RazonSocial);
                    $("#lblRutProveedor").text(v.RutProveedor);
                    $("#hiddenRutProveedor").val(v.RutProveedor);
                    $("#txtComentarios").val(v.Comentario);
                    $("#txtFechaIngreso").val(v.FechaIngreso); // Ya viene formateada desde el servidor
                    $("#txtFechaRecepcion").val(v.FechaRecepcion); // Ya viene formateada desde el servidor
                    $("#lblEstadoSolicitud").text(v.Estado);
                    $("#lblTipoRegistroSolicitud").text(v.TipoReg);
                    $("#lblTipoSolicitud").text(v.TipoSol);
                    $("#hiddenCodEstadoSolicitud").val(v.CodEstado);
                    $("#hiddenCodTipoSolicitud").val(v.CodTipoSol);
                    $("#hiddenCodTipoRegistro").val(v.CodTipoReg);
                    $("#hiddenFechaRecepcion").val(v.FechaRecepcion);
                    $("#hiddenCodRegion").val(v.IdRegion);
                    $("#hiddenEmailProveedor").val(v.EmailProveedor);
                    $("#lblEstadoProveedor").text(v.EstadoProveedor);

                    // Distribuir datos a las funciones de renderizado
                    CargarDocumentosAdjuntosEnTabla(data.DocumentosAdjuntos, v.CodEstado);
                    CargarLogSolicitudEnTabla(data.LogSolicitud);
                    CargarDocumentoResolucionEnTabla(data.DocumentosResolucion);
                    CargarObservacionesEnTabla(data.Observaciones);
                    CargarSolicitudesRelacionadasEnTabla(data.SolicitudesRelacionadas, numSolicitud);

                    

                } else {
                    
                    alert("No se encontró la solicitud o hubo un error al cargar los datos.");
                }

                //console.log("Fin del success callback.");

            },
            error: function () {
                $('#loadingSpinnerSolicitud').hide();
                alert('Ocurrió un error de comunicación al cargar la solicitud.');
            },
            complete: function () {
                $('#loadingSpinnerSolicitud').hide(); // Ocultamos el spinner
                $('#contenidoSolicitud').show();
                isLoadingSolicitud = false;
            }
        });
    }

    //SUBIDA DE DOCUMENTOS Y GUARDAR SOLICITUD POR AJAX
    function handleGuardarSolicitud() {
        
        var accion = $('#cmbAccionesSolicitud').val();
        var numSolicitud = $('#hiddenIdSolicitud').val();
        var areasSeleccionadas = true;

        if (accion == '000') {
            alert('Debe seleccionar una acción');
        } else {
            //console.log('accion ' + accion);
            //var formData = new FormData($("#formGuardarSolicitud")[0]);
            var listDetTEMP = []; // Array to hold item data objects
            //VALIDACIÓN DE ESTADO ABIERTA, PARA ENVIAR A ANALISIS
            if (accion == 'EAN') {
                $('#htmlTableDocAdj tbody tr').each(function () {
                    var rowId = $(this).attr('id'); // Get the ID of the table row
                    if (rowId != undefined) {
                        //Se extrae la parte numérica
                        rowId = rowId.replace(/^\D+/g, '');
                        console.log('nombre control hidden: ' + '#hiddenIdDet' + rowId);
                        var itemId = $('#hiddenIdDet' + rowId).val();
                        var idDet = $('#hiddenIdDet' + rowId).val();
                        //var area = $('#cmbAreaAnalisisDocumento' + rowId).val(); 
                        var area = $('#cmbAreaAnalisisDocumento' + rowId + ' option:selected').val();
                        var nombreDoc = $('#' + rowId).val();

                        if (area == '0') {
                            areasSeleccionadas = false;
                        } else {
                            var detTMP = {
                                IdDetalleSolicitud: idDet,
                                areaDeAnalisis: area
                            };
                            listDetTEMP.push(detTMP);
                        }
                        console.log('rowId ' + rowId);
                        console.log('itemId ' + itemId);
                        console.log('area ' + area);
                    }
                });

                var strList = JSON.stringify(listDetTEMP);
                $('#hiddenListDocs').val(strList);

                //formData.append('listDetTEMP', JSON.stringify(listDetTEMP));
                //formData.append('listDetTEMP', listDetTEMP);
                //formData.append('file', $('#fileInput')[0].files[0]);
                //console.log(formData);
            }

            var formData = new FormData($("#formGuardarSolicitud")[0]);

            if (areasSeleccionadas) {

                $('#btnGuardarSolicitud').css('display', 'none');
                $('#btnProcesandoGuardarSolicitud').css('display', 'block');
                //console.log("hiddenIdSolicitud " + $('#hiddenIdSolicitud').val());
                //console.log("hiddenIdSolPadre " + $('#hiddenIdSolPadre').val());
                //console.log("hiddenCodPerfilSolicitud " + $('#hiddenCodPerfilSolicitud').val());
                //console.log("hiddenCodEstadoSolicitud " + $('#hiddenCodEstadoSolicitud').val());
                //console.log("hiddenCodTipoSolicitud " + $('#hiddenCodTipoSolicitud').val());
                //console.log("txtComentariosAceptarSolicitud " + $('#txtComentariosAceptarSolicitud').val());
                //console.log("cmbAccionesSolicitud " + $('#cmbAccionesSolicitud').val());
                //console.log("lblRutProveedor " + $('#lblRutProveedor').text());

                $.ajax({
                    url: $("#formGuardarSolicitud").attr("action"),
                    type: "POST",
                    data: formData,
                    processData: false, // Important: tell jQuery not to process the data
                    contentType: false, // Important: tell jQuery not to set contentType
                    success: function (result) {

                        if (result.success) {
                            // --- LÓGICA DE ÉXITO ---

                            // 1. Construimos el mensaje de confirmación para el usuario.
                            var mensajeConfirmacion = result.message; // Usamos el mensaje del servidor.

                            // Si la solicitud fue eliminada, el mensaje ya es claro.
                            // Si no, añadimos la instrucción de que el modal se cerrará.
                            if (result.message !== 'SOLICITUD ELIMINADA') {
                                mensajeConfirmacion += ". El detalle se cerrará para actualizar los datos.";
                            }

                            // 2. Mostramos el alert. El código se detiene aquí hasta que el usuario hace clic en "Aceptar".
                            alert(mensajeConfirmacion);

                            // 3. Una vez que el usuario acepta, procedemos a cerrar y actualizar.
                            $('#modalSolicitud').modal('hide'); // Cierra el modal de forma segura.
                            ActualizarTablaSolicitudes(); // Actualiza el grid principal.

                        } else {
                            // --- LÓGICA DE ERROR (devuelto por el servidor) ---
                            $("#lblMsgGuardarSolicitud").text(result.message);
                            $("#alertMsgGuardarSolicitud").show();

                            // Como hubo un error, volvemos a habilitar el botón.
                            $('#btnGuardarSolicitud').prop('disabled', false);
                        }
                    },
                    error: function (error) {
                        $("#lblMsgGuardarSolicitud").text("Error de comunicación con el servidor. Intente nuevamente.");
                        $("#alertMsgGuardarSolicitud").show();

                        // También habilitamos el botón aquí.
                        $('#btnGuardarSolicitud').prop('disabled', false);
                        console.log(error);
                    },
                    complete: function () {
                        // --- SIEMPRE SE EJECUTA ---

                        // Ocultamos el spinner.
                        $('#btnProcesandoGuardarSolicitud').hide();

                        // Nos aseguramos de que el botón principal esté visible si el modal NO se cerró (es decir, si hubo un error).
                        if ($('#modalSolicitud').is(':visible')) {
                            $('#btnGuardarSolicitud').show();
                        }
                    }
                });
            } else {
                alert('Debe asignar todos los documentos a un área de análisis específica');
            }


        }

    }

    // #endregion

    // #region FUNCIONES DE INICIALIZACIÓN Y UI DEL MODAL

    function IniciarModal() {
        CardsDisplay('none');
        IniciarCollapseCards();
        ControlesModalSolicitud(false); // Pasamos false para que oculte el mensaje de alerta

        // Limpieza de tablas
        LimpiarTablaDocumentosAdjuntos();
        LimpiarTablaNotasSolicitud();
        LimpiarTablaDocumentoResolucion();
        LimpiarTablaObservacionesSolicitud();
        LimpiarTablaSolicitudesRelacionadas();

        var codPerfil = $("#hiddenCodPerfil").val();
        $('#hiddenCodPerfilSolicitud').val(codPerfil);

        // Limpieza de campos de cabecera (opcional pero recomendado)
        $("#lblRazonSocialProveedor, #lblRutProveedor, #lblEstadoProveedor, #lblEstadoSolicitud, #lblTipoRegistroSolicitud, #lblTipoSolicitud").text('');
        $("#txtFechaIngreso, #txtComentarios").val('');
    }

    function IniciarCollapseCards() {
        $('#collapseComentarios').collapse('hide');
        $('#collapseDocumentosAdjuntos').collapse('hide');
        $('#collapseLog').collapse('hide');
        $('#collapseObservacionesSolicitud').collapse('hide');
        $('#collapseDocumentoResolucion').collapse('hide');
        $('#collapseSolicitudesRelacionadas').collapse('hide');
    }

    function ControlesModalSolicitud(refresh) {
        if (!refresh) {
            $("#alertMsgGuardarSolicitud").css('display', 'none');
        }
        $("#txtComentariosAceptarSolicitud").val('');
        $('#txtComentariosAceptarSolicitud').css('display', 'none');
        $("#fileSubirDocResolucion").val('');
        $('#fileSubirDocResolucion').css('display', 'none');

        $('#btnGuardarSolicitud').css('display', 'block');
    }

    function TituloModal(titulo) {
        $("#lblTitle").text(titulo);
    }

    function CardsDisplay(param) {
        $("#cardDocumentosAdjuntos").css('display', param);
        $("#cardLogSolicitud").css('display', param);
        $("#cardResolucionSolicitud").css('display', param);
        $("#cardObservacionesSolicitud").css('display', param);
    }

    function CargarAccionesSolicitud(estado, codPerfil) {
        var estadoSolicitud = estado;
        console.log('estadoSolicitud ' + estadoSolicitud);
        console.log('codPerfil ' + codPerfil);
        //////////////////////////////////////////////////////////////////
        //SIMULACIÓN DE PERFIL PARA OPCIONES
        //codPerfil = 'AREG';
        /////////////////////////////////////////////////////////////////
        //$("#hiddenCodPerfilSolicitud").val(codPerfil);

        $('#cmbAccionesSolicitud').empty();
        $("#cmbAccionesSolicitud").append($("<option value='000' selected > (Seleccionar...) </option>"));

        var buttonText = '';

        $("#alertInformacionAcciones").css('display', 'none');
        $("#lblInformacionAcciones").text('');

        var userRegionPermiso = $("#hiddenUserRegion").val();
        var idRegion = $("#hiddenCodRegion").val();
        //console.log('userRegionPermiso ' + userRegionPermiso);
        //console.log('codPerfil ' + codPerfil);
        //console.log('idRegion ' + idRegion);

        var cantLegal = $("#hiddenCantLegalPerfil").val();
        var cantTecnica = $("#hiddenCantTecnicaPerfil").val();
        var cantContable = $("#hiddenCantContablePerfil").val();

        if (userRegionPermiso == 0 || userRegionPermiso == idRegion) {
            //PUEDE EDITAR SOBRE TODAS LAS REGIONES
            switch (estadoSolicitud) {
                case 'ABT': //Abierta
                    buttonText = 'Enviar';
                    switch (codPerfil) {
                        case 'ADM':
                            $('#divAccionesSolicitud').css('display', 'block');
                            //$("#cmbAccionesSolicitud").append($("<option></option>").val('VEA').html('Volver solicitud al estado anterior'));
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('EAN').html('Enviar a Análisis'));
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('SCO').html('Solicitar Correcciones'));
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('ELI').html('Eliminar Solicitud'));
                            buttonText = 'Aceptar';
                            break;
                        case 'JREG':
                            $('#divAccionesSolicitud').css('display', 'block');
                            //$("#cmbAccionesSolicitud").append($("<option></option>").val('VEA').html('Volver solicitud al estado anterior'));
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('ELI').html('Eliminar Solicitud'));
                            buttonText = 'Eliminar Solicitud';
                            break;
                        //case 'ADM':
                        case 'AREG':
                            $('#divAccionesSolicitud').css('display', 'block');
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('EAN').html('Enviar a Análisis'));
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('SCO').html('Solicitar Correcciones'));
                            break;
                        //case 'ALEG', 'ACON', 'JREG':
                        case 'ALEG':
                        case 'ACON':
                            $('#divAccionesSolicitud').css('display', 'none');
                            //$('#cmbAccionesSolicitud').empty();
                            break;
                    }

                    break;
                case 'EAN': //Enviada a Análisis            
                    switch (codPerfil) {
                        //case 'ADM', 'JREG':
                        case 'ADM':
                        case 'JREG':
                            $('#divAccionesSolicitud').css('display', 'block');

                            $("#cmbAccionesSolicitud").append($("<option></option>").val('VEA').html('Volver solicitud al estado anterior'));
                            //$("#cmbAccionesSolicitud").append($("<option></option>").val('CAA').html('Cambiar área de análisis'));
                            //$("#cmbAccionesSolicitud").append($("<option></option>").val('ERAA').html('Eliminar resolución del área de análisis'));
                            buttonText = 'Enviar';

                            //PARA ELIMINAR DOCUMENTOS DE RESOLUCIÓN, EN ESTADO EAN, SE MUESTRAN EN EL LIST LOS DOCUMENTOS DISPONIBLES

                            //$('#cmbEliminarDocumentosResolucion').empty();
                            //$("#cmbEliminarDocumentosResolucion").append($("<option value='000' selected > (Seleccionar...) </option>"));

                            //if (cantContable > 0) {
                            //    $("#cmbEliminarDocumentosResolucion").append($("<option></option>").val('C').html('Documento Area Contable'));
                            //}
                            //if (cantLegal > 0) {
                            //    $("#cmbEliminarDocumentosResolucion").append($("<option></option>").val('L').html('Documento Area Legal'));
                            //}
                            //if (cantTecnica > 0) {
                            //    $("#cmbEliminarDocumentosResolucion").append($("<option></option>").val('T').html('Documento Area Técnica'));
                            //}





                            break;
                        case 'ALEG':
                        case 'ACON':
                        case 'ATEC':
                        case 'AREG': //Exclusivamente, el Analista de Registro AREG puede aceptar una solicitud ya que hace la función de Analista Tecnico
                            //console.log('cantContable ' + cantContable);
                            //console.log('cantLegal ' + cantLegal);
                            //console.log('cantTecnica ' + cantTecnica);                   

                            if (cantContable > 0 && codPerfil == 'ACON') {
                                $('#divAccionesSolicitud').css('display', 'block');
                                $("#cmbAccionesSolicitud").append($("<option></option>").val('ACP').html('Aceptar')); //RESPONDER SOLICITUD ---- ACEPTAR ANÁLISIS
                                //$('#cmbAccionesSolicitud').empty();
                                buttonText = 'Responder';
                            }
                            else if (cantLegal > 0 && codPerfil == 'ALEG') {
                                $('#divAccionesSolicitud').css('display', 'block');
                                $("#cmbAccionesSolicitud").append($("<option></option>").val('ACP').html('Aceptar')); //RESPONDER SOLICITUD ---- ACEPTAR ANÁLISIS
                                //$('#cmbAccionesSolicitud').empty();
                                buttonText = 'Responder';
                            }
                            else if (cantTecnica > 0 && (codPerfil == 'ATEC' || codPerfil == 'AREG')) {
                                $('#divAccionesSolicitud').css('display', 'block');
                                $("#cmbAccionesSolicitud").append($("<option></option>").val('ACP').html('Aceptar')); //RESPONDER SOLICITUD ---- ACEPTAR ANÁLISIS
                                //$('#cmbAccionesSolicitud').empty();
                                buttonText = 'Responder';
                            } else {
                                $('#divAccionesSolicitud').css('display', 'none');
                                $("#alertInformacionAcciones").css('display', 'block');
                                $("#lblInformacionAcciones").text('Su perfil no tiene asignado ningún documento para Responder');
                            }



                            break;
                    }

                    break;
                case 'CAL': //Solicitud con Alcances //EN ESTE ESTADO, ESTÁ EN MANOS DEL PROVEEDOR 
                    switch (codPerfil) {
                        //case 'AREG', 'ADM':
                        //case 'ADM', 'JREG':
                        case 'ADM':
                        case 'JREG':
                            $('#divAccionesSolicitud').css('display', 'block');
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('VEA').html('Volver solicitud al estado anterior'));
                            buttonText = 'Enviar';
                            break;
                        default:
                            $('#divAccionesSolicitud').css('display', 'none');
                            break;
                    }
                    break;
                case 'RAN': //Respuesta del Análisis            
                    switch (codPerfil) {
                        //case 'AREG', 'ADM':
                        //case 'ADM', 'JREG':
                        case 'ADM':
                        case 'JREG':
                            $('#divAccionesSolicitud').css('display', 'block');
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('ACE').html('Aceptar Solicitud'));
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('CAL').html('Solicitud con Alcances'));
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('VEA').html('Volver solicitud al estado anterior'));
                            buttonText = 'Enviar';
                            break;
                        case 'AREG':
                            //case 'ATEC':
                            $('#divAccionesSolicitud').css('display', 'block');
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('ACE').html('Aceptar Solicitud'));
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('CAL').html('Solicitud con Alcances'));
                            buttonText = 'Enviar';
                            break;
                        default:
                            $('#divAccionesSolicitud').css('display', 'none');
                            break;
                    }
                    break;
                case 'SCO': //Solicitan Correcciones
                    switch (codPerfil) {
                        //case 'AREG', 'ADM':
                        //case 'ADM', 'JREG':
                        case 'ADM':
                        case 'JREG':
                            $('#divAccionesSolicitud').css('display', 'block');
                            //$("#cmbAccionesSolicitud").append($("<option></option>").val('VEA').html('Volver solicitud al estado anterior'));
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('ELI').html('Eliminar Solicitud'));
                            buttonText = 'Eliminar Solicitud';
                            break;
                        default:
                            $('#divAccionesSolicitud').css('display', 'none');
                            break;
                    }
                    break;
                case 'ACE': //Aceptada
                    switch (codPerfil) {
                        //case 'AREG', 'ADM':
                        //case 'ADM', 'JREG':
                        case 'ADM':
                        case 'JREG':
                            $('#divAccionesSolicitud').css('display', 'block');
                            $("#cmbAccionesSolicitud").append($("<option></option>").val('VEA').html('Volver solicitud al estado anterior'));
                            //$("#cmbAccionesSolicitud").append($("<option></option>").val('ACE').html('Eliminar Solicitud')); 
                            buttonText = 'Enviar';
                            break;
                        default:
                            $('#divAccionesSolicitud').css('display', 'none');
                            break;
                    }
                    break;
                
            }

        } else {
            //SÓLO PUEDE EDITAR CUANDO LA REGION DEL FUNCIONARIO ES LA MISMA DE LA SOLICITUD

            //if (userRegionPermiso == idRegion) {

            //} else {

            //}
            $('#divAccionesSolicitud').css('display', 'none');
            $("#alertInformacionAcciones").css('display', 'block');
            $("#lblInformacionAcciones").text('Este perfil sólo puede revisar esta Solicitud, no puede ejercer acciones');
        }



        //$("#btnGuardarSolicitud").prop("value", buttonText);
        //console.log('buttonText ' + buttonText);
        $("#btnGuardarSolicitud").html(buttonText);




    }

    function CargarAccionesSolicitudDañada(codPerfil, estadoSolicitud) {
        $("#alertInformacionAcciones").css('display', 'block');
        $("#lblInformacionAcciones").text('Solicitud dañada sin archivos adjuntos, sólo se puede eliminar');
        var buttonText = '---';
        switch (codPerfil) {
            case 'JREG':
            case 'ADM':
                $('#cmbAccionesSolicitud').empty();
                $("#cmbAccionesSolicitud").append($("<option value='000' selected > (Seleccionar...) </option>"));
                $("#cmbAccionesSolicitud").append($("<option></option>").val('ELI').html('Eliminar Solicitud'));
                buttonText = 'Aceptar';
                break;
        }

        $("#btnGuardarSolicitud").html(buttonText);
    }

    function SeleccionarAccionSolicitud(accion) {
        switch (accion) {
            case '000':
                $('#txtComentariosAceptarSolicitud').css('display', 'none');
                $('#fileSubirDocResolucion').css('display', 'none');
                break;
            case 'ABT':
                $('#txtComentariosAceptarSolicitud').css('display', 'none');
                $('#fileSubirDocResolucion').css('display', 'none');
                break;
            case 'ACP':
            case 'ACE':
            case 'CAL':
                $('#txtComentariosAceptarSolicitud').css('display', 'block');
                $('#fileSubirDocResolucion').css('display', 'block');
                break;
            default:
                $('#txtComentariosAceptarSolicitud').css('display', 'block');
                $('#fileSubirDocResolucion').css('display', 'none');
                break;
        }

    }

    // #endregion

    // #region FUNCIONES DE RENDERIZADO DE TABLAS Y CONTENIDO

    function CargarDocumentosAdjuntosEnTabla(datos, estadoSolicitud) {
        var rutProveedor = $("#lblRutProveedor").text();
        LimpiarTablaDocumentosAdjuntos();
        if (!datos || datos.length === 0) {
            var codPerfil = $('#hiddenCodPerfilSolicitud').val();
            CargarAccionesSolicitudDañada(codPerfil, estadoSolicitud);
        } else {
            $("#cardDocumentosAdjuntos").css('display', 'block');
            // ... (Tu lógica $.each para construir la tabla se mantiene aquí)
            // IMPORTANTE: Esta función llama a CargarAccionesSolicitud al final, eso está bien.
            $('#tablaDocumentosAdjuntos').append('<tr> ' +
                //'<th style="display:none;"> Id Det </th> ' +
                //'<th style="visibility:hidden"> Id Det </th> ' +
                '<th> N° </th> ' +
                '<th> Nombre Documento</th> ' +
                //'       <th> Observación </th>   ' +
                '       <th> Área de Análisis </th>   ' +
                '       <th> Estado </th>   ' +
                '       <th> Bajar </th>   ' +
                '       </tr> ');

            //console.log('estadoSolicitud '+ estadoSolicitud);
            var cantTec = 0; var cantLeg = 0; var cantCon = 0;
            var cantTecPerfil = 0; var cantLegPerfil = 0; var cantConPerfil = 0;
            //datos.DisponibleEnGesintel = 1;
            switch (estadoSolicitud) {
                case 'ABT':
                    $.each(datos, function (i, datos) {

                        $('#tablaDocumentosAdjuntos').append('<tr id="rowDocAdj' + i + '"> ' +
                            '<td> <input type="hidden" id="hiddenIdDet' + i + '"  value="' + datos.idDet + '" /> ' + datos.NumArchivo + '</td> ' +
                            '<td> ' + datos.NomDoc + '</td> ' +
                            '<td>  <select id="cmbAreaAnalisisDocumento' + i + '" name="cmbAreaAnalisisDocumento' + i + ' " class="form-select" value=""> ' +
                            '               <option value="0" selected>(Seleccionar...)</option>  ' +
                            '               <option value="CON">Contable</option>                   ' +
                            '               <option value="LEG">Legal</option>                      ' +
                            '               <option value="TEC">Técnica</option>                    ' +
                            '           </select>  </td>' +
                            /*' <td> ' + datos.Estado + '  </td>   ' +*/
                            ' <td> ---Sin estado---  </td>   ' +
                            '<td> <input type="hidden" id="hiddenIdArchivoGesintel' + i + '" value="' + datos.IdArchivoGesintel + '" />  ' +
                            ' <input type="hidden" id="hiddenIndex' + i + '" value="' + i + '" />  ' +
                            '   <a href = "/Solicitudes/BajarArchivoGesintel?idArchivoGesintel=' + datos.IdArchivoGesintel + '&rutProveedor=' + rutProveedor + '"  id="btnBajarArchivo' + i + '" class= "btn btn-primary download-link"> <i class="bi bi-download"></i></a>  ' +
                            ' <button id="btnProcesandoBajarArchivo' + i + '" class="btn btn-primary" type="button" style="display:none;" title="Bajar" disabled> ' +
                            ' <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ' +
                            ' </button>' +
                            ' </td>' +
                            ' </tr> '
                        );

                    });
                    break;
                default:
                    $.each(datos, function (i, datos) {
                        $('#tablaDocumentosAdjuntos').append('<tr id="rowDocAdj' + i + '"> ' +
                            '<td> <input type="hidden" id="hiddenIdDet' + i + '"  value="' + datos.idDet + '" /> ' + datos.NumArchivo + '</td> ' +
                            '<td> ' + datos.NomDoc + '</td> ' +
                            '<td> <input type="hidden" id="hiddenCodAreaAnalisis' + i + '"  value="' + datos.CodAreaAnalisis + '" /> ' + datos.AreaAnalisis + '  </td>' +
                            ' <td> ' + datos.Estado + '  </td>   ' +
                            '<td> <input type="hidden" id="hiddenIdArchivoGesintel' + i + '" value="' + datos.IdArchivoGesintel + '" /> ' +
                            //' <a href="/Solicitudes/BajarArchivoGesintel?idArchivoGesintel=' + datos.IdArchivoGesintel + '&rutProveedor=' + rutProveedor + '"  class= "btn btn-primary download-link"> <i class="bi bi-download"></i></a></td> ' +
                            ' <input type="hidden" id="hiddenIndex' + i + '" value="' + i + '" />  ' +
                            ' <a href="/Solicitudes/BajarArchivoGesintel?idArchivoGesintel=' + datos.IdArchivoGesintel + '&rutProveedor=' + rutProveedor + '" id="btnBajarArchivo' + i + '"  class="btn btn-primary download-link"> <i class="bi bi-download"></i></a>' +
                            ' <button id="btnProcesandoBajarArchivo' + i + '" class="btn btn-primary" type="button" style="display:none;" title="Bajar" disabled> ' +
                            ' <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ' +
                            ' </button>' +
                            ' </td> ' +
                            ' </tr> '
                        );

                        //console.log('CodEstado en iteración ' + datos.CodEstado);
                        //console.log('CodAreaAnalisis en iteración ' + datos.CodAreaAnalisis);
                        if (datos.CodEstado != 'RES') {
                            //Sólo si el estado del archivo es distinto a RES, osea que no está respondido por el área de análisis asignada
                            //Valores para manejar las acciones de un Analista sobre una solicitud en estado EAN, para aceptar análisis sólo si tiene archivos asignados a su propia área
                            switch (datos.CodAreaAnalisis) {
                                case 'LEG':
                                    cantLegPerfil++;
                                    break;
                                case 'CON':
                                    cantConPerfil++;
                                    break;
                                case 'TEC':
                                    cantTecPerfil++;
                                    break;
                            }
                        }

                        //console.log('cantTecPerfil ' + cantTecPerfil);

                        switch (datos.CodAreaAnalisis) {
                            case 'LEG':
                                cantLeg++;
                                break;
                            case 'CON':
                                cantCon++;
                                break;
                            case 'TEC':
                                cantTec++;
                                break;
                        }

                    });
                    break;
            }

            $("#hiddenCantLegalPerfil").val(cantLegPerfil);
            $("#hiddenCantTecnicaPerfil").val(cantTecPerfil);
            $("#hiddenCantContablePerfil").val(cantConPerfil);

            CargarListaDescargaMasivaDocumentos(estadoSolicitud, cantLeg, cantCon, cantTec);
            var codEstado = $("#hiddenCodEstadoSolicitud").val();
            var codPerfil = $('#hiddenCodPerfilSolicitud').val();
            CargarAccionesSolicitud(codEstado, codPerfil);
        }
    }

    function CargarLogSolicitudEnTabla(datos) {
        LimpiarTablaNotasSolicitud();
        if (!datos || datos.length === 0) {
            $("#cardLogSolicitud").css('display', 'none');
        } else {
            $("#cardLogSolicitud").css('display', 'block');

            $('#tablaLogSolicitud').append('<tr>  <th>Estado</th>        ' +
                '       <th> Fecha </th>   ' +
                '       <th> Acción </th>   ' +
                '       <th> Observación </th>   ' +
                '       <th> Comentario Analista</th>   ' +
                '       <th> Cuenta usuario</th>   ' +
                '       </tr >  <br />');

            $.each(datos, function (i, datos) {
                $('#tablaLogSolicitud').append('<tr>    <td>' + datos.Estado + '</td>' +
                    '<td>' + DateTimeFormat(datos.Fecha) + '</td>' +
                    '<td>' + datos.Comentario_1 + '</td> ' +
                    '<td>' + datos.Comentario_2 + '</td> ' +
                    '<td>' + datos.Comentario_3 + '</td> ' +
                    '<td>' + datos.CuentaUsuario + '</td> ' +

                    '</tr > '
                );
            });

        }
    }

    function CargarDocumentoResolucionEnTabla(datos) {
        LimpiarTablaDocumentoResolucion();
        if (!datos || datos.length === 0) {
            $("#cardResolucionSolicitud").css('display', 'none');
        } else {
            $("#cardResolucionSolicitud").css('display', 'block');

            var rutProveedor = $("#lblRutProveedor").text();

            $('#tablaDocumentoResolucion').append('<tr>  <th>Fecha Resolución</th>        ' +
                '       <th> Área </th>   ' +
                '       <th> Comentarios </th>   ' +
                '       <th> Documento </th>   ' +
                '       <th> Bajar </th>   ' +
                '       </tr >  <br />');

            $.each(datos, function (i, datos) {
                

                $('#tablaDocumentoResolucion').append('<tr>    <td>' + DateFormat(datos.FechaNota) + '</td>' +
                    '<td>' + datos.AreaAnalisis + '</td>' +
                    '<td>' + datos.Comentario + '</td>' +
                    '<td>' + datos.NomArchivo + '</td> ' +
                    '<td>  ' +
                    ' <input type="hidden" id="hiddenIdArchivoResolucionGesintel' + i + '" value="' + datos.IdArchivoGesintel + '" /> ' +
                    ' <input type="hidden" id="hiddenIndexResolucion' + i + '" value="' + i + '" />  ' +
                    ' <a href = "/Solicitudes/BajarArchivoGesintel?idArchivoGesintel=' + datos.IdArchivoGesintel + '&rutProveedor=' + rutProveedor + '" id="btnBajarArchivoResolucion' + i + '" class="btn btn-primary download-link" > <i class="bi bi-download"></i></a> ' +
                    ' <button id="btnProcesandoBajarArchivoResolucion' + i + '" class="btn btn-primary" type="button" style="display:none;" title="Bajar" disabled> ' +
                    ' <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ' +
                    ' </button>' +
                    ' </td > ' +
                    '</tr>  '
                );

                var cantResLeg = 0, cantResCon = 0, cantResTec = 0;
                if (datos.CodEstado == 'RES') {
                    //Sólo si el estado del archivo es distinto a RES, osea que no está respondido por el área de análisis asignada
                    //Valores para manejar las acciones de un Analista sobre una solicitud en estado EAN, para aceptar análisis sólo si tiene archivos asignados a su propia área
                    switch (datos.CodAreaAnalisis) {
                        case 'LEG':
                            cantResLeg++;
                            break;
                        case 'CON':
                            cantResCon++;
                            break;
                        case 'TEC':
                            cantResTec++;
                            break;
                    }
                }


                $("#hiddenCantLegalPerfil").val(cantResLeg);
                $("#hiddenCantTecnicaPerfil").val(cantResCon);
                $("#hiddenCantContablePerfil").val(cantResTec);


            });

        }
    }

    function CargarObservacionesEnTabla(datos) {
        LimpiarTablaObservacionesSolicitud();
        if (!datos || datos.length === 0) {
            $("#cardObservacionesSolicitud").css('display', 'none');
        } else {
            $("#cardObservacionesSolicitud").css('display', 'block');

            $('#tablaObservacionesSolicitud').append('<tr>  <th>Área de Análisis </th>        ' +
                '       <th> Fecha </th>   ' +
                '       <th> Observación </th>   ' +
                '       </tr >  <br />');

            $.each(datos, function (i, datos) {

                $('#tablaObservacionesSolicitud').append('<tr>    <td>' + datos.AreaAnalisis + '</td>' +
                    '<td>' + DateFormat(datos.FechaNota) + '</td>' +
                    '<td>' + datos.Comentario + '</td> ' +
                    '</tr>  '
                );

            });
        }
    }

    function CargarSolicitudesRelacionadasEnTabla(datos, numSolicitudConsulta) {
        LimpiarTablaSolicitudesRelacionadas();
        if (!datos || datos.length === 0) {
            $("#cardSolicitudesRelacionadas").css('display', 'none');
        } else {
            $("#cardSolicitudesRelacionadas").css('display', 'block');

            $('#tablaSolicitudesRelacionadas').append('<tr>  <th>   N° Solicitud  </th>        ' +
                '       <th> Fecha  </th>   ' +
                '       <th> Tipo </th>   ' +
                '       <th> Ver </th>   ' +
                '       </tr >  <br />');

            $.each(datos, function (i, datos) {
                //NO MUESTRA LA MISMA SOLICITUD Q SE ESTÁ CONSULTANDO
                if (datos.ID != numSolicitudConsulta) {
                    $('#tablaSolicitudesRelacionadas').append('<tr>    <td>' + datos.ID + '</td>' +
                        '<td>' + DateTimeFormat(datos.FechaCreacion) + '</td>' +
                        '<td>' + datos.TipoSolRel + '</td>  ' +
                        //'<td> <a href="#modalSolicitud" onclick="CargarSolicitudVista('+ datos.ID+');" class="btn btn-primary"> <i class="bi bi-search"></i></a></td> ' +
                        /*'<td> <a type="button" onclick="CargarSolicitudVista(' + datos.ID + ');" class="btn btn-primary"> <i class="bi bi-search"></i></a></td> ' +*/
                        '<td> <a id="btnVerSolicitudRelacionada_' + datos.ID + '" type="button"  class="btnVerSolicitudRelacionada btn btn-primary"> <i class="bi bi-search"></i></a></td> ' +

                        '</tr > '
                    );

                    $('.btnVerSolicitudRelacionada').click(function () {
                        var obj = $(this);
                        var id = obj.prop("id").split("_");
                        //CargarSolicitudVista(id[1]);
                        //CargarDatosSolicitud(numSolicitud, refresh);
                        var refresh = false;
                        CargarDatosSolicitud(id[1], refresh);
                    });

                }

            });
        }
    }

    function LimpiarTablaDocumentosAdjuntos() {
        $('#tablaDocumentosAdjuntos').html('');

    }

    function LimpiarTablaNotasSolicitud() {
        $('#tablaLogSolicitud').html('');
    }

    function LimpiarTablaDocumentoResolucion() {
        $('#tablaDocumentoResolucion').html('');
    }

    function LimpiarTablaObservacionesSolicitud() {
        $('#tablaObservacionesSolicitud').html('');
    }

    function LimpiarTablaSolicitudesRelacionadas() {
        $('#tablaSolicitudesRelacionadas').html('');
    }

    // #endregion

    // #region MANEJO DE DESCARGAS DE ARCHIVOS

    function handleDownloadLinkClick(event) {
        // Prevent the default behavior (i.e., navigating to the file download URL)
        event.preventDefault();

        var index = $(this).closest('tr').find('input[type="hidden"][id^="hiddenIndex"]').val();
        $('#btnBajarArchivo' + index).css('display', 'none');
        $('#btnProcesandoBajarArchivo' + index).css('display', 'block');

        //var idArchivoGesintel = $(this).closest('tr').find('input[type="hidden"]').val();
        var idArchivoGesintel = $(this).closest('tr').find('input[type="hidden"][id^="hiddenIdArchivoGesintel"]').val();

        var rutProveedor = $("#lblRutProveedor").text();
        //console.log('idArchivoGesintel ' + idArchivoGesintel);
        //console.log('rutProveedor ' + rutProveedor);

        $.get("/Solicitudes/BajarArchivoGesintel", { idArchivoGesintel: idArchivoGesintel, rutProveedor: rutProveedor })
            .done(function (data) {
                if (data.success === undefined) {

                    window.location.href = "/Solicitudes/BajarArchivoGesintel?idArchivoGesintel=" + idArchivoGesintel + "&rutProveedor=" + rutProveedor;
                } else {
                    alert(data.message);
                }

                $('#btnBajarArchivo' + index).css('display', 'initial');//se usa initial ya que block agranda el btn a todo el cuadro del td
                $('#btnProcesandoBajarArchivo' + index).css('display', 'none');

            }).fail(function () {
                // Handle AJAX error
                alert("Error al intentar descargar archivo: Intente más tarde");
            });


    }

    function handleDownloadResolucionLinkClick(event) {
        // Prevent the default behavior (i.e., navigating to the file download URL)
        event.preventDefault();
        var index = $(this).closest('tr').find('input[type="hidden"][id^="hiddenIndexResolucion"]').val();
        $('#btnBajarArchivoResolucion' + index).css('display', 'none');
        $('#btnProcesandoBajarArchivoResolucion' + index).css('display', 'block');

        //var idArchivoGesintel = $(this).closest('tr').find('input[type="hidden"]').val();
        var idArchivoGesintel = $(this).closest('tr').find('input[type="hidden"][id^="hiddenIdArchivoResolucionGesintel"]').val();

        var rutProveedor = $("#lblRutProveedor").text();

        console.log('idArchivoGesintel ' + idArchivoGesintel);
        console.log('rutProveedor ' + rutProveedor);

        $.get("/Solicitudes/BajarArchivoGesintel", { idArchivoGesintel: idArchivoGesintel, rutProveedor: rutProveedor })
            .done(function (data) {
                if (data.success === undefined) {

                    window.location.href = "/Solicitudes/BajarArchivoGesintel?idArchivoGesintel=" + idArchivoGesintel + "&rutProveedor=" + rutProveedor;
                } else {
                    alert(data.message);
                }


            }).fail(function () {
                // Handle AJAX error

                alert("Error al intentar descargar archivo: Intente más tarde");
            });

        $('#btnBajarArchivoResolucion' + index).css('display', 'initial');//se usa initial ya que block agranda el btn a todo el cuadro del td
        $('#btnProcesandoBajarArchivoResolucion' + index).css('display', 'none');




    }

    function DescargaMasivaDocumentos_______old() {
        var tipoDescarga = $('#cmbDescargaMasivaDocumentos').val();
        var nombreTipoDescarga = $('#cmbDescargaMasivaDocumentos option:selected').html();
        console.log('tipoDescarga ' + tipoDescarga);
        //TOD, CON, LEG, TEC
        var listDocTEMP = []; // Array to hold item data objects
        var numSolicitud = $('#hiddenIdSolicitud').val();

        var rutProveedor = $("#lblRutProveedor").text();

        if (tipoDescarga != 'SEL') {
            $('#btnDescargaMasivaDocumentos').css('display', 'none');
            $('#btnProcesandoDescargaMasivaDocs').css('display', 'block');
            switch (tipoDescarga) {
                case 'TOD':
                    $('#htmlTableDocAdj tbody tr').each(function () {
                        var rowId = $(this).attr('id'); // Get the ID of the table row
                        if (rowId != undefined) {
                            //Se extrae la parte numérica
                            rowId = rowId.replace(/^\D+/g, '');
                            //console.log('nombre control hidden: ' + '#hiddenIdDet' + rowId);
                            //var itemId = $('#hiddenIdDet' + rowId).val();
                            var idDet = $('#hiddenIdDet' + rowId).val();
                            var codAreaAnalisis = $('#hiddenCodAreaAnalisis' + rowId).val();
                            var idArchivoGesintel = $('#hiddenIdArchivoGesintel' + rowId).val();
                            listDocTEMP.push(idArchivoGesintel);

                        }
                    });
                    break;
                default:
                    $('#htmlTableDocAdj tbody tr').each(function () {
                        var rowId = $(this).attr('id'); // Get the ID of the table row
                        if (rowId != undefined) {
                            //Se extrae la parte numérica
                            rowId = rowId.replace(/^\D+/g, '');
                            //console.log('nombre control hidden: ' + '#hiddenIdDet' + rowId);
                            var idDet = $('#hiddenIdDet' + rowId).val();
                            var codAreaAnalisis = $('#hiddenCodAreaAnalisis' + rowId).val();
                            var idArchivoGesintel = $('#hiddenIdArchivoGesintel' + rowId).val();
                            if (codAreaAnalisis == tipoDescarga) {
                                console.log('OK');
                                listDocTEMP.push(idArchivoGesintel);
                            }

                        }
                    });
                    break;
            }
            var strList = JSON.stringify(listDocTEMP);
            $('#hiddenListDocsDescargaMasiva').val(strList);
            console.log(strList);

            var jsonData = {
                jsonArchivosGesintel: strList,
                rutProveedor: rutProveedor,
                numSolicitud: numSolicitud,
                nombreTipoDescarga: nombreTipoDescarga
            };

            // Create a form element
            var form = document.createElement('form');
            form.method = 'POST';
            form.action = '/Solicitudes/DescargaMasivaArchivosGesintel'; // URL of the download action
            // Create hidden input fields for each data parameter
            Object.keys(jsonData).forEach(function (key) {
                var input = document.createElement('input');
                input.type = 'hidden';
                input.name = key;
                input.value = jsonData[key];
                form.appendChild(input);
            });
            // Append the form to the document body
            document.body.appendChild(form);
            // Submit the form to initiate the download
            form.submit();
            // Remove the form from the document body after submission
            document.body.removeChild(form);


            $('#btnDescargaMasivaDocumentos').css('display', 'block');
            $('#btnProcesandoDescargaMasivaDocs').css('display', 'none');
        } else {
            alert('Debe seleccionar un tipo de descarga');
        }

    }

    function DescargaMasivaDocumentos() {
        var tipoDescarga = $('#cmbDescargaMasivaDocumentos').val();

        if (tipoDescarga != 'SEL') {
            // 1. Ocultar y mostrar botones INMEDIATAMENTE.
            $('#btnDescargaMasivaDocumentos').css('display', 'none');
            $('#btnProcesandoDescargaMasivaDocs').css('display', 'block');

            // 2. Retrasar la sumisión del formulario ligeramente (100 milisegundos).
            // Esto le da al navegador tiempo suficiente para renderizar el cambio de botones.
            setTimeout(function () {
                var nombreTipoDescarga = $('#cmbDescargaMasivaDocumentos option:selected').html();
                var listDocTEMP = [];
                var numSolicitud = $('#hiddenIdSolicitud').val();
                var rutProveedor = $("#lblRutProveedor").text();

                var selectorFilas = (tipoDescarga === 'TOD')
                    ? '#htmlTableDocAdj tbody tr'
                    : '#htmlTableDocAdj tbody tr:has(input[value="' + tipoDescarga + '"])';

                $(selectorFilas).each(function () {
                    var rowId = $(this).attr('id');
                    if (rowId) {
                        var idArchivoGesintel = $('#hiddenIdArchivoGesintel' + rowId.replace(/^\D+/g, '')).val();
                        listDocTEMP.push(idArchivoGesintel);
                    }
                });

                var strList = JSON.stringify(listDocTEMP);

                var jsonData = {
                    jsonArchivosGesintel: strList,
                    rutProveedor: rutProveedor,
                    numSolicitud: numSolicitud,
                    nombreTipoDescarga: nombreTipoDescarga
                };

                // La lógica del formulario se mantiene igual, pero ahora dentro del timeout.
                var form = document.createElement('form');
                form.method = 'POST';
                form.action = '/Solicitudes/DescargaMasivaArchivosGesintel';

                Object.keys(jsonData).forEach(function (key) {
                    var input = document.createElement('input');
                    input.type = 'hidden';
                    input.name = key;
                    input.value = jsonData[key];
                    form.appendChild(input);
                });

                document.body.appendChild(form);
                form.submit();
                document.body.removeChild(form);

            }, 100); // 100ms de retraso es imperceptible para el usuario.

            // 3. Revertir los botones después de un tiempo prudencial (ej. 2 segundos).
            // Como no sabemos cuándo termina la descarga, esto es un reseteo para que el usuario
            // pueda realizar otra acción si lo necesita.
            setTimeout(function () {
                $('#btnDescargaMasivaDocumentos').css('display', 'block');
                $('#btnProcesandoDescargaMasivaDocs').css('display', 'none');
            }, 2000); // 2000ms = 2 segundos.

        } else {
            alert('Debe seleccionar un tipo de descarga');
        }
    }

    function CargarListaDescargaMasivaDocumentos(estadoSolicitud, cantLeg, cantCon, cantTec) {
        $('#cmbDescargaMasivaDocumentos').empty();
        $("#cmbDescargaMasivaDocumentos").append($("<option value='SEL' selected > (Seleccionar...) </option>"));

        switch (estadoSolicitud) {
            case 'ABT':
            case 'SCO':
                $("#cmbDescargaMasivaDocumentos").append($("<option></option>").val('TOD').html('Todos'));
                break;
            default:
                //OTROS ESTADOS
                $("#cmbDescargaMasivaDocumentos").append($("<option></option>").val('TOD').html('Todos'));
                if (cantLeg > 0) {
                    $("#cmbDescargaMasivaDocumentos").append($("<option></option>").val('LEG').html('Legal'));
                }
                if (cantCon > 0) {
                    $("#cmbDescargaMasivaDocumentos").append($("<option></option>").val('CON').html('Contable'));
                }
                if (cantTec > 0) {
                    $("#cmbDescargaMasivaDocumentos").append($("<option></option>").val('TEC').html('Técnica'));
                }
                break;
        }
    }

    // #endregion

    // #region FUNCIONES HELPERS Y UTILIDADES

    function ActualizarTablaSolicitudes() {
        $('#btnBuscarSolicitudes').click();
    }

    function DateFormat(value, row, index) {
        //moment.locale('es');
        if (value == null) {
            //value == '---';
            return '---';
        } else {
            return moment(value).format('DD/MM/YYYY');
        }
    }

    function DateTimeFormat(value, row, index) {
        //moment.locale('es');

        if (value == null) {
            //value == '---';
            return '---';
        } else {
            //return moment(value).format('DD/MM/YYYY hh:mm:ss');
            return moment(value).format('DD/MM/YYYY hh:mm:ss a');
        }
    }

    function showWaitingModal() {
        $('#waitingModal').modal('show');
    }

    function hideWaitingModal() {
        $('#waitingModal').modal('hide');
    }

    // #endregion

    // =========================================================================
    // 4. EXPOSICIÓN DE FUNCIONES GLOBALES (NECESARIAS PARA ANGULAR)
    // =========================================================================
    // Hacemos que CargarDatosSolicitud sea accesible desde fuera de este archivo.
    window.CargarDatosSolicitud = CargarDatosSolicitud;

})(jQuery); // Pasamos jQuery a la IIFE.