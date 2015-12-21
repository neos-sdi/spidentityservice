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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SharePoint.IdentityService.Core;

namespace SharePoint.IdentityService.ActiveDirectory
{
    [DataContract]
    public class PersistedForests
    {
        private List<PersistedBadDomain> _baddomains;
        private List<PersistedRootDomain> _rootdomains;
        private PersistedGlobalParams _globalparams;
        private List<PersistedDomainConfig> _domainconfigs;

        /// <summary>
        /// Constructor
        /// </summary>
        public PersistedForests()
        {
            _baddomains = new List<PersistedBadDomain>();
            _rootdomains = new List<PersistedRootDomain>();
            _globalparams = new PersistedGlobalParams();
            _domainconfigs = new List<PersistedDomainConfig>();
        }

        /// <summary>
        /// RootDomains property implementation
        /// </summary>
        [DataMember]
        public List<PersistedRootDomain> RootDomains
        {
            get { return _rootdomains; }
            set { _rootdomains = value; }
        }

        /// <summary>
        /// BadDomains property implementation
        /// </summary>
        [DataMember]
        public List<PersistedBadDomain> BadDomains
        {
            get { return _baddomains; }
            set { _baddomains = value; }
        }

        /// <summary>
        /// DomainConfigs property implmentation
        /// </summary>
        [DataMember]
        public List<PersistedDomainConfig> DomainConfigurations
        {
            get { return _domainconfigs; }
            set { _domainconfigs = value; }
        }

        /// <summary>
        /// GlobalParams property implementation
        /// </summary>
        [DataMember]
        public PersistedGlobalParams GlobalParams
        {
            get { return _globalparams; }
            set { _globalparams = value; }
        }

        /// <summary>
        /// ProviderName property implementation
        /// </summary>
        [DataMember]
        public string ProviderName { get; set; }

        /// <summary>
        /// UserName property implementation
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        ///  Password property implementation
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// DefaultTimeOut property implementation
        /// </summary>
        [DataMember]
        public short DefaultTimeOut { get; set; }

        /// <summary>
        /// DefaultSuspendTime property implementation
        /// </summary>
        [DataMember]
        public short DefaultSuspendTime { get; set; }

        /// <summary>
        /// ElapsedTime property implementation
        /// </summary>
        [DataMember]
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// UsesScureConnection property implementation
        /// </summary>
        [DataMember]
        public bool UsesScureConnection { get; set; }

        /// <summary>
        /// MaxRowsPerDomain property implementation
        /// </summary>
        [DataMember]
        public int MaxRowsPerDomain { get; set; }


        public static implicit operator PersistedForests(ActiveDirectoryForests forests)
        {
            PersistedForests res = new PersistedForests();
            res.UserName = forests.UserName;
            res.Password = forests.Password;
            res.ProviderName = forests.ProviderName;
            res.UsesScureConnection = forests.UsesScureConnection;
            res.DefaultTimeOut = forests.DefaultTimeOut;
            res.DefaultSuspendTime = forests.DefaultSuspendTime;
            res.ElapsedTime = forests.ElapsedTime;
            res.MaxRowsPerDomain = forests.MaxRowsPerDomain;
            res.GlobalParams = forests.GlobalParams as ActiveDirectoryGlobalParams;
            foreach (IRootDomain ir in forests.RootDomains)
            {
                ActiveDirectoryRootDomain ar = ir as ActiveDirectoryRootDomain;
                PersistedRootDomain pr = ar;
                res.RootDomains.Add(pr);
            }
            foreach (IBadDomain ib in forests.BadDomains)
            {
                ActiveDirectoryBadDomain ab = ib as ActiveDirectoryBadDomain;
                PersistedBadDomain pb = ab;
                res.BadDomains.Add(pb);
            }
            foreach (IDomainConfig ig in forests.DomainConfigurations)
            {
                ActiveDirectoryDomainConfigurations ag = ig as ActiveDirectoryDomainConfigurations;
                PersistedDomainConfig pg = ag;
                res.DomainConfigurations.Add(pg);
            }
            return res;
        }

        public static implicit operator ActiveDirectoryForests(PersistedForests forests)
        {
            ActiveDirectoryForests res = new ActiveDirectoryForests();
            res.UserName = forests.UserName;
            res.Password = forests.Password;
            res.ProviderName = forests.ProviderName;
            res.UsesScureConnection = forests.UsesScureConnection;
            res.DefaultTimeOut = forests.DefaultTimeOut;
            res.DefaultSuspendTime = forests.DefaultSuspendTime;
            res.ElapsedTime = forests.ElapsedTime;
            res.MaxRowsPerDomain = forests.MaxRowsPerDomain;
            ActiveDirectoryGlobalParams gp = forests.GlobalParams;
            res.GlobalParams = gp as IGlobalParams;
            foreach (PersistedRootDomain ir in forests.RootDomains)
            {
                ActiveDirectoryRootDomain dr = ir;
                IRootDomain pr = dr as IRootDomain;
                res.RootDomains.Add(pr);
            }
            foreach (PersistedBadDomain ib in forests.BadDomains)
            {
                ActiveDirectoryBadDomain db = ib;
                IBadDomain pb = db as IBadDomain;
                res.BadDomains.Add(pb);
            }
            foreach (PersistedDomainConfig ig in forests.DomainConfigurations)
            {
                ActiveDirectoryDomainConfigurations dg = ig;
                IDomainConfig pg = dg as IDomainConfig;
                res.DomainConfigurations.Add(pg);
            }
            res.IsLoaded = true;
            res.IsLoadedFromCache = true;
            return res;
        }
    }

    [DataContract]
    public class PersistedRootDomain : PersistedDomain
    {
        private List<PersistedTopLevelName> _toplevelnames;

        public PersistedRootDomain()
        {
            _toplevelnames = new List<PersistedTopLevelName>();
        }
        /// <summary>
        /// TopLevelNames property implementation
        /// </summary>
        [DataMember] 
        public List<PersistedTopLevelName> TopLevelNames
        {
            get { return _toplevelnames; }
            set { _toplevelnames = value; }
        }

        public static implicit operator PersistedRootDomain(ActiveDirectoryRootDomain rootdomain)
        {
            PersistedRootDomain res = new PersistedRootDomain();
            res.ConnectString = rootdomain.ConnectString;
            res.DisplayName = rootdomain.DisplayName;
            res.DnsName = rootdomain.DnsName;
            foreach (IDomain id in rootdomain.Domains)
            {
                ActiveDirectoryDomain ad = id as ActiveDirectoryDomain;
                PersistedDomain pd = ad;
                res.Domains.Add(pd);
            }
            res.ElapsedTime = rootdomain.ElapsedTime;
            res.ErrorMessage = rootdomain.ErrorMessage;
            res.GlobalParams = (ActiveDirectoryGlobalParams)rootdomain.GlobalParams;
            res.IsMaster = rootdomain.IsMaster;
            res.IsReacheable = rootdomain.IsReacheable;
            res.IsRoot = rootdomain.IsRoot;
            res.MaxRows = rootdomain.MaxRows;
            res.NetbiosName = rootdomain.NetbiosName;
            res.Password = rootdomain.Password;
            res.Position = rootdomain.Position;
            res.Timeout = rootdomain.Timeout;
            foreach (ITopLevelName it in rootdomain.TopLevelNames)
            {
                ActiveDirectoryTopLevelName at = it as ActiveDirectoryTopLevelName;
                PersistedTopLevelName pt = at;
                res.TopLevelNames.Add(pt);
            }
            res.UserName = rootdomain.UserName;
            return res;
        }

        public static implicit operator ActiveDirectoryRootDomain(PersistedRootDomain rootdomain)
        {
            ActiveDirectoryRootDomain res = new ActiveDirectoryRootDomain();
            res.ConnectString = rootdomain.ConnectString;
            res.DisplayName = rootdomain.DisplayName;
            res.DnsName = rootdomain.DnsName;
            res.ElapsedTime = rootdomain.ElapsedTime;
            res.ErrorMessage = rootdomain.ErrorMessage;

            res.IsMaster = rootdomain.IsMaster;
            res.IsReacheable = rootdomain.IsReacheable;
            res.IsRoot = rootdomain.IsRoot;
            res.MaxRows = rootdomain.MaxRows;
            res.NetbiosName = rootdomain.NetbiosName;
            res.Password = rootdomain.Password;
            res.Position = rootdomain.Position;
            res.Timeout = rootdomain.Timeout;
            ActiveDirectoryGlobalParams gp = rootdomain.GlobalParams;
            res.GlobalParams = gp as IGlobalParams;
            foreach (PersistedDomain id in rootdomain.Domains)
            {
                ActiveDirectoryDomain dd = id;
                dd.Parent = res;
                IDomain pd = dd as IDomain;
                res.Domains.Add(pd);
            }
            foreach (PersistedTopLevelName it in rootdomain.TopLevelNames)
            {
                ActiveDirectoryTopLevelName dt = it;
                ITopLevelName pt = dt as ITopLevelName;
                res.TopLevelNames.Add(pt);
            }
            res.UserName = rootdomain.UserName;
            return res;
        }

    }

    [DataContract]
    public class PersistedDomain
    {
        private PersistedGlobalParams _globalparams;
        private List<PersistedDomain> _domains;

        /// <summary>
        /// Constructor
        /// </summary>
        public PersistedDomain()
        {
            _globalparams = new PersistedGlobalParams();
            _domains = new List<PersistedDomain>();
        }

        /// <summary>
        /// IsReacheable property implementation
        /// </summary>
        [DataMember]
        public bool IsReacheable { get; set; }

        /// <summary>
        /// ErrorMessage properety implementation
        /// </summary>
        [DataMember]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// UserName property implementation
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        ///  Password property implementation
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// IsMaster property implementation
        /// </summary>
        [DataMember]
        public bool IsMaster { get; set; }

        /// <summary>
        /// IsRoot property implementation
        /// </summary>
        [DataMember]
        public bool IsRoot { get; set; }

        /// <summary>
        /// DnsName property implementation
        /// </summary>
        [DataMember]
        public string DnsName { get; set; }

        /// <summary>
        /// DisplayName property implementation
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// SamName property implementation
        /// </summary>
        [DataMember]
        public string NetbiosName { get; set; }

        /// <summary>
        /// Domains property implementation
        /// </summary>
        [DataMember]
        public List<PersistedDomain> Domains
        {
            get { return _domains; }
            set { _domains = value; }
        }

        /// <summary>
        /// ElapsedTime property implementation
        /// </summary>
        [DataMember]
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// Timeout property implementation
        /// </summary>
        [DataMember]
        public short Timeout { get; set; }

        /// <summary>
        /// MaxRows property implementation
        /// </summary>
        [DataMember]
        public int MaxRows { get; set; }

        /// <summary>
        /// MaxRows property implementation
        /// </summary>
        [DataMember]
        public int Position { get; set; }

        /// <summary>
        /// ConnectString property implementation
        /// </summary>
        [DataMember]
        public string ConnectString { get; set; }

        /// <summary>
        /// GlobalParams property implementatioon
        /// </summary>
        [DataMember]
        public PersistedGlobalParams GlobalParams
        {
            get { return _globalparams; }
            set { _globalparams = value; }
        }

        public static implicit operator PersistedDomain(ActiveDirectoryDomain domain)
        {
            PersistedDomain res = new PersistedDomain();
            res.ConnectString = domain.ConnectString;
            res.DisplayName = domain.DisplayName;
            res.DnsName = domain.DnsName;
            foreach (IDomain id in domain.Domains)
            {
                ActiveDirectoryDomain ad = id as ActiveDirectoryDomain;
                PersistedDomain pd = ad;
                res.Domains.Add(pd);
            }
            res.ElapsedTime = domain.ElapsedTime;
            res.ErrorMessage = domain.ErrorMessage;
            res.GlobalParams = domain.GlobalParams as ActiveDirectoryGlobalParams;
            res.IsMaster = domain.IsMaster;
            res.IsReacheable = domain.IsReacheable;
            res.IsRoot = domain.IsRoot;
            res.MaxRows = domain.MaxRows;
            res.NetbiosName = domain.NetbiosName;
            res.Password = domain.Password;
            res.Position = domain.Position;
            res.Timeout = domain.Timeout;
            res.UserName = domain.UserName;
            return res;
        }

        public static implicit operator ActiveDirectoryDomain(PersistedDomain domain)
        {
            ActiveDirectoryDomain res = new ActiveDirectoryDomain();
            res.ConnectString = domain.ConnectString;
            res.DisplayName = domain.DisplayName;
            res.DnsName = domain.DnsName;
            res.ElapsedTime = domain.ElapsedTime;
            res.ErrorMessage = domain.ErrorMessage;
            res.IsMaster = domain.IsMaster;
            res.IsReacheable = domain.IsReacheable;
            res.IsRoot = domain.IsRoot;
            res.MaxRows = domain.MaxRows;
            res.NetbiosName = domain.NetbiosName;
            res.Password = domain.Password;
            res.Position = domain.Position;
            res.Timeout = domain.Timeout;
            res.UserName = domain.UserName;
            ActiveDirectoryGlobalParams gp = domain.GlobalParams;
            res.GlobalParams = gp as IGlobalParams;
            foreach (PersistedDomain id in domain.Domains)
            {
                ActiveDirectoryDomain dd = id;
                dd.Parent = res;
                IDomain pd = dd as IDomain;
                res.Domains.Add(pd);
            }
            return res;
        }
    }

    [DataContract]
    public class PersistedBadDomain
    {
        /// <summary>
        /// DnsName property implementation
        /// </summary>
        [DataMember]
        public string DnsName { get; set; }

        /// <summary>
        /// Message property implmentation
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// ElapsedTime property implementation
        /// </summary>
        [DataMember]
        public TimeSpan ElapsedTime { get; set; }

        public static implicit operator PersistedBadDomain(ActiveDirectoryBadDomain baddomain)
        {
            PersistedBadDomain res = new PersistedBadDomain();
            res.DnsName = baddomain.DnsName;
            res.ElapsedTime = baddomain.ElapsedTime;
            res.Message = baddomain.Message;
            return res;
        }

        public static implicit operator ActiveDirectoryBadDomain(PersistedBadDomain baddomain)
        {
            ActiveDirectoryBadDomain res = new ActiveDirectoryBadDomain();
            res.DnsName = baddomain.DnsName;
            res.ElapsedTime = baddomain.ElapsedTime;
            res.Message = baddomain.Message;
            return res;
        }
    }

    [DataContract]
    public class PersistedGlobalParams
    {
        /// <summary>
        /// SmoothRequestor property implementation
        /// </summary>
        [DataMember]
        public ProxySmoothRequest SmoothRequestor { get; set; }

        /// <summary>
        /// ClaimsMode property implementation
        /// </summary>
        [DataMember]
        public ProxyClaimsMode ClaimsMode { get; set; }

        /// <summary>
        /// ClaimsDisplayMode  property implemtation
        /// </summary>
        [DataMember]
        public ProxyClaimsDisplayMode ClaimsDisplayMode { get; set; }

        /// <summary>
        /// PeoplePickerDisplayMode property implemtation
        /// </summary>
        [DataMember]
        public ProxyClaimsDisplayMode PeoplePickerDisplayMode { get; set; }

        /// <summary>
        /// SearchByMail property implementation
        /// </summary>
        [DataMember]
        public bool SearchByMail { get; set; }

        /// <summary>
        /// SearchByDisplayName property implementation 
        /// </summary>
        [DataMember]
        public bool SearchByDisplayName { get; set; }

        /// <summary>
        /// SearchByDisplayName property implementation 
        /// </summary>
        [DataMember]
        public bool Trace { get; set; }

        /// <summary>
        /// PeoplePickerImages property implementation
        /// </summary>
        [DataMember]
        public bool PeoplePickerImages { get; set; }

        /// <summary>
        /// ShowSystemNodes property implementation
        /// </summary>
        [DataMember]
        public bool ShowSystemNodes { get; set; }

        public static implicit operator PersistedGlobalParams(ActiveDirectoryGlobalParams glbparams)
        {
            PersistedGlobalParams glb = new PersistedGlobalParams();
            glb.ClaimsDisplayMode = glbparams.ClaimsDisplayMode;
            glb.ClaimsMode = glbparams.ClaimsMode;
            glb.PeoplePickerDisplayMode = glbparams.PeoplePickerDisplayMode;
            glb.PeoplePickerImages = glbparams.PeoplePickerImages;
            glb.SearchByDisplayName = glbparams.SearchByDisplayName;
            glb.SearchByMail = glbparams.SearchByMail;
            glb.ShowSystemNodes = glbparams.ShowSystemNodes;
            glb.SmoothRequestor = glbparams.SmoothRequestor;
            glb.Trace = glbparams.Trace;
            return glb;
        }

        public static implicit operator ActiveDirectoryGlobalParams(PersistedGlobalParams glbparams)
        {
            ActiveDirectoryGlobalParams glb = new ActiveDirectoryGlobalParams();
            glb.ClaimsDisplayMode = glbparams.ClaimsDisplayMode;
            glb.ClaimsMode = glbparams.ClaimsMode;
            glb.PeoplePickerDisplayMode = glbparams.PeoplePickerDisplayMode;
            glb.PeoplePickerImages = glbparams.PeoplePickerImages;
            glb.SearchByDisplayName = glbparams.SearchByDisplayName;
            glb.SearchByMail = glbparams.SearchByMail;
            glb.ShowSystemNodes = glbparams.ShowSystemNodes;
            glb.SmoothRequestor = glbparams.SmoothRequestor;
            glb.Trace = glbparams.Trace;
            return glb;
        }
    }

    [DataContract]
    public class PersistedTopLevelName
    {
        /// <summary>
        /// TopLevelName property implementation
        /// </summary>
        [DataMember]
        public string TopLevelName { get; set; }

        /// <summary>
        /// Status property implementation
        /// </summary>
        [DataMember]
        public TopLevelNameStatus Status { get; set; }

        public static implicit operator PersistedTopLevelName(ActiveDirectoryTopLevelName toplevel)
        {
            PersistedTopLevelName res = new PersistedTopLevelName();
            res.TopLevelName = toplevel.TopLevelName;
            res.Status = toplevel.Status;
            return res;
        }

        public static implicit operator ActiveDirectoryTopLevelName(PersistedTopLevelName toplevel)
        {
            ActiveDirectoryTopLevelName res = new ActiveDirectoryTopLevelName();
            res.TopLevelName = toplevel.TopLevelName;
            res.Status = toplevel.Status;
            return res;
        }
    }

    [DataContract]
    public class PersistedDomainConfig
    {
        /// <summary>
        /// DomainName property implementation
        /// </summary>
        [DataMember]
        public string DomainName { get; set; }

        /// <summary>
        /// DisplayName property implementation
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// UserName property implementation
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// Password property implementation
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Timeout property implementation
        /// </summary>
        [DataMember]
        public short Timeout { get; set; }

        /// <summary>
        /// Enabled property implementation
        /// </summary>
        [DataMember]
        public bool Enabled { get; set; }

        /// <summary>
        /// SecureConnection property implementation
        /// </summary>
        [DataMember]
        public bool SecureConnection { get; set; }

        /// <summary>
        /// MaxRows property implementation
        /// </summary>
        [DataMember]
        public int MaxRows { get; set; }

        /// <summary>
        /// Position property implementation
        /// </summary>
        [DataMember]
        public int Position { get; set; }

        /// <summary>
        /// ConnectString property implementation
        /// </summary>
        [DataMember]
        public string ConnectString { get; set; }

        public static implicit operator PersistedDomainConfig(ActiveDirectoryDomainConfigurations config)
        {
            PersistedDomainConfig res = new PersistedDomainConfig();
            res.ConnectString = config.ConnectString;
            res.DisplayName = config.DisplayName;
            res.DomainName = config.DomainName;
            res.MaxRows = config.MaxRows;
            res.Password = config.Password;
            res.Position = config.Position;
            res.SecureConnection = config.SecureConnection;
            res.Timeout = config.Timeout;
            res.UserName = config.UserName;
            return res;
        }

        public static implicit operator ActiveDirectoryDomainConfigurations(PersistedDomainConfig config)
        {
            ActiveDirectoryDomainConfigurations res = new ActiveDirectoryDomainConfigurations();
            res.ConnectString = config.ConnectString;
            res.DisplayName = config.DisplayName;
            res.DomainName = config.DomainName;
            res.MaxRows = config.MaxRows;
            res.Password = config.Password;
            res.Position = config.Position;
            res.SecureConnection = config.SecureConnection;
            res.Timeout = config.Timeout;
            res.UserName = config.UserName;
            return res;
        }
    }
}
