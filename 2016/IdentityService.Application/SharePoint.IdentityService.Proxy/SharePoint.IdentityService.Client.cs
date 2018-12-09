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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using SharePoint.IdentityService.Core;
    using Microsoft.SharePoint.Administration.Claims;

    [Flags]
    public enum ExecuteOptions
    {
        None = 0x0,
        AsProcess = 0x1,
        Async = 0x2,
    }

    public sealed class IdentityServiceClient
    {
        private ServiceApplicationProxy m_proxy;
        private string m_selector;
        private bool m_isinitialized = false;


        /// <summary>
        /// 
        /// IdentityServiceClient constructor
        /// </summary>
        public IdentityServiceClient(string selector)
        {
            m_selector = selector;
            try
            {
                m_proxy = GetProxy();
                m_isinitialized = true;
            }
            catch (Exception)
            {
                // Nothing 
            }
        }

        /// <summary>
        /// IsInitialized property implementation
        /// </summary>
        public bool IsInitialized
        {
            get { return m_isinitialized; }
        }

        /// <summary>
        /// GetProxy method implementation
        /// </summary>
        private ServiceApplicationProxy GetProxy()
        {
            if (m_proxy == null)
            {
               // if (string.IsNullOrEmpty(m_selector))
                    m_proxy = GetDefaultProxy();
               // else
               // {
               //     m_proxy = FindProxy(m_selector);
               // }
            }
            if (m_proxy == null)
            {
                throw new ArgumentNullException("Identity Service Proxy", "This Web Application has no Identity Service Application assigned !");
            }
            return m_proxy;
        }

        /// <summary>
        /// FindProxy method implementation
        /// </summary>
        /// <returns></returns>
        private ServiceApplicationProxy FindProxy(string selector)
        {
            SPServiceProxyCollection proxies = SPFarm.Local.ServiceProxies;
            foreach (SPServiceProxy SPSP in proxies)
            {
                foreach (SPServiceApplicationProxy SPAP in SPSP.ApplicationProxies)
                {
                    if (SPAP is ServiceApplicationProxy)
                    {
                        ServiceApplicationProxy b = SPAP as ServiceApplicationProxy;
                        ProxyClaimsProviderParameters prm = b.FillClaimsProviderParameters();
                        if ((prm.ClaimProviderName != null) && (prm.ClaimProviderName.ToLower().Trim().Equals(selector.ToLower().Trim())))
                        {
                            return b;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// GetDefaultProxy method implementation
        /// </summary>
        /// <returns></returns>
        private ServiceApplicationProxy GetDefaultProxy()
        {
            SPServiceProxyCollection proxies = SPFarm.Local.ServiceProxies;
            foreach (SPServiceProxy SPSP in proxies)
            {
                foreach (SPServiceApplicationProxy SPAP in SPSP.ApplicationProxies)
                {
                    if (SPAP is ServiceApplicationProxy)
                    {
                        ServiceApplicationProxy b = SPAP as ServiceApplicationProxy;
                        return b;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// FillSearch method implementation
        /// </summary>
        public ProxyResults FillSearch(string pattern, string domain, bool recursive)
        {
            return GetProxy().FillSearch(pattern, domain, recursive);
        }

        /// <summary>
        /// FillResolve method implementation
        /// </summary>
        public ProxyResults FillResolve(string pattern, bool recursive)
        {
            return GetProxy().FillResolve(pattern, recursive);
        }

        /// <summary>
        /// FillValidate method implementation
        /// </summary>
        public ProxyResults FillValidate(string pattern, bool recursive)
        {
            return GetProxy().FillValidate(pattern, recursive);
        }

        /// <summary>
        /// FillHierarchy method implementation
        /// </summary>
        public ProxyDomain FillHierarchy(string hierarchyNodeID, int numberOfLevels)
        {
            return GetProxy().FillHierarchy(hierarchyNodeID, numberOfLevels);
        }

        /// <summary>
        /// FillAdditionalClaims method implemetation
        /// </summary>
        public List<ProxyClaims> FillAdditionalClaims(string entity)
        {
            return GetProxy().FillAdditionalClaims(entity);
        }

        /// <summary>
        /// FillBadDomains method implementation
        /// </summary>
        public List<ProxyBadDomain> FillBadDomains()
        {
            return GetProxy().FillBadDomains();
        }

        /// <summary>
        /// FillGeneralParameters method implementation
        /// </summary>
        public List<ProxyGeneralParameter> FillGeneralParameters()
        {
            return GetProxy().FillGeneralParameters();
        }

        /// <summary>
        /// FillClaimsProviderParameters method implementation
        /// </summary>
        public ProxyClaimsProviderParameters FillClaimsProviderParameters()
        {
           return GetProxy().FillClaimsProviderParameters();
        }

        /// <summary>
        /// GetServiceApplicationName method implementation
        /// </summary>
        public string GetServiceApplicationName()
        {
            return GetProxy().GetServiceApplicationName();
        }
    }
}