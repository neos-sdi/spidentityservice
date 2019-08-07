using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using PS = Microsoft.SharePoint.PowerShell;


namespace SharePoint.Files.SearchConnector.PowerShell
{
    [Cmdlet(VerbsLifecycle.Install, "SearchFileConnector", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None)]
    [PS.SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class InstallSearchFileConnector : PS.SPCmdlet
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
            ShouldProcessReason reason = ShouldProcessReason.None;
            if (ShouldProcess("Install-SearchFileConnector performs deployment of the sfile:// custom search connector", "Search Services will be restarted on each SharePoint Server", "Install-SearchFileConnector", out reason))
            {
                SPServerCollection servers = SearchFileInstallerUtilities.GetFarmServers();
                try
                {
                    this.WriteVerbose("Registration Registry entries for Search connector sfile://");
                    SearchFileInstallerUtilities.UpdateConnectorRegistry(true, servers, this);
                    this.WriteVerbose("Done !");
                    this.WriteVerbose("Registration of SearchConnector for Search connector sfile://");
                    SearchFileInstallerUtilities.UpdateConnectorRegistration(true, this);
                    this.WriteVerbose("Done !");
                    this.WriteVerbose("Registration of SecurityTrimmerPre for Search connector sfile://");
                    SearchFileInstallerUtilities.UpdateSecurityTrimmerRegistration(true, this);
                    this.WriteVerbose("Done !");
                    this.WriteVerbose("Restart of Search Services");
                    SearchFileInstallerUtilities.RestartSerchService(servers, this);
                    this.WriteVerbose("Done !");
                }
                catch (Exception ex)
                {
                    this.WriteVerbose(string.Format("Error on Installation of SharePoint.File.SearchConnector sfile:// Exception : {0}", ex.Message));
                    throw ex;
                }
            }
        }
    }

    [Cmdlet(VerbsLifecycle.Uninstall, "SearchFileConnector", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High, RemotingCapability = RemotingCapability.None)]
    [PS.SPCmdlet(RequireLocalFarmExist = true, RequireUserFarmAdmin = true)]
    internal sealed class UnInstallSearchFileConnector : PS.SPCmdlet
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
            ShouldProcessReason reason = ShouldProcessReason.None;
            if (ShouldProcess("UnInstall-SearchFileConnector performs retractation of the sfile:// custom search connector", "Search Services will be restarted on each SharePoint Server", "UnInstall-SearchFileConnector", out reason))
            {
                SPServerCollection servers = SearchFileInstallerUtilities.GetFarmServers();
                try
                {
                    this.WriteVerbose("Remove of SecurityTrimmerPre for Search connector sfile://");
                    SearchFileInstallerUtilities.UpdateSecurityTrimmerRegistration(false, this);
                    this.WriteVerbose("Done !");
                    this.WriteVerbose("Remove of SearchConnector for Search connector sfile://");
                    SearchFileInstallerUtilities.UpdateConnectorRegistration(false, this);
                    this.WriteVerbose("Done !");
                    this.WriteVerbose("Remove Registry entries for Search connector sfile://");
                    SearchFileInstallerUtilities.UpdateConnectorRegistry(false, servers, this);
                    this.WriteVerbose("Done !");
                    this.WriteVerbose("Restart of Search Services");
                    SearchFileInstallerUtilities.RestartSerchService(servers, this);
                    this.WriteVerbose("Done !");
                }
                catch (Exception ex)
                {
                    this.WriteVerbose(string.Format("Error on UnInstallation of SharePoint.File.SearchConnector sfile:// Exception : {0}", ex.Message));
                    throw ex;
                }
            }
        }
    }

    internal static class SearchFileInstallerUtilities
    { 
        /// <summary>
        /// GetFarmServers method implementation
        /// </summary>
        internal static SPServerCollection GetFarmServers()
        {
            SPFarm loc = SPFarm.Local;
            return loc.Servers;
        }

        /// <summary>
        /// UpdateConnectorRegistration method implementation
        /// </summary>
        internal static void UpdateConnectorRegistration(bool active, PS.SPCmdlet cmdlet)
        {
            if (IsCustomConnectorInstalled(cmdlet))
                RemoveCustomConnector(cmdlet);
            if (active)
            {
                string filepath = SPUtility.GetCurrentGenericSetupPath(@"CONFIG\SearchConnectors\sfile\") + "model.xml";
                AddCustomConnector(filepath, cmdlet);
            }
        }

        /// <summary>
        /// UpdateConnectorRegistration method implementation
        /// </summary>
        internal static void UpdateSecurityTrimmerRegistration(bool active, PS.SPCmdlet cmdlet)
        {
            if (IsCustomTrimmerInstalled(cmdlet))
                RemoveCustomTrimmer(cmdlet);
            if (active)
                AddCustomTrimmer(cmdlet);
        }

        #region Registry Updates
        /// <summary>
        /// UpdateConnectorRegistry method implementation
        /// </summary>
        internal static void UpdateConnectorRegistry(bool adding, SPServerCollection servers, PS.SPCmdlet cmdlet)
        {
            foreach (SPServer svr in servers)
            {
                if (svr.Role != SPServerRole.Invalid)
                {
                    try
                    {
                        RegistryKey root = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, svr.Address, RegistryView.Default);
                        using (RegistryKey key = root.OpenSubKey(@"SOFTWARE\Microsoft\Office Server\16.0\Search\Setup\ProtocolHandlers", true))
                        {
                            if (key != null)
                            {
                                if (adding)
                                {
                                    cmdlet.WriteVerbose(string.Format("Adding Registry keys for {0}", svr.Name));
                                    key.SetValue("sfile", "OSearch16.ConnectorProtocolHandler.1", RegistryValueKind.String);
                                }
                                else
                                {
                                    cmdlet.WriteVerbose(string.Format("Removing Registry keys for {0}", svr.Name));
                                    key.DeleteValue("sfile", false);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        cmdlet.WriteVerbose(string.Format("Registry Keys on Server {0} error : {1}", svr.Name, e.Message));
                        throw e;
                    }
                }
            }
        }
        #endregion

        #region Start / Stop Search Service
        /// <summary>
        /// ResetSerchService restart serarch Service
        /// </summary>
        internal static void RestartSerchService(SPServerCollection servers, PS.SPCmdlet cmdlet)
        {
            foreach (SPServer svr in servers)
            {
                if (svr.Role != SPServerRole.Invalid)
                {

                    try
                    {
                        ServiceController service = new ServiceController("OSearch16", svr.Address);
                        if (service != null)
                        {
                            if (service.Status.Equals(ServiceControllerStatus.Running)) 
                            {
                                cmdlet.WriteVerbose(string.Format("Stopping Search Service on {0}", svr.Name));
                                service.Stop();
                            }
                            cmdlet.WriteVerbose(string.Format("Starting Search Service on {0}", svr.Name));
                            service.Start();
                            cmdlet.WriteVerbose("Done !");
                        }
                    }
                    catch (Exception e)
                    {
                        cmdlet.WriteVerbose(string.Format("Your must restart All Search Services on Server {0} error : {1}", svr.Name, e.Message));
                    }
                }
            }
        }
        #endregion

        #region PowerShell Connector Registration
        /// <summary>
        /// IsCustomConnectorInstalled method implementation
        /// </summary>
        internal static bool IsCustomConnectorInstalled(PS.SPCmdlet cmdlet)
        {
            bool result = false;
            Runspace SPRunSpace = null;
            System.Management.Automation.PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                PSSnapInException SPExcept = null;
                PSSnapInInfo SPSnapInfo = SPRunConfig.AddPSSnapIn("Microsoft.SharePoint.PowerShell", out SPExcept);
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = System.Management.Automation.PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command AppListCommand = new Command("Get-SPEnterpriseSearchServiceApplication", false);
                pipeline.Commands.Add(AppListCommand);
                Collection<PSObject> PSOutput = pipeline.Invoke();

                foreach (PSObject searchapp in PSOutput)
                {
                    if (searchapp != null)
                    {
                        Pipeline spipeline = SPRunSpace.CreatePipeline();
                        Command AppCommand = new Command("Get-SPEnterpriseSearchCrawlCustomConnector", false);
                        CommandParameter AppParam = new CommandParameter("SearchApplication", searchapp);
                        AppCommand.Parameters.Add(AppParam);
                        CommandParameter proParam = new CommandParameter("Protocol", "sfile");
                        AppCommand.Parameters.Add(proParam);
                        spipeline.Commands.Add(AppCommand);

                        Command outDefault = new Command("out-default");
                        outDefault.MergeMyResults(PipelineResultTypes.All, PipelineResultTypes.Output);
                        pipeline.Commands.Add(outDefault);

                        Collection<PSObject> PSSubOutput = spipeline.Invoke();

                        foreach (PSObject customconnector in PSSubOutput)
                        {
                            if (customconnector != null)
                            {
                                result = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return result;
        }

        /// <summary>
        /// AddCustomConnector method implementation
        /// </summary>
        internal static bool AddCustomConnector(string modelfilepath, PS.SPCmdlet cmdlet)
        {
            bool result = false;
            Runspace SPRunSpace = null;
            System.Management.Automation.PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                PSSnapInException SPExcept = null;
                PSSnapInInfo SPSnapInfo = SPRunConfig.AddPSSnapIn("Microsoft.SharePoint.PowerShell", out SPExcept);
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = System.Management.Automation.PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command AppListCommand = new Command("Get-SPEnterpriseSearchServiceApplication", false);
                pipeline.Commands.Add(AppListCommand);
                Collection<PSObject> PSOutput = pipeline.Invoke();

                foreach (PSObject searchapp in PSOutput)
                {
                    if (searchapp != null)
                    {
                        Pipeline spipeline = SPRunSpace.CreatePipeline();
                        Command AppCommand = new Command("New-SPEnterpriseSearchCrawlCustomConnector", false);
                        CommandParameter AppParam = new CommandParameter("SearchApplication", searchapp);
                        AppCommand.Parameters.Add(AppParam);
                        CommandParameter proParam = new CommandParameter("Protocol", "sfile");
                        AppCommand.Parameters.Add(proParam);
                        CommandParameter modParam = new CommandParameter("ModelFilePath", modelfilepath);
                        AppCommand.Parameters.Add(modParam);
                        spipeline.Commands.Add(AppCommand);

                        Command outDefault = new Command("out-default");
                        outDefault.MergeMyResults(PipelineResultTypes.All, PipelineResultTypes.Output);
                        pipeline.Commands.Add(outDefault);

                        Collection<PSObject> PSSubOutput = spipeline.Invoke();


                        foreach (PSObject customconnector in PSSubOutput)
                        {
                            if (customconnector != null)
                            {
                                result = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return result;
        }

        /// <summary>
        /// RemoveCustomConnector method implementation
        /// </summary>
        internal static bool RemoveCustomConnector(PS.SPCmdlet cmdlet)
        {
            bool result = false;
            Runspace SPRunSpace = null;
            System.Management.Automation.PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                PSSnapInException SPExcept = null;
                PSSnapInInfo SPSnapInfo = SPRunConfig.AddPSSnapIn("Microsoft.SharePoint.PowerShell", out SPExcept);
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = System.Management.Automation.PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command AppListCommand = new Command("Get-SPEnterpriseSearchServiceApplication", false);
                pipeline.Commands.Add(AppListCommand);
                Collection<PSObject> PSOutput = pipeline.Invoke();

                foreach (PSObject searchapp in PSOutput)
                {
                    if (searchapp != null)
                    {
                        Pipeline spipeline = SPRunSpace.CreatePipeline();
                        try
                        {
                            Command AppCommand = new Command("Remove-SPEnterpriseSearchCrawlCustomConnector", false);
                            CommandParameter AppParam = new CommandParameter("SearchApplication", searchapp);
                            AppCommand.Parameters.Add(AppParam);
                            CommandParameter proParam = new CommandParameter("Identity", "sfile");
                            AppCommand.Parameters.Add(proParam);
                            CommandParameter confParam = new CommandParameter("Confirm", false);
                            AppCommand.Parameters.Add(confParam);
                            spipeline.Commands.Add(AppCommand);

                            Command outDefault = new Command("out-default");
                            outDefault.MergeMyResults(PipelineResultTypes.All, PipelineResultTypes.Output);
                            pipeline.Commands.Add(outDefault);

                            Collection<PSObject> PSSubOutput = spipeline.Invoke();

                            foreach (PSObject customconnector in PSSubOutput)
                            {
                                if (customconnector != null)
                                {
                                    result = true;
                                }
                            }
                        }
                        finally
                        {

                        }
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return result;
        }
        #endregion

        #region PowerShell Security Trimmer Registration
        /// <summary>
        /// IsCustomTrimmerInstalled method implementation
        /// </summary>
        internal static bool IsCustomTrimmerInstalled(PS.SPCmdlet cmdlet)
        {
            bool result = false;
            Runspace SPRunSpace = null;
            System.Management.Automation.PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                PSSnapInException SPExcept = null;
                PSSnapInInfo SPSnapInfo = SPRunConfig.AddPSSnapIn("Microsoft.SharePoint.PowerShell", out SPExcept);
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = System.Management.Automation.PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command AppListCommand = new Command("Get-SPEnterpriseSearchServiceApplication", false);
                pipeline.Commands.Add(AppListCommand);
                Collection<PSObject> PSOutput = pipeline.Invoke();

                foreach (PSObject searchapp in PSOutput)
                {
                    if (searchapp != null)
                    {
                        Pipeline spipeline = SPRunSpace.CreatePipeline();
                        Command AppCommand = new Command("Get-SPEnterpriseSearchSecurityTrimmer", false);
                        CommandParameter AppParam = new CommandParameter("SearchApplication", searchapp);
                        AppCommand.Parameters.Add(AppParam);
                        CommandParameter proParam = new CommandParameter("Identity", 457);
                        AppCommand.Parameters.Add(proParam);
                        spipeline.Commands.Add(AppCommand);

                        Command outDefault = new Command("out-default");
                        outDefault.MergeMyResults(PipelineResultTypes.All, PipelineResultTypes.Output);
                        pipeline.Commands.Add(outDefault);

                        Collection<PSObject> PSSubOutput = spipeline.Invoke();

                        foreach (PSObject customconnector in PSSubOutput)
                        {
                            if (customconnector != null)
                            {
                                result = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return result;
        }

        /// <summary>
        /// AddCustomTrimmer method implementation
        /// </summary>
        internal static bool AddCustomTrimmer(PS.SPCmdlet cmdlet)
        {
            bool result = false;
            Runspace SPRunSpace = null;
            System.Management.Automation.PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                PSSnapInException SPExcept = null;
                PSSnapInInfo SPSnapInfo = SPRunConfig.AddPSSnapIn("Microsoft.SharePoint.PowerShell", out SPExcept);
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = System.Management.Automation.PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command AppListCommand = new Command("Get-SPEnterpriseSearchServiceApplication", false);
                pipeline.Commands.Add(AppListCommand);
                Collection<PSObject> PSOutput = pipeline.Invoke();

                foreach (PSObject searchapp in PSOutput)
                {
                    if (searchapp != null)
                    {
                        Pipeline spipeline = SPRunSpace.CreatePipeline();
                        Command AppCommand = new Command("New-SPEnterpriseSearchSecurityTrimmer", false);
                        CommandParameter AppParam = new CommandParameter("SearchApplication", searchapp);
                        AppCommand.Parameters.Add(AppParam);
                        CommandParameter proParam = new CommandParameter("TypeName", "SharePoint.Files.SearchConnector.SearchPreTrimmer, SharePoint.Files.SearchConnector, Version=1.0.0.0, Culture=neutral, PublicKeyToken=1c8bdbf732fc20f9");
                        AppCommand.Parameters.Add(proParam);
                        CommandParameter modParam = new CommandParameter("Id", 457);
                        AppCommand.Parameters.Add(modParam);
                        spipeline.Commands.Add(AppCommand);

                        Command outDefault = new Command("out-default");
                        outDefault.MergeMyResults(PipelineResultTypes.All, PipelineResultTypes.Output);
                        pipeline.Commands.Add(outDefault);

                        Collection<PSObject> PSSubOutput = spipeline.Invoke();


                        foreach (PSObject customtrimmer in PSSubOutput)
                        {
                            if (customtrimmer != null)
                            {
                                result = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return result;
        }

        /// <summary>
        /// RemoveCustomTrimmer method implementation
        /// </summary>
        internal static bool RemoveCustomTrimmer(PS.SPCmdlet cmdlet)
        {
            bool result = false;
            Runspace SPRunSpace = null;
            System.Management.Automation.PowerShell SPPowerShell = null;
            try
            {
                RunspaceConfiguration SPRunConfig = RunspaceConfiguration.Create();
                PSSnapInException SPExcept = null;
                PSSnapInInfo SPSnapInfo = SPRunConfig.AddPSSnapIn("Microsoft.SharePoint.PowerShell", out SPExcept);
                SPRunSpace = RunspaceFactory.CreateRunspace(SPRunConfig);
                SPPowerShell = System.Management.Automation.PowerShell.Create();
                SPPowerShell.Runspace = SPRunSpace;
                SPRunSpace.Open();

                Pipeline pipeline = SPRunSpace.CreatePipeline();
                Command AppListCommand = new Command("Get-SPEnterpriseSearchServiceApplication", false);
                pipeline.Commands.Add(AppListCommand);
                Collection<PSObject> PSOutput = pipeline.Invoke();

                foreach (PSObject searchapp in PSOutput)
                {
                    if (searchapp != null)
                    {
                        Pipeline spipeline = SPRunSpace.CreatePipeline();
                        try
                        {
                            Command AppCommand = new Command("Remove-SPEnterpriseSearchSecurityTrimmer", false);
                            CommandParameter AppParam = new CommandParameter("SearchApplication", searchapp);
                            AppCommand.Parameters.Add(AppParam);
                            CommandParameter proParam = new CommandParameter("Id", 457);
                            AppCommand.Parameters.Add(proParam);
                            CommandParameter confParam = new CommandParameter("Confirm", false);
                            AppCommand.Parameters.Add(confParam);
                            spipeline.Commands.Add(AppCommand);

                            Command outDefault = new Command("out-default");
                            outDefault.MergeMyResults(PipelineResultTypes.All, PipelineResultTypes.Output);
                            pipeline.Commands.Add(outDefault);

                            Collection<PSObject> PSSubOutput = spipeline.Invoke();

                            foreach (PSObject customtrimmer in PSSubOutput)
                            {
                                if (customtrimmer != null)
                                {
                                    result = true;
                                }
                            }
                        }
                        finally
                        {

                        }
                    }
                }
            }
            finally
            {
                if (SPRunSpace != null)
                    SPRunSpace.Close();
            }
            return result;
        }
        #endregion
    }
}
