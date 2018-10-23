﻿//******************************************************************************************************************************************************************************************//
// Copyright (c) 2015 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //
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

using SharePoint.IdentityService;
using SharePoint.IdentityService.Core;
using System;

namespace SharePoint.IdentityService
{
    using Microsoft.IdentityModel.WindowsTokenService;
    using Microsoft.Office.Server.UserProfiles;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Administration.AccessControl;
    using Microsoft.SharePoint.Utilities;
    using SharePoint.IdentityService.Core;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Threading;
    using System.Xml;
    using System.Linq;

    internal static class IdentityServiceCentralAdministrationRights
    {
        public const SPCentralAdministrationRights Write = (SPCentralAdministrationRights)0x1 | SPCentralAdministrationRights.Read;
    }

    [IisWebServiceApplicationBackupBehavior]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession,  ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    [Guid("948E1B2F-9002-404C-852E-656893CC391F")]
    public sealed class IdentityServiceApplication : SPIisWebServiceApplication, IIdentityServiceContract
    {
        [Persisted]
        private ActiveDirectoryIdentityServiceDatabase m_Database;

        private static WrapperManager _wrappermanager = null;
        private UserProfileManager _profilemanager;
        private bool _withimages = false;
        private ProxyClaimsMode _claimmode = ProxyClaimsMode.Windows;

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public IdentityServiceApplication():base()
        {
        }

        /// <summary>
        /// Private constructor implementation
        /// </summary>
        private IdentityServiceApplication(string name, AdministrationService service, SPIisWebServiceApplicationPool applicationPool, ActiveDirectoryIdentityServiceDatabase database): base(name, service, applicationPool)
        {
            if (null == database)
            {
                throw new ArgumentNullException("database");
            }
            m_Database = database;
        }

        /// <summary>
        /// Create method implementation
        /// </summary>
        public static IdentityServiceApplication Create(string name, AdministrationService service, SPIisWebServiceApplicationPool applicationPool, SPDatabaseParameters databaseParameters)
        {
            if (null == name)
            {
                throw new ArgumentNullException("name");
            }
            if (null == service)
            {
                throw new ArgumentNullException("service");
            }
            if (null == applicationPool)
            {
                throw new ArgumentNullException("applicationPool");
            }
            if (null == databaseParameters)
            {
                throw new ArgumentNullException("databaseParameters");
            }
            ActiveDirectoryIdentityServiceDatabase database = new ActiveDirectoryIdentityServiceDatabase(databaseParameters);
            IdentityServiceApplication serviceApplication = new IdentityServiceApplication(name, service, applicationPool, database);
            database.Update(true);
            serviceApplication.Update();
            serviceApplication.AddServiceEndpoint("http", SPIisWebServiceBindingType.Http);
            serviceApplication.AddServiceEndpoint("https", SPIisWebServiceBindingType.Https, "secure");
            return serviceApplication;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Database property implementation
        /// </summary>
        public ActiveDirectoryIdentityServiceDatabase Database 
        { 
            get { return m_Database; } 
            internal set { m_Database = value; } 
        }
        
        /// <summary>
        /// DefaultEndpointName property implementation
        /// </summary>
        protected override string DefaultEndpointName
        {
            get { return "http"; }
        }

        /// <summary>
        /// ApplicationClassId property implementation
        /// </summary>
        public override Guid ApplicationClassId
        {
            get { return new Guid("948E1B2F-9002-404C-852E-656893CC391F"); }
        }

        /// <summary>
        /// ApplicationVersion property implementation
        /// </summary>
        public override Version ApplicationVersion
        {
            get { return new Version("16.0.0.0"); }
        }
        #endregion

        #region Display Values
        /// <summary>
        /// TypeName property implementation
        /// </summary>
        public override string TypeName
        {
            get { return "SharePoint Identity Application"; }
        }

        /// <summary>
        /// DisplayName property implementation
        /// </summary>
        public override string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.Name))
                    this.Name = "SharePoint Identity Service Application";
                return this.Name;
            }
        }
        #endregion

        #region Application Provisionning
        /// <summary>
        /// Provision method override
        /// </summary>
        public override void Provision()
        {
            if (SPObjectStatus.Provisioning != base.Status)
            {
                Status = SPObjectStatus.Provisioning;
                Update();
            }
            try
            {
                m_Database.Provision();
                base.Provision();
                Status = SPObjectStatus.Online;
                Update();
                InstallJobs();
                Update(true);
            }
            catch (Exception e)
            {
                Status = SPObjectStatus.Disabled;
                Update();
                throw e;
            }
        }

        /// <summary>
        /// Unprovision method override
        /// </summary>
        public override void Unprovision(bool deleteData)
        {
            try
            {
                StopJobs();
            }
            catch (Exception e)
            {
                LogEvent.Log(e, e.StackTrace, EventLogEntryType.Error, 20099);
            }
            if (SPObjectStatus.Unprovisioning != base.Status)
            {
                Status = SPObjectStatus.Unprovisioning;
                Update();
            } 
            try
            {
                base.Unprovision(deleteData);
                if (deleteData)
                {
                    if (m_Database.Exists)
                        m_Database.Unprovision();
                }
                Status = SPObjectStatus.Disabled;
                Update();
                RemoveJobs();
                Update(true);
            }
            catch (Exception e)
            {
                Status = SPObjectStatus.Disabled;
                Update();
                throw e;
            }
        }

        /// <summary>
        /// Upgrade()
        /// </summary>
        public override void Upgrade()
        {
            SPObjectStatus old = Status;
            Status = SPObjectStatus.Upgrading;
            try
            {
              //  base.Upgrade();
                if (m_Database.Exists)
                    m_Database.Upgrade();
            }
            finally
            {
                Status = old;
                Update();
            }
        }

        /// <summary>
        /// Delete method override
        /// </summary>
        public override void Delete()
        {
            // Delete the service application
            // This must be done BEFORE the database is deleted, or else a dependency error will occur
            base.Delete();
            if (m_Database != null)
            {
                // IF there are other service applications that have a dependency on this database,
                // you cannot delete the database object (only the last dependency can delete it)
                // This does not delete the physical database, only the persisted object reference
                // to the database (Unprovision is what deletes the physical database)
                try
                {
                    m_Database.Delete();
                }
                catch 
                {
                    // Do Nothing
                }
            }
        }

        #endregion

        #region Links & Uri
        /// <summary>
        /// InstallPath property implementation
        /// </summary>
        protected override string InstallPath
        {
           // get { return Microsoft.SharePoint.Utilities.SPUtility.GetVersionedGenericSetupPath(@"WebServices\SharePoint.IdentityService", 16); }
            get { return Microsoft.SharePoint.Utilities.SPUtility.GetCurrentGenericSetupPath(@"WebServices\SharePoint.IdentityService"); }
        }

        /// <summary>
        /// VirtualPath property implementation
        /// </summary>
        protected override string VirtualPath
        {
            get { return "SharePoint.IdentityService.svc"; }
        }

        /// <summary>
        /// ManageLink property implementation
        /// </summary>
        public override SPAdministrationLink ManageLink
        {
            get { return new SPAdministrationLink(String.Format("/_layouts/15/SharePoint.IdentityService/manageapp.aspx?id={0}", this.Id)); }
        }

        /// <summary>
        /// PropertiesLink property implementation
        /// </summary>
        public override SPAdministrationLink PropertiesLink
        {
            get { return new SPAdministrationLink(String.Format("/_admin/SharePoint.IdentityService/serviceapp.aspx?id={0}", this.Id)); }
        }
        #endregion

        #region Rights & Access
        /// <summary>
        /// OnProcessIdentityChanged method override
        /// </summary>
        protected override void OnProcessIdentityChanged(SecurityIdentifier processSecurityIdentifier)
        {
            base.OnProcessIdentityChanged(processSecurityIdentifier);
            m_Database.GrantApplicationPoolAccess(processSecurityIdentifier);
        }

        /// <summary>
        /// AdministrationAccessRights method override
        /// </summary>
        protected override SPNamedCentralAdministrationRights[] AdministrationAccessRights
        {
            get
            {
                return new SPNamedCentralAdministrationRights[]
                {
                    SPNamedCentralAdministrationRights.FullControl,
                    new SPNamedCentralAdministrationRights("Modification",SPCentralAdministrationRights.Read | IdentityServiceCentralAdministrationRights.Write),
                    SPNamedCentralAdministrationRights.Read
                };
            }
        }

        /// <summary>
        /// AccessRights method override
        /// </summary>
        protected override SPNamedIisWebServiceApplicationRights[] AccessRights
        {
            get
            {
                return new SPNamedIisWebServiceApplicationRights[]
                {
                    SPNamedIisWebServiceApplicationRights.FullControl, 
                    SPNamedIisWebServiceApplicationRights.Read
                };
            }
        }

        /// <summary>
        /// GetCentralAdminSite method implementation
        /// </summary>
        internal SPSiteAdministration GetCentralAdminSite()
        {
            using (SPSite site = SPAdministrationWebApplication.GetInstanceLocalToFarm(SPFarm.Local).Sites["/"])
            {
                return new SPSiteAdministration(site.ID);
            }
        }

        // the effective permissions are static, whereas the rights can be customized
        /// <summary>
        /// CheckAdministrationAccess method implementation
        /// </summary>
        internal new bool CheckAdministrationAccess(SPCentralAdministrationRights rights)
        {
            if (base.CheckAdministrationAccess((SPCentralAdministrationRights)rights))
            {
                return true;
            }

            SPCentralAdministrationSecurity accessControl = GetAdministrationAccessControl();
            if (accessControl != null)
            {
                foreach (SPAclAccessRule<SPCentralAdministrationRights> rule in accessControl.GetAccessRules())
                {
                    var allowedRights = (SPCentralAdministrationRights)rule.AllowedRights;
                    if (allowedRights.Has(rights))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// CheckFullControlAccess method implmentation
        /// </summary>
        internal void CheckFullControlAccess()
        {
            // Check for permissions to access this page
            if (!SPFarm.Local.CurrentUserIsAdministrator())
            {
                if (!CheckAdministrationAccess(SPCentralAdministrationRights.FullControl))
                    SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to access this page."));
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void Initialize()
        {
            DoInitialize(false);
        }

        /// <summary>
        /// DoInitialize method implementation
        /// </summary>
        private void DoInitialize(bool reset)
        {
            if (_wrappermanager == null)
                _wrappermanager = new WrapperManager(Database);
            _wrappermanager.DoInitialize(reset);
        }
        #endregion           

        #region IIdentityServiceContract Members
        /// <summary>
        /// FillSearch method implementation
        /// </summary>
        public ProxyResults FillSearch(string pattern, string domain, bool recursive)
        {
            try
            {
                using (SPMonitoredScope scp = new SPMonitoredScope("FillSearch"))
                {
                    Initialize();
                    ProxyResults res = _wrappermanager.FillSearch(pattern, domain, recursive);
                    if (WantPictures())
                    {
                        PatchUsersWithPictureUrl(res);
                    }
                    return DoSortAllResults(res);
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E20001"), pattern, domain), EventLogEntryType.Error, 20001);
                return null;
            }
        }

        /// <summary>
        /// FillResolve method implementation
        /// </summary>
        public ProxyResults FillResolve(string pattern, bool recursive)
        {
            try
            {
                using (SPMonitoredScope scp = new SPMonitoredScope("FillResolve"))
                {
                    Initialize();
                    return DoSortAllResults(_wrappermanager.FillResolve(pattern, recursive));
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E20002"), pattern), EventLogEntryType.Error, 20002);
                return null;
            }
        }

        /// <summary>
        /// FillValidate method implementation
        /// </summary>
        public ProxyResults FillValidate(string pattern, bool recursive)
        {
            try
            {
                using (SPMonitoredScope scp = new SPMonitoredScope("FillValidate"))
                {
                    Initialize();
                    return DoSortAllResults(_wrappermanager.FillValidate(pattern, recursive));
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E20003"), pattern), EventLogEntryType.Error, 20003);
                return null;
            }
        }


        /// <summary>
        /// FillHierarchy method imlementation
        /// </summary>
        public ProxyDomain FillHierarchy(string hierarchyNodeID, int numberOfLevels)
        {
            try
            {
                using (SPMonitoredScope scp = new SPMonitoredScope("FillHierarchy"))
                {
                    Initialize();
                    ProxyDomain prxy = _wrappermanager.FillHierarchy(hierarchyNodeID, numberOfLevels);
                    if (prxy != null)
                        prxy.Domains.Sort(new ProxyResultDomainsSort());
                    return prxy;
                }
            }
            catch (Exception E)
            {
                if (_wrappermanager != null)
                {
                    if (!string.IsNullOrEmpty(hierarchyNodeID))
                        LogEvent.Log(E, string.Format(ResourcesValues.GetString("E20004"), hierarchyNodeID), EventLogEntryType.Error, 20004);
                    else
                        LogEvent.Log(E, ResourcesValues.GetString("E20004B"), EventLogEntryType.Error, 20004);
                }
                return null;
            }
        }

        /// <summary>
        /// FillBadDomains method implementation
        /// </summary>
        public List<ProxyBadDomain> FillBadDomains()
        {
            try
            {
                using (SPMonitoredScope scp = new SPMonitoredScope("FillBadDomains"))
                {
                    Initialize();
                    return _wrappermanager.FillBadDomains();
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, ResourcesValues.GetString("E20005"), EventLogEntryType.Error, 20005);
                return null;
            }

        }

        /// <summary>
        /// PatchUsersWithPictureUrl method implmentation
        /// </summary>
        private void PatchUsersWithPictureUrl(ProxyResults results)
        {
            if (results == null)
                return;
            if (results.Nodes == null)
                return;
            foreach (ProxyResultsNode node in results.Nodes)
            {
                if (results.Results != null)
                {

                    foreach (ProxyResultObject o in node.Results)
                    {
                        if (o is ProxyUser)
                        {
                            ProxyUser u = o as ProxyUser;
                            u.PictureUrl = GetUserImage(GetFormattedUser(u));
                        }
                    }
                    PatchUsersWithPictureUrl(node);
                }
            }
        }

        /// <summary>
        /// PatchUsersWithPictureUrl method implementation
        /// </summary>
        private void PatchUsersWithPictureUrl(ProxyResultsNode nodes)
        {
            if (nodes == null)
                return;
            if (nodes.Results != null)
            {
                foreach (ProxyResultObject o in nodes.Results)
                {
                    if (o is ProxyUser)
                    {
                        ProxyUser u = o as ProxyUser;
                        u.PictureUrl = GetUserImage(GetFormattedUser(u));
                    }
                }
            }
            if (nodes.Nodes != null)
            {
                foreach (ProxyResultsNode node in nodes.Nodes)
                {
                    PatchUsersWithPictureUrl(node);
                }
            }
        }

        /// <summary>
        /// WantPictures method implementation
        /// </summary>
        private bool WantPictures()
        {
            return (this._withimages);
        }

        /// <summary>
        /// GetFormattedUser method implementation
        /// </summary>
        private string GetFormattedUser(ProxyUser u)
        {
            Initialize();
            if (this._claimmode == ProxyClaimsMode.Windows)
                return u.SamAaccount;
            else
                return string.Format("i:0e.t|{0}|{1}", _wrappermanager.Trustedissuer, u.UserPrincipalName);
        }

        /// <summary>
        /// DoSortAllResults method implementation
        /// </summary>
        private ProxyResults DoSortAllResults(ProxyResults res)
        {
            if (res != null)
            {
                res.Results.Sort(new ProxyResultObjectSort());
                res.Nodes.Sort(new ProxyResultsNodeSort());
                foreach (ProxyResultsNode nd in res.Nodes)
                {
                    DoSortResults(nd);
                }
            }
            return res;
        }

        /// <summary>
        /// DoSortResults method implementation
        /// </summary>
        private void DoSortResults(ProxyResultsNode res)
        {
            if (res != null)
            {
                res.Results.Sort(new ProxyResultObjectSort());
                res.Nodes.Sort(new ProxyResultsNodeSort());
                foreach (ProxyResultsNode nd in res.Nodes)
                {
                    DoSortResults(nd);
                }
            }
        }

        /// <summary>
        /// FillAdditionalClaims method implementation
        /// </summary>
        public List<ProxyClaims> FillAdditionalClaims(string entity)
        {
            try
            {
                using (SPMonitoredScope scp = new SPMonitoredScope("FillAdditionalClaims"))
                {
                    Initialize();
                    return _wrappermanager.FillAdditionalClaims(entity);
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, ResourcesValues.GetString("E20008"), EventLogEntryType.Error, 20008);
                return null;
            }
        }

        /// <summary>
        /// Reload method implementation
        /// </summary>
        public bool Reload()
        {
            using (SPMonitoredScope scp = new SPMonitoredScope("Reload"))
            {
                try
                {
                    Initialize();
                    _wrappermanager.Reload();
                    return true;
                }
                catch (Exception E)
                {
                    LogEvent.Log(E, ResourcesValues.GetString("E20006"), EventLogEntryType.Error, 20006);
                    return false;
                }
            }
        }

        /// <summary>
        /// ClearCache method implementation
        /// </summary>
        public bool ClearCache()
        {
            using (SPMonitoredScope scp = new SPMonitoredScope("ClearCache"))
            {
                try
                {
                    Database.ZapCache();
                    return true;
                }
                catch (Exception E)
                {
                    LogEvent.Log(E, ResourcesValues.GetString("E20006"), EventLogEntryType.Error, 20006);
                    return false;
                }
            }
        }

        /// <summary>
        /// LaunchStartCommand method implementation
        /// </summary>
        public void LaunchStartCommand()
        {
            using (SPMonitoredScope scp = new SPMonitoredScope("LaunchStartCommand"))
            {
                try
                {
                    Initialize();
                    _wrappermanager.LaunchStartCommand();
                }
                catch (Exception E)
                {
                    LogEvent.Log(E, ResourcesValues.GetString("E20007"), EventLogEntryType.Error, 20007);
                    return;
                }
            }
        }

        /// <summary>
        /// GetServiceApplicationName method implementation
        /// </summary>
        public string GetServiceApplicationName()
        {
            return this.Name;
        }
        #endregion

        #region Custom Administration Methods
        /// <summary>
        /// GetGlobalParameterList method implementation
        /// </summary>
        public IEnumerable<GlobalParameter> GetGlobalParameterList()
        {
            CheckFullControlAccess();
            IEnumerable<GlobalParameter> res = null;
            using (SPMonitoredScope scp = new SPMonitoredScope("GetGlobalParameterList"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.GetGlobalParameterList();
                });
                return res;
            }
        }

        /// <summary>
        /// SetGlobalParameter method implementation
        /// </summary>
        public bool SetGlobalParameter(GlobalParameter cfg, GlobalParameter newcfg)
        {
            bool res = false;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("SetGlobalParameter"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.SetGlobalParameter(cfg, newcfg);
                });
                return res;
            }
        }


        /// <summary>
        /// GetAssemblyConfigurationList method implementation
        /// </summary>
        public IEnumerable<AssemblyConfiguration> GetAssemblyConfigurationList()
        {
            CheckFullControlAccess();
            IEnumerable<AssemblyConfiguration> res = null;
            using (SPMonitoredScope scp = new SPMonitoredScope("GetAssemblyConfigurationList"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.GetAssemblyConfigurationList();
                });
                return res;
            }
        }

        /// <summary>
        /// GetAssemblyConfiguration method implementation
        /// </summary>
        public AssemblyConfiguration GetAssemblyConfiguration()
        {
            CheckFullControlAccess();
            AssemblyConfiguration res = null;
            using (SPMonitoredScope scp = new SPMonitoredScope("GetAssemblyConfiguration"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.GetAssemblyConfiguration();
                });
                return res;
            }
        }

        /// <summary>
        /// GetAssemblyConfiguration method implementation
        /// </summary>
        public IEnumerable<AssemblyConfiguration> GetAssemblyConfigurations()
        {
            CheckFullControlAccess();
            IEnumerable<AssemblyConfiguration> res = null;
            using (SPMonitoredScope scp = new SPMonitoredScope("GetAssemblyConfigurations"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    res = Database.GetAssemblyConfigurations();
                });
                return res;
            }
        }

        /// <summary>
        /// SetAssemblyConfiguration method implementation
        /// </summary>
        public bool SetAssemblyConfiguration(AssemblyConfiguration cfg, AssemblyConfiguration newcfg)
        {
            bool res = false;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("SetAssemblyConfiguration"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.SetAssemblyConfiguration(cfg, newcfg);
                });
                return res;
            }
        }

        /// <summary>
        /// SetAssemblyConfiguration method implementation
        /// </summary>
        public bool DeleteAssemblyConfiguration(AssemblyConfiguration cfg)
        {
            bool res = false;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("DeleteAssemblyConfiguration"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.DeleteAssemblyConfiguration(cfg);
                });
                return res;
            }
        }

        /// <summary>
        /// GetConnectionConfiguration method implementation
        /// </summary>
        public ConnectionConfiguration GetConnectionConfiguration(string name)
        {
            ConnectionConfiguration res = null;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("GetConnectionConfiguration"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.GetConnectionConfiguration(name);
                });
                return res;
            }
        }

        /// <summary>
        /// GetConnectionConfigurationList method implementation
        /// </summary>
        public IEnumerable<ConnectionConfiguration> GetConnectionConfigurationList()
        {
            IEnumerable<ConnectionConfiguration> res = null;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("GetConnectionConfigurationList"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.GetConnectionConfigurationList();
                });
                return res;
            }
        }

        /// <summary>
        /// SetConnectionConfiguration method implementation
        /// </summary>
        public bool SetConnectionConfiguration(ConnectionConfiguration config, ConnectionConfiguration newconfig)
        {
            bool res = false;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("SetConnectionConfiguration"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    string key = IdentityServiceCertificate.GetSharePointCertificate();
                    try
                    {
                        if (config != null)
                        {
                            string cleartext = PasswordManager.Decrypt(config.Password, key);
                            config.Password = PasswordManager.Encrypt(cleartext, key);
                        }
                        if (newconfig != null)
                        {
                            newconfig.Password = PasswordManager.Encrypt(newconfig.Password, key);
                        }
                    }
                    catch
                    {
                        newconfig.Password = PasswordManager.Encrypt(newconfig.Password, key);
                    }
                    res =  Database.SetConnectionConfiguration(config, newconfig);
                });
                return res;
            }
        }

        /// <summary>
        /// DeleteConnectionConfiguration method implementation
        /// </summary>
        public bool DeleteConnectionConfiguration(ConnectionConfiguration config)
        {
            bool res = false;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("DeleteConnectionConfiguration"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.DeleteConnectionConfiguration(config);
                });
                return res;
            }
        }

        /// <summary>
        /// GetDomainConfigurationList method implementation
        /// </summary>
        public IEnumerable<DomainConfiguration> GetDomainConfigurationList()
        {
            IEnumerable<DomainConfiguration> res = null;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("GetDomainConfigurationList"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.GetDomainConfigurationList();
                });
                return res;
            }
        }

        /// <summary>
        /// GetDomainConfiguration method implementation
        /// </summary>
        public DomainConfiguration GetDomainConfiguration(string name)
        {
            DomainConfiguration res = null;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("GetDomainConfiguration"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.GetDomainConfiguration(name);
                });
                return res;
            }
        }

        /// <summary>
        /// SetDomainConfiguration method implementation
        /// </summary>
        public bool SetDomainConfiguration(DomainConfiguration cfg, DomainConfiguration newcfg)
        {
            bool res = false;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("SetDomainConfiguration"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.SetDomainConfiguration(cfg, newcfg);
                });
                return res;
            }
        }

        /// <summary>
        /// DeleteDomainConfiguration method implementation
        /// </summary>
        public bool DeleteDomainConfiguration(DomainConfiguration cfg)
        {
            bool res = false;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("DeleteDomainConfiguration"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.DeleteDomainConfiguration(cfg);
                });
                return res;
            }
        }

        /// <summary>
        /// GetGeneralParameters method implementation
        /// </summary>
        public List<ProxyGeneralParameter> FillGeneralParameters()
        {
            try
            {
                CheckFullControlAccess();
                using (SPMonitoredScope scp = new SPMonitoredScope("FillGeneralParameters"))
                {

                    List<ProxyGeneralParameter> result = new List<ProxyGeneralParameter>();
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        IEnumerable<GeneralParameter> tmp = Database.GetGeneralParameters();
                        foreach (GeneralParameter t in tmp)
                        {
                            ProxyGeneralParameter x = new ProxyGeneralParameter();
                            x.ParamName = t.ParamName;
                            x.ParamValue = t.ParamValue;
                            result.Add(x);
                        }
                    });
                    return result;
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, ResourcesValues.GetString("E20009"), EventLogEntryType.Error, 20009);
                return null;
            }
        }

        /// <summary>
        /// FillClaimsProviderParameters method implementation
        /// </summary>
        public ProxyClaimsProviderParameters FillClaimsProviderParameters()
        {
            try
            {
                CheckFullControlAccess();
                using (SPMonitoredScope scp = new SPMonitoredScope("FillClaimsProviderParameters"))
                {
                    ProxyClaimsProviderParameters result = new ProxyClaimsProviderParameters();
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        IEnumerable<GeneralParameter> tmp = Database.GetGeneralParameters();
                        foreach (GeneralParameter t in tmp)
                        {
                            if (t.ParamName.ToLower().Trim().Equals("claimsmode"))
                            {
                                if (t.ParamValue.ToLower().Trim().Equals("windows"))
                                    result.ClaimProviderMode = ProxyClaimsMode.Windows;
                                else
                                    result.ClaimProviderMode = ProxyClaimsMode.Federated;
                                break;
                            }
                        }
                        foreach (GeneralParameter t in tmp)
                        {
                            if (t.ParamName.ToLower().Trim().Equals("claimidentitymode"))
                            {
                                if (result.ClaimProviderMode == ProxyClaimsMode.Windows)
                                    result.ClaimProviderIdentityMode = ProxyClaimsIdentityMode.SAMAccount;
                                else
                                {
                                    if (t.ParamValue.ToLower().Trim().Equals("email")) 
                                        result.ClaimProviderIdentityMode = ProxyClaimsIdentityMode.Email;
                                    else if (t.ParamValue.ToLower().Trim().Equals("samaccount")) 
                                        result.ClaimProviderIdentityMode = ProxyClaimsIdentityMode.SAMAccount;
                                    else
                                        result.ClaimProviderIdentityMode = ProxyClaimsIdentityMode.UserPrincipalName;
                                }
                            }
                            else if (t.ParamName.ToLower().Trim().Equals("claimidentity"))
                                result.ClaimProviderIdentityClaim = t.ParamValue;
                            else if (t.ParamName.ToLower().Trim().Equals("claimrolemode"))
                            {
                                if (result.ClaimProviderMode == ProxyClaimsMode.Windows)
                                    result.ClaimProviderRoleMode = ProxyClaimsRoleMode.SID;
                                else
                                {
                                    if (t.ParamValue.ToLower().Trim().Equals("role"))
                                        result.ClaimProviderRoleMode = ProxyClaimsRoleMode.Role;
                                    else
                                        result.ClaimProviderRoleMode = ProxyClaimsRoleMode.SID;
                                }
                            }
                            else if (t.ParamName.ToLower().Trim().Equals("claimrole"))
                                result.ClaimProviderRoleClaim = t.ParamValue;
                            else if (t.ParamName.ToLower().Trim().Equals("claimdisplaymode"))
                            {
                                if (t.ParamValue.ToLower().Trim().Equals("displayname"))
                                    result.ClaimsProviderDisplayMode = ProxyClaimsDisplayMode.DisplayName;
                                else if (t.ParamValue.ToLower().Trim().Equals("email"))
                                    result.ClaimsProviderDisplayMode = ProxyClaimsDisplayMode.Email;
                                else if (t.ParamValue.ToLower().Trim().Equals("upn"))
                                    result.ClaimsProviderDisplayMode = ProxyClaimsDisplayMode.UPN;
                                else if (t.ParamValue.ToLower().Trim().Equals("samaccount"))
                                    result.ClaimsProviderDisplayMode = ProxyClaimsDisplayMode.SAMAccount;
                                else
                                    result.ClaimsProviderDisplayMode = ProxyClaimsDisplayMode.UPN;
                            }
                            else if (t.ParamName.ToLower().Trim().Equals("peoplepickerdisplaymode"))
                            {
                                if (t.ParamValue.ToLower().Trim().Equals("displayname"))
                                    result.ClaimsProviderPeoplePickerMode = ProxyClaimsDisplayMode.DisplayName;
                                else if (t.ParamValue.ToLower().Trim().Equals("email"))
                                    result.ClaimsProviderPeoplePickerMode = ProxyClaimsDisplayMode.Email;
                                else if (t.ParamValue.ToLower().Trim().Equals("upn"))
                                    result.ClaimsProviderPeoplePickerMode = ProxyClaimsDisplayMode.UPN;
                                else if (t.ParamValue.ToLower().Trim().Equals("samaccount"))
                                    result.ClaimsProviderPeoplePickerMode = ProxyClaimsDisplayMode.SAMAccount;
                                else if (t.ParamValue.ToLower().Trim().Equals("displaynameandemail"))
                                    result.ClaimsProviderPeoplePickerMode = ProxyClaimsDisplayMode.DisplayNameAndEmail;
                                else
                                    result.ClaimsProviderPeoplePickerMode = ProxyClaimsDisplayMode.UPN;
                            }
                            else if (t.ParamName.ToLower().Trim().Equals("peoplepickerimages"))
                            {
                                result.PeoplePickerImages = bool.Parse(t.ParamValue);
                            }
                            else if (t.ParamName.ToLower().Trim().Equals("trustedloginprovidername"))
                            {
                                result.TrustedLoginProviderName = t.ParamValue;
                            }
                            else if (t.ParamName.ToLower().Trim().Equals("claimprovidername"))
                            {
                                result.ClaimProviderName = t.ParamValue;
                            }
                            else if (t.ParamName.ToLower().Trim().Equals("claimdisplayname"))
                            {
                                result.ClaimDisplayName = t.ParamValue;
                            }
                        }
                    });
                    return result;
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, ResourcesValues.GetString("E20010"), EventLogEntryType.Error, 20010);
                return null;
            }
        }

        /// <summary>
        /// GetGeneralParameters method implementation
        /// </summary>
        public string GetGeneralParameter(string paramname)
        {
            string res = string.Empty;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("GetGeneralParameter"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.GetGeneralParameter(paramname);
                });
                return res;
            }
        }

        /// <summary>
        /// SetGeneralParameters method implementation
        /// </summary>
        public bool SetGeneralParameter(string paramname, string paramvalue)
        {
            bool res = false;
            CheckFullControlAccess();
            using (SPMonitoredScope scp = new SPMonitoredScope("SetGeneralParameter"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    res = Database.SetGeneralParameter(paramname, paramvalue);
                });
                return res;
            }
        }

        /// <summary>
        /// GetProfileManager method implementation
        /// </summary>
        /// <returns></returns>
        private UserProfileManager GetProfileManager()
        {
            if (_profilemanager == null)
            {
                _profilemanager = new UserProfileManager(true);
            }
            return _profilemanager;
        }

        /// <summary>
        /// GetUserProfile method implementation
        /// </summary>
        [DebuggerNonUserCode]
        public string GetUserImage(string account)
        {
            string resurl = null;
            try
            {
                UserProfile prf = null;
                UserProfileManager mgr = null;
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    mgr = GetProfileManager();
                    if (mgr != null)
                        prf = mgr.GetUserProfile(account);
                    if (prf != null)
                    {
                        resurl = (string)prf[PropertyConstants.PictureUrl].Value;
                    }
                    else
                        resurl = string.Empty;
                });
                return resurl;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region Jobs
        /// <summary>
        /// WarmupJobDefinitions property implementation
        /// </summary>
        private IEnumerable<IdentityServiceApplicationJobDefinition> WarmupJobDefinitions
        {
            get
            {
                foreach (SPJobDefinition job in Service.JobDefinitions)
                {
                    var iteratorJob = job as IdentityServiceApplicationJobDefinition;
                    if ((iteratorJob != null) && (iteratorJob.ServiceApplicationId == Id))
                    {
                        yield return iteratorJob;
                    }
                }
            }
        }

        /// <summary>
        /// ReloadJobDefinitions property implementation
        /// </summary>
        private IEnumerable<IdentityServiceApplicationReloadJobDefinition> ReloadJobDefinitions
        {
            get
            {
                foreach (SPJobDefinition job in Service.JobDefinitions)
                {
                    var iteratorJob = job as IdentityServiceApplicationReloadJobDefinition;
                    if ((iteratorJob != null) && (iteratorJob.ServiceApplicationId == Id))
                    {
                        yield return iteratorJob;
                    }
                }
            }
        }

        /// <summary>
        /// StopJobs method implementation
        /// </summary>
        internal void StopJobs()
        {
            foreach (IdentityServiceApplicationReloadJobDefinition job in ReloadJobDefinitions)
            {
                if ((job != null))
                {
                    job.IsDisabled = true;
                    job.Update();
                    break;
                }
            }
            foreach (IdentityServiceApplicationJobDefinition job in WarmupJobDefinitions)
            {
                if ((job != null))
                {
                    job.IsDisabled = true;
                    job.Update();
                    break;
                }
            }
        }

        /// <summary>
        /// StartJobs method implementation
        /// </summary>
        internal void StartJobs()
        {
            foreach (IdentityServiceApplicationJobDefinition job in WarmupJobDefinitions)
            {
                if ((job != null))
                {
                    job.IsDisabled = false;
                    job.Update();
                    break;
                }
            }
            foreach (IdentityServiceApplicationReloadJobDefinition job in ReloadJobDefinitions)
            {
                if ((job != null))
                {
                    job.IsDisabled = false;
                    job.Update();
                    break;
                }
            }
        }

        /// <summary>
        /// InstallJobs() method implementation
        /// </summary>
        internal void InstallJobs()
        {
           // EnsureAccess(base.Farm.TimerService.ProcessIdentity);
            foreach (Type type in IdentityServiceApplicationJobDefinition.Types)
            {
                if (!JobExists(type))
                {
                    InstallJob(type);
                }
            }
            foreach (Type type in IdentityServiceApplicationReloadJobDefinition.Types)
            {
                if (!JobExists(type))
                {
                    InstallJob(type);
                }
            }
        }

        /// <summary>
        /// InstallJob method implementation
        /// </summary>
        private void InstallJob(Type type)
        {
            if (type == typeof(IdentityServiceApplicationJobDefinition))
            {
                IdentityServiceApplicationJobDefinition job = new IdentityServiceApplicationJobDefinition(this, this.Name);
                if (job != null)
                {
                    job.Schedule = job.DefaultSchedule;
                    job.Update();
                }
            }
            if (type == typeof(IdentityServiceApplicationReloadJobDefinition))
            {
                IdentityServiceApplicationReloadJobDefinition job = new IdentityServiceApplicationReloadJobDefinition(this, this.Name);
                if (job != null)
                {
                    job.Schedule = job.DefaultSchedule;
                    job.Update();
                }
            }
        }

        /// <summary>
        /// UpgradeJobs method implementation
        /// </summary>
        internal void UpgradeJobs()
        {
            foreach (Type type in IdentityServiceApplicationJobDefinition.Types)
            {
                if (JobExists(type))
                {
                    UpgradeJob(type);
                }
                else
                {
                    InstallJob(type);
                }
            }
            foreach (Type type in IdentityServiceApplicationReloadJobDefinition.Types)
            {
                if (JobExists(type))
                {
                    UpgradeJob(type);
                }
                else
                {
                    InstallJob(type);
                }
            }
        }

        /// <summary>
        /// UpgradeJob method implementation
        /// </summary>
        private void UpgradeJob(Type type)
        {
            if (type == typeof(IdentityServiceApplicationJobDefinition))
            {
                var existingJob = GetJobOrNull(type);
                if (existingJob != null)
                {
                    IdentityServiceApplicationJobDefinition job = new IdentityServiceApplicationJobDefinition(this, this.Name);
                    if (job != null)
                    {
                        existingJob.Title = job.Title;
                        existingJob.Update();
                    }
                }
            }
            if (type == typeof(IdentityServiceApplicationReloadJobDefinition))
            {
                var rexistingJob = GetReloadJobOrNull(type);
                if (rexistingJob != null)
                {
                    IdentityServiceApplicationReloadJobDefinition job = new IdentityServiceApplicationReloadJobDefinition(this, this.Name);
                    if (job != null)
                    {
                        rexistingJob.Title = job.Title;
                        rexistingJob.Update();
                    }
                }
            }
        }

        /// <summary>
        /// RemoveJobs method implementation
        /// </summary>
        internal void RemoveJobs()
        {
            foreach (IdentityServiceApplicationJobDefinition job in WarmupJobDefinitions)
            {
                job.Delete();
            }
            foreach (IdentityServiceApplicationReloadJobDefinition job in ReloadJobDefinitions)
            {
                job.Delete();
            }
        }

        /// <summary>
        ///JobExists method implementation
        /// </summary>
        internal bool JobExists(Type type)
        {
            if (type == typeof(IdentityServiceApplicationJobDefinition))
            {
                foreach (IdentityServiceApplicationJobDefinition job in WarmupJobDefinitions)
                {
                    if (type == job.GetType())
                    {
                        return true;
                    }
                }
            }
            if (type == typeof(IdentityServiceApplicationReloadJobDefinition))
            {
                foreach (IdentityServiceApplicationReloadJobDefinition job in ReloadJobDefinitions)
                {
                    if (type == job.GetType())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// GetJob method implementation
        /// </summary>
        internal T GetJob<T>() where T : IdentityServiceApplicationJobDefinition
        {
            foreach (IdentityServiceApplicationJobDefinition job in WarmupJobDefinitions)
            {
                var local = job as T;
                if (local != null)
                {
                    return local;
                }
            }
            return default(T);
        }

        /// <summary>
        /// GetJob method implementation
        /// </summary>
        internal T GetReloadJob<T>() where T : IdentityServiceApplicationReloadJobDefinition
        {
            foreach (IdentityServiceApplicationReloadJobDefinition job in ReloadJobDefinitions)
            {
                var local = job as T;
                if (local != null)
                {
                    return local;
                }
            }
            return default(T);
        }

        internal IdentityServiceApplicationJobDefinition GetJobOrNull(Type type)
        {
            foreach (IdentityServiceApplicationJobDefinition job in WarmupJobDefinitions)
            {
                if (type == job.GetType())
                {
                    return job;
                }
            }
            return null;
        }

        /// <summary>
        /// GetReloadJobOrNull method implementation
        /// </summary>
        internal IdentityServiceApplicationReloadJobDefinition GetReloadJobOrNull(Type type)
        {
            foreach (IdentityServiceApplicationReloadJobDefinition job in ReloadJobDefinitions)
            {
                if (type == job.GetType())
                {
                    return job;
                }
            }
            return null;
        }
        #endregion
     }

    #region Sort Classes
    /// <summary>
    /// ProxyResultDomainsSort class
    /// </summary>
    public class ProxyResultDomainsSort: Comparer<ProxyDomain>
    {
        public override int Compare(ProxyDomain x, ProxyDomain y)
        {
            if (x.Position < y.Position)
                return -1;
            else if (x.Position > y.Position)
                return 1;
            else
            {
                if (x.DisplayName == null)
                    x.DisplayName = x.DnsName;
                if (y.DisplayName == null)
                    y.DisplayName = y.DnsName;
                return string.Compare(x.DisplayName, y.DisplayName);
            }
        }
    }

    /// <summary>
    /// ProxyResultObjectSort class
    /// </summary>
    public class ProxyResultObjectSort : Comparer<ProxyResultObject>
    {
        public override int Compare(ProxyResultObject x, ProxyResultObject y)
        {
            if ((x is ProxyUser) && (y is ProxyRole))
                return -1;
            else if ((y is ProxyUser) && (x is ProxyRole))
                return 1;
            else
            {
                if (x.DisplayName == null)
                    x.DisplayName = string.Empty; //x.DomainName;
                if (y.DisplayName == null)
                    y.DisplayName = string.Empty; //y.DomainName;
                return string.Compare(x.DisplayName, y.DisplayName);
            }
        }
    }

    /// <summary>
    /// ProxyResultsNodeSort Class
    /// </summary>
    public class ProxyResultsNodeSort: Comparer<ProxyResultsNode>
    {
        public override int Compare(ProxyResultsNode x, ProxyResultsNode y)
        {
            if (x.Position < y.Position)
                return -1;
            else if (x.Position > y.Position)
                return 1;
            else
            {
                if (x.DisplayName == null)
                    x.DisplayName = x.Name;
                if (y.DisplayName == null)
                    y.DisplayName = y.Name;
                return string.Compare(x.DisplayName, y.DisplayName);
            }
        }
    }
    #endregion

    #region SPMapToWindows internal class
    /// <summary>
    /// SMapToWindows Class
    /// </summary>
    internal class SPMapToWindows
    {
        internal List<ProxyClaims> GetWindowsMappedClaims(string entity)
        {
            List<ProxyClaims> result = new List<ProxyClaims>();
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    string upn = entity.Substring(entity.LastIndexOf("|") + 1);
                    WindowsIdentity id = S4UClient.UpnLogon(upn);
                    if (id != null)
                    {
                        result.Add(new ProxyClaims(false, true, "http://schemas.microsoft.com/sharepoint/2009/08/claims/identityprovider", "windows"));   // SharePoint Issuer
                        result.Add(new ProxyClaims(true, false, "http://schemas.microsoft.com/sharepoint/2009/08/claims/userlogonname", id.Name));        // Windows Isser
                        result.Add(new ProxyClaims(true, false, "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid", id.User.Value));    // Windows Isser
                        result.Add(new ProxyClaims(false, true, "http://schemas.microsoft.com/sharepoint/2009/08/claims/userid", @"0#.w|" + id.Name));    // SharePoint Issuer
                    }
                });
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    #endregion

    #region ClaimsAugmenter internal class
    /// <summary>
    /// ClaimsAugmenter class
    /// </summary>
    public class ClaimsAugmenter : IIdentityServiceClaimsAugmenter
    {
        public List<ProxyClaims> FillAdditionalClaims(string entity)
        {
            List<ProxyClaims> result = new List<ProxyClaims>();
            result.Add(new ProxyClaims(false, true, "http://schemas.xmlsoap.org/sp/2013/02/identity/claims/trademark", "powered and secured by Neos-Sdi"));   // SharePoint Issuer
            return result;
        }
    }
    #endregion

    #region WrapperManager internal class
    /// <summary>
    /// Wrapper Manager
    /// </summary>
    internal class WrapperManager: IWrapperNoInit
    {
        private static object _lockobj = new Object();
        private string _trustedissuer = string.Empty;
        private bool _withimages = false;
        private string _claimprovidername;
        private string _cacheduration = "15";
        private ProxyClaimsMode _claimmode = ProxyClaimsMode.Windows;
        private IIdentityServiceClaimsAugmenter _augmenter = null;
        private Type _typeaugmenter = null;

        private List<ConnectorConfigurationWrapper> _assemblywrappers = null;
        private IEnumerable<AssemblyConfiguration> _assemblyconfigs = null;
        private List<ProxyFullConfiguration> _configs = null;
        private List<ProxyGeneralParameter> _params = null;
        private bool _isinitialized = false;
        private ActiveDirectoryIdentityServiceDatabase _database;


        public WrapperManager(ActiveDirectoryIdentityServiceDatabase db)
        {
            Database = db;
        }

        #region IWrapperNoInit
        /// <summary>
        /// ClaimsProviderName method implmentation
        /// </summary>
        public string ClaimsProviderName
        {
            get { return _claimprovidername; }
            set { _claimprovidername = value; }
        }

        /// <summary>
        /// Trustedissuer method implementation
        /// </summary>
        public string Trustedissuer
        {
            get { return _trustedissuer; }
            set { _trustedissuer = value; }
        }

        /// <summary>
        /// Database property implementation
        /// </summary>
        public ActiveDirectoryIdentityServiceDatabase Database
        {
            get { return _database; }
            set { _database = value; }
        }

        /// <summary>
        /// Assemblywrappers property implmentation
        /// </summary>
        internal List<ConnectorConfigurationWrapper> Assemblywrappers
        {
            get { return _assemblywrappers; }
            set { _assemblywrappers = value; }
        }

        /// <summary>
        /// Assemblyconfigs property implementation
        /// </summary>
        internal IEnumerable<AssemblyConfiguration> Assemblyconfigs
        {
            get { return _assemblyconfigs; }
            set { _assemblyconfigs = value; }
        }

        /// <summary>
        /// Configurations property implementation
        /// </summary>
        internal List<ProxyFullConfiguration> Configurations
        {
            get { return _configs; }
            set { _configs = value; }
        }

        /// <summary>
        /// GlobalParameters property implementation
        /// </summary>
        internal List<ProxyGeneralParameter> GlobalParameters
        {
            get { return _params; }
            set { _params = value; }
        }

        /// <summary>
        /// EnsureLoaded method implementation
        /// </summary>
        public void EnsureLoaded()
        {
            DoInitialize(false);
        }

        #region Fill methods
        /// <summary>
        /// FillSearch method implementation
        /// </summary>
        public ProxyResults FillSearch(string pattern, string domain, bool recursive)
        {
            ProxyResults result = null;
            EnsureLoaded();
            foreach (ConnectorConfigurationWrapper wr in Assemblywrappers)
            {
                ProxyResults res = wr.Wrapper.FillSearch(pattern, domain, recursive);
                if (res != null)
                {
                    if (result == null)
                        result = res;
                    else
                        result.Merge(res);
                }
            }
            return result;
        }

        /// <summary>
        /// FillResolve method implementation
        /// </summary>
        public ProxyResults FillResolve(string pattern, bool recursive)
        {
            ProxyResults result = null;
            EnsureLoaded();
            foreach (ConnectorConfigurationWrapper wr in Assemblywrappers)
            {
                ProxyResults res = wr.Wrapper.FillResolve(pattern, recursive);
                if (res != null)
                {
                    if (result == null)
                        result = res;
                    else
                        result.Merge(res);
                }
            }
            return result;
        }

        /// <summary>
        /// FillValidate method implementation
        /// </summary>
        public ProxyResults FillValidate(string pattern, bool recursive)
        {
            ProxyResults result = null;
            EnsureLoaded();
            foreach (ConnectorConfigurationWrapper wr in Assemblywrappers)
            {
                ProxyResults res = wr.Wrapper.FillValidate(pattern, recursive);
                if (res != null)
                {
                    if (result == null)
                        result = res;
                    else
                        result.Merge(res);
                }
            }
            return result;
        }

        /// <summary>
        /// FillHierarchy method implementation
        /// </summary>
        public ProxyDomain FillHierarchy(string hierarchyNodeID, int numberOfLevels)
        {
            ProxyDomain result = null;
            EnsureLoaded();
            foreach (ConnectorConfigurationWrapper wr in Assemblywrappers)
            {
                ProxyDomain res = wr.Wrapper.FillHierarchy(hierarchyNodeID, numberOfLevels);
                if (res != null)
                {
                    if (result == null)
                        result = res;
                    else
                        result.Merge(res);
                }
            }
            return result;

        }

        /// <summary>
        /// FillBadDomains method implementation
        /// </summary>
        /// <returns></returns>
        public List<ProxyBadDomain> FillBadDomains()
        {
            List<ProxyBadDomain> result = null;
            EnsureLoaded();
            foreach (ConnectorConfigurationWrapper wr in Assemblywrappers)
            {
                List<ProxyBadDomain> res = wr.Wrapper.FillBadDomains();
                if (res != null)
                {
                    if (result == null)
                        result = res;
                    else
                        result.AddRange(res);
                }
            }
            return result;
        }

        /// <summary>
        /// FillAdditionalClaims method implementation
        /// </summary>
        public List<ProxyClaims> FillAdditionalClaims(string entity)
        {
            try
            {
                EnsureLoaded();
                SPMapToWindows map = new SPMapToWindows();
                List<ProxyClaims> _map = map.GetWindowsMappedClaims(entity);
                if (_augmenter != null)
                {
                    try
                    {
                        List<ProxyClaims> _tmp = _augmenter.FillAdditionalClaims(entity);
                        if (_tmp != null)
                            _map.AddRange(_tmp);
                    }
                    catch (Exception E)
                    {
                        Log(E, ResourcesValues.GetString("E20008"), EventLogEntryType.Error, 20008);
                    }
                }
                return _map;
            }
            catch (Exception E)
            {
                Log(E, ResourcesValues.GetString("E20008"), EventLogEntryType.Error, 20008);
                return null;
            }
        }
        #endregion

        /// <summary>
        /// Reload method implementation
        /// </summary>
        public void Reload()
        {
            DoInitialize(true);
        }

        public void LaunchStartCommand()
        {
            DoInitialize(false);
        }

        /// <summary>
        /// Log method implementation
        /// </summary>
        public void Log(Exception ex, string message, EventLogEntryType eventLogEntryType, int eventid = 0)
        {
            LogEvent.Log(ex, message, eventLogEntryType, eventid);
        }

        /// <summary>
        /// Trace method implementation
        /// </summary>
        public void Trace(string message, EventLogEntryType eventLogEntryType, int eventid = 0)
        {
            LogEvent.Trace(message, eventLogEntryType, eventid);
        }
        #endregion

        #region Initialize methods
        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private void Initialize()
        {
            if (!_isinitialized)
            {
                Assemblyconfigs = Database.GetAssemblyConfigurations();
                GlobalParameters = GetGeneralParameters(Database.GetGeneralParameters());
                Configurations = GetFullConfigurations(Database.GetFullConfigurations());
                Assemblywrappers = new List<ConnectorConfigurationWrapper>();
                _isinitialized = true;
            }
        }

        /// <summary>
        /// Reset method implementation
        /// </summary>
        private void Reset()
        {
            Assemblyconfigs = null;
            GlobalParameters = null;
            Configurations = null;
            Assemblywrappers = null;
            _isinitialized = false;
        }

        /// <summary>
        /// Doinitialize method implementation
        /// </summary>
        /// <param name="reset"></param>
        public void DoInitialize(bool reset = false)
        {
            lock (_lockobj)
            {
                if (reset)
                    Reset();
                if (_isinitialized)
                    return;
                using (SPMonitoredScope scp = new SPMonitoredScope("DoInitialize"))
                {
                    try
                    {
                        Initialize();
                        try
                        {
                            Loadwrappers(); 
                            LoadExtend();

                            string val = Database.GetGeneralParameter("CacheDuration");
                            if (string.IsNullOrEmpty(val))
                                val = "15";
                            ResetAccessCache(Convert.ToDouble(val));
                            try
                            {
                                if (CheckInitializeAccess(val))
                                    InitializeFromDatabase();
                                else
                                    InitializeFromCache();
                            }
                            catch (Exception E)
                            {
                                Database.ZapCache();
                                throw E;
                            }

                        }
                        catch (Exception E)
                        {
                            Database.ZapCache();
                            throw E;
                        }
                    }
                    catch (Exception E)
                    {
                        LogEvent.Log(E, E.Message, EventLogEntryType.Error, 20000);
                        throw E;
                    }
                }
            }
        }


        /// <summary>
        /// LoadWrappers method implementation
        /// </summary>
        public void Loadwrappers()
        {
            using (SPMonitoredScope scp = new SPMonitoredScope("LoadWrappers"))
            {
                foreach (AssemblyConfiguration ass in Assemblyconfigs)
                {
                    ConnectorConfigurationWrapper wrp = null;
                    if (this.Assemblywrappers.Count>0)
                       wrp = (from wrappers in this.Assemblywrappers where  wrappers.ConnectorID == ass.ID select wrappers).FirstOrDefault<ConnectorConfigurationWrapper>();

                    if (wrp != null)
                    {
                        if (ass.Selected)
                        {
                            if (wrp.Wrapper == null)
                            {
                                Assembly assembly = Assembly.Load(ass.AssemblyFulldescription);
                                Type typetoload = assembly.GetType(ass.AssemblyTypeDescription);
                                object o = Activator.CreateInstance(typetoload);
                                if (o != null)
                                {
                                    IWrapper _wrapper = o as IWrapper;
                                    wrp = ass;
                                    wrp.Wrapper = _wrapper;
                                    SplitParameters(wrp.Wrapper, this.GlobalParameters);
                                    wrp.Wrapper.Initialize(SplitFullConfigurations(this.Configurations, wrp.ConnectorID), this.GlobalParameters);
                                    wrp.Wrapper.EnsureLoaded();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ass.Selected)
                        {
                            Assembly assembly = Assembly.Load(ass.AssemblyFulldescription);
                            Type typetoload = assembly.GetType(ass.AssemblyTypeDescription);
                            object o = Activator.CreateInstance(typetoload);
                            if (o != null)
                            {
                                ConnectorConfigurationWrapper newwrp = new ConnectorConfigurationWrapper(ass.ID, ass.AssemblyFulldescription, ass.AssemblyTypeDescription, ass.TraceResolve);
                                IWrapper _wrapper = o as IWrapper;
                                newwrp.Wrapper = _wrapper;
                                SplitParameters(newwrp.Wrapper, this.GlobalParameters);
                                newwrp.Wrapper.Initialize(SplitFullConfigurations(this.Configurations, newwrp.ConnectorID), this.GlobalParameters);
                                newwrp.Wrapper.EnsureLoaded();
                                Assemblywrappers.Add(newwrp);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// LoadExtend method implementation
        /// </summary>
        public void LoadExtend()
        {
            try
            {
                AssemblyConfiguration cfg = Database.GetAssemblyAugmenter();
                if (cfg != null)
                {
                    if (_typeaugmenter == null)
                    {
                        Assembly assembly = Assembly.Load(cfg.AssemblyFulldescription);
                        _typeaugmenter = assembly.GetType(cfg.AssemblyTypeDescription);
                        if (_typeaugmenter.IsClass && !_typeaugmenter.IsAbstract && _typeaugmenter.GetInterface("IIdentityServiceClaimsAugmenter") != null)
                        {
                            object o = Activator.CreateInstance(_typeaugmenter);
                            if (o != null)
                                _augmenter = o as IIdentityServiceClaimsAugmenter;
                        }
                        else
                            _augmenter = null;
                    }
                    else
                    {
                        object o = Activator.CreateInstance(_typeaugmenter);
                        if (o != null)
                            _augmenter = o as IIdentityServiceClaimsAugmenter;
                    }
                }
            }
            catch
            {
                // Nothing, no errors, no claims augmentation
            }
        }

        /// <summary>
        /// InitializeFromDatabase method implementation
        /// </summary>
        private void InitializeFromDatabase()
        {
            string key = GetSharePointCertificate();
            Initialize();
            EnsureLoaded();

            foreach (ConnectorConfigurationWrapper cfg in Assemblywrappers)
            {
                IWrapperCaching cch = cfg.Wrapper as IWrapperCaching;
                if (cch != null)
                {
                    XmlDocument res = cch.Save();
                    string data = PasswordEncrypt(res.OuterXml, key);
                    Database.SetDataToCache(data);
                }
            }
            Database.ClearAccessToCache();
        }

        /// <summary>
        /// InitializeFromCache method implementation
        /// </summary>
        private void InitializeFromCache()
        {
            string key = GetSharePointCertificate();
            string cdata = Database.GetDataFromCache();
            string data = PasswordDecrypt(cdata, key);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);

            foreach (ConnectorConfigurationWrapper cfg in Assemblywrappers)
            {
                IWrapperCaching cch = cfg.Wrapper as IWrapperCaching;
                if (cch != null)
                    cch.Restore(doc);
            }
        }

        /// <summary>
        /// SplitFullConfigurations method implementation
        /// </summary>
        private List<ProxyFullConfiguration> SplitFullConfigurations(List<ProxyFullConfiguration> src, long connector)
        {
            List<ProxyFullConfiguration> result = (from s in src where s.ConnectorID == connector select s).ToList<ProxyFullConfiguration>();
            return result;
        }

        /// <summary>
        /// SplitParameters method implementation
        /// </summary>
        private void SplitParameters(IWrapper wr, List<ProxyGeneralParameter> src)
        {
            if (wr == null)
                return;
            ProxyGeneralParameter  result = (from s in src where s.ParamName.ToLower() == "claimdisplayname" select s).First<ProxyGeneralParameter>();
            if (result!=null)
                wr.ClaimsProviderName = result.ParamValue;
        }

        /// <summary>
        /// GetFullConfigurations method implementation
        /// </summary>
        private List<ProxyFullConfiguration> GetFullConfigurations(IEnumerable<FullConfiguration> src)
        {
            string key = GetSharePointCertificate();
            List<ProxyFullConfiguration> prxy = new List<ProxyFullConfiguration>();
            foreach (FullConfiguration c in src)
            {
                ProxyFullConfiguration p = new ProxyFullConfiguration();
                p.DisplayName = c.DisplayName;
                p.DnsName = c.DnsName;
                p.Enabled = c.Enabled;
                p.Maxrows = c.Maxrows;
                p.DisplayPosition = c.DisplayPosition;
                p.ConnectString = c.ConnectString;
                p.ConnectorID = c.ConnectorID;
                try
                {
                    p.Password = PasswordDecrypt(c.Password, key);
                }
                catch (SharePointIdentityCryptographicException)
                {
                    p.Password = c.Password;
                    ConnectionConfiguration xcfg = new ConnectionConfiguration();
                    xcfg.ConnectionName = c.Connection;
                    xcfg.Username = c.UserName;
                    xcfg.Password = PasswordEncrypt(c.Password, key);
                    xcfg.Timeout = c.Timeout;
                    xcfg.Secure = c.Secure;
                    xcfg.Maxrows = c.Maxrows;
                    xcfg.ConnectString = c.ConnectString;
                    xcfg.ConnectorID = c.ConnectorID;
                    Database.SetConnectionConfiguration(null, xcfg); // do insert
                }
                p.Secure = c.Secure;
                p.Timeout = c.Timeout;
                p.UserName = c.UserName;
                p.IsDefault = c.Connection.ToLower().Trim().Equals("default");
                prxy.Add(p);
            }
            return prxy;
        }

        /// <summary>
        /// GetGeneralParameters
        /// </summary>
        private List<ProxyGeneralParameter> GetGeneralParameters(IEnumerable<GeneralParameter> src)
        {
            List<ProxyGeneralParameter> glb = new List<ProxyGeneralParameter>();
            foreach (GeneralParameter t in src)
            {
                ProxyGeneralParameter x = new ProxyGeneralParameter();
                x.ParamName = t.ParamName;
                x.ParamValue = t.ParamValue;
                glb.Add(x);
                if (t.ParamName.ToLower().Equals("claimdisplayname"))
                    ClaimsProviderName = t.ParamValue;
                if (t.ParamName.ToLower().Equals("peoplepickerimages"))
                    this._withimages = bool.Parse(t.ParamValue);
                if (t.ParamName.ToLower().Equals("trustedloginprovidername"))
                    this.Trustedissuer = t.ParamValue;
                if (t.ParamName.ToLower().Equals("claimsmode"))
                    this._claimmode = (ProxyClaimsMode)Enum.Parse(typeof(ProxyClaimsMode), t.ParamValue);
                if (t.ParamName.ToLower().Equals("cacheduration"))
                {
                    this._cacheduration = t.ParamValue;
                    if (string.IsNullOrEmpty(this._cacheduration))
                        this._cacheduration = "15";
                    ResetAccessCache(Convert.ToDouble(this._cacheduration));
                }
            }
            return glb;
        }
        #endregion

        /// <summary>
        /// GetSharePointCertificate method implementation
        /// </summary>
        private string GetSharePointCertificate()
        {
            const string errorstr = "Cannot Access to SharePoint Certificate Store, Process cannot continue";
            string key = null;
            try
            {
                key = IdentityServiceCertificate.GetSharePointCertificate();
                if (string.IsNullOrEmpty(key))
                    throw new Exception(errorstr);
            }
            catch (Exception E)
            {
                LogEvent.Log(E, errorstr, EventLogEntryType.Error, 20000);
                throw E;
            }
            return key;
        }

        /// <summary>
        /// PasswordDecrypt method implementation
        /// </summary>
        private string PasswordDecrypt(string cdata, string key)
        {
            const string errorstr = "Cannot decrypt password, Process cannot continue";
            string data = null;
            try
            {
                data = PasswordManager.Decrypt(cdata, key);
                if (string.IsNullOrEmpty(data))
                    throw new Exception(errorstr);
            }
            catch (Exception E)
            {
                LogEvent.Log(E, errorstr, EventLogEntryType.Error, 20000);
                throw E;
            }
            return data;
        }

        /// <summary>
        /// PasswordEncrypt method implementation
        /// </summary>
        private string PasswordEncrypt(string cdata, string key)
        {
            const string errorstr = "Cannot encrypt password, Process cannot continue";
            string data = null;
            try
            {
                data = PasswordManager.Encrypt(cdata, key);
                if (string.IsNullOrEmpty(data))
                    throw new Exception(errorstr);
            }
            catch (Exception E)
            {
                LogEvent.Log(E, errorstr, EventLogEntryType.Error, 20000);
                throw E;
            }
            return data;
        }

        /// <summary>
        /// CheckInitializeAccess method implmentation
        /// </summary>
        private bool CheckInitializeAccess(string val)
        {
            if (val.ToLowerInvariant().Equals("0"))
            {
                return true;
            }

            bool trueload = true;
            bool canaccess = false;
            do
            {
                canaccess = Database.GetAccessTocache(out trueload);
                if (!canaccess)
                    Thread.Sleep(1000);
            }
            while (!canaccess);
            return trueload;
        }

        /// <summary>
        /// ResetAccessCache method implementation
        /// </summary>
        private void ResetAccessCache(double minutes)
        {
            Database.ResetAccessCache(minutes);
        }
    }
    #endregion

    #region ConfigurationWrapper internal class
    /// <summary>
    /// ConnectorConfigurationWrapper Class
    /// </summary>
    internal class ConnectorConfigurationWrapper
    {
        private IWrapper _wrapper = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectorConfigurationWrapper()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectorConfigurationWrapper(Int64 connector, string assemblydesc, string typedesc, bool trace = false)
        {
            this.ConnectorID = connector;
            this.AssemblyFulldescription = assemblydesc;
            this.AssemblyTypeDescription = typedesc;
            this.TraceResolve = trace;
        }

        /// <summary>
        /// AssemblyConfigurationWrapper implict operator
        /// </summary>
        public static implicit operator ConnectorConfigurationWrapper(AssemblyConfiguration config)
        {
            return new ConnectorConfigurationWrapper(config.ID, config.AssemblyFulldescription, config.AssemblyTypeDescription, config.TraceResolve);
        }

        /// <summary>
        /// AssemblyFulldescription method implementation
        /// </summary>
        public string AssemblyFulldescription { get; set; }

        /// <summary>
        /// AssemblyTypeDescription method implementation
        /// </summary>
        public string AssemblyTypeDescription { get; set; }

        /// <summary>
        /// TraceResolve method implementation
        /// </summary>
        public bool TraceResolve { get; set; }

        /// <summary>
        /// TraceResolve method implementation
        /// </summary>
        public Int64 ConnectorID { get; set; }

        /// <summary>
        /// Wrapper property implementation
        /// </summary>
        public IWrapper Wrapper
        {
            get { return _wrapper; }
            set { _wrapper = value; }
        }
    }   
    #endregion
}