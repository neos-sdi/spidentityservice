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
    using System.Linq;
    using System.Text;
    using Microsoft.SharePoint.Administration;

    [System.Runtime.InteropServices.Guid("AD0A2AF6-925D-404C-A5EB-CC69951B13CA")]
    public sealed class AdministrationService : SPIisWebService, IServiceAdministration
    {
        public AdministrationService()
        {
        }

        // NOTE: A constructor with the signature (String, SPFarm) is required
        // if this service is installed by psconfig -cmd services -install.
        internal AdministrationService(SPFarm farm): base(farm)
        {
        }

        #region IServiceAdministration Members description methods

        /// <summary>
        /// GetApplicationTypes method implementation
        /// </summary>
        public Type[] GetApplicationTypes()
        {
            return new Type[] { typeof(IdentityServiceApplication) };
        }

        /// <summary>
        /// GetApplicationTypeDescription method override
        /// </summary>
        public SPPersistedTypeDescription GetApplicationTypeDescription(Type serviceApplicationType)
        {
            if (serviceApplicationType != typeof(IdentityServiceApplication))
            {
                throw new NotSupportedException();
            }
            return new SPPersistedTypeDescription("SharePoint Identity Service", "SharePoint Identity Service Application.");
        }
        #endregion

        #region Application & Proxy Creation
        /// <summary>
        /// CreateApplication method override 
        /// </summary>
        public SPServiceApplication CreateApplication(string name, Type serviceApplicationType, SPServiceProvisioningContext provisioningContext)
        {
            if (null == provisioningContext)
            {
                throw new ArgumentNullException("provisioningContext");
            }
            if (serviceApplicationType != typeof(IdentityServiceApplication))
            {
                throw new NotSupportedException();
            }
            IdentityServiceApplication application = this.Farm.GetObject(name, this.Id, serviceApplicationType) as IdentityServiceApplication;
            if (null == application)
            {
                SPDatabaseParameters databaseParameters = SPDatabaseParameters.CreateParameters(name, SPDatabaseParameterOptions.None);
                databaseParameters.Validate(SPDatabaseValidation.CreateNew);
                application = IdentityServiceApplication.Create(name, this, provisioningContext.IisWebServiceApplicationPool, databaseParameters);
            }
            return application;
        }

        /// <summary>
        /// CreateProxy method override
        /// </summary>
        public SPServiceApplicationProxy CreateProxy(string name, SPServiceApplication serviceApplication, SPServiceProvisioningContext provisioningContext)
        {
            if (null == serviceApplication)
            {
                throw new ArgumentNullException("ServiceApplication");
            }

            if (serviceApplication.GetType() != typeof(IdentityServiceApplication))
            {
                throw new NotSupportedException();
            }

            IdentityServiceProxy serviceProxy = (IdentityServiceProxy)this.Farm.GetObject(string.Empty, this.Farm.Id, typeof(IdentityServiceProxy));
            if (null == serviceProxy)
            {
                throw new InvalidOperationException("SharePoint.IdentityServiceProxy doesn't exist in the farm");
            }
            ServiceApplicationProxy applicationProxy = serviceProxy.ApplicationProxies.GetValue<ServiceApplicationProxy>(name);
            if (null == applicationProxy)
            {
                applicationProxy = new ServiceApplicationProxy(name, serviceProxy, ((IdentityServiceApplication)serviceApplication).Uri);
            }
            return applicationProxy;
        }
        #endregion

        #region Links
        public override SPAdministrationLink GetCreateApplicationLink(Type serviceApplicationType)
        {
            return new SPAdministrationLink("/_admin/SharePoint.IdentityService/serviceapp.aspx");
        }
        #endregion
    }
}