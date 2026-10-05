$(document).ready(function () {
    CargarFiltroTipoRegistroMOP();
});



function CargarFiltroTipoRegistroMOP() {
    var codigoPadre = 2237;//TIPO DE REGISTRO
    $.ajax({
        type: 'POST',
        url: "/Solicitudes/JsonListarParametrosPorTipo",
        dataType: "json",
        data: { codigoPadre: codigoPadre, __RequestVerificationToken: $("[name='__RequestVerificationToken']").val() },
        success: function (datos) {
            if (datos === "EMPTY") {

            } else {
                $('#cmbBuscarTipoRegistro').empty();
                $("#cmbBuscarTipoRegistro").append($("<option value='' selected > (Seleccionar...) </option>"));
                $(datos).each(function () {
                    $("#cmbBuscarTipoRegistro").append($("<option></option>").val(this.CODIGO).html(this.DESCRIPCION));
                });

            }
        },
        error: function (jqXHR, textStatus, errorThrown) { }
    });
}



function MainCtrlRepEspecialidadesProveedores($scope, $http, $q, $interval, uiGridConstants) {
    //i18nService.setCurrentLang('es');

    var self = this;
    //DEFINICIÓN DE COLUMNAS DE LA GRILLA
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
        //paginationPageSizes: [25, 50, 100],
        //paginationPageSize: 25,
        //enableGridMenu: true,
        //gridMenuShowHideColumns: false,
        //enableSelectAll: true,
        //exporterMenuCsv: false,
        //exporterMenuExcel: false,

        onRegisterApi: function (gridApi) {
            self.gridApi = gridApi;
            $scope.gridApi = gridApi;
        }

    };



    $scope.gridOptions.columnDefs = [
        //{ name: 'IdSol', displayName: 'N°', type: 'number', width: '4%' },
        //{
        //    name: 'ID', displayName: 'N° Sol', type: 'number', width: '5%',
        //    //cellTemplate: '<center> <a href="" ng-click="grid.appScope.AbrirModalSolicitud(' + "'" + "{{row.entity.ID}}" + "'" + ')">  {{row.entity.ID}} </a>  </center>'
        //    cellTemplate: '<center> <a href="" ng-click="grid.appScope.AbrirModalSolicitud(row.entity)">  {{row.entity.ID}} </a>  </center>'
        //    //cellTemplate: '<center> <a href="" ng-click="grid.appScope.AbrirModalSolicitud(' + "{{row.entity.ID}}" + ')">  {{row.entity.ID}} </a> </center> '
        //},
        //{ name: 'IdSolPadre', displayName: 'Sol Padre', width: '5%' },
        { name: 'Rut', displayName: 'RUT', width: '8%' },
        { name: 'RazonSocialProveedor', displayName: 'Razón Social', width: '30%' },
        { name: 'TipoRegistro', displayName: 'Tipo Registro', width: '15%' },
        { name: 'CodTipoEspecialidad', displayName: 'Cod Especialidad', width: '12%' },
        { name: 'TipoEspecialidad', displayName: 'Especialidad', width: '15%' },
        { name: 'TipoCategoria', displayName: 'Categoría', width: '8%' },
        { name: 'CapacidadEconomica', displayName: 'Cap. Económica', type: 'number', width: '8%' },
        //{ name: 'COD_REGION', displayName: 'Región', type: 'number', width: '6%' },
        { name: 'EstadoProveedor', displayName: 'Estado Proveedor', width: '8%' },
        { name: 'FechaIngresoReg', displayName: 'Fecha Ingreso', type: 'date', width: '9%' },
        { name: 'FechaSuspension', displayName: 'Fecha Renovación', type: 'date', width: '9%' },
        { name: 'NombreMesRenovacion', displayName: 'Mes Renovación', width: '9%' },
        { name: 'FechaEstadoFinanciero', displayName: 'Fecha Estado Financiero', type: 'date', width: '9%' },
        { name: 'Region', displayName: 'Región', width: '9%' }

    ];

    $scope.uiGridConstants = uiGridConstants;


    self.$scope = $scope;

    //$.ajax({
    //    type: 'POST',
    //    url: "/ReporteEspecialidadesProveedores/JsonBuscarReporteEspecialidadesProveedores",
    //    dataType: "json",
    //    data: {
    //        __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
    //    },
    //    success: function (datos) {
    //        $scope.gridOptions.data = datos;
    //        $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

    //        $("#divCargando").css('display', 'none');

    //    },
    //    error: function (jqXHR, textStatus, errorThrown) { }
    //});



    self.BuscarRepEspecialidadesProveedores = function () {


        var codTipoRegistro = $("#cmbBuscarTipoRegistro").val();
        //console.log('codTipoRegistro ' + codTipoRegistro);


        if (codTipoRegistro == '') {
            //console.log('sin datos');
            alert('Debe seleccionar un Tipo de Registro');
        } else {
            $('#btnBuscarRepEspecialidadesProveedores').css('display', 'none');
            $('#procesandoBuscarRepEspecialidadesProveedores').css('display', 'block');



            $.ajax({
                type: 'POST',
                url: "/ReporteEspecialidadesProveedores/JsonBuscarReporteEspecialidadesProveedores",
                dataType: "json",
                data: {
                    codTipoRegistro: codTipoRegistro,
                    __RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
                },
                success: function (datos) {
                    $scope.gridOptions.data = datos;
                    $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

                    //$("#divCargando").css('display', 'none');

                    $('#btnBuscarRepEspecialidadesProveedores').css('display', 'block');
                    $('#procesandoBuscarRepEspecialidadesProveedores').css('display', 'none');

                },
                error: function (jqXHR, textStatus, errorThrown) { }
            });

        }

        $('#btnBuscarRepEspecialidadesProveedores').css('display', 'block');
        $('#procesandoBuscarRepEspecialidadesProveedores').css('display', 'none');

        


    }




    self.DownloadExcel = function () {
        DownloadExcel($scope);
    };

}



function DownloadExcel($scope) {
    //$('#btnDownloadExcel').css('display', 'none');
    //$('#btnProcesandoExcel').css('display', 'block');
    var lista = [];
    //console.log('asdfdsfasdf');
    var filteredRows = $scope.gridApi.core.getVisibleRows($scope.gridApi.grid);
    //var filteredRows = $scope.gridOptions.data;
    //console.log(filteredRows);

    for (var i in filteredRows) {

        var obj = filteredRows[i].entity;
        //var obj = filteredRows[i];

        var reporte = newReporte([
            obj.Rut,
            obj.RazonSocialProveedor,
            decode_utf8(obj.TipoRegistro),
            decode_utf8(obj.CodTipoEspecialidad),
            decode_utf8(obj.TipoEspecialidad),
            decode_utf8(obj.TipoCategoria),
            obj.CapacidadEconomica,
            decode_utf8(obj.EstadoProveedor),
            obj.FechaIngresoReg,
            obj.FechaSuspension,
            obj.NombreMesRenovacion,
            obj.FechaEstadoFinanciero,
            obj.Region
        ]);
        lista.push(reporte);
    }
    var json = JSON.stringify(lista);
    var args = {
        file: 'Especialidades de Proveedores',
        //base64: btoa(json)
        stringDatos: json
    };
    var form = $('<form>', {
        "id": "downloadReporteEspecialidadesProveedores",
        "method": "post",
        "html": '<textarea id="data" name="data" style="display:none;">#1</textarea>'.replace('#1', JSON.stringify(args)),
        "action": /*window.siteRoot +*/ '/ReporteEspecialidadesProveedores/ReporteEspecialidadesProveedores'
    }).appendTo($('#modalForm')).submit();
    form.remove();

    //console.log(lista);

    //$('#btnDownloadExcel').css('display', 'block');
    //$('#btnProcesandoExcel').css('display', 'none');

}


function newReporte(obj = undefined) {
    return {
        'Rut': obj?.length > 0 ? obj[0] : undefined,
        'RazonSocialProveedor': obj?.length > 0 ? obj[1] : undefined,
        'TipoRegistro': obj?.length > 0 ? obj[2] : undefined,
        'CodTipoEspecialidad': obj?.length > 0 ? obj[3] : undefined,
        'TipoEspecialidad': obj?.length > 0 ? obj[4] : undefined,
        'TipoCategoria': obj?.length > 0 ? obj[5] : undefined,
        'CapacidadEconomica': obj?.length > 0 ? obj[6] : undefined,
        'EstadoProveedor': obj?.length > 0 ? obj[7] : undefined,
        'FechaIngresoReg': obj?.length > 0 ? obj[8] : undefined,
        'FechaSuspension': obj?.length > 0 ? obj[9] : undefined,
        'NombreMesRenovacion': obj?.length > 0 ? obj[10] : undefined,
        'FechaEstadoFinanciero': obj?.length > 0 ? obj[11] : undefined,
        'Region': obj?.length > 0 ? obj[12] : undefined
    }
}

function decode_utf8(s) {
    try {
        return decodeURIComponent(escape(s));
    }
    catch (err) {
        return s;
    }
}

var app = angular.module('gridRepEspecialidadesProveedores',
    [
        //'ngAria',
        'ngAnimate',
        'ngTouch',
        'ui.grid',
        /*'ui.grid.edit',*/
        'ui.grid.selection',
        'ui.grid.cellNav',
        'ui.grid.resizeColumns'//,
        //'ui.grid.pagination'
    ]).controller('MainCtrlRepEspecialidadesProveedores', MainCtrlRepEspecialidadesProveedores);





MainCtrlRepEspecialidadesProveedores.$inject = ['$scope', '$http', '$q', '$interval', 'uiGridConstants'];





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
