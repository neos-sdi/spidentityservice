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

namespace SharePoint.IdentityService.PowerShell
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Management.Automation;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.PowerShell;
    using SharePoint.IdentityService;
    using System.Security;
    using System.Net;
    using Microsoft.SharePoint.Administration.AccessControl;

    [Cmdlet(VerbsLifecycle.Install, "IdentityService", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class InstallIdentityService : SPCmdlet
    {
        /// <summary>
        /// RequireUserFarmAdmin method override
        /// </summary>
        protected override bool RequireUserFarmAdmin()
        {
            return true;
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void InternalProcessRecord()
        {
            Utilities.InstallIdentityServiceSystem(ShouldProcess("SharePoint Identity Service Application"), ShouldProcess("SharePoint Identity Service Application Proxy"));
        }
    }

    [Cmdlet(VerbsLifecycle.Uninstall, "IdentityService", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class UnInstallIdentityService : SPCmdlet
    {
        /// <summary>
        /// RequireUserFarmAdmin method override
        /// </summary>
        protected override bool RequireUserFarmAdmin()
        {
            return true;
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void InternalProcessRecord()
        {
            Utilities.UnInstallIdentityServiceSystem(ShouldProcess("SharePoint Identity Service Application"), ShouldProcess("SharePoint Identity Service Application Proxy"));
        }
    }

    [Cmdlet("Repair", "IdentityService", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class RepairIdentityService : SPCmdlet
    {
        /// <summary>
        /// RequireUserFarmAdmin method override
        /// </summary>
        protected override bool RequireUserFarmAdmin()
        {
            return true;
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void InternalProcessRecord()
        {
            Utilities.UpdateIdentityServiceSystem(ShouldProcess("SharePoint Identity Service Application"), ShouldProcess("SharePoint Identity Service Application Proxy"));
        }
    }

    [Cmdlet("Upgrade", "IdentityServiceDatabases", SupportsShouldProcess = true)]
    [SPCmdlet(RequireUserFarmAdmin = true)]
    internal sealed class UpgradeIdentityServiceDatabases : SPCmdlet
    {
        /// <summary>
        /// RequireUserFarmAdmin method override
        /// </summary>
        protected override bool RequireUserFarmAdmin()
        {
            return true;
        }

        /// <summary>
        /// ProcessInternalRecord method override
        /// </summary>
        protected override void InternalProcessRecord()
        {
            Utilities.UpgradeIdentityServiceDatabases();
        }
    }

    [Cmdlet(VerbsCommon.New, "IdentityServiceApplication", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class NewIdentityServiceApplication : SPCmdlet
    {
        private string m_Name;
        private string m_DatabaseName = "IdentityServiceDatabase";
        private string m_DatabaseServer;
        private string m_FailoverDatabaseServer;
        private SwitchParameter m_resolvedb;
        private SwitchParameter m_useexistingdb;
        private SPIisWebServiceApplicationPoolPipeBind m_ApplicationPool;
        private PSCredential m_DatabaseCredentials;

        /// <summary>
        /// RequireUserFarmAdmin method implementation
        /// </summary>
        protected override bool RequireUserFarmAdmin()
        {
            return true;
        }

        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [Parameter(Mandatory = true)]
        [ValidateNotNull]
        public SPIisWebServiceApplicationPoolPipeBind ApplicationPool
        {
            get { return m_ApplicationPool; }
            set { m_ApplicationPool = value; }
        }

        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string DatabaseName
        {
            get { return m_DatabaseName; }
            set { m_DatabaseName = value; }
        }

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string DatabaseServer
        {
            get { return m_DatabaseServer; }
            set { m_DatabaseServer = value; }
        }

        [Parameter(Mandatory = false)]
        [ValidateNotNullOrEmpty]
        public string FailoverDatabaseServer
        {
            get { return m_FailoverDatabaseServer; }
            set { m_FailoverDatabaseServer = value; }
        }

        [Parameter(Mandatory = false)]
        [ValidateNotNull]
        public PSCredential DatabaseCredentials
        {
            get { return m_DatabaseCredentials; }
            set { m_DatabaseCredentials = value; }
        }

        [Parameter(Mandatory = false)]
        public SwitchParameter ResolveConflicts
        {
            get { return m_resolvedb; }
            set { m_resolvedb = value; }
        }

        [Parameter(Mandatory = false)]
        public SwitchParameter UseExistingDatabase
        {
            get { return m_useexistingdb; }
            set { m_useexistingdb = value; }
        }

        /// <summary>
        /// InternalProcessRecord method override
        /// </summary>
        protected override void InternalProcessRecord()
        {
            SPIisWebServiceApplicationPool applicationPool = this.ApplicationPool.Read();
            if (null == applicationPool)
            {
                WriteError(new InvalidOperationException("The specified application pool could not be found."), ErrorCategory.InvalidArgument, this);
                SkipProcessCurrentRecord();
            }
            this.WriteObject(Utilities.CreateServiceApplicationAndProxy(ShouldProcess(this.Name), this.Name, applicationPool, this.DatabaseName, this.DatabaseServer, this.FailoverDatabaseServer, (null == this.DatabaseCredentials ? null : this.DatabaseCredentials.GetNetworkCredential()),  (this.m_resolvedb.IsPresent), this.m_useexistingdb.IsPresent));
        }
    }

    [Cmdlet(VerbsCommon.Reset, "IdentityServiceApplication", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class ResetIdentityServiceApplication : SPCmdlet
    {
        /// <summary>
        /// InternalProcessRecord method override
        /// </summary>
        protected override void InternalProcessRecord()
        {
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint server farm not found."), ErrorCategory.ResourceUnavailable, this);
            }
            else 
            {
                foreach (SPService sps in farm.Services)
                {
                    foreach (SPServiceInstance dep in sps.Instances)
                    {
                        if (dep is IdentityServiceInstance)
                        {
                            if (dep.Status == SPObjectStatus.Online)
                            {
                                try
                                {
                                    Host.UI.WriteLine("-----------------------------------------------------------------------");
                                    Host.UI.WriteLine(ConsoleColor.Yellow, ConsoleColor.Black, "IdentityServiceInstance on Server " + dep.Server.Name + " -> Stopping...");
                                    dep.Unprovision();
                                    Host.UI.WriteLine(ConsoleColor.Red, ConsoleColor.Black, "IdentityServiceInstance on Server " + dep.Server.Name + " -> Stopped !");
                                    Host.UI.WriteLine();
                                    Host.UI.WriteLine(ConsoleColor.Yellow, ConsoleColor.Black, "IdentityServiceInstance on Server " + dep.Server.Name + " -> Starting...");
                                    dep.Provision();
                                    Host.UI.WriteLine(ConsoleColor.Green, ConsoleColor.Black, "IdentityServiceInstance on Server " + dep.Server.Name + " -> Started !");
                                    Host.UI.WriteLine();
                                    Host.UI.WriteLine();
                                }
                                catch (Exception e)
                                {
                                    Host.UI.WriteLine();
                                    Host.UI.WriteErrorLine(e.Message);
                                    Host.UI.WriteLine();
                                    Host.UI.WriteLine();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [Cmdlet("Reload", "IdentityServiceApplication", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class ReloadIdentityServiceApplication : SPCmdlet
    {
        private string m_Name;

        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        /// <summary>
        /// InternalProcessRecord method override
        /// </summary>
        protected override void InternalProcessRecord()
        {
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint server farm not found."), ErrorCategory.ResourceUnavailable, this);
            }
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            if (null == service)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint Identity Service not found."), ErrorCategory.ResourceUnavailable, this);
            }
            IdentityServiceApplication existingServiceApplication = service.Applications.GetValue<IdentityServiceApplication>(this.Name);
            if (null == existingServiceApplication)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint Identity Service Application not found."), ErrorCategory.ResourceUnavailable, this);
            }
            this.WriteObject(existingServiceApplication.Reload());
        }
    }

    [Cmdlet(VerbsCommon.Set, "IdentityServiceApplicationData", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class SetIdentityServiceApplicationData : SPCmdlet
    {
        private const string AssemblyParameterSetName = "AssemblyConfiguration";
        private const string NewAssemblyParameterSetName = "NewAssemblyConfiguration";
        private const string ConfigurationParameterSetName = "ConnectionConfiguration";
        private const string NewConfigurationParameterSetName = "NewConnectionConfiguration";
        private const string DomainParameterSetName = "DomainConfiguration";
        private const string NewDomainParameterSetName = "NewDomainConfiguration";

        private string m_Name;
        private ConnectionConfiguration m_connection;
        private DomainConfiguration m_domain;
        private AssemblyConfiguration m_fulladdesc;
        private AssemblyConfiguration m_newfulladdesc;
        private DomainConfiguration m_newdomain;
        private ConnectionConfiguration m_newconnection;

        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [Parameter(ParameterSetName = AssemblyParameterSetName, Mandatory = false)]
        public AssemblyConfiguration AssemblyConfiguration
        {
            get { return m_fulladdesc; }
            set { m_fulladdesc = value; }
        }

        [Parameter(ParameterSetName = NewAssemblyParameterSetName, Mandatory = false)]
        public AssemblyConfiguration NewAssemblyConfiguration
        {
            get { return m_newfulladdesc; }
            set { m_newfulladdesc = value; }
        }

        [Parameter(ParameterSetName = ConfigurationParameterSetName, Mandatory = false)]
        public ConnectionConfiguration ConnectionConfiguration
        {
            get { return m_connection; }
            set { m_connection = value; }
        }

        [Parameter(ParameterSetName = NewConfigurationParameterSetName, Mandatory = false)]
        public ConnectionConfiguration NewConnectionConfiguration
        {
            get { return m_newconnection; }
            set { m_newconnection = value; }
        }

        [Parameter(ParameterSetName = DomainParameterSetName, Mandatory = false)]
        public DomainConfiguration DomainConfiguration
        {
            get { return m_domain; }
            set { m_domain = value; }
        }

        [Parameter(ParameterSetName = NewDomainParameterSetName, Mandatory = false)]
        public DomainConfiguration NewDomainConfiguration
        {
            get { return m_newdomain; }
            set { m_newdomain = value; }
        }

        /// <summary>
        /// InternalProcessRecord method override
        /// </summary>
        protected override void InternalProcessRecord()
        {
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint server farm not found."), ErrorCategory.ResourceUnavailable, this);
            }
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            if (null == service)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint Identity Service not found."), ErrorCategory.ResourceUnavailable, this);
            }
            IdentityServiceApplication existingServiceApplication = service.Applications.GetValue<IdentityServiceApplication>(this.Name);
            if (null == existingServiceApplication)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint Identity Service Application not found."), ErrorCategory.ResourceUnavailable, this);
            }
            if (this.ParameterSetName == AssemblyParameterSetName)
            {
                if (existingServiceApplication.SetAssemblyConfiguration(this.AssemblyConfiguration, this.NewAssemblyConfiguration))
                    this.WriteObject("Assembly defintion correctly updated !");
            }
            else if (this.ParameterSetName == ConfigurationParameterSetName)
            {
                if (existingServiceApplication.SetConnectionConfiguration(this.ConnectionConfiguration, this.NewConnectionConfiguration))
                    this.WriteObject("Connection parameters defintion correctly updated !");
            }
            else if (this.ParameterSetName == DomainParameterSetName)
            {
                if (existingServiceApplication.SetDomainConfiguration(this.DomainConfiguration, this.NewDomainConfiguration))
                    this.WriteObject("Domain Configuration defintion correctly updated !");
            }
            else
            {
                throw new NotSupportedException("Parameter set not supported.");
            }
        }
    }

    [Cmdlet(VerbsCommon.Get, "IdentityServiceApplicationData", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class GetIdentityServiceApplicationData : SPCmdlet
    {
        private const string AssemblyParameterSetName = "Assembly";
        private const string ConfigurationParameterSetName = "ConnectionConfiguration";
        private const string DomainParameterSetName = "DomainConfiguration";
        private const string AllConfigurationParameterSetName = "AllConnectionConfigurations";
        private const string AllDomainParameterSetName = "AllDomainConfigurations";
        private const string AllGlobalParameterSetName = "AllGlobalParameters";
        private const string ReloadParameterSetName = "ReLoad";


        private string m_Name;
        private string m_connection;
        private string m_domain;
        private SwitchParameter m_fulladdesc;
        private SwitchParameter m_allconnection;
        private SwitchParameter m_alldomain;
        private SwitchParameter m_allparameters;

        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [Parameter(ParameterSetName = AssemblyParameterSetName, Mandatory = false)]
        public SwitchParameter AssemblyDecription
        {
            get { return m_fulladdesc; }
            set { m_fulladdesc = value; }
        }

        [Parameter(ParameterSetName = ConfigurationParameterSetName, Mandatory = false)]
        public string ConnectionConfiguration
        {
            get { return m_connection; }
            set { m_connection = value; }
        }

        [Parameter(ParameterSetName = AllConfigurationParameterSetName, Mandatory = false)]
        public SwitchParameter AllConnectionConfigurations
        {
            get { return m_allconnection; }
            set { m_allconnection = value; }
        }

        [Parameter(ParameterSetName = DomainParameterSetName, Mandatory = false)]
        public string DomainConfiguration
        {
            get { return m_domain; }
            set { m_domain = value; }
        }

        [Parameter(ParameterSetName = AllDomainParameterSetName, Mandatory = false)]
        public SwitchParameter AllDomainConfigurations
        {
            get { return m_alldomain; }
            set { m_alldomain = value; }
        }

        [Parameter(ParameterSetName = AllGlobalParameterSetName, Mandatory = false)]
        public SwitchParameter AllGlobalParameters
        {
            get { return m_allparameters; }
            set { m_allparameters = value; }
        }

        /// <summary>
        /// InternalProcessRecord method override
        /// </summary>
        protected override void InternalProcessRecord()
        {
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint server farm not found."), ErrorCategory.ResourceUnavailable, this);
            }
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            if (null == service)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint Identity Service not found."), ErrorCategory.ResourceUnavailable, this);
            }
            IdentityServiceApplication existingServiceApplication = service.Applications.GetValue<IdentityServiceApplication>(this.Name);
            if (null == existingServiceApplication)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint Identity Service Application not found."), ErrorCategory.ResourceUnavailable, this);
            }
            if (this.ParameterSetName == AssemblyParameterSetName)
            {
                this.WriteObject(existingServiceApplication.GetAssemblyConfiguration());
            }
            else if (this.ParameterSetName == ConfigurationParameterSetName)
            {
                this.WriteObject(existingServiceApplication.GetConnectionConfiguration(m_connection));
            }
            else if (this.ParameterSetName == AllConfigurationParameterSetName)
            {
                this.WriteObject(existingServiceApplication.GetConnectionConfigurationList());
            }
            else if (this.ParameterSetName == DomainParameterSetName)
            {
                this.WriteObject(existingServiceApplication.GetDomainConfiguration(m_domain));
            }
            else if (this.ParameterSetName == AllDomainParameterSetName)
            {
                this.WriteObject(existingServiceApplication.GetDomainConfigurationList());
            }
            else if (this.ParameterSetName == AllGlobalParameterSetName)
            {
                this.WriteObject(existingServiceApplication.FillGeneralParameters());
            }
            else if (this.ParameterSetName == ReloadParameterSetName)
            {
                this.WriteObject(existingServiceApplication.Reload());
            }
            else
            {
                throw new NotSupportedException("Parameter set not supported.");
            }
        }
    }

    [Cmdlet(VerbsCommon.Remove, "IdentityServiceApplicationData", SupportsShouldProcess = true)]
    [SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class RemoveIdentityServiceApplicationData : SPCmdlet
    {
        private const string ConfigurationParameterSetName = "ConnectionConfiguration";
        private const string DomainParameterSetName = "DomainConfiguration";

        private string m_Name;
        private ConnectionConfiguration m_connection;
        private DomainConfiguration m_domain;

        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        [Parameter(ParameterSetName = ConfigurationParameterSetName, Mandatory = false)]
        public ConnectionConfiguration ConnectionConfiguration
        {
            get { return m_connection; }
            set { m_connection = value; }
        }

        [Parameter(ParameterSetName = DomainParameterSetName, Mandatory = false)]
        public DomainConfiguration DomainConfiguration
        {
            get { return m_domain; }
            set { m_domain = value; }
        }

        /// <summary>
        /// InternalProcessRecord method override
        /// </summary>
        protected override void InternalProcessRecord()
        {
            SPFarm farm = SPFarm.Local;
            if (null == farm)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint server farm not found."), ErrorCategory.ResourceUnavailable, this);
            }
            AdministrationService service = farm.Services.GetValue<AdministrationService>();
            if (null == service)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint Identity Service not found."), ErrorCategory.ResourceUnavailable, this);
            }
            IdentityServiceApplication existingServiceApplication = service.Applications.GetValue<IdentityServiceApplication>(this.Name);
            if (null == existingServiceApplication)
            {
                ThrowTerminatingError(new InvalidOperationException("SharePoint Identity Service Application not found."), ErrorCategory.ResourceUnavailable, this);
            }
            if (this.ParameterSetName == ConfigurationParameterSetName)
            {
                if (existingServiceApplication.DeleteConnectionConfiguration(this.ConnectionConfiguration))
                    this.WriteObject(string.Format("Connection parameters defintion {0} correctly deleted !", this.ConnectionConfiguration.ConnectionName));
            }
            else if (this.ParameterSetName == DomainParameterSetName)
            {
                if (existingServiceApplication.DeleteDomainConfiguration(this.DomainConfiguration))
                    this.WriteObject(string.Format("Domain Configuration {0} defintion correctly deleted !", this.DomainConfiguration.DnsName));
            }
            else
            {
                throw new NotSupportedException("Parameter set not supported.");
            }
        }
    }

}
