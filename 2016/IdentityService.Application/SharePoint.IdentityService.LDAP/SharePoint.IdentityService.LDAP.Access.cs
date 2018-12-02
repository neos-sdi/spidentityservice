using SharePoint.IdentityService.Core;
using SharePoint.IdentityService.LDAP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SharePoint.IdentityService.LDAP
{
    #region LDAPGlobalParams class
    public class LDAPGlobalParams : IGlobalParams
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
        private bool _supportsuserkey;


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

        /// <summary>
        /// ClaimProviderSupportsUserKey property implementation
        /// </summary>
        public bool ClaimProviderSupportsUserKey
        {
            get { return _supportsuserkey; }
            set { _supportsuserkey = value; }
        }

    }
    #endregion

    #region LDAPDomainConfigurations
    public class LDAPDomainConfigurations : IDomainConfig
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
        internal LDAPDomainConfigurations()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public LDAPDomainConfigurations(string domainname, string displayname, string username, string password, short timeout, bool enabled, bool secure, int maxrows, int position, string connectstring)
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
            internal set { _domainname = value; }
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
}
