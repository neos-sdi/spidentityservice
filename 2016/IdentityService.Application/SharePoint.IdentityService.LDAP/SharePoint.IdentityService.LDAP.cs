using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SharePoint.IdentityService.Core;
using System.DirectoryServices;
using System.Text.RegularExpressions;

namespace SharePoint.IdentityService.LDAP
{
    public class LDAPWrapper : IWrapper
    {

        private string _ldapuser;
        private string _ldappwd;
        private short _defaulttimeout = 30;
        private bool _usesecureconnection = false;
        private int _maxrowsperdomain = 200;
        private LDAPGlobalParams _adprm;
        private bool _isloaded = false;
        private string _providername;
        private static object lokobj = new Object();

        private List<IDomainConfig> _domainconfigs;
        private List<ProxyBadDomain> _baddomains;
        private List<ProxyDomain> _domains;
        private Int64 _connectorid = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        public LDAPWrapper()
        {
            _domainconfigs = new List<IDomainConfig>();
            _baddomains = new List<ProxyBadDomain>();
            _domains = new List<ProxyDomain>();
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(List<ProxyFullConfiguration> AllData, List<ProxyGeneralParameter> AllParams)
        {
            foreach (ProxyFullConfiguration f in AllData)
            {
                if (f.IsDefault)
                {
                    _ldapuser = f.UserName;
                    _ldappwd = f.Password;
                    _defaulttimeout = f.Timeout;
                    _usesecureconnection = f.Secure;
                    _maxrowsperdomain = f.Maxrows;
                }
                this.DomainConfigurations.Add(new LDAPDomainConfigurations(f.DnsName, f.DisplayName, f.UserName, f.Password, f.Timeout, f.Enabled, f.Secure, f.Maxrows, f.DisplayPosition, f.ConnectString));
            }
            _adprm = new LDAPGlobalParams();
            _adprm.ClaimProviderSupportsUserKey = true; // Set Default if Database not Upgraded
            foreach (ProxyGeneralParameter p in AllParams)
            {
                if (p.ParamName.ToLower().Trim().Equals("smoothrequestor"))
                {
                    GlobalParams.SmoothRequestor = (ProxySmoothRequest)Enum.Parse(typeof(ProxySmoothRequest), p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("claimsmode"))
                {
                    GlobalParams.ClaimsMode = (ProxyClaimsMode)Enum.Parse(typeof(ProxyClaimsMode), p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("claimsdisplaymode"))
                {
                    GlobalParams.ClaimsDisplayMode = (ProxyClaimsDisplayMode)Enum.Parse(typeof(ProxyClaimsDisplayMode), p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("peoplepickerdisplaymode"))
                {
                    GlobalParams.PeoplePickerDisplayMode = (ProxyClaimsDisplayMode)Enum.Parse(typeof(ProxyClaimsDisplayMode), p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("searchbymail"))
                {
                    GlobalParams.SearchByMail = bool.Parse(p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("searchbydisplayname"))
                {
                    GlobalParams.SearchByDisplayName = bool.Parse(p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("traceresolve"))
                {
                    GlobalParams.Trace = bool.Parse(p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("peoplepickerimages"))
                {
                    GlobalParams.PeoplePickerImages = bool.Parse(p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("showsystemnodes"))
                {
                    GlobalParams.ShowSystemNodes = bool.Parse(p.ParamValue);
                }
                else if (p.ParamName.ToLower().Trim().Equals("supportsuserkey"))
                {
                    GlobalParams.ClaimProviderSupportsUserKey = bool.Parse(p.ParamValue);
                }
            }
        }

        /// <summary>
        /// DomainConfigs property implmentation
        /// </summary>
        private List<IDomainConfig> DomainConfigurations
        {
            get { return _domainconfigs; }
        }


        public LDAPGlobalParams GlobalParams
        {
            get { return _adprm; }
        }

        /// <summary>
        /// Domains property implementation
        /// </summary>
        public List<ProxyDomain> Domains
        {
            get { return _domains; }
        }

        /// <summary>
        /// FillBadDomains method implementation
        /// </summary>
        public List<ProxyBadDomain> FillBadDomains()
        {
            return _baddomains;
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
        /// ClaimsProviderName property implementation
        /// </summary>
        public string ClaimsProviderName
        {
            get { return _providername; }
            set { _providername = value; }
        }

        /// <summary>
        /// ConnectorID property implementation
        /// </summary>
        public Int64 ConnectorID
        {
            get { return _connectorid; }
            set { _connectorid = value; }
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
        /// FillHierarchy method implementation
        /// </summary>
        public ProxyDomain FillHierarchy(string hierarchyNodeID, int numberOfLevels)
        {
            ProxyDomain results = new ProxyDomain();
           // results.ElapsedTime = ElapsedTime;
            results.IsReacheable = true;
            results.IsRoot = true;
            results.DnsName = "Root";
            results.DisplayName = "Root";
            if (string.IsNullOrEmpty(hierarchyNodeID))
            {
                foreach (ProxyDomain d in Domains)
                {
                    DoFillHierachy(results, d, 1, numberOfLevels);
                }
            }
            else
            {
                List<ProxyDomain> dom = GetDomain(hierarchyNodeID);
                if (dom != null)
                {
                    foreach (ProxyDomain dm in dom)
                    {
                        foreach (ProxyDomain d in dm.Domains)
                        {
                            DoFillHierachy(results, d, 1, numberOfLevels);
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// FillResolve method implementation
        /// </summary>
        public ProxyResults FillResolve(string pattern, bool recursive)
        {
            if (string.IsNullOrEmpty(pattern))
                return null;
            EnsureLoaded();
            ProxyResults results = new ProxyResults();

            DoFillResolve(results, pattern, false);
            return results;
        }

        /// <summary>
        /// FillSearch method implementation
        /// </summary>
        public ProxyResults FillSearch(string pattern, string domain, bool recursive)
        {
            if (string.IsNullOrEmpty(pattern))
                return null;
            EnsureLoaded();
            ProxyResults results = new ProxyResults();

            DoFillSearch(results, domain ,pattern, false);
            return results;
        }

        public ProxyResults FillValidate(string pattern, bool recursive)
        {
            if (string.IsNullOrEmpty(pattern))
                return null;
            EnsureLoaded();
            ProxyResults results = new ProxyResults();

            DoFillValidate(results, pattern, false);
            return results;

        }

        /// <summary>
        /// LaunchStartCommand method implementation
        /// </summary>
        public void LaunchStartCommand()
        {
            try
            {
                EnsureLoaded();
            }
            catch (Exception E)
            {
                LogEvent.Log(E, ResourcesValues.GetString("E20007"), EventLogEntryType.Error, 20007);
            }
        }

        /// <summary>
        /// Reload method implementation
        /// </summary>
        public void Reload()
        {
            DateTime db = DateTime.Now;
            Trace(string.Format(ResourcesValues.GetString("E1900"), this.ClaimsProviderName), EventLogEntryType.Information, 1900);
            lock (this)
            {
                _isloaded = false;
                LoadDomains();
            }
            TimeSpan _e = DateTime.Now.Subtract(db);
            Trace(string.Format(ResourcesValues.GetString("E1900B"), this.ClaimsProviderName, _e.Minutes, _e.Seconds, _e.Milliseconds), EventLogEntryType.Information, 1900);
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

        /// <summary>
        /// ConfigureSearcherForUsers method implementation
        /// </summary>
        private void ConfigureSearcherForUsers(DirectorySearcher src, int maxrows, int timeout)
        {
            src.SizeLimit = maxrows;
            src.ClientTimeout = new TimeSpan(0, 0, Convert.ToInt32(timeout));
            src.SearchScope = SearchScope.Subtree;
            src.PropertiesToLoad.Clear();
            src.PropertiesToLoad.Add("uid");
            src.PropertiesToLoad.Add("mail");
            src.PropertiesToLoad.Add("sn");
            src.PropertiesToLoad.Add("givenName");
            src.PropertiesToLoad.Add("cn");
        }

        /// <summary>
        /// LoadDomains method implementation
        /// </summary>
        private void LoadDomains()
        {
            DateTime db = DateTime.Now;
            this._baddomains.Clear();

            foreach (IDomainConfig dom in this.DomainConfigurations)
            {
                if (!dom.Enabled)
                    throw new Exception(string.Format("Domain {0} is disabled !", dom.DomainName));
                string ldapPath = string.Empty;
                string ldapbaseDN = dom.ConnectString; // ldap://hostname:port/base DN
                AuthenticationTypes auth = AuthenticationTypes.None;
                if (dom.SecureConnection)
                {
                    ldapPath = string.Format("LDAP://{0}:686/{1}", dom.DomainName, ldapbaseDN);
                    auth = AuthenticationTypes.Secure;
                }
                else
                {
                    ldapPath = string.Format("LDAP://{0}:389/{1}", dom.DomainName, ldapbaseDN);
                    auth = AuthenticationTypes.None;
                }
                string ldapquery = "cn=*";
                DirectoryEntry ldapDirectory = null;
                DirectorySearcher ldapDirectorySearcher;
                SearchResultCollection ldapSearchResult;
                try
                {
                    ldapDirectory = new DirectoryEntry(ldapPath, dom.UserName, dom.Password, auth);
                    ldapDirectorySearcher = new DirectorySearcher(ldapDirectory, ldapquery);
                    ConfigureSearcherForUsers(ldapDirectorySearcher, 1, dom.Timeout);
                    ldapSearchResult = ldapDirectorySearcher.FindAll();
                    if (ldapSearchResult.Count != 1)
                        throw new Exception("Invalid domain !");
                    ProxyDomain xdom = new ProxyDomain();
                    xdom.DisplayName = dom.DisplayName;
                    xdom.DnsName = dom.DomainName;
                    xdom.ElapsedTime = DateTime.Now.Subtract(db);
                    xdom.IsReacheable = true;
                    xdom.IsRoot = true;
                    xdom.Position = dom.Position;
                    this.Domains.Add(xdom);
                }
                catch (Exception Ex)
                {
                    ProxyBadDomain bd = new ProxyBadDomain();
                    bd.DnsName = dom.DomainName;
                    bd.Message = string.Format("This root domain {0} is administratively Disabled : {1} ", dom.DomainName, Ex.Message);
                    bd.ElapsedTime = DateTime.Now.Subtract(db);
                    this._baddomains.Add(bd);
                }
            }
            _isloaded = true;
        }

        /// <summary>
        /// GetDomainEntry method implementation
        /// </summary>
        private DirectoryEntry GetDomainEntry(ProxyDomain dom, out int timeout, out int maxrows)
        {
            string ldapPath = string.Empty;
            string ldapbaseDN = string.Empty;
            AuthenticationTypes auth = AuthenticationTypes.None;
            timeout = 30;
            maxrows = 200;

            foreach (IDomainConfig cfg in this.DomainConfigurations)
            {
                if ((cfg.DomainName == dom.DnsName) && (cfg.DisplayName == dom.DisplayName))
                {
                    if (!cfg.Enabled)
                        return null;
                    ldapPath = string.Empty;
                    ldapbaseDN = cfg.ConnectString; 
                    if (cfg.SecureConnection)
                    {
                        ldapPath = string.Format("LDAP://{0}:686/{1}", cfg.DomainName, ldapbaseDN);
                        auth = AuthenticationTypes.Secure;
                    }
                    else
                    {
                        ldapPath = string.Format("LDAP://{0}:389/{1}", cfg.DomainName, ldapbaseDN);
                        auth = AuthenticationTypes.None;
                    }
                    timeout = cfg.Timeout;
                    maxrows = cfg.MaxRows;
                    return new DirectoryEntry(ldapPath, cfg.UserName, cfg.Password, auth); ;
                }
            }
            return null;
        }

        /// <summary>
        /// DoFillSearch method implementation 
        /// </summary>
        private void DoFillSearch(ProxyResults lst, string sdom, string searchPattern, bool recursive = true)
        {
            int timeout = 0;
            int maxrows = 0;
            if (string.IsNullOrEmpty(searchPattern))
                return;
            List<ProxyDomain> lstdom = null;
            LDAPInspectValues inspect = LDAPRegEx.Parse(searchPattern);
            if (!string.IsNullOrEmpty(sdom))
            {
                lstdom = GetScopedDomain(sdom, inspect);
                if (lstdom == null)
                    return;
            }
            else
                lstdom = this.Domains;
            foreach (ProxyDomain dom in lstdom)
            {
                DirectoryEntry domain = GetDomainEntry(dom, out timeout, out maxrows);
                if (domain == null)
                    return;
                try
                {
                    DateTime db = DateTime.Now;

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

                    // Load Users anyway
                    try
                    {
                        string qryldap = "(|";
                        if (inspect.IsUPNForm())
                            qryldap += "(uid=" + leadstar + inspect.Pattern + endstar + ")";
                        if (this.GlobalParams.SearchByDisplayName)
                            qryldap += "(cn=" + leadstar + searchPattern + endstar + ")";
                        if (this.GlobalParams.SearchByMail)
                            qryldap += "(mail=" + leadstar + searchPattern + endstar + ")";
                        qryldap += ")";

                        using (DirectorySearcher dsusr = new DirectorySearcher(domain, qryldap))
                        {
                            ProxyResultsNode nd = null;
                            ConfigureSearcherForUsers(dsusr, maxrows, timeout);
                            using (SearchResultCollection resultsusr = dsusr.FindAll())
                            {

                                foreach (SearchResult sr in resultsusr)
                                {
                                    try
                                    {
                                        if (nd == null)
                                        {
                                            nd = CreateProxyNode(dom.DnsName, dom.DisplayName, dom.Position);
                                            AddNodeIfNotExists(lst, nd);
                                        }
                                        AddResultIfNotExists(nd, CreateProxyUser(dom, sr));
                                    }
                                    catch (Exception E)
                                    {
                                        if (this.GlobalParams.Trace)
                                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2002"), sr.Path, dom.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2002);
                                    }
                                }
                            };
                        };
                    }
                    catch (Exception E)
                    {
                        if (this.GlobalParams.Trace)
                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2000"), dom.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2000);
                    }
                }
                catch (Exception E)
                {
                    LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2001"), dom.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2001);
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
        /// DoFillResolve method implmentation
        /// </summary>
        private void DoFillResolve(ProxyResults lst, string searchPattern, bool recursive)
        {
            int timeout = 0;
            int maxrows = 0;
            if (string.IsNullOrEmpty(searchPattern))
                return;
            foreach (ProxyDomain dom in this.Domains)
            {
                DirectoryEntry domain = GetDomainEntry(dom, out timeout, out maxrows);
                if (domain == null)
                    return;
                try
                {
                    DateTime db = DateTime.Now;
                    LDAPInspectValues inspect = LDAPRegEx.Parse(searchPattern);

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

                    // Load Users anyway
                    try
                    {
                        string qryldap = "(|";
                        if (inspect.IsUPNForm())
                            qryldap += "(uid=" + leadstar + inspect.Pattern + endstar + ")";
                        if (this.GlobalParams.SearchByDisplayName)
                            qryldap += "(cn=" + leadstar + searchPattern + endstar + ")";
                        if (this.GlobalParams.SearchByMail)
                            qryldap += "(mail=" + leadstar + searchPattern + endstar + ")";
                        qryldap += ")";

                        using (DirectorySearcher dsusr = new DirectorySearcher(domain, qryldap))
                        {
                            ProxyResultsNode nd = null;
                            ConfigureSearcherForUsers(dsusr, maxrows, timeout);
                            using (SearchResultCollection resultsusr = dsusr.FindAll())
                            {
                                List<ProxyUser> babes = new List<ProxyUser>();
                                foreach (SearchResult sr in resultsusr)
                                {
                                    try
                                    {
                                        ProxyUser babe = CreateProxyUser(dom, sr);
                                        if (CheckUserBabe(babe, inspect))
                                        {
                                            babes.Add(babe);
                                        }
                                    }
                                    catch (Exception E)
                                    {
                                        if (this.GlobalParams.Trace)
                                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2502C"), sr.Path, dom.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2502);
                                    }
                                }
                                if (babes.Count == 0)
                                {
                                    foreach (SearchResult sr in resultsusr)
                                    {
                                        try
                                        {
                                            if (nd == null)
                                            {
                                                nd = CreateProxyNode(dom.DnsName, dom.DisplayName, dom.Position);
                                                AddNodeIfNotExists(lst, nd);
                                            }
                                            AddResultIfNotExists(nd, CreateProxyUser(dom, sr));
                                        }
                                        catch (Exception E)
                                        {
                                            if (this.GlobalParams.Trace)
                                                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2502C"), sr.Path, dom.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2502);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (ProxyUser babe in babes)
                                    {
                                        if (nd == null)
                                            AddNodeIfNotExists(lst, CreateProxyNode(dom.DnsName, dom.DisplayName, dom.Position));
                                        AddResultIfNotExists(nd, babe);
                                    }
                                }
                            };
                        };
                    }
                    catch (Exception E)
                    {
                        if (this.GlobalParams.Trace)
                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2500B"), dom.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2500);
                    }
                }
                catch (Exception E)
                {
                    LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2501B"), dom.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2501);
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
        /// DFillValidate method implementation
        /// </summary>
        private void DoFillValidate(ProxyResults lst, string searchPattern, bool recursive = true)
        {
            int timeout = 0;
            int maxrows = 0;
            if (string.IsNullOrEmpty(searchPattern))
                return;
            foreach (ProxyDomain dom in this.Domains)
            {
                DirectoryEntry domain = GetDomainEntry(dom, out timeout, out maxrows);
                if (domain == null)
                    return;
                try
                {
                    DateTime db = DateTime.Now;
                    LDAPInspectValues inspect = LDAPRegEx.Parse(searchPattern);

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

                    // Load Users anyway
                    try
                    { 
                        string qryldap = "(|";
                        if (inspect.IsUPNForm())
                            qryldap += "(uid=" + leadstar + inspect.Pattern + endstar + ")";
                        if (this.GlobalParams.SearchByDisplayName)
                            qryldap += "(cn=" + leadstar + searchPattern + endstar + ")";
                        if (this.GlobalParams.SearchByMail)
                            qryldap += "(mail=" + leadstar + searchPattern + endstar + ")";
                        qryldap += ")";

                        using (DirectorySearcher dsusr = new DirectorySearcher(domain, qryldap))
                        {
                            ProxyResultsNode nd = null;
                            ConfigureSearcherForUsers(dsusr, maxrows, timeout);

                            SearchResult resultsusr = dsusr.FindOne();
                            if (resultsusr != null)
                            {
                                try
                                {
                                    nd = CreateProxyNode(dom.DnsName, dom.DisplayName, dom.Position);
                                    AddNodeIfNotExists(lst, nd);
                                    AddResultIfNotExists(nd, CreateProxyUser(dom, resultsusr));
                                }
                                catch (Exception E)
                                {
                                    if (this.GlobalParams.Trace)
                                        LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2502C"), resultsusr.Path, dom.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2502);
                                }
                            }
                        };
                    }
                    catch (Exception E)
                    {
                        if (this.GlobalParams.Trace)
                            LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2500B"), dom.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2500);
                    }
                }
                catch (Exception E)
                {
                    LogEvent.Log(E, string.Format(ResourcesValues.GetString("E2501B"), dom.DnsName, searchPattern), System.Diagnostics.EventLogEntryType.Warning, 2501);
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
        /// DoFillHierachy method implementation
        /// </summary>
        private void DoFillHierachy(ProxyDomain dom, ProxyDomain idom, int countLevels, int numberOfLevels)
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
            foreach (ProxyDomain d in idom.Domains)
            {
                DoFillHierachy(temp, d, countLevels + 1, numberOfLevels);
            }
        }

        #region utilities methods
        /// <summary>
        /// CheckUserBabe method implementation
        /// </summary>
        private bool CheckUserBabe(ProxyUser babe, LDAPInspectValues inspect)
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
            return false;
        }

        /// <summary>
        /// AddResultIfNotExists method implementation
        /// </summary>
        private bool AddResultIfNotExists(ProxyResults lst, ProxyResultObject obj)
        {
            if (!lst.HasResults)
            {
                lst.Results.Add(obj);
                lst.HasResults = true;
                return true;
            }
            else
            {
                if (obj is ProxyUser)
                {
                    foreach (ProxyResultObject xobj in lst.Results)
                    {
                        if (xobj is ProxyUser)
                        {
                            if (((ProxyUser)obj).UserPrincipalName == ((ProxyUser)xobj).UserPrincipalName)
                                return false;
                        }
                    }
                    lst.Results.Add(obj);
                    lst.HasResults = true;
                }
            }
            return true;
        }

        /// <summary>
        /// AddNodeIfNotExists method implementation
        /// </summary>
        private void AddNodeIfNotExists(ProxyResults lst, ProxyResultsNode anode)
        {
            foreach (ProxyResultsNode nd in lst.Nodes)
            {
                if ((nd.Name.ToLowerInvariant().Equals(anode.Name.ToLowerInvariant())) && (nd.DisplayName.ToLowerInvariant().Equals(anode.DisplayName.ToLowerInvariant())))
                    return;
            }
            lst.Nodes.Add(anode);
            return;
        }

        /// <summary>
        /// ActiveDirectoryUser constructor overload
        /// </summary>
        public ProxyUser CreateProxyUser(ProxyDomain dom, SearchResult sr)
        {
            ProxyUser usr = null;
            try
            {
                using (DirectoryEntry DirEntry = sr.GetDirectoryEntry())
                {
                    usr = new ProxyUser();
                    usr.DomainName = dom.DnsName;
                    usr.DomainDisplayName = dom.DisplayName;
                    if ((DirEntry.Properties.Contains("upn")) && (DirEntry.Properties["upn"].Value != null))
                        usr.UserPrincipalName = DirEntry.Properties["upn"].Value.ToString();
                    else if ((DirEntry.Properties.Contains("uid")) && (DirEntry.Properties["uid"].Value != null))
                        usr.UserPrincipalName = DirEntry.Properties["uid"].Value.ToString();
                    if ((!string.IsNullOrEmpty(usr.UserPrincipalName) && (!usr.UserPrincipalName.Contains('@'))))
                        usr.UserPrincipalName = usr.UserPrincipalName + "@" + dom.DnsName;
                    if ((DirEntry.Properties.Contains("displayName")) && (DirEntry.Properties["displayName"].Value != null))
                        usr.DisplayName = DirEntry.Properties["displayName"].Value.ToString();
                    if ((DirEntry.Properties.Contains("mail")) && (DirEntry.Properties["mail"].Value != null))
                        usr.EmailAddress = DirEntry.Properties["mail"].Value.ToString();

                };
            }
            catch (Exception E)
            {
                throw new Exception(ResourcesValues.GetString("INVUSER"), E);
            }
            return usr;
        }

        /// <summary>
        /// CreateProxyNode method implementation
        /// </summary>
        private ProxyResultsNode CreateProxyNode(string dnsName, string displayName, int position)
        {
            ProxyResultsNode node = new ProxyResultsNode();
            node.Name = dnsName;
            node.DisplayName = displayName;
            node.Position = position;
            return node;
        }
        #endregion

        /// <summary>
        /// GetDomain method implementation
        /// </summary>
        public List<ProxyDomain> GetDomain(string domain)
        {
            List<ProxyDomain> dta = FindScopedDomain(null, domain);
            if (dta == null)
                return null;
            else
                return dta;
        }

        /// <summary>
        /// GetScopedDomain method implementation
        /// </summary>
        public List<ProxyDomain> GetScopedDomain(string scope, LDAPInspectValues reg = null)
        {
            if ((reg != null) && (reg.HasDomain))
                scope = reg.DomainPart;
            List<ProxyDomain> dta = FindScopedDomain(null, scope);
            if (dta == null)
                return null;
            else
                return dta;
        }

        /// <summary>
        /// FindScopedDomain method implementation
        /// </summary>
        private List<ProxyDomain> FindScopedDomain(ProxyDomain root, string scope)
        {
            if (string.IsNullOrEmpty(scope))
                return null;
            List<ProxyDomain> lst = new List<ProxyDomain>();
            if (root == null)
            {
                // finding scope
                foreach (ProxyDomain d in this.Domains)
                {
                    bool found = false;
                    if (d.IsReacheable)
                    {
                        if (scope.ToLowerInvariant().Equals(d.DisplayName.ToLowerInvariant()))
                            found = true;
                        else if (scope.ToLowerInvariant().Equals(d.DnsName.ToLowerInvariant()))
                            found = true;
                        if (found)
                            lst.Add(d);
                        else
                        {
                            List<ProxyDomain> ad = FindScopedDomain(d, scope);
                            if (ad != null)
                                return ad;
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
                foreach (ProxyDomain d in root.Domains)
                {
                    bool found = false;
                    if (d.IsReacheable)
                    {
                        if (scope.ToLowerInvariant().Equals(d.DisplayName.ToLowerInvariant()))
                            found = true;
                        else if (scope.ToLowerInvariant().Equals(d.DnsName.ToLowerInvariant()))
                            found = true;
                        if (found)
                            lst.Add(d);
                        else
                        {
                            List<ProxyDomain> ad = FindScopedDomain(d, scope);
                            if (ad != null)
                                return ad;
                        }
                    }
                }
                if (lst.Count > 0)
                    return lst;
            }
            return null;
        }
    }

    #region LDAPUserSearchMode enumeration
    [Flags]
    public enum LDAPUserSearchMode
    {
        AllOptions = 0,
        UserPrincipalName = 1,
        DisplayName = 2,
    }
    #endregion

    #region LDAPInspectValues class
    public class LDAPInspectValues
    {
        public LDAPUserSearchMode Mode = LDAPUserSearchMode.AllOptions;
        public string DomainPart;
        public string UserNamePart;
        public string Pattern;
        public bool Tagged = false;
        public bool IsLocal = false;

        /// <summary>
        /// HasDomain property implementation
        /// </summary>
        public bool HasDomain
        {
            get { return (!string.IsNullOrEmpty(DomainPart)) && Tagged && (!DomainPart.ToLowerInvariant().Equals("builtin")); }
        }

        /// <summary>
        /// IsUPNForm property implementation
        /// </summary>
        public bool IsUPNForm(bool strict = true)
        {
            if (strict)
                return (((Mode & LDAPUserSearchMode.UserPrincipalName) == LDAPUserSearchMode.UserPrincipalName));
            else
                return (((Mode & LDAPUserSearchMode.UserPrincipalName) == LDAPUserSearchMode.UserPrincipalName) || (Mode == LDAPUserSearchMode.AllOptions));
        }

        /// <summary>
        /// IsDisplayForm property implementation
        /// </summary>
        public bool IsDisplayForm(bool strict = true)
        {
            if (strict)
                return (((Mode & LDAPUserSearchMode.DisplayName) == LDAPUserSearchMode.DisplayName));
            else
                return (((Mode & LDAPUserSearchMode.DisplayName) == LDAPUserSearchMode.DisplayName) || (Mode == LDAPUserSearchMode.AllOptions));
        }

        /// <summary>
        /// IsAllOptions 
        /// </summary>
        public bool IsAllOptions()
        {
            return (Mode == LDAPUserSearchMode.AllOptions);
        }

        /// <summary>
        /// CheckDomain
        /// </summary>
        public bool CheckDomain(ProxyDomain domain)
        {
            bool match = false;
            if (HasDomain)
            {
                string g = DomainPart.ToLowerInvariant();
                if (g.StartsWith("*"))
                    g = g.TrimStart('*');
                match = (domain.DnsName.ToLowerInvariant().EndsWith(g));
                return match;
            }
            else
                return false;
        }
    }
    #endregion

    #region ActiveDirectoryRegEx
    public static class LDAPRegEx
    {
        private static string upnpattern = @"^(?![\x20.]+$)([^\\/\x22[\]:|<>+=;,?@]+)@([*a-z][a-z0-9.-]+)$";

        /// <summary>
        /// Parse method implementation
        /// </summary>
        public static LDAPInspectValues Parse(string pattern)
        {
            LDAPInspectValues ret = new LDAPInspectValues();
            Regex rg1 = new Regex(upnpattern);
            if (rg1.IsMatch(pattern))
            {
                string[] gp = rg1.Split(pattern);
                ret.UserNamePart = gp[1];
                ret.DomainPart = gp[2];
                ret.Mode = (LDAPUserSearchMode.UserPrincipalName);
                ret.Pattern = pattern;
                ret.Tagged = true;
            }
            else
            {
                ret.DomainPart = null;
                ret.UserNamePart = pattern;
                ret.Pattern = pattern;
                ret.Mode = LDAPUserSearchMode.AllOptions;
                ret.Tagged = false;
            }
            return ret;
        }

        /// <summary>
        /// GetDomainNamePart method implementation
        /// </summary>
        public static string GetDomainNamePart(LDAPInspectValues inspect)
        {
            if (inspect.Mode.Equals((LDAPUserSearchMode.UserPrincipalName)))
            {
                if (inspect.DomainPart.StartsWith("*"))
                    return inspect.DomainPart.Substring(2);
            }
            return inspect.DomainPart;
        }
    }
    #endregion
}
