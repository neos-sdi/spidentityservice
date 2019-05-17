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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.AccessControl;
using Microsoft.SharePoint.Utilities;
using System.Threading;


namespace SharePoint.IdentityService.AdminLayoutPages
{
    public class AdminLayoutsPageBase : LayoutsPageBase
    {
        private IdentityServiceApplication _serviceApp;
        private Guid _serviceAppId;
        private const string resfilename = "SharePoint.IdentityService.Administration";

        /// <summary>
        /// OnInit method override
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        /// <summary>
        /// OnLoad method override
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                if (ServiceApplicationId != Guid.Empty && ServiceApplication == null)
                    SPUtility.HandleAccessDenied(new InvalidOperationException("Unable to locate service application"));
                   // throw new InvalidOperationException("Unable to locate service application");

                if (ServiceApplicationId != Guid.Empty)
                {
                    if (!CheckReadAccess())
                       SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to access this page."));
                }
            }
        }

        /// <summary>
        /// ServiceApplication property implementation
        /// </summary>
        protected IdentityServiceApplication ServiceApplication
        {
            get
            {
                return this._serviceApp ?? (this._serviceApp = Utilities.GetApplicationById(ServiceApplicationId));
            }
        }

        /// <summary>
        /// ServiceApplicationId property implementation
        /// </summary>
        protected virtual Guid ServiceApplicationId
        {
            get
            {
                if (_serviceAppId == Guid.Empty)
                {
                    var appId = this.Page.Request["id"];
                    if (!string.IsNullOrEmpty(appId))
                    {
                        try
                        {
                            _serviceAppId = new Guid(appId);
                        }
                        catch (FormatException)
                        {
                            throw new SPException("Invalid application id in the querystring of this page.");
                        }
                    }
                }
                
                return _serviceAppId;
            }
        }

        /// <summary>
        /// GetFormattedTitle method implementation
        /// </summary>
        public string GetFormattedTitle(string formatstr)
        {
            return string.Format(SPUtility.GetLocalizedString("$Resources:" + formatstr, resfilename, Convert.ToUInt32(Thread.CurrentThread.CurrentUICulture.LCID)), ServiceApplication.Name);
        }

        /// <summary>
        /// GetUIString method implementation
        /// </summary>
        public string GetUIString(string formatstr)
        {
            return SPUtility.GetLocalizedString("$Resources:" + formatstr, resfilename, Convert.ToUInt32(Thread.CurrentThread.CurrentUICulture.LCID));
        }

        /// <summary>
        /// GetFormattedTitle method implementation
        /// </summary>
        public string GetID()
        {
            return ServiceApplicationId.ToString("D");
        }

        /// <summary>
        /// CheckModifyAccess method implementation
        /// </summary>
        internal bool CheckModifyAccess()
        {
            return (ServiceApplication.CheckAdministrationAccess(IdentityServiceCentralAdministrationRights.Write) || ServiceApplication.CheckAdministrationAccess(SPCentralAdministrationRights.FullControl));
        }

        /// <summary>
        /// CheckReadAccess method implementation
        /// </summary>
        internal bool CheckReadAccess()
        {
            return (ServiceApplication.CheckAdministrationAccess(SPCentralAdministrationRights.Read) || ServiceApplication.CheckAdministrationAccess(SPCentralAdministrationRights.FullControl));
        }

    }
}
