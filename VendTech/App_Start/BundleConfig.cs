using System.Web;
using System.Web.Optimization;

namespace VendTech
{

    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                 "~/Scripts/jquery-1.10.2.js",
                   "~/Scripts/jquery-migrate-1.2.1.js"
               ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                      "~/Scripts/jquery.unobtrusive*",
                      "~/Scripts/jquery.validate*"));


            bundles.Add(new ScriptBundle("~/bundles/custom").Include(
                        "~/Scripts/ajax-form.js",
                        "~/Scripts/html5.js",
                         "~/Scripts/UserDefinedScripts/Common.js",
                          "~/Content/themes/jquery.datatables/jqueryDataTable.js",
						 "~/Content/themes/jquery.datatables/bootstrap-adapter/js/datatables.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                             "~/Content/themes/bootstrap/dist/js/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/themeSpecific").Include(
                 "~/Content/themes/behaviour/general.js",
                            "~/Content/themes/behaviour/voice-commands.js",
                            "~/Content/themes/jquery.flot/jquery.flot.js",
                            "~/Content/themes/jquery.flot/jquery.flot.pie.js",
                             "~/Content/themes/jquery.flot/jquery.flot.resize.js",
                              "~/Content/themes/jquery.flot/jquery.flot.labels.js"
                            ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));



            bundles.Add(new StyleBundle("~/Content/beforeLoginCss").Include(
                      "~/Content/themes/bootstrap/dist/css/bootstrap.css",
                      "~/Content/fonts/font-awesome-4/css/font-awesome.min.css",
                     "~/Content/themes/jquery.gritter/css/jquery.gritter.css",
                      "~/Content/style.css"));

            bundles.Add(new StyleBundle("~/Content/beforeLoginAdminCss").Include(
                     "~/Content/themes/bootstrap/dist/css/bootstrap.css",
                     "~/Content/fonts/font-awesome-4/css/font-awesome.min.css",
                     "~/Content/themes/jquery.gritter/css/jquery.gritter.css",
                     "~/Content/adminStyle.css"));

            bundles.Add(new StyleBundle("~/Content/multiSelectCss").Include(
                     "~/Content/themes/bootstrap.multiselect/css/bootstrap-multiselect.css",
                     "~/Content/fonts/jquery.multiselect/css/multi-select.css"));

            bundles.Add(new ScriptBundle("~/bundles/multiSelectjs").Include(
                            "~/Content/themes/bootstrap.multiselect/js/bootstrap-multiselect.js",
                            "~/Content/themes/jquery.multiselect/js/jquery.multi-select.js"
                            ));

            bundles.Add(new StyleBundle("~/Content/layoutCSS").Include(
                     "~/Content/themes/bootstrap/dist/css/bootstrap.css",
                     "~/Content/fonts/font-awesome-4/css/font-awesome.min.css",
                     "~/Content/themes/jquery.gritter/css/jquery.gritter.css",
                     "~/Content/themes/jquery.nanoscroller/nanoscroller.css",
                     "~/Content/themes/jquery.easypiechart/jquery.easy-pie-chart.css",
                     "~/Content/themes/bootstrap.switch/bootstrap-switch.css",
                     "~/Content/themes/bootstrap.datetimepicker/css/bootstrap-datetimepicker.min.css",
                     "~/Content/themes/jquery.select2/select2.css",
                     "~/Content/themes/bootstrap.slider/css/slider.css",
                     "~/Content/themes/intro.js/introjs.css",
                     "~/Content/themes/jquery.vectormaps/jquery-jvectormap-1.2.2.css",
                     "~/Content/themes/jquery.magnific-popup/dist/magnific-popup.css",
                     "~/Content/themes/jquery.niftymodals/css/component.css",
                     "~/Content/themes/intro.js/bootstrap.summernote/dist/summernote.css",
                     "~/Content/style.css"));

            bundles.Add(new StyleBundle("~/Content/adminLayoutCSS").Include(
                   "~/Content/themes/bootstrap/dist/css/bootstrap.css",
                   "~/Content/fonts/font-awesome-4/css/font-awesome.min.css",
                   "~/Content/themes/jquery.gritter/css/jquery.gritter.css",
                   "~/Content/themes/jquery.nanoscroller/nanoscroller.css",
                   "~/Content/themes/jquery.easypiechart/jquery.easy-pie-chart.css",
                   "~/Content/themes/bootstrap.switch/bootstrap-switch.css",
                   "~/Content/themes/bootstrap.datetimepicker/css/bootstrap-datetimepicker.min.css",
                   "~/Content/themes/jquery.select2/select2.css",
                   "~/Content/themes/bootstrap.slider/css/slider.css",
                   "~/Content/themes/intro.js/introjs.css",
                   "~/Content/themes/jquery.vectormaps/jquery-jvectormap-1.2.2.css",
                   "~/Content/themes/jquery.magnific-popup/dist/magnific-popup.css",
                   "~/Content/themes/jquery.niftymodals/css/component.css",
                   "~/Content/themes/intro.js/bootstrap.summernote/dist/summernote.css",
                    "~/Content/themes/jquery.datatables/bootstrap-adapter/css/datatables.css",
                    "~/Content/sweetalert.css",
                   "~/Content/adminStyle.css"));

            bundles.Add(new ScriptBundle("~/bundles/extendedjs").Include(
                 "~/Content/themes/jquery.gritter/js/jquery.gritter.js",
                 "~/Content/themes/jquery.nanoscroller/jquery.nanoscroller.js",

                 "~/Content/themes/jquery.ui/jquery-ui.js",
                 "~/Content/themes/jquery.sparkline/jquery.sparkline.min.js",
                 "~/Content/themes/jquery.easypiechart/jquery.easy-pie-chart.js",
                   "~/Content/themes/jquery.nestable/jquery.nestable.js",
                 "~/Content/themes/bootstrap.switch/bootstrap-switch.min.js",
                 "~/Content/themes/bootstrap.datetimepicker/js/bootstrap-datetimepicker.js",
                //"~/Content/themes/bootstrap.datetimepicker/js/bootstrap-datetimepicker.min.js",
                //"~/Content/themes/jquery.select2/select2.min.js",
                 "~/Content/themes/jquery.select2/select2.js",
                 "~/Content/themes/skycons/skycons.js",
                 "~/Content/themes/bootstrap.slider/js/bootstrap-slider.js",
                 "~/Content/themes/jquery.fullcalendar/fullcalendar/fullcalendar.js",
                 "~/Content/themes/jquery.niftymodals/js/jquery.modalEffects.js",
                 "~/Content/themes/bootstrap.summernote/dist/summernote.min.js",
                 "~/Content/themes/jquery.vectormaps/jquery-jvectormap-1.2.2.min.js",
                 "~/Content/themes/jquery.vectormaps/maps/jquery-jvectormap-us-merc-en.js",
                 "~/Content/themes/jquery.vectormaps/maps/jquery-jvectormap-world-mill-en.js",
                 "~/Content/themes/jquery.vectormaps/maps/jquery-jvectormap-fr-merc-en.js",
                 "~/Content/themes/jquery.vectormaps/maps/jquery-jvectormap-uk-mill-en.js",
                 "~/Content/themes/jquery.vectormaps/maps/jquery-jvectormap-us-il-chicago-mill-en.js",
                 "~/Content/themes/jquery.vectormaps/maps/jquery-jvectormap-au-mill-en.js",
                 "~/Content/themes/jquery.vectormaps/maps/jquery-jvectormap-in-mill-en.js",
                 "~/Content/themes/jquery.vectormaps/maps/jquery-jvectormap-map.js",
                 "~/Content/themes/jquery.vectormaps/maps/jquery-jvectormap-ca-lcc-en.js",
                 "~/Scripts/sweetalert.min.js"
                ));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = false;
        }
       
    }
}
