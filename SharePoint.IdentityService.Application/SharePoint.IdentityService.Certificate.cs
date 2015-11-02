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
using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace SharePoint.IdentityService
{
    class IdentityServiceCertificate
    {

        /// <summary>
        /// This method is used to fetch certificate details insatalled on the machine
        /// using Cryptography 
        /// </summary>
        public static string GetSharePointCertificate()
        {
            string thumbprint = null;
            //Create certificate store object and open the same
            X509Store store = new X509Store("SharePoint", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            //Open certificate collection
            X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
            X509Certificate2Collection findCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindBySubjectName, "SharePoint Security Token Service", false);

            //Iterate through all certificates in the collection
            foreach (X509Certificate2 x509 in findCollection)
            {
                //Fetch the raw Data from certificate object
                byte[] rawData = x509.RawData;
                thumbprint = x509.Thumbprint;
                x509.Reset();
                break;
            }
            store.Close();
            return thumbprint;
        }
    }
}
