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

namespace SharePoint.IdentityService
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;
    using SharePoint.IdentityService.Core;
    using Microsoft.SharePoint.Administration.Claims;

    [IisWebServiceApplicationProxyBackupBehavior]
    [Guid("EDF76E21-FBA9-404C-B414-A4380D818169")]
    public sealed class ServiceApplicationProxy : SPIisWebServiceApplicationProxy
    {
        [Persisted]
        private SPServiceLoadBalancer m_LoadBalancer;

        // Used to cache the client channel factory
        private string m_EndpointConfigurationName;
        private ChannelFactory<IIdentityServiceContract> m_ChannelFactory;
        private object m_ChannelFactoryLock = new object();

        public ServiceApplicationProxy()
        {
        }

        public ServiceApplicationProxy(string name, IdentityServiceProxy serviceProxy, Uri serviceApplicationAddress) : base(name, serviceProxy, serviceApplicationAddress)
        {
            m_LoadBalancer = new SPRoundRobinServiceLoadBalancer(serviceApplicationAddress);
        }

        #region Display Values
        /// <summary>
        /// TypeName property implementation
        /// </summary>
        public override string TypeName
        {
            get { return "SharePoint Identity Proxy"; }
        }

        /// <summary>
        /// DisplayName property implementation
        /// </summary>
        public override string DisplayName
        {
            get 
            { 
                if (string.IsNullOrEmpty(this.Name))
                   this.Name = "SharePoint Identity Service Proxy";
                return this.Name; 
            }
        }
        #endregion

        #region Provisionning
        /// <summary>
        /// Provision method override
        /// </summary>
        public override void Provision()
        {
            m_LoadBalancer.Provision();
            base.Provision();
            this.Update();
        }

        /// <summary>
        /// Unprovision method override
        /// </summary>
        public override void Unprovision(bool deleteData)
        {
            ProxyClaimsProviderParameters prm = null;
            try
            {
                prm = this.FillClaimsProviderParameters();
                if (prm != null)
                    DropClaimProvider(prm);
            }
            catch
            {
                // Nothing ! let everything in place
            }
            m_LoadBalancer.Unprovision();
            base.Unprovision(deleteData);
            this.Update(true);
        }

        /// <summary>
        /// DropClaimProvider method implmentation
        /// </summary>
        private void DropClaimProvider(ProxyClaimsProviderParameters prm)
        {
            SPClaimProviderManager cpm = SPClaimProviderManager.Local;
            SPClaimProviderDefinition ppv = cpm.GetClaimProvider(prm.ClaimProviderName);
            if (ppv == null)
                return;
            if (ppv.TypeName.ToLower().Equals("sharepoint.identityservice.claimsprovider.identityserviceclaimsprovider"))
            {
                try
                {
                    cpm.DeleteClaimProvider(ppv);
                }
                finally
                {
                    cpm.Update(true);
                }

                if (prm.ClaimProviderMode == ProxyClaimsMode.Windows)
                    ReCreateWindowsClaimProvider(prm);
              /*  else
                {
                    ReCreateTrustedClaimProvider(prm);
                } it seem that it is not needed */
            }
        }

        /// <summary>
        /// ReCreateWindowsClaimProvider method implementation
        /// </summary>
        private void ReCreateWindowsClaimProvider(ProxyClaimsProviderParameters prm)
        {
            SPClaimProviderManager cpm = SPClaimProviderManager.Local;
            try
            {
                SPClaimProviderDefinition ppva = new SPClaimProviderDefinition("AD", "Windows Claim Provider", "Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", "Microsoft.SharePoint.Administration.Claims.SPActiveDirectoryClaimProvider");
                ppva.IsEnabled = true;
                ppva.IsVisible = true;
                cpm.AddClaimProvider(ppva);
            }
            finally
            {
                cpm.Update(true);
            }
        }

        /// <summary>
        /// ReCreateTrustedClaimProvider method implementation
        /// </summary>
/*        private void ReCreateTrustedClaimProvider(ProxyClaimsProviderParameters prm)
        {
            SPClaimProviderManager cpm = SPClaimProviderManager.Local;
            SPSecurityTokenServiceManager ctm = SPSecurityTokenServiceManager.Local;
            try
            {
                SPTrustedLoginProvider lg = ctm.TrustedLoginProviders[prm.TrustedLoginProviderName];
                try
                {
                     SPClaimProviderDefinitionArguments defs = new SPClaimProviderDefinitionArguments(prm.TrustedLoginProviderName, prm.TrustedLoginProviderName, "Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c", "Microsoft.SharePoint.Administration.Claims.SPTrustedClaimProvider");
                     SPTrustedClaimProviderDefinition xdefs = new SPTrustedClaimProviderDefinition(defs, lg);

                     cpm.AddClaimProvider(xdefs);
                     lg.ClaimProviderName = prm.TrustedLoginProviderName;
                }
                finally
                {
                    lg.Update();
                }
            }
            finally
            {
                cpm.Update(true);
            }
        } */
        #endregion

        #region IIdentityServiceContract Members
        /// <summary>
        /// FillSearch method implementation
        /// </summary>
        public ProxyResults FillSearch(string pattern, string domain, bool recursive)
        {
            return Execute("FillSearch", ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.FillSearch(pattern, domain, recursive));
        }

        /// <summary>
        /// FillResolve method implementation
        /// </summary>
        public ProxyResults FillResolve(string pattern, bool recursive)
        {
            return Execute("FillResolve", ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.FillResolve(pattern, recursive));
        }

        /// <summary>
        /// FillResolve method implementation
        /// </summary>
        public ProxyResults FillValidate(string pattern, bool recursive)
        {
            return Execute("FillValidate", ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.FillValidate(pattern, recursive));
        }

        /// <summary>
        /// FillHierarchy method implementation
        /// </summary>
        public ProxyDomain FillHierarchy(string hierarchyNodeID, int numberOfLevels)
        {
            return Execute("FillHierarchy", ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.FillHierarchy(hierarchyNodeID, numberOfLevels));
        }

        /// <summary>
        /// FillAdditionalClaims method implementation
        /// </summary>
        public List<ProxyClaims> FillAdditionalClaims(string entity)
        {
            return Execute("FillAdditionalClaims", ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.FillAdditionalClaims(entity));
        }

        /// <summary>
        /// FillBadDomains method implementation
        /// </summary>
        public List<ProxyBadDomain> FillBadDomains()
        {
            return Execute("FillBadDomains", ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.FillBadDomains());
        }

        /// <summary>
        /// GetServiceApplicationName method implementation
        /// </summary>
        public string GetServiceApplicationName()
        {
            return Execute("GetServiceApplicationName", ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.GetServiceApplicationName());
        }

        /// <summary>
        /// GetGeneralParameters method implementation
        /// </summary>
        public List<ProxyGeneralParameter> FillGeneralParameters()
        {
            return Execute("FillGeneralParameters", ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.FillGeneralParameters());
        }

        /// <summary>
        /// FillClaimsProviderParameters method implementation
        /// </summary>
        /// <returns></returns>
        public ProxyClaimsProviderParameters FillClaimsProviderParameters()
        {
            return Execute("FillClaimsProviderParameters", ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.FillClaimsProviderParameters());
        }

        /// <summary>
        /// LaunchStartCommand method implementation
        /// </summary>
        /// <param name="uri"></param>
        public void LaunchStartCommand(string machine)
        {
           Invoke<IIdentityServiceContract>("LaunchStartCommand", machine, ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.LaunchStartCommand());
        }

        /// <summary>
        /// LaunchReloadCommand method implementation
        /// </summary>
        /// <param name="uri"></param>
        public void LaunchReloadCommand(string machine)
        {
            Execute("LaunchReladCommand", machine, ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.Reload());
        }

        /// <summary>
        /// LaunchReloadCommand method implementation
        /// </summary>
        /// <param name="uri"></param>
        public void LaunchClearCacheCommand(string machine)
        {
            Execute("LaunchClearCacheCommand", machine, ExecuteOptions.AsProcess, (IIdentityServiceContract channel) => channel.ClearCache());
        }

        #endregion

        #region Execution Procedures
        private delegate T ExecuteDelegate<T>(IIdentityServiceContract serviceContract);
        private delegate void InvokeDelegate<T>(IIdentityServiceContract serviceContract);

        /// <summary>
        /// ExecuteOnChannel method implementation
        /// </summary>
        private T Execute<T>(string operationName, ExecuteOptions options, ExecuteDelegate<T> operation)
        {
            T result;
            object obj = null;
            using (new SPMonitoredScope("Execute :" + operationName))
            {
                bool mustexit = false;
                string firstaddress = "";
                do
                {
                    SPServiceLoadBalancerContext loadBalancerContext = m_LoadBalancer.BeginOperation();
                    try
                    {
                        if (firstaddress.Equals(loadBalancerContext.EndpointAddress.ToString()))
                            mustexit = true;
                        if (!mustexit)
                        {
                            if ((loadBalancerContext.Status == SPServiceLoadBalancerStatus.Succeeded))
                            {
                                if (string.IsNullOrEmpty(firstaddress))
                                    firstaddress = loadBalancerContext.EndpointAddress.ToString();

                                IIdentityServiceContract identityapplication = GetChannel(loadBalancerContext.EndpointAddress, options);
                                IClientChannel clientChannel = identityapplication as IClientChannel;
                                try
                                {
                                    obj = operation(identityapplication);
                                    clientChannel.Close();
                                    mustexit = true;
                                }
                                catch (TimeoutException)
                                {
                                    loadBalancerContext.Status = SPServiceLoadBalancerStatus.Failed;
                                }
                                catch (EndpointNotFoundException)
                                {
                                    loadBalancerContext.Status = SPServiceLoadBalancerStatus.Failed;
                                }
                                finally
                                {
                                    if (clientChannel.State != CommunicationState.Closed)
                                    {
                                        clientChannel.Abort();
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        loadBalancerContext.EndOperation();
                    }
                } 
                while (!mustexit);
                result = (T)((object)obj);
            }
            return result;
        }

        /// <summary>
        /// Execute method implementation
        /// </summary>
        private T Execute<T>(string operationName, string uri, ExecuteOptions options, ExecuteDelegate<T> operation)
        {
            T result;
            object obj = null;
            using (new SPMonitoredScope("Execute :" + operationName+"_"+uri))
            {
                string ep = FindLoadBalancerEndPoint(uri);
                if (!string.IsNullOrEmpty(ep))
                {
                    Uri xu = new Uri(ep);
                    IIdentityServiceContract identityapplication = GetChannel(xu, options);
                    IClientChannel clientChannel = identityapplication as IClientChannel;
                    try
                    {
                        obj = operation(identityapplication);
                        clientChannel.Close();
                    }
                    finally
                    {
                        if (clientChannel.State != CommunicationState.Closed)
                        {
                            clientChannel.Abort();
                        }
                    }
                }
            }
            return result = (T)((object)obj);
        }

        /// <summary>
        /// Invoke method implementation
        /// </summary>
        private T Invoke<T>(string operationName, string uri, ExecuteOptions options, InvokeDelegate<T> operation)
        {
            using (new SPMonitoredScope("Invoke :" + operationName + "_" + uri))
            {
                string ep = FindLoadBalancerEndPoint(uri);
                if (!string.IsNullOrEmpty(ep))
                {
                    Uri xu = new Uri(ep);
                    IIdentityServiceContract identityapplication = GetChannel(xu, options);
                    IClientChannel clientChannel = identityapplication as IClientChannel;
                    try
                    {
                        operation(identityapplication);
                        clientChannel.Close();
                    }
                    finally
                    {
                        if (clientChannel.State != CommunicationState.Closed)
                        {
                            clientChannel.Abort();
                        }
                    }
                }
            }
            return default(T);
        }

        /// <summary>
        /// FindLoadBalancerEndPoint method implementation
        /// </summary>
        private string FindLoadBalancerEndPoint(string virtualpath)
        {
            bool mustexit = false;
            string res = string.Empty;
            string firstchanceaddress = "";
            do
            {
                SPServiceLoadBalancerContext loadBalancerContext = m_LoadBalancer.BeginOperation();
                try
                {
                    if (string.IsNullOrEmpty(firstchanceaddress))
                        firstchanceaddress = loadBalancerContext.EndpointAddress.ToString();
                    else if (loadBalancerContext.EndpointAddress.ToString().Equals(firstchanceaddress))
                    {
                        mustexit = true;
                    }
                    if (loadBalancerContext.EndpointAddress.ToString().ToLowerInvariant().Contains(virtualpath.ToLowerInvariant()))
                    {
                        mustexit = true;
                        res = loadBalancerContext.EndpointAddress.ToString();
                    }
                }
                catch (TimeoutException)
                {
                    loadBalancerContext.Status = SPServiceLoadBalancerStatus.Failed;
                }
                catch (EndpointNotFoundException)
                {
                    loadBalancerContext.Status = SPServiceLoadBalancerStatus.Failed;
                }
                finally
                {
                    loadBalancerContext.EndOperation();
                }
            } while (mustexit == false);
            return res;
        }

        /// <summary>
        /// Gets the endpoint configuration name for a given endpoint address.
        /// </summary>
        private string GetEndpointConfigurationName(Uri address)       
        {
            string configurationName;
            if (null == address)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Scheme == Uri.UriSchemeHttps)
            {
                configurationName = "https";
            }
            else if (address.Scheme == Uri.UriSchemeHttp)
            {
                configurationName = "http";
            }
            else
            {
                throw new NotSupportedException("Unsupported endpoint address");
            }
            return configurationName;
        }

        /// <summary>
        /// GetChannel method implementation
        /// </summary>
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Assert, Name = "FullTrust")]
        private IIdentityServiceContract GetChannel(Uri address, ExecuteOptions options)
        {
            string endpointConfigurationName = GetEndpointConfigurationName(address);
            if ((null == m_ChannelFactory) || (endpointConfigurationName != m_EndpointConfigurationName))
            {
                lock (m_ChannelFactoryLock)
                {
                    if ((null == m_ChannelFactory) || (endpointConfigurationName != m_EndpointConfigurationName))
                    {
                        m_ChannelFactory = CreateChannelFactory<IIdentityServiceContract>(endpointConfigurationName);
                        m_EndpointConfigurationName = endpointConfigurationName;
                    }
                }
            }
            if (ExecuteOptions.AsProcess == (options & ExecuteOptions.AsProcess))
            {
                return m_ChannelFactory.CreateChannelAsProcess<IIdentityServiceContract>(new EndpointAddress(address));
            }
            else
            {
                return m_ChannelFactory.CreateChannelActingAsLoggedOnUser<IIdentityServiceContract>(new EndpointAddress(address));
            }
        }

        /// <summary>
        /// CreateChannelFactory method implementation
        /// </summary>
        private ChannelFactory<T> CreateChannelFactory<T>(string endpointConfigurationName)
        {
            string clientConfigurationPath = Microsoft.SharePoint.Utilities.SPUtility.GetCurrentGenericSetupPath(@"WebClients\SharePoint.IdentityService");
            Configuration clientConfiguration = this.OpenClientConfiguration(clientConfigurationPath);
            ConfigurationChannelFactory<T> factory = new ConfigurationChannelFactory<T>(endpointConfigurationName, clientConfiguration, null);
            factory.ConfigureCredentials(SPServiceAuthenticationMode.Claims);
            return factory;
        }
        #endregion
    }
}