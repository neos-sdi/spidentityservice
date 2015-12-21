//******************************************************************************************************************************************************************************************//
// Copyright (c) 2015 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                                             //
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
namespace SharePoint.IdentityService.Core
{
    using System;
    using System.ServiceModel;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using Microsoft.SharePoint.Administration.Claims;

    [ServiceContract(Namespace="http://sharepoint.identityservice.application")]
    public interface IIdentityServiceContract
    {
        [OperationContract]
        ProxyResults FillSearch(string pattern, string domain, bool recursive);

        [OperationContract]
        ProxyResults FillResolve(string pattern, bool recursive);

        [OperationContract]
        ProxyResults FillValidate(string pattern, bool recursive);

        [OperationContract]
        ProxyDomain FillHierarchy(string hierarchyNodeID, int numberOfLevels);

        [OperationContract]
        List<ProxyBadDomain> FillBadDomains();

        [OperationContract]
        List<ProxyGeneralParameter> FillGeneralParameters();

        [OperationContract]
        ProxyClaimsProviderParameters FillClaimsProviderParameters();

        [OperationContract]
        List<ProxyClaims> FillAdditionalClaims(string entity);

        [OperationContract]
        bool Reload();

        [OperationContract]
        bool ClearCache();

        [OperationContract]
        void LaunchStartCommand();

        [OperationContract]
        string GetServiceApplicationName();
    }

    [DataContract]
    public class ProxyClaims
    {
        bool _isWindows;
        bool _isSharePoint;
        string _claimType;
        string _claimValue;

        public ProxyClaims()
        {
        }

        public ProxyClaims(bool iswindows, bool issharepoint, string claimtype, string claimvalue)
        {
            this._isWindows = iswindows;
            this._isSharePoint = issharepoint;
            this._claimType = claimtype;
            this._claimValue = claimvalue;
        }

        [DataMember]
        public bool IsWindows
        {
            get { return _isWindows; }
            set { _isWindows = value; }
        }

        [DataMember]
        public bool IsSharePoint
        {
            get { return _isSharePoint; }
            set { _isSharePoint = value; }
        }

        [DataMember]
        public string ClaimType
        {
            get { return _claimType; }
            set { _claimType = value; }
        }

        [DataMember]
        public string ClaimValue
        {
            get { return _claimValue; }
            set { _claimValue = value; }
        }
    }

    public interface IIdentityServiceClaimsAugmenter
    {
        List<ProxyClaims> FillAdditionalClaims(string entity);
    }

    [KnownType(typeof(ProxyUser))]
    [KnownType(typeof(ProxyRole))]
    [DataContract]
    public class ProxyResults
    {
        List<ProxyResultObject> _results = new List<ProxyResultObject>();
        List<ProxyResultsNode> _nodes = new List<ProxyResultsNode>();

        [DataMember]
        public List<ProxyResultsNode> Nodes 
        {
            get { return _nodes; }
            set { _nodes = value; }
        }

        [DataMember]
        public List<ProxyResultObject> Results 
        { 
            get { return _results; }
            set { _results = value; }
            }

        [DataMember]
        public bool HasResults { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DisplayName { get; set; }
    }

    [DataContract]
    public class ProxyResultsNode : ProxyResults 
    {
        [DataMember]
        public int Position { get; set; }

    }

    [KnownType(typeof(ProxyUser))]
    [KnownType(typeof(ProxyRole))]
    [DataContract]
    public class ProxyResultObject: IComparer<ProxyResultObject>
    {
        [DataMember]
        public string DomainName { get; set; }

        [DataMember]
        public string DomainDisplayName { get; set; }

        [DataMember]
        public bool IsBuiltIn { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string SamAaccount { get; set; }

        public int Compare(ProxyResultObject x, ProxyResultObject y)
        {
            return string.Compare(x.DisplayName, y.DisplayName);
        }
    }

    [DataContract]
    public class ProxyUser : ProxyResultObject
    {
        [DataMember]
        public string UserPrincipalName { get; set; }

        [DataMember]
        public string EmailAddress { get; set; }

        [DataMember]
        public string PictureUrl { get; set; }

        [DataMember]
        public string JobTitle { get; set; }

        [DataMember]
        public string Department { get; set; }

        [DataMember]
        public string Location { get; set; }

        [DataMember]
        public string MobilePhone { get; set; }

        [DataMember]
        public string SIPAddress { get; set; }

        [DataMember]
        public string WorkPhone { get; set; }
    }

    [DataContract]
    public class ProxyRole : ProxyResultObject 
    {
        [DataMember]
        public Nullable<int> GroupScope { get; set; }

        [DataMember]
        public Nullable<bool> IsSecurityGroup { get; set; }

        [DataMember]
        public Nullable<Guid> GUID { get; set; }

        [DataMember]
        public string SID { get; set; }
    }

    [DataContract]
    public class ProxyDomain
    {
        private List<ProxyDomain> _children = new List<ProxyDomain>();

        [DataMember]
        public bool IsReacheable { get; set; }

        [DataMember]
        public bool IsRoot { get; set; }

        [DataMember]
        public string DnsName { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public TimeSpan ElapsedTime { get; set; }

        [DataMember]
        public List<ProxyDomain> Domains
        {
            get { return _children; }
            set { _children = value; }
        }
    }

    [DataContract]
    public class ProxyBadDomain
    {
        [DataMember]
        public string DnsName { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public TimeSpan ElapsedTime { get; set; }
    }

    public class ProxyFullConfiguration
    {
        public string DisplayName { get; set; }
        public string DnsName { get; set; }
        public bool Enabled { get; set; }
        public int DisplayPosition { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Int16 Timeout { get; set; }
        public bool Secure { get; set; }
        public int Maxrows { get; set; }
        public string ConnectString { get; set; }
        public bool IsDefault { get; set; }
    }

    [DataContract]
    public class ProxyGeneralParameter
    {
        [DataMember]
        public string ParamName { get; set; }

        [DataMember]
        public string ParamValue { get; set; }
    }

    [DataContract]
    public class ProxyClaimsProviderParameters
    {
        [DataMember]
        public string TrustedLoginProviderName { get; set; }

        [DataMember]
        public string ClaimProviderName { get; set; }

        [DataMember]
        public string ClaimDisplayName { get; set; }

        [DataMember]
        public bool PeoplePickerImages { get; set; }

        [DataMember]
        public ProxyClaimsMode ClaimProviderMode { get; set; }

        [DataMember]
        public ProxyClaimsIdentityMode ClaimProviderIdentityMode { get; set; }

        [DataMember]
        public string ClaimProviderIdentityClaim { get; set; }

        [DataMember]
        public ProxyClaimsRoleMode ClaimProviderRoleMode { get; set; }

        [DataMember]
        public string ClaimProviderRoleClaim { get; set; }

        [DataMember]
        public ProxyClaimsDisplayMode ClaimsProviderDisplayMode { get; set; }

        [DataMember]
        public ProxyClaimsDisplayMode ClaimsProviderPeoplePickerMode { get; set; }
    }

    [DataContract]
    public enum ProxySmoothRequest
    {
        [EnumMember]
        Strict,

        [EnumMember]
        StarsBefore,

        [EnumMember]
        StarsAfter,

        [EnumMember]
        Smooth
    }

    [DataContract]
    public enum ProxyClaimsMode
    {
        [EnumMember]
        Windows,

        [EnumMember]
        Federated
    }

    [DataContract]
    public enum ProxyClaimsDisplayMode
    {
        [EnumMember]
        DisplayName,

        [EnumMember]
        Email,

        [EnumMember]
        UPN,

        [EnumMember]
        SAMAccount,

        [EnumMember]
        DisplayNameAndEmail
    }

    [DataContract]
    public enum ProxyClaimsIdentityMode
    {
        [EnumMember]
        UserPrincipalName,

        [EnumMember]
        Email,

        [EnumMember]
        SAMAccount
    }

    [DataContract]
    public enum ProxyClaimsRoleMode
    {
        [EnumMember]
        SID,

        [EnumMember]
        Role
    }
}