using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Identity.Web;

namespace LegacyWebApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<ActionResult> ViewProfile()
        {
            try
            {
                var graphServiceClient = this.GetGraphServiceClient();
                var user = await graphServiceClient.Me.Request().GetAsync();
                ViewBag.User = user;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View();
            }
        }
    }
}