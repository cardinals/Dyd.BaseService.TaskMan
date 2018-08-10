using Dyd.BaseService.TaskManager.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Dyd.BaseService.TaskManager.Web.Controllers
{
    [AuthorityCheck]
    public class HostController : BaseWebController
    {
        //
        // GET: /Host/

        public ActionResult Index()
        {
            return View();
        }

    }
}
