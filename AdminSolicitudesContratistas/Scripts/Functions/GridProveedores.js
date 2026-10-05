// Scripts/Functions/GridProveedores.js
(function () {
    'use strict';

    function MainCtrlProveedores($scope, $http, $q, $interval, uiGridConstants) {
        var self = this;

        // =================================================================
        // CONFIGURACIÓN DE LA GRILLA
        // =================================================================
        $scope.gridOptions = {
            enableFiltering: true,
            enableRowSelection: true,
            enableRowHeaderSelection: false,
            multiSelect: false,
            showGridFooter: true,
            onRegisterApi: function (gridApi) {
                $scope.gridApi = gridApi;
            },
            columnDefs: [
                { name: 'Rut', displayName: 'RUT', width: '8%', cellTemplate: '<div class="ui-grid-cell-contents text-center"><a href="" ng-click="grid.appScope.abrirModalProveedor(row.entity)">{{row.entity.Rut}}</a></div>' },
                { name: 'RazonSocial', displayName: 'Razón Social', width: '30%' },
                { name: 'Region', displayName: 'Región', width: '20%' },
                { name: 'EstadoMOP', displayName: 'Estado MOP', width: '8%' },
                { name: 'CodTipoRegistro', displayName: 'Cod Tipo Registro', visible: false },
                { name: 'TipoRegistro', displayName: 'Tipo Registro', width: '15%' },
                { name: 'MesRenovacion', displayName: 'Mes de Renovación', width: '10%' },
                { name: 'FechaIngresoReg', displayName: 'Fecha Ingreso Registro', width: '18%', cellFilter: 'date:\'dd/MM/yyyy\'' }
            ]
        };

        // =================================================================
        // LÓGICA DE DATOS
        // =================================================================
        self.BuscarProveedores = function () {
            $('#btnBuscarProveedores').hide();
            $('#procesandoBuscarProveedores').show();

            var rutProveedor = $("#txtBuscarRutProveedor").val().trim();
            var razonSocial = $("#txtBuscarRazonSocial").val().trim();
            var codEstadoMOP = $("#cmbBuscarEstadoProveedor").val();
            var region = $("#cmbBuscarRegion").val() || 0;
            var tipoRegistro = $("#cmbBuscarTipoRegistro").val();

            $.ajax({
                type: 'POST',
                url: "/Proveedores/JsonTraerProveedores",
                dataType: "json",
                data: { rutProveedor: rutProveedor, razonSocial: razonSocial, codEstadoMOP: codEstadoMOP, region: region, tipoRegistro: tipoRegistro, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
                success: function (datos) {
                    $scope.$apply(function () {
                        if (datos !== "EMPTY") {
                            $scope.gridOptions.data = datos;
                            $('#alertMensajes').hide();
                            $('#btnDownloadExcelProv').show();
                        } else {
                            $scope.gridOptions.data = [];
                            $("#lblMensajeTablaProveedores").text('No se encontraron registros para los filtros aplicados');
                            $('#alertMensajes').show();
                            $('#btnDownloadExcelProv').hide();
                        }
                    });
                },
                error: function () {
                    alert("Ocurrió un error al buscar proveedores.");
                },
                complete: function () {
                    $('#btnBuscarProveedores').show();
                    $('#procesandoBuscarProveedores').hide();
                }
            });
        };

        // =================================================================
        // FUNCIONES EXPUESTAS AL SCOPE
        // =================================================================
        $scope.abrirModalProveedor = function (proveedor) {
            $('#modalProveedor').modal('show');
            window.CargarDatosProveedor(proveedor);
        };

        self.DownloadExcel = function () {
            var listaParaExportar = $scope.gridApi.core.getVisibleRows($scope.gridApi.grid).map(function (row) {
                return {
                    Rut: row.entity.Rut,
                    RazonSocial: row.entity.RazonSocial,
                    Region: row.entity.Region,
                    EstadoMOP: row.entity.EstadoMOP,
                    TipoRegistro: row.entity.TipoRegistro,
                    MesRenovacion: row.entity.MesRenovacion,
                    FechaIngresoReg: moment(row.entity.FechaIngresoReg).isValid() ? moment(row.entity.FechaIngresoReg).format('DD/MM/YYYY') : ''
                };
            });

            //console.log('listaParaExportar');
            //console.log(listaParaExportar);

            if (listaParaExportar.length === 0) {
                alert("No hay datos para exportar.");
                return;
            }

            var args = {
                file: 'Listado de Proveedores',
                stringDatos: JSON.stringify(listaParaExportar)
            };

            var form = $('<form>', {
                "method": "post",
                "html": '<textarea name="data" style="display:none;">' + JSON.stringify(args) + '</textarea>',
                "action": '/Proveedores/ReporteProveedores'
            }).appendTo('body').submit();
            form.remove();
        };
    }

    // =================================================================
    // DEFINICIÓN DEL MÓDULO ANGULAR
    // =================================================================
    angular.module('app', ['ngAnimate', 'ngTouch', 'ui.grid', 'ui.grid.selection', 'ui.grid.cellNav', 'ui.grid.resizeColumns'])
        .controller('MainCtrl', MainCtrlProveedores); // Renombrado para claridad

    MainCtrlProveedores.$inject = ['$scope', '$http', '$q', '$interval', 'uiGridConstants'];

})();