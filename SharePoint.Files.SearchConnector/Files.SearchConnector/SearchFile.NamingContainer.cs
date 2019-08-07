using System;
using System.Text;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.BusinessData.Runtime;
using Microsoft.Office.Server.Search.Connector.BDC;
using Microsoft.SharePoint.Utilities;
using System.IO;

namespace SharePoint.Files.SearchConnector
{
    class SearchFileNamingContainer : INamingContainer
    {
        private Uri SourceUri;
        private Uri AccessUri;

        /// <summary>
        /// This defines the crawled property category GUID of any crawled properties emitted by your connector.
        /// </summary>
        private static Guid PropertySetGuid = new Guid("{1851EF67-883E-4620-BABE-AFB56A80FA79}");
        public Guid PropertySet
        {
            get { return SearchFileNamingContainer.PropertySetGuid; }
        }

        public Guid PartitionId
        {
            get { return Guid.Empty; }
        }

        /// <summary>
        /// The Initialize method in this class can't really do any processing because it hasn't received any BCS
        /// metadata objects yet.
        /// </summary>
        public void Initialize(Uri uri)
        {
            this.SourceUri = uri;
        }

        #region AccessUri region
        /// <summary>
        /// GetAccessUri for Entity instance
        /// </summary>
        public Uri GetAccessUri(IEntityInstance entityInstance, IEntityInstance parentEntityInstance)
        {
            return GetAccessUri(entityInstance);
        }

        /// <summary>
        /// GetAccessUri for Entity instance
        /// </summary>
        public Uri GetAccessUri(IEntityInstance entityInstance)
        {

            object[] ids = entityInstance.GetIdentity().GetIdentifierValues();
            string path = ids[0].ToString();

            path = path.Substring(path.LastIndexOf('\\') + 1); 
            this.AccessUri = new Uri(SPHttpUtility.UrlPathEncode(this.SourceUri + "/" + path, false));
            return this.AccessUri;
        }

        /// <summary>
        /// GetAccessUri  Entity
        /// </summary>
        public Uri GetAccessUri(IEntity entity, ILobSystemInstance lobSystemInstance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetAccessUri for LobSystemInstance
        /// </summary>
        public Uri GetAccessUri(ILobSystemInstance lobSystemInstance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetAccessUri for LobSystem
        /// </summary>
        public Uri GetAccessUri(ILobSystem lobSystem)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region DisplayUri region
        /// <summary>
        /// GetDisplayUri for Entity Instance
        /// </summary>
        public Uri GetDisplayUri(IEntityInstance entityInstance, IEntityInstance parentEntityInstance)
        {
            object[] ids = entityInstance.GetIdentity().GetIdentifierValues();

            string pid = ids[0].ToString();
           /* pid = pid.Replace(@".\", @"\");
            if (pid.EndsWith("."))
                pid = pid.Substring(0, pid.Length - 1); */
            return new Uri(pid);
        }

        /// <summary>
        /// GetDisplayUri for Entity Instance
        /// </summary>
        public Uri GetDisplayUri(IEntityInstance entityInstance, string computedDisplayUri)
        {
            if (!String.IsNullOrEmpty(computedDisplayUri))
            {
                return new Uri(computedDisplayUri);
            }
            return GetDisplayUri(entityInstance, (IEntityInstance)null);
        }

        /// <summary>
        /// GetDisplayUri for Entity 
        /// </summary>
        public Uri GetDisplayUri(IEntity entity, ILobSystemInstance lobSystemInstance)
        {
            return null;
        }

        /// <summary>
        /// GetDisplayUri for LobSystemInstance
        /// </summary>
        public Uri GetDisplayUri(ILobSystemInstance lobSystemInstance)
        {
            return null;
        }

        /// <summary>
        /// GetDisplayUri for LobSystem 
        /// </summary>
        public Uri GetDisplayUri(ILobSystem lobSystem)
        {
            return null;
        }
        #endregion
    }
}
