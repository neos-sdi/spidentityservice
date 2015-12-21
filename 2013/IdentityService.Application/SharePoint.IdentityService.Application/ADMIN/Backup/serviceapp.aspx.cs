using System;
using System.Linq;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.AccessControl;
using Microsoft.SharePoint.ApplicationPages;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using SharePoint.IdentityService;
using System.Net;
using System.Threading;
using Microsoft.SharePoint.Administration.Claims;
using System.Collections.Generic;
using SharePoint.IdentityService.Core;
using System.Security.Principal;
using System.Diagnostics.CodeAnalysis;

namespace SharePoint.IdentityService.AdminPages
{
    public partial class ServiceAppPage : GlobalAdminPageBase
    {
        protected ContentDatabaseSection DatabaseSection;
        protected IisWebServiceApplicationPoolSection ApplicationPoolSection;

        private IdentityServiceApplication _serviceApp;
        private Guid _serviceAppId;
        private List<ClaimProviderDefinition> _trustedproviderslist;

        #region Properties
        /// <summary>
        /// IsNewClaimProvider property implementation
        /// </summary>
        public bool IsNewClaimProvider
        {
            get 
            {
                if (ViewState["isnewclaimprovider"] != null)
                    return bool.Parse(ViewState["isnewclaimprovider"].ToString());
                else
                    return true;
            }
            set 
            {
                if (ViewState["initclaimprovidername"] != null)
                    ViewState["isnewclaimprovider"] = value;
                else
                    ViewState.Add("isnewclaimprovider", value);
            }
        }

        /// <summary>
        /// InitialClaimProviderName property implementation
        /// </summary>
        public string InitialClaimProviderName
        {
            get 
            {
                if (ViewState["initclaimprovidername"] != null)
                    return ViewState["initclaimprovidername"].ToString();
                else
                    return string.Empty;
            }
            set 
            {
                if (ViewState["initclaimprovidername"] != null)
                    ViewState["initclaimprovidername"] = value;
                else
                    ViewState.Add("initclaimprovidername", value);
            }
        }

        /// <summary>
        /// InitialTrustedProviderName property implementation
        /// </summary>
        public string InitialTrustedProviderName
        {
            get
            {
                if (ViewState["inittrustedprovidername"] != null)
                    return ViewState["inittrustedprovidername"].ToString();
                else
                    return string.Empty;
            }
            set
            {
                if (ViewState["inittrustedprovidername"] != null)
                    ViewState["inittrustedprovidername"] = value;
                else
                    ViewState.Add("inittrustedprovidername", value);
            }
        }

        /// <summary>
        /// ClaimProviderList method implementation
        /// </summary>
        public List<ClaimProviderDefinition> TrustedProviderList
        {
            get { return _trustedproviderslist; }
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
        protected Guid ServiceApplicationId
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
        /// DialogMaster property implementation
        /// </summary>
        private DialogMaster DialogMaster
        {
            get { return (DialogMaster)this.Page.Master; }
        }
        #endregion

        #region Page Events
        /// <summary>
        /// OnInit event override
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            DialogMaster.OkButton.Click += OnOkButtonClick;
            DatabaseSection.DatabaseSubmitted += DatabaseSubmitted;
            DatabaseSection.DatabaseErrorMessage = "Cette base de données existe déjà !";
            _trustedproviderslist = Utilities.GetClaimProviderCandidates();
            foreach (ClaimProviderDefinition current in TrustedProviderList)
            {
                InputClaimProviderDropBox.Items.Add(new ListItem(current.DisplayName, current.TrustedTokenIssuer));
            }
            InputClaimProviderDropBox.SelectedIndexChanged += OnSelectedTrustedIssuerChanged;
        }

        /// <summary>
        /// OnLoad event override
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                if (ServiceApplicationId != Guid.Empty && ServiceApplication == null)
                    throw new SPException("Unable to locate service application");
                #region Update an existing Service Application
                if (ServiceApplicationId != Guid.Empty)
                {
                    // Check for permissions to access this page
                    if (!SPFarm.Local.CurrentUserIsAdministrator())
                    {
                        if (!ServiceApplication.CheckAdministrationAccess(SPCentralAdministrationRights.FullControl))
                           SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to access this page."));
                    }

                    DialogMaster.OkButton.Text = "OK";
                    if (ServiceApplication.Database != null)
                    {
                        DatabaseSection.ConnectionString = ServiceApplication.Database.ConnectString();
                        if (ServiceApplication.Database.FailoverServer != null)
                        {
                            DatabaseSection.IncludeFailoverDatabaseServer = true;
                            DatabaseSection.FailoverDatabaseServer = ServiceApplication.Database.FailoverServer.Name;
                        }
                        if (!string.IsNullOrEmpty(ServiceApplication.Database.Username))
                        {
                            DatabaseSection.UseWindowsAuthentication = false;
                            DatabaseSection.DatabaseUserName = ServiceApplication.Database.Username;
                        }
                    }
                    ApplicationPoolSection.SetSelectedApplicationPool(ServiceApplication.ApplicationPool);
                    txtServiceApplicationName.Text = ServiceApplication.Name;
                    ProxyClaimsProviderParameters prm = null;
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        prm = ServiceApplication.FillClaimsProviderParameters();
                    });
                    try
                    {
                        if (prm != null)
                        {
                            if (((!string.IsNullOrEmpty(prm.TrustedLoginProviderName)) && (!string.IsNullOrEmpty(prm.ClaimProviderName))))
                            {   // Update of an existing Service application (Windows Or Trusted)
                                SPClaimProviderDefinition def = Utilities.GetClaimProvider(prm.ClaimProviderName);
                                this.txtInputFormTextClaimName.Text = prm.ClaimProviderName;
                                this.txtInputFormDisplayClaimName.Text = prm.ClaimDisplayName;
                                this.InitialTrustedProviderName = prm.TrustedLoginProviderName;
                                if (def != null)
                                {
                                    this.txtInputFormTextClaimDesc.Text = def.Description;
                                    this.visibilityCB.Checked = def.IsUsedByDefault;
                                    this.InitialClaimProviderName = def.DisplayName;
                                    this.InputClaimProviderDropBox.SelectedValue = prm.TrustedLoginProviderName;
                                }
                                else
                                    this.InputClaimProviderDropBox.SelectedValue = "AD";
                                this.IsNewClaimProvider = false;
                            }
                            else
                            {
                                throw new Exception("Cannot find essential parameters (TrustedLoginProviderName, ClaimProviderName) !"); 
                            }
                        }
                        else
                        {
                            throw new Exception("Cannot find essential parameters (TrustedLoginProviderName, ClaimProviderName) !"); 
                        }
                    }
                    catch (Exception ex)
                    {
                        RedirectToErrorPage(String.Format("Failed to create service applicaton {0} \n Execption : {1}", ServiceApplication.Name, ex.Message));
                    }
                }
                #endregion
                #region Create a New Service Application
                else // Creation of New Service Application
                {
                    // Check for permissions to access this page
                    if (!SPFarm.Local.CurrentUserIsAdministrator())
                    {
                        if (!ServiceApplication.CheckAdministrationAccess(SPCentralAdministrationRights.FullControl))
                            SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to access this page."));
                    }

                    DialogMaster.OkButton.Text = "OK";
                    DatabaseSection.DatabaseServer = SPWebService.ContentService.DefaultDatabaseInstance.NormalizedDataSource;
                    DatabaseSection.DatabaseName = "IdentityServiceDatabase_"+Guid.NewGuid().ToString("D");
                    txtServiceApplicationName.Text = litServiceApplicationTitle.Text + " (Your Application Name)";
                    litServiceApplicationTitle.Text = "Créer " + litServiceApplicationTitle.Text;
                    this.txtInputFormTextClaimName.Text = "AD";
                    this.txtInputFormDisplayClaimName.Text = "Windows";
                    this.txtInputFormTextClaimDesc.Text = "Données de revendication Active Directory.";
                    this.visibilityCB.Checked = false;
                    this.InitialClaimProviderName = string.Empty;
                    this.InitialTrustedProviderName = string.Empty;
                    this.IsNewClaimProvider = true;
                    this.InputClaimProviderDropBox.SelectedValue = "AD";
                }
                #endregion
            }
        }

        /// <summary>
        /// CanUpdateProvider method implementation
        /// </summary>
        internal bool CanUpdateProvider()
        {
            return (this.InitialClaimProviderName.ToLowerInvariant().Trim().Equals(this.txtInputFormTextClaimName.Text.ToLowerInvariant().Trim()) &&
                    this.InitialTrustedProviderName.ToLowerInvariant().Trim().Equals(this.InputClaimProviderDropBox.SelectedValue.ToLowerInvariant().Trim()));
            //** ICI ** 
        }
        #endregion

        #region Form Events

        /// <summary>
        /// OnOkButtonClick method implementation
        /// </summary>
        protected void OnOkButtonClick(object sender, EventArgs e)
        {
            if (this.Page.IsValid && SPUtility.ValidateFormDigest())
            {
                if (this.ServiceApplicationId != Guid.Empty)
                {
                    this.UpdateServiceApp();
                }
                else
                {
                    this.CreateNewServiceApp();
                }
                this.CommitPopup();
            }
        }

        /// <summary>
        /// SelectedTrustedIssuerChanged method implementation
        /// </summary>
        public void OnSelectedTrustedIssuerChanged(object sender, EventArgs e)
        {
            this.txtInputFormTextClaimName.Text = this.InputClaimProviderDropBox.SelectedValue;
            SPClaimProviderDefinition def = Utilities.GetClaimProvider(this.InputClaimProviderDropBox.SelectedValue);
            if (def != null)
            {
                this.txtInputFormDisplayClaimName.Text = string.Empty;
                this.txtInputFormTextClaimDesc.Text = def.Description;
                this.visibilityCB.Checked = def.IsUsedByDefault;
            }
            else
            {
                this.txtInputFormDisplayClaimName.Text = string.Empty;
                this.txtInputFormTextClaimDesc.Text = this.InputClaimProviderDropBox.SelectedValue;
                this.visibilityCB.Checked = false;
            }
        }

        /// <summary>
        /// CreateNewServiceApp method implementation
        /// </summary>
        private void CreateNewServiceApp()
        {
            using (var operation = new SPLongOperation(this))
            {
                operation.Begin();
                NetworkCredential cred = null;
                string name = null;
                try
                {
                    name = this.txtServiceApplicationName.Text.Trim();
                    ContentDatabaseSection db = this.DatabaseSection;
                    if (db.UseWindowsAuthentication)
                        cred = CredentialCache.DefaultNetworkCredentials;
                    else
                    cred = new NetworkCredential(db.DatabaseUserName, db.DatabasePassword);
                    SPIisWebServiceApplicationPool ap = (this.ApplicationPoolSection == null) ? null : this.ApplicationPoolSection.GetOrCreateApplicationPool();
                    Utilities.CreateServiceApplicationAndProxy(true, name, ap, db.DatabaseName.Trim(), db.DatabaseServer.Trim(), db.FailoverDatabaseServer, cred, false, CBReplaceDB.Checked); 
                    Utilities.CreateUpdateDeleteClaimProvider(name, this.InitialClaimProviderName, this.InputClaimProviderDropBox.SelectedValue, this.txtInputFormTextClaimName.Text, this.txtInputFormDisplayClaimName.Text, this.txtInputFormTextClaimDesc.Text,
                                                                this.visibilityCB.Checked, this.CanUpdateProvider());
                }
                catch (Exception ex)
                {
                    RedirectToErrorPage(String.Format("Failed to create service applicaton {0} \n Execption : {1}", name, ex.Message));
                   // new SPException(String.Format("Failed to create service applicaton {0}", name), ex);
                }
            }
        }

        /// <summary>
        /// UpdateServiceApp method implementation
        /// </summary>
        private void UpdateServiceApp()
        {
            using (var operation = new SPLongOperation(this))
            {
                operation.Begin();
                NetworkCredential cred = null;
                string name = null;
                try
                {
                    name = this.txtServiceApplicationName.Text.Trim();
                    ContentDatabaseSection db = this.DatabaseSection;
                    if (db.UseWindowsAuthentication)
                        cred = CredentialCache.DefaultNetworkCredentials;
                    else
                        cred = new NetworkCredential(db.DatabaseUserName, db.DatabasePassword);
                    SPIisWebServiceApplicationPool ap = (this.ApplicationPoolSection == null) ? null : this.ApplicationPoolSection.GetOrCreateApplicationPool();
                    Utilities.UpdateServiceApplicationAndProxy(true, this.ServiceApplication, name, ap, db.DatabaseName.Trim(), db.DatabaseServer.Trim(), db.FailoverDatabaseServer, cred, false, CBReplaceDB.Checked);
                    Utilities.CreateUpdateDeleteClaimProvider(name, this.InitialClaimProviderName, this.InputClaimProviderDropBox.SelectedValue, this.txtInputFormTextClaimName.Text, this.txtInputFormDisplayClaimName.Text, this.txtInputFormTextClaimDesc.Text,
                                                                this.visibilityCB.Checked, this.CanUpdateProvider());
                }
                catch (Exception ex)
                {
                   // new SPException(String.Format("Failed to update service applicaton {0}", name), ex);
                    RedirectToErrorPage(String.Format("Failed to create service applicaton {0} \n Execption : {1}", name, ex.Message));
                }
            }
        }

        /// <summary>
        /// CommitPopup method override
        /// </summary>
        void CommitPopup()
        {
            Context.Response.Write("<script type='text/javascript'>window.frameElement.commitPopup();</script>");
            Context.Response.Flush();
            Context.Response.End();
        }
        #endregion

        #region Form Validation
        /// <summary>
        /// ValidateUniqueName method implementation
        /// </summary>
        protected void ValidateUniqueName(object sender, ServerValidateEventArgs e)
        {
            ArgumentValidator.IsNotNull(e, "e");
            string name = this.txtServiceApplicationName.Text.Trim();
            SPServiceApplication applicationByName = Utilities.GetApplicationByName(name);
            if (this.ServiceApplicationId != Guid.Empty)
                e.IsValid = (applicationByName == null || applicationByName.Id == ServiceApplicationId);
            else
                e.IsValid = (applicationByName == null) || !applicationByName.Name.ToLowerInvariant().Trim().Equals((litServiceApplicationTitle.Text + " (Your Application Name)").ToLowerInvariant().Trim()); 
        }

        /// <summary>
        /// ValidateUniqueName method implementation
        /// </summary>
        protected void ValidateUniqueClaimName(object sender, ServerValidateEventArgs e)
        {
            ArgumentValidator.IsNotNull(e, "e");
            string name = this.txtInputFormTextClaimName.Text.Trim();
            e.IsValid = Utilities.DoesClaimProviderIsValid(name, this.InputClaimProviderDropBox.SelectedValue);
           /* if (this.ServiceApplicationId != Guid.Empty)
                e.IsValid = Utilities.DoesClaimProviderExist(name, this.InputClaimProviderDropBox.SelectedValue);
            else
                e.IsValid = !Utilities.DoesClaimProviderExist(name, this.InputClaimProviderDropBox.SelectedValue); */
        }

        /// <summary>
        /// DatabaseSubmitted method implementation
        /// </summary>
        protected void DatabaseSubmitted(object source, ServerValidateEventArgs args)
        {
            string currentdb = "";
            if (this.ServiceApplicationId != Guid.Empty)
            {
                if (ServiceApplication.Database==null)
                {
                    args.IsValid = true;
                    return; 
                }
                currentdb = ServiceApplication.Database.Name;
                if (currentdb.ToLowerInvariant().Trim().Equals(args.Value.ToLowerInvariant().Trim()))
                {
                    args.IsValid = true;
                    return;
                }
            }
            SPDatabaseParameters databaseParameters = null;
            NetworkCredential cred = null;
            if (this.DatabaseSection.UseWindowsAuthentication)
                cred = CredentialCache.DefaultNetworkCredentials;
            else
                cred = new NetworkCredential(this.DatabaseSection.DatabaseUserName, this.DatabaseSection.DatabasePassword);
            if (this.DatabaseSection.IncludeFailoverDatabaseServer)
                databaseParameters = SPDatabaseParameters.CreateParameters(args.Value, this.DatabaseSection.DatabaseServer, cred, this.DatabaseSection.FailoverDatabaseServer, SPDatabaseParameterOptions.None);
            else
                databaseParameters = SPDatabaseParameters.CreateParameters(args.Value, this.DatabaseSection.DatabaseServer, cred, null, SPDatabaseParameterOptions.None);
            ActiveDirectoryIdentityServiceDatabase db = new ActiveDirectoryIdentityServiceDatabase(databaseParameters);
            args.IsValid = !db.Exists || this.CBReplaceDB.Checked;
        }
        #endregion
    }
}
