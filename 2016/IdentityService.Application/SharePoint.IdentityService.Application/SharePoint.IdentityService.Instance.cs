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
    using Microsoft.SharePoint.Administration;
    using System.Runtime.InteropServices;
    using System.Diagnostics;

    [Guid("E5AF5D0B-C282-404C-925E-7B7FD022042F")]
    internal sealed class IdentityServiceInstance : SPIisWebServiceInstance
    {
        const string _eventlogsource = "ActiveDirectory Identity Service";

        public IdentityServiceInstance()
        {

        }

        /// <summary>
        /// IdentityServiceInstance method implementation
        /// </summary>
        internal IdentityServiceInstance(SPServer server, AdministrationService service): base(server, service)
        {
        }

        /// <summary>
        /// OnDeserialization method override
        /// </summary>
        protected override void OnDeserialization()
        {
            base.OnDeserialization();
        }

        #region Display Values
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

        /// <summary>
        /// TypeName property implementation
        /// </summary>
        public override string TypeName
        {
            get { return "SharePoint Identity Service"; }
        }

        /// <summary>
        /// Provision method override
        /// </summary>
        public override void Provision()
        {
            try
            {
                try
                {
                    if (!EventLog.SourceExists(_eventlogsource))
                        System.Diagnostics.EventLog.CreateEventSource(_eventlogsource, "Application");
                }
                catch
                {
                }
               // if (this.WasCreated)
                base.Provision();
            }
            catch 
            {
                // throw new Exception(E.Message + "\n" + E.StackTrace);
            }
        }
        #endregion
    }
}