#define supportskey
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

using Microsoft.SharePoint.Administration.Claims;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using SharePoint.IdentityService.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace SharePoint.IdentityService.ClaimsProvider
{
    #region ClaimProvider
    public class IdentityServiceClaimsProvider : SPClaimProvider
    {
        private static object _lockobj = new Object();
        private string _displayname;
        private string _localizeddisplayname;
        private string _trustedloginprovidername;
        private bool _iscontextloaded = false;
        private string _useridentityclaim = SPClaimTypes.UserPrincipalName; // to be overriden
        private string _roleidentityclaim = SPClaimTypes.ModernGroupClaimType; // to be overriden
        private bool _supportsuserkey = true; // to be overriden;
        private IdentityServiceClient _ad = null;
        private ProxyClaimsMode _claimsmode = ProxyClaimsMode.Federated;  // to be overriden
        private ProxyClaimsIdentityMode _claimidentitymode = ProxyClaimsIdentityMode.UserPrincipalName; // Can be overriden 
        private ProxyClaimsRoleMode _claimrolemode = ProxyClaimsRoleMode.SID; // Can be overriden 
        private ProxyClaimsDisplayMode _claimsdisplaymode = ProxyClaimsDisplayMode.DisplayName;  // to be overriden
        private ProxyClaimsDisplayMode _peoplepickerdisplaymode = ProxyClaimsDisplayMode.DisplayNameAndEmail;  // to be overriden

        private bool _peoplepickerimages = false;

        /// <summary>
        /// IdentityServiceClaimsProvider constructor
        /// </summary>
        public IdentityServiceClaimsProvider(string displayName): base(displayName)
        {
            lock (_lockobj)
            {
                _displayname = displayName;
            }
        }

        /// <summary>
        /// EnsureContext method implementation
        /// </summary>
        private void EnsureContext()
        {
            try
            {
                if ((!IsContextAvailable) && (_ad == null) || (!_ad.IsInitialized))
                {
                    lock (_lockobj)
                    {
                        using (SPMonitoredScope scp = new SPMonitoredScope("IdentityServiceClaimsProvider:EnsureContext"))
                        {
                            _ad = new IdentityServiceClient(GetInternalName(_displayname));
                            if ((_ad != null) && (_ad.IsInitialized))
                            {
                                ProxyClaimsProviderParameters prm = _ad.FillClaimsProviderParameters();
                                _trustedloginprovidername = prm.TrustedLoginProviderName;
                                _localizeddisplayname = prm.ClaimDisplayName;
                                _peoplepickerimages = prm.PeoplePickerImages;
                                _claimsmode = prm.ClaimProviderMode;
                                _claimidentitymode = prm.ClaimProviderIdentityMode;
                                _claimrolemode = prm.ClaimProviderRoleMode;
                                _useridentityclaim = prm.ClaimProviderIdentityClaim;
                                _roleidentityclaim = prm.ClaimProviderRoleClaim;
                                _claimsdisplaymode = prm.ClaimsProviderDisplayMode;
                                _peoplepickerdisplaymode = prm.ClaimsProviderPeoplePickerMode;
                                _supportsuserkey = prm.ClaimProviderSupportsUserKey;
                                _iscontextloaded = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _iscontextloaded = false;
                _ad = null;
            }
        }

        /// <summary>
        /// GetClaimProviderInternalName method implementation
        /// </summary>
        private string GetInternalName(string value)
        {
            return ClaimProviderNameHeader.GetClaimProviderInternalName(value);
        }

        /// <summary>
        /// IsContextAvailable property implementation
        /// </summary>
        private bool IsContextAvailable
        {
            get { return _iscontextloaded; }
        }

        /// <summary>
        /// ShowPeoplePickerImages property implementation
        /// </summary>
        private bool ShowPeoplePickerImages
        {
            get { return _peoplepickerimages; }
        }

        /// <summary>
        /// Name property implementation
        /// </summary>
        public override string Name
        {
            get { return _displayname; }
        } 

        /// <summary>
        /// FillDefaultLocalizedDisplayName method implmentation
        /// </summary>
        protected override void FillDefaultLocalizedDisplayName(CultureInfo culture, out string localizedName)
        {
            if (this._claimsmode == ProxyClaimsMode.Windows)
            {
                if (string.IsNullOrEmpty(_localizeddisplayname))
                {
                    if (_displayname.ToLowerInvariant().Trim().Equals("ad"))
                        localizedName = "Active Directory";
                    else
                        localizedName = _displayname;
                }
                else
                    localizedName = _localizeddisplayname;
            }
            else
            {
                if (string.IsNullOrEmpty(_localizeddisplayname))
                    localizedName = _displayname;
                else
                    localizedName = _localizeddisplayname;
            }
        }

        /// <summary>
        /// InternalGetDisplayName method implementation
        /// </summary>
        private string InternalGetDisplayName()
        {
            if (string.IsNullOrEmpty(_localizeddisplayname))
                return _displayname;
            else
                return _localizeddisplayname;
        }

        /// <summary>
        /// AssociatedTrustedLoginProviderName method implementation
        /// The AssociatedTrustedLoginProviderName property is used for 
        /// generating the SPClaim object. 
        /// </summary>
        public string AssociatedTrustedLoginProviderName
        {
            get 
            {
                EnsureContext();
                if (this._claimsmode == ProxyClaimsMode.Windows)
                    return "Windows";
                else
                    return _trustedloginprovidername; 
            }
        }

        // The claim from this provider should have redhook as the provider name.
        private string IssuerName
        {
            get
            {
                string result = string.Empty;
                if (_claimsmode == ProxyClaimsMode.Federated)
                    result = SPOriginalIssuers.Format(SPOriginalIssuerType.TrustedProvider, AssociatedTrustedLoginProviderName);
                else
                    result = SPOriginalIssuers.Format(SPOriginalIssuerType.Windows, AssociatedTrustedLoginProviderName);
                return result;
            }
        }


        /// <summary>
        /// SupportsEntityInformation property override
        /// </summary>
        public override bool SupportsEntityInformation
        {
            get { return true; }
        }

        /// <summary>
        /// SupportsResolve property override
        /// </summary>
        public override bool SupportsResolve
        {
            get { return true; }
        }

        /// <summary>
        /// SupportsSearch property override
        /// </summary>
        public override bool SupportsSearch
        {
            get { return true; }
        }

        /// <summary>
        /// SupportsHierarchy property override
        /// </summary>
        public override bool SupportsHierarchy
        {
            get { return true; }
        }

        /// <summary>
        /// SupportsUserSpecificHierarchy property override
        /// </summary>
        public override bool SupportsUserSpecificHierarchy
        {
            get  { return false; }
        }

#if supportskey
        /// <summary>
        /// SupportsUserKey property override;
        /// </summary>
        public override bool SupportsUserKey 
        {
            get { return _supportsuserkey; }
        }

        /// <summary>
        /// GetClaimTypeForUserKey method override
        /// </summary>
        public override string GetClaimTypeForUserKey()
        {
            if (!IsContextAvailable)
            {
                if (_displayname.ToLowerInvariant().Trim().Equals("ad"))
                    return Microsoft.IdentityModel.Claims.ClaimTypes.WindowsAccountName;
                else
                    return Microsoft.IdentityModel.Claims.ClaimTypes.Upn;
            }
            else
                return _useridentityclaim;
        }

        /// <summary>
        /// GetUserKeyForEntity method override
        /// </summary>
        protected override SPClaim GetUserKeyForEntity(SPClaim entity)
        {
            if (String.Equals(entity.OriginalIssuer, this.IssuerName, StringComparison.InvariantCultureIgnoreCase))
                return entity;

            SPClaimProviderManager cpm = SPClaimProviderManager.Local;
            SPClaim curUser = SPClaimProviderManager.DecodeUserIdentifierClaim(entity);
            if (curUser != null)
                return CreateClaimForSTS(GetClaimTypeForUserKey(), curUser.Value);
            else
                return null;

/*
            if (entity.ClaimType.ToLowerInvariant().Equals(_useridentityclaim.ToLowerInvariant()))
                return entity;
            else
                return new SPClaim(GetClaimTypeForUserKey(), entity.Value, entity.ValueType, entity.OriginalIssuer);          
*/
        }
#endif
        #region Claims Augmentation
        /// <summary>
        /// FillClaimTypes method override
        /// </summary>
        /// <param name="claimTypes"></param>
        protected override void FillClaimTypes(List<string> claimTypes)
        {
            if (null == claimTypes)
            {
                throw new ArgumentNullException("claimTypes");
            }
        }

        /// <summary>
        /// FillClaimValueTypes method override
        /// </summary>
        protected override void FillClaimValueTypes(List<string> claimValueTypes)
        {
            if (null == claimValueTypes)
            {
                throw new ArgumentNullException("claimValueTypes");
            }
        }

        /// <summary>
        /// FillEntityTypes method override
        /// </summary>
        protected override void FillEntityTypes(List<string> entityTypes)
        {
            entityTypes.Clear();
            entityTypes.Add(SPClaimEntityTypes.User);
            entityTypes.Add(SPClaimEntityTypes.Trusted);
            entityTypes.Add(SPClaimEntityTypes.FormsRole);
            entityTypes.Add(SPClaimEntityTypes.SecurityGroup);
        }

        /// <summary>
        /// Required for the People Picker.
        /// FillSchema method override
        /// </summary>
        protected override void FillSchema(SPProviderSchema schema)
        {
            if (schema==null)
                return;
            schema.AddSchemaElement(new SPSchemaElement(PeopleEditorEntityDataKeys.AccountName, PeopleEditorEntityDataKeys.AccountName, SPSchemaElementType.TableViewOnly));
            schema.AddSchemaElement(new SPSchemaElement(PeopleEditorEntityDataKeys.DisplayName, PeopleEditorEntityDataKeys.DisplayName, SPSchemaElementType.TableViewOnly));
            schema.AddSchemaElement(new SPSchemaElement(PeopleEditorEntityDataKeys.Email, PeopleEditorEntityDataKeys.Email, SPSchemaElementType.TableViewOnly));
            schema.AddSchemaElement(new SPSchemaElement(PeopleEditorEntityDataKeys.MobilePhone, PeopleEditorEntityDataKeys.MobilePhone, SPSchemaElementType.TableViewOnly));
            schema.AddSchemaElement(new SPSchemaElement(PeopleEditorEntityDataKeys.WorkPhone, PeopleEditorEntityDataKeys.WorkPhone, SPSchemaElementType.TableViewOnly));
            schema.AddSchemaElement(new SPSchemaElement(PeopleEditorEntityDataKeys.Department, PeopleEditorEntityDataKeys.Department, SPSchemaElementType.TableViewOnly));
            schema.AddSchemaElement(new SPSchemaElement(PeopleEditorEntityDataKeys.JobTitle, PeopleEditorEntityDataKeys.JobTitle, SPSchemaElementType.TableViewOnly));
            schema.AddSchemaElement(new SPSchemaElement(PeopleEditorEntityDataKeys.Location, PeopleEditorEntityDataKeys.Location, SPSchemaElementType.TableViewOnly));
            schema.AddSchemaElement(new SPSchemaElement(PeopleEditorEntityDataKeys.SIPAddress, PeopleEditorEntityDataKeys.SIPAddress, SPSchemaElementType.TableViewOnly));
            schema.AddSchemaElement(new SPSchemaElement("Picture", "Picture", SPSchemaElementType.DetailViewOnly));  
        }

        /// <summary>
        /// FillClaimsForEntity method overrider
        /// Implement this method if the provider supports claims augmentation. Claims Augmentation MUST be desactivated in Windows Claims Authentication, because this method is called for each ressource access
        /// </summary>
        protected override void FillClaimsForEntity(Uri context, SPClaim entity, List<SPClaim> claims)
        {
            if (null == entity)
            {
                throw new ArgumentNullException("entity");
            }
            if (null == claims)
            {
                throw new ArgumentNullException("claims");
            }
            EnsureContext();
            if ((IsContextAvailable) && (!entity.Value.StartsWith("0#.w|"))) // exit if windows claims
            {
                try
                {
                    string v = entity.Value;

                    if (!v.ToLowerInvariant().Trim().Contains("|"+this.AssociatedTrustedLoginProviderName.ToLowerInvariant().Trim()+"|"))
                        return;
                    
                    string kuser = v.Substring(v.LastIndexOf("|")+1);
                    List<ProxyClaims> res = _ad.FillAdditionalClaims(kuser);
                    if (res != null)
                    {
                        foreach (ProxyClaims c in res)
                        {
                            if ((c.IsWindows) && (!c.IsSharePoint))
                                claims.Insert(0,new SPClaim(c.ClaimType, c.ClaimValue, Microsoft.IdentityModel.Claims.ClaimValueTypes.String, SPOriginalIssuers.Format(SPOriginalIssuerType.Windows, AssociatedTrustedLoginProviderName)));
                            else if ((!c.IsWindows) && (c.IsSharePoint))
                                claims.Insert(0,new SPClaim(c.ClaimType, c.ClaimValue, Microsoft.IdentityModel.Claims.ClaimValueTypes.String, SPOriginalIssuers.Format(SPOriginalIssuerType.SecurityTokenService, AssociatedTrustedLoginProviderName)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogEvent.Log(ex, ex.Message, EventLogEntryType.Error, 7000);
                }
            }
        }
#endregion

        // The claim from this provider should have redhook as the provider name.
        private SPClaim CreateClaimForSTS(string claimtype, string claimValue)
        {
            SPClaim result = null;
            if (_claimsmode==ProxyClaimsMode.Federated)
                result = new SPClaim(claimtype, claimValue, Microsoft.IdentityModel.Claims.ClaimValueTypes.String, SPOriginalIssuers.Format (SPOriginalIssuerType.TrustedProvider,  AssociatedTrustedLoginProviderName));
            else
                result = new SPClaim(claimtype, claimValue, Microsoft.IdentityModel.Claims.ClaimValueTypes.String, SPOriginalIssuers.Format(SPOriginalIssuerType.Windows, AssociatedTrustedLoginProviderName));
            return result;
        }

        /// <summary>
        /// ClaimTypeIdentifier property implementation
        /// </summary>
        private string UserClaimTypeIdentifier
        {
            get 
            {
                EnsureContext();
                return this._useridentityclaim;
            }
        }

        /// <summary>
        /// RoleClaimTypeIdentifier property implementation
        /// </summary>
        private string RoleClaimTypeIdentifier
        {
            get
            {
                return this._roleidentityclaim;
            }
        }

        private bool IsRoleClaimGroupSID()
        {
            return this._roleidentityclaim.ToLowerInvariant().Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid");
        }

#region FillSearch
        /// <summary>
        /// FillSearch method override
        /// </summary>
        protected override void FillSearch(Uri context, string[] entityTypes, string searchPattern, string hierarchyNodeID, int maxCount, SPProviderHierarchyTree searchTree)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("search pattern", "FillSearch needs a correct input value !");
            }
            if ((!EntityTypesContain(entityTypes, SPClaimEntityTypes.User)) && (!EntityTypesContain(entityTypes, SPClaimEntityTypes.Trusted)))
            {
                LogEvent.Trace(string.Format(ResourcesValues.GetString("E07005"), searchPattern), EventLogEntryType.Warning, 7005);
                return;
            }
            try
            {
                using (SPMonitoredScope scp = new SPMonitoredScope("IdentityServiceClaimsProvider::FillSearch"))
                {
                    string keyword = searchPattern.ToLowerInvariant();
                    EnsureContext();
                    if (IsContextAvailable)
                    {
                        ProxyResults lst = _ad.FillSearch(keyword, hierarchyNodeID, true);
                        if (lst != null)
                        {
                            searchTree.Name = InternalGetDisplayName();
                            DoFillSearch(lst, searchTree, true);
                        }
                    }
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E07000"), searchPattern), EventLogEntryType.Warning, 7000);
                throw E;
            }
        }


        /// <summary>
        /// DoFillSearch method implementation
        /// </summary>
        private void DoFillSearch(ProxyResults lst, SPProviderHierarchyElement searchTree, bool isroot)
        {
            SPProviderHierarchyNode matchnode = null;
            foreach (ProxyResultObject usr in lst.Results)
            {
                CreateSearchedPickerEntity(usr, searchTree);
            }
            foreach (ProxyResultsNode node in lst.Nodes)
            {
                if (!searchTree.HasChild(node.Name))
                {
                    if (node.DisplayName == null)
                        node.DisplayName = node.Name;
                   // matchnode = new SPProviderHierarchyNode(InternalGetDisplayName(), node.DisplayName, node.Name, false);
                    matchnode = new SPProviderHierarchyNode(InternalGetDisplayName(), node.DisplayName, node.DisplayName, false);
                    searchTree.AddChild(matchnode);
                }
                else
                {
                    matchnode = searchTree.GetChild(node.Name);
                }
                DoFillSearch(node, matchnode, false);
            }
        }

        /// <summary>
        /// CreateSearchedPickerEntity method implementation
        /// </summary>
        private void CreateSearchedPickerEntity(ProxyResultObject usr, SPProviderHierarchyElement searchNode)
        {
            if (this._claimsmode == ProxyClaimsMode.Federated)
                CreateFederatedClaimsSearchedPickerEntity(usr, searchNode);
            else
                CreateWindowsClaimsSearchedPickerEntity(usr, searchNode);
        }
#endregion

#region FillResolve
        /// <summary>
        /// FillResolve method override
        /// Required for resoving a user.
        /// This method is called by all the claim provider instance assigned to your web application (initial and extends)
        /// </summary>
        protected override void FillResolve(Uri context, string[] entityTypes,  SPClaim resolveInput, List<PickerEntity> resolved)
        {
            if (resolveInput == null)
            {
                throw new ArgumentNullException("input spclaim", "FillResolve needs a correct input value !");
            }
            if ((!EntityTypesContain(entityTypes, SPClaimEntityTypes.User)) && (!EntityTypesContain(entityTypes, SPClaimEntityTypes.Trusted)))
            {
                LogEvent.Trace(ResourcesValues.GetString("E07004"), EventLogEntryType.Warning, 7004);
                return;
            }
            try
            {
                using (SPMonitoredScope scp = new SPMonitoredScope("IdentityServiceClaimsProvider:FillResolve(SPClaim)"))
                {
                    EnsureContext();
                    if (IsContextAvailable)
                    {
                        string keyword = resolveInput.Value.ToLowerInvariant();
                        string kclaim = resolveInput.ClaimType.ToLowerInvariant();

                        if ( !kclaim.Equals(this.UserClaimTypeIdentifier.ToLowerInvariant()) && !kclaim.Equals(this.RoleClaimTypeIdentifier.ToLowerInvariant()))
                            return;

                        string kissuer = resolveInput.OriginalIssuer.Replace(SPOriginalIssuerType.TrustedProvider + ":", "");
                        if (!kissuer.ToLowerInvariant().Trim().Equals(this.AssociatedTrustedLoginProviderName.ToLowerInvariant().Trim()))
                            return;

                        ProxyResults lst = _ad.FillValidate(keyword, true);
                        if (lst != null)
                        {
                            DoFillResolve(lst, resolved, true);
                        }
                    }
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E07001"), resolveInput.Value), EventLogEntryType.Warning, 7001);
                throw E;
            }
        }

        /// <summary>
        /// FillResolve method override
        /// Required if you implement claims resolve for the People Picker.
        /// This method is only called by your claim provider instance
        /// </summary>
        protected override void FillResolve(Uri context, string[] entityTypes, string  resolveInput, List<PickerEntity> resolved)
        {
            if (resolveInput == null)
            {
                throw new ArgumentNullException("input text", "FillResolve needs a correct input value !");
            }
            if ((!EntityTypesContain(entityTypes, SPClaimEntityTypes.User)) && (!EntityTypesContain(entityTypes, SPClaimEntityTypes.Trusted)))
            {
                LogEvent.Trace(string.Format(ResourcesValues.GetString("E07006"), resolveInput), EventLogEntryType.Warning, 7006);
                return;
            }
            try
            {
                using (SPMonitoredScope scp = new SPMonitoredScope("IdentityServiceClaimsProvider:FillResolve(String)"))
                {
                    EnsureContext();
                    if (IsContextAvailable)
                    {
                        string keyword = resolveInput.ToLowerInvariant();
                        ProxyResults lst = _ad.FillResolve(keyword, true);
                        if (lst != null)
                        {
                            DoFillResolve(lst, resolved, true);
                        }
                    }
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, string.Format(ResourcesValues.GetString("E07002"), resolveInput), EventLogEntryType.Warning, 7002);
                throw E;
            }
        }

        /// <summary>
        /// DoFillResolve method implementation
        /// </summary>
        private void DoFillResolve(ProxyResults lst, List<PickerEntity> resolved, bool isroot)
        {
            foreach (ProxyResultObject usr in lst.Results)
            {
                CreateResolvedPickerEntity(usr, resolved);
            }
            foreach (ProxyResultsNode node in lst.Nodes)
            {
                DoFillResolve(node, resolved, false);
            }
        }

        /// <summary>
        /// CreateResolvedPickerEntity method implementation
        /// </summary>
        private void CreateResolvedPickerEntity(ProxyResultObject usr, List<PickerEntity> resolved)
        {
            if (this._claimsmode == ProxyClaimsMode.Federated)
                CreateFederatedClaimsResolvedPickerEntity(usr, resolved);
            else
                CreateWindowsClaimsResolvedPickerEntity(usr, resolved);
        }

#endregion

#region PickerEntity
        /// <summary>
        /// CreateFederatedClaimsSearchedPickerEntity
        /// </summary>
        private void CreateFederatedClaimsSearchedPickerEntity(ProxyResultObject usr, SPProviderHierarchyElement searchNode)
        {
            PickerEntity entity = CreatePickerEntity();
            string IdentityValue = string.Empty;
            if (usr is ProxyUser)
            {
                ProxyUser xusr = usr as ProxyUser;
                switch (this._claimidentitymode)
                {
                    case ProxyClaimsIdentityMode.UserPrincipalName:
                        IdentityValue = xusr.UserPrincipalName;
                        break;
                    case ProxyClaimsIdentityMode.Email:
                        IdentityValue = xusr.EmailAddress;
                        break;
                    case ProxyClaimsIdentityMode.SAMAccount:
                        IdentityValue = xusr.SamAaccount;
                        break;
                }
                if (string.IsNullOrEmpty(IdentityValue))
                    return;
                entity.Claim = CreateClaimForSTS(this._useridentityclaim, IdentityValue);
                entity.EntityType = SPClaimEntityTypes.Trusted;
                entity.EntityData[PeopleEditorEntityDataKeys.AccountName]     = IdentityValue;
                if (!string.IsNullOrEmpty(xusr.EmailAddress))
                    entity.EntityData[PeopleEditorEntityDataKeys.Email]       = xusr.EmailAddress;
                if (!string.IsNullOrEmpty(xusr.MobilePhone))
                    entity.EntityData[PeopleEditorEntityDataKeys.MobilePhone] = xusr.MobilePhone;
                if (!string.IsNullOrEmpty(xusr.WorkPhone))
                    entity.EntityData[PeopleEditorEntityDataKeys.WorkPhone]   = xusr.WorkPhone;
                if (!string.IsNullOrEmpty(xusr.Department))
                    entity.EntityData[PeopleEditorEntityDataKeys.Department]  = xusr.Department;
                if (!string.IsNullOrEmpty(xusr.JobTitle))
                    entity.EntityData[PeopleEditorEntityDataKeys.JobTitle]    = xusr.JobTitle;
                if (!string.IsNullOrEmpty(xusr.Location))
                    entity.EntityData[PeopleEditorEntityDataKeys.Location]    = xusr.Location;
                if (!string.IsNullOrEmpty(xusr.SIPAddress))
                    entity.EntityData[PeopleEditorEntityDataKeys.SIPAddress]  = xusr.SIPAddress;
                if (!string.IsNullOrEmpty(xusr.PictureUrl))
                    entity.EntityData["Picture"] = xusr.PictureUrl;

                entity.EntityGroupName = "Personnes";
               // entity.HierarchyIdentifier = xusr.DomainName;
                entity.HierarchyIdentifier = xusr.DomainDisplayName;

                switch (this._peoplepickerdisplaymode)
                {
                    case ProxyClaimsDisplayMode.UPN:
                        entity.DisplayText = xusr.UserPrincipalName;
                        entity.Description = xusr.UserPrincipalName;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.UserPrincipalName;
                        break;
                    case ProxyClaimsDisplayMode.SAMAccount:
                        if (string.IsNullOrEmpty(xusr.SamAaccount))
                            xusr.EmailAddress = IdentityValue;
                        entity.DisplayText = xusr.SamAaccount;
                        entity.Description = xusr.SamAaccount;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.SamAaccount;
                        break;
                    case ProxyClaimsDisplayMode.Email:
                        if (string.IsNullOrEmpty(xusr.EmailAddress))
                            xusr.EmailAddress = IdentityValue;
                        entity.DisplayText = xusr.EmailAddress;
                        entity.Description = xusr.EmailAddress;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.EmailAddress;
                        break;
                    case ProxyClaimsDisplayMode.DisplayName:
                        if (string.IsNullOrEmpty(xusr.DisplayName))
                            xusr.DisplayName = IdentityValue;
                        entity.DisplayText = xusr.DisplayName;
                        entity.Description = xusr.DisplayName;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName;
                        break;
                    case ProxyClaimsDisplayMode.DisplayNameAndEmail:
                        if (string.IsNullOrEmpty(xusr.DisplayName))
                            xusr.DisplayName = IdentityValue;
                        if (string.IsNullOrEmpty(xusr.EmailAddress))
                        {
                            entity.DisplayText = xusr.DisplayName;
                            entity.Description = xusr.DisplayName;
                            entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName;
                        }
                        else
                        {
                            entity.DisplayText = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                            entity.Description = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                            entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                        }
                        break;
                }
                entity.IsResolved = true;
                searchNode.AddEntity(entity);
                return;
            }
            else
            {
                ProxyRole xcl = usr as ProxyRole;
                switch (this._claimrolemode)
                {
                    case ProxyClaimsRoleMode.SID:
                        IdentityValue = xcl.SID;
                        break;
                    case ProxyClaimsRoleMode.Role:
                        IdentityValue = xcl.SamAaccount;
                        break;
                }
                if (string.IsNullOrEmpty(IdentityValue))
                    return;
                entity.Claim = CreateClaimForSTS(this._roleidentityclaim, IdentityValue);
                if (IsRoleClaimGroupSID())
                    entity.EntityType = SPClaimEntityTypes.SecurityGroup;
                else
                    entity.EntityType = SPClaimEntityTypes.FormsRole;
                entity.EntityData[PeopleEditorEntityDataKeys.AccountName] = IdentityValue;
                entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xcl.SamAaccount;
                entity.EntityData[PeopleEditorEntityDataKeys.Email] = xcl.EmailAddress;

                entity.Description = xcl.SamAaccount;
                entity.DisplayText = xcl.SamAaccount;
                entity.EntityGroupName = "Roles";
               // entity.HierarchyIdentifier = xcl.DomainName;
                entity.HierarchyIdentifier = xcl.DomainDisplayName;
                entity.IsResolved = true;
                searchNode.AddEntity(entity);
                return;
            }
        }

        /// <summary>
        /// CreateWindowsClaimsSearchedPickerEntity
        /// </summary>
        private void CreateWindowsClaimsSearchedPickerEntity(ProxyResultObject usr, SPProviderHierarchyElement searchNode)
        {
            PickerEntity entity = CreatePickerEntity();
            string IdentityValue = string.Empty;
            if (usr is ProxyUser)
            {   
                ProxyUser xusr = usr as ProxyUser;
                IdentityValue = xusr.SamAaccount;
                if (string.IsNullOrEmpty(IdentityValue))
                    return;
                entity.Claim = CreateClaimForSTS(this._useridentityclaim, IdentityValue);
                entity.EntityType = SPClaimEntityTypes.User;
                entity.EntityData[PeopleEditorEntityDataKeys.AccountName] = xusr.SamAaccount;
                if (!string.IsNullOrEmpty(xusr.EmailAddress))
                    entity.EntityData[PeopleEditorEntityDataKeys.Email] = xusr.EmailAddress;
                if (!string.IsNullOrEmpty(xusr.MobilePhone))
                    entity.EntityData[PeopleEditorEntityDataKeys.MobilePhone] = xusr.MobilePhone;
                if (!string.IsNullOrEmpty(xusr.WorkPhone))
                    entity.EntityData[PeopleEditorEntityDataKeys.WorkPhone] = xusr.WorkPhone;
                if (!string.IsNullOrEmpty(xusr.Department))
                    entity.EntityData[PeopleEditorEntityDataKeys.Department] = xusr.Department;
                if (!string.IsNullOrEmpty(xusr.JobTitle))
                    entity.EntityData[PeopleEditorEntityDataKeys.JobTitle] = xusr.JobTitle;
                if (!string.IsNullOrEmpty(xusr.Location))
                    entity.EntityData[PeopleEditorEntityDataKeys.Location] = xusr.Location;
                if (!string.IsNullOrEmpty(xusr.SIPAddress))
                    entity.EntityData[PeopleEditorEntityDataKeys.SIPAddress] = xusr.SIPAddress;
                if (!string.IsNullOrEmpty(xusr.PictureUrl))
                    entity.EntityData["Picture"] = xusr.PictureUrl;

                entity.EntityGroupName = "Personnes";
               // entity.HierarchyIdentifier = xusr.DomainName;
                entity.HierarchyIdentifier = xusr.DomainDisplayName;

                switch (this._peoplepickerdisplaymode)
                {
                    case ProxyClaimsDisplayMode.SAMAccount:
                        entity.DisplayText = xusr.SamAaccount;
                        entity.Description = xusr.SamAaccount;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.SamAaccount;
                        break;
                    case ProxyClaimsDisplayMode.UPN:
                        if (string.IsNullOrEmpty(xusr.UserPrincipalName))
                            xusr.EmailAddress = IdentityValue;
                        entity.DisplayText = xusr.UserPrincipalName;
                        entity.Description = xusr.UserPrincipalName;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.UserPrincipalName;
                        break;
                    case ProxyClaimsDisplayMode.Email:
                        if (string.IsNullOrEmpty(xusr.EmailAddress))
                            xusr.EmailAddress = IdentityValue;
                        entity.DisplayText = xusr.EmailAddress;
                        entity.Description = xusr.EmailAddress;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.EmailAddress;
                        break;
                    case ProxyClaimsDisplayMode.DisplayName:
                        if (string.IsNullOrEmpty(xusr.DisplayName))
                            xusr.DisplayName = IdentityValue;
                        entity.DisplayText = xusr.DisplayName;
                        entity.Description = xusr.DisplayName;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName;
                        break;
                    case ProxyClaimsDisplayMode.DisplayNameAndEmail:
                        if (string.IsNullOrEmpty(xusr.DisplayName))
                            xusr.DisplayName = IdentityValue;
                        if (string.IsNullOrEmpty(xusr.EmailAddress))
                        {
                            entity.DisplayText = xusr.DisplayName;
                            entity.Description = xusr.DisplayName;
                            entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName;
                        }
                        else
                        {
                            entity.DisplayText = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                            entity.Description = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                            entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                        }
                        break;
                }
                entity.IsResolved = true;
                searchNode.AddEntity(entity);
                return;
            }
            else
            {
                ProxyRole xcl = usr as ProxyRole;
                IdentityValue = xcl.SID;
                if (string.IsNullOrEmpty(IdentityValue))
                    return;
                entity.Claim = CreateClaimForSTS(this._roleidentityclaim, IdentityValue);
                entity.EntityType = SPClaimEntityTypes.SecurityGroup;
                entity.Description = xcl.SamAaccount;
                entity.DisplayText = xcl.SamAaccount;
                entity.EntityGroupName = "Roles";
               // entity.HierarchyIdentifier = xcl.DomainName;
                entity.HierarchyIdentifier = xcl.DomainDisplayName;
                entity.EntityData[PeopleEditorEntityDataKeys.AccountName] = xcl.SamAaccount;
                entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xcl.SamAaccount;
                entity.EntityData[PeopleEditorEntityDataKeys.Email] = xcl.EmailAddress;
                entity.IsResolved = true;
                searchNode.AddEntity(entity);
                return;
            }
        }

        /// <summary>
        /// CreateWindowsClaimsResolvedPickerEntity method implementation
        /// </summary>
        private void CreateFederatedClaimsResolvedPickerEntity(ProxyResultObject usr, List<PickerEntity> resolved)
        {
            PickerEntity entity = CreatePickerEntity();
            string IdentityValue = string.Empty;
            if (usr is ProxyUser)
            {
                ProxyUser xusr = usr as ProxyUser;
                switch (this._claimidentitymode)
                {
                    case ProxyClaimsIdentityMode.UserPrincipalName:
                        IdentityValue = xusr.UserPrincipalName;
                        break;
                    case ProxyClaimsIdentityMode.Email:
                        IdentityValue = xusr.EmailAddress;
                        break;
                    case ProxyClaimsIdentityMode.SAMAccount:
                        IdentityValue = xusr.SamAaccount;
                        break;
                }
                if (string.IsNullOrEmpty(IdentityValue))
                    return;
                entity.Claim = CreateClaimForSTS(this._useridentityclaim, IdentityValue);
                entity.EntityType = SPClaimEntityTypes.Trusted;
                entity.EntityData[PeopleEditorEntityDataKeys.AccountName] = IdentityValue;
                if (!string.IsNullOrEmpty(xusr.EmailAddress))
                    entity.EntityData[PeopleEditorEntityDataKeys.Email] = xusr.EmailAddress;
                if (!string.IsNullOrEmpty(xusr.MobilePhone))
                    entity.EntityData[PeopleEditorEntityDataKeys.MobilePhone] = xusr.MobilePhone;
                if (!string.IsNullOrEmpty(xusr.WorkPhone))
                    entity.EntityData[PeopleEditorEntityDataKeys.WorkPhone] = xusr.WorkPhone;
                if (!string.IsNullOrEmpty(xusr.Department))
                    entity.EntityData[PeopleEditorEntityDataKeys.Department] = xusr.Department;
                if (!string.IsNullOrEmpty(xusr.JobTitle))
                    entity.EntityData[PeopleEditorEntityDataKeys.JobTitle] = xusr.JobTitle;
                if (!string.IsNullOrEmpty(xusr.Location))
                    entity.EntityData[PeopleEditorEntityDataKeys.Location] = xusr.Location;
                if (!string.IsNullOrEmpty(xusr.SIPAddress))
                    entity.EntityData[PeopleEditorEntityDataKeys.SIPAddress] = xusr.SIPAddress;
                if (!string.IsNullOrEmpty(xusr.PictureUrl))
                    entity.EntityData["Picture"] = xusr.PictureUrl;

                switch (this._claimsdisplaymode)
                {
                    case ProxyClaimsDisplayMode.UPN:
                        entity.DisplayText = xusr.UserPrincipalName;
                        entity.Description = xusr.UserPrincipalName;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.UserPrincipalName;
                        break;
                    case ProxyClaimsDisplayMode.SAMAccount:
                        if (string.IsNullOrEmpty(xusr.SamAaccount))
                            xusr.SamAaccount = IdentityValue;
                        entity.DisplayText = xusr.SamAaccount;
                        entity.Description = xusr.SamAaccount;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.SamAaccount;
                        break;
                    case ProxyClaimsDisplayMode.Email:
                        if (string.IsNullOrEmpty(xusr.EmailAddress))
                            xusr.EmailAddress = IdentityValue;
                        entity.DisplayText = xusr.EmailAddress;
                        entity.Description = xusr.EmailAddress;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.EmailAddress;
                        break;
                    case ProxyClaimsDisplayMode.DisplayName:
                        if (string.IsNullOrEmpty(xusr.DisplayName))
                            xusr.DisplayName = IdentityValue;
                        entity.DisplayText = xusr.DisplayName;
                        entity.Description = xusr.DisplayName;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName;
                        break;
                    case ProxyClaimsDisplayMode.DisplayNameAndEmail:
                        if (string.IsNullOrEmpty(xusr.DisplayName))
                            xusr.DisplayName = IdentityValue;
                        if (string.IsNullOrEmpty(xusr.EmailAddress))
                        {
                            entity.DisplayText = xusr.DisplayName;
                            entity.Description = xusr.DisplayName;
                            entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName;
                        }
                        else
                        {
                            entity.DisplayText = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                            entity.Description = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                            entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                        }
                        break;
                }
                entity.IsResolved = true;
                resolved.Add(entity);
                return;
            }
            else
            {
                ProxyRole xcl = usr as ProxyRole;
                switch (this._claimrolemode)
                {
                    case ProxyClaimsRoleMode.SID:
                        IdentityValue = xcl.SID;
                        break;
                    case ProxyClaimsRoleMode.Role:
                        IdentityValue = xcl.SamAaccount;
                        break;
                }
                if (string.IsNullOrEmpty(IdentityValue))
                    return;
                entity.Claim = CreateClaimForSTS(this._roleidentityclaim, IdentityValue);
                if (IsRoleClaimGroupSID())
                    entity.EntityType = SPClaimEntityTypes.SecurityGroup;
                else
                    entity.EntityType = SPClaimEntityTypes.FormsRole;
                entity.EntityData[PeopleEditorEntityDataKeys.AccountName] = IdentityValue;
                entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xcl.SamAaccount;
                entity.EntityData[PeopleEditorEntityDataKeys.Email] = xcl.EmailAddress;

                entity.Description = xcl.SamAaccount;
                entity.DisplayText = xcl.SamAaccount;
                entity.IsResolved = true;
                resolved.Add(entity);
                return;
            }
        }

        /// <summary>
        /// CreateWindowsClaimsResolvedPickerEntity method implementation
        /// </summary>
        private void CreateWindowsClaimsResolvedPickerEntity(ProxyResultObject usr, List<PickerEntity> resolved)
        {
            PickerEntity entity = CreatePickerEntity();
            string IdentityValue = string.Empty;
            if (usr is ProxyUser)
            {
                ProxyUser xusr = usr as ProxyUser;
                IdentityValue = xusr.SamAaccount;
                if (string.IsNullOrEmpty(IdentityValue))
                    return;
                entity.Claim = CreateClaimForSTS(this._useridentityclaim, IdentityValue);
                entity.EntityType = SPClaimEntityTypes.User;
                entity.EntityData[PeopleEditorEntityDataKeys.AccountName] = IdentityValue;
                if (!string.IsNullOrEmpty(xusr.EmailAddress))
                    entity.EntityData[PeopleEditorEntityDataKeys.Email] = xusr.EmailAddress;
                if (!string.IsNullOrEmpty(xusr.MobilePhone))
                    entity.EntityData[PeopleEditorEntityDataKeys.MobilePhone] = xusr.MobilePhone;
                if (!string.IsNullOrEmpty(xusr.WorkPhone))
                    entity.EntityData[PeopleEditorEntityDataKeys.WorkPhone] = xusr.WorkPhone;
                if (!string.IsNullOrEmpty(xusr.Department))
                    entity.EntityData[PeopleEditorEntityDataKeys.Department] = xusr.Department;
                if (!string.IsNullOrEmpty(xusr.JobTitle))
                    entity.EntityData[PeopleEditorEntityDataKeys.JobTitle] = xusr.JobTitle;
                if (!string.IsNullOrEmpty(xusr.Location))
                    entity.EntityData[PeopleEditorEntityDataKeys.Location] = xusr.Location;
                if (!string.IsNullOrEmpty(xusr.SIPAddress))
                    entity.EntityData[PeopleEditorEntityDataKeys.SIPAddress] = xusr.SIPAddress;
                if (!string.IsNullOrEmpty(xusr.PictureUrl))
                    entity.EntityData["Picture"] = xusr.PictureUrl;

                switch (this._claimsdisplaymode)
                {
                    case ProxyClaimsDisplayMode.SAMAccount:
                        entity.DisplayText = xusr.SamAaccount;
                        entity.Description = xusr.SamAaccount;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.SamAaccount;
                        break;
                    case ProxyClaimsDisplayMode.UPN:
                        if (string.IsNullOrEmpty(xusr.UserPrincipalName))
                            xusr.UserPrincipalName = IdentityValue;
                        entity.DisplayText = xusr.UserPrincipalName;
                        entity.Description = xusr.UserPrincipalName;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.UserPrincipalName;
                        break;
                    case ProxyClaimsDisplayMode.Email:
                        if (string.IsNullOrEmpty(xusr.EmailAddress))
                            xusr.EmailAddress = IdentityValue;
                        entity.DisplayText = xusr.EmailAddress;
                        entity.Description = xusr.EmailAddress;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.EmailAddress;
                        break;
                    case ProxyClaimsDisplayMode.DisplayName:
                        if (string.IsNullOrEmpty(xusr.DisplayName))
                            xusr.DisplayName = IdentityValue;
                        entity.DisplayText = xusr.DisplayName;
                        entity.Description = xusr.DisplayName;
                        entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName;
                        break;
                    case ProxyClaimsDisplayMode.DisplayNameAndEmail:
                        if (string.IsNullOrEmpty(xusr.DisplayName))
                            xusr.DisplayName = IdentityValue;
                        if (string.IsNullOrEmpty(xusr.EmailAddress))
                        {
                            entity.DisplayText = xusr.DisplayName;
                            entity.Description = xusr.DisplayName;
                            entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName;
                        }
                        else
                        {
                            entity.DisplayText = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                            entity.Description = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                            entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xusr.DisplayName + " (" + xusr.EmailAddress + ")";
                        }
                        break;
                }
                entity.IsResolved = true;
                resolved.Add(entity);
                return;
            }
            else
            {
                ProxyRole xcl = usr as ProxyRole;
                IdentityValue = xcl.SID;
                if (string.IsNullOrEmpty(IdentityValue))
                    return;
                entity.Claim = CreateClaimForSTS(this._roleidentityclaim, IdentityValue);
                entity.EntityType = SPClaimEntityTypes.SecurityGroup;
                entity.Description = xcl.SamAaccount;
                entity.DisplayText = xcl.SamAaccount;
                entity.EntityData[PeopleEditorEntityDataKeys.AccountName] = xcl.SamAaccount;
                entity.EntityData[PeopleEditorEntityDataKeys.DisplayName] = xcl.SamAaccount;
                entity.EntityData[PeopleEditorEntityDataKeys.Email] = xcl.EmailAddress;
                entity.IsResolved = true;
                resolved.Add(entity);
                return;
            }
        }
#endregion

#region FillHierarchy

        /// <summary>
        /// FillHierarchy method implementation
        /// </summary>
        protected override void FillHierarchy(Uri context, string[] entityTypes, string hierarchyNodeID, int numberOfLevels, SPProviderHierarchyTree hierarchy)
        {
            if ((!EntityTypesContain(entityTypes, SPClaimEntityTypes.User)) && (!EntityTypesContain(entityTypes, SPClaimEntityTypes.Trusted)))
            {
                LogEvent.Trace(ResourcesValues.GetString("E07007"), EventLogEntryType.Warning, 7007);
                return;
            }
            try
            {
                using (SPMonitoredScope scp = new SPMonitoredScope("IdentityServiceClaimsProvider:FillHierarchy"))
                {
                    EnsureContext();
                    if (IsContextAvailable)
                    {
                        ProxyDomain dom = _ad.FillHierarchy(hierarchyNodeID, numberOfLevels);
                        if (dom != null)
                        {
                            foreach (ProxyDomain d in dom.Domains)
                            {
                                if (d.IsReacheable)
                                {
                                    if (string.IsNullOrEmpty(d.DisplayName))
                                        hierarchy.AddChild(new SPProviderHierarchyNode(InternalGetDisplayName(), d.DnsName, d.DnsName, false));
                                    else
                                        hierarchy.AddChild(new SPProviderHierarchyNode(InternalGetDisplayName(), d.DisplayName, d.DisplayName, false));
                                       // hierarchy.AddChild(new SPProviderHierarchyNode(InternalGetDisplayName(), d.DisplayName, d.DnsName, false));
                                }
                            }
                        }
                        else
                            LogEvent.Trace(ResourcesValues.GetString("E07008"), EventLogEntryType.Warning, 7008);
                    }
                }
            }
            catch (Exception E)
            {
                LogEvent.Log(E, ResourcesValues.GetString("E07003"), EventLogEntryType.Warning, 7003);
                throw E;
            }
        }
#endregion
    }
#endregion
}
