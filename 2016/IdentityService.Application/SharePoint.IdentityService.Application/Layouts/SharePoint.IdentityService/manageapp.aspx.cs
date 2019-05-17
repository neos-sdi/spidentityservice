//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using SharePoint.IdentityService;
using Microsoft.SharePoint.Administration;

namespace SharePoint.IdentityService.AdminLayoutPages
{
    public partial class ManageAppPage : AdminLayoutsPageBase
    {

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                this.IDPARAMS.NavigateUrl = string.Format("~/_layouts/15/SharePoint.IdentityService/manageparams.aspx?id={0}", GetID());
                this.IDENTITIES.NavigateUrl = string.Format("~/_layouts/15/SharePoint.IdentityService/manageentities.aspx?id={0}", GetID());
                this.IDCONNECTIONS.NavigateUrl = string.Format("~/_layouts/15/SharePoint.IdentityService/manageconnections.aspx?id={0}", GetID());
                this.IDEXTENTIONS.NavigateUrl = string.Format("~/_layouts/15/SharePoint.IdentityService/managedlls.aspx?id={0}", GetID());
                this.RETURNBACK.NavigateUrl = "~/_admin/ServiceApplications.aspx";
                Page.DataBind();
            }
        }

        /// <summary>
        /// LinkButtonRefresh_Click method implmentation
        /// </summary>
        protected void LinkButtonRefresh_Click(object sender, EventArgs e)
        {
            if (!CheckModifyAccess())
                SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to access this page."));
            ExecuteOnProxy(false);
        }

        /// <summary>
        /// LinkButtonClearCache_Click method implmentation
        /// </summary>
        protected void LinkButtonClearCache_Click(object sender, EventArgs e)
        {
            if (!CheckModifyAccess())
                SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to access this page."));
            ExecuteOnProxy(true);
        }

        /// <summary>
        /// ExecuteOnProxy method implementation
        /// </summary>
        public void ExecuteOnProxy(bool clearcache)
        {
            try
            {
                SPFarm farm = SPFarm.Local;
                IdentityServiceProxy serviceProxy = farm.ServiceProxies.GetValue<IdentityServiceProxy>();
                if (null != serviceProxy)
                {
                    foreach (SPServiceApplicationProxy prxy in serviceProxy.ApplicationProxies)
                    {
                        if (prxy is IdentityServiceApplicationProxy)
                        {
                            if (CheckApplicationProxy(ServiceApplication, prxy as IdentityServiceApplicationProxy))
                            {
                                if (clearcache)
                                {
                                    foreach (SPServer srv in farm.Servers)
                                    {
                                        IdentityServiceApplication app = srv.ServiceInstances.GetValue<IdentityServiceApplication>(new Guid(this.GetID()));
                                        if ((app != null) && (app.Status == SPObjectStatus.Online))
                                        {
                                            ((IdentityServiceApplicationProxy)prxy).LaunchClearCacheCommand(srv.Name);  // Only on one valid Server
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (SPServer srv in farm.Servers)
                                    {
                                        IdentityServiceApplication app = srv.ServiceInstances.GetValue<IdentityServiceApplication>(new Guid(this.GetID()));
                                        if ((app != null) && (app.Status == SPObjectStatus.Online))
                                            ((IdentityServiceApplicationProxy)prxy).LaunchReloadCommand(srv.Name);   // On Each Servers
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Do Nothing
            }
        }

        /// <summary>
        /// CheckApplicationProxy metho implementation
        /// </summary>
        private bool CheckApplicationProxy(IdentityServiceApplication app, IdentityServiceApplicationProxy prxy)
        {
            bool result = false;
            try
            {
                string path = app.IisVirtualDirectoryPath;
                string[] xpath = path.Split('\\');
                result = (prxy.ServiceEndpointUri.ToString().ToLower().Contains(xpath[1]));
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}
