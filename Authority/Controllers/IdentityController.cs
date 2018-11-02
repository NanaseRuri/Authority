using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Authority.Controllers
{
    public class IdentityController : Controller
    {
        // GET: Identity
        public ActionResult Index()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("Placeholder", "Placeholder");
            return View(data);
        }
    }
}