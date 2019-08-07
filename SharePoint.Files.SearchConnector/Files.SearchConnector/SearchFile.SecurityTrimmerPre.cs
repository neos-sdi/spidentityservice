using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Security;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using System.Collections;
using Microsoft.SharePoint;
using System.Web;
using System.IO;
using System.Diagnostics;
using Microsoft.IdentityModel.Claims;


namespace SharePoint.Files.SearchConnector
{
    public class SearchPreTrimmer : ISecurityTrimmerPre
    {
        string _claimUserType = "http://schemas.sharepoint.files.com/ws/2019/06/identity/claims/name";
        string _claimRoleType = "http://schemas.sharepoint.files.com/ws/2019/06/identity/claims/role";
        string _claimInsideCorporateNetwork = "http://schemas.microsoft.com/ws/2012/01/insidecorporatenetwork";
        string _claimUpn = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
        string _claimUserAgent = "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-user-agent";
        string _claimName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        string _claimRole = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        string _claimGroup = "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid";
        string _claimPrimarySID = "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid";
        string _claimPrimaryPPID = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/privatepersonalidentifier";
        string _sidEveryOne = "S-1-1-0";
        string _sidAuthenticatedUsers = "S-1-5-11";

        string _searchfileissuer = "searchfileissuer";
        bool _allowChrome = false;
        bool _allowEdgeChromium = false;
        bool _allowFireFox = false;
        bool _allowIE = true;
        bool _allowOthers = false;

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public void Initialize(NameValueCollection staticProperties, SearchServiceApplication searchApplication)
        {
            if (staticProperties.Get("claimusertype") != null)
            {
                _claimUserType = staticProperties.Get("claimusertype");
            }
            if (staticProperties.Get("claimroletype") != null)
            {
                _claimRoleType = staticProperties.Get("claimroletype");
            }
            if (staticProperties.Get("AllowChrome") != null)
            {
                _allowChrome = bool.Parse(staticProperties.Get("AllowChrome"));
            }
            if (staticProperties.Get("AllowEdgeChromium") != null)
            {
                _allowEdgeChromium = bool.Parse(staticProperties.Get("AllowEdgeChromium"));
            }
            if (staticProperties.Get("AllowFireFox") != null)
            {
                _allowFireFox = bool.Parse(staticProperties.Get("AllowFireFox"));
            }
            if (staticProperties.Get("AllowIE") != null)
            {
                _allowIE = bool.Parse(staticProperties.Get("AllowIE"));
            }
            if (staticProperties.Get("AllowOthers") != null)
            {
                _allowOthers = bool.Parse(staticProperties.Get("AllowOthers"));
            }
        }

        /// <summary>
        /// AddAccess method implementation
        /// </summary>
        public IEnumerable<Tuple<Claim, bool>> AddAccess(IDictionary<string, object> sessionProperties, IIdentity userIdentity)
        {
            List<Tuple<Claim, bool>> lst = new List<Tuple<Claim, bool>>();
            ClaimsIdentity cid = new ClaimsIdentity(userIdentity);
            if (!IsInsideCorporateNetwork(cid.Claims))
              return lst;
            if (!IsBrowserAgentAllowed(cid.Claims))
                return lst;

            foreach (Microsoft.IdentityModel.Claims.Claim cm in cid.Claims)
            {
                if (cm.ClaimType.ToLower().Equals(_claimUpn))
                {
                    string cmvalue = cm.Value.ToUpper();
                    lst.Add(new Tuple<Claim, bool>(new Claim(_claimUserType, cmvalue, ClaimValueTypes.String, _searchfileissuer), false));
                }
                else if (cm.ClaimType.ToLower().Equals(_claimPrimarySID))
                {
                    string cmvalue = cm.Value.ToUpper();
                    lst.Add(new Tuple<Claim, bool>(new Claim(_claimUserType, cmvalue, ClaimValueTypes.String, _searchfileissuer), false));
                }
                else if (cm.ClaimType.ToLower().Equals(_claimPrimaryPPID))
                {
                    string cmvalue = cm.Value.ToUpper();
                    lst.Add(new Tuple<Claim, bool>(new Claim(_claimUserType, cmvalue, ClaimValueTypes.String, _searchfileissuer), false));
                }
                else if (cm.ClaimType.ToLower().Equals(_claimName))
                {
                    string cmvalue = cm.Value.Substring(cm.Value.LastIndexOf('|') + 1);
                    lst.Add(new Tuple<Claim, bool>(new Claim(_claimUserType, cmvalue, ClaimValueTypes.String, _searchfileissuer), false));
                }
                else if (cm.ClaimType.ToLower().Equals(_claimRole))
                {
                    string cmvalue = cm.Value.ToUpper();
                    lst.Add(new Tuple<Claim, bool>(new Claim(_claimRoleType, cmvalue, ClaimValueTypes.String, _searchfileissuer), false));
                }
                else if (cm.ClaimType.ToLower().Equals(_claimGroup))
                {
                    string cmvalue = cm.Value.ToUpper();
                    lst.Add(new Tuple<Claim, bool>(new Claim(_claimRoleType, cmvalue, ClaimValueTypes.String, _searchfileissuer), false));
                }
                lst.Add(new Tuple<Claim, bool>(new Claim(_claimRoleType, _sidEveryOne, ClaimValueTypes.String, _searchfileissuer), false));
                lst.Add(new Tuple<Claim, bool>(new Claim(_claimRoleType, _sidAuthenticatedUsers, ClaimValueTypes.String, _searchfileissuer), false));
            }
            return lst;
        }

        /// <summary>
        /// IsBrowserAgentAllowed
        /// </summary>
        private bool IsBrowserAgentAllowed(ClaimCollection claims)
        {
            foreach (Claim cm in claims)
            {
                if (cm.ClaimType.ToLower().Equals(_claimUserAgent))
                {
                    string useragentvalue = cm.Value;
                    if (useragentvalue.ToLower().Contains(" firefox/"))
                        return _allowFireFox;
                    else if (useragentvalue.ToLower().Contains(" chrome/") && useragentvalue.ToLower().Contains(" edg/"))
                        return _allowEdgeChromium;
                    else if (useragentvalue.ToLower().Contains(" chrome/") && !useragentvalue.ToLower().Contains(" edg/"))
                        return _allowChrome;
                    else if (useragentvalue.ToLower().Contains(" chrome/") && useragentvalue.ToLower().Contains("edge/"))
                        return _allowChrome;
                    else if (useragentvalue.ToLower().Contains("trident/7.0"))
                        return _allowIE;
                    else
                        return _allowOthers;
                }
            }
            return true;
        }

        /// <summary>
        /// IsInsideCorporateNetwork method implementation
        /// </summary>
        private bool IsInsideCorporateNetwork(ClaimCollection claims)
        {
            foreach (Claim cm in claims)
            {
                if (cm.ClaimType.ToLower().Equals(_claimInsideCorporateNetwork))
                {
                    bool result = false;
                    bool.TryParse(cm.Value, out result);
                    return result;
                }
            }
            return false;
        }
    }
}
