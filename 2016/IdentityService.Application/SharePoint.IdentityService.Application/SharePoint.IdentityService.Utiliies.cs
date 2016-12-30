//******************************************************************************************************************************************************************************************//
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

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.AccessControl;
using Microsoft.SharePoint.Administration.Claims;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Principal;


namespace SharePoint.IdentityService
{
    
    static class Utilities
    {
         const string _eventlogsource = "ActiveDirectory Identity Service";
         static string szassembly = "SharePoint.IdentityService.ClaimsProvider, Version=16.0.0.0, Culture=neutral, PublicKeyToken=";
         static string sztypename = "SharePoint.IdentityService.ClaimsProvider.IdentityServiceClaimsProvider";

        /// <summary>
        /// InstallIdentityServiceSystem method implementation
        /// </summary>
        public static void InstallIdentityServiceSystem(bool shouldprocessapp, bool shouldprocessprx)
        {
            if (!shouldprocessapp)
                return;
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                throw new InvalidOperationException("SharePoint Server Farm not found.");
            }
            SPServer lsrv = SPServer.Local;
            if (null == lsrv)
            {
                throw new InvalidOperationException("SharePoint Local Server registration not found.");
            }
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            if (null == service)
            {
                service = new AdministrationService(farm);
                service.Update();
                service.Provision();
            }
            if (shouldprocessapp)
            {
                foreach (SPServer srv in farm.Servers)
                {
                    if (srv.Role != SPServerRole.Invalid)
                    {
                        IdentityServiceInstance serviceInstance = srv.ServiceInstances.GetValue<IdentityServiceInstance>();
                        if (null != serviceInstance)
                        {
                            if (serviceInstance.Status != SPObjectStatus.Disabled)
                                serviceInstance.Unprovision();
                            serviceInstance.Delete();
                            serviceInstance = null;
                        }
                        if (null == serviceInstance)
                        {
                            serviceInstance = new IdentityServiceInstance(srv, service);
                            serviceInstance.Update(true);
                        }
                    }
                }
            }
            if (shouldprocessprx)
            {
                IdentityServiceProxy serviceProxy = farm.ServiceProxies.GetValue<IdentityServiceProxy>();
                if (null != serviceProxy)
                {
                    if (serviceProxy.Status != SPObjectStatus.Disabled)
                        serviceProxy.Unprovision();
                    serviceProxy.Delete();
                    serviceProxy = null;
                }
                if (null == serviceProxy)
                {
                    serviceProxy = new IdentityServiceProxy(farm);
                    serviceProxy.Update(true);
                }
            }
        }

        /// <summary>
        /// UnInstallIdentityServiceSystem method implementation
        /// </summary>
        public static void UnInstallIdentityServiceSystem(bool shouldprocessapp, bool shouldprocessprx)
        {
            if (!shouldprocessapp)
                return;
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                throw new InvalidOperationException("SharePoint Server Farm not found.");
            }
            SPServer lsrv = SPServer.Local;
            if (null == lsrv)
            {
                throw new InvalidOperationException("SharePoint Local Server registration not found.");
            }
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            if (null == service)
            {
                service = new AdministrationService(farm);
                service.Update();
                service.Provision();
            }
            if (shouldprocessapp)
            {
                foreach (SPServer srv in farm.Servers)
                {
                    if (srv.Role != SPServerRole.Invalid)
                    {
                        IdentityServiceInstance serviceInstance = srv.ServiceInstances.GetValue<IdentityServiceInstance>();
                        if (null != serviceInstance)
                        {
                            try
                            {
                                if (serviceInstance.Status != SPObjectStatus.Disabled)
                                    serviceInstance.Unprovision();
                                serviceInstance.Delete();
                            }
                            catch
                            {
                                // avoid errors !!!
                            }
                            serviceInstance = null;
                        }
                    }
                }
            }
            if (shouldprocessprx)
            {
                IdentityServiceProxy serviceProxy = farm.ServiceProxies.GetValue<IdentityServiceProxy>();
                if (null != serviceProxy)
                {
                    if (serviceProxy.Status != SPObjectStatus.Disabled)
                        serviceProxy.Unprovision();
                    serviceProxy.Delete();
                    serviceProxy = null;
                }
            }
            if ((shouldprocessapp) && (shouldprocessprx))
            {
                SPClaimProviderManager cpm = SPClaimProviderManager.Local;
                try
                {
                    foreach (SPClaimProviderDefinition ppv in cpm.ClaimProviders)
                    {
                        if (ppv.TypeName.ToLower().Equals("SharePoint.IdentityService.ClaimsProvider.IdentityServiceClaimsProvider"))
                            cpm.DeleteClaimProvider(ppv);
                    }
                }
                finally
                {
                    cpm.Update();
                }
            }
        }

        /// <summary>
        /// UpdateIdentityServiceSystem method implementation
        /// </summary>
        public static void UpdateIdentityServiceSystem(bool shouldprocessapp, bool shouldprocessprx)
        {
            if (!shouldprocessapp)
                return;
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                throw new InvalidOperationException("SharePoint Server Farm not found.");
            }
            SPServer lsrv = SPServer.Local;
            if (null == lsrv)
            {
                throw new InvalidOperationException("SharePoint Local Server registration not found.");
            }
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            if (null != service)
            {
                foreach (SPServer srv in farm.Servers)
                {
                    if (srv.Role != SPServerRole.Invalid)
                    {
                        IdentityServiceInstance serviceInstance = srv.ServiceInstances.GetValue<IdentityServiceInstance>();
                        if (null == serviceInstance)
                        {
                            serviceInstance = new IdentityServiceInstance(srv, service);
                            serviceInstance.Update(true);
                        }
                    }
                }
            }
            if (shouldprocessprx)
            {
                IdentityServiceProxy serviceProxy = farm.ServiceProxies.GetValue<IdentityServiceProxy>();
                if (null == serviceProxy)
                {
                    serviceProxy = new IdentityServiceProxy(farm);
                    serviceProxy.Update(true);
                }
            }
        }


        /// <summary>
        /// UpgradeIdentityServiceDatabases method implementation
        /// </summary>
        public static void UpgradeIdentityServiceDatabases()
        {
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                throw new InvalidOperationException("SharePoint Server Farm not found.");
            }
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            if (null == service)
            {
                throw new InvalidOperationException("SharePoint Identity Web Service not found.");
            }
            foreach (SPServiceApplication sp in service.Applications)
            {
                if (sp is IdentityServiceApplication)
                {
                    sp.Upgrade();
                }
            }
        }

        #region Creation of Service Application
        /// <summary>
        /// CreateServiceApplicationAndProxy method implementation
        /// </summary>
        public static IdentityServiceApplication CreateServiceApplicationAndProxy(bool shouldprocess, string name, SPIisWebServiceApplicationPool applicationPool, string dbname, string dbserver, string failover, NetworkCredential cred, bool resolvedb, bool useexitingdb)
        {
            if (string.IsNullOrEmpty(name))
                name = "SharePoint Identity Service Application";
            IdentityServiceApplication app = CreateServiceApplication(shouldprocess, name, applicationPool, dbname, dbserver, failover, cred, resolvedb, useexitingdb);
            if (app != null)
            {
                string prxyname = name + " Proxy";
                CreateServiceProxy(shouldprocess, prxyname, app);
            }
            return app;
        }

        /// <summary>
        /// CreateServiceApplication method implementation
        /// </summary>
        public static IdentityServiceApplication CreateServiceApplication(bool shouldprocess, string name, SPIisWebServiceApplicationPool applicationPool, string dbname, string dbserver, string failover, NetworkCredential cred, bool resolvedb, bool useexistingdb)
        {
            if (!shouldprocess)
                return null; 
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                throw new InvalidOperationException("SharePoint server farm not found.");
            }
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            if (null == service)
            {
                throw new InvalidOperationException("SharePoint Identity Web Service not found.");
            }
            IdentityServiceApplication existingServiceApplication = service.Applications.GetValue<IdentityServiceApplication>();
            if (null != existingServiceApplication)
            {
                throw new InvalidOperationException("SharePoint Identity Service Application exists.");
            }
            if (null == applicationPool)
            {
                throw new InvalidOperationException("The specified application pool could not be found.");
            }
            SPDatabaseParameterOptions prm = SPDatabaseParameterOptions.GenerateUniqueName;
            if (resolvedb)
               prm = SPDatabaseParameterOptions.ResolveNameConflict;
            else
               prm = SPDatabaseParameterOptions.None;
            SPDatabaseParameters databaseParameters = SPDatabaseParameters.CreateParameters(dbname, dbserver, cred, failover, prm);
            if (useexistingdb)
                databaseParameters.Validate(SPDatabaseValidation.AttachExisting);
            else
                databaseParameters.Validate(SPDatabaseValidation.CreateNew);
            IdentityServiceApplication serviceApplication = IdentityServiceApplication.Create(name, service, applicationPool, databaseParameters);
            serviceApplication.Provision();

            var mgr = SPClaimProviderManager.Local;
            var identity = WindowsIdentity.GetCurrent();
            if (identity != null)
            {
                var claim = mgr.ConvertIdentifierToClaim(identity.Name, SPIdentifierTypes.WindowsSamAccountName);
                SPCentralAdministrationSecurity security = serviceApplication.GetAdministrationAccessControl();
                SPIisWebServiceApplicationSecurity websec = serviceApplication.GetAccessControl();
                security.AddAccessRule(new SPAclAccessRule<SPCentralAdministrationRights>(claim, SPCentralAdministrationRights.FullControl));
                websec.AddAccessRule(new SPAclAccessRule<SPIisWebServiceApplicationRights>(claim, SPIisWebServiceApplicationRights.FullControl));
                serviceApplication.SetAdministrationAccessControl(security);
                serviceApplication.SetAccessControl(websec);
            }
            serviceApplication.Update(true);
            return serviceApplication;
        }

        /// <summary>
        /// CreateServiceProxy method implementation
        /// </summary>
        public static ServiceApplicationProxy CreateServiceProxy(bool shouldprocess, string name, IdentityServiceApplication serviceApplication)
        {
            if (!shouldprocess)
                return null;
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                throw new InvalidOperationException("SharePoint server farm not found.");
            }
            IdentityServiceProxy serviceProxy = farm.ServiceProxies.GetValue<IdentityServiceProxy>();
            if (null == serviceProxy)
            {
                throw new InvalidOperationException("SharePoint Identity Service Proxy not found.");
            }
            Uri serviceApplicationUri = null;
            ISharedServiceApplication sharedServiceApplication = serviceApplication as ISharedServiceApplication;
            if (null == sharedServiceApplication)
            {
                throw new InvalidOperationException("Connecting to the specified service application is not supported.");
            }

            serviceApplicationUri = sharedServiceApplication.Uri;
            ServiceApplicationProxy serviceApplicationProxy = null;
            if (null != serviceApplicationUri)
            {
                serviceApplicationProxy = new ServiceApplicationProxy(name, serviceProxy, serviceApplicationUri);
                serviceApplicationProxy.Provision();
                SPServiceApplicationProxyGroup grp = serviceApplication.ServiceApplicationProxyGroup;
                grp.Add(serviceApplicationProxy);
                grp.Update();
                serviceApplicationProxy.Update();
            }
            else
                throw new InvalidOperationException("SharePoint Identity Service Application do not exists.");
            serviceApplication.Update(true);
            return serviceApplicationProxy;
        }
        #endregion

        #region Update of Service Application
        /// <summary>
        /// UpdateServiceApplicationAndProxy method implementation
        /// </summary>
        public static IdentityServiceApplication UpdateServiceApplicationAndProxy(bool shouldprocess, IdentityServiceApplication svcapp, string name, SPIisWebServiceApplicationPool applicationPool, string dbname, string dbserver, string failover, NetworkCredential cred, bool resolvedb, bool useexitingdb)
        {
            if (string.IsNullOrEmpty(name))
                name = "SharePoint Identity Service Application";
            svcapp = GetApplicationById(svcapp.Id);
            if (null == svcapp)
                throw new NullReferenceException("Service application does not exist");
            IdentityServiceApplication app = UpdateServiceApplication(shouldprocess, svcapp, name, applicationPool, dbname, dbserver, failover, cred, resolvedb, useexitingdb);
            if (app != null)
            {
                string prxyname = name + " Proxy";
                UpdateServiceProxy(shouldprocess, prxyname, app);
                app.UpgradeJobs();
            }
            return app;
        }

        /// <summary>
        /// CreateServiceApplication method implementation
        /// </summary>
        public static IdentityServiceApplication UpdateServiceApplication(bool shouldprocess, IdentityServiceApplication svcapp, string name, SPIisWebServiceApplicationPool applicationPool, string dbname, string dbserver, string failover, NetworkCredential cred, bool resolvedb, bool useexistingdb)
        {
            if (!shouldprocess)
                return null;
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                throw new InvalidOperationException("SharePoint server farm not found.");
            }
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            if (null == service)
            {
                throw new InvalidOperationException("SharePoint Identity Web Service not found.");
            }
            if (null == svcapp)
            {
                throw new InvalidOperationException("SharePoint Identity Service Application does not exists.");
            }
            if (null == applicationPool)
            {
                throw new InvalidOperationException("The specified application pool could not be found.");
            }

            bool needsupdate = false;
            IdentityServiceApplication serviceApplication = GetApplicationById(svcapp.Id);
            if (serviceApplication.Status != SPObjectStatus.Online)
                serviceApplication.Provision();

            if (!serviceApplication.ApplicationPool.Equals(applicationPool))
            {
                serviceApplication.ApplicationPool = applicationPool;
                needsupdate = true;
            }

            SPDatabaseParameterOptions prm = SPDatabaseParameterOptions.None;
            SPDatabaseParameters databaseParameters = SPDatabaseParameters.CreateParameters(dbname, dbserver, cred, failover, prm);

            ActiveDirectoryIdentityServiceDatabase existingdb = serviceApplication.Database;
            try
            {
                // DatabaseName changed
                if (CheckForNewDatabase(databaseParameters, existingdb))
                {
                    if (useexistingdb)
                    {
                        databaseParameters.Validate(SPDatabaseValidation.AttachExisting);
                        serviceApplication.Database = new ActiveDirectoryIdentityServiceDatabase(databaseParameters);
                        serviceApplication.Database.Update(true);
                       // serviceApplication.Database.Provision();
                    }
                    else
                    {
                        databaseParameters.Validate(SPDatabaseValidation.CreateNew);
                        serviceApplication.Database = new ActiveDirectoryIdentityServiceDatabase(databaseParameters);
                        serviceApplication.Database.Provision();
                    }
                    needsupdate = true;
                }
                else if (CheckForNewCredentials(databaseParameters, existingdb))
                {
                    existingdb.Username = databaseParameters.Username;
                    existingdb.Password = databaseParameters.Password;
                    existingdb.Update(true);
                    needsupdate = true;
                }
            }
            finally
            {
                if (needsupdate)
                   serviceApplication.Update(true);
            }
            return serviceApplication;
        }

        /// <summary>
        /// CreateServiceProxy method implementation
        /// </summary>
        public static ServiceApplicationProxy UpdateServiceProxy(bool shouldprocess, string name, IdentityServiceApplication serviceApplication)
        {
            if (!shouldprocess)
                return null;
            bool mustcreateproxy = false;
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                throw new InvalidOperationException("SharePoint server farm not found.");
            }
            IdentityServiceProxy serviceProxy = farm.ServiceProxies.GetValue<IdentityServiceProxy>();
            if (null == serviceProxy)
            {
                throw new InvalidOperationException("SharePoint Identity Service Proxy not found.");
            }
            ServiceApplicationProxy serviceApplicationProxy = GetApplicationProxy(serviceApplication, serviceProxy);
            if (null == serviceApplicationProxy)
            {
                mustcreateproxy = true;
            }
            Uri serviceApplicationUri = null;
            ISharedServiceApplication sharedServiceApplication = serviceApplication as ISharedServiceApplication;
            if (null == sharedServiceApplication)
            {
                throw new InvalidOperationException("Connecting to the specified service application is not supported.");
            }

            serviceApplicationUri = sharedServiceApplication.Uri;
            if (!mustcreateproxy)
            {
                if (null != serviceApplicationUri)
                {
                    if (serviceApplicationProxy.Status != SPObjectStatus.Online)
                    {
                        serviceApplicationProxy.Provision();
                        SPServiceApplicationProxyGroup grp = serviceApplication.ServiceApplicationProxyGroup;
                        grp.Add(serviceApplicationProxy);
                        grp.Update();
                        serviceApplication.Update(true);
                    }
                }
            }
            else
            {
                serviceApplicationProxy = CreateServiceProxy(shouldprocess, name, serviceApplication);
            }
            return serviceApplicationProxy;
        }
        #endregion

        #region Services & Applications utilities methods
        /// <summary>
        /// GetAdminService method
        /// </summary>
        public static AdministrationService GetAdminService(bool throwOnNull)
        {
            SPFarm farm = SPFarm.Local;
            if (throwOnNull && farm == null)
                throw new InvalidOperationException("Farm is not properly provisioned.");
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            return service;
        }

        /// <summary>
        /// CheckForNewDatabase method implementation
        /// </summary>
        private static bool CheckForNewDatabase(SPDatabaseParameters databaseParameters, ActiveDirectoryIdentityServiceDatabase existingdb)
        {
            if (existingdb == null)
                return true;
            string FailoverServerName = "";
            if (existingdb.FailoverServer != null)
                FailoverServerName = existingdb.FailoverServer.Name;
            return ((databaseParameters.Database != existingdb.Name) || (databaseParameters.Server != existingdb.NormalizedDataSource) || (databaseParameters.FailoverPartner != FailoverServerName));
        }

        /// <summary>
        /// CheckForNewCredentials method implementation
        /// </summary>
        private static bool CheckForNewCredentials(SPDatabaseParameters databaseParameters, ActiveDirectoryIdentityServiceDatabase existingdb)
        {
            if (existingdb == null)
                return false;
            if (string.IsNullOrEmpty(databaseParameters.Username) && string.IsNullOrEmpty(databaseParameters.Password) && string.IsNullOrEmpty(existingdb.Username) && string.IsNullOrEmpty(existingdb.Password))
                return false;
            return ((databaseParameters.Username != existingdb.Username) || (databaseParameters.Password != existingdb.Password));
        }

        /// <summary>
        /// GetApplicationById 
        /// </summary>
        public static IdentityServiceApplication GetApplicationById(Guid applicationId)
        {
            AdministrationService service = GetAdminService(false);
            ArgumentValidator.IsNotNull(service, "service");
            ArgumentValidator.IsNotEmpty(applicationId, "applicationId");
            return service.Applications.GetValue<IdentityServiceApplication>(applicationId);
        }

        /// <summary>
        /// GetApplicationByName
        /// </summary>
        public static IdentityServiceApplication GetApplicationByName(string applicationname)
        {
            AdministrationService service = GetAdminService(false);
            ArgumentValidator.IsNotNull(service, "service");
            ArgumentValidator.IsNotEmpty(applicationname, "applicationName");
            foreach (SPServiceApplication sp in service.Applications)
            {
                if (sp is IdentityServiceApplication)
                {
                    if (sp.Name.ToLowerInvariant().Trim().Equals(applicationname.ToLowerInvariant().Trim()))
                        return sp as IdentityServiceApplication;
                }
            }
            return null;
        }

        /// <summary>
        /// GetApplicationProxy method implementation
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static ServiceApplicationProxy GetApplicationProxy(IdentityServiceApplication app, IdentityServiceProxy serviceProxy)
        {
            ArgumentValidator.IsNotNull(app, "ServiceApplication");
            ArgumentValidator.IsNotNull(app, "ServiceApplicationProxy");
            if (null != serviceProxy)
            {
                foreach(SPServiceApplicationProxy prxy in serviceProxy.ApplicationProxies)
                {
                    if (prxy is ServiceApplicationProxy)
                    {
                        if (CheckApplicationProxy(app, prxy as ServiceApplicationProxy))
                        {
                            return prxy as ServiceApplicationProxy;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// CheckApplicationProxy metho implementation
        /// </summary>
        private static bool CheckApplicationProxy(IdentityServiceApplication app, ServiceApplicationProxy prxy)
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

        /// <summary>
        /// GetPublicKeyTokenFromAssembly()
        /// </summary>
        private static string GetPublicKeyTokenFromAssembly()
        {
            var bytes = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken();
            if (bytes == null || bytes.Length == 0)
                return "None";
            var publicKeyToken = string.Empty;
            for (int i = 0; i < bytes.GetLength(0); i++)
                publicKeyToken += string.Format("{0:x2}", bytes[i]);
            return publicKeyToken;
        }
        #endregion

        #region Claims Providers

        /// <summary>
        /// GetClaimProviderInternalName method implementation
        /// </summary>
        public static string GetClaimProviderInternalName(string value)
        {
            if (value.StartsWith("0x2477"))
                return value;
            else
                return "0x2477"+value;
        }

        /// <summary>
        /// GetClaimProviderInternalName method implementation
        /// </summary>
        public static string GetClaimProviderName(string value)
        {
            if (value.StartsWith("0x2477"))
                return value.Replace("0x2477","");
            else
                return value;
        }

        /// <summary>
        /// CreateUpdateDeleteClaimProvider method implementation
        /// </summary>
        public static void CreateUpdateDeleteClaimProvider(string applicationname, string trustedtokenissuer, string displayname, string desc, bool isusedbydefault, bool canupdate)
        {
            bool iswindows = trustedtokenissuer.ToLowerInvariant().Trim().Equals("ad");
            if (iswindows)
                CreateUpdateWindowsClaimProvider(applicationname, displayname, desc, isusedbydefault, canupdate);
            else
                CreateUpdateTrustedClaimProvider(applicationname, trustedtokenissuer, displayname, desc, isusedbydefault, canupdate);
        }

        /// <summary>
        /// CreateUpdateWindowsClaimProvider
        /// </summary>
        private static void CreateUpdateWindowsClaimProvider(string applicationname, string displayname, string desc, bool isusedbydefault, bool canupdate)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                if ((canupdate) && DoesClaimProviderExist("AD"))
                {
                    try
                    {
                        SPClaimProviderDefinition cp = UpdateClaimProvider("AD", desc, isusedbydefault);
                    }
                    catch (Exception Ex)
                    {
                        LogEvent.Log(Ex, "SharePoint Identity Service Error Application when updating SPClaimProvider : AD", EventLogEntryType.Error, 9998);
                    }
                }
                else 
                {  
                    try
                    {

                        if (DoesClaimProviderExist("AD"))
                            DeleteClaimProvider("AD");
                        SPClaimProviderDefinition cp = AddClaimProvider("AD", desc, szassembly + GetPublicKeyTokenFromAssembly(), sztypename, isusedbydefault);
                    }
                    catch (Exception Ex)
                    {
                        LogEvent.Log(Ex, "SharePoint Identity Service Error Application when creating SPClaimProvider : AD", EventLogEntryType.Error, 9999);
                    }
                }
                IdentityServiceApplication app = GetApplicationByName(applicationname);
                if (app != null)
                {
                    app.SetGeneralParameter("ClaimDisplayName", displayname);
                    app.SetGeneralParameter("ClaimProviderName", "AD");
                    app.SetGeneralParameter("TrustedLoginProviderName", "AD");
                    app.SetGeneralParameter("ClaimsMode", "Windows");
                    app.SetGeneralParameter("ClaimIdentityMode", "SAMAccount");
                }
            });
        }

        /// <summary>
        /// CreateUpdateTrustedClaimProvider
        /// </summary>
        private static void CreateUpdateTrustedClaimProvider(string applicationname, string trustedtokenissuer, string displayname, string desc, bool isusedbydefault,bool canupdate)
        {
            string cpname = GetClaimProviderInternalName(trustedtokenissuer);
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                if ((canupdate) || DoesClaimProviderExist(cpname))
                {
                    try
                    {
                        SPClaimProviderDefinition cp = UpdateClaimProvider(cpname, desc, isusedbydefault);
                    }
                    catch (Exception Ex)
                    {
                        LogEvent.Log(Ex, "SharePoint Identity Service Error Application when updating SPClaimProvider : " + cpname, EventLogEntryType.Error, 9998);
                    }
                }
                else
                {
                    try
                    {
                        if (DoesClaimProviderExist(cpname))
                           DeleteClaimProvider(cpname);
                        SPClaimProviderDefinition cp = AddClaimProvider(cpname, desc, szassembly + GetPublicKeyTokenFromAssembly(), sztypename, isusedbydefault);
                    }
                    catch (Exception Ex)
                    {
                        LogEvent.Log(Ex, "SharePoint Identity Service Error Application when creating SPClaimProvider : " + cpname, EventLogEntryType.Error, 9999);
                    }
                }
                IdentityServiceApplication app = GetApplicationByName(applicationname);
                if (app != null)
                {
                    app.SetGeneralParameter("ClaimDisplayName", displayname);
                    app.SetGeneralParameter("ClaimProviderName", cpname);
                    app.SetGeneralParameter("TrustedLoginProviderName", trustedtokenissuer);
                    app.SetGeneralParameter("ClaimsMode", "Federated");
                    SetTrustedTokenIssuer(cpname, trustedtokenissuer);
                }
            });
        }

        /// <summary>
        /// SetTrustedTokenIssuer method implementation
        /// </summary>
        private static bool SetTrustedTokenIssuer(string claimprovidername, string trustedtokenissuer)
        {
            SPSecurityTokenServiceManager ctm = SPSecurityTokenServiceManager.Local;
            foreach (SPTrustedLoginProvider current in ctm.TrustedLoginProviders)
            {
                if (string.IsNullOrEmpty(current.Name))
                    continue;
                if (current.Name.ToLowerInvariant().Trim().Equals(trustedtokenissuer.ToLowerInvariant().Trim()))
                {
                    current.ClaimProviderName = claimprovidername;
                    // current.Update(true);
                    current.Update();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// GetTrustedTokenIssuer method implementation
        /// </summary>
        private static bool ValidateTrustedTokenIssuer(string claimprovidername, string trustedtokenissuer)
        {
            SPSecurityTokenServiceManager ctm = SPSecurityTokenServiceManager.Local;
            foreach (SPTrustedLoginProvider current in ctm.TrustedLoginProviders)
            {
                if (string.IsNullOrEmpty(current.ClaimProviderName))
                    continue;
                if (current.ClaimProviderName.ToLowerInvariant().Trim().Equals(claimprovidername.ToLowerInvariant().Trim()))
                {
                    return (current.Name.ToLowerInvariant().Trim().Equals(trustedtokenissuer.ToLowerInvariant().Trim()));
                }
            }
            return true;
        }

        /// <summary>
        /// GetClaimProviderCandidates method implementation
        /// </summary>
        public static List<ClaimProviderDefinition> GetClaimProviderCandidates(bool onlyunassigned)
        {
            List<ClaimProviderDefinition> lst = new List<ClaimProviderDefinition>();
            ClaimProviderDefinition itm = new ClaimProviderDefinition();
            itm.TrustedTokenIssuer = "AD";
            itm.ClaimProviderName = "AD";
            itm.DisplayName = string.Format("{0} (windows claims)", "Active Directory");
            itm.Description = "Active Directory";
            itm.IsEnabled = true;
            itm.IsUsedByDefault = false;
            itm.IsVisible = true;
            lst.Add(itm);

            SPSecurityTokenServiceManager ctm = SPSecurityTokenServiceManager.Local;
            foreach (SPTrustedLoginProvider current in ctm.TrustedLoginProviders)
            {
             /*   SPClaimProviderManager mgr = SPClaimProviderManager.Local;
                foreach(SPTrustedClaimProvider tprov in mgr.TrustedClaimProviders)
                {
                    tprov.
                }

                foreach(SPClaimProviderDefinition prov in mgr.ClaimProviders)
                {
                }
                */
                ClaimProviderDefinition trustitm = new ClaimProviderDefinition();
                trustitm.TrustedTokenIssuer = current.Name;
                trustitm.ClaimProviderName = current.ClaimProviderName;
                trustitm.DisplayName = string.Format("{0} (federated)", current.DisplayName);
                trustitm.Description = current.Description;
                trustitm.IsEnabled = true;
                trustitm.IsUsedByDefault = false;
                trustitm.IsVisible = true;
                lst.Add(trustitm);
            }
            if (onlyunassigned)
            {
                SPClaimProviderManager mgr = SPClaimProviderManager.Local;
                for (int i = lst.Count - 1; i >= 0; i--)
                {
                    if (DoesClaimProviderExist(lst[i].ClaimProviderName))
                    {
                        SPClaimProviderDefinition def = GetClaimProvider(lst[i].ClaimProviderName);
                        if (def.AssemblyName.ToLower().Equals((szassembly + GetPublicKeyTokenFromAssembly()).ToLower()) && def.TypeName.ToLower().Equals(def.TypeName.ToLower()))
                        {
                            lst.RemoveAt(i);
                        }
                    }
                }
            }
            return lst;
        }

        /// <summary>
        /// ExistsClaimProvider method implementation
        /// </summary>
        internal static bool DoesClaimProviderExist(string name)
        {
            try
            {
                SPClaimProviderManager cpm = SPClaimProviderManager.Local;
                string ppv;
                if (name.ToLowerInvariant().Trim().Equals("active directory"))
                     ppv = "AD";
                else  if (name.ToLowerInvariant().Trim().Equals("windows"))
                    ppv = "AD";
                else
                    ppv = name;
                return cpm.DoesClaimProviderExist(ppv);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// ExistsClaimProvider method implementation
        /// </summary>
        internal static bool DoesClaimProviderIsValid(string name, string issuer)
        {
            SPClaimProviderManager cpm = SPClaimProviderManager.Local;
            string ppv;
            if (name.ToLowerInvariant().Trim().Equals("active directory"))
                ppv = "AD";
            else if (name.ToLowerInvariant().Trim().Equals("windows"))
                ppv = "AD";
            else
                ppv = name;
            return ValidateTrustedTokenIssuer(ppv, issuer);
        }

        /// <summary>
        /// GetClaimProvider method implentation
        /// </summary>
        internal static SPClaimProviderDefinition GetClaimProvider(string name)
        {
            SPClaimProviderManager cpm = SPClaimProviderManager.Local;
            string ppv;
            if (name.ToLowerInvariant().Trim().Equals("active directory"))
                ppv = "AD";
            else if (name.ToLowerInvariant().Trim().Equals("windows"))
                ppv = "AD";
            else
                ppv = name;
            return cpm.GetClaimProvider(ppv);
        }

        /// <summary>
        /// DeleteClaimProvider method implementation
        /// </summary>
        internal static void DeleteClaimProvider(string name)
        {
            try
            {
                SPClaimProviderManager cpm = SPClaimProviderManager.Local;
                string ppv;
                if (name.ToLowerInvariant().Trim().Equals("active directory"))
                    ppv = "AD";
                else if (name.ToLowerInvariant().Trim().Equals("windows"))
                    ppv = "AD";
                else
                    ppv = name;
                cpm.DeleteClaimProvider(ppv);
                cpm.Update();
            }
            catch (Exception)
            {
                // Nothing
            }
        }

        /// <summary>
        /// UpdateClaimProvider method implementation
        /// </summary>
        internal static SPClaimProviderDefinition UpdateClaimProvider(string name, string desc, bool isusedbydefault)
        {
            bool needsupdate = false;
            SPClaimProviderManager cpm = SPClaimProviderManager.Local;
            string ppv;
            if (name.ToLowerInvariant().Trim().Equals("active directory"))
                ppv = "AD";
            else if (name.ToLowerInvariant().Trim().Equals("windows"))
                ppv = "AD";
            else
                ppv = name;
            SPClaimProviderDefinition cpd = cpm.GetClaimProvider(ppv);
            if (desc != cpd.Description)
            {
                cpd.Description = desc;
                needsupdate = true;
            }
            if (isusedbydefault != cpd.IsUsedByDefault)
            {
                cpd.IsUsedByDefault = isusedbydefault;
                needsupdate = true;
            }
            if (needsupdate)
                cpm.Update(true);
            return cpd;
        }

        /// <summary>
        /// AddClaimProvider method implementation
        /// </summary>
        internal static SPClaimProviderDefinition AddClaimProvider(string name, string desc, string assembly, string typename, bool isusedbydefault)
        {
            SPClaimProviderManager cpm = SPClaimProviderManager.Local;
            string ppv;
            if (name.ToLowerInvariant().Trim().Equals("active directory"))
                ppv = "AD";
            else if (name.ToLowerInvariant().Trim().Equals("windows"))
                ppv = "AD";
            else
                ppv = name;
            SPClaimProviderDefinition cpd = new SPClaimProviderDefinition(ppv, desc, assembly, typename);
            cpd.IsEnabled = true;
            cpd.IsVisible = true;
            cpd.IsUsedByDefault = isusedbydefault;
            cpm.AddClaimProvider(cpd);
            cpm.Update(true);
            return cpd;
        }
        #endregion
    }

    #region Logs & trace
    /// <summary>
    /// LogEvent Class
    /// </summary>
    public static class LogEvent
    {
        const string _eventlogsource = "ActiveDirectory Identity Service";

        /// <summary>
        /// Constructor
        /// </summary>
        static LogEvent()
        {
            try
            {
                // using (Identity impersonate = Identity.ImpersonateAdmin()) 
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    if (!EventLog.SourceExists(_eventlogsource))
                        System.Diagnostics.EventLog.CreateEventSource(_eventlogsource, "Application");
                }
                );
            }
            catch
            {
            }
        }

        /// <summary>
        /// Log method implementation
        /// </summary>
        public static void Log(Exception ex, string message, EventLogEntryType eventLogEntryType, int eventid = 0)
        {
            try
            {
                // using (Identity impersonate = Identity.ImpersonateAdmin()) 
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    EventLog evtL = new EventLog("Application");
                    evtL.Source = _eventlogsource;

                    string contents = String.Format("{0}\r\n{1}\r\n{2}", message, ex.Message, ex.StackTrace);
                    while ((ex = ex.InnerException) != null)
                    {
                        contents = String.Format("{3}\r\n\r\n{0}\r\n{1}\r\n{2}", message, ex.Message, ex.StackTrace, contents);
                    }
                    evtL.WriteEntry(contents, eventLogEntryType, eventid);
                }
                );
            }
            catch
            {
            }
        }

        /// <summary>
        /// Trace method implementation
        /// </summary>
        public static void Trace(string message, EventLogEntryType eventLogEntryType, int eventid = 0)
        {
            try
            {
                //using (Identity impersonate = Identity.ImpersonateAdmin()) 
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    EventLog evtL = new EventLog("Application");
                    evtL.Source = _eventlogsource;
                    string contents = String.Format("{0}", message);
                    evtL.WriteEntry(contents, eventLogEntryType, eventid);
                }
                );
            }
            catch
            {
            }
        }
    }
    #endregion

    #region ClaimProviderDefinition
    public class ClaimProviderDefinition
    {
        public string TrustedTokenIssuer { get; set; }
        public string ClaimProviderName { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsUsedByDefault { get; set; }
        public bool IsVisible { get; set; }
    }
    #endregion

    #region EnumExtensions class
    public static class EnumExtensions
    {
        /// <summary>
        /// Check to see if a flags enumeration has a specific flag set.
        /// </summary>
        /// <param name="variable">Flags enumeration to check</param>
        /// <param name="value">Flag to check for</param>
        /// <returns></returns>
        public static bool Has(this Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException("value");

            // Not as good as the .NET 4 version of this function, but should be good enough
            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(), variable.GetType()));
            }

            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);
        }
    }
    #endregion

    #region ArgumentValidator

    public static class ArgumentValidator
    {
        public static void IsNotNull(object argument, string paramName)
        {
            if (argument == null)
                throw new ArgumentNullException(string.Format("Object \"{0}\" cannot be null", paramName));
        }

        public static void IsNotNull(Type argument, string paramName)
        {
            if (argument == null)
                throw new ArgumentNullException(string.Format("Given type \"{0}\" cannot be null", paramName));
        }

        public static void IsNotEmpty(string argument, string paramName)
        {
            if (string.IsNullOrEmpty(argument))
                throw new ArgumentException(string.Format("String \"{0}\" cannot be null or empty", paramName));
        }

        public static void IsNotEmpty(Guid argument, string paramName)
        {
            if (argument == Guid.Empty)
                throw new ArgumentNullException(string.Format("Guid \"{0}\" cannot be empty", paramName));
        }

        public static void IsNotEqual(Type argument, Type matchingType)
        {
            if (argument != matchingType)
                throw new ArgumentException(string.Format("Type \"{0}\" is not of type \"{1}\"", argument, matchingType));
        }
    }
    #endregion
}


