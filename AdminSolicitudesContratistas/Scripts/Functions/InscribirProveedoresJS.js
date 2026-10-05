// Scripts/Functions/InscribirProveedores.js
(function ($) {
    'use strict';

    // =========================================================================
    // 1. CÓDIGO DE INICIALIZACIÓN
    // =========================================================================
    $(document).ready(function () {
        $(document).ajaxError(function (e, xhr) {
            if (xhr.status == 401) window.location = "../Home/Login";
            else if (xhr.status == 403) alert("No tiene permisos para acceder a este recurso.");
        });

        asignarEventos();
        IniciarFormulario();
        CargarDatosParaFormulario();
    });

    // =========================================================================
    // 2. ASIGNACIÓN DE EVENTOS
    // =========================================================================
    function asignarEventos() {
        $('#btnBuscarProveedor').on('click', function () {
            var rutProveedor = $('#txtBuscarRutProveedor').val();
            BuscarProveedor(rutProveedor);
        });

        // Eventos del formulario de inscripción
        $('#cmbTipoRegistro').on('change', function () {
            var codTipoRegistro = $(this).val();
            $("#hiddenCodTipoRegistro").val(codTipoRegistro);

            // Limpiamos los campos dependientes antes de gestionar la visibilidad
            $("#cmbEditarEstadoProveedor").val('');
            $("#txtEditarFechaInscripcion").val('');
            $("#txtFechaRenovacion").val('');
            $("#txtMotivoCambioEstadoMOP").val('');
            $('#divAdjuntarDocumentoResolucion').hide();


            gestionarVisibilidadControles();
            CargarSolicitudesParaCambioEstadoProveedor(codTipoRegistro);
        });

        $('#cmbEditarEstadoProveedor').on('change', gestionarVisibilidadControles);
        $('#cmbBuscarSolicitudConResolucion').on('change', BuscarDocumentoResolucion);

        $('#txtEditarFechaInscripcion').on("change", function () {
            var $fechaInput = $(this);
            var fechaInscripcion = $fechaInput.val();
            if (!moment(fechaInscripcion, 'YYYY-MM-DD', true).isValid()) {
                alert("La fecha de inscripción no es válida. Use el formato AAAA-MM-DD.");
                $("#txtFechaRenovacion").val('');
                $fechaInput.addClass('is-invalid');
                return;
            }
            $fechaInput.removeClass('is-invalid');

            var tipoRegistro = $("#hiddenCodTipoRegistro").val();
            CalcularYMostrarFechaRenovacion(fechaInscripcion, tipoRegistro);
        });

        $('#btnGuardarEstadoMOP').on('click', InscribirProveedor);
    }

    // =========================================================================
    // 3. DEFINICIONES DE FUNCIONES
    // =========================================================================

    // #region CARGA INICIAL Y LÓGICA DE BÚSQUEDA

    function IniciarFormulario() {
        $("#cardDatosInscripcionRegistroMOP, #cardDatosRegistroMOP, #lblRazonSocialProveedor").hide();
        IniciarControles();
    }

    function CargarDatosParaFormulario() {
        // La llamada a CargarMesesRenovacion ya no es necesaria con el nuevo input de texto.
        CargarEstadosProveedor();
    }

    function BuscarProveedor(rutProveedor) {
        if (!rutProveedor) {
            alert('Por favor, ingrese un RUT de proveedor.');
            return;
        }

        $('#btnBuscarProveedor').hide();
        $('#btnProcesandoBuscarProveedor').show();

        $.ajax({
            type: 'POST',
            url: "/InscribirProveedores/JsonBuscarProveedorInscrito",
            dataType: "json",
            data: { rutProveedor: rutProveedor, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (datos) {
                if (datos === "EMPTY") {
                    mostrarErrorProveedorNoEncontrado();
                } else {
                    mostrarPanelesDeInscripcion();
                    $.each(datos, function (key, val) {
                        $("#lblRazonSocialProveedor").show().text(val.RazonSocial);
                        $("#hiddenRutInscribirProveedor").val(val.Rut);
                    });
                    CargarFichaRegistroProveedor(rutProveedor);
                }
            },
            error: function () {
                alert('Ocurrió un error al buscar el proveedor.');
            },
            complete: function () {
                $('#btnBuscarProveedor').show();
                $('#btnProcesandoBuscarProveedor').hide();
            }
        });
    }

    function CargarFichaRegistroProveedor(rutProveedor) {
        $.ajax({
            type: 'POST',
            url: "/InscribirProveedores/JsonTraerEstadosProveedor",
            dataType: "json",
            data: { rutProveedor: rutProveedor, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (datos) {
                if (datos == "EMPTY") {
                    CargarFichaEstadoRegistroMOPEnTabla_SinRegistros();
                } else {
                    CargarFichaEstadoRegistroMOPEnTabla(datos);
                }
            }
        });
    }

    // #endregion

    // #region LÓGICA DE INSCRIPCIÓN (GUARDADO)

    function InscribirProveedor() {
        $('#btnGuardarEstadoMOP').prop('disabled', true).hide();
        $('#btnProcesandoGuardarEstadoMOP').show();

        // Recolectar y validar datos
        var rutProveedor = $("#hiddenRutInscribirProveedor").val();
        var codEstado = $("#cmbEditarEstadoProveedor").val();
        var fechaInscripcion = $("#txtEditarFechaInscripcion").val();
        var codTipoReg = $("#hiddenCodTipoRegistro").val();
        var numSolAsociada = $("#hiddenNumSolicitudAsociada").val() || 0;
        var motivoCambioEstado = $("#txtMotivoCambioEstadoMOP").val();
        var nombreNuevoEstado = $('#cmbEditarEstadoProveedor option:selected').html();

        // Validaciones
        if (!codTipoReg || !codEstado || !motivoCambioEstado || (codEstado === 'VIG' && !numSolAsociada)) {
            alert('Debe completar todos los campos requeridos.');
            $('#btnGuardarEstadoMOP').prop('disabled', false).show();
            $('#btnProcesandoGuardarEstadoMOP').hide();
            return;
        }

        if (confirm('¿Está seguro de inscribir al Proveedor?')) {
            $.ajax({
                type: 'POST',
                url: "/InscribirProveedores/JsonInscribirProveedorRegistroMOP",
                dataType: "json",
                data: {
                    rutProveedor: rutProveedor,
                    codEstado: codEstado,
                    fechaInscripcion: fechaInscripcion,
                    codTipoRegistro: codTipoReg,
                    numSolicitudAsociada: numSolAsociada,
                    __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                },
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        // Guardar el log después del éxito
                        var nombreDocumentoResolucion = $("#lblNombreDocumentoResolucion").text();
                        var comentario2 = 'Inscripción de Proveedor en estado [' + nombreNuevoEstado + ']';
                        if (codEstado == 'VIG') {
                            comentario2 += ' | N° Solicitud asociada: ' + numSolAsociada + ' - Doc. Resolución: ' + nombreDocumentoResolucion;
                        }
                        GuardarMOPLogProveedor(rutProveedor, 'CEV', '', motivoCambioEstado, comentario2, codTipoReg);

                        // Refrescar la ficha completa para mostrar el nuevo estado
                        BuscarProveedor(rutProveedor);
                    }
                },
                error: function () {
                    alert('Ocurrió un error de comunicación al intentar inscribir.');
                },
                complete: function () {
                    $('#btnGuardarEstadoMOP').prop('disabled', false).show();
                    $('#btnProcesandoGuardarEstadoMOP').hide();
                }
            });
        } else {
            // Si el usuario cancela, habilitar el botón de nuevo
            $('#btnGuardarEstadoMOP').prop('disabled', false).show();
            $('#btnProcesandoGuardarEstadoMOP').hide();
        }
    }

    // #endregion

    // #region FUNCIONES DE UI Y RENDERIZADO

    function IniciarControles() {
        // Limpieza de campos
        $("#lblRazonSocialProveedor").hide().text('');
        $("#cmbTipoRegistro, #cmbEditarEstadoProveedor, #cmbBuscarSolicitudConResolucion").val(''); // Usar 'none' o '' según el valor del option inicial
        $("#txtFechaRenovacion, #txtEditarFechaInscripcion, #txtMotivoCambioEstadoMOP").val('');
        $("#hiddenNumSolicitudAsociada, #hiddenRutInscribirProveedor, #hiddenCodTipoRegistro").val('');
        $("#lblSolicitudAsociada, #lblNombreDocumentoResolucion").text('');

        // --- LÓGICA DE ESTADO INICIAL ---
        // Habilitamos solo el primer dropdown
        $('#cmbTipoRegistro').prop('disabled', false);

        // Deshabilitamos el resto de los controles del formulario
        $('#cmbEditarEstadoProveedor').prop('disabled', true);
        $('#txtEditarFechaInscripcion').prop('disabled', true).removeClass('is-invalid');
        $('#txtMotivoCambioEstadoMOP').prop('disabled', true);
        $('#divAdjuntarDocumentoResolucion').hide();
    }

    function mostrarPanelesDeInscripcion() {
        $("#cardDatosInscripcionRegistroMOP, #cardDatosRegistroMOP").show();
        IniciarControles();
    }

    function mostrarErrorProveedorNoEncontrado() {
        $("#cardDatosInscripcionRegistroMOP").hide();
        $("#cardDatosRegistroMOP").hide();
        $("#lblRazonSocialProveedor").show().text('');
        alert('Proveedor no encontrado en los registros MOP...');
    }

    function CargarFichaEstadoRegistroMOPEnTabla(datos) {
        var existeRegistroObraMayor = '';
        var existeRegistroObraMenor = '';
        var existeRegistroConsultor = '';
        LimpiarTablaFichaEstadoRegistroMOP();
        $('#tablaFichaDeEstadoRegistropMOP').append('<tr><th>Tipo de Registro</th><th>Estado</th><th>Fecha Inscripción</th><th>Fecha Renovación</th><th>Comentarios</th></tr>');

        $.each(datos, function (i, item) {
            $('#tablaFichaDeEstadoRegistropMOP').append('<tr><td>' + item.TipoRegistro + '</td><td>' + item.Estado + '</td><td>' + DateFormat(item.FechaIngresoReg) + '</td><td>' + DateFormat(item.FechaSuspension) + '</td><td>' + item.Log + '</td></tr>');
            switch (item.CodTipoReg) {
                case 'CMA': existeRegistroObraMayor = 'CMA'; break;
                case 'CME': existeRegistroObraMenor = 'CME'; break;
                case 'CNS': existeRegistroConsultor = 'CNS'; break;
            }
        });
        CargarTipoRegistroMOP(existeRegistroObraMayor, existeRegistroObraMenor, existeRegistroConsultor);
    }

    function CargarFichaEstadoRegistroMOPEnTabla_SinRegistros() {
        LimpiarTablaFichaEstadoRegistroMOP();
        $('#tablaFichaDeEstadoRegistropMOP').append('<tr><td><h3>--- PROVEEDOR AÚN NO INSCRITO ---</h3></td></tr>');
        CargarTipoRegistroMOP('', '', '');
    }

    function LimpiarTablaFichaEstadoRegistroMOP() {
        $('#tablaFichaDeEstadoRegistropMOP').html('');
    }

    // Esta función ya no es necesaria si reemplazaste el dropdown por el input de texto.
    // function CargarMesesRenovacion() { /* ... */ } 

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
                        //Muestra todos menos Eliminado
                        if (this.CODIGO != 'ELI') {
                            $("#cmbEditarEstadoProveedor").append($("<option></option>").val(this.CODIGO).html(this.DESCRIPCION));
                        }

                    });

                }
            },
            error: function (jqXHR, textStatus, errorThrown) { }
        });
    }

    function CargarTipoRegistroMOP(existeRegistroObraMayor, existeRegistroObraMenor, existeRegistroConsultor) {
        //console.log('existeRegistroObraMayor ' + existeRegistroObraMayor);
        //console.log('existeRegistroObraMenor ' + existeRegistroObraMenor);
        //console.log('existeRegistroConsultor ' + existeRegistroConsultor);
        var codigoPadre = 2237;//TIPO DE REGISTRO
        $.ajax({
            type: 'POST',
            url: "/Solicitudes/JsonListarParametrosPorTipo",
            dataType: "json",
            data: { codigoPadre: codigoPadre, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (datos) {
                if (datos === "EMPTY") {

                } else {
                    $('#cmbTipoRegistro').empty();
                    $("#cmbTipoRegistro").append($("<option value='' selected > (Seleccionar...) </option>"));
                    $(datos).each(function () {
                        if (existeRegistroObraMayor == this.CODIGO || existeRegistroObraMenor == this.CODIGO || existeRegistroConsultor == this.CODIGO) {

                        } else {
                            $("#cmbTipoRegistro").append($("<option></option>").val(this.CODIGO).html(this.DESCRIPCION));
                        }

                    });

                }
            },
            error: function (jqXHR, textStatus, errorThrown) { }
        });
    }

    function gestionarVisibilidadControles() {
        var tipoRegistro = $('#cmbTipoRegistro').val();
        var estadoSeleccionado = $("#cmbEditarEstadoProveedor").val();

        // Si no se ha seleccionado un Tipo de Registro, mantenemos todo deshabilitado y limpio.
        if (!tipoRegistro || tipoRegistro === 'none') {
            $('#cmbEditarEstadoProveedor').prop('disabled', true).val('');
            $('#txtEditarFechaInscripcion').prop('disabled', true).val('');
            $('#txtMotivoCambioEstadoMOP').prop('disabled', true).val('');
            $('#divAdjuntarDocumentoResolucion').hide();
            return;
        }

        // Si ya hay un Tipo de Registro, habilitamos los siguientes campos.
        $('#cmbEditarEstadoProveedor').prop('disabled', false);
        $('#txtEditarFechaInscripcion').prop('disabled', false);
        $('#txtMotivoCambioEstadoMOP').prop('disabled', false);

        // Finalmente, gestionamos la visibilidad de la sección de resolución
        // basándonos en el estado que el usuario seleccione.
        if (estadoSeleccionado === 'VIG') {
            $('#divAdjuntarDocumentoResolucion').show();
        } else {
            $('#divAdjuntarDocumentoResolucion').hide();
        }
    }

    function CalcularYMostrarFechaRenovacion(fechaInscripcion, tipoRegistro) {
        // 1. Validaciones iniciales (se mantienen)
        if (!fechaInscripcion || !tipoRegistro || !moment(fechaInscripcion, 'YYYY-MM-DD', true).isValid()) {
            $("#txtFechaRenovacion").val('');
            return;
        }

        var fechaInscripcionMoment = moment(fechaInscripcion, 'YYYY-MM-DD');
        var mesInscripcionJS = fechaInscripcionMoment.month();

        // 2. Determinar el mes y año de la renovación inicial
        var fechaRenovacion = fechaInscripcionMoment.clone().startOf('month');

        // Aplicar la lógica de negocio para el mes
        if ((tipoRegistro === 'CMA' || tipoRegistro === 'CME') && (mesInscripcionJS === 1 || mesInscripcionJS === 2)) {
            // Caso especial: Contratistas en Feb/Mar -> Renovación un mes DESPUÉS
            fechaRenovacion.add(1, 'month');
        } else {
            // Caso General: Renovación un mes ANTES
            fechaRenovacion.subtract(1, 'month');
        }

        // 3. Asegurar que la renovación sea siempre DESPUÉS de la inscripción.
        // Esta es la lógica clave que calcula el año correcto.
        if (fechaRenovacion.isBefore(fechaInscripcionMoment)) {
            fechaRenovacion.add(1, 'year');
        }

        // 4. EL BUCLE 'WHILE' HA SIDO ELIMINADO.
        // No forzamos la fecha a ser futura con respecto a HOY.

        // 5. Formatear y mostrar el resultado.
        $("#txtFechaRenovacion").val(fechaRenovacion.format('MMMM YYYY'));
    }

    function CargarSolicitudesParaCambioEstadoProveedor(codTipoRegistro) {
        var rutProveedor = $("#hiddenRutInscribirProveedor").val();

        $.ajax({
            type: 'POST',
            url: "/Proveedores/JsonTraerSolicitudesParaCambioEstado",
            dataType: "json",
            data: { rutProveedor: rutProveedor, codTipoRegistro: codTipoRegistro, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (datos) {
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

    function BuscarDocumentoResolucion() {
        //var numSolicitud = $("#txtBuscarSolicitudConResolucion").val();
        var numSolicitud = $("#cmbBuscarSolicitudConResolucion").val();
        console.log('numSolicitud ' + numSolicitud);
        var rutProveedor = $("#hiddenRutInscribirProveedor").val();

        //console.log('numSolicitud: ' + numSolicitud)
        if (numSolicitud === undefined || numSolicitud == '' || numSolicitud == null) {
            alert('Debe ingresar un N° de Solicitud');
        } else {
            $.ajax({
                type: 'POST',
                url: "/Proveedores/JsonTraerDocumentoResolucionParaCambioEstado",
                dataType: "json",
                data: { numSolicitud: numSolicitud, rutProveedor: rutProveedor, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
                success: function (datos) {

                    var m = '';
                    if (datos === "EMPTY") {
                        m = 'La solicitud ingresada no corresponde a este Proveedor, o no tiene un Documento de Resolución válido';
                    } else {
                        $.each(datos, function (key, v) {
                            m = 'Nombre documento resolución: ' + v.NomArchivo + ', de la Solicitud N°: ' + numSolicitud;
                        });
                    }
                    $("#lblNombreDocumentoResolucion").text(m);
                    //Se guarda el N° de solicitud para posterior validación al momento de guardar un cambio de estado a VIGENTE
                    $("#hiddenNumSolicitudAsociada").val(numSolicitud);
                },
                error: function (jqXHR, textStatus, errorThrown) { }
            });
        }
    }

    // #endregion

    // #region HELPERS Y OTRAS FUNCIONES
    function GuardarMOPLogProveedor(rutProveedor, codTipoOperacion, codEstado, comentario1, comentario2, comentario3) { /* Tu código original se mantiene */ }

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

})(jQuery);