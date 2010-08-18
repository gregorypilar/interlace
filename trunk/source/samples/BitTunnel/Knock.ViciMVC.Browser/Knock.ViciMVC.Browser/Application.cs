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
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary;
using Knock.ViciMVC.Browser.Knock;

namespace Knock.ViciMVC.Browser
{
    public class Application
    {                
        public static void Init()
        {
            WebAppConfig.Router.AddDefaultRoutes(null);
        }
    }
}
