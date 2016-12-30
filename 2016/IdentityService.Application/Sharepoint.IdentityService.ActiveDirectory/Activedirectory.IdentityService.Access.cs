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
#define enabledisabled

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace SharePoint.IdentityService.ActiveDirectory
{
    using System.DirectoryServices;
    using System.DirectoryServices.AccountManagement;
    using System.DirectoryServices.ActiveDirectory;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using SharePoint.IdentityService.Core;
    using Core = SharePoint.IdentityService.Core;
    using Microsoft.SharePoint;
    using System.Web;
    using System.IO;
    using System.Diagnostics;
    using System.Xml;
    using System.Text;
    using System.Runtime.Serialization;

 
    #region Utility Classes
    #region ActiveDirectoryUserSearchMode enumeration
    [Flags]
    public enum ActiveDirectoryUserSearchMode
    {
        AllOptions = 0,
        UserPrincipalName = 1,
        DisplayName = 2 ,
        SamAccount = 4,
        SID = 8
   //     Groups = 8
    }
    #endregion

    #region ActiveDirectoryInspectValues
    public class ActiveDirectoryInspectValues
    {
        public ActiveDirectoryUserSearchMode Mode = ActiveDirectoryUserSearchMode.AllOptions;
        public string DomainPart;
        public string UserNamePart;
        public string Pattern;
        public bool Tagged = false;
        public bool IsLocal = false;
       // public bool IsSID = false;

        /// <summary>
        /// HasDomain property implementation
        /// </summary>
        public bool HasDomain
        {
            get { return (!string.IsNullOrEmpty(DomainPart)) && Tagged && (!DomainPart.ToLowerInvariant().Equals("builtin"));}
        }

        /// <summary>
        /// IsUPNForm property implementation
        /// </summary>
        public bool IsUPNForm(bool strict = true)
        {
            if (strict)
                return (((Mode & ActiveDirectoryUserSearchMode.UserPrincipalName) == ActiveDirectoryUserSearchMode.UserPrincipalName));
            else
                return (((Mode & ActiveDirectoryUserSearchMode.UserPrincipalName) == ActiveDirectoryUserSearchMode.UserPrincipalName) || (Mode == ActiveDirectoryUserSearchMode.AllOptions));
        }

        /// <summary>
        /// IsSAMForm property implementation
        /// </summary>
        public bool IsSAMForm(bool strict = true)
        {
            if (strict)
               // return (((Mode & ActiveDirectoryUserSearchMode.Groups) == ActiveDirectoryUserSearchMode.Groups) || ((Mode & ActiveDirectoryUserSearchMode.SamAccount) == ActiveDirectoryUserSearchMode.SamAccount));
                return (((Mode & ActiveDirectoryUserSearchMode.SamAccount) == ActiveDirectoryUserSearchMode.SamAccount));
            else
               // return (((Mode & ActiveDirectoryUserSearchMode.Groups) == ActiveDirectoryUserSearchMode.Groups) || ((Mode & ActiveDirectoryUserSearchMode.SamAccount) == ActiveDirectoryUserSearchMode.SamAccount) || (Mode == ActiveDirectoryUserSearchMode.AllOptions));
                return (((Mode & ActiveDirectoryUserSearchMode.SamAccount) == ActiveDirectoryUserSearchMode.SamAccount) || (Mode == ActiveDirectoryUserSearchMode.AllOptions));
        }

        /// <summary>
        /// IsDisplayForm property implementation
        /// </summary>
        public bool IsDisplayForm(bool strict = true)
        {
            if (strict)
                return (((Mode & ActiveDirectoryUserSearchMode.DisplayName) == ActiveDirectoryUserSearchMode.DisplayName));
            else
                return (((Mode & ActiveDirectoryUserSearchMode.DisplayName) == ActiveDirectoryUserSearchMode.DisplayName) || (Mode == ActiveDirectoryUserSearchMode.AllOptions));
        }

        /// <summary>
        /// IsSID property implementation
        /// </summary>
        public bool IsSID(bool strict = true)
        {
            if (strict)
                return (((Mode & ActiveDirectoryUserSearchMode.SID) == ActiveDirectoryUserSearchMode.SID));
            else
                return (((Mode & ActiveDirectoryUserSearchMode.SID) == ActiveDirectoryUserSearchMode.SID) || (Mode == ActiveDirectoryUserSearchMode.AllOptions));
        }

        /// <summary>
        /// IsAllOptions 
        /// </summary>
        public bool IsAllOptions()
        {
            return (Mode == ActiveDirectoryUserSearchMode.AllOptions);
        }

        /// <summary>
        /// CheckDomain
        /// </summary>
        public bool CheckDomain(IDomain domain)
        {
            bool match = false;
            if (HasDomain)
            {
                string g = DomainPart.ToLowerInvariant();
                if (g.StartsWith("*"))
                    g = g.TrimStart('*');
                match =  (domain.DnsName.ToLowerInvariant().EndsWith(g) || g.Equals(domain.NetbiosName.ToLowerInvariant()));
                if ((!match) && (domain is IRootDomain))
                {
                    IRootDomain d = domain as IRootDomain;
                    foreach (ITopLevelName s in d.TopLevelNames)
                    {
                        if (g.EndsWith(s.TopLevelName.ToLowerInvariant()) && (s.Status == Core.TopLevelNameStatus.Enabled))
                            return true;
                    }
                }
                return match;
            }
            else
                return false;
        }

    }
    #endregion

    #region ActiveDirectoryRegEx
    public static class ActiveDirectoryRegEx
    {
        private static string domainpattern = @"^([a-z][a-z0-9._-]+)\\((?! +\r?$)[a-z0-9'’éèùàêëç* -_]+)\r?$";
        private static string upnpattern = @"^(?![\x20.]+$)([^\\/\x22[\]:|<>+=;,?@]+)@([*a-z][a-z0-9.-]+)$";

        /// <summary>
        /// Parse method implementation
        /// </summary>
        public static ActiveDirectoryInspectValues Parse(string pattern)
        {
            ActiveDirectoryInspectValues ret = new ActiveDirectoryInspectValues();
            Regex rg1 = new Regex(upnpattern);
            if (rg1.IsMatch(pattern))
            {
                string[] gp = rg1.Split(pattern);
                ret.UserNamePart = gp[1];
                ret.DomainPart = gp[2];
                ret.Mode = (ActiveDirectoryUserSearchMode.UserPrincipalName);
                ret.Pattern = pattern;
                ret.Tagged = true;
            }
            else
            {
                Regex rg2 = new Regex(domainpattern);
                if (rg2.IsMatch(pattern))
                {
                    string[] gp = rg2.Split(pattern);
                    ret.DomainPart = gp[1];
                    ret.UserNamePart = gp[2];
                   // ret.Mode = (ActiveDirectoryUserSearchMode.SamAccount | ActiveDirectoryUserSearchMode.Groups);
                    ret.Mode = (ActiveDirectoryUserSearchMode.SamAccount);
                    ret.Pattern = pattern;
                    ret.Tagged = true;
                }
                else if (pattern.ToLowerInvariant().StartsWith("s-1"))
                {
                    try
                    {
                        string nt = GetNTNameFromSIDString(pattern);
                        string[] sp = nt.Split('\\');
                        ret.DomainPart = sp[0];
                        ret.UserNamePart = sp[1];
                    }
                    catch
                    {
                        // possibly approbation relationships must not be effective in weak environmments, it sometimes may fail on GetNTNameFromSIDString
                    }
                    ret.Pattern = pattern;
                    // ret.Mode = (ActiveDirectoryUserSearchMode.SamAccount | ActiveDirectoryUserSearchMode.Groups);
                    ret.Mode = (ActiveDirectoryUserSearchMode.SID);
                    ret.Tagged = true;
                }
                else
                {
                    ret.DomainPart = null;
                    ret.UserNamePart = pattern;
                    ret.Pattern = pattern;
                    ret.Mode = ActiveDirectoryUserSearchMode.AllOptions;
                    ret.Tagged = false;
                }
            }
            return ret;
        }

        /// <summary>
        /// GetDomainNamePart method implementation
        /// </summary>
        public static string GetDomainNamePart(ActiveDirectoryInspectValues inspect)
        {
            if (inspect.Mode.Equals((ActiveDirectoryUserSearchMode.UserPrincipalName)))
            {
                if (inspect.DomainPart.StartsWith("*"))
                    return inspect.DomainPart.Substring(2);
            }
            return inspect.DomainPart;
        }

        /// <summary>
        /// GetNTNameFromSIDString method implementation
        /// </summary>
        private static string GetNTNameFromSIDString(string value)
        {
            System.Security.Principal.SecurityIdentifier ss = new System.Security.Principal.SecurityIdentifier(value);
            System.Security.Principal.IdentityReference uu = ss.Translate(typeof(System.Security.Principal.NTAccount));
            return uu.Value;
        }
    }
    #endregion

    #region ActiveDirectoryForestLoadState
    public class ActiveDirectoryForestLoadState : IForestLoadState
    {
        private string _forestname;
        private List<ITopLevelName> _toplevelnames;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActiveDirectoryForestLoadState(string forestname, List<ITopLevelName> toplevelnames)
        {
            _forestname = forestname;
            _toplevelnames = toplevelnames;
        }

        public void Initialize(string forestname, List<ITopLevelName> toplevelnames)
        {
            _forestname = forestname;
            _toplevelnames = toplevelnames;
        }

        /// <summary>
        /// ForestName property implementation
        /// </summary>
        public string ForestName
        {
            get { return _forestname; }
        }

        /// <summary>
        /// TopLevelNames property implementation
        /// </summary>
        public List<ITopLevelName> TopLevelNames
        {
            get { return _toplevelnames; }
        }
    }
    #endregion

    #region ActiveDirectoryBadDomain
    public class ActiveDirectoryBadDomain: IBadDomain
    {
        private string _dnsname;
        private string _message;
        private TimeSpan _elapsedtime;

        /// <summary>
        /// Constructor
        /// </summary>
        internal ActiveDirectoryBadDomain()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActiveDirectoryBadDomain(string dnsname, string message, TimeSpan elapsedtime)
        {
            _dnsname = dnsname;
            _message = message;
            _elapsedtime = elapsedtime;
        }

        public void Initialize(string dnsname, string message, TimeSpan elapsedtime)
        {
            _dnsname = dnsname;
            _message = message;
            _elapsedtime = elapsedtime;
        }

        /// <summary>
        /// DnsName property implementation
        /// </summary>
        public string DnsName
        {
            get { return _dnsname; }
            internal set { _dnsname = value; }
        }

        /// <summary>
        /// Message property implmentation
        /// </summary>
        public string Message
        {
            get { return _message; }
            internal set { _message = value; }
        }

        /// <summary>
        /// ElapsedTime property implementation
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get { return _elapsedtime; }
            internal set { _elapsedtime = value; }
        }
    }
    #endregion

    #region ActiveDirectoryDomainConfigs
    public class ActiveDirectoryDomainConfigurations : IDomainConfig
    {
        private string _username;
        private string _password;
        private string _displayname;
        private string _domainname;
        private short _timeout;
        private bool _enabled;
        private bool _secure;
        private int _maxrows;
        private int _position;
        private string _connectstring;

        /// <summary>
        /// Constructor
        /// </summary>
        internal ActiveDirectoryDomainConfigurations()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
         public ActiveDirectoryDomainConfigurations(string domainname, string displayname, string username, string password, short timeout, bool enabled, bool secure, int maxrows, int position, string connectstring)
         {
             _username = username;
             _password = password;
             _domainname = domainname;
             _displayname = displayname;
             _enabled = enabled;
             _timeout = timeout;
             _secure = secure;
             _maxrows = maxrows;
             _position = position;
             _connectstring = connectstring;
         } 

        /// <summary>
        /// Constructor
        /// </summary>
         public void Initialize(string domainname, string displayname, string username, string password, short timeout, bool enabled, bool secure, int maxrows, int position, string connectstring)
        {
            _username = username;
            _password = password;
            _domainname = domainname;
            _displayname = displayname;
            _enabled = enabled;
            _timeout = timeout;
            _secure = secure;
            _maxrows = maxrows;
            _position = position;
            _connectstring = connectstring;
        }

        /// <summary>
        /// DomainName property implementation
        /// </summary>
        public string DomainName
        {
            get { return _domainname; }
            internal set { _domainname = value;  }
        }

        /// <summary>
        /// DisplayName property implementation
        /// </summary>
        public string DisplayName
        {
            get { return _displayname; }
            internal set { _displayname = value; }
        }

        /// <summary>
        /// UserName property implementation
        /// </summary>
        public string UserName
        {
            get { return _username; }
            internal set { _username = value; }
        }

        /// <summary>
        /// Password property implementation
        /// </summary>
        public string Password
        {
            get { return _password; }
            internal set { _password = value; }
        }

        /// <summary>
        /// Timeout property implementation
        /// </summary>
        public short Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        /// <summary>
        /// Enabled property implementation
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// SecureConnection property implementation
        /// </summary>
        public bool SecureConnection
        {
            get { return _secure; }
            set { _secure = value; }
        }

        /// <summary>
        /// MaxRows property implementation
        /// </summary>
        public int MaxRows
        {
            get { return _maxrows; }
            set { _maxrows = value; }
        }

        /// <summary>
        /// Position property implementation
        /// </summary>
        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary>
        /// ConnectString property implementation
        /// </summary>
        public string ConnectString
        {
            get { return _connectstring; }
            internal set { _connectstring = value; }
        }

    }
    #endregion

    #region ActiveDirectoryFillSearchLoadState
    public class ActiveDirectoryFillSearchLoadState: IFillSearchLoadState
    {
        public IDomain Domain; 
        public IResults Node;
        public string Pattern; 
        public bool Recursive;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActiveDirectoryFillSearchLoadState(IDomain domain, IResults lst, string pattern, bool recursive)
        {
            Domain = domain;
            Node = lst;
            Pattern = pattern; 
            Recursive = recursive;
        } 

        /// <summary>
        /// Constructor
        /// </summary>
        public void Initialize(IDomain domain, IResults lst, string pattern, bool recursive)
        {
            Domain = domain;
            Node = lst;
            Pattern = pattern;
            Recursive = recursive;
        }
    }
    #endregion

    #region ActiveDirectoryTopLevelName
    public class ActiveDirectoryTopLevelName: ITopLevelName
    {
        private string _toplevelname;
        private SharePoint.IdentityService.Core.TopLevelNameStatus _status;

        /// <summary>
        /// Constructor
        /// </summary>
        internal ActiveDirectoryTopLevelName()
        {
        }

        /// <summary>
        /// ActiveDirectoryTopLevelName constructor
        /// </summary>
        public ActiveDirectoryTopLevelName(string name, SharePoint.IdentityService.Core.TopLevelNameStatus status)
        {
            _status = status;
            _toplevelname = name;
        }

        /// <summary>
        /// ActiveDirectoryTopLevelName constructor
        /// </summary>
        public void Initialize(string name, SharePoint.IdentityService.Core.TopLevelNameStatus status)
        {
            _status = status;
            _toplevelname = name;
        }

        /// <summary>
        /// TopLevelName property implementation
        /// </summary>
        public string TopLevelName
        {
            get { return _toplevelname; }
            internal set { _toplevelname = value; }
        }

        /// <summary>
        /// Status property implementation
        /// </summary>
        public SharePoint.IdentityService.Core.TopLevelNameStatus Status
        {
            get { return _status; }
            internal set { _status = value; }
        }
    }
    #endregion
    #endregion

    #region ActiveDirectoryWrapper class
    public class ActiveDirectoryWrapper : IWrapper, IWrapperCaching
    {
        private IForests _wrp = null;

        /// <summary>
        /// ActiveDirectoryWrapper constructor
        /// </summary>
        public ActiveDirectoryWrapper()
        {
            _wrp = new ActiveDirectoryForests();
        }

        /// <summary>
        /// Initialize method override
        /// </summary>
        public void EnsureLoaded()
        {
            _wrp.EnsureLoaded();
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(List<ProxyFullConfiguration> domainscfg, List<ProxyGeneralParameter> paramscfg)
        {
            _wrp.Initialize(domainscfg, paramscfg);
        }

        /// <summary>
        /// Reload method implementation
        /// </summary>
        public void Reload()
        {
            try
            {
                _wrp.Reload();
            }
            finally
            {
                _wrp.EnsureLoaded();
            }

        }

        /// <summary>
        /// Save method implementation
        /// </summary>
        public XmlDocument Save()
        {
            return _wrp.Save();
        }

        /// <summary>
        /// Restore method implementation
        /// </summary>
        public void Restore(XmlDocument data)
        {
            IForests forest = _wrp.Restore(data);
            if (forest != null)
            {
                _wrp = forest;
            }
        }

        /// <summary>
        /// IsLoadedFromCache property implementation
        /// </summary>
        public bool IsLoadedFromCache 
        { 
            get
            {
                return _wrp.IsLoadedFromCache;
            } 
        }

        /// <summary>
        /// SavedTime property implementation
        /// </summary>
        public DateTime SavedTime
        {
            get
            {
                return _wrp.SavedTime;
            }
        }

        /// <summary>
        /// LaunchStartCommand method implementation
        /// </summary>
        public void LaunchStartCommand()
        {
            try
            {
                _wrp.EnsureLoaded();
            }
            catch
            {
            }
        }

        /// <summary>
        /// ClaimsProviderName property implementation
        /// </summary>
        public string ClaimsProviderName
        {
            get { return _wrp.ProviderName; }
            set { _wrp.ProviderName = value; }
        }

        /// <summary>
        /// FillBadDomains method implementation
        /// </summary>
        public List<ProxyBadDomain> FillBadDomains()
        {
            List<ProxyBadDomain> bad = new List<ProxyBadDomain>();
            foreach (IBadDomain b in _wrp.BadDomains)
            {
                ProxyBadDomain d = new ProxyBadDomain();
                d.DnsName = b.DnsName;
                d.ElapsedTime = b.ElapsedTime;
                d.Message = b.Message;
                bad.Add(d);
            }
            return bad;
        }

        #region FillSearch
        /// <summary>
        /// FillSearch method implementation
        /// </summary>
        public ProxyResults FillSearch(string pattern, string domain, bool recursive)
        {
            if (string.IsNullOrEmpty(pattern))
                return null;
            EnsureLoaded();
            ProxyResults results = null;
            IResults lst = new ActiveDirectoryResultsRoot();
            _wrp.FillSearch(lst, pattern, domain, recursive);
            if (lst.HasResults())
            {
                results = new ProxyResults();
                results.HasResults = true;
                DoFillSearch(lst, results, true);
            }
            return results;
        }

        /// <summary>
        /// DoFillSearch method implementation
        /// </summary>
        private void DoFillSearch(IResults lst, ProxyResults results, bool isroot)
        {
            foreach (IResultObject usr in lst.GetResults())
            {
                if (!usr.IsBuiltIn)
                {
                    if (usr is IUser)
                    {
                        ProxyUser u = new ProxyUser();
                        u.DisplayName = ((IUser)usr).DisplayName;
                        u.DomainDisplayName = ((IUser)usr).DomainDisplayName;
                        u.DomainName = ((IUser)usr).DomainName;
                        u.EmailAddress = ((IUser)usr).EmailAddress;
                        u.IsBuiltIn = ((IUser)usr).IsBuiltIn;
                        u.SamAaccount = ((IUser)usr).SamAaccount;
                        u.UserPrincipalName = ((IUser)usr).UserPrincipalName;
                        u.JobTitle = ((IUser)usr).JobTitle;
                        u.Department = ((IUser)usr).Department;
                        u.Location = ((IUser)usr).Location;
                        u.MobilePhone = ((IUser)usr).MobilePhone;
                        u.SIPAddress = ((IUser)usr).SIPAddress;
                        u.WorkPhone = ((IUser)usr).WorkPhone;
                        results.Results.Add(u);
                    }
                    else
                    {
                        ProxyRole u = new ProxyRole();
                        u.DisplayName = ((IRole)usr).DisplayName;
                        u.DomainDisplayName = ((IRole)usr).DomainDisplayName;
                        u.DomainName = ((IRole)usr).DomainName;
                        u.GroupScope = Convert.ToInt32(((IRole)usr).GroupScope);
                        u.IsBuiltIn = ((IRole)usr).IsBuiltIn;
                        u.SamAaccount = ((IRole)usr).SamAaccount;
                        u.GUID = ((IRole)usr).GUID;
                        u.SID = ((IRole)usr).SID;
                        u.IsSecurityGroup = ((IRole)usr).IsSecurityGroup;
                        results.Results.Add(u);
                    }
                }
            }
            foreach (IResultsNode node in lst.GetNodes())
            {
                ProxyResultsNode nd = new ProxyResultsNode();
                nd.Name = node.GetName();
                nd.DisplayName = node.GetDisplayName();
                nd.Position = node.GetPosition();
                nd.HasResults = node.HasResults();
                results.Nodes.Add(nd);
                DoFillSearch(node, nd, false);
            }
            if ((isroot) && (this._wrp.GlobalParams.ShowSystemNodes))
            {
                IResultsNode node = new ActiveDirectoryResultsNode("System", "System", 9999);
                MigrateBuiltInObjects(lst, node);
                if (node.HasResults())
                {
                    ProxyResultsNode nd = new ProxyResultsNode();
                    nd.Name = node.GetName();
                    nd.DisplayName = node.GetDisplayName();
                    nd.Position = node.GetPosition();
                    nd.HasResults = node.HasResults();
                    results.Nodes.Add(nd);
                    foreach (IResultObject usr in node.GetResults())
                    {
                        if (usr.IsBuiltIn)
                        {
                            if (usr is IUser)
                            {
                                ProxyUser u = new ProxyUser();
                                u.DisplayName = ((IUser)usr).DisplayName;
                                u.DomainDisplayName = "System";
                                u.DomainName = ((IUser)usr).DomainName;
                                u.EmailAddress = ((IUser)usr).EmailAddress;
                                u.IsBuiltIn = ((IUser)usr).IsBuiltIn;
                                u.SamAaccount = ((IUser)usr).SamAaccount;
                                u.UserPrincipalName = ((IUser)usr).UserPrincipalName;
                                u.JobTitle = ((IUser)usr).JobTitle;
                                u.Department = ((IUser)usr).Department;
                                u.Location = ((IUser)usr).Location;
                                u.MobilePhone = ((IUser)usr).MobilePhone;
                                u.SIPAddress = ((IUser)usr).SIPAddress;
                                u.WorkPhone = ((IUser)usr).WorkPhone;
                                nd.Results.Add(u);
                            }
                            else
                            {
                                ProxyRole u = new ProxyRole();
                                u.DisplayName = ((IRole)usr).DisplayName;
                                u.DomainDisplayName = "System";
                                u.DomainName = ((IRole)usr).DomainName;
                                u.GroupScope = Convert.ToInt32(((IRole)usr).GroupScope);
                                u.IsBuiltIn = ((IRole)usr).IsBuiltIn;
                                u.SamAaccount = ((IRole)usr).SamAaccount;
                                u.GUID = ((IRole)usr).GUID;
                                u.SID = ((IRole)usr).SID;
                                u.IsSecurityGroup = ((IRole)usr).IsSecurityGroup;
                                nd.Results.Add(u);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region FillResolve
        /// <summary>
        /// FillResolve method implementation
        /// </summary>
        public ProxyResults FillResolve(string pattern, bool recursive)
        {
            if (string.IsNullOrEmpty(pattern))
                return null;
            EnsureLoaded();
            ProxyResults results = null;
            IResults lst = new ActiveDirectoryResultsRoot();
           // _wrp.FillSearch(lst, pattern, null, recursive);
            _wrp.FillResolve(lst, pattern, true);
            if (lst.HasResults())
            {
                results = new ProxyResults();
                results.HasResults = true;
                DoFillResolve(lst, results, true);
            }
            return results;
        }

        /// <summary>
        /// DoFillResolve method implementation
        /// </summary>
        private void DoFillResolve(IResults lst, ProxyResults results, bool isroot)
        {
            foreach (IResultObject usr in lst.GetResults())
            {
                if (!usr.IsBuiltIn)
                {
                    if (usr is IUser)
                    {
                        ProxyUser u = new ProxyUser();
                        u.DisplayName = ((IUser)usr).DisplayName;
                        u.DomainDisplayName = ((IUser)usr).DomainDisplayName;
                        u.DomainName = ((IUser)usr).DomainName;
                        u.EmailAddress = ((IUser)usr).EmailAddress;
                        u.IsBuiltIn = ((IUser)usr).IsBuiltIn;
                        u.SamAaccount = ((IUser)usr).SamAaccount;
                        u.UserPrincipalName = ((IUser)usr).UserPrincipalName;
                        u.JobTitle = ((IUser)usr).JobTitle;
                        u.Department = ((IUser)usr).Department;
                        u.Location = ((IUser)usr).Location;
                        u.MobilePhone = ((IUser)usr).MobilePhone;
                        u.SIPAddress = ((IUser)usr).SIPAddress;
                        u.WorkPhone = ((IUser)usr).WorkPhone;
                        results.Results.Add(u);
                    }
                    else
                    {
                        ProxyRole u = new ProxyRole();
                        u.DisplayName = ((IRole)usr).DisplayName;
                        u.DomainDisplayName = ((IRole)usr).DomainDisplayName;
                        u.DomainName = ((IRole)usr).DomainName;
                        u.GroupScope = Convert.ToInt32(((IRole)usr).GroupScope);
                        u.IsBuiltIn = ((IRole)usr).IsBuiltIn;
                        u.SamAaccount = ((IRole)usr).SamAaccount;
                        u.GUID = ((IRole)usr).GUID;
                        u.SID = ((IRole)usr).SID;
                        u.IsSecurityGroup = ((IRole)usr).IsSecurityGroup;
                        results.Results.Add(u);
                    }
                }
            }
           /* foreach (IResultsNode node in lst.GetNodes())
            {
                ProxyResultsNode nd = new ProxyResultsNode();
                nd.Name = node.GetName();
                nd.DisplayName = node.GetDisplayName();
                nd.Position = node.GetPosition();
                nd.HasResults = node.HasResults();
                results.Nodes.Add(nd);
                DoFillResolve(node, nd, false);
            } */
            foreach (IResultsNode node in lst.GetNodes())
            {
                DoFillResolve(node, results, false);
            }

            if ((isroot) && (this._wrp.GlobalParams.ShowSystemNodes))
            {
                IResultsNode node = new ActiveDirectoryResultsNode("System", "System", 9999);
                MigrateBuiltInObjects(lst, node);
                if (node.HasResults())
                {
                    ProxyResultsNode nd = new ProxyResultsNode();
                    nd.Name = node.GetName();
                    nd.DisplayName = node.GetDisplayName();
                    nd.Position = node.GetPosition();
                    nd.HasResults = node.HasResults();
                    results.Nodes.Add(nd);
                    foreach (IResultObject usr in node.GetResults())
                    {
                        if (usr.IsBuiltIn)
                        {
                            if (usr is IUser)
                            {
                                ProxyUser u = new ProxyUser();
                                u.DisplayName = ((IUser)usr).DisplayName;
                                u.DomainDisplayName = "System";
                                u.DomainName = ((IUser)usr).DomainName;
                                u.EmailAddress = ((IUser)usr).EmailAddress;
                                u.IsBuiltIn = ((IUser)usr).IsBuiltIn;
                                u.SamAaccount = ((IUser)usr).SamAaccount;
                                u.UserPrincipalName = ((IUser)usr).UserPrincipalName;
                                u.JobTitle = ((IUser)usr).JobTitle;
                                u.Department = ((IUser)usr).Department;
                                u.Location = ((IUser)usr).Location;
                                u.MobilePhone = ((IUser)usr).MobilePhone;
                                u.SIPAddress = ((IUser)usr).SIPAddress;
                                u.WorkPhone = ((IUser)usr).WorkPhone;
                                nd.Results.Add(u);
                            }
                            else
                            {
                                ProxyRole u = new ProxyRole();
                                u.DisplayName = ((IRole)usr).DisplayName;
                                u.DomainDisplayName = "System";
                                u.DomainName = ((IRole)usr).DomainName;
                                u.GroupScope = Convert.ToInt32(((IRole)usr).GroupScope);
                                u.IsBuiltIn = ((IRole)usr).IsBuiltIn;
                                u.SamAaccount = ((IRole)usr).SamAaccount;
                                u.GUID = ((IRole)usr).GUID;
                                u.SID = ((IRole)usr).SID;
                                u.IsSecurityGroup = ((IRole)usr).IsSecurityGroup;
                                nd.Results.Add(u);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region FillValidate
        /// <summary>
        /// FillValidate method implementation
        /// </summary>
        public ProxyResults FillValidate(string pattern, bool recursive)
        {
            EnsureLoaded();
            ProxyResults results = null;
            IResults lst = new ActiveDirectoryResultsRoot();
            _wrp.FillValidate(lst, pattern, recursive);
            if (lst.HasResults())
            {
                results = new ProxyResults();
                results.HasResults = true;
                DoFillValidate(lst, results, true);
            }
            return results;
        }

        /// <summary>
        /// DoFillValidate method implementation
        /// </summary>
        private void DoFillValidate(IResults lst, ProxyResults resolved, bool isroot)
        {
            foreach (IResultObject usr in lst.GetResults())
            {
                if (!usr.IsBuiltIn)
                {
                    if (usr is IUser)
                    {
                        ProxyUser u = new ProxyUser();
                        u.DisplayName = ((IUser)usr).DisplayName;
                        u.DomainDisplayName = ((IUser)usr).DomainDisplayName;
                        u.DomainName = ((IUser)usr).DomainName;
                        u.EmailAddress = ((IUser)usr).EmailAddress;
                        u.IsBuiltIn = ((IUser)usr).IsBuiltIn;
                        u.SamAaccount = ((IUser)usr).SamAaccount;
                        u.UserPrincipalName = ((IUser)usr).UserPrincipalName;
                        resolved.Results.Add(u);
                    }
                    else
                    {
                        ProxyRole u = new ProxyRole();
                        u.DisplayName = ((IRole)usr).DisplayName;
                        u.DomainDisplayName = ((IRole)usr).DomainDisplayName;
                        u.DomainName = ((IRole)usr).DomainName;
                        u.GroupScope = Convert.ToInt32(((IRole)usr).GroupScope);
                        u.IsBuiltIn = ((IRole)usr).IsBuiltIn;
                        u.SamAaccount = ((IRole)usr).SamAaccount;
                        u.GUID = ((IRole)usr).GUID;
                        u.SID = ((IRole)usr).SID;
                        u.IsSecurityGroup = ((IRole)usr).IsSecurityGroup;
                        resolved.Results.Add(u);
                    }
                }
            }
            foreach (IResultsNode node in lst.GetNodes())
            {
                DoFillValidate(node, resolved, false);
            }
            if ((isroot) && (this._wrp.GlobalParams.ShowSystemNodes))
            {
                IResultsNode node = new ActiveDirectoryResultsNode("System", "System", 9999);
                MigrateBuiltInObjects(lst, node);
                if (node.HasResults())
                {
                    foreach (IResultObject usr in node.GetResults())
                    {
                        if (usr.IsBuiltIn)
                        {
                            if (usr is IUser)
                            {
                                ProxyUser u = new ProxyUser();
                                u.DisplayName = ((IUser)usr).DisplayName;
                                u.DomainDisplayName = "System";
                                u.DomainName = ((IUser)usr).DomainName;
                                u.EmailAddress = ((IUser)usr).EmailAddress;
                                u.IsBuiltIn = ((IUser)usr).IsBuiltIn;
                                u.SamAaccount = ((IUser)usr).SamAaccount;
                                u.UserPrincipalName = ((IUser)usr).UserPrincipalName;
                                u.JobTitle = ((IUser)usr).JobTitle;
                                u.Department = ((IUser)usr).Department;
                                u.Location = ((IUser)usr).Location;
                                u.MobilePhone = ((IUser)usr).MobilePhone;
                                u.SIPAddress = ((IUser)usr).SIPAddress;
                                u.WorkPhone = ((IUser)usr).WorkPhone;
                                resolved.Results.Add(u);
                            }
                            else
                            {
                                ProxyRole u = new ProxyRole();
                                u.DisplayName = ((IRole)usr).DisplayName;
                                u.DomainDisplayName = "System";
                                u.DomainName = ((IRole)usr).DomainName;
                                u.GroupScope = Convert.ToInt32(((IRole)usr).GroupScope);
                                u.IsBuiltIn = ((IRole)usr).IsBuiltIn;
                                u.SamAaccount = ((IRole)usr).SamAaccount;
                                u.GUID = ((IRole)usr).GUID;
                                u.SID = ((IRole)usr).SID;
                                u.IsSecurityGroup = ((IRole)usr).IsSecurityGroup;
                                resolved.Results.Add(u);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// MigrateBuiltInObjects method implementation
        /// </summary>
        private void MigrateBuiltInObjects(IResults lst, IResultsNode systemnode)
        {
            foreach (IResultsNode node in lst.GetNodes())
            {
                foreach (IResultObject usr in node.GetResults())
                {
                    if (usr.IsBuiltIn)
                    {
                        systemnode.AddResultIfNotExists(usr);
                    }
                }
            }
        }

        /// <summary>
        /// FillHierarchy method imlementation
        /// </summary>
        public ProxyDomain FillHierarchy(string hierarchyNodeID, int numberOfLevels)
        {
            ProxyDomain results = new ProxyDomain();
                results.ElapsedTime = _wrp.ElapsedTime;
                results.IsReacheable = true;
                results.IsRoot = true;
                results.DnsName = "Root";
                results.DisplayName = "Root";
            if (string.IsNullOrEmpty(hierarchyNodeID))
            {
                foreach (IDomain d in _wrp.RootDomains)
                {
                    DoFillHierachy(results, d, 1, numberOfLevels);
                }
            }
            else
            {
                List<IDomain> dom = _wrp.GetDomain(hierarchyNodeID);
                if (dom != null)
                {
                    foreach (IDomain dm in dom)
                    {
                        foreach (IDomain d in dm.Domains)
                        {
                            DoFillHierachy(results, d, 1, numberOfLevels);
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// DoFillHierachy method implementation
        /// </summary>
        private void DoFillHierachy(ProxyDomain dom, IDomain idom, int countLevels, int numberOfLevels)
        {
            if (countLevels > numberOfLevels)
                return;
            ProxyDomain temp = new ProxyDomain();
            temp.ElapsedTime = idom.ElapsedTime;
            temp.IsReacheable = idom.IsReacheable;
            temp.IsRoot = idom.IsRoot;
            temp.DnsName = idom.DnsName;
            temp.DisplayName = idom.DisplayName;
            temp.Position = idom.Position;
            dom.Domains.Add(temp);
            foreach (IDomain d in idom.Domains)
            {
                DoFillHierachy(temp, d, countLevels + 1, numberOfLevels);
            }
        }

        /// <summary>
        /// Log method implementation
        /// </summary>
        public void Log(Exception ex, string message, EventLogEntryType eventLogEntryType, int eventid = 0)
        {
            LogEvent.Log(ex, message, eventLogEntryType, eventid);
        }

        /// <summary>
        /// Trace method implementation
        /// </summary>
        public void Trace(string message, EventLogEntryType eventLogEntryType, int eventid = 0)
        {
            LogEvent.Trace(message, eventLogEntryType, eventid);
        }
    }
    #endregion

    #region ActiveDirectoryGlobalParams class
    public class ActiveDirectoryGlobalParams: IGlobalParams
    {
        private ProxySmoothRequest _smoothrequestor;
        private ProxyClaimsMode _claimsmode;
        private ProxyClaimsDisplayMode _claimdisplaymode;
        private ProxyClaimsDisplayMode _peoplepickerdisplaymode;
        private bool _searchbymail;
        private bool _searchbydisplayname;
        private bool _traceresolve;
        private bool _peoplepickerimages;
        private bool _showsystemnodes;

        /// <summary>
        /// SmoothRequestor property implementation
        /// </summary>
        public ProxySmoothRequest SmoothRequestor
        {
            get { return _smoothrequestor; }
            set { _smoothrequestor = value; }
        }
      
        /// <summary>
        /// ClaimsMode property implementation
        /// </summary>
        public ProxyClaimsMode ClaimsMode
        {
            get { return _claimsmode; }
            set { _claimsmode = value; }
        }
        
        /// <summary>
        /// ClaimsDisplayMode  property implemtation
        /// </summary>
        public ProxyClaimsDisplayMode ClaimsDisplayMode
        {
            get { return _claimdisplaymode; }
            set { _claimdisplaymode = value; }
        }
        
        /// <summary>
        /// PeoplePickerDisplayMode property implemtation
        /// </summary>
        public ProxyClaimsDisplayMode PeoplePickerDisplayMode
        {
            get { return _peoplepickerdisplaymode; }
            set { _peoplepickerdisplaymode = value; }
        }
        
        /// <summary>
        /// SearchByMail property implementation
        /// </summary>
        public bool SearchByMail
        {
            get { return _searchbymail; }
            set { _searchbymail = value; }
        }
        
        /// <summary>
        /// SearchByDisplayName property implementation 
        /// </summary>
        public bool SearchByDisplayName
        {
            get { return _searchbydisplayname; }
            set { _searchbydisplayname = value; }
        }

        /// <summary>
        /// SearchByDisplayName property implementation 
        /// </summary>
        public bool Trace
        {
            get { return _traceresolve; }
            set { _traceresolve = value; }
        }

        /// <summary>
        /// PeoplePickerImages property implementation
        /// </summary>
        public bool PeoplePickerImages
        {
            get { return _peoplepickerimages; }
            set { _peoplepickerimages = value; }
        }

        /// <summary>
        /// ShowSystemNodes property implementation
        /// </summary>
        public bool ShowSystemNodes
        {
            get { return _showsystemnodes; }
            set { _showsystemnodes = value; }
        }

    }
    #endregion

    #region ActiveDirectoryForests class
    public class ActiveDirectoryForests: IForests
    {
        private ManualResetEvent manual = new ManualResetEvent(false);
        private static int countforest = 0;
        private static int countfillsearch = 0;
        private static object lokobj = new Object();

        private string _aduser;
        private string _adpwd;
        private short _defaulttimeout = 30;
        private short _defaultsuspend = 1;
        private bool _usesecureconnection = true;
        private int _maxrowsperdomain = 200;
        private TimeSpan _elapsedtime;
        private List<IRootDomain> _rootdomains;
        private List<IBadDomain> _baddomains;
        private List<IDomainConfig> _domainconfigs;
        private IGlobalParams _adprm;
        private bool _isloaded = false;
        private string  _providername;

        private DateTime _savedtime;
        private bool _isloadedfromcache;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActiveDirectoryForests()
        {
            _rootdomains = new List<IRootDomain>();
            _baddomains = new List<IBadDomain>();
            _domainconfigs = new List<IDomainConfig>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public void Initialize(List<ProxyFullConfiguration> AllData, List<ProxyGeneralParameter> allParams)
        {
            foreach (ProxyFullConfiguration f in AllData)
            {
                if (f.IsDefault)
                {
                    _aduser = f.UserName;
                    _adpwd = f.Password; 
                    _defaulttimeout = f.Timeout;
                    _usesecureconnection = f.Secure;
                    _maxrowsperdomain = f.Maxrows;
                }
                this.DomainConfigurations.Add(new ActiveDirectoryDomainConfigurations(f.DnsName, f.DisplayName, f.UserName, f.Password, f.Timeout, f.Enabled, f.Secure, f.Maxrows, f.DisplayPosition, f.ConnectString));
            }
            _adprm = new ActiveDirectoryGlobalParams();
            foreach (ProxyGeneralParameter p in allParams)
            {
                if (p.ParamName.ToLower().Trim().Equals("smoothrequestor"))
                {
                    _adprm.SmoothRequestor = (ProxySmoothRequest)Enum.Parse(typeof(ProxySmoothRequest), p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("claimsmode"))
                {
                    _adprm.ClaimsMode = (ProxyClaimsMode)Enum.Parse(typeof(ProxyClaimsMode), p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("claimsdisplaymode"))
                {
                    _adprm.ClaimsDisplayMode = (ProxyClaimsDisplayMode)Enum.Parse(typeof(ProxyClaimsDisplayMode), p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("peoplepickerdisplaymode"))
                {
                    _adprm.PeoplePickerDisplayMode = (ProxyClaimsDisplayMode)Enum.Parse(typeof(ProxyClaimsDisplayMode), p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("searchbymail"))
                {
                    _adprm.SearchByMail = bool.Parse(p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("searchbydisplayname"))
                {
                    _adprm.SearchByDisplayName = bool.Parse(p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("traceresove"))
                {
                    _adprm.Trace = bool.Parse(p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("peoplepickerimages"))
                {
                    _adprm.PeoplePickerImages = bool.Parse(p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("showsystemnodes"))
                {
                    _adprm.ShowSystemNodes = bool.Parse(p.ParamValue);
                }
            }
        }

        /// <summary>
        /// EnsureLoaded method implementation
        /// </summary>
        public void EnsureLoaded()
        {
            if (!_isloaded)
            {
                lock (this)
                {
                    LoadDomains();
                }
            }
        }

        /// <summary>
        /// Reload method Implementation
        /// </summary>
        public void Reload()
        {
            DateTime db = DateTime.Now;
            LogEvent.Trace(string.Format(ResourcesValues.GetString("E1900"), this.ProviderName), EventLogEntryType.Information, 1900);

            lock (this)
            {
                _isloaded = false;
                LoadDomains();
            }
            TimeSpan _e = DateTime.Now.Subtract(db);
            LogEvent.Trace(string.Format(ResourcesValues.GetString("E1900B"), this.ProviderName, _e.Minutes, _e.Seconds, _e.Milliseconds), EventLogEntryType.Information, 1900);
        }

        /// <summary>
        /// IsLoaded property implementation
        /// </summary>
        public bool IsLoaded
        {
            get { return _isloaded; }
            internal set { _isloaded = value; }
        }

        /// <summary>
        /// ProviderName property implementation
        /// </summary>
        public string ProviderName
        {
          get { return _providername; }
          set { _providername = value; }
        }

        #region Cache management

        /// <summary>
        /// SaveMethod implementation
        /// </summary>
        public XmlDocument Save()
        {
            XmlDocument doc = new XmlDocument();
            PersistedForests root = this;
            Stream memstm = new MemoryStream();
            DataContractSerializer dcs = new DataContractSerializer(typeof(PersistedForests));
            dcs.WriteObject(memstm, root);
            memstm.Position = 0;
            doc.Load(memstm);
            // doc.Save(@"c:\temp\" + this.ProviderName + ".xml");
            _savedtime = DateTime.Now;
            return doc;
        }

        /// <summary>
        /// Restore method implementation
        /// </summary>
        public IForests Restore(XmlDocument data)
        {
            ActiveDirectoryForests res = null;
            try
            {
                DateTime db = DateTime.Now;
                lock (this)
                {
                    PersistedForests root = null;
                    DataContractSerializer dcs = new DataContractSerializer(typeof(PersistedForests));
                    Stream memstm = new MemoryStream();
                    data.Save(memstm);
                    memstm.Position = 0;
                    root = dcs.ReadObject(memstm) as PersistedForests;
                    res = root;
                    _isloadedfromcache = true;
                    _isloaded = true;
                }
                LogEvent.Trace(string.Format(ResourcesValues.GetString("E1901"), res.ProviderName), EventLogEntryType.Information, 1901);
                TimeSpan _e = DateTime.Now.Subtract(db);
                LogEvent.Trace(string.Format(ResourcesValues.GetString("E1901B"), res.ProviderName, _e.Minutes, _e.Seconds, _e.Milliseconds), EventLogEntryType.Information, 1901);
            }
            catch (Exception E)
            {
                _isloaded = false;
                _isloadedfromcache = false;
                LogEvent.Log(E, ResourcesValues.GetString("E1902"), EventLogEntryType.Information, 1902);
                res = null;
            }
            return res;
        }

        /// <summary>
        /// IsLoadedFromCache property implementation
        /// </summary>
        public bool IsLoadedFromCache 
        {
            get { return _isloadedfromcache; }
            internal set { _isloadedfromcache = value; }
        }

        /// <summary>
        /// SavedTime property implementation
        /// </summary>
        public DateTime SavedTime 
        {
            get { return _savedtime; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// RootDomains property implementation
        /// </summary>
        public List<IRootDomain> RootDomains
        {
            get { return _rootdomains; }
        }

        /// <summary>
        /// BadDomains property implementation
        /// </summary>
        public List<IBadDomain> BadDomains
        {
            get { return _baddomains; }
        }

        /// <summary>
        /// DomainConfigs property implmentation
        /// </summary>
        public List<IDomainConfig> DomainConfigurations
        {
            get { return _domainconfigs; }
        }

        /// <summary>
        /// UserName property implementation
        /// </summary>
        public string UserName
        {
            get { return _aduser; }
            internal set { _aduser = value;} 
        }

        /// <summary>
        ///  Password property implementation
        /// </summary>
        public string Password
        {
            get { return _adpwd; }
            internal set { _adpwd = value; } 
        }

        /// <summary>
        /// DefaultTimeOut property implementation
        /// </summary>
        public short DefaultTimeOut
        {
            get { return _defaulttimeout; }
            internal set { _defaulttimeout = value; } 
        }

        /// <summary>
        /// DefaultSuspendTime property implementation
        /// </summary>
        public short DefaultSuspendTime
        {
            get { return _defaultsuspend; }
            internal set { _defaultsuspend = value; } 
        }

        /// <summary>
        /// ElapsedTime property implementation
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get { return _elapsedtime; }
            internal set { _elapsedtime = value; } 
        }

        /// <summary>
        /// UsesScureConnection property implementation
        /// </summary>
        public bool UsesScureConnection
        {
            get { return _usesecureconnection; }
            internal set { _usesecureconnection = value; } 
        }

        /// <summary>
        /// MaxRowsPerDomain property implementation
        /// </summary>
        public int MaxRowsPerDomain
        {
            get { return _maxrowsperdomain; }
            internal set { _maxrowsperdomain = value; } 
        }

        /// <summary>
        /// GlobalParams property implementation
        /// </summary>
        public IGlobalParams GlobalParams
        {
            get { return _adprm; }
            set { _adprm = value; }
        }

        #endregion

        #region Domains find methods

        /// <summary>
        /// FindScopedDomain method implementation
        /// </summary>
        private List<ActiveDirectoryDomain> FindScopedDomain(ActiveDirectoryDomain root, string scope)
        {
            if (string.IsNullOrEmpty(scope))
                return null;
            List<ActiveDirectoryDomain> lst = new List<ActiveDirectoryDomain>();
            if (root == null)
            {
                // finding scope
                foreach (ActiveDirectoryRootDomain d in this.RootDomains)
                {
                    bool found = false;
                    if (d.IsReacheable)
                    {
                        if (scope.ToLowerInvariant().Equals(d.DisplayName.ToLowerInvariant()))
                            found = true;
                        else if (scope.ToLowerInvariant().Equals(d.DnsName.ToLowerInvariant()))
                            found = true;
                        else if (scope.ToLowerInvariant().Equals(d.NetbiosName.ToLowerInvariant()))
                            found = true;
                        if (found)
                            lst.Add(d);
                        else
                        {
                            List<ActiveDirectoryDomain> ad = FindScopedDomain(d, scope);
                            if (ad!=null)
                                return ad;
                            foreach (ITopLevelName lvn in d.TopLevelNames)
                            {
                                if (lvn.TopLevelName.ToLowerInvariant().Equals(scope.ToLowerInvariant()))
                                    lst.Add(d); // Shortcut - Search in all the branch
                            }
                        }
                    }
                }
                if (lst.Count > 0)
                    return lst;
            }
            else
            {
                if (scope.ToLowerInvariant().Equals(root.DnsName.ToLowerInvariant()))
                    lst.Add(root);
                else if (scope.ToLowerInvariant().Equals(root.DisplayName.ToLowerInvariant()))
                    lst.Add(root);
                else if (scope.ToLowerInvariant().Equals(root.NetbiosName.ToLowerInvariant()))
                    lst.Add(root);
                foreach (ActiveDirectoryDomain d in root.Domains)
                {
                    bool found = false;
                    if (d.IsReacheable)
                    {
                        if (scope.ToLowerInvariant().Equals(d.DisplayName.ToLowerInvariant()))
                            found = true;
                        else if (scope.ToLowerInvariant().Equals(d.DnsName.ToLowerInvariant()))
                            found = true;
                        else if (scope.ToLowerInvariant().Equals(d.NetbiosName.ToLowerInvariant()))
                            found = true;
                        if (found)
                            lst.Add(d);
                        else
                        {
                            List<ActiveDirectoryDomain> ad = FindScopedDomain(d, scope);
                            if (ad!=null)
                                return ad;
                        }
                    }
                }
                if (lst.Count > 0)
                    return lst;
            }
            return null;
        }

        /// <summary>
        /// GetDomain method implementation
        /// </summary>
        public List<IDomain> GetDomain(string domain)
        {
            List<ActiveDirectoryDomain> dta = FindScopedDomain(null, domain);
            if (dta == null)
                return null;
            else
                return dta.ToList<IDomain>();
        }

        /// <summary>
        /// GetScopedDomain method implementation
        /// </summary>
        public List<ActiveDirectoryDomain> GetScopedDirectoryDomain(string scope, ActiveDirectoryInspectValues reg = null)
        {
            if ((reg != null) && (reg.HasDomain))
                scope = reg.DomainPart;
            List<ActiveDirectoryDomain> dta = FindScopedDomain(null, scope);
            if (dta == null)
                return null;
            else
                return dta;
        }

        /// <summary>
        /// GetScopedDomain method implementation
        /// </summary>
        public List<IDomain> GetScopedDomain(string scope, ActiveDirectoryInspectValues reg = null)
        {
            if ((reg != null) && (reg.HasDomain))
                scope = reg.DomainPart;
            List<ActiveDirectoryDomain> dta = FindScopedDomain(null, scope);
            if (dta == null)
                return null;
            else
                return dta.ToList<IDomain>();
        }

        #endregion

        #region Relationships & UPN
        /// <summary>
        /// internalGetTopLevelNames()
        /// </summary>
        private List<ITopLevelName> internalGetTopLevelNames()
        {
            List<ITopLevelName> lst = new List<ITopLevelName>();
            DirectoryEntry myRootDSE = null;
            DirectoryEntry oDomain = null;
            DirectoryEntry oPartition = null;
            try
            {
                myRootDSE = new DirectoryEntry(@"LDAP://RootDSE", _aduser, _adpwd, AuthenticationTypes.Signing | AuthenticationTypes.Sealing | AuthenticationTypes.Secure);
                myRootDSE.RefreshCache();
                string strNamingContext = myRootDSE.Properties["defaultNamingContext"].Value.ToString();
                string strConfigContext = myRootDSE.Properties["configurationNamingContext"].Value.ToString();
                oDomain = new DirectoryEntry(@"LDAP://" + strNamingContext, _aduser, _adpwd, AuthenticationTypes.Signing | AuthenticationTypes.Sealing | AuthenticationTypes.Secure);

                oPartition = new DirectoryEntry(@"LDAP://CN=Partitions," + strConfigContext, _aduser, _adpwd, AuthenticationTypes.Signing | AuthenticationTypes.Sealing | AuthenticationTypes.Secure);
                oDomain.Invoke("GetInfoEx", new object[] { "canonicalName" }, 0);

                lst.Add(new ActiveDirectoryTopLevelName(oDomain.InvokeGet("canonicalName").ToString().Replace("/", ""), Core.TopLevelNameStatus.Enabled));
                oPartition.Invoke("GetEx", new object[] { "uPNSuffixes" });
                object[] suffixes = (object[])oPartition.InvokeGet("uPNSuffixes");
                foreach (object o in suffixes)
                {
                    lst.Add(new ActiveDirectoryTopLevelName(o.ToString(), Core.TopLevelNameStatus.Enabled));
                }
            }
            catch
            {
            }
            finally
            {
                if (myRootDSE != null)
                    myRootDSE.Dispose();
                if (oDomain != null)
                    oDomain.Dispose();
                if (oPartition != null)
                    oPartition.Dispose();
            }
            return lst;
        }

        /// <summary>
        /// relationshipsGetTopLevelNames()
        /// </summary>
        private List<ITopLevelName> relationshipsGetTopLevelNames(ForestTrustRelationshipInformation inf)
        {
            List<ITopLevelName> lst = new List<ITopLevelName>();
            try
            {
                foreach (TopLevelName x in inf.TopLevelNames)
                {
                    lst.Add(new ActiveDirectoryTopLevelName(x.Name, (Core.TopLevelNameStatus)x.Status));
                }
            }
            catch
            {
            }
            return lst;
        }
        #endregion

        #region Loading of forests and domains lists
        /// <summary>
        /// LoadDomains method implementation
        /// </summary>
        private void LoadDomains()
        {
            this.RootDomains.Clear();
            DateTime db = DateTime.Now;
            bool rootit = false;
            using (Identity impersonate = Identity.Impersonate(_aduser, _adpwd))
            {
                DirectoryContext dctx = null;
                if (string.IsNullOrEmpty(_aduser) || string.IsNullOrEmpty(_adpwd))
                    dctx = new DirectoryContext(DirectoryContextType.Forest);
                else
                    dctx = new DirectoryContext(DirectoryContextType.Forest, _aduser, _adpwd);
                Forest f = Forest.GetForest(dctx);
                try
                {
                    LogEvent.Trace(string.Format(ResourcesValues.GetString("E1000"), this.ProviderName), System.Diagnostics.EventLogEntryType.Information, 1000);
                    foreach (Domain d in f.Domains)
                    {
                        if (d.Parent == null)
                        {
                            List<ActiveDirectoryDomainParam> lprm = GetDomainConfigurations(d.Name);
                            foreach (ActiveDirectoryDomainParam prm in lprm)
                            {
                                if (prm.Enabled)
                                {
                                    ActiveDirectoryRootDomain r = new ActiveDirectoryRootDomain(d, internalGetTopLevelNames(), prm, this.GlobalParams);
                                    this.RootDomains.Add(r);
                                    r.IsRoot = true;
                                    LoadChildDomainList(r, d);
                                }
                                else
                                    this.BadDomains.Add(new ActiveDirectoryBadDomain(d.Name, string.Format("This root domain {0} is administratively Disabled ", d), DateTime.Now.Subtract(db)));
                            }
                        }
                    }
                    try
                    {
                        TrustRelationshipInformationCollection t = f.GetAllTrustRelationships();
                        foreach (ForestTrustRelationshipInformation i in t)
                        {
                            List<ActiveDirectoryDomainParam> lprmx = GetDomainConfigurations(i.TargetName);
                            foreach (ActiveDirectoryDomainParam prmx in lprmx)
                            {
                                if (!prmx.Enabled)
                                {
                                    this.BadDomains.Add(new ActiveDirectoryBadDomain(i.TargetName, string.Format("This domain {0} is administratively Disabled ", i.TargetName), DateTime.Now.Subtract(db)));
                                    continue;
                                }
                                if (!rootit)
                                {
                                    rootit = true;
                                    manual.Reset();
                                }
                                try
                                {
                                    lock (lokobj)
                                    {
                                        countforest++;
                                    }
                                    ThreadPool.QueueUserWorkItem(new WaitCallback(InternalLoadForest), new ActiveDirectoryForestLoadState(i.TargetName, relationshipsGetTopLevelNames(i)));
                                }
                                catch (Exception E)
                                {
                                    LogEvent.Log(E, string.Format(ResourcesValues.GetString("E1001"), i.TargetName), System.Diagnostics.EventLogEntryType.Error, 1001);
                                }
                            }
                        }
                    }
                    catch (Exception E)
                    {
                        LogEvent.Log(E, ResourcesValues.GetString("E1501"), System.Diagnostics.EventLogEntryType.Error, 1501);
                    }
                    if (rootit)
                        manual.WaitOne();
                    _isloaded = true;
                    TimeSpan _e = DateTime.Now.Subtract(db);
                    LogEvent.Trace(string.Format(ResourcesValues.GetString("E1000B"), this.ProviderName, _e.Minutes, _e.Seconds, _e.Milliseconds), System.Diagnostics.EventLogEntryType.Information, 1000);
                }
                catch (Exception E)
                {
                    _isloaded = false;
                    LogEvent.Log(E, ResourcesValues.GetString("E1500"), System.Diagnostics.EventLogEntryType.Error, 1500);
                }
                finally
                {
                    f.Dispose();
                }
                _elapsedtime = DateTime.Now.Subtract(db);
                _isloadedfromcache = false;
            }
        }

        /// <summary>
        /// InternalLoadForest method implementation
        /// </summary>
        private void InternalLoadForest(object objectstate)
        {
            ActiveDirectoryForestLoadState data = (ActiveDirectoryForestLoadState)objectstate;
            DirectoryContext dctx = null;
            if (string.IsNullOrEmpty(_aduser) || string.IsNullOrEmpty(_adpwd))
                dctx = new DirectoryContext(DirectoryContextType.Forest, data.ForestName);
            else
                dctx = new DirectoryContext(DirectoryContextType.Forest, data.ForestName, _aduser, _adpwd);
            try
            {
                DateTime db = DateTime.Now;
                Forest ff = null;
                try
                {
                    ff = Forest.GetForest(dctx);
                }
                catch (Exception E)
                {
                    this.BadDomains.Add(new ActiveDirectoryBadDomain(data.ForestName, E.Message, DateTime.Now.Subtract(db)));
                    throw E;
                }
                foreach (Domain d in ff.Domains)
                {
                    if (d.Parent == null)
                    {
                        List<ActiveDirectoryDomainParam> lprm = GetDomainConfigurations(d.Name);
                        foreach (ActiveDirectoryDomainParam prm in lprm)
                        {
                            if (prm.Enabled)
                            {
                                ActiveDirectoryRootDomain r = null;
                                try
                                {

                                    r = new ActiveDirectoryRootDomain(d, data.TopLevelNames, prm, this.GlobalParams);
                                    this.RootDomains.Add(r);
                                    r.IsRoot = true;
                                    LoadChildDomainList(r, d);
                                }
                                catch (Exception E)
                                {
                                    this.BadDomains.Add(new ActiveDirectoryBadDomain(d.Name, E.Message, DateTime.Now.Subtract(db)));
                                    throw E;
                                }
                            }
                            else
                                this.BadDomains.Add(new ActiveDirectoryBadDomain(d.Name, string.Format("This domain {0} is administratively Disabled ", d.Name), DateTime.Now.Subtract(db)));
                        }
                    }
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E1200"), data.ForestName), System.Diagnostics.EventLogEntryType.Error, 1200);
            }
            finally
            {
                lock (lokobj)
                {
                    countforest--;
                    if (countforest == 0)
                    {
                        manual.Set();
                    }
                }
            }
        }

        /// <summary>
        /// LoadChildDomainList method implementation
        /// </summary>
        private void LoadChildDomainList(ActiveDirectoryDomain ad, Domain a)
        {
            if (!ad.IsReacheable)
                return;
            foreach (Domain d in a.Children)
            {
                List<ActiveDirectoryDomainParam> lprm = GetDomainConfigurations(d.Name);
                foreach (ActiveDirectoryDomainParam prm in lprm)
                {
                    if (prm.Enabled)
                    {
                        try
                        {
                            ActiveDirectoryDomain wr = new ActiveDirectoryDomain(d, prm, this.GlobalParams);
                            ad.Domains.Add(wr);
                            wr.Parent = ad;
                            if (wr.IsReacheable)
                                LoadChildDomainList(wr, d);
                        }
                        catch (Exception E)
                        {
                            this.BadDomains.Add(new ActiveDirectoryBadDomain(d.Name, string.Format("Error loading domain {0}", d.Name), DateTime.Now.Subtract(DateTime.Now)));
                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E1100"), d.Name), System.Diagnostics.EventLogEntryType.Error, 1100);
                            throw E;
                        }
                    }
                    else
                        this.BadDomains.Add(new ActiveDirectoryBadDomain(d.Name, string.Format("This domain {0} is administratively Disabled ", d.Name), DateTime.Now.Subtract(DateTime.Now)));
                }
            }
        }

        /// <summary>
        /// GetDomainConfiguration method implementation
        /// </summary>
        private List<ActiveDirectoryDomainParam> GetDomainConfigurations(string dnsname)
        {
            List<ActiveDirectoryDomainParam> result = new List<ActiveDirectoryDomainParam>();
            ActiveDirectoryDomainConfigurations res = null;
            ActiveDirectoryDomainParam tmp = null;
            foreach (ActiveDirectoryDomainConfigurations e in this._domainconfigs)
            {
                if (e.DomainName.ToLowerInvariant().Equals(dnsname.ToLowerInvariant()))
                {
                    res = e;
                    tmp = new ActiveDirectoryDomainParam();
                    tmp.DisplayName = e.DisplayName;
                    tmp.DnsName = e.DomainName;
                    tmp.MaxRows = e.MaxRows;
                    tmp.Password = e.Password;
                    tmp.QueryTimeout = e.Timeout;
                    tmp.SecureConnection = e.SecureConnection;
                    tmp.SuspendDelay = this.DefaultSuspendTime;
                    tmp.UserName = e.UserName;
                    tmp.Enabled = e.Enabled;
                    tmp.Position = e.Position;
                    tmp.ConnectString = e.ConnectString;
                    result.Add(tmp);
                }
            }
            if (result.Count == 0)
            {
                tmp = new ActiveDirectoryDomainParam();
                tmp.DisplayName = dnsname;
                tmp.DnsName = dnsname;
                tmp.MaxRows = this.MaxRowsPerDomain;
                tmp.Password = this.Password;
                tmp.QueryTimeout = this.DefaultTimeOut;
                tmp.SecureConnection = this.UsesScureConnection;
                tmp.SuspendDelay = this.DefaultSuspendTime;
                tmp.UserName = this.UserName;
                tmp.Enabled = true;
                tmp.Position = 0;
                tmp.ConnectString = string.Empty;
                result.Add(tmp);
            }
            return result;
        }
        #endregion

        #region Search Methods
        /// <summary>
        /// FillSearch method implementation
        /// </summary>
        public void FillSearch(IResults lst, string pattern, string domain, bool recursive = true)
        {
            if (lst == null)
                throw new NullReferenceException();
            EnsureLoaded();
            DateTime db = DateTime.Now;
            ActiveDirectoryInspectValues inspect = ActiveDirectoryRegEx.Parse(pattern);
            List<IDomain> dom = GetScopedDomain(domain, inspect);
            if (dom!=null)
            {
                foreach(IDomain xd in dom)
                {
                    IDomain d = xd;
                    if ((d != null) && (d.IsReacheable))
                    {
                        IResultsNode nd = new ActiveDirectoryResultsNode(d.DnsName, d.DisplayName, d.Position);
                        d.FillSearch(nd, pattern, recursive);
                        if (nd.HasResults())
                        {
                            IResultsNode added = nd;
                            while (d.Parent != null)
                            {
                                IResultsNode np = new ActiveDirectoryResultsNode(d.Parent.DnsName, d.Parent.DisplayName, d.Parent.Position);
                                np.AddNodeIfNotExists(added);
                                added = np.AddNodeIfNotExists(added);
                                d = d.Parent;
                            }
                            lst.AddNodeIfNotExists(added);
                        }
                    }
                }
            }
            else
            {
                bool rootit = false;
                try
                {
                    if (inspect.Tagged)
                    {
                        string dx = ActiveDirectoryRegEx.GetDomainNamePart(inspect);
                        foreach (IRootDomain dm in this.RootDomains)
                        {
                            bool finded = false;
                            foreach (ITopLevelName t in dm.TopLevelNames)
                            {
                                if (dx.ToLowerInvariant().EndsWith(t.TopLevelName))
                                {
                                    if (!dm.IsReacheable)
                                        continue;
                                    if (!rootit)
                                    {
                                        rootit = true;
                                        manual.Reset();
                                    }
                                    try
                                    {
                                        lock (lokobj)
                                        {
                                            countfillsearch++;
                                        }
                                        finded = true;
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(InternalFillSearch), new ActiveDirectoryFillSearchLoadState(dm, lst, pattern, true));
                                    }
                                    catch (Exception E)
                                    {
                                        LogEvent.Log(E, string.Format(ResourcesValues.GetString("E1020"), t.TopLevelName), System.Diagnostics.EventLogEntryType.Error, 1020);
                                    }
                                }
                            }
                            if (!finded)
                            {
                                IDomain xm = FindNETBIOSDomain(dm, dx);
                                if (xm != null)
                                {
                                    if (!dm.IsReacheable)
                                        continue;
                                    if (!rootit)
                                    {
                                        rootit = true;
                                        manual.Reset();
                                    }
                                    try
                                    {
                                        lock (lokobj)
                                        {
                                            countfillsearch++;
                                        }
                                        ThreadPool.QueueUserWorkItem(new WaitCallback(InternalFillSearch), new ActiveDirectoryFillSearchLoadState(xm, lst, pattern, true));
                                    }
                                    catch (Exception E)
                                    {
                                        LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2010"), xm.DnsName), System.Diagnostics.EventLogEntryType.Error, 2010);
                                    }
                                }
                                else
                                {
                                    foreach (ActiveDirectoryRootDomain offdm in this.RootDomains)
                                    {
                                        if (!offdm.IsReacheable)
                                            continue;
                                        if (!rootit)
                                        {
                                            rootit = true;
                                            manual.Reset();
                                        }
                                        try
                                        {
                                            lock (lokobj)
                                            {
                                                countfillsearch++;
                                            }
                                            ThreadPool.QueueUserWorkItem(new WaitCallback(InternalFillSearch), new ActiveDirectoryFillSearchLoadState(offdm, lst, pattern, true));
                                        }
                                        catch (Exception E)
                                        {
                                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2010B"), offdm.DnsName), System.Diagnostics.EventLogEntryType.Error, 2010);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (ActiveDirectoryRootDomain dm in this.RootDomains)
                        {
                            if (!dm.IsReacheable)
                                continue;
                            if (!rootit)
                            {
                                rootit = true;
                                manual.Reset();
                            }
                            try
                            {
                                lock (lokobj)
                                {
                                    countfillsearch++;
                                }
                                ThreadPool.QueueUserWorkItem(new WaitCallback(InternalFillSearch), new ActiveDirectoryFillSearchLoadState(dm, lst, pattern, true));
                            }
                            catch (Exception E)
                            {
                                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2010B"), dm.DnsName), System.Diagnostics.EventLogEntryType.Error, 2010);
                            }
                        }
                    }
                }
                finally
                {
                    if (rootit)
                        manual.WaitOne();
                }
            }
            _elapsedtime = DateTime.Now.Subtract(db);
            return;
        }


        /// <summary>
        /// InternalFillSearch method implementation
        /// </summary>
        private void InternalFillSearch(object objectstate)
        {
            ActiveDirectoryFillSearchLoadState data = (ActiveDirectoryFillSearchLoadState)objectstate;
            try
            {
                if (data.Domain.IsReacheable)
                {
                    ActiveDirectoryResultsNode nd = new ActiveDirectoryResultsNode(data.Domain.DnsName, data.Domain.DisplayName, data.Domain.Position);
                    data.Domain.FillSearch(nd, data.Pattern, data.Recursive);
                    if (nd.HasResults())
                        data.Node.AddNodeIfNotExists(nd);
                }

            }
            catch (Exception E)
            {
                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2010C"), data.Domain.DnsName), System.Diagnostics.EventLogEntryType.Error, 2010);
            }
            finally
            {
                lock (lokobj)
                {
                    countfillsearch--;
                    if (countfillsearch == 0)
                        manual.Set();
                }
            }
        }

        /// <summary>
        /// FindNETBIOSDomain method implementation
        /// </summary>
        private IDomain FindNETBIOSDomain(IDomain dm, string dx)
        {
            if ((!string.IsNullOrEmpty(dm.NetbiosName)) && (dx.ToLowerInvariant().Equals(dm.NetbiosName.ToLowerInvariant())))
                return dm;
            foreach (IDomain xm in dm.Domains)
            {
                if (xm.IsReacheable)
                {
                    if (dx.ToLowerInvariant().Equals(xm.NetbiosName.ToLowerInvariant()))
                        return xm;
                    else
                    {
                        IDomain sm = FindNETBIOSDomain(xm, dx);
                        if (sm != null)
                            return sm;
                    }
                }
            }
            return null;
        }
        #endregion

        #region Resolve methods
        /// <summary>
        /// FillResolve method implementation
        /// </summary>
        public void FillResolve(IResults lst, string pattern, bool recursive = true)
        {
            if (lst == null)
                throw new NullReferenceException();
            bool hasresult = false;
            EnsureLoaded();

            ActiveDirectoryInspectValues inspect = ActiveDirectoryRegEx.Parse(pattern);
            List<IDomain> dom = GetScopedDomain(inspect.DomainPart, inspect);
            if (dom!=null)
            {
                foreach (IDomain xd in dom)
                {
                    IDomain d = xd;
                    if ((d != null) && (d.IsReacheable))
                    {
                        IResultsNode nd = new ActiveDirectoryResultsNode(d.DnsName, d.DisplayName, d.Position);
                        d.FillResolve(nd, pattern, recursive);
                        if (nd.HasResults())
                        {
                            hasresult = true;
                            IResultsNode added = nd;
                            while (d.Parent != null)
                            {
                                IResultsNode np = new ActiveDirectoryResultsNode(d.Parent.DnsName, d.Parent.DisplayName, d.Parent.Position);
                                added = np.AddNodeIfNotExists(added);
                                d = d.Parent;
                            }
                            lst.AddNodeIfNotExists(added);
                        }
                    }
                }
            }
            else
            {
                foreach (ActiveDirectoryRootDomain dm in this.RootDomains)
                {
                    if (!dm.IsReacheable)
                        continue;
                    ActiveDirectoryResultsNode nd = new ActiveDirectoryResultsNode(dm.DnsName, dm.DisplayName, dm.Position);
                    dm.FillResolve(nd, pattern, recursive);
                    if (nd.HasResults())
                    {
                        hasresult = true;
                        lst.AddNodeIfNotExists(nd);
                        if (inspect.IsSID())  // eg : BuiltIn
                            break;
                    }
                }
            }
            if ((GlobalParams.Trace) && (hasresult == false))
                LogEvent.Trace(string.Format(ResourcesValues.GetString("E8001"), pattern), System.Diagnostics.EventLogEntryType.Warning, 8001);
        }
        #endregion

        #region Validate methods
        /// <summary>
        /// FillValidate method implementation
        /// </summary>
        public void FillValidate(IResults lst, string pattern, bool recursive = true)
        {
            if (lst == null)
                throw new NullReferenceException();
            bool hasresult = false;
            EnsureLoaded();

            ActiveDirectoryInspectValues inspect = ActiveDirectoryRegEx.Parse(pattern);
            List<IDomain> dom = GetScopedDomain(inspect.DomainPart, inspect);
            if (dom!=null)
            {
                foreach (IDomain xd in dom)
                {
                    IDomain d = xd;
                    if ((d != null) && (d.IsReacheable))
                    {
                        IResultsNode nd = new ActiveDirectoryResultsNode(d.DnsName, d.DisplayName, d.Position);
                        d.FillValidate(nd, pattern, recursive);
                        if (nd.HasResults())
                        {
                            hasresult = true;
                            IResultsNode added = nd;
                            while (d.Parent != null)
                            {
                                IResultsNode np = new ActiveDirectoryResultsNode(d.Parent.DnsName, d.Parent.DisplayName, d.Parent.Position);
                                // np.AddNodeIfNotExists(added);
                                added = np.AddNodeIfNotExists(added);
                                d = d.Parent;
                            }
                            lst.AddNodeIfNotExists(added);
                        }
                    }
                }
            }
            else
            {
                foreach (ActiveDirectoryRootDomain dm in this.RootDomains)
                {
                    if (!dm.IsReacheable)
                        continue;
                    ActiveDirectoryResultsNode nd = new ActiveDirectoryResultsNode(dm.DnsName, dm.DisplayName, dm.Position);
                    dm.FillValidate(nd, pattern, recursive);
                    if (nd.HasResults())
                    {
                        hasresult = true;
                        lst.AddNodeIfNotExists(nd);
                        if (inspect.IsSID())  // eg : BuiltIn
                            break;
                    }
                }
            }
            if ((GlobalParams.Trace) && (hasresult == false))
                LogEvent.Trace(string.Format(ResourcesValues.GetString("E8001B"), pattern), System.Diagnostics.EventLogEntryType.Warning, 8001);
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// GetUser method imlementation
        /// </summary>
        public IUser GetUser(string account)
        {
            IUser res = null;
            try
            {
                ActiveDirectoryResults lst = new ActiveDirectoryResultsRoot();
                FillResolve(lst, account);
                foreach (ActiveDirectoryResultObject usr in lst.GetResults())
                {
                    if (!usr.IsBuiltIn)
                    {
                        if (usr is ActiveDirectoryUser)
                            return (IUser)usr;
                    }
                }
                foreach (ActiveDirectoryResultsNode node in lst.GetNodes())
                {
                    res = DoFillUser(node);
                    if ((res != null) && (!res.IsBuiltIn))
                    {
                        if (res is ActiveDirectoryUser)
                            return (IUser)res;
                    }
                }
                return res;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// DoFillUser mlethod implementation
        /// </summary>
        private ActiveDirectoryUser DoFillUser(ActiveDirectoryResultsNode node)
        {
            ActiveDirectoryUser res = null;
            foreach (ActiveDirectoryResultObject usr in node.GetResults())
            {
                if (!usr.IsBuiltIn)
                {
                    if (usr is ActiveDirectoryUser)
                    return (ActiveDirectoryUser)usr;
                }
            }
            foreach (ActiveDirectoryResultsNode nde in node.GetNodes())
            {
                res = DoFillUser(nde);
                if ((res != null) && (!res.IsBuiltIn))
                {
                    if (res is ActiveDirectoryUser)
                        return (ActiveDirectoryUser)res;
                }
            }
            return null;
        }
        #endregion
    }
    #endregion

    #region ActiveDirectoryRootDomain class
    public class ActiveDirectoryRootDomain : ActiveDirectoryDomain, IRootDomain
    {
        private List<ITopLevelName> _toplevelnames = null;

        /// <summary>
        /// Constructor
        /// </summary>
        internal ActiveDirectoryRootDomain():base()
        {
            _toplevelnames = new List<ITopLevelName>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActiveDirectoryRootDomain(Domain domain, List<ITopLevelName> toplevels, IDomainParam prm, IGlobalParams global): base(domain, (ActiveDirectoryDomainParam)prm, global)
        {
            _toplevelnames = toplevels as List<ITopLevelName>;
            _ismaster = true;
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(string domain, List<ITopLevelName> toplevels, IDomainParam prm, IGlobalParams global)
        {
            base.Initialize(domain, prm, global);
            _toplevelnames = toplevels;
            _ismaster = true;
        }

        /// <summary>
        /// TopLevelNames property implementation
        /// </summary>
        public List<ITopLevelName> TopLevelNames
        {
            get { return _toplevelnames; }
        }

    }
    #endregion

    #region ActiveDirectoryDomainParam class
    public class ActiveDirectoryDomainParam: IDomainParam
    {
        public string DnsName { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Password{ get; set; }
        public bool SecureConnection { get; set; }
        public short QueryTimeout { get; set; }
        public short SuspendDelay { get; set; }
        public int MaxRows { get; set; }
        public bool Enabled { get; set; }
        public int Position { get; set; }
        public string ConnectString { get; set; }
    }
    #endregion

    #region ActiveDirectoryDomain class
    public class ActiveDirectoryDomain: IDomain
    {
        private string _aduser;
        private string _adpwd;
        private short _timeout = 30;
        private short _suspend = 1;
        private TimeSpan _elapsedtime;
     //   private string _ctxpath;
        private int _maxrows = 200;
        protected bool _ismaster = false;
        private bool _isroot = false;
        private bool _isreacheable = false;
        private string _reachmessage = String.Empty;
        private string _dnsname;
        private string _displayname;
        private string _netbiosname;
        private int _position = 9999;
        private string _connecstring;
        private AuthenticationTypes _secureparams;
        private DateTime _suspendtime = DateTime.MinValue;
        private List<IDomain> _children;
        private IDomain _parent;
        private IGlobalParams _adprm;

        /// <summary>
        /// Constructor
        /// </summary>
        internal ActiveDirectoryDomain()
        {
            _children = new List<IDomain>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ActiveDirectoryDomain(Domain domain, ActiveDirectoryDomainParam parameters, IGlobalParams global)
        {
            DateTime db = DateTime.Now;
            try
            {
                _dnsname = parameters.DnsName;
                _displayname = parameters.DisplayName;
                _aduser = parameters.UserName;
                _adpwd = parameters.Password;
                _timeout = parameters.QueryTimeout;
                _suspend = parameters.SuspendDelay;
                _maxrows = parameters.MaxRows;
                _position = parameters.Position;
                _connecstring = parameters.ConnectString;
                if (parameters.SecureConnection)
                {
                    _secureparams = AuthenticationTypes.Signing | AuthenticationTypes.Sealing | AuthenticationTypes.Secure | AuthenticationTypes.FastBind | AuthenticationTypes.ReadonlyServer;
                }
                else
                {
                    _secureparams = AuthenticationTypes.Signing | AuthenticationTypes.Secure | AuthenticationTypes.FastBind;
                }
                _ismaster = false;
                _dnsname = domain.Name.ToLower();
                _isreacheable = Initialize(domain.Name);
                _children = new List<IDomain>();
                GlobalParams = global;
            }
            finally
            {
                _elapsedtime = DateTime.Now.Subtract(db);
            }
        }

        public void Initialize(string domain, IDomainParam parameters, IGlobalParams global)
        {
            DateTime db = DateTime.Now;
            try
            {
                _dnsname = parameters.DnsName;
                _displayname = parameters.DisplayName;
                _aduser = parameters.UserName;
                _adpwd = parameters.Password;
                _timeout = parameters.QueryTimeout;
                _suspend = parameters.SuspendDelay;
                _maxrows = parameters.MaxRows;
                _position = parameters.Position;
                _connecstring = parameters.ConnectString;
                if (parameters.SecureConnection)
                {
                    _secureparams = AuthenticationTypes.Signing | AuthenticationTypes.Sealing | AuthenticationTypes.Secure | AuthenticationTypes.FastBind | AuthenticationTypes.ReadonlyServer;
                }
                else
                {
                    _secureparams = AuthenticationTypes.Signing | AuthenticationTypes.Secure | AuthenticationTypes.FastBind;
                }
                _ismaster = false;
                _dnsname = domain.ToLower();
                _isreacheable = Initialize(domain);
                _children = new List<IDomain>();
                GlobalParams = global;
            }
            finally
            {
                _elapsedtime = DateTime.Now.Subtract(db);
            }
        }
        
        /// <summary>
        /// Initialize method implementation
        /// </summary>
        private bool Initialize(string  domain)
        {
            bool result = true;
            using (Identity impersonate = Identity.Impersonate(_aduser, _adpwd))
            {
                DirectoryEntry RootDSE = null;
                try
                {
                    RootDSE = new DirectoryEntry(string.Format("LDAP://{0}/RootDSE", this.DnsName), _aduser, _adpwd, AuthenticationTypes.Signing | AuthenticationTypes.Sealing | AuthenticationTypes.Secure);
                    RootDSE.RefreshCache();
                    string strNamingContext = RootDSE.Properties["defaultNamingContext"].Value.ToString();
                    string strConfigContext = RootDSE.Properties["configurationNamingContext"].Value.ToString();
                    string ldapqry = string.Format("LDAP://{0}/CN=Partitions,{1}", this.DnsName, strConfigContext);
                    DateTime db = DateTime.Now;

                    _netbiosname = GetNetBiosName(string.Format(ldapqry, strConfigContext), this.DnsName, this.UserName, this.Password, _secureparams);
                    _isreacheable = (!string.IsNullOrEmpty(_netbiosname));
                    TimeSpan elapsedtime = DateTime.Now.Subtract(db);
                    if (elapsedtime.TotalSeconds > this._timeout)
                        throw new Exception();
                    if (!_isreacheable)
                        throw new Exception("Invalid NetBIOS name !");
                }
                catch (Exception Ex)
                {
                    _isreacheable = false;
                    result = false;
                    LogEvent.Log(Ex, string.Format(ResourcesValues.GetString("E1100B"), this.DnsName), System.Diagnostics.EventLogEntryType.Error, 1100);
                }
                finally
                {
                    if (RootDSE != null)
                        RootDSE.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// GetNetBiosName method implementation
        /// </summary>
        private string GetNetBiosName(string ldapUrl, string dnsname, string userName, string password, AuthenticationTypes secure)
        {
            string netbiosName = string.Empty;
            using (DirectoryEntry dirEntry = new DirectoryEntry(ldapUrl, userName, password, secure))
            {
                using (DirectorySearcher searcher = new DirectorySearcher(dirEntry))
                {
                    searcher.Filter = "netbiosname=*";
                    searcher.PropertiesToLoad.Add("cn");
                    searcher.PropertiesToLoad.Add("nETBIOSName");
                    searcher.PropertiesToLoad.Add("name");
                    searcher.PropertiesToLoad.Add("dnsRoot");

                    SearchResultCollection results = searcher.FindAll();
                    if (results != null)
                    {
                        foreach (SearchResult sr in results)
                        {
                            ResultPropertyValueCollection dns = sr.Properties["dnsRoot"];
                            string dnsroot = dns[0].ToString();
                            if (dnsroot.ToLowerInvariant().Equals(dnsname))
                            {
                                ResultPropertyValueCollection rpvc = sr.Properties["nETBIOSName"];
                                netbiosName = rpvc[0].ToString();
                                if (string.IsNullOrEmpty(netbiosName))
                                {
                                    rpvc = sr.Properties["name"];
                                    netbiosName = rpvc[0].ToString();
                                    if (string.IsNullOrEmpty(netbiosName))
                                    {
                                        rpvc = sr.Properties["cn"];
                                        netbiosName = rpvc[0].ToString();
                                    }
                                }
                            }
                        }
                    }
                    return netbiosName;
                };
            };
        } 


        #region properties
        /// <summary>
        /// Parent property implementation
        /// </summary>
        public IDomain Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// IsReacheable property implementation
        /// </summary>
        public bool IsReacheable
        {
            get { return _isreacheable; }
            internal set { _isreacheable = value; }
        }

        /// <summary>
        /// ErrorMessage properety implementation
        /// </summary>
        public string ErrorMessage
        {
            get { return _reachmessage; }
            internal set { _reachmessage = value; }
        }

        /// <summary>
        /// UserName property implementation
        /// </summary>
        public string UserName
        {
            get { return _aduser; }
            internal set { _aduser = value; }
        }

        /// <summary>
        ///  Password property implementation
        /// </summary>
        public string Password
        {
            get { return _adpwd; }
            internal set { _adpwd = value; }
        }

        /// <summary>
        /// IsMaster property implementation
        /// </summary>
        public bool IsMaster
        {
            get { return _ismaster; }
            internal set { _ismaster = value; }
        }

        /// <summary>
        /// IsRoot property implementation
        /// </summary>
        public bool IsRoot
        {
            get { return _isroot; }
            set { _isroot = value; }
        }

        /// <summary>
        /// DnsName property implementation
        /// </summary>
        public string DnsName
        {
            get { return _dnsname; }
            internal set { _dnsname = value; }
        }

        /// <summary>
        /// DisplayName property implementation
        /// </summary>
        public string DisplayName
        {
            get { return _displayname; }
            internal set { _displayname = value; }
        }

        /// <summary>
        /// SamName property implementation
        /// </summary>
        public string NetbiosName
        {
            get { return _netbiosname; }
            internal set { _netbiosname = value; }
        }

        /// <summary>
        /// Domains property implementation
        /// </summary>
        public List<IDomain> Domains
        {
            get { return _children; }
        }

        /// <summary>
        /// ElapsedTime property implementation
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get { return _elapsedtime; }
            internal set { _elapsedtime = value; }
        }

        /// <summary>
        /// Timeout property implementation
        /// </summary>
        public short Timeout
        {
            get { return _timeout; }
            internal set { _timeout = value; }
        }

        /// <summary>
        /// MaxRows property implementation
        /// </summary>
        public int MaxRows
        {
            get { return _maxrows; }
            internal set { _maxrows = value; }
        }

        /// <summary>
        /// MaxRows property implementation
        /// </summary>
        public int Position
        {
            get { return _position; }
            internal set { _position = value; }
        }

        /// <summary>
        /// ConnectString property implementation
        /// </summary>
        public string ConnectString
        {
            get { return _connecstring; }
            internal set 
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (value.ToLower().StartsWith("ldap://"))
                        _connecstring = "LDAP://" + value.Substring(7);
                    else
                        _connecstring = value;
                }
                else
                    _connecstring = value;
            }
        }

        /// <summary>
        /// GlobalParams property implementatioon
        /// </summary>
        public IGlobalParams GlobalParams
        {
            get { return _adprm; }
            set { _adprm = value; }
        }
        #endregion

        #region Searchers Configuration

        /// <summary>
        /// ConfigureSearcherForGroups method implementation
        /// </summary>
        private void ConfigureSearcherForGroups(DirectorySearcher src)
        {
            src.SizeLimit = MaxRows;
            src.ClientTimeout = new TimeSpan(0, 0, Convert.ToInt32(this._timeout));
            src.SearchScope = SearchScope.Subtree;
            src.PropertiesToLoad.Clear();
            src.PropertiesToLoad.Add("displayName");
            src.PropertiesToLoad.Add("sAMAccountName");
            src.PropertiesToLoad.Add("objectGuid");
            src.PropertiesToLoad.Add("objectSid");
            src.PropertiesToLoad.Add("groupType");
        }

        /// <summary>
        /// ConfigureSearcherForUsers method implementation
        /// </summary>
        private void ConfigureSearcherForUsers(DirectorySearcher src)
        {
            src.SizeLimit = MaxRows;
            src.ClientTimeout = new TimeSpan(0, 0, Convert.ToInt32(this._timeout));
            src.SearchScope = SearchScope.Subtree;
            src.PropertiesToLoad.Clear();
            src.PropertiesToLoad.Add("displayName");
            src.PropertiesToLoad.Add("sAMAccountName");
            src.PropertiesToLoad.Add("userPrincipalName");
            src.PropertiesToLoad.Add("mail");
        }

        /// <summary>
        /// GetDomainEntry method implementation
        /// </summary>
        private DirectoryEntry GetDomainEntry()
        {
            if (string.IsNullOrEmpty(this.ConnectString))
                return Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, this.DnsName, this.UserName, this.Password)).GetDirectoryEntry();
            else
                return new DirectoryEntry(this.ConnectString, this.UserName, this.Password);
        }

        /// <summary>
        /// FillSearch method implementation
        /// </summary>
        public void FillSearch(IResults lst, string searchPattern, bool recursive = true)
        {
            if (!IsReacheable)
                return;
            if ((!_ismaster) && (_suspendtime.AddMinutes(this._suspend) > DateTime.Now))
                return;
            if (string.IsNullOrEmpty(searchPattern))
                return;
            using (Identity impersonate = Identity.Impersonate(_aduser, _adpwd))
            {
                DirectoryEntry domain = null;
                try
                {
                    DateTime db = DateTime.Now;
                    ActiveDirectoryInspectValues inspect = ActiveDirectoryRegEx.Parse(searchPattern);
                    domain = GetDomainEntry();

                    string leadstar = "";
                    string endstar = "";

                    switch (this.GlobalParams.SmoothRequestor)
                    {
                        case ProxySmoothRequest.Smooth:
                            leadstar = "*";
                            endstar = "*";
                            break;
                        case ProxySmoothRequest.StarsAfter:
                            leadstar = "";
                            endstar = "*";
                            break;
                        case ProxySmoothRequest.StarsBefore:
                            leadstar = "*";
                            endstar = "";
                            break;
                        default:
                            leadstar = "";
                            endstar = "";
                            break;
                    }
                    if (inspect.IsSAMForm())
                        leadstar = "";

                    // Load Groups if needed
                    if (inspect.IsSAMForm() || inspect.IsAllOptions() || inspect.IsSID())
                    {
                        string grpldap = "(&(objectClass=group)";
                        if (inspect.IsSID())
                            grpldap += "(objectSid=" + inspect.Pattern + ")";
                        else
                            if (!string.IsNullOrEmpty(inspect.UserNamePart))
                                grpldap += "(sAMAccountName=" + leadstar + inspect.UserNamePart + endstar + ")";
                            else
                                grpldap += "(sAMAccountName=" + leadstar + inspect.Pattern + endstar + ")";
                        grpldap += ")";
                        try
                        {
                            using (DirectorySearcher dsgrp = new DirectorySearcher(domain, grpldap))
                            {
                                ConfigureSearcherForGroups(dsgrp);
                                using (SearchResultCollection resultsgrp = dsgrp.FindAll())
                                {
                                    foreach (SearchResult sr in resultsgrp)
                                    {
                                        try
                                        {
                                            lst.AddResultIfNotExists(new ActiveDirectoryRole(this, sr));
                                        }
                                        catch (Exception E)
                                        {
                                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2502"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2502);
                                        }
                                    }
                                };
                            };
                        }
                        catch (Exception E)
                        {
                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2501"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2501);
                        }
                        _elapsedtime = DateTime.Now.Subtract(db);
                        if (recursive)
                        {
                            foreach (ActiveDirectoryDomain d in this.Domains)
                            {
                                ActiveDirectoryResultsNode nd = new ActiveDirectoryResultsNode(d.DnsName, d.DisplayName, d.Position);
                                d.FillSearch(nd, searchPattern, recursive);
                                if (nd.HasResults())
                                    lst.AddNodeIfNotExists(nd);
                            }
                        }
                    }
                    // Load Users anyway
                    try
                    {
                        string qryldap = "(&(objectCategory=user)(objectClass=user)(|";
                        if (inspect.IsUPNForm())
                            qryldap += "(userprincipalname=" + leadstar + inspect.Pattern + endstar + ")";
                        if (inspect.IsSAMForm() || inspect.IsAllOptions())
                            qryldap += "(sAMAccountName=" + leadstar + inspect.UserNamePart + endstar + ")";
                        if (this.GlobalParams.SearchByDisplayName)
                            qryldap += "(displayName=" + leadstar + searchPattern + endstar + ")";
                        if (this.GlobalParams.SearchByMail)
                            qryldap += "(mail=" + leadstar + searchPattern + endstar + ")";
#if enabledonly
                        qryldap += ")(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
#endif
#if enabledisabled
                        qryldap += ")(userAccountControl:1.2.840.113556.1.4.803:=512)";
#endif
#if smartenabled
                        qryldap += ")(userAccountControl:1.2.840.113556.1.4.803:=2))";
#endif
                        qryldap += ")";

                        using (DirectorySearcher dsusr = new DirectorySearcher(domain, qryldap))
                        {
                            ConfigureSearcherForUsers(dsusr);

                            using (SearchResultCollection resultsusr = dsusr.FindAll())
                            {
                                foreach (SearchResult sr in resultsusr)
                                {
                                    try
                                    {
                                        lst.AddResultIfNotExists(new ActiveDirectoryUser(this, sr));
                                    }
                                    catch (Exception E)
                                    {
                                        LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2002"), sr.Path, this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2002);
                                    }
                                }
                            };
                        };
                    }
                    catch (Exception E)
                    {
                        LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2000"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2000);
                    }
                    _elapsedtime = DateTime.Now.Subtract(db);
                    if (recursive)
                    {
                        foreach (ActiveDirectoryDomain d in this.Domains)
                        {
                            ActiveDirectoryResultsNode nd = new ActiveDirectoryResultsNode(d.DnsName, d.DisplayName, d.Position);
                            d.FillSearch(nd, searchPattern, recursive);
                            if (nd.HasResults())
                                lst.AddNodeIfNotExists(nd);
                        }
                    }
                    _suspendtime = DateTime.MinValue;
                }
                catch (Exception E)
                {
                    _suspendtime = DateTime.Now;
                    LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2001"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2001);
                    return;
                }
                finally
                {
                    if (domain != null)
                       domain.Dispose();
                }
            }
        }

        /// <summary>
        /// FillResolve method implementation
        /// </summary>
        public void FillResolve(IResults lst, string searchPattern, bool recursive = true)
        {
            if (!IsReacheable)
                return;
            if ((!_ismaster) && (_suspendtime.AddMinutes(this._suspend) > DateTime.Now))
                return;
            if (string.IsNullOrEmpty(searchPattern))
                return;
            using (Identity impersonate = Identity.Impersonate(_aduser, _adpwd))
            {
                DirectoryEntry domain = null;
                try
                {
                    DateTime db = DateTime.Now;
                    ActiveDirectoryInspectValues inspect = ActiveDirectoryRegEx.Parse(searchPattern);
                    domain = GetDomainEntry();

                    string leadstar = "";
                    string endstar = "";

                    switch (this.GlobalParams.SmoothRequestor)
                    {
                        case ProxySmoothRequest.Smooth:
                            leadstar = "*";
                            endstar = "*";
                            break;
                        case ProxySmoothRequest.StarsAfter:
                            leadstar = "";
                            endstar = "*";
                            break;
                        case ProxySmoothRequest.StarsBefore:
                            leadstar = "*";
                            endstar = "";
                            break;
                        default:
                            leadstar = "";
                            endstar = "";
                            break;
                    }
                    if (inspect.IsSAMForm())
                    {
                        leadstar = "";
                    }

                    // Load Groups if needed
                    if (inspect.IsSAMForm() || inspect.IsAllOptions() || inspect.IsSID())
                    {
                        string grpldap = "(&(objectClass=group)";
                        if (inspect.IsSID())
                            grpldap += "(objectSid=" + inspect.Pattern + ")";
                        else
                            if (!string.IsNullOrEmpty(inspect.UserNamePart))
                                grpldap += "(sAMAccountName=" + leadstar + inspect.UserNamePart + endstar + ")";
                            else
                                grpldap += "(sAMAccountName=" + leadstar + inspect.Pattern + endstar + ")";
                        grpldap += ")";
                        try
                        {
                            using (DirectorySearcher dsgrp = new DirectorySearcher(domain, grpldap))
                            {
                                ConfigureSearcherForGroups(dsgrp);
                                using (SearchResultCollection resultsgrp = dsgrp.FindAll())
                                {
                                    List<ActiveDirectoryRole> babes = new List<ActiveDirectoryRole>();
                                    foreach (SearchResult sr in resultsgrp)
                                    {
                                        try
                                        {
                                            ActiveDirectoryRole babe = new ActiveDirectoryRole(this, sr);
                                            if (CheckRoleBabe(babe, inspect))
                                            {
                                                babes.Add(babe);
                                            }
                                        }
                                        catch (Exception E)
                                        {
                                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2502B"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2502);
                                        }
                                    }
                                    if (babes.Count == 0)
                                    {
                                        foreach (SearchResult sr in resultsgrp)
                                        {
                                            try
                                            {
                                                lst.AddResultIfNotExists(new ActiveDirectoryRole(this, sr));
                                            }
                                            catch (Exception E)
                                            {
                                                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2502"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2502);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (ActiveDirectoryRole babe in babes)
                                        {
                                            lst.AddResultIfNotExists(babe);
                                        }
                                    }
                                };
                            };
                        }
                        catch (Exception E)
                        {
                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2500"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2500);
                        }
                        _elapsedtime = DateTime.Now.Subtract(db);

                        if (recursive)
                        {
                            foreach (IDomain d in this.Domains)
                            {
                                IResultsNode nd = new ActiveDirectoryResultsNode(d.DnsName, d.DisplayName, d.Position);
                                d.FillResolve(nd, searchPattern, recursive);
                                if (nd.HasResults())
                                    lst.AddNodeIfNotExists(nd);
                            }
                        }
                    }

                    // Load Users anyway
                    try
                    {
                        string qryldap = "(&(objectCategory=user)(objectClass=user)(|";
                        if (inspect.IsUPNForm())
                            qryldap += "(userprincipalname=" + leadstar + inspect.Pattern + endstar + ")";
                        if (inspect.IsSAMForm() || inspect.IsAllOptions())
                            qryldap += "(sAMAccountName=" + leadstar + inspect.UserNamePart + endstar + ")";
                        if (this.GlobalParams.SearchByDisplayName)
                            qryldap += "(displayName=" + leadstar + searchPattern + endstar + ")";
                        if (this.GlobalParams.SearchByMail)
                            qryldap += "(mail=" + leadstar + searchPattern + endstar + ")";
#if enabledonly
                        qryldap += ")(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
#endif
#if enabledisabled
                        qryldap += ")(userAccountControl:1.2.840.113556.1.4.803:=512)";
#endif
#if smartenabled
                        qryldap += ")(userAccountControl:1.2.840.113556.1.4.803:=2))";
#endif
                        qryldap += ")";

                        using (DirectorySearcher dsusr = new DirectorySearcher(domain, qryldap))
                        {
                            ConfigureSearcherForUsers(dsusr);
                            using (SearchResultCollection resultsusr = dsusr.FindAll())
                            {
                                List<ActiveDirectoryUser> babes = new List<ActiveDirectoryUser>();
                                foreach (SearchResult sr in resultsusr)
                                {
                                    try
                                    {
                                        ActiveDirectoryUser babe = new ActiveDirectoryUser(this, sr);
                                        if (CheckUserBabe(babe, inspect))
                                        {
                                            babes.Add(babe);
                                        }
                                    }
                                    catch (Exception E)
                                    {
                                        LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2502C"), sr.Path, this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2502);
                                    }
                                }
                                if (babes.Count == 0)
                                {
                                    foreach (SearchResult sr in resultsusr)
                                    {
                                        try
                                        {
                                            lst.AddResultIfNotExists(new ActiveDirectoryUser(this, sr));
                                        }
                                        catch (Exception E)
                                        {
                                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2502C"), sr.Path, this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2502);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (ActiveDirectoryUser babe in babes)
                                    {
                                        lst.AddResultIfNotExists(babe);
                                    }
                                }
                            };
                        };
                    }
                    catch (Exception E)
                    {
                        LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2500B"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2500);
                    }
                    _elapsedtime = DateTime.Now.Subtract(db);
                    if (recursive)
                    {
                        foreach (IDomain d in this.Domains)
                        {
                            IResultsNode nd = new ActiveDirectoryResultsNode(d.DnsName, d.DisplayName, d.Position);
                            d.FillResolve(nd, searchPattern, recursive);
                            if (nd.HasResults())
                                lst.AddNodeIfNotExists(nd);
                        }
                    }
                    _suspendtime = DateTime.MinValue;
                }
                catch (Exception E)
                {
                    _suspendtime = DateTime.Now;
                    LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2501B"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2501);
                    return;
                }
                finally
                {
                    if (domain != null)
                        domain.Dispose();
                }
            }
        }

        /// <summary>
        /// CheckUserBabe method implementation
        /// </summary>
        private bool CheckUserBabe(ActiveDirectoryUser babe, ActiveDirectoryInspectValues inspect)
        {
            bool result = false;
            if (this.GlobalParams.SearchByMail)
            {
                if (!string.IsNullOrEmpty(babe.EmailAddress))
                {
                    result = (inspect.Pattern.ToLowerInvariant().Trim().Equals(babe.EmailAddress.ToLowerInvariant().Trim()));
                    if (result) return true;
                }
            }
            if (this.GlobalParams.SearchByDisplayName)
            {
                if (!string.IsNullOrEmpty(babe.DisplayName))
                {
                    result = (inspect.Pattern.ToLowerInvariant().Trim().Equals(babe.DisplayName.ToLowerInvariant().Trim()));
                    if (result) return true;
                }
            }
            if (inspect.IsUPNForm())
            {
                if (!string.IsNullOrEmpty(babe.UserPrincipalName))
                {
                    result = (inspect.Pattern.ToLowerInvariant().Trim().Equals(babe.UserPrincipalName.ToLowerInvariant().Trim()));
                    if (result) return true;
                }
            }
            if (inspect.IsSAMForm() || inspect.IsAllOptions())
            {
                if (!string.IsNullOrEmpty(babe.SamAaccount))
                {
                    result = ((this.NetbiosName + "\\" + inspect.UserNamePart).ToLowerInvariant().Trim().Equals(babe.SamAaccount.ToLowerInvariant().Trim()));
                    if (result) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// CheckRoleBabe method implementation
        /// </summary>
        private bool CheckRoleBabe(ActiveDirectoryRole babe, ActiveDirectoryInspectValues inspect)
        {
            bool result = false;
            if (inspect.IsSID())
            {
                if (!string.IsNullOrEmpty(babe.SID))
                {
                    result = (inspect.Pattern.ToLowerInvariant().Trim().Equals(babe.SID.ToLowerInvariant().Trim()));
                    if (result) return true;
                }
            }
            if (!string.IsNullOrEmpty(inspect.UserNamePart))
            {
                if (!string.IsNullOrEmpty(babe.SamAaccount))
                {
                    result = ((this.NetbiosName + "\\" + inspect.UserNamePart).ToLowerInvariant().Trim().Equals(babe.SamAaccount.ToLowerInvariant().Trim()));
                    if (result) return true;
                }
            }
            else if (!string.IsNullOrEmpty(inspect.Pattern))
            {
                if (!string.IsNullOrEmpty(babe.SamAaccount))
                {
                    result = (inspect.Pattern.ToLowerInvariant().Trim().Equals(babe.SamAaccount.ToLowerInvariant().Trim()));
                    if (result) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// FillValidate method implementation
        /// </summary>
        public void FillValidate(IResults lst, string searchPattern, bool recursive = true)
        {
            if (!IsReacheable)
                return;
            if ((!_ismaster) && (_suspendtime.AddMinutes(this._suspend) > DateTime.Now))
                return;
            if (string.IsNullOrEmpty(searchPattern))
                return;
            using (Identity impersonate = Identity.Impersonate(_aduser, _adpwd))
            {
                DirectoryEntry domain = null;
                try
                {
                    DateTime db = DateTime.Now;
                    ActiveDirectoryInspectValues inspect = ActiveDirectoryRegEx.Parse(searchPattern);
                    domain = GetDomainEntry();

                    bool trusted = CheckDomain(inspect);

                    // Load Groups if needed
                    if (inspect.IsSID())
                    {
                        string grpldap = "(&(objectClass=group)";
                        grpldap += "(objectSid=" + inspect.Pattern + ")";
                        grpldap += ")";
                        try
                        {
                            using (DirectorySearcher dsgrp = new DirectorySearcher(domain, grpldap))
                            {
                                ConfigureSearcherForGroups(dsgrp);
                                SearchResult resultsgrp = dsgrp.FindOne();
                                {
                                    if (resultsgrp != null)
                                    {
                                        try
                                        {
                                            lst.AddResultIfNotExists(new ActiveDirectoryRole(this, resultsgrp));
                                        }
                                        catch (Exception E)
                                        {
                                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2502B"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2502);
                                        }
                                    }
                                }
                            };
                        }
                        catch (Exception E)
                        {
                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2500"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2500);
                        }
                        _elapsedtime = DateTime.Now.Subtract(db);
                    }
                    if (recursive)
                    {
                        foreach (IDomain d in this.Domains)
                        {
                            IResultsNode nd = new ActiveDirectoryResultsNode(d.DnsName, d.DisplayName, d.Position);
                            d.FillValidate(nd, searchPattern, recursive);
                            if (nd.HasResults())
                                lst.AddNodeIfNotExists(nd);
                        }
                    }

                    // Load Users anyway
                    if ((trusted) && (inspect.IsSAMForm() || inspect.IsUPNForm()) && (!inspect.IsSID()))
                    {
                        try
                        {
                            string qryldap = "(&(objectCategory=user)(objectClass=user)(|";
                            if (inspect.IsUPNForm())
                                qryldap += "(userprincipalname=" + inspect.Pattern + ")";
                            if (inspect.IsSAMForm())
                                qryldap += "(sAMAccountName=" + inspect.UserNamePart + ")";
#if enabledonly
                            qryldap += ")(!(userAccountControl:1.2.840.113556.1.4.803:=2))";
#endif
#if enabledisabled
                            qryldap += ")(userAccountControl:1.2.840.113556.1.4.803:=512)";
#endif
#if smartenabled
                            qryldap += ")(userAccountControl:1.2.840.113556.1.4.803:=512))";  // allow validation for disabled users
#endif
                            qryldap += ")";

                            using (DirectorySearcher dsusr = new DirectorySearcher(domain, qryldap))
                            {
                                ConfigureSearcherForUsers(dsusr);
                                SearchResult resultsusr = dsusr.FindOne();
                                if (resultsusr != null)
                                {
                                    try
                                    {
                                        lst.AddResultIfNotExists(new ActiveDirectoryUser(this, resultsusr));
                                    }
                                    catch (Exception E)
                                    {
                                        LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2502C"), resultsusr.Path, this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2502);
                                    }
                                }
                            };
                        }
                        catch (Exception E)
                        {
                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2500B"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2500);
                        }
                        _elapsedtime = DateTime.Now.Subtract(db);
                    }
                    if (recursive)
                    {
                        foreach (IDomain d in this.Domains)
                        {
                            IResultsNode nd = new ActiveDirectoryResultsNode(d.DnsName, d.DisplayName, d.Position);
                            d.FillValidate(nd, searchPattern, recursive);
                            if (nd.HasResults())
                                lst.AddNodeIfNotExists(nd);
                        }
                    }
                    _suspendtime = DateTime.MinValue;
                }
                catch (Exception E)
                {
                    _suspendtime = DateTime.Now;
                    LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2501B"), this.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2501);
                    return;
                }
                finally
                {
                    if (domain != null)
                        domain.Dispose();
                }
            }
        }

        /// <summary>
        /// CheckDomain method implementation
        /// </summary>
        private bool CheckDomain(ActiveDirectoryInspectValues inspect)
        {
            bool result = false;
            if (inspect.HasDomain)
            {
                if (inspect.IsSAMForm())
                {
                    result = inspect.DomainPart.ToLowerInvariant().Trim().Equals(this.NetbiosName.ToLowerInvariant().Trim());
                }
                else if (inspect.IsUPNForm())
                {
                    result = true;  // Always verified before with Toplevelnames
                }
            }
            else if (inspect.IsSID())
            {
                result = true;
            }
            return result;
        }
        #endregion
    }
    #endregion

    #region ActiveDirectoryResults
    public abstract class ActiveDirectoryResults: IResults
    {
        /// <summary>
        /// Nodes method implementation
        /// </summary>
        public abstract List<IResultsNode> GetNodes();

        /// <summary>
        /// Results method implementation
        /// </summary>
        public abstract List<IResultObject> GetResults();

        /// <summary>
        /// HasResults method implementation
        /// </summary>
        public abstract bool HasResults();

        /// <summary>
        /// AddResultIfNotExists method implementation
        /// </summary>
        public abstract IResultsNode AddNodeIfNotExists(IResultsNode anode);

        /// <summary>
        /// AddResultIfNotExists method implementation
        /// </summary>
        public abstract bool AddResultIfNotExists(IResultObject obj);

        /// <summary>
        /// GetName method implementation
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// GetDisplayName method implementation
        /// </summary>
        /// <returns></returns>
        public abstract string GetDisplayName();

        /// <summary>
        /// GetPosition method implementation
        /// </summary>
        /// <returns></returns>
        public abstract int GetPosition();

    }
    #endregion

    #region ActiveDirectoryResultsRoot
    public class ActiveDirectoryResultsRoot : ActiveDirectoryResults, IResultsRoot
    {

        private List<IResultsNode> _nodelist;
        private bool _created = false;

        /// <summary>
        /// ActiveDirectoryResults constructor
        /// </summary>
        public ActiveDirectoryResultsRoot()
        {
            if (!_created)
            {
                _nodelist = new List<IResultsNode>();
                _created = true;
            }
        }

        /// <summary>
        /// Initialize method override
        /// </summary>
        public void Initialize()
        {
            if (!_created)
            {
                _nodelist = new List<IResultsNode>();
                _created = true;
            }
        }

        /// <summary>
        /// Nodes property implementation
        /// </summary>
        public override List<IResultsNode> GetNodes()
        {
            return _nodelist;
        }

        /// <summary>
        /// Nodes property implementation
        /// </summary>
        public override List<IResultObject> GetResults()
        {
            return new List<IResultObject>();  
        }

        /// <summary>
        /// HasResults method implementation
        /// </summary>
        public override bool HasResults()
        {
            foreach (IResultsNode nd in _nodelist)
            {
                if (nd.HasResults())
                   return true;
            }
            return false;
        }

        /// <summary>
        /// AddNodeIfNotExists method implementation
        /// </summary>
        public override IResultsNode AddNodeIfNotExists(IResultsNode anode)
        {
            foreach (IResultsNode nd in _nodelist)
            {
                if ((nd.GetName().ToLowerInvariant().Equals(anode.GetName().ToLowerInvariant())) && (nd.GetDisplayName().ToLowerInvariant().Equals(anode.GetDisplayName().ToLowerInvariant())))
                    return nd;
            }
            _nodelist.Add(anode);
            return anode;
        }

        /// <summary>
        /// AddResultIfNotExists method implementation
        /// </summary>
        public override bool AddResultIfNotExists(IResultObject obj)
        {
            return false;
        }

        /// <summary>
        /// Name property implementation
        /// </summary>
        public override string GetName()
        {
            return "Root"; 
        }

        /// <summary>
        /// Name property implementation
        /// </summary>
        public override string GetDisplayName()
        {
            return "Root";
        }

        /// <summary>
        /// Position property implementation
        /// </summary>
        public override int GetPosition()
        {
            return 0;
        }
    }
    #endregion

    #region ActiveDirectoryResultsNode
    public class ActiveDirectoryResultsNode : ActiveDirectoryResults, IResultsNode
    {
        private List<IResultsNode> _nodelist;
        private List<IResultObject> _results;
        private string _name;
        private string _displayname;
        private int _position;
        private bool _created = false;

        /// <summary>
        /// ActiveDirectoryResultsNode constructor
        /// </summary>
        public ActiveDirectoryResultsNode(string name, string displayname, int position)
        {
            if (!_created)
            {
                _name = name;
                _displayname = displayname;
                _position = position;
                _nodelist = new List<IResultsNode>();
                _results = new List<IResultObject>();
                _created = true;
            }
        }

        public void Initialize(string name, string displayname, int position)
        {
            if (!_created)
            {
                _name = name;
                _displayname = displayname;
                _position = position;
                _nodelist = new List<IResultsNode>();
                _results = new List<IResultObject>();
                _created = true;
            }
        }

        /// <summary>
        /// Nodes property implementation
        /// </summary>
        public override List<IResultsNode> GetNodes()
        {
            return _nodelist; 
        }

        /// <summary>
        /// Nodes property implementation
        /// </summary>
        public override List<IResultObject> GetResults()
        {
            return _results; 
        }

        /// <summary>
        /// HasResults method implementation
        /// </summary>
        public override bool HasResults()
        {
            if (_results.Count > 0)
                return true;
            else
                foreach (IResultsNode nd in _nodelist)
                {
                    if (nd.HasResults())
                        return true;
                }
            return false;
        }

        /// <summary>
        /// AddNodeIfNotExists method implementation
        /// </summary>
        public override IResultsNode AddNodeIfNotExists(IResultsNode anode)
        {
            foreach (IResultsNode nd in _nodelist)
            {
                if ((nd.GetName().ToLowerInvariant().Equals(anode.GetName().ToLowerInvariant())) && (nd.GetDisplayName().ToLowerInvariant().Equals(anode.GetDisplayName().ToLowerInvariant())))
                    return nd;
            }
            _nodelist.Add(anode);
            return anode;

        }

        /// <summary>
        /// AddResultIfNotExists method implementation
        /// </summary>
        public override bool AddResultIfNotExists(IResultObject obj)
        {
            if (_results.Count == 0)
            {
                _results.Add(obj);
                return true;
            }
            else
            {
                if (obj is IUser)
                {
                    foreach (IResultObject xobj in _results)
                    {
                        if (xobj is IUser)
                        {
                            if (((IUser)obj).UserPrincipalName == ((IUser)xobj).UserPrincipalName)
                                return false;
                        }
                    }
                }
                else if (obj is IRole)
                {
                    foreach (IResultObject xobj in _results)
                    {
                        if (xobj is IRole)
                        {
                            if (((IRole)obj).SID.ToLowerInvariant().Equals(((IRole)xobj).SID.ToLowerInvariant()))
                                return false;
                        }
                    }
                }
                _results.Add(obj);
            }
            return true;
        }

        /// <summary>
        /// GetName method implementation
        /// </summary>
        public override string GetName()
        {
            return _name; 
        }

        /// <summary>
        /// GetDisplayName method implementation
        /// </summary>
        public override string GetDisplayName()
        {
            return _displayname;
        }

        /// <summary>
        /// GetPosition method implementation
        /// </summary>
        public override int GetPosition()
        {
            return _position;
        }

    }
    #endregion

    #region ActiveDirectoryResultObject
    /// <summary>
    /// ActiveDirectoryResultObject class
    /// </summary>
    public class ActiveDirectoryResultObject: IResultObject
    {
        public string DomainName { get; set; }
        public string DomainDisplayName { get; set; }
        public virtual bool IsBuiltIn { get; set; }
        public string DisplayName { get; set; }
        public string SamAaccount { get; set; }
    }
    #endregion

    #region ActiveDirectoryUser
    /// <summary>
    /// ActiveDirectoryUser Class
    /// </summary>
    public class ActiveDirectoryUser : ActiveDirectoryResultObject, IUser
    {
        /// <summary>
        /// ActiveDirectoryUser constructor
        /// </summary>
        public ActiveDirectoryUser(): base()
        {
        }

        /// <summary>
        /// ActiveDirectoryUser constructor overload
        /// </summary>
        public ActiveDirectoryUser(ActiveDirectoryDomain dom, SearchResult sr): base()
        {
            try
            {
                using (DirectoryEntry DirEntry = sr.GetDirectoryEntry())
                {
                    DomainName = dom.DnsName;
                    DomainDisplayName = dom.DisplayName;
                    UserPrincipalName = DirEntry.Properties["userPrincipalName"].Value.ToString();
                    SamAaccount = dom.NetbiosName + "\\" + DirEntry.Properties["sAMAccountName"].Value.ToString();
                    if ((DirEntry.Properties.Contains("displayName")) && (DirEntry.Properties["displayName"].Value != null))
                        DisplayName = DirEntry.Properties["displayName"].Value.ToString();
                    if ((DirEntry.Properties.Contains("mail")) && (DirEntry.Properties["mail"].Value != null))
                        EmailAddress = DirEntry.Properties["mail"].Value.ToString();
                    if ((DirEntry.Properties.Contains("mobile")) && (DirEntry.Properties["mobile"].Value != null))
                        MobilePhone = DirEntry.Properties["mobile"].Value.ToString();
                    if ((DirEntry.Properties.Contains("telephoneNumber")) && (DirEntry.Properties["telephoneNumber"].Value != null))
                        WorkPhone = DirEntry.Properties["telephoneNumber"].Value.ToString();
                    if ((DirEntry.Properties.Contains("msRTCSIP-PrimaryUserAddress")) && (DirEntry.Properties["msRTCSIP-PrimaryUserAddress"].Value != null))
                        SIPAddress = DirEntry.Properties["msRTCSIP-PrimaryUserAddress"].Value.ToString();
                    if ((DirEntry.Properties.Contains("title")) && (DirEntry.Properties["title"].Value != null))
                        JobTitle = DirEntry.Properties["title"].Value.ToString();
                    if ((DirEntry.Properties.Contains("department")) && (DirEntry.Properties["department"].Value != null))
                        Department = DirEntry.Properties["department"].Value.ToString();
                    if ((DirEntry.Properties.Contains("physicalDeliveryOfficeName")) && (DirEntry.Properties["physicalDeliveryOfficeName"].Value != null))
                        Location = DirEntry.Properties["physicalDeliveryOfficeName"].Value.ToString();
                };
            }
            catch (Exception E)
            {
                throw new Exception(ResourcesValues.GetString("INVUSER"), E);
            }
        }

        /// <summary>
        /// UserPrincipalName property implementation
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// EmailAddress property implementation
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// IsBuiltIn property implementation
        /// </summary>
        public override bool IsBuiltIn
        {
            get { return false; }
            set { base.IsBuiltIn = false;}
        }

        /// <summary>
        /// PictureUrl property implementation
        /// </summary>
        public string PictureUrl { get; set; }

        /// <summary>
        /// JobTitle property implementation
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// Department property implementation
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Location property implementation
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// MobilePhone property implementation
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// SIPAddress property implementation
        /// </summary>
        public string SIPAddress { get; set; }

        /// <summary>
        /// WorkPhone property implementation
        /// </summary>
        public string WorkPhone { get; set; }

    }
    #endregion

    #region ActiveDirectoryRole
    /// <summary>
    /// ActiveDirectoryRole property implementation
    /// </summary>
    public class ActiveDirectoryRole : ActiveDirectoryResultObject, IRole
    {
        const int GlobalSecurityGroup = -2147483646;
        const int LocalSecurityGroup = -2147483644;
        const int BuiltInGroup = -2147483643;
        const int UniversalSecurityGroup = -2147483640;

        const int GlobalDistributionGroup = 2;
        const int LocalDistributionGroup = 4;
        const int UniversalDistributionGroup = 8;

        /// <summary>
        /// ActiveDirectoryRole constructor
        /// </summary>
        public ActiveDirectoryRole(): base()
        {
        }

        /// <summary>
        /// ActiveDirectoryRole constructor overload
        /// </summary>
        public ActiveDirectoryRole(ActiveDirectoryDomain dom, SearchResult sr): base()
        {
            try
            {
                using (DirectoryEntry DirEntry = sr.GetDirectoryEntry())
                {

                    DomainName = dom.DnsName;
                    DomainDisplayName = dom.DisplayName;
                    GUID = new Guid((byte[])DirEntry.Properties["objectGuid"].Value);
                    SID = ConvertSidToString((byte[])DirEntry.Properties["objectSid"].Value);
                    int grptype = (int)DirEntry.Properties["groupType"].Value;
                    if (DirEntry.Properties["displayName"].Value == null)
                        DisplayName = DirEntry.Properties["sAMAccountName"].Value.ToString();
                    else
                        DisplayName = DirEntry.Properties["displayName"].Value.ToString();
                    IsBuiltIn = false;
                    switch (grptype)
                    {
                        case GlobalSecurityGroup:
                            GroupScope = Core.GroupScope.Global;
                            SamAaccount = dom.NetbiosName + "\\" + DirEntry.Properties["sAMAccountName"].Value.ToString();
                            break;
                        case LocalSecurityGroup:
                            GroupScope = Core.GroupScope.Local;
                            SamAaccount = dom.NetbiosName + "\\" + DirEntry.Properties["sAMAccountName"].Value.ToString();
                            break;
                        case UniversalSecurityGroup:
                            GroupScope = Core.GroupScope.Universal;
                            SamAaccount = dom.NetbiosName + "\\" + DirEntry.Properties["sAMAccountName"].Value.ToString();
                            break;
                        case GlobalDistributionGroup:
                            GroupScope = Core.GroupScope.Global;
                            SamAaccount = dom.NetbiosName + "\\" + DirEntry.Properties["sAMAccountName"].Value.ToString();
                            break;
                        case LocalDistributionGroup:
                            GroupScope = Core.GroupScope.Local;
                            SamAaccount = dom.NetbiosName + "\\" + DirEntry.Properties["sAMAccountName"].Value.ToString();
                            break;
                        case UniversalDistributionGroup:
                            GroupScope = Core.GroupScope.Universal;
                            SamAaccount = dom.NetbiosName + "\\" + DirEntry.Properties["sAMAccountName"].Value.ToString();
                            break;
                        default:
                            GroupScope = Core.GroupScope.Builtin;
                            IsBuiltIn = true;
                            SamAaccount = "BUILTIN\\" + DirEntry.Properties["sAMAccountName"].Value.ToString();
                            break;
                    }
                };
            }
            catch (Exception E)
            {
                throw new Exception(ResourcesValues.GetString("INVGRP"), E);
            }
        }

        /// <summary>
        /// ConvertSidToString method implementation
        /// </summary>
        private string ConvertSidToString(byte[] objectSid)
        {
            SecurityIdentifier si = new SecurityIdentifier(objectSid, 0);
            return si.ToString();
        }

        /// <summary>
        /// GroupScope property implementation
        /// </summary>
        public SharePoint.IdentityService.Core.GroupScope? GroupScope { get; set; }

        /// <summary>
        /// IsSecurityGroup property implementation
        /// </summary>
        public bool? IsSecurityGroup { get; set; }

        /// <summary>
        /// GUID property implementation
        /// </summary>
        public Guid? GUID { get; set; }

        /// <summary>
        /// SID property implementation
        /// </summary>
        public string SID { get; set; }
    }
    #endregion
}
