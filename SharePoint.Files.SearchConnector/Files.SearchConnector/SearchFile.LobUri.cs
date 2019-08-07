using System;
using System.Text;
using Microsoft.BusinessData.MetadataModel;
using Microsoft.Office.Server.Search.Connector.BDC;
using Microsoft.BusinessData.Runtime;
using Microsoft.SharePoint.Utilities;
using System.IO;

namespace SharePoint.Files.SearchConnector
{
    class SearchFileLobUri : LobUri
    {
        public SearchFileLobUri(): base("sfile")
        {
           this.lobSystem = this.Catalog.GetLobSystem("SearchFileSystem");
           this.lobSystemInstance = this.lobSystem.GetLobSystemInstances()[0].Value;
        }

        public override void Initialize(Microsoft.Office.Server.Search.Connector.IConnectionContext context)
        {
            Uri sourceUri = context.Path;
            
            string filepath = @"\\" + sourceUri.Host + sourceUri.AbsolutePath.Replace('/', '\\');

            filepath = SPHttpUtility.UrlPathDecode(filepath, false);
            if (Directory.Exists(filepath))
            {
                this.entity = this.Catalog.GetEntity("SearchFileConnector", "SearchFolder");
                this.identity = new Identity(filepath);
            }
            else if (File.Exists(filepath))
            {
                this.entity = this.Catalog.GetEntity("SearchFileConnector", "SearchFile");
                this.identity = new Identity(filepath);
            }
            else
            {
                this.entity = this.Catalog.GetEntity("SearchFileConnector", "SearchFolder");
                this.identity = null;
            }
        }

        private ILobSystem lobSystem;
        public override ILobSystem LobSystem => this.lobSystem;

        private ILobSystemInstance lobSystemInstance;
        public override ILobSystemInstance LobSystemInstance => this.lobSystemInstance;

        private IEntity entity;
        public override IEntity Entity => this.entity;

        private Microsoft.BusinessData.Runtime.Identity identity;
        public override Microsoft.BusinessData.Runtime.Identity Identity => this.identity;

        public override Guid PartitionId
        {
            get { throw new NotImplementedException(); }
        }

        private Uri sourceUri;
        public override Uri SourceUri
        {
            get
            {
                return this.sourceUri;
            }
            set
            {
                this.sourceUri = value;
            }
        }
    }
}
