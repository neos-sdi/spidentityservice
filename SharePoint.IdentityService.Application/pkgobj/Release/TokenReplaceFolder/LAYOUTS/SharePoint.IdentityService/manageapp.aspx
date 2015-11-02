<%@ Assembly Name="SharePoint.IdentityService.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ad9787278992c174" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

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
                <asp:Label ID="LabelMessage" Runat="server" EnableViewState="False" class="ms-descriptionText"/> 
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
		<span style="font-size:140%"><asp:HyperLink ID="IDPARAMS" Text="MANAGEIDPARAMSTEXT"  runat="server" /></span><br />
        <asp:Literal ID="DSPARAMS" runat="server" Text="MANAGEIDPARAMSDESC" />
	</p>
	<p>
		<span style="font-size:140%"><asp:HyperLink ID="IDENTITIES" Text="MANAGEIDENTITIESTEXT" runat="server"/></span><br />
        <asp:Literal ID="DSIDENTITIES" runat="server" Text="MANAGEIDENTITIESDESC" />
	</p>
	<p>
		<span style="font-size:140%"><asp:HyperLink ID="IDCONNECTIONS" Text="MANAGEIDCONNECTIONSTEXT" runat="server"/></span><br />
        <asp:Literal ID="DSCONNECTIONS" runat="server" Text="MANAGEIDCONNECTIONSDESC" />
	</p>
	<p>
		<span style="font-size:140%"><asp:HyperLink ID="IDEXTENTIONS" Text="MANAGEIDEXTENSIONSTEXT" runat="server"/></span><br />
        <asp:Literal ID="DSEXTENSIONS" runat="server" Text="MANAGEIDEXTENSIONSDESC" />
	</p>
    <SharePoint:FormDigest ID="FormDigest1" runat="server"/>
</asp:content>



