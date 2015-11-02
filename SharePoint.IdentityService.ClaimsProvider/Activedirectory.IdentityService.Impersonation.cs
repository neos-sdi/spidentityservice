using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SharePoint.IdentityService.ClaimsProvider
{
    // group type enum
    public enum SECURITY_IMPERSONATION_LEVEL : int
    {
        SecurityAnonymous = 0,
        SecurityIdentification = 1,
        SecurityImpersonation = 2,
        SecurityDelegation = 3
    }

    public sealed class Identity : IDisposable
    {
        const int LOGON32_PROVIDER_DEFAULT = 0;

        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;
        const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        const int LOGON_TYPE_NEW_CREDENTIALS = 9;

        /// <summary>
        /// Windows identity used for the Application Pool
        /// </summary>
        private static WindowsIdentity _appPoolIdentity;
        private static WindowsIdentity _userIdentity;

        /// <summary>
        /// Gets the windows identity used for the Application Pool
        /// </summary>
        private static WindowsIdentity AppPoolIdentity
        {
            get
            {
                // Lock current type to ensure thread safety on
                //  identity creation.
                lock (typeof(Identity))
                {
                    if (_appPoolIdentity == null)
                    {
                        // Create a new handle from this one
                        IntPtr token = WindowsIdentity.GetCurrent().Token;

                        // Throw an exception if we have an empty token
                        if (token == IntPtr.Zero)
                        {
                            throw new ApplicationException("Unable to fetch AppPool's identity token !");
                        }

                        // Create a duplicate of the user's token in order to use it for impersonation
                        if (!DuplicateToken(token, 2, ref token))
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to duplicate AppPool's identity token !");
                        }

                        // Throw an exception if we were unable to duplicate the token
                        if (token == IntPtr.Zero)
                        {
                            throw new ApplicationException("Unable to duplicate AppPool's identity token !");
                        }

                        // Store app pool's identity
                        _appPoolIdentity = new WindowsIdentity(token);

                        // Free the windows unmanaged resource
                        CloseHandle(token);
                    }
                    return _appPoolIdentity;
                }
            }
        }

        /// <summary>
        /// Attempts to impersonate a user.  If successful, returns 
        /// a WindowsImpersonationContext of the new users identity.
        /// </summary>
        private static WindowsIdentity UserIdentity(string sUsername, string sPassword)
        {
            // Lock current type to ensure thread safety on
            //  identity creation.
            lock (typeof(Identity))
            {
                if (_userIdentity == null)
                {
                    // initialize tokens
                    IntPtr pExistingTokenHandle = new IntPtr(0);
                    IntPtr pDuplicateTokenHandle = new IntPtr(0);
                    pExistingTokenHandle = IntPtr.Zero;
                    pDuplicateTokenHandle = IntPtr.Zero;

                    string sDomain = null;
                    string[] sz = sUsername.Split('\\');
                    if (sz.Length == 1)
                    {
                        sDomain = System.Environment.MachineName;
                        sUsername = sz[0];
                    }
                    else
                    {
                        sDomain = sz[0];
                        sUsername = sz[1];
                    }
                    try
                    {
                        string sResult = null;
                        bool bImpersonated = LogonUser(sUsername, sDomain, sPassword, LOGON_TYPE_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, ref pExistingTokenHandle);
                        if (false == bImpersonated)
                        {
                            int nErrorCode = Marshal.GetLastWin32Error();
                            sResult = "LogonUser() failed with error code: " + nErrorCode + "\r\n";
                            throw new ApplicationException(sResult);

                        }
                        sResult += "Before impersonation: " + WindowsIdentity.GetCurrent().Name + "\r\n";
                        bool bRetVal = DuplicateToken(pExistingTokenHandle, (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, ref pDuplicateTokenHandle);
                        if (false == bRetVal)
                        {
                            int nErrorCode = Marshal.GetLastWin32Error();
                            CloseHandle(pExistingTokenHandle); // close existing handle
                            sResult += "DuplicateToken() failed with error code: " + nErrorCode + "\r\n";
                            throw new ApplicationException(sResult);
                        }
                        else
                        {
                            _userIdentity = new WindowsIdentity(pDuplicateTokenHandle);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        // close handle(s)
                        if (pExistingTokenHandle != IntPtr.Zero)
                            CloseHandle(pExistingTokenHandle);
                        if (pDuplicateTokenHandle != IntPtr.Zero)
                            CloseHandle(pDuplicateTokenHandle);
                    }
                }
            }
            return _userIdentity;
        }


        /// <summary>
        /// This function returns the current user's login name.
        /// </summary>
        public static string CurrentUserName
        {
            get { return WindowsIdentity.GetCurrent().Name; }
        }

        /// <summary>
        /// Stores the currently available Windows Impersonation context.
        /// </summary>
        private WindowsImpersonationContext _context;

        /// <summary>
        /// Stores the app pool's identity context.
        /// </summary>
        private WindowsImpersonationContext _selfContext;

        /// <summary>
        /// Private constructor, static function accessed class.
        /// </summary>
        private Identity()
        {
            // Try catch structure to ensure we don't change context in case
            //  we had an error duplicating the token.
            try
            {
                _selfContext = WindowsIdentity.Impersonate(IntPtr.Zero); // REVERT to AppPool identity!
                _context = AppPoolIdentity.Impersonate();
            }
            catch
            {
                // Close the context
                UndoImpersonation();

                // Rethrow the exception
                throw;
            }
        }

        /// <summary>
        /// Private constructor, static function accessed class.
        /// </summary>
        private Identity(string suser, string spwd)
        {
            // Try catch structure to ensure we don't change context in case
            //  we had an error duplicating the token.
            try
            {
                _selfContext = WindowsIdentity.Impersonate(IntPtr.Zero); // REVERT to AppPool identity!
                _context = UserIdentity(suser, spwd).Impersonate();
            }
            catch
            {
                // Close the context
                UndoImpersonation();

                // Rethrow the exception
                throw;
            }
        }

        /// <summary>
        /// This method creates a new impersonation context.
        /// </summary>
        public static Identity ImpersonateAdmin()
        {
            return new Identity();
        }

        /// <summary>
        /// This method creates a new impersonation context.
        /// </summary>
        public static Identity Impersonate(string suser, string spwd)
        {
            if (string.IsNullOrEmpty(suser) || string.IsNullOrEmpty(spwd))
                return new Identity();
            else
                return new Identity(suser, spwd);
        }

        /// <summary>
        /// This method closes the current impersonation context in order revert the user
        /// to his real principal.
        /// </summary>
        public void UndoImpersonation()
        {
            if (_context != null)
            {
                _context.Undo();
                _context = null;
            }
            if (_selfContext != null)
            {
                _selfContext.Undo();
                _selfContext = null;
            }
        }

        /// <summary>
        /// Duplicates a token in order to have it working for impersonation.
        /// </summary>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DuplicateToken(IntPtr hToken_, int impersonationLevel_, ref IntPtr hNewToken_);

        /// <summary>
        /// Closes an unmanaged handle in order to free allocated resources.
        /// </summary>
        /// <returns>True if the call succeeded, false otherwise.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(IntPtr handle);

        /// <summary>
        /// Execute the Logon 
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);


        #region IDisposable Members

        /// <summary>
        /// This method disposes the current object, it frees all resources used by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Ensure I'm garbage collected.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This method disposes the current object, it frees all resources used by this class.
        /// </summary>
        /// <param name="disposing_">Do actual disposing or not.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.UndoImpersonation();
            }
        }
        #endregion
    }
}
