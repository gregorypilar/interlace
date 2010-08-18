using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Vici.Mvc;
using System.Collections.Generic;
using Knock.ViciMVC.Browser.Knock;

namespace Knock.ViciMVC.Browser.Controllers
{
    public class index : Controller
    {
        public void Run()
        {
            ViewData["Status"] = KnockClientManager.Instance.ClientStatus.ToString();

            if (KnockClientManager.Instance.ClientStatus != KnockClientManager.Status.Connected)
            {
                Redirect("AppSettingsDetails");
            }
            else
            {
                Redirect("browser");
            }
        }
    }
}
