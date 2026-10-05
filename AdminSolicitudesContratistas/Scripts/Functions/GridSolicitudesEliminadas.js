




function MainCtrlSolicitudesEliminadas($scope, $http, $q, $interval, uiGridConstants) {
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
        { name: 'IdSol', displayName: 'N°', type: 'number', width: '4%' },
        //{
        //    name: 'ID', displayName: 'N° Sol', type: 'number', width: '5%',
        //    //cellTemplate: '<center> <a href="" ng-click="grid.appScope.AbrirModalSolicitud(' + "'" + "{{row.entity.ID}}" + "'" + ')">  {{row.entity.ID}} </a>  </center>'
        //    cellTemplate: '<center> <a href="" ng-click="grid.appScope.AbrirModalSolicitud(row.entity)">  {{row.entity.ID}} </a>  </center>'
        //    //cellTemplate: '<center> <a href="" ng-click="grid.appScope.AbrirModalSolicitud(' + "{{row.entity.ID}}" + ')">  {{row.entity.ID}} </a> </center> '
        //},
        { name: 'IdSolPadre', displayName: 'Sol Padre', width: '5%' },
        { name: 'RutProveedor', displayName: 'RUT', width: '6%' },
        { name: 'RazonSocial', displayName: 'Razón Social', width: '23%' },
        { name: 'Estado', displayName: 'Estado Original', width: '7%' },
        { name: 'TipoSol', displayName: 'Tipo Solicitud', width: '7%' },
        { name: 'TipoReg', displayName: 'Tipo Registro', width: '10%' },
        { name: 'Region', displayName: 'Región', width: '8%' },
        //{ name: 'COD_REGION', displayName: 'Región', type: 'number', width: '6%' },
        { name: 'FechaCreacion', displayName: 'Fecha Creación', width: '6%' },
        { name: 'FechaEliminacion', displayName: 'Fecha Eliminación', type: 'date', width: '7%' },
        { name: 'FuncionarioResponsable', displayName: 'Responsable', width: '8%' },
        { name: 'MotivoEliminacion', displayName: 'Motivo Eliminación', width: '20%' }
        
    ];

    $scope.uiGridConstants = uiGridConstants;


    self.$scope = $scope;

    $.ajax({
        type: 'POST',
        url: "/SolicitudesEliminadas/JsonBuscarSolicitudesEliminadas",
        dataType: "json",
        data: {__RequestVerificationToken: $("[name='__RequestVerificationToken']").val()
        },
        success: function (datos) {

            var msg = "";

            $scope.gridOptions.data = datos;
            $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);

            //if (datos === "EMPTY") {
            //    //MostrarInfoTablaSinDatos('SIN DATOS PARA ESTA OPCIÓN');
            //    //TablaHistorialSolicitudesVacia();
            //    msg = 'No se encontraron registros para los filtros aplicados';
            //    //se carga Data vacía
            //    $scope.gridOptions.data = [];
            //    $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
            //    $('#procesando').css('display', 'none');
            //    $('#alertMensajes').css('display', 'block');
            //    $('#btnBuscarSolicitudes').css('display', 'block');
            //    $('#btnDownloadExcel').css('display', 'none');
            //} else {

            //    $scope.gridOptions.data = datos;
            //    $scope.gridApi.core.notifyDataChange(uiGridConstants.dataChange.ALL);
            //    $('#procesando').css('display', 'none');
            //    $('#alertMensajes').css('display', 'none');
            //    $('#btnBuscarSolicitudes').css('display', 'block');
            //    $('#btnDownloadExcel').css('display', 'block');
            //}



            //$("#lblMensajeTablaSolicitudes").text(msg);

        },
        error: function (jqXHR, textStatus, errorThrown) { }
    });

    


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

        var solicitud = newSolicitud([
            obj.IdSol,
            obj.IdSolPadre,
            decode_utf8(obj.RutProveedor),
            decode_utf8(obj.RazonSocial),
            decode_utf8(obj.Estado),
            obj.TipoSol,
            obj.TipoReg,
            decode_utf8(obj.Region),
            obj.FechaCreacion,
            obj.FechaEliminacion,
            obj.FuncionarioResponsable,
            obj.MotivoEliminacion
        ]);
        lista.push(solicitud);
    }
    var json = JSON.stringify(lista);
    var args = {
        file: 'Solicitudes Eliminadas',
        //base64: btoa(json)
        stringDatos: json
    };
    var form = $('<form>', {
        "id": "downloadReportSolicitudes",
        "method": "post",
        "html": '<textarea id="data" name="data" style="display:none;">#1</textarea>'.replace('#1', JSON.stringify(args)),
        "action": /*window.siteRoot +*/ '/SolicitudesEliminadas/ReporteSolicitudesEliminadas'
    }).appendTo($('#modalForm')).submit();
    form.remove();

    //console.log(lista);

    //$('#btnDownloadExcel').css('display', 'block');
    //$('#btnProcesandoExcel').css('display', 'none');

}


function newSolicitud(obj = undefined) {
    return {
        'IdSol': obj?.length > 0 ? obj[0] : undefined,
        'IdSolPadre': obj?.length > 0 ? obj[1] : undefined,
        'RutProveedor': obj?.length > 0 ? obj[2] : undefined,
        'RazonSocial': obj?.length > 0 ? obj[3] : undefined,
        'Estado': obj?.length > 0 ? obj[4] : undefined,
        'TipoSol': obj?.length > 0 ? obj[5] : undefined,
        'TipoReg': obj?.length > 0 ? obj[6] : undefined,
        'Region': obj?.length > 0 ? obj[7] : undefined,
        'FechaCreacion': obj?.length > 0 ? obj[8] : undefined,
        'FechaEliminacion': obj?.length > 0 ? obj[9] : undefined,
        'FuncionarioResponsable': obj[10]?.length > 0 ? obj[10] : '---',
        'MotivoEliminacion': obj[11]?.length > 0 ? obj[11] : '---'
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

var app = angular.module('gridSolicitudesEliminadas',
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
    ]).controller('MainCtrlSolicitudesEliminadas', MainCtrlSolicitudesEliminadas);





MainCtrlSolicitudesEliminadas.$inject = ['$scope', '$http', '$q', '$interval', 'uiGridConstants'];





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
