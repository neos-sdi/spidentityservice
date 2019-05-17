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

namespace SharePoint.IdentityService.PowerShell
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Management.Automation;
    using System.Security;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.PowerShell;
    using SharePoint.IdentityService;

    
    [Cmdlet(VerbsCommon.New, "IdentityServiceProxy", SupportsShouldProcess = true)]
    internal sealed class NewIdentityServiceProxy : SPCmdlet
    {
        private const string UriParameterSetName = "Uri";
        private const string ServiceApplicationParameterSetName = "ServiceApplication";

        private string m_Name;
        private string m_ClaimProviderName;
        private Uri m_Uri;
        private SPServiceApplicationPipeBind m_ServiceApplicationPipeBind;

        /// <summary>
        /// Name property implementation
        /// </summary>
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        /// <summary>
        /// ClaimProviderName property implementation
        /// </summary>
        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string ClaimProviderName
        {
            get { return m_ClaimProviderName; }
            set { m_ClaimProviderName = value; }
        }

        /// <summary>
        /// Uri property implementation
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = UriParameterSetName)]
        [ValidateNotNullOrEmpty]
        public string Uri
        {
            get { return m_Uri.ToString(); }
            set { m_Uri = new Uri(value); }
        }

        /// <summary>
        /// ServiceApplication property implementation
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ServiceApplicationParameterSetName)]
        public SPServiceApplicationPipeBind ServiceApplication
        {
            get { return m_ServiceApplicationPipeBind; }
            set { m_ServiceApplicationPipeBind = value; }
        }

        /// <summary>
        /// RequireUserFarmAdmin method implementation
        /// </summary>
        protected override bool RequireUserFarmAdmin()
        {
            return true;
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
            IdentityServiceProxy serviceProxy = farm.ServiceProxies.GetValue<IdentityServiceProxy>();
            if (null == serviceProxy)
            {
                ThrowTerminatingError(new InvalidOperationException("Identity Web Service proxy not found."), ErrorCategory.ResourceUnavailable, this);
            }
            IdentityServiceApplicationProxy existingServiceApplicationProxy = serviceProxy.ApplicationProxies.GetValue<IdentityServiceApplicationProxy>();
            if (null != existingServiceApplicationProxy)
            {
                WriteError(new InvalidOperationException("Identity Web service application proxy exists."), ErrorCategory.ResourceExists, existingServiceApplicationProxy);
                SkipProcessCurrentRecord();
            }
            Uri serviceApplicationUri = null;
            if (this.ParameterSetName == UriParameterSetName)
            {
                serviceApplicationUri = m_Uri;
            }
            else if (this.ParameterSetName == ServiceApplicationParameterSetName)
            {
                SPServiceApplication serviceApplication = m_ServiceApplicationPipeBind.Read();
                if (null == serviceApplication)
                {
                    WriteError(new InvalidOperationException("Service application not found."), ErrorCategory.ResourceExists, serviceApplication);
                    SkipProcessCurrentRecord();
                }
                ISharedServiceApplication sharedServiceApplication = serviceApplication as ISharedServiceApplication;
                if (null == sharedServiceApplication)
                {
                    WriteError(new InvalidOperationException("Connecting to the specified service application is not supported."), ErrorCategory.ResourceExists, serviceApplication);
                    SkipProcessCurrentRecord();
                }
                serviceApplicationUri = sharedServiceApplication.Uri;
            }
            else
            {
                ThrowTerminatingError(new InvalidOperationException("Invalid parameter set."), ErrorCategory.InvalidArgument, this);
            }
            if((null!=serviceApplicationUri) && (ShouldProcess(this.Name)))
            {
                IdentityServiceApplicationProxy serviceApplicationProxy = new IdentityServiceApplicationProxy(this.Name, serviceProxy, serviceApplicationUri, this.ClaimProviderName);
                serviceApplicationProxy.Provision();
                WriteObject(serviceApplicationProxy);
            }
        }
    }
}