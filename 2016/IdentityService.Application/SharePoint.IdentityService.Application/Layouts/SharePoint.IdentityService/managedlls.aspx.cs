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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SharePoint.IdentityService.AdminLayoutPages
{
    public partial class managedlls : AdminLayoutsPageBase
    {
        /// <summary>
        /// Load event implementation
        /// </summary>
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
            ServiceDataSource.TypeName = typeof(AssemblyConfigurationWrapper).AssemblyQualifiedName;
            ServiceDataSource.DataObjectTypeName = typeof(AssemblyConfigurationWrapper).AssemblyQualifiedName;
            ServiceDataSource.ConflictDetection = ConflictOptions.CompareAllValues;
            ServiceDataSource.SelectMethod = "Select";
            ServiceDataSource.Selecting += new ObjectDataSourceSelectingEventHandler(SelectingData);
            ServiceDataSource.UpdateMethod = "Update";
            ServiceDataSource.Updating += new ObjectDataSourceMethodEventHandler(UpdatingData);
            ServiceDataSource.DeleteMethod = "Delete";
            ServiceDataSource.Deleting += new ObjectDataSourceMethodEventHandler(DeletingData);
            ServiceDataSource.InsertMethod = "Insert";
            ServiceDataSource.Inserting += new ObjectDataSourceMethodEventHandler(InsertingData);
            ServiceDataSource.OldValuesParameterFormatString = "__{0}";
            Grid.PagerTemplate = null;
            this.RETURNBACK.NavigateUrl = string.Format("~/_layouts/15/SharePoint.IdentityService/manageapp.aspx?id={0}", GetID());
        }

        /// <summary>
        /// RowCommand implmentations
        /// </summary>
        protected void Grid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "New")
            {
                if (!CheckModifyAccess())
                {
                    e.Handled = false;
                    SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to access this page."));
                }
                string connectorid = ((TextBox)Grid.FooterRow.FindControl("newConnectorID")).Text;
                string assembly = ((TextBox)Grid.FooterRow.FindControl("newAssemblyFulldescription")).Text;
                string comptype = ((TextBox)Grid.FooterRow.FindControl("newAssemblyTypeDescription")).Text;
                bool selected = ((CheckBox)Grid.FooterRow.FindControl("cbnSelected")).Checked;
                bool trace = ((CheckBox)Grid.FooterRow.FindControl("cbnTrace")).Checked;
                bool claims = ((CheckBox)Grid.FooterRow.FindControl("cbnClaims")).Checked;
                if (CheckIsValidAssembly(assembly, comptype, !claims))
                {
                    IDataSource odsSrc = (IDataSource)ServiceDataSource;
                    ObjectDataSourceView odsView = (ObjectDataSourceView)odsSrc.GetView("DefaultView");

                    OrderedDictionary dict = new OrderedDictionary();
                    dict.Add("ConnectorID", connectorid);
                    dict.Add("AssemblyFulldescription", assembly);
                    dict.Add("AssemblyTypeDescription", comptype);
                    dict.Add("Selected", selected);
                    dict.Add("TraceResolve", trace);
                    dict.Add("ClaimsExt", claims);
                    odsView.Insert(dict);
                }
                else
                {
                    IValidator cv1 = this.Page.Validators[0];
                    IValidator cv2 = this.Page.Validators[1];
                    cv1.IsValid = false;
                    cv2.IsValid = false;
                    cv1.ErrorMessage = GetUIString("DLLVALIDATORADDMESSAGE"); //"Invalid Assembly or Component type. \nThis reference can't be trusted";
                    if (claims)
                        cv2.ErrorMessage = GetUIString("DLLVALIDATORDETMESSAGE1"); //"You must implement SharePoint.IdentityService.Core.IIdentityServiceClaimsAugmenter in your assembly and deploy it.";
                    else
                        cv2.ErrorMessage = GetUIString("DLLVALIDATORDETMESSAGE2"); // "You must implement SharePoint.IdentityService.Core.IWrapper in your assembly and deploy it";
                    e.Handled = false;
                }
            }
        }

        /// <summary>
        /// Grid_RowUpdating implementation
        /// </summary>
        protected void Grid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            string connectorid = ((TextBox)Grid.Rows[e.RowIndex].FindControl("txtConnectorID")).Text;
            string assembly = ((TextBox)Grid.Rows[e.RowIndex].FindControl("txtAssemblyFulldescription")).Text;
            string comptype = ((TextBox)Grid.Rows[e.RowIndex].FindControl("txtAssemblyTypeDescription")).Text;
            bool selected = ((CheckBox)Grid.Rows[e.RowIndex].FindControl("cbaSelected")).Checked;
            bool trace = ((CheckBox)Grid.Rows[e.RowIndex].FindControl("cbaTrace")).Checked;
            bool claims = ((CheckBox)Grid.Rows[e.RowIndex].FindControl("cbaClaims")).Checked;
            if (!CheckIsValidAssembly(assembly, comptype, !claims))
            {
                IValidator cv1 = this.Page.Validators[0];
                IValidator cv2 = this.Page.Validators[1];
                cv1.IsValid = false;
                cv2.IsValid = false;
                cv1.ErrorMessage = GetUIString("DLLVALIDATORADDMESSAGE"); //"Invalid Assembly or Component type. \nThis reference can't be trusted";
                if (claims)
                    cv2.ErrorMessage = GetUIString("DLLVALIDATORDETMESSAGE1"); //"You must implement SharePoint.IdentityService.Core.IIdentityServiceClaimsAugmenter in your assembly and deploy it.";
                else
                    cv2.ErrorMessage = GetUIString("DLLVALIDATORDETMESSAGE2"); // "You must implement SharePoint.IdentityService.Core.IWrapper in your assembly and deploy it";
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Grid_RowDataBound method imlementation
        /// </summary>
        protected void Grid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            AssemblyConfigurationWrapper wr = (e.Row.DataItem as AssemblyConfigurationWrapper);
            if ((wr != null) && (string.IsNullOrEmpty(wr.AssemblyFulldescription)))
                e.Row.Visible = false;
        }


        /// <summary>
        /// CheckIsValidAssembly method imlementation
        /// </summary>
        private bool CheckIsValidAssembly(string _assembly, string _type, bool _iswrapper)
        {
            bool result = false;
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    Assembly assembly = Assembly.Load(_assembly);
                    Type _typetoload = assembly.GetType(_type);
                    if (_iswrapper)
                        result = (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IWrapper") != null);
                    else
                        result = (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IIdentityServiceClaimsAugmenter") != null);
                });
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Grid_PageIndexChanging implementation
        /// </summary>
        public void Grid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            Grid.PageIndex = e.NewPageIndex;
        }

        /// <summary>
        /// SelectingData method implementation
        /// </summary>
        protected void SelectingData(object sender, ObjectDataSourceSelectingEventArgs e)
        {
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
                AssemblyConfigurationWrapper wr = prm.Value as AssemblyConfigurationWrapper;
                if (wr != null)
                    wr.ServiceApplication = ServiceApplication;
            }
        }

        /// <summary>
        /// DeletingData method implementation
        /// </summary>
        protected void DeletingData(object sender, ObjectDataSourceMethodEventArgs e)
        {
            if (!CheckModifyAccess())
            {
                e.Cancel = true;
                SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to call this operation."));
            }
            foreach (System.Collections.DictionaryEntry prm in e.InputParameters)
            {
                AssemblyConfigurationWrapper wr = prm.Value as AssemblyConfigurationWrapper;
                if (wr != null)
                    wr.ServiceApplication = ServiceApplication;
            }
        }

        /// <summary>
        /// InsertingData method implementation
        /// </summary>
        protected void InsertingData(object sender, ObjectDataSourceMethodEventArgs e)
        {
            if (!CheckModifyAccess())
            {
                e.Cancel = true;
                SPUtility.HandleAccessDenied(new UnauthorizedAccessException("You are not authorized to call this operation."));
            }
            foreach (System.Collections.DictionaryEntry prm in e.InputParameters)
            {
                AssemblyConfigurationWrapper wr = prm.Value as AssemblyConfigurationWrapper;
                if (wr != null)
                    wr.ServiceApplication = ServiceApplication;
            }
        }
    }

    #region ConfigurationWrapper
    public class AssemblyConfigurationWrapper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AssemblyConfigurationWrapper()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AssemblyConfigurationWrapper(Int64 id, string assemblydesc, string typedesc, bool selected = false, bool trace = false, bool augment = false)
        {
            this.ConnectorID = id;
            this.AssemblyFulldescription = assemblydesc;
            this.AssemblyTypeDescription = typedesc;
            this.Selected = selected;
            this.TraceResolve = trace;
            this.ClaimsExt = augment;
        }

        public string AssemblyFulldescription { get; set; }
        public string AssemblyTypeDescription { get; set; }
        public bool Selected { get; set; }
        public bool TraceResolve { get; set; }
        public bool ClaimsExt { get; set; }
        public Int64 ConnectorID { get; set; }
        public IdentityServiceApplication ServiceApplication { get; set; }

        /// <summary>
        /// Select method implementation
        /// </summary>
        public static IEnumerable<AssemblyConfigurationWrapper> Select(IdentityServiceApplication serviceapplication)
        {
            List<AssemblyConfigurationWrapper> lst = new List<AssemblyConfigurationWrapper>();
            List<AssemblyConfiguration> src = serviceapplication.GetAssemblyConfigurationList().ToList<AssemblyConfiguration>();
            foreach (AssemblyConfiguration ass in src)
            {
                lst.Add(new AssemblyConfigurationWrapper(ass.ID, ass.AssemblyFulldescription, ass.AssemblyTypeDescription, ass.Selected, ass.TraceResolve, ass.ClaimsExt));
            }
            if (lst.Count == 0)
                lst.Add(new AssemblyConfigurationWrapper());
            return lst;
        }

        /// <summary>
        /// Update method implementation
        /// </summary>
        public static void Update(AssemblyConfigurationWrapper values, AssemblyConfigurationWrapper __values)
        {
            __values.ServiceApplication.SetAssemblyConfiguration(new AssemblyConfiguration(__values.ConnectorID, __values.AssemblyFulldescription, __values.AssemblyTypeDescription, __values.Selected, __values.TraceResolve, __values.ClaimsExt), 
                                                                 new AssemblyConfiguration(values.ConnectorID, values.AssemblyFulldescription, values.AssemblyTypeDescription, values.Selected, values.TraceResolve, values.ClaimsExt));
        }

        /// Insert method implementation
        /// </summary>
        public static void Insert(AssemblyConfigurationWrapper values)
        {
            values.ServiceApplication.SetAssemblyConfiguration(null, new AssemblyConfiguration(values.ConnectorID, values.AssemblyFulldescription, values.AssemblyTypeDescription, values.Selected, values.TraceResolve, values.ClaimsExt));
        }

        /// Delete method implementation
        /// </summary>
        public static void Delete(AssemblyConfigurationWrapper __values)
        {
            __values.ServiceApplication.DeleteAssemblyConfiguration(new AssemblyConfiguration(__values.ConnectorID, __values.AssemblyFulldescription, __values.AssemblyTypeDescription, __values.Selected, __values.TraceResolve, __values.ClaimsExt));
            __values = null;
        }
    }
    #endregion
}
