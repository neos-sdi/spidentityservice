<%@ Assembly Name="SharePoint.IdentityService.Application, Version=15.0.0.0, Culture=neutral, PublicKeyToken=$SharePoint.Project.AssemblyPublicKeyToken$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="manageapp.aspx.cs" Inherits="SharePoint.IdentityService.AdminLayoutPages.ManageAppPage" MasterPageFile="~/_admin/admin.master" %>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
SharePoint Identity Services
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
<%=GetFormattedTitle("MANAGETITLE") %>
</asp:Content>

<asp:content ID="PageDescription" contentplaceholderid="PlaceHolderPageDescription" runat="server">
<%=GetFormattedTitle("MANAGEDESC") %>
</asp:content>

<asp:content ID="Main" contentplaceholderid="PlaceHolderMain" runat="server">
	<table width="100%" class="propertysheet" cellspacing="0" cellpadding="0" border="0"> 
        <tr> 
            <td class="ms-descriptionText"> 
                <asp:Label ID="LabelMessage" Runat="server" EnableViewState="False" /> 
            </td> 
        </tr> 
        <tr> 
            <td class="ms-error">
                <asp:Label ID="LabelErrorMessage" Runat="server" EnableViewState="False" />
            </td> 
        </tr> 
        <tr> 
            <td class="ms-descriptionText"> 
                <asp:ValidationSummary ID="ValSummary" HeaderText="<%$SPHtmlEncodedResources:spadmin, ValidationSummaryHeaderText%>" DisplayMode="BulletList" ShowSummary="True" runat="server"> 
                </asp:ValidationSummary> 
            </td> 
        </tr> 
    </table>

	<p>
		<span style="font-size:140%"><asp:HyperLink ID="IDPARAMS" Text='<%# GetUIString("MANAGEIDPARAMSTEXT") %>' runat="server" /></span><br />
        <asp:Literal ID="DSPARAMS" runat="server" Text='<%# GetUIString("MANAGEIDPARAMSDESC") %>' />
	</p>
	<p>
		<span style="font-size:140%"><asp:HyperLink ID="IDENTITIES" Text='<%# GetUIString("MANAGEIDENTITIESTEXT") %>' runat="server"/></span><br />
        <asp:Literal ID="DSIDENTITIES" runat="server" Text='<%# GetUIString("MANAGEIDENTITIESDESC") %>' />
	</p>
	<p>
		<span style="font-size:140%"><asp:HyperLink ID="IDCONNECTIONS" Text='<%# GetUIString("MANAGEIDCONNECTIONSTEXT") %>' runat="server"/></span><br />
        <asp:Literal ID="DSCONNECTIONS" runat="server" Text='<%# GetUIString("MANAGEIDCONNECTIONSDESC") %>' />
	</p>
	<p>
		<span style="font-size:140%"><asp:HyperLink ID="IDEXTENTIONS" Text='<%# GetUIString("MANAGEIDEXTENSIONSTEXT") %>' runat="server"/></span><br />
        <asp:Literal ID="DSEXTENSIONS" runat="server" Text='<%# GetUIString("MANAGEIDEXTENSIONSDESC") %>' />
	</p>
    <br />
    <br />
    <asp:LinkButton ID="LinkButtonRefresh" runat="server" OnClick="LinkButtonRefresh_Click"><%# GetUIString("MANAGEREFRESH") %></asp:LinkButton>
    <br />
    <br />
    <asp:LinkButton ID="LinkButtonClearCache" runat="server" OnClick="LinkButtonClearCache_Click"><%# GetUIString("MANAGECLEARCACHE") %></asp:LinkButton>
    <br />
    <br />
    <br />
    <asp:HyperLink ID="RETURNBACK" Text='<%# GetUIString("MANAGERETURN") %>' runat="server" />
    <SharePoint:FormDigest ID="FormDigest1" runat="server"/>
</asp:content>



