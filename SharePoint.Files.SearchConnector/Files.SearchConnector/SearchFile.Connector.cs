using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Server.Search.Connector.BDC;

namespace SharePoint.Files.SearchConnector
{
    public class SearchFileConnector : StructuredRepositorySystemUtility<SearchFileProxy>
    {


        protected override SearchFileProxy CreateProxy()
        {
            return new SearchFileProxy();
        }

        protected override void DisposeProxy(SearchFileProxy proxy)
        {
            proxy.Dispose();
        }

        protected override void SetConnectionString(SearchFileProxy proxy, string connectionString)
        {
            Uri startAddress = new Uri(connectionString);
            proxy.Connect(@"\\" + startAddress.Host + startAddress.AbsolutePath.Replace('/', '\\'));
        }

        protected override void SetCertificates(SearchFileProxy proxy, System.Security.Cryptography.X509Certificates.X509CertificateCollection certifcates)
        {
            throw new NotImplementedException();
        }

        protected override void SetCookies(SearchFileProxy proxy, System.Net.CookieContainer cookies)
        {
            throw new NotImplementedException();
        }

        protected override void SetCredentials(SearchFileProxy proxy, string userName, string passWord)
        {
            throw new NotImplementedException();
        }

        protected override void SetProxyServerInfo(SearchFileProxy proxy, string proxyServerName, string bypassList, bool bypassProxyForLocalAddress)
        {
            throw new NotImplementedException();
        }
    }
}
