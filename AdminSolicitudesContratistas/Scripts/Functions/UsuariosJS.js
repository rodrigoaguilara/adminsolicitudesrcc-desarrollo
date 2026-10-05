
(function ($) {
    'use strict';

    // =========================================================================
    // 1. CÓDIGO DE INICIALIZACIÓN
    // =========================================================================
    $(document).ready(function () {
        asignarEventos();
        CargarDatosIniciales();
    });

    // =========================================================================
    // 2. ASIGNACIÓN DE EVENTOS
    // =========================================================================
    function asignarEventos() {
        $('#btnBuscarFuncionarioMOP').on('click', BuscarFuncionariosMOP);
        $('#btnNuevoUsuario').on('click', CargarNuevoUsuario);
        $('#btnGuardarUsuarioMOP').on('click', GuardarUsuario);
        // ===== MANEJAR EL CIERRE DEL MODAL =====
        var modalUsuario = document.getElementById('modalUsuarioMOP');

        modalUsuario.addEventListener('show.bs.modal', function () {
            $('#btnGuardarUsuarioMOP').prop('disabled', false).show();
            $('#btnProcesandoGuardarUsuarioMOP').hide();
        });

        // Escuchamos el evento que se dispara JUSTO ANTES de que el modal comience a ocultarse.
        modalUsuario.addEventListener('hide.bs.modal', function (event) {
            // Al detectar que el modal va a cerrarse, movemos inmediatamente el foco
            // a un elemento seguro que NO será ocultado (como el botón "Agregar Nuevo Usuario").
            var btnNuevoUsuario = document.getElementById('btnNuevoUsuario');
            if (btnNuevoUsuario) {
                btnNuevoUsuario.focus();
            }
        });
    }

    // =========================================================================
    // 3. DEFINICIONES DE FUNCIONES
    // =========================================================================

    // #region CARGA INICIAL Y DROPDOWNS

    function CargarDatosIniciales() {
        // Ponemos los dropdowns en estado "Cargando..."
        var dropdowns = $('#cmbTipoSolicitud, #cmbTipoRegistro, #cmbPerfilUsuario, #cmbRegion');
        dropdowns.empty().append($('<option disabled selected>Cargando...</option>')).prop('disabled', true);

        // Hacemos una única llamada para obtener todos los datos
        $.ajax({
            type: 'POST',
            url: "/UsuariosMOP/JsonTraerParametrosUsuario",
            dataType: "json",
            data: { __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (data) {
                if (data) {
                    PoblarDropdown($('#cmbTipoSolicitud'), data.TiposSolicitud, 'Codigo', 'Descripcion', true);
                    PoblarDropdown($('#cmbTipoRegistro'), data.TiposRegistro, 'Codigo', 'Descripcion', true);
                    PoblarDropdown($('#cmbPerfilUsuario'), data.Perfiles, 'CodPerfil', 'DescPerfil');
                    PoblarDropdown($('#cmbRegion'), data.Regiones, 'CodRegion', 'NombreRegion', true, 0);
                }
            },
            error: function () {
                dropdowns.empty().append($('<option disabled selected>Error al cargar</option>'));
                alert('No se pudieron cargar los datos para el formulario de usuario.');
            }
        });
    }

    // Función auxiliar genérica y mejorada
    function PoblarDropdown($selector, datos, valorProp, textoProp, incluirTodos, valorTodos) {
        $selector.empty().append($("<option value='none' selected> (Seleccionar...) </option>"));
        if (incluirTodos) {
            var val = (valorTodos !== undefined) ? valorTodos : 'TODOS';
            $selector.append($("<option></option>").val(val).html('Todas'));
        }
        if (datos && datos.length > 0) {
            $.each(datos, function () {
                $selector.append($("<option></option>").val(this[valorProp]).html(this[textoProp]));
            });
        }
        $selector.prop('disabled', false);
    }

    // #endregion

    // #region LÓGICA DEL MODAL DE USUARIO

    function CargarDatosUsuarioMOP(usernameAD) {
        TituloModal('Detalle de Usuario');
        $("#divBuscarUsuarioMOP").hide();

        $.ajax({
            type: 'POST',
            url: "/UsuariosMOP/JsonTraerDatosUsuarioMOP",
            dataType: "json",
            data: { usernameAD: usernameAD, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (datos) {
                if (datos != "EMPTY") {
                    $.each(datos, function (key, v) {
                        $("#txtUsernameAD").val(v.UsernameAD);
                        $("#txtNombreUsuario").val(v.Nombre);
                        $("#txtEmailUsuario").val(v.Email);
                        $("#cmbPerfilUsuario").val(v.CodPerfil);
                        $("#cmbTipoSolicitud").val(v.CodTipoSol);
                        $("#cmbTipoRegistro").val(v.CodTipoReg);
                        $("#cmbRegion").val(v.CodRegion);
                        $("#cmbUsuariovigente").val(v.Vigente ? 'S' : 'N');
                    });
                }
            }
        });
    }

    function CargarNuevoUsuario() {
        $('#modalUsuarioMOP').modal('show');
        TituloModal('Nuevo Usuario');
        $("#divBuscarUsuarioMOP").show();
        $("#txtUsernameFuncionarioADBuscar").val('');
        $("#txtUsernameAD, #txtNombreUsuario, #txtEmailUsuario, #hiddenUsernameFuncionarioAD").val('');
        $("#cmbPerfilUsuario, #cmbTipoSolicitud, #cmbTipoRegistro, #cmbRegion").val('none');
        $("#cmbUsuariovigente").val('S');
    }

    function BuscarFuncionariosMOP() {
        var funcionarioAD = $("#txtUsernameFuncionarioADBuscar").val();
        if (!funcionarioAD) {
            alert('Ingrese la cuenta del funcionario a buscar.');
            return;
        }

        $('#btnBuscarFuncionarioMOP').hide();
        $('#btnProcesandoBuscarFuncionarioMOP').show();

        $.ajax({
            type: 'POST',
            url: "/UsuariosMOP/JsonBuscarFuncionarioMOPWS",
            dataType: "json",
            data: { funcionarioAD: funcionarioAD, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
            success: function (datos) {
                if (datos === "EXISTE") {
                    alert('El funcionario ya está ingresado en el Sistema. Se cargarán sus datos para edición.');
                    $("#hiddenUsernameFuncionarioAD").val('');
                    CargarDatosUsuarioMOP(funcionarioAD);
                } else if (datos && datos.length > 0) {
                    var v = datos[0];
                    if (v.estadoValidaAD == 1) {
                        $("#txtNombreUsuario").val(v.cnAD);
                        $("#txtUsernameAD").val(v.nomuserAD);
                        $("#txtEmailUsuario").val(v.mailAD);
                        $("#hiddenUsernameFuncionarioAD").val(funcionarioAD);
                    } else {
                        $("#hiddenUsernameFuncionarioAD").val('');
                        alert('Funcionario no encontrado en Active Directory.');
                    }
                } else {
                    $("#hiddenUsernameFuncionarioAD").val('');
                    alert('Funcionario no encontrado.');
                }
            },
            complete: function () {
                $('#btnBuscarFuncionarioMOP').show();
                $('#btnProcesandoBuscarFuncionarioMOP').hide();
            }
        });
    }

    // En UsuariosJS.js

    function GuardarUsuario() {
        // 1. Recolectar todos los datos del formulario.
        var nombreUsuario = $('#txtNombreUsuario').val();
        var usernameAD = $('#txtUsernameAD').val();
        var emailUsuario = $('#txtEmailUsuario').val();
        var perfilUsuario = $('#cmbPerfilUsuario').val();
        var tipoSolicitud = $('#cmbTipoSolicitud').val();
        var tipoRegistro = $('#cmbTipoRegistro').val();
        var regiones = $('#cmbRegion').val();
        var vigente = ($("#cmbUsuariovigente").val() === 'S'); // Conversión directa a booleano

        // 2. Realizar las validaciones de campos requeridos.
        // Usamos un array para hacer la validación más limpia.
        var camposInvalidos = [];
        if (!nombreUsuario) camposInvalidos.push("Nombre");
        if (!usernameAD) camposInvalidos.push("Cuenta de usuario");
        if (!emailUsuario) camposInvalidos.push("e-mail");
        if (!perfilUsuario || perfilUsuario === 'none') camposInvalidos.push("Perfil");
        if (!tipoSolicitud || tipoSolicitud === 'none') camposInvalidos.push("Tipos de Solicitud");
        if (!tipoRegistro || tipoRegistro === 'none') camposInvalidos.push("Tipos de Registro");
        if (!regiones || regiones === 'none') camposInvalidos.push("Regiones");

        if (camposInvalidos.length > 0) {
            alert('Debe completar todos los campos requeridos: ' + camposInvalidos.join(', '));
            return; // Detiene la ejecución si hay errores.
        }

        // 3. Si las validaciones pasan, procedemos con el guardado.
        if (confirm('¿Está seguro de guardar los cambios para este usuario?')) {
            // Deshabilitamos el botón para prevenir doble clic.
            $('#btnGuardarUsuarioMOP').prop('disabled', true).hide();
            $('#btnProcesandoGuardarUsuarioMOP').show();

            $.ajax({
                type: 'POST',
                url: "/UsuariosMOP/JsonGuardarUsuarioMOP",
                dataType: "json",
                data: {
                    // Ya NO enviamos 'nuevoUsuario'. El SP lo decidirá.
                    usernameAD: usernameAD,
                    nombreUsuario: nombreUsuario,
                    emailUsuario: emailUsuario,
                    vigente: vigente,
                    perfilUsuario: perfilUsuario,
                    tipoSolicitud: tipoSolicitud,
                    tipoRegistro: tipoRegistro,
                    regiones: regiones,
                    __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                },
                success: function (datos) {
                    // El SP ahora devuelve 2 para éxito y -1 para error.
                    if (datos == "2") {
                        alert("Usuario guardado correctamente");
                        $('#modalUsuarioMOP').modal('hide'); // Cierra el modal
                        window.ActualizarTablaUsuarios(); // Refresca la tabla principal
                    } else {
                        alert("Ocurrió un error al guardar el usuario en la base de datos.");
                        // Si falla, volvemos a habilitar el botón para que se pueda intentar de nuevo.
                        $('#btnGuardarUsuarioMOP').prop('disabled', false).show();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert("Error de comunicación al intentar guardar el usuario.");
                    // También habilitamos el botón en caso de error de red.
                    $('#btnGuardarUsuarioMOP').prop('disabled', false).show();
                },
                complete: function () {
                    // En cualquier caso, ocultamos el spinner.
                    $('#btnProcesandoGuardarUsuarioMOP').hide();
                }
            });
        }
    }

    // #endregion

    // #region HELPERS
    function TituloModal(titulo) {
        $("#lblTitle").text(titulo);
    }
    // #endregion

    // =========================================================================
    // 4. EXPOSICIÓN DE FUNCIONES GLOBALES (PARA ANGULAR)
    // =========================================================================
    window.CargarDatosUsuarioMOP = CargarDatosUsuarioMOP;

})(jQuery);