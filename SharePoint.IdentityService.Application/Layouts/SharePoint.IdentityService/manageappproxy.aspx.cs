using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using MyCorp.SP.ServiceClients;

namespace MyCorp.SP.ServiceApplication.AdminLayoutPages
{
    public partial class ManageAppProxyPage : AdminLayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var isCentralAdmin = SPContext.Current.Site.WebApplication.IsAdministrationWebApplication;
            if (!isCentralAdmin)
                SPUtility.HandleAccessDenied(new InvalidOperationException());

            var idStr = Request.QueryString["id"];
            if (idStr.IsNullOrEmpty())
            {
                // If there is only one proxy, redirect to it
                var proxies = MCServiceUtility.GetApplicationProxies().ToArray();
                if (proxies.Any())
                {
                    Response.Redirect(string.Format("manageappproxy.aspx?id={0}", proxies.First().Id), true);
                    return;
                }
                SPUtility.HandleAccessDenied(new InvalidOperationException());
                return;
            }

            var id = new Guid(idStr);
            var proxy = MCServiceUtility.GetApplicationProxyById(id);
            InitPage(proxy);
        }

        private void InitPage(MCServiceApplicationProxy proxy)
        {
            this.litPageTitle.Text = proxy.DisplayName;
            this.litPageTitleInTitleArea.Text = proxy.DisplayName;
        }
    }
}
