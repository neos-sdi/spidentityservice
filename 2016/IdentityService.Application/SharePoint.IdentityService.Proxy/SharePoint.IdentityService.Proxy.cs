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

namespace SharePoint.IdentityService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.SharePoint.Administration;
    using System.Runtime.InteropServices;
    using Core;
    using System.Diagnostics;
    using Microsoft.SharePoint;

    [Guid("14937DA6-C50B-404C-8E3C-8E19338719B2")]
    [SupportedServiceApplication("948E1B2F-9002-404C-852E-656893CC391F", "16.0.0.0", typeof(IdentityServiceApplicationProxy))]
    public sealed class IdentityServiceProxy : SPIisWebServiceProxy, IServiceProxyAdministration
    {
        private string m_claimProviderName;

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public IdentityServiceProxy()
        {
        }

        /// <summary>
        /// Constructor implementation
        /// </summary>
        public IdentityServiceProxy(SPFarm farm): base(farm)
        {
        }

        #region IServiceProxyAdministration Members
        /// <summary>
        /// GetProxyTypes method implementation
        /// </summary>
        public Type[] GetProxyTypes()
        {
            return new Type[] { typeof(IdentityServiceApplicationProxy) };
        }

        /// <summary>
        /// GetProxyTypeDescription method implementation
        /// </summary>
        public SPPersistedTypeDescription GetProxyTypeDescription(Type serviceApplicationProxyType)
        {
            return new SPPersistedTypeDescription("SharePoint Identity Service Application Proxy", "Connects a Proxy to an SharePoint Identity Service Application.");
        }

        /// <summary>
        /// CreateProxy CreateProxy method iomplementation
        /// </summary>
        public SPServiceApplicationProxy CreateProxy(Type serviceApplicationProxyType, string name, Uri serviceApplicationUri, SPServiceProvisioningContext provisioningContext)
        {
            if (serviceApplicationProxyType != typeof(IdentityServiceApplicationProxy))
               throw new NotSupportedException();
            return new IdentityServiceApplicationProxy(name, this, serviceApplicationUri, ClaimProviderName);
        }
        #endregion

        /// <summary>
        /// ClaimProviderName property implmentation
        /// </summary>
        public string ClaimProviderName
        {
            get { return m_claimProviderName; }
            set { m_claimProviderName = ClaimProviderNameHeader.GetClaimProviderInternalName(value); }
        } 
    }
}