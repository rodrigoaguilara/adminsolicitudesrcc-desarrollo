// Se envuelve todo el código en una "función anónima autoejecutable" (IIFE).
// Esto crea un ámbito privado, evitando que tus variables y funciones contaminen el ámbito global.
// Es una práctica muy recomendada.
(function ($) {
    'use strict'; // Habilita el modo estricto de JS para detectar errores comunes.

    // =========================================================================
    // 1. CÓDIGO DE INICIALIZACIÓN (SE EJECUTA CUANDO EL DOM ESTÁ LISTO)
    // =========================================================================
    $(document).ready(function () {

        // Asignación de eventos globales
        $(document).ajaxError(function (e, xhr) {
            if (xhr.status == 401) window.location = "../Home/Login";
            else if (xhr.status == 403) alert("You have no enough permissions to request this resource.");
        });

        // Asignación de eventos específicos de esta página
        asignarEventos();

        // Carga inicial de datos
        IniciarForm();
        CargarFiltros();
        CargarDatosParaElModal();
    });

    // =========================================================================
    // 2. ASIGNACIÓN DE EVENTOS (ON CLICK, ON CHANGE, ETC.)
    // =========================================================================
    function asignarEventos() {
        $('#btnLimpiarFiltros').on('click', LimpiarFiltros);
        //$('#btnBuscarDocumentoResolucion').on('click', BuscarDocumentoResolucion);
        $('#btnGuardarRazonSocial').on('click', GuardarCambioRazonSocial);
        $('#btnGuardarCapacidadEconomica').on('click', GuardarNuevaCapacidadEconomica);
        $('#btnGuardarEstadoMOP').on('click', GuardarCambioEstadoMOP);
        $('#btnRenovarProveedorVigente').on('click', RenovarProveedor);
        $('#cmbEditarEstadoProveedor').on('change', SelectedEstadoProveedor);
        $('#cmbAreaSubirDocumentoProveedor').on('change', CargarTipoDocSubirDoc);

        // Eventos para los collapse
        $('#btnVerHistorialCapEconomica').on('click', function () { $('#collapseHistorialCapEconomica').collapse('toggle'); });
        $('#btnVerHistorialEstadoMOP').on('click', function () { $('#collapseHistorialEstadoMOP').collapse('toggle'); });
        $('#btnVerHistorialRazonSocial').on('click', function () { $('#collapseHistorialRazonSocial').collapse('toggle'); });
        $('#btnVerListadoEspecialidadesYCategorias').on('click', function () { $('#collapseListadoEspecialidadesYCategorias').collapse('toggle'); });
        $('#btnVerHistorialEspecialidadesYCategorias').on('click', function () { $('#collapseHistorialEspecialidadesYCategorias').collapse('toggle'); });
        $('#btnVerListadoDocumentosProveedor').on('click', function () { $('#collapseListadoDocumentosProveedor').collapse('toggle'); });
        $('#btnVerSubirDocumentosProveedor').on('click', function () { $('#collapseSubirDocumentosProveedor').collapse('toggle'); });

        // Eventos de tablas y otros
        $('#tablaDocumentosDelProveedor').on('click', 'a.download-link', function (event) {
            // Prevent the default behavior (i.e., navigating to the file download URL)
            event.preventDefault();

            var index = $(this).closest('tr').find('input[type="hidden"][id^="hiddenIndex"]').val();
            $('#btnBajarArchivo' + index).css('display', 'none');
            $('#btnProcesandoBajarArchivo' + index).css('display', 'block');

            var idArchivoGesintel = $(this).closest('tr').find('input[type="hidden"][id^="hiddenIdArchivoGesintel"]').val();



            //var rutProveedor = $("#lblRutProveedor").text();
            var rutProveedor = $('#txtBuscarRutProveedor').val();

            console.log('idArchivoGesintel ' + idArchivoGesintel);
            console.log('rutProveedor ' + rutProveedor);

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



        //SUBIDA DE DOCUMENTOS POR AJAX

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


        $('#txtEditarFechaInscripcion').on("change", function () {
            var $fechaInput = $(this);
            var fechaInscripcion = $fechaInput.val();
            var tipoRegistro = $("#hiddenCodTipoRegistro").val();

            // 1. Validación de la fecha
            if (!fechaInscripcion || !moment(fechaInscripcion, 'YYYY-MM-DD', true).isValid()) {
                alert("La fecha de inscripción no es válida. Por favor, use el formato AAAA-MM-DD.");
                // Opcional: limpiar los campos dependientes
                $("#txtFechaRenovacion").val('');
                // $("#cmbEditarMesRenovacion").val(''); // Si mantienes el dropdown de mes
                return; // Detiene la ejecución
            }

            // 2. Lógica para calcular y mostrar la nueva fecha de renovación (ver mejora #2)
            CalcularYMostrarFechaRenovacion(fechaInscripcion, tipoRegistro);
        });




        $('#cmbBuscarSolicitudConResolucion').on('change', function () {
            // Cuando el usuario selecciona una opción, llamamos directamente a la función de búsqueda.
            // No es necesario verificar si el valor es válido, ya que la función BuscarDocumentoResolucion ya lo hace.
            BuscarDocumentoResolucion();
        });

        // Usamos delegación de eventos en el contenedor de la tabla.
        // El evento 'change' se disparará para cualquier radio button con la clase 'permiso-categoria-check'
        // que esté dentro de '#especialidadesCategoriasContainer'.
        $('#especialidadesCategoriasContainer').on('change', '.permiso-categoria-check', function () {
            var $radioButton = $(this); // El radio button que fue clickeado

            // Obtenemos los datos desde el HTML principal y los atributos data-*
            var rutProveedor = $("#lblRutProveedor").text();
            var codTipoReg = $("#hiddenCodTipoRegistro").val();

            var idRegistroCategoria = $radioButton.data('id-registro');
            var codEspecialidad = $radioButton.data('cod-especialidad');
            var codCategoria = $radioButton.data('cod-categoria');
            var nombreEspecialidad = $radioButton.data('nombre-especialidad');

            // Llamamos a la función de guardado con todos los datos recolectados
            GuardarCategoriaEspecialidad(rutProveedor, codTipoReg, idRegistroCategoria, codEspecialidad, codCategoria, nombreEspecialidad);
        });

    }

    // =========================================================================
    // 3. DEFINICIONES DE FUNCIONES (Las "herramientas")
    // =========================================================================

    // #region INICIALIZACIÓN Y FILTROS

    function IniciarForm() {
        $('#alertMensajes').css('display', 'none');
        //$('#alertMensajesModalProveedor').css('display', 'none');
        $('#btnDownloadExcelProv').css('display', 'none');
    }

    function IniciarModal() {

        // Reseteo general de campos
        $("#lblRazonSocialProveedor, #lblRutProveedor, #lblTipoRegistroProveedor, #lblEmailProveedor").text('');
        $("#txtEditarRazonSocial, #hiddenRazonSocial, #txtEditarNombreFantasia, #hiddenNombreFantasia").val('');
        $("#txtEditarFechaInscripcion, #txtFechaRenovacion").val('');
        $("#cmbEditarEstadoProveedor, #cmbBuscarSolicitudConResolucion").val('');
        $("#hiddenNumSolicitudAsociada, #hiddenAnteriorEstadoProveedor, #hiddenNombreAnteriorEstadoProveedor").val('');
        $("#lblSolicitudAsociada, #lblNombreDocumentoResolucion").text('');
        $("#txtMotivoCambioEstadoMOP").val('');
        $("#txtEditarCapacidadEconomica, #txtEditarFechaEstadoFinanciero").val('');
        $('#divSinDatosCapacidadEconomica').hide();

        // Deshabilitar fecha de inscripción por defecto
        $('#txtEditarFechaInscripcion').prop('disabled', true).removeClass('is-invalid');

        // --- Lógica de UI para Estado MOP ---
        // Ocultar todos los controles de acción por defecto
        $('#divAdjuntarDocumentoResolucion').hide();
        $('#btnRenovarProveedorVigente').hide();
        $('#btnGuardarEstadoMOP').hide(); // El botón de guardar también se oculta al inicio



        var primeraPestaña = $('.nav-link[data-bs-target="#panelRazonSocial"]');

        if (primeraPestaña.length) {
            bootstrap.Tab.getOrCreateInstance(primeraPestaña[0]).show();
        }



        IniciarCollapseCards();

        IniciarDocumentosDelProveedor();


    }

    function CargarFiltros() {
        // 1. Definimos los selectores de nuestros dropdowns para fácil acceso.
        var $cmbTipoRegistro = $('#cmbBuscarTipoRegistro');
        var $cmbEstadoProveedor = $('#cmbBuscarEstadoProveedor');
        var $cmbRegion = $('#cmbBuscarRegion');

        // 2. Juntamos todos los dropdowns y los ponemos en estado "Cargando...".
        var dropdowns = $cmbTipoRegistro.add($cmbEstadoProveedor).add($cmbRegion);
        dropdowns.empty().append($('<option disabled selected>Cargando...</option>')).prop('disabled', true);

        // 3. Realizamos la llamada AJAX consolidada que ya tenías.
        $.ajax({
            type: 'POST',
            url: "/Proveedores/JsonTraerFiltrosPrincipales",
            dataType: "json",
            data: { __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (data) {
                // 4. En caso de éxito, poblamos los dropdowns usando la función auxiliar.
                // Es importante pasar el objeto jQuery ($cmbTipoRegistro) en lugar del string ('#cmbBuscarTipoRegistro')
                // para un rendimiento ligeramente mejor, aunque ambas formas funcionan.
                PoblarDropdown($cmbTipoRegistro, data.TiposRegistro, 'Codigo', 'Descripcion');
                PoblarDropdown($cmbEstadoProveedor, data.EstadosProveedor, 'Codigo', 'Descripcion');
                PoblarDropdown($cmbRegion, data.Regiones, 'CodRegion', 'NombreRegion');
            },
            error: function (jqXHR, textStatus, errorThrown) {
                // 5. En caso de error, mostramos un mensaje de error en los dropdowns.
                dropdowns.empty().append($('<option disabled selected>Error al cargar</option>')).prop('disabled', true);
                alert('Error al cargar los filtros de búsqueda.');
                console.error("Error en carga de filtros: ", textStatus, errorThrown);
            }
        });
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

    function LimpiarFiltros() {
        $("#txtBuscarRutProveedor").val('');
        $("#txtBuscarRazonSocial").val('');
        //$("#txtBuscarNumeroSolicitud").val('');

        $("#cmbBuscarEstadoProveedor option:selected").prop("selected", false);
        $("#cmbBuscarTipoRegistro option:selected").prop("selected", false);
        $("#cmbBuscarRegion option:selected").prop("selected", false);

        $('#alertMensajes').css('display', 'none');
    }

    function CargarDatosParaElModal() {
        CargarMesesRenovacion();
        CargarEstadosProveedor();
    }

    function CargarMesesRenovacion() {

        $.ajax({
            type: 'POST',
            url: "/Proveedores/JsonTraerMesesRenovacion",
            dataType: "json",
            data: { __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (datos) {
                if (datos === "EMPTY") {

                } else {
                    $('#cmbEditarMesRenovacion').empty();
                    $("#cmbEditarMesRenovacion").append($("<option value='' selected > (Seleccionar...) </option>"));
                    $(datos).each(function () {
                        $("#cmbEditarMesRenovacion").append($("<option></option>").val(this.IdMes).html(this.NombreMes));
                    });

                }
            },
            error: function (jqXHR, textStatus, errorThrown) { }
        });
    }

    function CargarEstadosProveedor() {
        var codigoPadre = 2246;//TipoVigenciaProveedorMOP
        $.ajax({
            type: 'POST',
            url: "/Solicitudes/JsonListarParametrosPorTipo",
            dataType: "json",
            data: { codigoPadre: codigoPadre, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (datos) {
                if (datos === "EMPTY") {

                } else {
                    $('#cmbEditarEstadoProveedor').empty();
                    $("#cmbEditarEstadoProveedor").append($("<option value='' selected > (Seleccionar...) </option>"));
                    $(datos).each(function () {
                        $("#cmbEditarEstadoProveedor").append($("<option></option>").val(this.CODIGO).html(this.DESCRIPCION));
                    });

                }
            },
            error: function (jqXHR, textStatus, errorThrown) { }
        });
    }

    function IniciarCollapseCards() {
        $('#collapseHistorialRazonSocial').collapse('hide');
        $('#collapseHistorialEstadoMOP').collapse('hide');
        $('#collapseHistorialCapEconomica').collapse('hide');
        $('#collapseListadoEspecialidadesYCategorias').collapse('hide');
        $('#collapseHistorialEspecialidadesYCategorias').collapse('hide');
        $('#collapseSubirDocumentosProveedor').collapse('hide');
        $('#collapseListadoDocumentosProveedor').collapse('hide');

    }


    // #endregion

    // #region LÓGICA PRINCIPAL DEL MODAL

    // FUNCIÓN LLAMADA DESDE GRIDPROVEEDORES.JS (VERSIÓN REFACTORIZADA)
    function CargarDatosProveedor(Proveedor) {
        // ---- 1. INICIALIZACIÓN  ----
        IniciarModal();


        var rutProveedor = Proveedor.Rut;
        var codTipoRegistro = Proveedor.CodTipoRegistro;
        var tipoRegistro = Proveedor.TipoRegistro;
        TituloModal('Detalle de Proveedor');

        //showWaitingModal();
        $('#loadingSpinnerProveedor').show();
        $('#contenidoProveedor').hide();
        $('#modalProveedor').modal('show');

        // ---- 3. ÚNICA LLAMADA AJAX ----
        $.ajax({
            type: 'POST',
            url: "/Proveedores/JsonTraerDetalleCompletoProveedor",
            dataType: "json",
            data: {
                rutProveedor: rutProveedor,
                codTipoRegistro: codTipoRegistro,
                __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
            },
            success: function (data) {
                if (data && data.DetallePrincipal) {

                    // ---- 4. POBLAR LA CABECERA DEL MODAL ----
                    $("#lblRazonSocialProveedor").text(data.DetallePrincipal.RazonSocial);
                    $("#lblRutProveedor").text(data.DetallePrincipal.Rut);
                    $("#hiddenRutProveedorSubirDoc").val(data.DetallePrincipal.Rut);
                    $("#lblTipoRegistroProveedor").text(tipoRegistro);
                    $("#lblEmailProveedor").text(data.DetallePrincipal.Email);
                    $("#hiddenCodTipoRegistro").val(codTipoRegistro);

                    ///////////////////////////////////////////////////////////////////
                    // CARGA DE DATOS EN TABS 
                    ///////////////////////////////////////////////////////////////////

                    // ---- RAZÓN SOCIAL ----
                    CargarDatosRazonSocial(data.DetallePrincipal.RazonSocial, data.DetallePrincipal.NombreFantasia);
                    bootstrapTableHistorialCambioRazonSocial(data.HistorialRazonSocial);

                    // ---- ESTADO MOP ----
                    if (data.EstadoMopActual) {
                        var v = data.EstadoMopActual;
                        var codEstadoActual = v.COD_ESTADO;
                        $("#lblEstadoVigenciaProveedor").text(v.ESTADO);
                        $("#cmbEditarMesRenovacion").val(v.MES_RENOVACION);
                        $("#cmbEditarEstadoProveedor").val(v.COD_ESTADO);
                        $("#hiddenAnteriorEstadoProveedor").val(v.COD_ESTADO);
                        $("#hiddenNombreAnteriorEstadoProveedor").val(v.ESTADO);
                        //$("#txtEditarFechaInscripcion").val(v.FECHA_INGRESO_REG);
                        $("#txtEditarFechaInscripcion").val(moment(v.FECHA_INGRESO_REG).format('YYYY-MM-DD'));
                        $('#cmbEditarMesRenovacion').prop('disabled', true);

                        if (v.FECHA_SUSPENSION_REG) {
                            $("#txtFechaRenovacion").val(moment(v.FECHA_SUSPENSION_REG).format('MMMM YYYY'));
                        } else {
                            $("#txtFechaRenovacion").val('');
                        }

                        if (v.NUM_SOLICITUD_ASOCIADA != 0 && v.COD_ESTADO == 'VIG') {
                            $("#lblSolicitudAsociada").text('Sol. Asociada N° ' + v.NUM_SOLICITUD_ASOCIADA);
                        }

                        $('#divAdjuntarDocumentoResolucion').hide();

                        if (v.COD_ESTADO == 'VIG') {
                            $('#btnRenovarProveedorVigente').show();
                        } else {
                            $('#btnRenovarProveedorVigente').hide();
                        }

                        




                        CargarSolicitudesParaCambioEstadoProveedor(codTipoRegistro);
                        
                    }
                    bootstrapTableHistorialCambioEstadoMOP(data.HistorialEstadoMop);

                    // ---- ESPECIALIDADES Y CATEGORÍAS ----
                    CargarDatosEspecialidadesCategoriasProveedor_Razor(data.EspecialidadesCompletas);
                    bootstrapTableHistorialEspecialidadesYCategorias(data.HistorialEspecialidades);



                    // ---- CAPACIDAD ECONÓMICA ----
                    if (codTipoRegistro == 'CNS') {
                        $("#tabCapacidadEconomica").hide();
                    } else {
                        $("#tabCapacidadEconomica").show();
                        if (data.CapacidadEconomicaActual) {
                            $("#txtEditarCapacidadEconomica").val(data.CapacidadEconomicaActual.CapacidadEconomica);
                            $('#txtEditarFechaEstadoFinanciero').val(moment(data.CapacidadEconomicaActual.FechaEstadoFinanciero).format('YYYY-MM-DD'));
                            $('#divSinDatosCapacidadEconomica').css('display', 'none');
                        } else {
                            $("#txtEditarCapacidadEconomica").val(0);
                            $('#divSinDatosCapacidadEconomica').css('display', 'block');
                        }
                        bootstrapTableHistorialCapacidadEconomica(data.HistorialCapacidadEconomica);
                    }

                    // ---- DOCUMENTOS DEL PROVEEDOR ----
                    bootstrapTableDocumentosDelProveedor(data.Documentos, rutProveedor);

                } else {
                    alert("No se encontró información para el proveedor seleccionado.");
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $('#loadingSpinnerProveedor').hide();
                alert('Ocurrió un error al cargar los datos del proveedor. Por favor, intente nuevamente.');
                console.error("Error en AJAX: ", textStatus, errorThrown);
            },
            complete: function () {
                $('#loadingSpinnerProveedor').hide(); // Ocultamos el spinner
                $('#contenidoProveedor').show();
            }
        });
    }

    // NOTA: Para que esta función sea visible desde tu Grid de Angular,
    // debemos exponerla al ámbito global (window). Haremos esto al final.

    function ActualizarTablaProveedores() {
        $('#btnBuscarProveedores').click();
    }

    function TituloModal(titulo) {
        //document.getElementById('lblTitle').innerHTML = titulo;
        $("#lblTitle").text(titulo);
    }


    // #region CAMBIO RAZON SOCIAL

    
        function CargarDatosRazonSocial(razonSocial, nombreFantasia) {
            $("#txtEditarRazonSocial").val(razonSocial);
            $("#hiddenRazonSocial").val(razonSocial);
            $("#txtEditarNombreFantasia").val(nombreFantasia);
            $("#hiddenNombreFantasia").val(nombreFantasia);

        }
        function CargarDatosHistorialCambioRazonSocial(rutProveedor) {
            var codTipoLog = 'CM_RS'; //CMB_RS: CAMBIO RAZÓN SOCIAL
            $.ajax({
                type: 'POST',
                url: "/Proveedores/JsonTraerMOPLog",
                dataType: "json",
                data: { rutProveedor: rutProveedor, codTipoLog: codTipoLog, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
                success: function (datos) {
                    if (datos === "EMPTY") {
                        var msg = 'Sin registros históricos';
                        $("#lblInfoHistorialRazonSocial").text(msg);
                        $("#alertInfoHistorialRazonSocial").css('display', 'block');
                        $('#tablaHistorialRazonSocial').bootstrapTable("destroy");
                    } else {
                        $("#lblInfoHistorialRazonSocial").text('');
                        $("#alertInfoHistorialRazonSocial").css('display', 'none');
                        bootstrapTableHistorialCambioRazonSocial(datos);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) { }
            });
        }

        function bootstrapTableHistorialCambioRazonSocial(datos) {

            if (!datos || datos.length === 0) {
                var msg = 'Sin registros históricos';
                $("#lblInfoHistorialRazonSocial").text(msg);
                $("#alertInfoHistorialRazonSocial").css('display', 'block');
                $('#tablaHistorialRazonSocial').bootstrapTable("destroy"); // Limpia la tabla por si tenía datos
            } else {
                $("#lblInfoHistorialRazonSocial").text('');
                $("#alertInfoHistorialRazonSocial").css('display', 'none');


                $('#tablaHistorialRazonSocial').bootstrapTable("destroy");
                $('#tablaHistorialRazonSocial').bootstrapTable({
                    search: false, pagination: true, searchAlign: "center" /*searchAlign: "left"*/,
                    pageSize: 10,
                    showPaginationSwitch: false,
                    filterControl: false, //formats: ['xlsx', 'csv', 'txt'],        
                    //showExport: true,
                    //exportDataType: 'all',
                    columns: [{
                        field: 'ID',
                        title: 'ID',
                        visible: false,
                        //filterControl: 'select',
                        sortable: true
                    }, {
                        field: 'FECHA_LOG',
                        title: 'Fecha',
                        formatter: DateFormat,
                        sortable: true
                    }, {
                        field: 'COMENTARIO_1',
                        title: 'Operación',
                        //filterControl: 'select',
                        sortable: true
                    }, {
                        field: 'COMENTARIO_2',
                        title: 'Valores previos',
                        //filterControl: 'select',
                        sortable: true
                    }, {
                        field: 'COMENTARIO_3',
                        title: 'Valores nuevos',
                        //filterControl: 'select',
                        sortable: true
                    }, {
                        field: 'USUARIO_RESPONSABLE',
                        title: 'Usuario Responsable',
                        sortable: true
                    },

                    ],
                    data: datos
                });
            }




        }

    /////////////////////////
    //PND CARGA ARCHIVO PARA CAMBIO RAZON SOCIAL

    function GuardarCambioRazonSocial() {
        var rutProveedor = $("#lblRutProveedor").text();
        var razonSocialAnterior = $("#hiddenRazonSocial").val();
        var razonSocialNueva = $("#txtEditarRazonSocial").val();
        var nombreFantasiaAnterior = $("#hiddenNombreFantasia").val();
        var nombreFantasiaNuevo = $("#txtEditarNombreFantasia").val();
        var camposRequeridos = true;
        var archivoAdjunto = false; //txtDocumentoEvidencia


        if (razonSocialNueva == '' || razonSocialNueva === '') {
            camposRequeridos = false;
        }

        //console.log('razonSocialAnterior ' + razonSocialAnterior);
        //console.log('razonSocialNueva ' + razonSocialNueva);
        //console.log('nombreFantasiaAnterior ' + nombreFantasiaAnterior);
        //console.log('nombreFantasiaNuevo ' + nombreFantasiaNuevo);

        if (camposRequeridos) {
            var msgConfirm = '¿Está seguro de guardar los cambios?';
            if (confirm(msgConfirm) == true) {
                //SE OCULTA BOTÓN BUSCAR Y SE MUESTRA BOTÓN DE PROCESANDO
                $('#btnGuardarRazonSocial').css('display', 'none');
                $('#btnProcesandoGuardarRazonSocial').css('display', 'block');

                $.ajax({
                    type: 'POST',
                    url: "/Proveedores/JsonGuardarCambioRazonSocial",
                    dataType: "json",
                    data: {
                        rutProveedor: rutProveedor, razonSocial: razonSocialNueva, nombreFantasia: nombreFantasiaNuevo, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                    },
                    success: function (datos) {
                        if (datos == "1") {
                            ActualizarTablaProveedores();
                            $("#lblRazonSocialProveedor").text(razonSocialNueva);
                            var msg = "Cambio de razón social ejecutado correctamente";
                            alert(msg);
                            //////////////////////////////////////////////////////////////
                            //////////////////////////////////////////////////////////////
                            //////////////////////////////////////////////////////////////
                            //ACTUALIZAR RAZON SOCIAL DE ENCABEZADO MODAL PROVEEDOR



                            //ACTUALIZAR HISTORIAL
                            CargarDatosHistorialCambioRazonSocial(rutProveedor);

                            $('#btnGuardarRazonSocial').css('display', 'block');
                            $('#btnProcesandoGuardarRazonSocial').css('display', 'none');

                            //GUARDAR LOG
                            var codTipoOperacion = 'CM_RS'; //Cambio Razon Social
                            var codEstado = ''; // ???????
                            var comentario1 = 'Cambio de Razón Social de Proveedor';
                            var comentario2 = 'Razón social: ' + razonSocialAnterior + ' | Nombre fantasía: ' + nombreFantasiaAnterior;
                            var comentario3 = 'Razón social: ' + razonSocialNueva + ' | Nombre fantasía: ' + nombreFantasiaNuevo;
                            GuardarMOPLogProveedor(rutProveedor, codTipoOperacion, codEstado, comentario1, comentario2, comentario3);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) { }
                });
            }
        }
    }


    // #endregion

    // #region CAMBIO ESTADO PROVEEDOR




        function CargarSolicitudesParaCambioEstadoProveedor(codTipoRegistro) {
            var rutProveedor = $("#lblRutProveedor").text();

            $.ajax({
                type: 'POST',
                url: "/Proveedores/JsonTraerSolicitudesParaCambioEstado",
                dataType: "json",
                data: { rutProveedor: rutProveedor, codTipoRegistro: codTipoRegistro, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
                success: function (datos) {
                    //if (datos === "EMPTY") {
                    //} else {

                    //}

                    if (datos === "EMPTY") {
                        $('#cmbBuscarSolicitudConResolucion').empty();
                        $("#cmbBuscarSolicitudConResolucion").append($("<option value='' selected > (No hay Solicitudes con resolución para este usuario) </option>"));
                    } else {
                        $('#cmbBuscarSolicitudConResolucion').empty();
                        $("#cmbBuscarSolicitudConResolucion").append($("<option value='' selected > (Seleccionar...) </option>"));
                        $(datos).each(function () {
                            $("#cmbBuscarSolicitudConResolucion").append($("<option></option>").val(this.IdSol).html(this.Solicitudes));
                        });
                    }

                },
                error: function (jqXHR, textStatus, errorThrown) { }
            });
        }

        //function BuscarDocumentoResolucion() {

        //    //var numSolicitud = $("#txtBuscarSolicitudConResolucion").val();
        //    var numSolicitud = $("#cmbBuscarSolicitudConResolucion").val();
        //    console.log('numSolicitud ' + numSolicitud);
        //    var rutProveedor = $("#lblRutProveedor").text();

        //    //console.log('numSolicitud: ' + numSolicitud)
        //    if (numSolicitud === undefined || numSolicitud == '' || numSolicitud == null) {
        //        alert('Debe ingresar un N° de Solicitud');
        //    } else {
        //        $.ajax({
        //            type: 'POST',
        //            url: "/Proveedores/JsonTraerDocumentoResolucionParaCambioEstado",
        //            dataType: "json",
        //            data: { numSolicitud: numSolicitud, rutProveedor: rutProveedor, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
        //            success: function (datos) {

        //                var m = '';
        //                if (datos === "EMPTY") {
        //                    m = 'La solicitud ingresada no corresponde a este Proveedor, o no tiene un Documento de Resolución válido';
        //                } else {
        //                    $.each(datos, function (key, v) {
        //                        //m = 'Nombre documento resolución: ' + v.NomArchivo + ', de la Solicitud N°: ' + numSolicitud;
        //                        m = 'Documento resolución: ' + v.NomArchivo + ', de la Solicitud N°: ' + numSolicitud + '. \n Comentarios: ' + v.Comentario;
        //                    });
        //                }
        //                $("#lblNombreDocumentoResolucion").text(m);
        //                //Se guarda el N° de solicitud para posterior validación al momento de guardar un cambio de estado a VIGENTE
        //                $("#hiddenNumSolicitudAsociada").val(numSolicitud);
        //            },
        //            error: function (jqXHR, textStatus, errorThrown) { }
        //        });
        //    }



        //}

        function BuscarDocumentoResolucion() {
            var numSolicitud = $("#cmbBuscarSolicitudConResolucion").val();
            var rutProveedor = $("#lblRutProveedor").text();

            if (!numSolicitud) {
                // Simplificamos la validación. Si no hay valor, mostramos el error.
                // No es necesario el alert de "Debe ingresar...", ya que el usuario seleccionó del dropdown.
                $("#lblNombreDocumentoResolucion").text('Por favor, seleccione una solicitud válida.');
                return;
            }

            $.ajax({
                type: 'POST',
                url: "/Proveedores/JsonTraerDocumentoResolucionParaCambioEstado",
                dataType: "json",
                data: { numSolicitud: numSolicitud, rutProveedor: rutProveedor, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
                success: function (datos) {
                    var htmlContent = ''; // Usaremos una variable con un nombre más descriptivo

                    if (datos === "EMPTY") {
                        htmlContent = 'La solicitud ingresada no corresponde a este Proveedor, o no tiene un Documento de Resolución válido';
                    } else {
                        $.each(datos, function (key, v) {
                            // ===== CAMBIO CLAVE AQUÍ =====
                            // Construimos una cadena de HTML con la etiqueta <br> para el salto de línea
                            // y <strong> para resaltar los títulos.
                            htmlContent = '<strong>Documento resolución:</strong> ' + v.NomArchivo +
                                ', de la Solicitud N°: ' + numSolicitud + '.<br>' +
                                '<strong>Comentarios:</strong> ' + v.Comentario;
                        });
                    }

                    // ===== Y CAMBIO CLAVE AQUÍ =====
                    // Usamos .html() para que el navegador renderice las etiquetas <br> y <strong>
                    $("#lblNombreDocumentoResolucion").html(htmlContent);

                    $("#hiddenNumSolicitudAsociada").val(numSolicitud);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $("#lblNombreDocumentoResolucion").text('Error al buscar la resolución.');
                }
            });
        }                           


        function GuardarCambioEstadoMOP() {
        // Recolección de datos
        var nuevoEstado = $("#cmbEditarEstadoProveedor").val();
        var nombreNuevoEstado = $('#cmbEditarEstadoProveedor option:selected').html();
        var anteriorEstado = $("#hiddenAnteriorEstadoProveedor").val();
        var nombreAnteriorEstado = $("#hiddenNombreAnteriorEstadoProveedor").val();
        var rutProveedor = $("#lblRutProveedor").text();
        var codEstado = nuevoEstado;
        var fechaInscripcion = $("#txtEditarFechaInscripcion").val();
        var codTipoReg = $("#hiddenCodTipoRegistro").val();
        var motivoCambioEstado = $("#txtMotivoCambioEstadoMOP").val();
        var numSolAsociada = $("#hiddenNumSolicitudAsociada").val() || 0;
        var nombreDocumentoResolucion = $("#lblNombreDocumentoResolucion").text();

        // --- LÓGICA DE VALIDACIÓN MEJORADA ---
        var errores = [];
        if (!codTipoReg || codTipoReg === 'none') {
            errores.push("Tipo de Registro");
        }
        if (!codEstado || codEstado === 'none') {
            // Manejo especial si el estado es 'Vigente' pero no ha cambiado
            if (anteriorEstado === 'VIG' && !codEstado) {
                codEstado = 'VIG'; // Asumimos que es una renovación de Vigente
            } else {
                errores.push("Estado");
            }
        }
        if (!motivoCambioEstado) {
            errores.push("Motivo de cambio");
        }
        if (codEstado === 'VIG' && !numSolAsociada) {
            errores.push("Asociación de Documento de Resolución");
        }
        if (!fechaInscripcion || !moment(fechaInscripcion, 'YYYY-MM-DD', true).isValid()) {
            errores.push("Fecha de Inscripción válida");
        }

        if (errores.length > 0) {
            alert("Por favor, complete los siguientes campos requeridos:\n- " + errores.join('\n- '));
            return; // Detiene la ejecución
        }

        // --- FIN DE LA VALIDACIÓN ---

        if (confirm('¿Está seguro de guardar los cambios?')) {
            $('#btnGuardarEstadoMOP').prop('disabled', true); // Deshabilitar botón

            $.ajax({
                type: 'POST',
                url: "/Proveedores/JsonGuardarCambioEstadoProveedor",
                dataType: "json",
                data: {
                    rutProveedor: rutProveedor,
                    codEstado: codEstado,
                    codEstadoAnterior: anteriorEstado,
                    fechaInscripcion: fechaInscripcion,
                    codTipoReg: codTipoReg,
                    numSolicitudAsociada: numSolAsociada,
                    __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                },
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        ActualizarTablaProveedores();

                        // Actualizamos la UI después del éxito
                        var estadoFinal = nombreNuevoEstado || nombreAnteriorEstado;
                        $("#lblEstadoVigenciaProveedor").text(estadoFinal);
                        CargarDatosHistorialCambioEstadoMOP(rutProveedor); // Refrescar historial

                        // Lógica para guardar el log
                        var comentario2 = 'Cambio de estado de [' + nombreAnteriorEstado + '] a [' + estadoFinal + ']';
                        if (codEstado == 'VIG') {
                            comentario2 += ' | N° Solicitud asociada: ' + numSolAsociada + ' - Doc. Resolución: ' + nombreDocumentoResolucion;
                        }
                        GuardarMOPLogProveedor(rutProveedor, 'CEV', '', motivoCambioEstado, comentario2, codTipoReg);

                        // Cerrar el modal para forzar la recarga de datos si se vuelve a abrir
                        $('#modalProveedor').modal('hide');
                    }
                },
                error: function () {
                    alert("Ocurrió un error de comunicación al intentar guardar.");
                },
                complete: function () {
                    // Volver a habilitar el botón en cualquier caso
                    $('#btnGuardarEstadoMOP').prop('disabled', false);
                }
            });
        }
    }


        function CargarDatosHistorialCambioEstadoMOP(rutProveedor) {
            var codTipoLog = 'CEV'; //CEV: CAMBIO ESTADO VIGENCIA
            $.ajax({
                type: 'POST',
                url: "/Proveedores/JsonTraerMOPLog",
                dataType: "json",
                data: { rutProveedor: rutProveedor, codTipoLog: codTipoLog, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
                success: function (datos) {
                    if (datos === "EMPTY") {
                        var msg = 'Sin registros históricos';
                        $("#lblInfoHistorialEstadoMOP").text(msg);
                        $("#alertInfoHistorialEstadoMOP").css('display', 'block');
                        $('#tablaHistorialEstadoMOP').bootstrapTable("destroy");
                    } else {
                        $("#lblInfoHistorialEstadoMOP").text('');
                        $("#alertInfoHistorialEstadoMOP").css('display', 'none');
                        bootstrapTableHistorialCambioEstadoMOP(datos);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) { }
            });
        }



        function bootstrapTableHistorialCambioEstadoMOP(datos) {

            if (!datos || datos.length === 0) {
                var msg = 'Sin registros históricos';
                $("#lblInfoHistorialEstadoMOP").text(msg);
                $("#alertInfoHistorialEstadoMOP").css('display', 'block');
                $('#tablaHistorialEstadoMOP').bootstrapTable("destroy");
            } else {
                $("#lblInfoHistorialEstadoMOP").text('');
                $("#alertInfoHistorialEstadoMOP").css('display', 'none');


                $('#tablaHistorialEstadoMOP').bootstrapTable("destroy");
                $('#tablaHistorialEstadoMOP').bootstrapTable({
                    search: false, pagination: true, searchAlign: "center" /*searchAlign: "left"*/,
                    pageSize: 10,
                    showPaginationSwitch: false,
                    filterControl: false, //formats: ['xlsx', 'csv', 'txt'],        
                    //showExport: true,
                    //exportDataType: 'all',
                    columns: [{
                        field: 'ID',
                        title: 'ID',
                        visible: false,
                        //filterControl: 'select',
                        sortable: true
                    }, {
                        field: 'FECHA_LOG',
                        title: 'Fecha',
                        formatter: DateFormat,
                        sortable: true
                    }, {
                        field: 'COMENTARIO_1',
                        title: 'Motivo del cambio de Estado',
                        //filterControl: 'select',
                        sortable: true
                    }, {
                        field: 'COMENTARIO_2',
                        title: 'Valores registrados',
                        //filterControl: 'select',
                        sortable: true
                    }, {
                        field: 'USUARIO_RESPONSABLE',
                        title: 'Usuario Responsable',
                        sortable: true
                    },

                    ],
                    data: datos
                });
            }


        }



        function SelectedEstadoProveedor() {
            var nuevoEstado = $("#cmbEditarEstadoProveedor").val();
            var anteriorEstado = $("#hiddenAnteriorEstadoProveedor").val();

            // Si no se ha seleccionado nada, no hacemos nada.
            if (!nuevoEstado) return;

            // --- LÓGICA PARA HABILITAR/DESHABILITAR LA FECHA DE INSCRIPCIÓN ---
            // Esta es la única condición bajo la cual el campo de fecha se vuelve editable.
            if (anteriorEstado === 'ELI' && nuevoEstado === 'VIG') {
                $('#txtEditarFechaInscripcion').prop('disabled', false);
            } else {
                // Para cualquier otra combinación de estados, el campo permanece deshabilitado.
                $('#txtEditarFechaInscripcion').prop('disabled', true);
            }

            // --- LÓGICA DE VISIBILIDAD ---

            // 1. Caso: Reactivación (Suspendido/Eliminado -> Vigente)
            // El estado anterior NO era vigente, pero el NUEVO SÍ lo es.
            if (anteriorEstado !== 'VIG' && nuevoEstado === 'VIG') {
                $('#divAdjuntarDocumentoResolucion').show(); // Mostrar directamente
                $('#btnRenovarProveedorVigente').hide();     // Ocultar el botón Renovar
                $('#btnGuardarEstadoMOP').show();           // Mostrar el botón Guardar
            }
            // 2. Caso: Renovación (Vigente -> Vigente)
            // El estado anterior ERA vigente y el nuevo también (o el usuario se arrepintió y volvió a seleccionarlo).
            else if (anteriorEstado === 'VIG' && nuevoEstado === 'VIG') {
                // En este caso, el flujo es a través del botón "Renovar".
                // No mostramos la resolución automáticamente, pero sí el botón Renovar.
                $('#divAdjuntarDocumentoResolucion').hide();
                $('#btnRenovarProveedorVigente').show();
                // Ocultamos el botón Guardar hasta que se presione "Renovar"
                $('#btnGuardarEstadoMOP').hide();
            }
            // 3. Caso: Cambio a un estado no vigente (Vigente -> Suspendido, etc.)
            else {
                // Para cualquier otro cambio, ocultamos todo y solo mostramos el botón de guardar.
                $('#divAdjuntarDocumentoResolucion').hide();
                $('#btnRenovarProveedorVigente').hide();
                $('#btnGuardarEstadoMOP').show();
            }
        }


    


        function CalcularYMostrarFechaRenovacion(fechaInscripcion, tipoRegistro) {
            // Validaciones iniciales
            if (!fechaInscripcion || !tipoRegistro || !moment(fechaInscripcion, 'YYYY-MM-DD', true).isValid()) {
                $("#txtFechaRenovacion").val('');
                return;
            }

            var fechaInscripcionMoment = moment(fechaInscripcion, 'YYYY-MM-DD');
            var mesInscripcionJS = fechaInscripcionMoment.month(); // 0 para Enero, 11 para Diciembre

            // 1. Determinar el mes de renovación y establecerlo en el mismo año de la inscripción.
            // Usamos clone() para no modificar la fecha de inscripción original.
            var fechaRenovacion = fechaInscripcionMoment.clone().startOf('month');

            // Aplicar la lógica de negocio para el mes
            if ((tipoRegistro === 'CMA' || tipoRegistro === 'CME') && (mesInscripcionJS === 1 || mesInscripcionJS === 2)) {
                // Caso especial: Contratistas en Feb/Mar -> Renovación un mes DESPUÉS
                fechaRenovacion.add(1, 'month');
            } else {
                // Caso General: Renovación un mes ANTES
                fechaRenovacion.subtract(1, 'month');
            }

            // 2. Asegurar que la primera renovación sea siempre DESPUÉS de la inscripción.
            // Si la fecha de renovación calculada (ej. abril 2024) es anterior a la fecha de inscripción (ej. mayo 2024),
            // significa que el primer ciclo de renovación corresponde al año siguiente.
            if (fechaRenovacion.isBefore(fechaInscripcionMoment)) {
                fechaRenovacion.add(1, 'year');
            }


            // 3. Formatear y mostrar el resultado.
            $("#txtFechaRenovacion").val(fechaRenovacion.format('MMMM YYYY'));
        }


        

        function RenovarProveedor() {
            //var nuevoEstado = $("#cmbEditarEstadoProveedor").val();
            //var anteriorEstado = $("#hiddenAnteriorEstadoProveedor").val();

            // 1. Ocultamos el propio botón para evitar dobles clics.
            $(this).hide();

            // 2. Mostramos la sección para buscar el documento de resolución.
            $('#divAdjuntarDocumentoResolucion').show();

            // 3. Mostramos el botón "Guardar Estado MOP" para completar la acción.
            $('#btnGuardarEstadoMOP').show();

        }



// #endregion

    // #region CAMBIO CAPACIDAD ECONOMICA



        function CargarDatosHistorialCapacidadEconomicaProveedor(rutProveedor, codTipoReg) {

            $.ajax({
                type: 'POST',
                url: "/Proveedores/JsonTraerHistorialCapacidadEconomicaProveedor",
                dataType: "json",
                data: { rutProveedor: rutProveedor, codTipoReg: codTipoReg, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
                success: function (datos) {
                    if (datos === "EMPTY") {
                        var msg = 'Sin registros históricos';
                        $("#lblInfoHistorialCapEconomica").text(msg);
                        $("#alertInfoHistorialCapEconomica").css('display', 'block');
                        $('#tablaHistorialCapacidadEconomica').bootstrapTable("destroy");
                    } else {
                        $("#lblInfoHistorialCapEconomica").text('');
                        $("#alertInfoHistorialCapEconomica").css('display', 'none');
                        bootstrapTableHistorialCapacidadEconomica(datos);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) { }
            });
        }

        function bootstrapTableHistorialCapacidadEconomica(datos) {

            if (!datos || datos.length === 0) {
                var msg = 'Sin registros históricos';
                $("#lblInfoHistorialCapEconomica").text(msg);
                $("#alertInfoHistorialCapEconomica").css('display', 'block');
                $('#tablaHistorialCapacidadEconomica').bootstrapTable("destroy");
            } else {
                $("#lblInfoHistorialCapEconomica").text('');
                $("#alertInfoHistorialCapEconomica").css('display', 'none');


                $('#tablaHistorialCapacidadEconomica').bootstrapTable("destroy");
                $('#tablaHistorialCapacidadEconomica').bootstrapTable({
                    search: false, pagination: true, searchAlign: "center" /*searchAlign: "left"*/,
                    pageSize: 10,
                    showPaginationSwitch: false,
                    filterControl: false, //formats: ['xlsx', 'csv', 'txt'],        
                    //showExport: true,
                    //exportDataType: 'all',
                    columns: [{
                        field: 'CapacidadEconomica',
                        title: 'Monto (UTM)',
                        visible: true,
                        //filterControl: 'select',
                        sortable: true
                    }, {
                        field: 'FechaEstadoFinanciero',
                        title: 'Fecha del estado financiero',
                        formatter: DateFormat,
                        sortable: true
                    },
                        //    {
                        //    field: 'COMENTARIO_1',
                        //    title: 'Comentario 1',
                        //    //filterControl: 'select',
                        //    sortable: true
                        //}, {
                        //    field: 'COMENTARIO_2',
                        //    title: 'Comentario 2',
                        //    //filterControl: 'select',
                        //    sortable: true
                        //}, {
                        //    field: 'USUARIO_RESPONSABLE',
                        //    title: 'Usuario Responsable',
                        //    sortable: true
                        //},

                    ],
                    data: datos
                });
            }


        }

        function GuardarNuevaCapacidadEconomica() {


            var rutProveedor = $("#lblRutProveedor").text();
            var nombreTipoRegistro = $("#lblTipoRegistroProveedor").text();
            var codTipoReg = $("#hiddenCodTipoRegistro").val();
            var montoCapacidad = $("#txtEditarCapacidadEconomica").val();
            var fechaEstadoFinanciero = $("#txtEditarFechaEstadoFinanciero").val();

            var camposRequeridos = true;
            var errorMsg = '';

            console.log('montoCapacidad: ' + montoCapacidad);

            if (montoCapacidad < 0 || montoCapacidad == null) {
                camposRequeridos = false;
                errorMsg = 'La capacidad económica debe ser mayor o igual a cero';
            }

            if (fechaEstadoFinanciero === '' || fechaEstadoFinanciero == null) {
                camposRequeridos = false;
                errorMsg = 'Debe ingresar la fecha de estado financiero';
            }



            if (camposRequeridos) {

                var msgConfirm = '¿Está seguro de guardar los cambios?';
                if (confirm(msgConfirm) == true) {
                    //SE OCULTA BOTÓN BUSCAR Y SE MUESTRA BOTÓN DE PROCESANDO
                    $('#btnGuardarCapacidadEconomica').css('display', 'none');
                    $('#btnProcesandoGuardarCapacidadEconomica').css('display', 'block');

                    $.ajax({
                        type: 'POST',
                        url: "/Proveedores/JsonGuardarNuevaCapacidadEconomicaProveedor",
                        dataType: "json",
                        data: {
                            rutProveedor: rutProveedor, codTipoReg: codTipoReg, montoCapacidad: montoCapacidad, fechaEstadoFinanciero: fechaEstadoFinanciero,
                            __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                        },
                        success: function (datos) {
                            //console.log('datos: ' + datos);
                            if (datos == "2") {
                                //2 INDICA QUE SE DESACTIVÓ EL REGISTRO ANTERIOR Y SE INSERTÓ EL NUEVO REGISTRO                        
                                var msg = "Datos guardados para Capacidad Económica";
                                //ShowAlertModal(msg, 'OK');
                                alert(msg);
                                //ACTUALIZAR HISTORIAL                
                                CargarDatosHistorialCapacidadEconomicaProveedor(rutProveedor, codTipoReg);

                                //GUARDAR LOG
                                var codTipoOperacion = 'LCAEC'; //LOG Capacidad Economica
                                var codEstado = 'GRB'; // ???????
                                //var comentario1 = 'Se registra Capacidad Económica | ' + ' Tipo de Registro: ' + codTipoReg;
                                var comentario1 = 'Se registra Capacidad Económica para ' + nombreTipoRegistro;
                                var comentario2 = 'CE: ' + montoCapacidad + ' (UTM)' + ' | ' + 'Fecha: ' + fechaEstadoFinanciero;
                                var comentario3 = codTipoReg;//SE GUARDA EL TIPO DE REGISTRO PARA PODER FILTRAR DESPUÉS EN EL LOG
                                GuardarMOPLogProveedor(rutProveedor, codTipoOperacion, codEstado, comentario1, comentario2, comentario3);
                            }
                            $('#btnGuardarCapacidadEconomica').css('display', 'block');
                            $('#btnProcesandoGuardarCapacidadEconomica').css('display', 'none');



                        },
                        error: function (jqXHR, textStatus, errorThrown) { }
                    });

                }



            } else {
                alert(errorMsg);

            }


        }





    // #endregion


    // #region CAMBIO ESPECIALIDADES Y CATEGORIAS



    function CargarDatosEspecialidadesCategoriasProveedor_Razor(especialidadesData) {

        var msg = 'Espere mientras se cargan todas las especialidades';
        $("#lblMensajeEspecialidadesYCategorias").text(msg);

        if (!especialidadesData || especialidadesData.length === 0) {
            $("#lblMensajeEspecialidadesYCategorias").text("No hay especialidades disponibles para este tipo de registro.");
            $("#especialidadesCategoriasContainer").html('');
            return;
        }

        $.ajax({
            type: 'POST',
            url: "/Proveedores/PartialRenderEspecialidades",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(especialidadesData),

            success: function (html) {
                //console.log('html: ');
                //console.log(html);
                $("#especialidadesCategoriasContainer").html(html);
                var msgFinal = 'Acá se despliegan todas las especialidades. Al seleccionar una categoría, el cambio se guardará automáticamente.';
                $("#lblMensajeEspecialidadesYCategorias").text(msgFinal);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $("#lblMensajeEspecialidadesYCategorias").text("Error al renderizar las especialidades.");
                console.error("Error en renderizado parcial: ", textStatus, errorThrown, jqXHR.responseText);
                
            }
        });

    }

    function GuardarCategoriaEspecialidad(rutProveedor, codTipoReg, idRegistroCategoria, codEspecialidad, codCategoria, nombreEspecialidad) {
        $.ajax({
            type: 'POST',
            url: "/Proveedores/JsonGuardarCategoriasPorEspecialidadProveedor",
            dataType: "json",
            data: {
                rutProveedor: rutProveedor, codTipoReg: codTipoReg, codEspecialidad: codEspecialidad, codCategoria: codCategoria,
                __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
            },
            success: function (datos) {


                //MENSAJE DE OK
                console.log('Registro guardado OK');
                console.log('codCategoria ' + codCategoria);

                //GUARDAR LOG
                var codTipoOperacion = 'LESCA'; //LOG Especialidades y Categorias
                var codEstado = 'GRB'; // ???????
                //var comentario1 = 'Se registra Capacidad Económica | ' + ' Tipo de Registro: ' + codTipoReg;
                //var comentario1 = 'Se Graba Especialidad y Categorias '// + nombreTipoRegistro;
                var comentario1 = 'Especialidad: [' + nombreEspecialidad + ']';
                //var comentario2 = 'Especialidad: [' + nombreEspecialidad + '] | Categoría: [' + codCategoria+']';
                var comentario2 = 'Categoría: [' + codCategoria + ']';
                if (codCategoria == '-1') {
                    comentario2 = 'Se quita selección de categoría para esta especialidad';
                }
                var comentario3 = codTipoReg;//SE GUARDA EL TIPO DE REGISTRO PARA PODER FILTRAR DESPUÉS EN EL LOG
                GuardarMOPLogProveedor(rutProveedor, codTipoOperacion, codEstado, comentario1, comentario2, comentario3);

                //ACTUALIZA HISTORIAL DE CAMBIOS

                CargarDatosHistorialEspecialidadesYCategorias(rutProveedor, codTipoReg);






            },
            error: function (jqXHR, textStatus, errorThrown) { }
        });




    }

    function CargarDatosHistorialEspecialidadesYCategorias(rutProveedor, codTipoReg) {
        $.ajax({
            type: 'POST',
            url: "/Proveedores/JsonTraerHistorialEspecialidadesYCategorias",
            dataType: "json",
            data: { rutProveedor: rutProveedor, codTipoReg: codTipoReg, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (datos) {
                if (datos === "EMPTY") {
                    var msg = 'Sin registros históricos';
                    $("#lblInfoHistorialEspecialidadesYCategorias").text(msg);
                    $("#alertInfoHistorialEspecialidadesYCategorias").css('display', 'block');
                    $('#tablaHistorialEspecialidadesYCategorias').bootstrapTable("destroy");
                } else {
                    $("#lblInfoHistorialEspecialidadesYCategorias").text('');
                    $("#alertInfoHistorialEspecialidadesYCategorias").css('display', 'none');
                    bootstrapTableHistorialEspecialidadesYCategorias(datos);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) { }
        });
    }

    function bootstrapTableHistorialEspecialidadesYCategorias(datos) {
        if (!datos || datos.length === 0) {
            var msg = 'Sin registros históricos';
            $("#lblInfoHistorialEspecialidadesYCategorias").text(msg);
            $("#alertInfoHistorialEspecialidadesYCategorias").css('display', 'block');
            $('#tablaHistorialEspecialidadesYCategorias').bootstrapTable("destroy");
        } else {
            $("#lblInfoHistorialEspecialidadesYCategorias").text('');
            $("#alertInfoHistorialEspecialidadesYCategorias").css('display', 'none');

            $('#tablaHistorialEspecialidadesYCategorias').bootstrapTable("destroy");
            $('#tablaHistorialEspecialidadesYCategorias').bootstrapTable({
                search: false, pagination: true, searchAlign: "center" /*searchAlign: "left"*/,
                pageSize: 10,
                showPaginationSwitch: false,
                filterControl: true, //formats: ['xlsx', 'csv', 'txt'],        
                //showExport: true,
                //exportDataType: 'all',
                columns: [{
                    field: 'FECHA_LOG',
                    title: 'Fecha del registro',
                    visible: true,
                    formatter: DateTimeFormat,
                    sortable: true
                }, {
                    field: 'COMENTARIO_1',
                    title: 'Especialidad',
                    filterControl: 'select',
                    sortable: true
                }, {
                    field: 'COMENTARIO_2',
                    title: 'Categoría',
                    sortable: true
                }, {
                    field: 'USUARIO_RESPONSABLE',
                    title: 'Usuario',
                    sortable: true
                },

                ],
                data: datos
            });
        }



    }


    // #endregion



    // #region SUBIDA DE DOCUMENTOS DEL PROVEEDOR


        function IniciarDocumentosDelProveedor() {

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


        }

        function CargarTipoDocSubirDoc() {
            var areaDocumento = $("#cmbAreaSubirDocumentoProveedor").val();
            //console.log('areaDocumento ' + areaDocumento);
            if (areaDocumento != '0') {
                $.ajax({
                    type: 'POST',
                    url: "/Proveedores/JsonTraerTipoDocumento",
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

        function CargarDocumentosDelProveedor(rutProveedor) {
            IniciarDocumentosDelProveedor();


            $.ajax({
                type: 'POST',
                url: "/Proveedores/JsonTraerDocumentosDelProveedor",
                dataType: "json",
                data: { rutProveedor: rutProveedor, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
                success: function (datos) {
                    if (datos === "EMPTY") {

                    } else {
                        bootstrapTableDocumentosDelProveedor(datos, rutProveedor);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) { }
            });
        }

        function bootstrapTableDocumentosDelProveedor(datos, rutProveedor) {

            $('#tablaDocumentosDelProveedor').bootstrapTable("destroy");
            $('#tablaDocumentosDelProveedor').bootstrapTable({
                search: false, pagination: true, searchAlign: "center" /*searchAlign: "left"*/,
                pageSize: 10,
                showPaginationSwitch: false,
                filterControl: true, //formats: ['xlsx', 'csv', 'txt'],
                //showExport: true,
                //exportDataType: 'all',
                columns: [{
                    field: 'NOMBRE_ARCHIVO',
                    title: 'Nombre',
                    visible: true,
                    filterControl: 'input',
                    sortable: true
                }, {
                    field: 'DESCRIPCION',
                    title: 'Descripción',
                    filterControl: 'select',
                    sortable: true
                }, {
                    field: 'ID_SOL',
                    title: 'N° Sol',
                    filterControl: 'input',
                    sortable: true
                }, {
                    field: 'AREA_ANALISIS',
                    title: 'Á. Análisis',
                    filterControl: 'select',
                    sortable: true
                }, {
                    field: 'TIPO_DOCUMENTO',
                    title: 'Tipo Doc',
                    filterControl: 'select',
                    sortable: true
                }, {
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
                    title: 'Visible al Prov',
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
                        return '<div class="container">' +
                            ' <input type="hidden" id="hiddenIdArchivoGesintel' + index + '" value="' + row.ID_ARCHIVO_GESINTEL + '" />  ' +
                            ' <input type="hidden" id="hiddenIndex' + index + '" value="' + index + '" />  ' +
                            ' <button id="btnProcesandoBajarArchivo' + index + '" class="btn btn-primary" type="button" style="display:none;" disabled> ' +
                            ' <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ' +
                            ' </button>' +
                            ' <a href="/Solicitudes/BajarArchivoGesintel?idArchivoGesintel=' + row.ID_ARCHIVO_GESINTEL + '&rutProveedor=' + rutProveedor + '" id="btnBajarArchivo' + index + '"  class= "btn btn-primary download-link"> <i class="bi bi-download"></i></a> ' +
                            '</div> ';
                    }
                }


                ],
                data: datos
            });

        }



        function GuardarMOPLogProveedor(rutProveedor, codTipoOperacion, codEstado, comentario1, comentario2, comentario3) {
            //DATOS SOLICITUD EN 0
            var idSol = 0;
            //var idSolRel = 0;
            var idDoc = 0; //NO SE USA POR AHORA

            $.ajax({
                type: 'POST',
                url: "/Proveedores/JsonGuardarMOPLog",
                dataType: "json",
                data: {
                    rutProveedor: rutProveedor, idSol: idSol, idDoc: idDoc, codEstado: codEstado, codTipoOperacion: codTipoOperacion,
                    comentario1: comentario1, comentario2: comentario2, comentario3: comentario3,
                    __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                },
                success: function (datos) {
                    //if (datos == "1") {
                    //} else {
                    //}
                },
                error: function (jqXHR, textStatus, errorThrown) { }
            });

        }





    // #endregion



    // #region WAITING MODALS


        function showWaitingModal() {
            $('#waitingModal').modal('show');
        }

        function hideWaitingModal() {
            $('#waitingModal').modal('hide');
        }


    // #endregion



    // #region FORMATOS DE FECHA

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




    // #endregion



    // #endregion

    // =========================================================================
    // 4. EXPOSICIÓN DE FUNCIONES GLOBALES (Si es necesario)
    // =========================================================================
    // Tu grid de Angular necesita llamar a CargarDatosProveedor. Como hemos envuelto
    // todo en una IIFE, la función ahora es privada. Debemos "exponerla" manualmente.
    window.CargarDatosProveedor = CargarDatosProveedor;

})(jQuery); // Pasamos jQuery a la IIFE para poder usar $ de forma segura.