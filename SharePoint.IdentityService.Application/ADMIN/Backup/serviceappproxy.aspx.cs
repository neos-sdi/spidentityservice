using System;
using System.Globalization;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.ApplicationPages;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using MyCorp.SP.ServiceApplication.Security;

namespace MyCorp.SP.ServiceApplication.AdminPages
{
    public partial class ServiceAppProxyPage : GlobalAdminPageBase
    {
        #region Properties

        private MCServiceApplicationProxy _serviceAppProxy;
        private Guid _serviceAppProxyId;
        protected MCServiceApplicationProxy ServiceAppProxy
        {
            get
            {
                return this._serviceAppProxy ??
                       (this._serviceAppProxy = MCServiceUtility.GetApplicationProxyById(ServiceAppProxyId));
            }
        }

        protected Guid ServiceAppProxyId
        {
            get
            {
                if (_serviceAppProxyId == Guid.Empty)
                {
                    var appId = this.Page.Request["id"];
                    if (!string.IsNullOrEmpty(appId))
                    {
                        try
                        {
                            _serviceAppProxyId = new Guid(appId);
                        }
                        catch (FormatException)
                        {
                            throw new SPException("Invalid application id in the querystring of this page.");
                        }
                    }
                }
                return _serviceAppProxyId;
            }
        }

        private DialogMaster DialogMaster
        {
            get { return (DialogMaster)this.Page.Master; }
        }

        #endregion

        #region Page Events
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            DialogMaster.OkButton.Click += OnOkButtonClick;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                if (ServiceAppProxyId == Guid.Empty || ServiceAppProxy == null)
                    throw new InvalidOperationException("Unable to locate service application proxy");

                // Check for permissions to access this page
                if (!SPFarm.Local.CurrentUserIsAdministrator())
                {
                    if (
                        !ServiceAppProxy.ServiceApplication.CheckAdministrationAccess(
                            MCServiceApplicationCentralAdminRights.FullControl))
                        SPUtility.HandleAccessDenied(
                            new UnauthorizedAccessException("You are not authorized to access this page."));
                }

                DialogMaster.OkButton.Text = "Update";
                txtServiceApplicationProxyName.Text = ServiceAppProxy.Name;
                txtOpenChannelTimeout.Text = ServiceAppProxy.OpenTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                txtSendChannelTimeout.Text = ServiceAppProxy.SendTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                txtReceiveChannelTimeout.Text = ServiceAppProxy.ReceiveTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                txtCloseChannelTimeout.Text = ServiceAppProxy.CloseTimeout.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                txtMaximumExecutionTime.Text = ServiceAppProxy.MaximumExecutionTime.ToString(CultureInfo.InvariantCulture);
            }
        }

        #endregion

        #region Form Events

        protected void OnOkButtonClick(object sender, EventArgs e)
        {
            if (this.Page.IsValid && SPUtility.ValidateFormDigest())
            {
                this.UpdateServiceAppProxy();
                this.CommitPopup();
            }
        }

        private void UpdateServiceAppProxy()
        {
            Log.Info(LogCategory.ServiceApplication, "Update MyCorp Service Application Proxy");
            using (var operation = new SPLongOperation(this))
            {
                operation.Begin();
                try
                {
                    if (SPFarm.Local == null)
                        throw new NullReferenceException("SPFarm.Local");

                    var service = MCServiceUtility.GetLocalService(true);

                    // Retrieve the service applicaton
                    var serviceApplicationProxy = MCServiceUtility.GetApplicationProxyById(ServiceAppProxyId);
                    if (serviceApplicationProxy == null)
                    {
                        throw new SPException("Unable to find application proxy to edit");
                    }

                    var newName = this.txtServiceApplicationProxyName.Text.Trim();
                    var newProxyName = newName.Replace(" Proxy", "") + " Proxy";

                    serviceApplicationProxy.Name = newProxyName;
                    serviceApplicationProxy.CloseTimeout = TimeSpan.FromSeconds(Convert.ToDouble(txtCloseChannelTimeout.Text));
                    serviceApplicationProxy.OpenTimeout = TimeSpan.FromSeconds(Convert.ToDouble(txtOpenChannelTimeout.Text));
                    serviceApplicationProxy.ReceiveTimeout = TimeSpan.FromSeconds(Convert.ToDouble(txtReceiveChannelTimeout.Text));
                    serviceApplicationProxy.SendTimeout = TimeSpan.FromSeconds(Convert.ToDouble(txtSendChannelTimeout.Text));
                    serviceApplicationProxy.MaximumExecutionTime = Convert.ToUInt32(txtMaximumExecutionTime.Text);
                    serviceApplicationProxy.Update();
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat(LogCategory.ServiceApplication, "Updating service application proxy error: {0}", ex.Message);
                    Log.Exception(LogCategory.ServiceApplication, ex);
                    throw new SPException("Failed to update service applicaton proxy", ex);
                }
            }
        }

        void CommitPopup()
        {
            Context.Response.Write("<script type='text/javascript'>window.frameElement.commitPopup();</script>");
            Context.Response.Flush();
            Context.Response.End();
        }

        #endregion

        #region Form Validation
        protected void ValidateUniqueName(object sender, ServerValidateEventArgs e)
        {
            ArgumentValidator.IsNotNull(e, "e");

            var name = this.txtServiceApplicationProxyName.Text.Trim();

            var applicationProxyByName = MCServiceUtility.GetApplicationProxyByName(name);
            e.IsValid = (applicationProxyByName == null || applicationProxyByName.Id == ServiceAppProxyId);
        }
        #endregion
    }
}
