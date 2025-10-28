$(document).ready(function(){
   
    $(".select2_single").select2({
        placeholder: "Select a state",
        allowClear: true
    });
    $(".select2_group").select2({});
    $(".select2_multiple").select2({

        allowClear: true
    });
        $('#wizard').smartWizard();

        function onFinishCallback() {
            $('#wizard').smartWizard('showMessage', 'Finish Clicked');
            //alert('Finish Clicked');
        }
    });

    $(document).ready(function () {
        // Smart Wizard
        $('#wizard_verticle').smartWizard({
            transitionEffect: 'slide'
        });


    //Datatable
    $('#datatable').dataTable();
       
        $('#datatable-responsive').DataTable();
      
        $('#datatable-scroller').DataTable({
            ajax: "js/datatables/json/scroller-demo.json",
            deferRender: true,
            scrollY: 380,
            scrollCollapse: true,
            scroller: true
        });
        var table = $('#datatable-fixed-header').DataTable({
            fixedHeader: true
        });
        
        $("#slideshow > div:gt(0)").hide();

                setInterval(function () {
                    $('#slideshow > div:first')
                        .fadeOut(1000)
                        .next()
                        .fadeIn(1000)
                        .end()
                        .appendTo('#slideshow');
                }, 3000);


      //DataTable
      var handleDataTableButtons = function () {
      "use strict";
      0 !== $("#datatable-buttons").length && $("#datatable-buttons").DataTable({
          dom: "Bfrtip",
          buttons: [{
              extend: "copy",
              className: "btn-sm"
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
          responsive: !0
      })
  },
      TableManageButtons = function () {
          "use strict";
          return {
              init: function () {
                  handleDataTableButtons()
              }
          }
      }();


   //   function init() {
   //    var map = new google.maps.Map(document.getElementById('map-canvas'), {
   //        center: {
   //            lat: 12.9715987,
   //            lng: 77.59456269999998
   //        },
   //        zoom: 12
   //    });


   //    var searchBox = new google.maps.places.SearchBox(document.getElementById('pac-input'));
   //    map.controls[google.maps.ControlPosition.TOP_CENTER].push(document.getElementById('pac-input'));
   //    google.maps.event.addListener(searchBox, 'places_changed', function () {
   //        searchBox.set('map', null);


   //        var places = searchBox.getPlaces();

   //        var bounds = new google.maps.LatLngBounds();
   //        var i, place;
   //        for (i = 0; place = places[i]; i++) {
   //            (function (place) {
   //                var marker = new google.maps.Marker({

   //                    position: place.geometry.location
   //                });
   //                marker.bindTo('map', searchBox, 'map');
   //                google.maps.event.addListener(marker, 'map_changed', function () {
   //                    if (!this.getMap()) {
   //                        this.unbindAll();
   //                    }
   //                });
   //                bounds.extend(place.geometry.location);


   //            }(place));

   //        }
   //        map.fitBounds(bounds);
   //        searchBox.set('map', map);
   //        map.setZoom(Math.min(map.getZoom(), 12));

   //    });
   //}
  // google.maps.event.addDomListener(window, 'load', init);
        
      $(".select2_single").select2({
          placeholder: "Select a state",
          allowClear: true
      });
      $(".select2_group").select2({});
      $(".select2_multiple").select2({

          allowClear: true
      });

      NProgress.done();


            var cb = function (start, end, label) {
                console.log(start.toISOString(), end.toISOString(), label);
                $('#reportrange span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
                //alert("Callback has fired: [" + start.format('MMMM D, YYYY') + " to " + end.format('MMMM D, YYYY') + ", label = " + label + "]");
            }

            var optionSet1 = {
                startDate: moment().subtract(29, 'days'),
                endDate: moment(),
                minDate: '01/01/2012',
                maxDate: '12/31/2015',
                dateLimit: {
                    days: 60
                },
                showDropdowns: true,
                showWeekNumbers: true,
                timePicker: false,
                timePickerIncrement: 1,
                timePicker12Hour: true,
                ranges: {
                    'Today': [moment(), moment()],
                    'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                    'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                    'Last 30 Days': [moment().subtract(29, 'days'), moment()],
                    'This Month': [moment().startOf('month'), moment().endOf('month')],
                    'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
                },
                opens: 'left',
                buttonClasses: ['btn btn-default'],
                applyClass: 'btn-small btn-primary',
                cancelClass: 'btn-small',
                format: 'MM/DD/YYYY',
                separator: ' to ',
                locale: {
                    applyLabel: 'Submit',
                    cancelLabel: 'Clear',
                    fromLabel: 'From',
                    toLabel: 'To',
                    customRangeLabel: 'Custom',
                    daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                    monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                    firstDay: 1
                }
            };
            $('#reportrange span').html(moment().subtract(29, 'days').format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));
            $('#reportrange').daterangepicker(optionSet1, cb);
            $('#reportrange').on('show.daterangepicker', function () {
                console.log("show event fired");
            });
            $('#reportrange').on('hide.daterangepicker', function () {
                console.log("hide event fired");
            });
            $('#reportrange').on('apply.daterangepicker', function (ev, picker) {
                console.log("apply event fired, start/end dates are " + picker.startDate.format('MMMM D, YYYY') + " to " + picker.endDate.format('MMMM D, YYYY'));
            });
            $('#reportrange').on('cancel.daterangepicker', function (ev, picker) {
                console.log("cancel event fired");
            });
            $('#options1').click(function () {
                $('#reportrange').data('daterangepicker').setOptions(optionSet1, cb);
            });
            $('#options2').click(function () {
                $('#reportrange').data('daterangepicker').setOptions(optionSet2, cb);
            });
            $('#destroy').click(function () {
                $('#reportrange').data('daterangepicker').remove();
            });


            $('#reservation2').daterangepicker(null, function (start, end, label) {
            console.log(start.toISOString(), end.toISOString(), label);
        });
        $('#reservation').daterangepicker(null, function (start, end, label) {
            console.log(start.toISOString(), end.toISOString(), label);
        });
        $('#reservation3').daterangepicker(null, function (start, end, label) {
            console.log(start.toISOString(), end.toISOString(), label);
        });
        $('#single_cal2').daterangepicker({
            singleDatePicker: true,
            calender_style: "picker_2"
        }, function (start, end, label) {
            console.log(start.toISOString(), end.toISOString(), label);
        });

        // [17, 74, 6, 39, 20, 85, 7]
                 //[82, 23, 66, 9, 99, 6, 2]
                 var data1 = [
                     [gd(2012, 1, 1), 17],
                     [gd(2012, 1, 2), 74],
                     [gd(2012, 1, 3), 6],
                     [gd(2012, 1, 4), 39],
                     [gd(2012, 1, 5), 20],
                     [gd(2012, 1, 6), 85],
                     [gd(2012, 1, 7), 7]
                 ];

                 var data2 = [
                     [gd(2012, 1, 1), 82],
                     [gd(2012, 1, 2), 23],
                     [gd(2012, 1, 3), 66],
                     [gd(2012, 1, 4), 9],
                     [gd(2012, 1, 5), 119],
                     [gd(2012, 1, 6), 6],
                     [gd(2012, 1, 7), 9]
                 ];
                 $("#canvas_dahs").length && $.plot($("#canvas_dahs"), [
                     data1, data2
                 ], {
                         series: {
                             lines: {
                                 show: false,
                                 fill: true
                             },
                             splines: {
                                 show: true,
                                 tension: 0.4,
                                 lineWidth: 1,
                                 fill: 0.4
                             },
                             points: {
                                 radius: 0,
                                 show: true
                             },
                             shadowSize: 2
                         },
                         grid: {
                             verticalLines: true,
                             hoverable: true,
                             clickable: true,
                             tickColor: "#d5d5d5",
                             borderWidth: 1,
                             color: '#fff'
                         },
                         colors: ["rgba(38, 185, 154, 0.38)", "rgba(3, 88, 106, 0.38)"],
                         xaxis: {
                             tickColor: "rgba(51, 51, 51, 0.06)",
                             mode: "time",
                             tickSize: [1, "day"],
                             //tickLength: 10,
                             axisLabel: "Date",
                             axisLabelUseCanvas: true,
                             axisLabelFontSizePixels: 12,
                             axisLabelFontFamily: 'Verdana, Arial',
                             axisLabelPadding: 10
                             //mode: "time", timeformat: "%m/%d/%y", minTickSize: [1, "day"]
                         },
                         yaxis: {
                             ticks: 8,
                             tickColor: "rgba(51, 51, 51, 0.06)",
                         },
                         tooltip: false
                     });

                 function gd(year, month, day) {
                     return new Date(year, month - 1, day).getTime();
                 }




                 $('#world-map-gdp').vectorMap({
               map: 'world_mill_en',
               backgroundColor: 'transparent',
               zoomOnScroll: false,
               series: {
                   regions: [{
                       values: gdpData,
                       scale: ['#E6F2F0', '#149B7E'],
                       normalizeFunction: 'polynomial'
                   }]
               },
               onRegionTipShow: function (e, el, code) {
                   el.html(el.html() + ' (GDP - ' + gdpData[code] + ')');
               }
           });

           var data = {
           labels: [
               "Symbian",
               "Blackberry",
               "Other",
               "Android",
               "IOS"
           ],
           datasets: [{
               data: [15, 20, 30, 10, 30],
               backgroundColor: [
                   "#BDC3C7",
                   "#9B59B6",
                   "#455C73",
                   "#26B99A",
                   "#3498DB"
               ],
               hoverBackgroundColor: [
                   "#CFD4D8",
                   "#B370CF",
                   "#34495E",
                   "#36CAAB",
                   "#49A9EA"
               ]

           }]
       };
       var icons = new Skycons({
           "color": "#73879C"
       }),
           list = [
               "clear-day", "clear-night", "partly-cloudy-day",
               "partly-cloudy-night", "cloudy", "rain", "sleet", "snow", "wind",
               "fog"
           ],
           i;

       for (i = list.length; i--;)
           icons.set(list[i], list[i]);

       icons.play();

});
