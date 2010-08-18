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
using Knock.ViciMVC.Browser.Knock;
using Vici.Mvc;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace Knock.ViciMVC.Browser.Controllers
{
    public class AppSettingsDetails : Controller
    {
        public void Run()
        {
            SetStatus();

            AppSettingsForm form = new AppSettingsForm();
            form.Bind();
            if (form.Validated)
            {
                OnValidatedPostBack(form);

            }
            else if (form.ValidationResult.Success)//Not validate but Success = not postback
            {
                form.IPAddress = KnockClientManager.Instance.ServerAddress.ToString();
                form.Port = KnockClientManager.Instance.Port;
            }
            else
            {
                StringBuilder builder = new StringBuilder();

                foreach (string message in form.ValidationResult.Messages.Select(m => m.Message))
                {
                    builder.AppendFormat("{0}{1}", builder.Length > 0 ? ", " : "", message);
                }

                ViewData["Error"] = string.Format("The following validation errors were detected: {0}", builder.ToString());
            }
        }

        private void OnValidatedPostBack(AppSettingsForm form)
        {
            if (KnockClientManager.Instance.ClientStatus == KnockClientManager.Status.Connected)
            {
                Disconnect();
            }
            else
            {
                IPAddress address;
                if (IPAddress.TryParse(form.IPAddress, out address))
                {
                    KnockClientManager.Instance.ServerAddress = address;
                    KnockClientManager.Instance.Port = form.Port;

                    KnockClientManager.Instance.Connect();

                    SetStatus();

                    LoadFileList();
                }
                else
                {
                    ViewData["Error"] = "Unable to translate IP Address";
                }
            }
        }

        private void LoadFileList()
        {
            List<DirectoryWrapper> files = KnockClientManager.Instance.GetFileList();
        
            if (files != null)
            {
                ViewData["FileList"] = files;
            }
            else
            {
                ViewData["Error"] = "Unable to retrieve file list - timeout";
            }
        }

        private void Disconnect()
        {
            KnockClientManager.Instance.Disconnect();

            SetStatus();
        }

        private void SetStatus()
        {
            ViewData["Status"] = GetStatus();
        }

        public static string GetStatus()
        {
            return KnockClientManager.Instance.ClientStatus == KnockClientManager.Status.Connected ? "Connected" : "Unconnected";
        }
    }

    public class AppSettingsForm : WebForm
    {
        protected override void OnFill()
        {
            IPAddress = KnockClientManager.Instance.ServerAddress == null ? "127.0.0.1" : KnockClientManager.Instance.ServerAddress.ToString();
            Port = KnockClientManager.Instance.Port;
        }

        [ValidateRegEx(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b", Message="Bad IPAddress")]
        [FormTextBox(MaxLength=15)]
        public string IPAddress;

        [ValidateRange(1, 65535, Message="Bad Port")]
        [FormTextBox()]
        public int Port;
    }
}
