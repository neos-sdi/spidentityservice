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
using Microsoft.SharePoint.Utilities;
using SharePoint.IdentityService.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SharePoint.IdentityService.AdminLayoutPages
{
    public partial class manageparams : AdminLayoutsPageBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {
                Page.UnobtrusiveValidationMode = System.Web.UI.UnobtrusiveValidationMode.None;
                Page.DataBind();
            }
        }

        /// <summary>
        /// Init event implmentation
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ServiceDataSource.TypeName = typeof(GlobalParameterWrapper).AssemblyQualifiedName;
            ServiceDataSource.DataObjectTypeName = typeof(GlobalParameterWrapper).AssemblyQualifiedName;
            ServiceDataSource.ConflictDetection = ConflictOptions.CompareAllValues;
            ServiceDataSource.SelectMethod = "Select";
            ServiceDataSource.Selecting += new ObjectDataSourceSelectingEventHandler(SelectingData);
            ServiceDataSource.UpdateMethod = "Update";
            ServiceDataSource.Updating += new ObjectDataSourceMethodEventHandler(UpdatingData);
            ServiceDataSource.OldValuesParameterFormatString = "__{0}";

            DropSourceClaimsDisplayMode.TypeName = typeof(DropDownListWrapper).AssemblyQualifiedName;
            DropSourceClaimsDisplayMode.DataObjectTypeName = typeof(DropDownListWrapper).AssemblyQualifiedName;
            DropSourceClaimsDisplayMode.ConflictDetection = ConflictOptions.CompareAllValues;
            DropSourceClaimsDisplayMode.SelectMethod = "SelectClaimsDisplayMode";
            
            DropSourceClaimIdentityMode.TypeName = typeof(DropDownListWrapper).AssemblyQualifiedName;
            DropSourceClaimIdentityMode.DataObjectTypeName = typeof(DropDownListWrapper).AssemblyQualifiedName;
            DropSourceClaimIdentityMode.ConflictDetection = ConflictOptions.CompareAllValues;
            DropSourceClaimIdentityMode.SelectMethod = "SelectClaimIdentityMode";

            DropSourceClaimRoleMode.TypeName = typeof(DropDownListWrapper).AssemblyQualifiedName;
            DropSourceClaimRoleMode.DataObjectTypeName = typeof(DropDownListWrapper).AssemblyQualifiedName;
            DropSourceClaimRoleMode.ConflictDetection = ConflictOptions.CompareAllValues;
            DropSourceClaimRoleMode.SelectMethod = "SelectClaimRoleMode";

            DropSourceSmoothRequestor.TypeName = typeof(DropDownListWrapper).AssemblyQualifiedName;
            DropSourceSmoothRequestor.DataObjectTypeName = typeof(DropDownListWrapper).AssemblyQualifiedName;
            DropSourceSmoothRequestor.ConflictDetection = ConflictOptions.CompareAllValues;
            DropSourceSmoothRequestor.SelectMethod = "SelectSmoothRequestor";

            DropSourceClaimsMode.TypeName = typeof(DropDownListWrapper).AssemblyQualifiedName;
            DropSourceClaimsMode.DataObjectTypeName = typeof(DropDownListWrapper).AssemblyQualifiedName;
            DropSourceClaimsMode.ConflictDetection = ConflictOptions.CompareAllValues;
            DropSourceClaimsMode.SelectMethod = "SelectClaimsMode";

            Grid.PagerTemplate = null;
            this.RETURNBACK.NavigateUrl = string.Format("~/_layouts/15/SharePoint.IdentityService/manageapp.aspx?id={0}", GetID());
        }

        /// <summary>
        /// Grid_ModeChanging method implmentation
        /// </summary>
        protected void Grid_ModeChanging(object sender, FormViewModeEventArgs e)
        {
            switch (e.NewMode)
            {
                case FormViewMode.Insert:
                    e.Cancel = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Grid_DataBound methog implementation
        /// </summary>
        protected void Grid_DataBound(object sender, EventArgs e)
        {
            DropDownList mde = (DropDownList)(this.Grid.FindControl("txtClaimIdentityMode"));
            DropDownList rle = (DropDownList)(this.Grid.FindControl("txtClaimRoleMode"));
            TextBox tiv = (TextBox)(this.Grid.FindControl("txtClaimIdentityValue"));
            TextBox trv = (TextBox)(this.Grid.FindControl("txtClaimRoleValue"));

            if (Grid.CurrentMode == FormViewMode.Edit)
            {
                TextBox txt = (TextBox)(this.Grid.FindControl("txtTrustedLoginProviderName"));
                if (txt.Text.ToLower().Equals("ad"))
                {
                    mde.Enabled = false;
                    rle.Enabled = false;
                    tiv.ReadOnly = true;
                    tiv.BorderStyle = BorderStyle.None;
                    trv.ReadOnly = true;
                    trv.BorderStyle = BorderStyle.None;
                }
                else
                {
                    mde.Enabled = true;
                    rle.Enabled = true;
                    tiv.ReadOnly = false;
                    tiv.BorderStyle = BorderStyle.NotSet;
                    trv.ReadOnly = false;
                    trv.BorderStyle = BorderStyle.NotSet;
                }
            }
            else
            {
                mde.Enabled = false;
                rle.Enabled = false;
                tiv.ReadOnly = true;
                tiv.BorderStyle = BorderStyle.None;
                trv.ReadOnly = true;
                trv.BorderStyle = BorderStyle.None;
            }
        }

        /// <summary>
        /// SelectingData method implementation
        /// </summary>
        protected void SelectingData(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (!CheckModifyAccess())
            {
                e.Cancel = true;
                SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to call this operation."));
            }
            if (!e.ExecutingSelectCount)
            {
                e.InputParameters.Clear();
                e.InputParameters.Add("serviceapplication", ServiceApplication);
            }
        }

        /// <summary>
        /// UpdatingData method implementation
        /// </summary>
        protected void UpdatingData(object sender, ObjectDataSourceMethodEventArgs e)
        {
            if (!CheckModifyAccess())
            {
                e.Cancel = true;
                SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to call this operation."));
            }
            foreach (System.Collections.DictionaryEntry prm in e.InputParameters)
            {
                GlobalParameterWrapper wr = prm.Value as GlobalParameterWrapper;
                if (wr != null)
                    wr.ServiceApplication = ServiceApplication;
            }
        }
    }

    #region ConfigurationWrapper
    public class GlobalParameterWrapper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GlobalParameterWrapper()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GlobalParameterWrapper(int cacheduration, ProxyClaimsDisplayMode claimsdisplaymode, string claimdisplayname, ProxyClaimsIdentityMode claimidentitymode, string claimidentity, string claimprovidername,
                               ProxyClaimsRoleMode claimrolemode, string claimrole, ProxyClaimsMode claimsmode, ProxyClaimsDisplayMode peoplepickerdisplaymode, bool peoplepickerimages, bool searchbydisplayname,
                               bool searchbymail, bool showsystemnodes, ProxySmoothRequest smoothrequestor, string trustedloginprovidername)
        {
            this.CacheDuration = cacheduration;
            this.ClaimsDisplayMode = claimsdisplaymode;
            this.ClaimDisplayName = claimdisplayname;
            this.ClaimIdentityMode = claimidentitymode;
            this.ClaimIdentity = claimidentity;
            this.ClaimProviderName = claimprovidername;
            this.ClaimRoleMode = claimrolemode;
            this.ClaimRole = claimrole;
            this.ClaimsMode = claimsmode;
            this.PeoplePickerDisplayMode = peoplepickerdisplaymode;
            this.PeoplePickerImages = peoplepickerimages;
            this.SearchByDisplayName = searchbydisplayname;
            this.SearchByMail = searchbymail;
            this.ShowSystemNodes = showsystemnodes;
            this.SmoothRequestor = smoothrequestor;
            this.TrustedLoginProviderName = trustedloginprovidername;
        }

        public int CacheDuration { get; set; }
        public ProxyClaimsDisplayMode ClaimsDisplayMode { get; set; }
        public string ClaimDisplayName { get; set; }
        public ProxyClaimsIdentityMode ClaimIdentityMode { get; set; }
        public string ClaimIdentity { get; set; }
        public string ClaimProviderName { get; set; }
        public ProxyClaimsRoleMode ClaimRoleMode { get; set; }
        public string ClaimRole { get; set; }
        public ProxyClaimsMode ClaimsMode { get; set; }
        public ProxyClaimsDisplayMode PeoplePickerDisplayMode { get; set; }
        public bool PeoplePickerImages { get; set; }
        public bool SearchByDisplayName { get; set; }
        public bool SearchByMail { get; set; }
        public bool ShowSystemNodes { get; set; }
        public ProxySmoothRequest SmoothRequestor { get; set; }
        public string TrustedLoginProviderName { get; set; }

        public IdentityServiceApplication ServiceApplication { get; set; }

        /// <summary>
        /// Select method implementation
        /// </summary>
        public static IEnumerable<GlobalParameterWrapper> Select(IdentityServiceApplication serviceapplication)
        {
            List<GlobalParameterWrapper> lst = new List<GlobalParameterWrapper>();
            List<GlobalParameter> src = serviceapplication.GetGlobalParameterList().ToList<GlobalParameter>();
            foreach (GlobalParameter glb in src)
            {
                lst.Add(new GlobalParameterWrapper(glb.CacheDuration, glb.ClaimsDisplayMode, glb.ClaimDisplayName, glb.ClaimIdentityMode, glb.ClaimIdentity, glb.ClaimProviderName, glb.ClaimRoleMode, glb.ClaimRole, glb.ClaimsMode, glb.PeoplePickerDisplayMode, glb.PeoplePickerImages, glb.SearchByDisplayName, glb.SearchByMail, glb.ShowSystemNodes, glb.SmoothRequestor, glb.TrustedLoginProviderName));
            }
            return lst;
        }

        /// <summary>
        /// Update method implementation
        /// </summary>
        public static void Update(GlobalParameterWrapper values, GlobalParameterWrapper __values)
        {
            __values.ServiceApplication.SetGlobalParameter(new GlobalParameter(__values.CacheDuration, __values.ClaimsDisplayMode, __values.ClaimDisplayName, __values.ClaimIdentityMode, __values.ClaimIdentity, __values.ClaimProviderName, __values.ClaimRoleMode, __values.ClaimRole, __values.ClaimsMode, __values.PeoplePickerDisplayMode, __values.PeoplePickerImages, __values.SearchByDisplayName, __values.SearchByMail, __values.ShowSystemNodes, __values.SmoothRequestor, __values.TrustedLoginProviderName),
                                                           new GlobalParameter(values.CacheDuration, values.ClaimsDisplayMode, values.ClaimDisplayName, values.ClaimIdentityMode, values.ClaimIdentity, values.ClaimProviderName, values.ClaimRoleMode, values.ClaimRole, values.ClaimsMode, values.PeoplePickerDisplayMode, values.PeoplePickerImages, values.SearchByDisplayName, values.SearchByMail, values.ShowSystemNodes, values.SmoothRequestor, values.TrustedLoginProviderName));
        }
    }
    #endregion

    #region DropDownListWrapper
    public class DropDownListWrapper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DropDownListWrapper()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DropDownListWrapper(string value, string text)
        {
            this.Value = value;
            this.Text = text;
        }

        public string Value { get; set; }
        public string Text { get; set; }

        /// <summary>
        /// GetUIString method implementation
        /// </summary>
        public string GetUIString(string formatstr)
        {
            return SPUtility.GetLocalizedString("$Resources:" + formatstr, "SharePoint.IdentityService.Administration", Convert.ToUInt32(Thread.CurrentThread.CurrentUICulture.LCID));
        }


        public IEnumerable<DropDownListWrapper> SelectClaimsDisplayMode()
        {
            List<DropDownListWrapper> lst = new List<DropDownListWrapper>();
            lst.Add(new DropDownListWrapper("DisplayName", GetUIString("PRMCLAIMSDROPDISPLAYNAME")));
            lst.Add(new DropDownListWrapper("Email", GetUIString("PRMCLAIMSDROPEMAIL")));
            lst.Add(new DropDownListWrapper("UPN", GetUIString("PRMCLAIMSDROPUPN")));
            lst.Add(new DropDownListWrapper("SAMAccount", GetUIString("PRMCLAIMSDROPSAM")));
            lst.Add(new DropDownListWrapper("DisplayNameAndEmail", GetUIString("PRMCLAIMSDROPDESCEMAIL")));
            return lst;
        }

        public IEnumerable<DropDownListWrapper> SelectClaimIdentityMode()
        {
            List<DropDownListWrapper> lst = new List<DropDownListWrapper>();
            lst.Add(new DropDownListWrapper("Email", GetUIString("PRMCLAIMSDROPEMAIL")));
            lst.Add(new DropDownListWrapper("UserPrincipalName", GetUIString("PRMCLAIMSDROPUPN")));
            lst.Add(new DropDownListWrapper("SAMAccount", GetUIString("PRMCLAIMSDROPSAM")));
            return lst;
        }

        public IEnumerable<DropDownListWrapper> SelectClaimRoleMode()
        {
            List<DropDownListWrapper> lst = new List<DropDownListWrapper>();
            lst.Add(new DropDownListWrapper("SID", GetUIString("PRMCLAIMSDROPSID")));
            lst.Add(new DropDownListWrapper("Role", GetUIString("PRMCLAIMSDROPROLE")));
            return lst;
        }

        public IEnumerable<DropDownListWrapper> SelectSmoothRequestor()
        {
            List<DropDownListWrapper> lst = new List<DropDownListWrapper>();
            lst.Add(new DropDownListWrapper("Strict", GetUIString("PRMCLAIMSQRYSTRICT")));
            lst.Add(new DropDownListWrapper("StarsBefore", GetUIString("PRMCLAIMSQRYBEF")));
            lst.Add(new DropDownListWrapper("StarsAfter", GetUIString("PRMCLAIMSQRYAFT")));
            lst.Add(new DropDownListWrapper("Smooth", GetUIString("PRMCLAIMSQRYFULL")));
            return lst;
        }

        public IEnumerable<DropDownListWrapper> SelectClaimsMode()
        {
            List<DropDownListWrapper> lst = new List<DropDownListWrapper>();
            lst.Add(new DropDownListWrapper("Windows", GetUIString("PRMCLAIMSDROPWIN")));
            lst.Add(new DropDownListWrapper("Federated", GetUIString("PRMCLAIMSDROPFED")));
            return lst;
        }
    }
    #endregion
}
