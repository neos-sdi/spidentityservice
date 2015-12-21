using Microsoft.SharePoint.Utilities;
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
using System.Collections.Specialized;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SharePoint.IdentityService.AdminLayoutPages
{
    public partial class manageentities : AdminLayoutsPageBase
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
            ServiceDataSource.TypeName = typeof(DomainConfigurationWrapper).AssemblyQualifiedName;
            ServiceDataSource.DataObjectTypeName = typeof(DomainConfigurationWrapper).AssemblyQualifiedName;
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

            LookupDataSource.TypeName = typeof(ConnectionConfigurationWrapper).AssemblyQualifiedName;
            LookupDataSource.DataObjectTypeName = typeof(ConnectionConfigurationWrapper).AssemblyQualifiedName;
            LookupDataSource.SelectMethod = "Select";
            LookupDataSource.Selecting += new ObjectDataSourceSelectingEventHandler(SelectingLookup);
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
                string domainname = ((TextBox)Grid.FooterRow.FindControl("newDisplayName")).Text;
                string compdnsname = ((TextBox)Grid.FooterRow.FindControl("newDnsname")).Text;
                string connection = ((DropDownList)Grid.FooterRow.FindControl("newConnection")).SelectedValue;
                bool enabled = ((CheckBox)Grid.FooterRow.FindControl("newEnabled")).Checked;
                string position = ((TextBox)Grid.FooterRow.FindControl("newPosition")).Text;

                IDataSource odsSrc = (IDataSource)ServiceDataSource;
                ObjectDataSourceView odsView = (ObjectDataSourceView)odsSrc.GetView("DefaultView");

                OrderedDictionary dict = new OrderedDictionary();
                dict.Add("DisplayName", domainname);
                dict.Add("DnsName", compdnsname);
                dict.Add("Connection", connection);
                dict.Add("Enabled", enabled);
                dict.Add("DisplayPosition", position);
                odsView.Insert(dict);
            }
        }

        /// <summary>
        /// Grid_RowDataBound method implementation
        /// </summary>
        protected void Grid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            DomainConfigurationWrapper wr = (e.Row.DataItem as DomainConfigurationWrapper);
            if ((wr!=null) && (wr.DisplayPosition < 0))
                e.Row.Visible = false;
        }

        /// <summary>
        /// Grid_PageIndexChanging implementation
        /// </summary>
        public void Grid_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            Grid.PageIndex = e.NewPageIndex;
        }

        /// <summary>
        /// SelectingLookup method implementation
        /// </summary>
        protected void SelectingLookup(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (!e.ExecutingSelectCount)
            {
                e.InputParameters.Clear();
                e.InputParameters.Add("serviceapplication", ServiceApplication);
            }
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
                DomainConfigurationWrapper wr = prm.Value as DomainConfigurationWrapper;
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
                DomainConfigurationWrapper wr = prm.Value as DomainConfigurationWrapper;
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
                DomainConfigurationWrapper wr = prm.Value as DomainConfigurationWrapper;
                if (wr != null)
                    wr.ServiceApplication = ServiceApplication;
            }
        }
    }

    #region ConfigurationWrapper
    public class DomainConfigurationWrapper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DomainConfigurationWrapper()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DomainConfigurationWrapper(string displayname, string dnsname, string connection, bool enabled, int position)
        {
            this.DisplayName = displayname;
            this.DnsName = dnsname;
            this.Connection = connection;
            this.Enabled = enabled;
            this.DisplayPosition = position;
        }

        public string DisplayName { get; set; }
        public string DnsName { get; set; }
        public bool Enabled { get; set; }
        public string Connection { get; set; }
        public int DisplayPosition { get; set; }
        public IdentityServiceApplication ServiceApplication { get; set; }

        /// <summary>
        /// Select method implementation
        /// </summary>
        public static IEnumerable<DomainConfigurationWrapper> Select(IdentityServiceApplication serviceapplication)
        {
            List<DomainConfigurationWrapper> lst = new List<DomainConfigurationWrapper>();
            List<DomainConfiguration> src = serviceapplication.GetDomainConfigurationList().ToList<DomainConfiguration>();
            foreach (DomainConfiguration dom in src)
            {
                lst.Add(new DomainConfigurationWrapper(dom.DisplayName, dom.DnsName, dom.Connection, dom.Enabled, dom.DisplayPosition));
            }
            if (lst.Count == 0)    // For Showing Footer Row
                lst.Add(new DomainConfigurationWrapper(string.Empty, string.Empty, string.Empty, false, -1));
            return lst;
        }

        /// <summary>
        /// Update method implementation
        /// </summary>
        public static void Update(DomainConfigurationWrapper values, DomainConfigurationWrapper __values)
        {
            __values.ServiceApplication.SetDomainConfiguration(new DomainConfiguration(__values.DisplayName, __values.DnsName, __values.Connection, __values.Enabled, __values.DisplayPosition),
                                                                 new DomainConfiguration(values.DisplayName, values.DnsName, values.Connection, values.Enabled, values.DisplayPosition));
        }

        /// Insert method implementation
        /// </summary>
        public static void Insert(DomainConfigurationWrapper values)
        {
            values.ServiceApplication.SetDomainConfiguration(null, new DomainConfiguration(values.DisplayName, values.DnsName, values.Connection, values.Enabled, values.DisplayPosition));
        }

        /// Delete method implementation
        /// </summary>
        public static void Delete(DomainConfigurationWrapper __values)
        {
            __values.ServiceApplication.DeleteDomainConfiguration(new DomainConfiguration(__values.DisplayName, __values.DnsName, __values.Connection, __values.Enabled, __values.DisplayPosition));
            __values = null;
        }
    }
    #endregion
}
