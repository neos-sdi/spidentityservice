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
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using Microsoft.SharePoint.Utilities;

namespace SharePoint.IdentityService.AdminLayoutPages
{
    public partial class manageconnections : AdminLayoutsPageBase
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
            ServiceDataSource.TypeName = typeof(ConnectionConfigurationWrapper).AssemblyQualifiedName;
            ServiceDataSource.DataObjectTypeName = typeof(ConnectionConfigurationWrapper).AssemblyQualifiedName;
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
                string connectionid = ((TextBox)Grid.FooterRow.FindControl("newConnectorID")).Text;
                string connectionname = ((TextBox)Grid.FooterRow.FindControl("newConnectionName")).Text;
                string username = ((TextBox)Grid.FooterRow.FindControl("newUserName")).Text;
                string password = ((TextBox)Grid.FooterRow.FindControl("newPassword")).Text;
                string timeout = ((TextBox)Grid.FooterRow.FindControl("newTimeOut")).Text;
                bool secure = ((CheckBox)Grid.FooterRow.FindControl("newSecure")).Checked;
                string maxrows = ((TextBox)Grid.FooterRow.FindControl("newMaxRows")).Text;
                string connectstring = ((TextBox)Grid.FooterRow.FindControl("newConnectString")).Text; 

                IDataSource odsSrc = (IDataSource)ServiceDataSource;
                ObjectDataSourceView odsView = (ObjectDataSourceView)odsSrc.GetView("DefaultView");

                OrderedDictionary dict = new OrderedDictionary();
                dict.Add("ConnectorID", connectionid);
                dict.Add("ConnectionName", connectionname);
                dict.Add("Username", username);
                dict.Add("Password",password);
                dict.Add("Timeout", timeout);
                dict.Add("Secure", secure);
                dict.Add("Maxrows", maxrows);
                dict.Add("ConnectString", connectstring);
                odsView.Insert(dict);
            }
        }

        /// <summary>
        /// Grid_RowDataBound implementation
        /// </summary>
        protected void Grid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            ConnectionConfigurationWrapper wr = (e.Row.DataItem as ConnectionConfigurationWrapper);
            if ((wr != null) && (string.IsNullOrEmpty(wr.ConnectionName)))
                e.Row.Visible = false;

        }

        /// <summary>
        /// Grid_PageIndexChanging implementation
        /// </summary>
        protected void Grid_PageIndexChanging(object sender, GridViewPageEventArgs e)
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
                ConnectionConfigurationWrapper wr = prm.Value as ConnectionConfigurationWrapper;
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
                ConnectionConfigurationWrapper wr = prm.Value as ConnectionConfigurationWrapper;
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
                ConnectionConfigurationWrapper wr = prm.Value as ConnectionConfigurationWrapper;
                if (wr != null)
                    wr.ServiceApplication = ServiceApplication;
            }
        }
    }

    #region ConfigurationWrapper
    public class ConnectionConfigurationWrapper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectionConfigurationWrapper()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectionConfigurationWrapper(Int64 id, string connectionname, string username, string password, Int16 timeout, bool secure, int maxrows, string connectstring)
        {
            this.ConnectorID = id;
            this.ConnectionName = connectionname;
            this.Username = username;
            this.Password = password;
            this.Timeout = timeout;
            this.Secure = secure;
            this.Maxrows = maxrows;
            this.ConnectString = connectstring;
        }

        public string ConnectionName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Int16 Timeout { get; set; }
        public bool Secure { get; set; }
        public int Maxrows { get; set; }
        public string ConnectString { get; set; }
        public Int64 ConnectorID { get; set; }
        public IdentityServiceApplication ServiceApplication { get; set; }

        /// <summary>
        /// Select method implementation
        /// </summary>
        public static IEnumerable<ConnectionConfigurationWrapper> Select(IdentityServiceApplication serviceapplication)
        {
            List<ConnectionConfigurationWrapper> lst = new List<ConnectionConfigurationWrapper>();
            List<ConnectionConfiguration> src = serviceapplication.GetConnectionConfigurationList().ToList<ConnectionConfiguration>();
            foreach (ConnectionConfiguration dom in src)
            {
                lst.Add(new ConnectionConfigurationWrapper(dom.ConnectorID, dom.ConnectionName, dom.Username, dom.Password, dom.Timeout, dom.Secure, dom.Maxrows, dom.ConnectString));
            }
            if (lst.Count == 0)
                lst.Add(new ConnectionConfigurationWrapper());
            return lst;
        }

        /// <summary>
        /// Update method implementation
        /// </summary>
        public static void Update(ConnectionConfigurationWrapper values, ConnectionConfigurationWrapper __values)
        {
            __values.ServiceApplication.SetConnectionConfiguration(new ConnectionConfiguration(__values.ConnectionName, __values.Username, __values.Password, __values.Timeout, __values.Secure, __values.Maxrows, __values.ConnectString, __values.ConnectorID),
                                                                   new ConnectionConfiguration(values.ConnectionName, values.Username, values.Password, values.Timeout, values.Secure, values.Maxrows, values.ConnectString, values.ConnectorID));
        }

        /// Insert method implementation
        /// </summary>
        public static void Insert(ConnectionConfigurationWrapper values)
        {
            values.ServiceApplication.SetConnectionConfiguration(null, new ConnectionConfiguration(values.ConnectionName, values.Username, values.Password, values.Timeout, values.Secure, values.Maxrows, values.ConnectString, values.ConnectorID));
        }

        /// Delete method implementation
        /// </summary>
        public static void Delete(ConnectionConfigurationWrapper __values)
        {
            __values.ServiceApplication.DeleteConnectionConfiguration(new ConnectionConfiguration(__values.ConnectionName, __values.Username, __values.Password, __values.Timeout, __values.Secure, __values.Maxrows, __values.ConnectString, __values.ConnectorID));
            __values = null;
        }
    }
    #endregion
}
