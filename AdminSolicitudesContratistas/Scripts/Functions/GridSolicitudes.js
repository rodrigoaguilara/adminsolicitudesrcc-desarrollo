// Scripts/Functions/GridSolicitudes.js
(function () {
    'use strict';

    // =========================================================================
    // 1. DEFINICIÓN DEL CONTROLADOR DE ANGULAR
    // =========================================================================
    function MainCtrlSolicitudes($scope, $http, $q, $interval, uiGridConstants) {
        var self = this;

        // --- Configuración de la Grilla (UI-GRID) ---
        $scope.gridOptions = {
            enableFiltering: true,
            enableCellEditOnFocus: false,
            enableRowSelection: true,
            enableRowHeaderSelection: false,
            enableHiding: false,
            selectionRowHeaderWidth: 30,
            multiSelect: false,
            showGridFooter: true,
            enablePagination: false,
            onRegisterApi: function (gridApi) {
                $scope.gridApi = gridApi;
            },
            columnDefs: [
                { name: 'ID', displayName: 'N° Sol', type: 'number', width: '5%', cellTemplate: '<div class="ui-grid-cell-contents text-center"><a href="" ng-click="grid.appScope.abrirModalSolicitud(row.entity)">{{row.entity.ID}}</a></div>' },
                { name: 'ID_SOL_PADRE', displayName: 'Sol Padre', width: '5%' },
                { name: 'RUT', displayName: 'RUT', width: '8%' },
                { name: 'RAZON_SOCIAL', displayName: 'Razón Social', width: '30%' },
                { name: 'ESTADO', displayName: 'Estado', width: '8%' },
                { name: 'TIPO_SOL', displayName: 'Tipo Solicitud', width: '10%' },
                { name: 'TIPO_REG', displayName: 'Tipo Registro', width: '13%' },
                { name: 'REGION', displayName: 'Región', width: '15%' },
                { name: 'FECHA_RECEPCION', displayName: 'Fecha Recepción', type: 'date', cellFilter: 'date:"dd/MM/yyyy"', width: '10%' },
                { name: 'FECHA_INGRESO', displayName: 'Fecha Envío Análisis', type: 'date', cellFilter: 'date:"dd/MM/yyyy"', width: '10%' },
                { name: 'FECHA_ANALISIS_TEC', displayName: 'Fecha Análisis Técnico', type: 'date', cellFilter: 'date:"dd/MM/yyyy"', width: '15%' },
                { name: 'ESTADO_ANALISIS_TEC', displayName: 'Estado Análisis Técnico', width: '10%' },
                { name: 'FECHA_ANALISIS_LEG', displayName: 'Fecha Análisis Legal', type: 'date', cellFilter: 'date:"dd/MM/yyyy"', width: '15%' },
                { name: 'ESTADO_ANALISIS_LEG', displayName: 'Estado Análisis Legal', width: '10%' },
                { name: 'FECHA_ANALISIS_CONTABLE', displayName: 'Fecha Análisis Contable', type: 'date', cellFilter: 'date:"dd/MM/yyyy"', width: '15%' },
                { name: 'ESTADO_ANALISIS_CONTABLE', displayName: 'Estado Análisis Contable', width: '10%' },
                { name: 'FECHA_RESPUESTA', displayName: 'Respuesta Final', type: 'date', cellFilter: 'date:"dd/MM/yyyy"', width: '10%' },
                { name: 'DIAS_DESDE_INGRESO', displayName: 'Días', type: 'number', width: '10%' }
            ]
        };

        // --- Lógica de Datos ---
        self.BuscarSolicitudes = function () {
            $('#btnBuscarSolicitudes').hide();
            $('#procesando').show();

            var rutProveedor = $("#txtBuscarRutProveedor").val();
            var idSolicitud = $("#txtBuscarNumeroSolicitud").val() || 0;
            var estado = $("#cmbBuscarEstadoMOP").val();
            var tipoSolicitud = $("#cmbBuscarTipoSolicitud").val();
            var userTipoSolicitud = $("#hiddenUserTipoSolicitud").val();
            if (tipoSolicitud == 0 && userTipoSolicitud != 'TODOS') {
                tipoSolicitud = userTipoSolicitud;
            }
            var tipoRegistro = $("#cmbBuscarTipoRegistro").val();
            var userTipoRegistro = $("#hiddenUserTipoRegistro").val();
            if (tipoRegistro == 0 && userTipoRegistro != 'TODOS') {
                tipoRegistro = userTipoRegistro;
            }
            var userRegion = 'TODOS';
            var razonSocial = $("#txtBuscarRazonSocial").val();
            var ano = 0;

            $.ajax({
                type: 'POST',
                url: "/Solicitudes/JsonBuscarSolicitudes",
                dataType: "json",
                data: {
                    rutProveedor: rutProveedor, idSolicitud: idSolicitud, tipoSolicitud: tipoSolicitud, estado: estado, tipoRegistro: tipoRegistro,
                    userRegion: userRegion, razonSocial: razonSocial, ano: ano, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                },
                success: function (datos) {
                    $scope.$apply(function () {
                        if (datos && datos !== "EMPTY") {
                            $scope.gridOptions.data = datos;
                            $('#alertMensajes').hide();
                            $('#btnDownloadExcel').show();
                        } else {
                            $scope.gridOptions.data = [];
                            $("#lblMensajeTablaSolicitudes").text('No se encontraron registros para los filtros aplicados');
                            $('#alertMensajes').show();
                            $('#btnDownloadExcel').hide();
                        }
                    });
                },
                complete: function () {
                    $('#btnBuscarSolicitudes').show();
                    $('#procesando').hide();
                }
            });
        };

        // --- Funciones Expuestas al Scope ---
        $scope.abrirModalSolicitud = function (solicitud) {
            $('#modalSolicitud').modal('show');
            window.CargarDatosSolicitud(solicitud.ID, false);
        };

        self.DownloadExcel = function () {
            // Llama a la función de ayuda que está fuera del controlador
            DownloadExcel($scope);
        };
    }

    // =========================================================================
    // 2. FUNCIONES DE AYUDA (Fuera del Controlador pero dentro de la IIFE)
    // =========================================================================

    function DownloadExcel($scope) {
        var lista = [];
        var filteredRows = $scope.gridApi.core.getVisibleRows($scope.gridApi.grid);

        for (var i in filteredRows) {
            var obj = filteredRows[i].entity;
            var solicitud = newSolicitud([
                obj.ID, obj.ID_SOL_PADRE, decode_utf8(obj.RUT), decode_utf8(obj.RAZON_SOCIAL),
                decode_utf8(obj.ESTADO), obj.TIPO_SOL, obj.TIPO_REG, decode_utf8(obj.REGION),
                obj.FECHA_RECEPCION, obj.FECHA_INGRESO, obj.FECHA_ANALISIS_TEC, obj.ESTADO_ANALISIS_TEC,
                obj.FECHA_ANALISIS_LEG, obj.ESTADO_ANALISIS_LEG, obj.FECHA_ANALISIS_CONTABLE,
                obj.ESTADO_ANALISIS_CONTABLE, obj.FECHA_RESPUESTA
            ]);
            lista.push(solicitud);
        }

        var json = JSON.stringify(lista);
        var args = {
            file: 'Listado de Solicitudes',
            stringDatos: json
        };

        var form = $('<form>', {
            "method": "post",
            "html": '<textarea name="data" style="display:none;">' + JSON.stringify(args) + '</textarea>',
            "action": '/Solicitudes/ReporteSolicitudes'
        }).appendTo($('#modalForm')).submit();

        form.remove();
    }

    function newSolicitud(obj) {
        obj = obj || [];
        return {
            'ID': obj[0],
            'ID_SOL_PADRE': obj[1],
            'RUT': obj[2],
            'RAZON_SOCIAL': obj[3],
            'ESTADO': obj[4],
            'TIPO_SOL': obj[5],
            'TIPO_REG': obj[6],
            'REGION': obj[7],
            'FECHA_RECEPCION': obj[8],
            'FECHA_INGRESO': obj[9],
            'FECHA_ANALISIS_TEC': obj[10] || '---',
            'ESTADO_ANALISIS_TEC': obj[11] || '---',
            'FECHA_ANALISIS_LEG': obj[12] || '---',
            'ESTADO_ANALISIS_LEG': obj[13] || '---',
            'FECHA_ANALISIS_CONTABLE': obj[14] || '---',
            'ESTADO_ANALISIS_CONTABLE': obj[15] || '---',
            'FECHA_RESPUESTA': obj[16] || '---'
        };
    }

    function decode_utf8(s) {
        try {
            return decodeURIComponent(escape(s));
        }
        catch (err) {
            return s;
        }
    }

    // =========================================================================
    // 3. DEFINICIÓN DEL MÓDULO DE ANGULAR
    // =========================================================================
    angular.module('gridSolicitudes', [
        'ngAnimate',
        'ngTouch',
        'ui.grid',
        'ui.grid.selection',
        'ui.grid.cellNav',
        'ui.grid.resizeColumns'
    ]).controller('MainCtrlSolicitudes', MainCtrlSolicitudes);

    MainCtrlSolicitudes.$inject = ['$scope', '$http', '$q', '$interval', 'uiGridConstants'];

})();