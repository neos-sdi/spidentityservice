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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace SharePoint.IdentityService.Core
{
    #region IWrapperNoInit
    public interface IWrapperNoInit
    {
        void EnsureLoaded();
        ProxyResults FillSearch(string pattern, string domain, bool recursive);
        ProxyResults FillResolve(string pattern, bool recursive);
        ProxyResults FillValidate(string pattern, bool recursive);
        ProxyDomain FillHierarchy(string hierarchyNodeID, int numberOfLevels);
        List<ProxyBadDomain> FillBadDomains();
        void Reload();
        void LaunchStartCommand();
        void Log(Exception ex, string message, EventLogEntryType eventLogEntryType, int eventid = 0);
        void Trace(string message, EventLogEntryType eventLogEntryType, int eventid = 0);
        string ClaimsProviderName { get; set; }

    }
    #endregion

    #region IWrapper
    public interface IWrapper: IWrapperNoInit
    {
        Int64 ConnectorID { get; set; }
        void Initialize(List<ProxyFullConfiguration> configs, List<ProxyGeneralParameter> glbparams);
    }
    #endregion

    #region IWrapperCaching
    public interface IWrapperCaching
    {
        XmlDocument Save();
        void Restore(XmlDocument data);
        bool IsLoadedFromCache { get;}
        DateTime SavedTime { get; }
    }
    #endregion

    #region IForests
    public interface IForests
    {
        void Initialize(List<ProxyFullConfiguration> configs, List<ProxyGeneralParameter> glbparams);
        void FillSearch(IResults lst, string pattern, string domain, bool recursive = true);
        void FillResolve(IResults lst, string pattern, bool recursive = true);
        void FillValidate(IResults lst, string pattern, bool recursive = true);
        List<IDomain> GetDomain(string domain);
        void EnsureLoaded();
        void Reload();
        IUser GetUser(string account);
        bool IsLoaded { get; }
        List<IRootDomain> RootDomains { get; }
        List<IBadDomain> BadDomains { get; }
        List<IDomainConfig> DomainConfigurations { get; }
        string UserName { get; }
        string Password { get; }
        short DefaultTimeOut { get; }
        short DefaultSuspendTime { get; }
        TimeSpan ElapsedTime { get; }
        bool UsesScureConnection { get; }
        int MaxRowsPerDomain { get; }
        string ProviderName { get; set; }
        IGlobalParams GlobalParams { get; set; }

        XmlDocument Save();
        IForests Restore(XmlDocument data);
        bool IsLoadedFromCache { get; }
        DateTime SavedTime { get; }

    }
    #endregion

    public interface IGlobalParams
    {
        ProxySmoothRequest SmoothRequestor { get; set; }
        ProxyClaimsMode ClaimsMode { get; set; }
        ProxyClaimsIdentityMode ClaimIdentityMode { get; set; }
        ProxyClaimsRoleMode ClaimRoleMode { get; set; }
        bool IsWindows { get; set; }
        ProxyClaimsDisplayMode ClaimsDisplayMode { get; set; }
        ProxyClaimsDisplayMode PeoplePickerDisplayMode { get; set; }
        bool SearchByMail { get; set; }
        bool SearchByDisplayName { get; set; }
        bool Trace { get; set; }
        bool PeoplePickerImages { get; set; }
        bool ShowSystemNodes { get; set; }
    }

    #region IRootDomain
    public interface IRootDomain : IDomain
    {
        void Initialize(string domain, List<ITopLevelName> toplevels, IDomainParam prm, IGlobalParams global);
        List<ITopLevelName> TopLevelNames { get; }
    }
    #endregion

    #region IDomainParam
    public interface IDomainParam
    {
        string DisplayName { get; set; }
        string DnsName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        bool SecureConnection { get; set; }
        short QueryTimeout { get; set; }
        short SuspendDelay { get; set; }
        int MaxRows { get; set; }
        int Position { get; set; }
        string ConnectString { get; set; }
    }
    #endregion

    #region IDomain 
    public interface IDomain
    {
        void Initialize(string domain, IDomainParam parameters, IGlobalParams global);
        IDomain Parent { get; set; }
        bool IsReacheable { get; }
        string ErrorMessage { get; }
        string UserName { get; }
        string Password { get; }
        bool IsMaster { get; }
        bool IsRoot { get; set; }
        string DnsName { get; }
        string DisplayName { get; }
        string NetbiosName { get; }
        List<IDomain> Domains { get; }
        TimeSpan ElapsedTime { get; }
        short Timeout { get; }
        int MaxRows { get; }
        int Position { get; }
        void FillSearch(IResults lst, string searchPattern, bool recursive = true);
        void FillResolve(IResults lst, string searchPattern, bool recursive = true);
        void FillValidate(IResults lst, string searchPattern, bool recursive = true);
    }
    #endregion

    #region IResults
    public interface IResults
    {
        List<IResultsNode> GetNodes();
        List<IResultObject> GetResults();
        bool HasResults();
        IResultsNode AddNodeIfNotExists(IResultsNode anode);
        bool AddResultIfNotExists(IResultObject obj);
        string GetName();
        string GetDisplayName();
    }
    #endregion

    #region IResultsRoot
    public interface IResultsRoot : IResults 
    {
        void Initialize();
    }
    #endregion

    #region IResultsNode
    public interface IResultsNode : IResults 
    {
        void Initialize(string name, string displayname, int position);
        int GetPosition();
    }
    #endregion

    #region IResultObject
    public interface IResultObject
    {
        string DomainName { get; set; }
        string DomainDisplayName { get; set; }
        bool IsBuiltIn { get; set; }
        string DisplayName { get; set; }
        string SamAaccount { get; set; }
    }
    #endregion

    #region IUser
    public interface IUser : IResultObject
    {
        string UserPrincipalName { get; set; }
        string EmailAddress { get; set; }
        string PictureUrl { get; set; }
        string JobTitle { get; set; }
        string Department { get; set; }
        string Location { get; set; }
        string MobilePhone { get; set; }
        string SIPAddress { get; set; }
        string WorkPhone { get; set; }
    }
    #endregion

    #region IRole
    public interface IRole : IResultObject
    {
        GroupScope? GroupScope { get; set; }
        bool? IsSecurityGroup { get; set; }
        Guid? GUID { get; set; }
        string SID { get; set; }
        string EmailAddress { get; set; }
    }
    #endregion

    #region IForestLoadState
    public interface IForestLoadState
    {
        void Initialize(string forestname, List<ITopLevelName> toplevelnames);
        string ForestName { get; }
        List<ITopLevelName> TopLevelNames { get; }
    }
    #endregion

    #region IBadDomain
    public interface IBadDomain
    {
        void Initialize(string dnsname, string message, TimeSpan elapsedtime);
        string DnsName { get; }
        string Message { get; }
        TimeSpan ElapsedTime { get; }
    }
    #endregion

    #region IDomainConfig
    public interface IDomainConfig
    {
        void Initialize(string domainname, string displayname, string username, string password, short timeout, bool enabled, bool secure, int maxrows, int position, string connectstring);
        string UserName { get; }
        string Password { get; }
        string DomainName { get; }
        string DisplayName { get; }
        short Timeout { get; set; }
        bool Enabled { get; set; }
        bool SecureConnection { get; set; }
        int MaxRows { get; set; }
        int Position { get; set; }
        string ConnectString { get; }
    }
    #endregion

    #region IFillSearchLoadState
    public interface IFillSearchLoadState
    {
        void Initialize(IDomain domain, IResults lst, string pattern, bool recursive);
    }
    #endregion

    #region ITopLevelName
    public interface ITopLevelName
    {
        void Initialize(string name, TopLevelNameStatus status);
        string TopLevelName { get; }
        TopLevelNameStatus Status { get; }
    }
    #endregion

    #region TopLevelNameStatus
    public enum TopLevelNameStatus
    {
        Enabled = 0,
        NewlyCreated = 1,
        AdminDisabled = 2,
        ConflictDisabled = 4,
    }
    #endregion

    #region UserSearchMode enumeration
    [Flags]
    public enum UserSearchMode
    {
        AllOptions = 0,
        UserPrincipalName = 1,
        DisplayName = 2,
        SamAccount = 4,
        Groups = 8
    }
    #endregion

    #region Group Scopes
    public enum GroupScope
    {
        Local = 0,
        Global = 1,
        Universal = 2,
        Builtin = 3
    }
    #endregion

    /// <summary>
    /// ClaimProviderNameHeader method implementation
    /// </summary>
    public static class ClaimProviderNameHeader
    {
        public const string Header = "SPIS2477";
        /// <summary>
        /// GetClaimProviderInternalName method implementation
        /// </summary>
        public static string GetClaimProviderInternalName(string value)
        {
            if (value.ToLower().Equals("ad"))
                return "AD";
            if (value.ToLower().Equals("windows"))
                return "AD";
            if (value.StartsWith(ClaimProviderNameHeader.Header))
                return value;
            else
                return ClaimProviderNameHeader.Header + value;
        }

        /// <summary>
        /// GetClaimProviderInternalName method implementation
        /// </summary>
        public static string GetClaimProviderName(string value)
        {
            if (value.ToLower().Equals("ad"))
                return "AD";
            if (value.ToLower().Equals("windows"))
                return "AD";
            if (value.StartsWith(ClaimProviderNameHeader.Header))
                return value.Replace(ClaimProviderNameHeader.Header, "");
            else
                return value;
        }
    }
}
