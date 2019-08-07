using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Util;

namespace SharePoint.Files.SearchConnector
{
    public class SearchFileProxy: IDisposable
    {
        private const string _searchfileuserclaim = "http://schemas.sharepoint.files.com/ws/2019/06/identity/claims/name";
        private const string _searchfileroleclaim = "http://schemas.sharepoint.files.com/ws/2019/06/identity/claims/role";
        private const string _searchfileissuer = "searchfileissuer";
        private static  Dictionary<string, string> _extensions = new Dictionary<string, string>();
        private bool AllowLocalAccounts = true;
        private string Path = string.Empty;

        /// <summary>
        /// Static constructor
        /// </summary>
        static SearchFileProxy()
        {
            PopulateExtensions();
        }

        public void Connect(string folderpath)
        {
            Path = folderpath;
        }

        #region Folders
        [Browsable(true)]
        public SearchFolder[] GetFolders(string FolderPath)
        {
            List<SearchFolder> myFolders = new List<SearchFolder>();
            foreach (string dirpath in Directory.GetDirectories(FolderPath, "*.*"))
            {
                DirectoryInfo di = new DirectoryInfo(dirpath);
                if ((di.Attributes & FileAttributes.Hidden) == 0)
                {
                    SearchFolder myfolder = new SearchFolder();
                    if (!string.IsNullOrEmpty(di.Extension))
                        myfolder.Path = di.FullName+".";
                    else
                        myfolder.Path = di.FullName;
                    myfolder.Name = di.Name;
                    myfolder.LastModified = di.LastWriteTimeUtc;

                    myfolder.UsesPluggableAuth = true;
                    myfolder.docaclmeta = "access";
                    myFolders.Add(myfolder);
                }
            }
            return myFolders.ToArray();
        }

        [Browsable(true)]
        public SearchFolder GetFolder(string folderpath)
        {
            if (!Directory.Exists(folderpath))
                throw new Microsoft.BusinessData.Runtime.ObjectNotFoundException(String.Format("No folder exists at the path {0}", folderpath));

            DirectoryInfo di = new DirectoryInfo(folderpath);
            SearchFolder myfolder = new SearchFolder();
            if (!string.IsNullOrEmpty(di.Extension))
                myfolder.Path = di.FullName + ".";
            else
                myfolder.Path = di.FullName;
            myfolder.Name = di.Name;         
            myfolder.LastModified = di.LastWriteTimeUtc;
            myfolder.UsesPluggableAuth = true;
            myfolder.docaclmeta = "access";
            return myfolder; 
        }

        [Browsable(true)]
        public byte[] GetFolderSecurity(string filepath)
        {
            if (Directory.Exists(filepath))
            {
                DirectoryInfo fi = new DirectoryInfo(filepath);
                DirectorySecurity sec = fi.GetAccessControl();
                return GetSecurityDescriptor(sec);
            }
            else
                return null;
        }
        #endregion

        #region Files
        [Browsable(true)]
        public SearchFile[] GetFiles(string folderpath)
        {
            List<SearchFile> myfiles = new List<SearchFile>();

            foreach (string filepath in Directory.EnumerateFiles(folderpath, "*.*"))
            {
                FileInfo fi = new FileInfo(filepath);
                if ((fi.Attributes & FileAttributes.Hidden) == 0)
                {
                    SearchFile myfile = new SearchFile();
                    myfile.Path = filepath;
                    myfile.Name = fi.Name;
                    myfile.Extension = fi.Extension.TrimStart(new char[] { '.' });
                    if (!IsFileTypeAllowed(myfile.Extension))
                        continue;
                    myfile.ContentType = GetMimeType(myfile.Extension);
                    myfile.LastModified = fi.LastWriteTimeUtc;

                    myfile.UsesPluggableAuth = true;
                    myfile.docaclmeta = "access";
                    myfiles.Add(myfile);
                }
            }
            return myfiles.ToArray();
        }

        [Browsable(true)]
        public SearchFile GetFile(string filepath)
        {
            if (!File.Exists(filepath))
                throw new Microsoft.BusinessData.Runtime.ObjectNotFoundException(String.Format("No file exists at the path {0}",filepath));

            FileInfo fi = new FileInfo(filepath);
            SearchFile myfile = new SearchFile();
            myfile.Path = filepath;
            myfile.Name = fi.Name;
            myfile.Extension = fi.Extension.TrimStart(new char[] { '.' });
            myfile.ContentType = GetMimeType(myfile.Extension);
            myfile.LastModified = fi.LastWriteTimeUtc;

            myfile.UsesPluggableAuth = true;
            myfile.docaclmeta = "access";
            return myfile;
        }

        [Browsable(true)]
        public FileStream GetFileStream(string filepath)
        {
            if (File.Exists(filepath))
                return new FileStream(filepath, FileMode.Open, FileAccess.Read);
            else
                return null;
        }

        [Browsable(true)]
        public byte[] GetFileSecurity(string filepath)
        {
            if (File.Exists(filepath))
            {
                FileInfo fi = new FileInfo(filepath);
                FileSecurity sec = fi.GetAccessControl();
                return GetSecurityDescriptor(sec);
            }
            else
                return null;
        }

        #endregion

        #region Utility Methods
        /// <summary>
        /// GetPrincipalContext method implementation
        /// </summary>
        private PrincipalContext GetPrincipalContext()
        {
            //TODO : Vérifier la relation d'approbation si multi domaine
            return new PrincipalContext(ContextType.Domain);//, Environment.UserDomainName);
        }

        /// <summary>
        /// GetSecurityDescriptor method implementation
        /// </summary>
        private byte[] GetSecurityDescriptor(FileSystemSecurity fssec)
        {
            AuthorizationRuleCollection acl = fssec.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
            using (var aclStream = new MemoryStream())
            {
                var dest = new BinaryWriter(aclStream);
                foreach (FileSystemAccessRule ace in acl)
                { 
                    if (IsAccountAllowed(ace))
                    {
                        bool isallowed = IsAccessAllowed(ace.FileSystemRights, ace.AccessControlType);
                        PrincipalContext oPrincipalContext = GetPrincipalContext();
                        try
                        {
                            var usrPrincipal = UserPrincipal.FindByIdentity(oPrincipalContext, IdentityType.Sid, ace.IdentityReference.Value);
                            if (usrPrincipal == null)
                            {
                                var grpPrincipal = GroupPrincipal.FindByIdentity(oPrincipalContext, IdentityType.Sid, ace.IdentityReference.Value);
                                if (grpPrincipal != null)
                                {
                                    AddClaimAcl(dest, !isallowed, _searchfileroleclaim, grpPrincipal.Sid.ToString(), _searchfileissuer);
                                    dest.Flush();
                                }
                            }
                            else
                            {
                                AddClaimAcl(dest, !isallowed, _searchfileuserclaim, usrPrincipal.UserPrincipalName, _searchfileissuer);
                                dest.Flush();
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        PrincipalContext oPrincipalContext = GetPrincipalContext();
                        var grpPrincipal = GroupPrincipal.FindByIdentity(oPrincipalContext, IdentityType.Sid, ace.IdentityReference.Value);
                        AddClaimAcl(dest, true, _searchfileroleclaim, grpPrincipal.Sid.ToString(), _searchfileissuer);
                    }
                }
                aclStream.Position = 0;
                return aclStream.ToArray();
            }
        }

        /// <summary>
        /// IsAccountAllowed method implementation
        /// </summary>
        private bool IsAccountAllowed(FileSystemAccessRule ace)
        {
            if (((System.Security.Principal.SecurityIdentifier)ace.IdentityReference).AccountDomainSid != null)
                return true;
            if (AllowLocalAccounts)
            {
                if (((System.Security.Principal.SecurityIdentifier)ace.IdentityReference).Value.ToUpperInvariant().Equals("S-1-2-0")) // Local Connected Users
                    return true;
                else if (((System.Security.Principal.SecurityIdentifier)ace.IdentityReference).Value.ToUpperInvariant().Equals("S-1-5-11")) // Utilisateurs Authentifiés
                    return true;
                else if (((System.Security.Principal.SecurityIdentifier)ace.IdentityReference).Value.ToUpperInvariant().Equals("S-1-5-18")) // Sys Local
                    return true;
                else if (((System.Security.Principal.SecurityIdentifier)ace.IdentityReference).Value.ToUpperInvariant().Equals("S-1-5-19")) // Svc Local
                    return true;
                else if (((System.Security.Principal.SecurityIdentifier)ace.IdentityReference).Value.ToUpperInvariant().Equals("S-1-5-32-544")) // Administrateurs locaux
                    return true;
                else if (((System.Security.Principal.SecurityIdentifier)ace.IdentityReference).Value.ToUpperInvariant().Equals("S-1-5-32-545")) // Utilisateurs locaux
                    return true;
                else if (((System.Security.Principal.SecurityIdentifier)ace.IdentityReference).Value.ToUpperInvariant().Equals("S-1-5-32-547")) // Utilisateurs locaux avec pouvoir
                    return true;
            }
            if (((System.Security.Principal.SecurityIdentifier)ace.IdentityReference).Value.ToUpperInvariant().Equals("S-1-1-0")) // EveryOne
                return true;
            else if (((System.Security.Principal.SecurityIdentifier)ace.IdentityReference).Value.ToUpperInvariant().Equals("S-1-5-11")) // Utilisateurs Authentifiés
                return true;
            return false;

            #region Well Known SIDs
            /*
                Everyone                                        S-1-1-0 
                Enterprise Domain Controllers                   S-1-5-9 
                Authenticated Users                             S-1-5-11 
                Domain Admins                                   S-1-5-21domain-512 
                Domain Users                                    S-1-5-21domain-513 
                Domain Computers                                S-1-5-21domain-515 
                Domain Controllers                              S-1-5-21domain-516 
                Cert Publishers                                 S-1-5-21domain-517 
                Schema Admins                                   S-1-5-21domain-518 
                Enterprise Admins                               S-1-5-21domain-519 
                Group Policy Creator Owners                     S-1-5-21domain-520 
                Administrators                                  S-1-5-32-544 
                Users                                           S-1-5-32-545 
                Guests                                          S-1-5-32-546 
                Account Operators                               S-1-5-32-548 
                Server Operators                                S-1-5-32-549 
                Print Operators                                 S-1-5-32-550 
                Backup Operators                                S-1-5-32-551 
                Replicators                                     S-1-5-32-552 
                Pre-Windows 2000 Compatible Access              S-1-5-32-554 
                Remote Desktop Users                            S-1-5-32-555 
                Network Configuration Operators                 S-1-5-32-556 
                Incoming Forest Trust Builders                  S-1-5-32-557 
                Enterprise Read-only Domain Controllers         S-1-5-21domain-498 
                Read-only Domain Controllers                    S-1-5-21domain-521 
                Allowed RODC Password Replication Group         S-1-5-21domain-571 
                Denied RODC Password Replication Group          S-1-5-21domain-572 
                Event Log Readers S-1-5-32-573 
             */
            #endregion
        }

        /// <summary>
        /// IsAccessAllowed method implementation
        /// </summary>
        private static bool IsAccessAllowed(FileSystemRights fileSystemRights, AccessControlType accessControlType)
        {
            if (accessControlType.HasFlag(AccessControlType.Deny))
                return false;
            if (fileSystemRights.HasFlag(FileSystemRights.Read) ||
                fileSystemRights.HasFlag(FileSystemRights.ReadData) ||
                fileSystemRights.HasFlag(FileSystemRights.ReadAndExecute) ||
                fileSystemRights.HasFlag(FileSystemRights.ListDirectory) ||
                fileSystemRights.HasFlag(FileSystemRights.Traverse) ||
                fileSystemRights.HasFlag(FileSystemRights.Synchronize) ||
                fileSystemRights.HasFlag(FileSystemRights.FullControl))
                return true;
            return false;

            #region FileSystemRights Enum
            /*
            ReadData = 1, // *
            ListDirectory = 1, // * 
            WriteData = 2,
            CreateFiles = 2,
            AppendData = 4,
            CreateDirectories = 4,
            ReadExtendedAttributes = 8,
            WriteExtendedAttributes = 16,
            ExecuteFile = 32,
            Traverse = 32, // ?
            DeleteSubdirectoriesAndFiles = 64,
            ReadAttributes = 128,
            WriteAttributes = 256,
            Write = 278,
            Delete = 65536,       
            ReadPermissions = 131072,
            Read = 131209,  // *
            ReadAndExecute = 131241, // *
            Modify = 197055,
            ChangePermissions = 262144,
            TakeOwnership = 524288,
            Synchronize = 1048576, //*
            FullControl = 2032127 // *
            */
            #endregion
        }

        /// <summary>
        /// ACL Claim encoding
        /// </summary>
        private static void AddClaimAcl(BinaryWriter dest, bool isDeny, string claimtype, string claimvalue, string claimissuer)
        {
            const string datatype = @"http://www.w3.org/2001/XMLSchema#string";

            if (string.IsNullOrEmpty(claimvalue))
            {
                return;
            }

            dest.Write(isDeny ? (byte)1 : (byte)0); // Allow = 0, Deny = 1
            dest.Write((byte)1); // Indicate that this is a non-NT claim type

            // Claim Value
            dest.Write((Int32)claimvalue.Length);
            dest.Write(Encoding.Unicode.GetBytes(claimvalue));

            // Claim Type
            dest.Write((Int32)claimtype.Length);
            dest.Write(Encoding.Unicode.GetBytes(claimtype));

            // Claim Data Value Type
            dest.Write((Int32)datatype.Length);
            dest.Write(Encoding.Unicode.GetBytes(datatype));

            // Claim Original Issuer
            dest.Write((Int32)claimissuer.Length);
            dest.Write(Encoding.Unicode.GetBytes(claimissuer));
        }
       
        /// <summary>
        /// GetMimeType method
        /// </summary>
        public static string GetMimeType(string ext)
        {
            string st = string.Empty;
            if (_extensions.ContainsKey(ext.ToLower()))
                _extensions.TryGetValue(ext.ToLower(), out st);
            else
                st = "text/plain";
            return st;
        }

        /// <summary>
        /// IsFileTypeAllowed method
        /// </summary>
        public static bool IsFileTypeAllowed(string ext)
        {
            return true;
           /* string st = string.Empty;
            if (_extensions.ContainsKey(ext.ToLower()))
                return true;
            else
                return false; */
        }


        /// <summary>
        /// PopulateExtensions() method
        /// </summary>
        private static void PopulateExtensions()
        {
            _extensions.Add("doc", "application/msword");
            _extensions.Add("docm", "application/vnd.ms-word.document.macroEnabled.12");
            _extensions.Add("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            _extensions.Add("dot", "application/msword");
            _extensions.Add("dotm", "application/vnd.ms-word.template.macroEnabledTemplate.12");
            _extensions.Add("dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template");
            _extensions.Add("eml", "message/rfc822");
            _extensions.Add("gif", "image/gif");
            _extensions.Add("html", "text/html");
            _extensions.Add("infopathml", "text/xml");
            _extensions.Add("jpeg", "image/jpeg");
            _extensions.Add("png", "image/png");
            _extensions.Add("mhtml", "multipart/related");
            _extensions.Add("msg", "application/vnd.ms-outlook");
            _extensions.Add("obd", "application/vnd.ms-binder");
            _extensions.Add("obt", "application/vnd.ms-binder");
            _extensions.Add("odp", "application/vnd.oasis.opendocument.presentation");
            _extensions.Add("ods", "application/vnd.oasis.opendocument.spreadsheet");
            _extensions.Add("odt", "application/vnd.oasis.opendocument.text");
            _extensions.Add("one", "application/msonenote");
            _extensions.Add("pdf", "application/pdf");
            _extensions.Add("pot", "application/vnd.ms-powerpoint");
            _extensions.Add("potm", "application/vnd.ms-powerpoint.template.macroEnabled.12");
            _extensions.Add("potx", "application/vnd.openxmlformats-officedocument.presentationml.template");
            _extensions.Add("ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12");
            _extensions.Add("pps", "application/vnd.ms-powerpoint");
            _extensions.Add("ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12");
            _extensions.Add("ppsx", "application/vnd.openxmlformats-officedocument.presentation.slideshow");
            _extensions.Add("ppt", "application/vnd.ms-powerpoint");
            _extensions.Add("pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12");
            _extensions.Add("pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
            _extensions.Add("pub", "application/x-mspublisher");
            _extensions.Add("rtf", "text/rtf");
            _extensions.Add("txt", "text/plain");
            _extensions.Add("vcf", "text/x-vcard");
            _extensions.Add("vcs", "text/x-vCalendar");
            _extensions.Add("vdw", "application/vnd.visio");
            _extensions.Add("vdx", "application/vnd.visio");
            _extensions.Add("vsd", "application/vnd.visio");
            _extensions.Add("vsdm", "application/vnd.ms-visio.drawing.macroEnabled");
            _extensions.Add("vsdx", "application/vnd.ms-visio.drawing");
            _extensions.Add("vss", "application/vnd.visio");
            _extensions.Add("vssm", "application/vnd.ms-visio.stencil.macroEnabled");
            _extensions.Add("vssx", "application/vnd.ms-visio.stencil");
            _extensions.Add("vst", "application/vnd.visio");
            _extensions.Add("vstm", "application/vnd.ms-visio.template.macroEnabled");
            _extensions.Add("vstx", "application/vnd.ms-visio.template");
            _extensions.Add("vsx", "application/vnd.visio");
            _extensions.Add("vtx", "application/vnd.visio");
            _extensions.Add("xlb", "application/vnd.ms-excel");
            _extensions.Add("xlc", "application/vnd.ms-excel");
            _extensions.Add("xls", "application/vnd.ms-excel");
            _extensions.Add("xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12");
            _extensions.Add("xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12");
            _extensions.Add("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            _extensions.Add("xlt", "application/vnd.ms-excel");
            _extensions.Add("xml", "text/xml");
            _extensions.Add("xps", "application/vnd.ms-xpsdocument");
            _extensions.Add("zip", "application/zip");
        }

        public void Dispose()
        {
           // throw new NotImplementedException();
        }
        #endregion
    }
}
