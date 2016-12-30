<%@ Assembly Name="SharePoint.IdentityService.Application, Version=16.0.0.0, Culture=neutral, PublicKeyToken=$SharePoint.Project.AssemblyPublicKeyToken$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SPSWC" Namespace="Microsoft.SharePoint.Portal.WebControls" Assembly="Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="~/_controltemplates/15/InputFormSection.ascx" %> 
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/15/InputFormControl.ascx" %> 
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="manageparams.aspx.cs" Inherits="SharePoint.IdentityService.AdminLayoutPages.manageparams" MasterPageFile="~/_admin/admin.master" %>

<asp:Content ID="PlaceHolderPageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
SharePoint Identity Services
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
<%=GetFormattedTitle("MANAGETITLE") %>
</asp:Content>

<asp:content ID="PageDescription" contentplaceholderid="PlaceHolderPageDescription" runat="server">
<%=GetFormattedTitle("MANAGEPARAMETERSDESC") %>
</asp:content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <SPSWC:PageLevelError runat="server" Id="pageLevelError" />
    <table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet" width="500px">
        <wssuc:InputFormSection runat="server" Title=<%# GetUIString("PRMFORMSECTIONTITLE") %> id="AttributesSection" >
            <Template_Description>
                <p>
                    <%# GetUIString("PRMFORMSECTIONDESC") %>
                </p>
            </Template_Description>

            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText=<%# GetUIString("PRMINPUTCONTROLTITLE") %> > 
                    <Template_Control>
                        <asp:FormView ID="Grid" AllowPaging="False" runat="server" DataSourceID="ServiceDataSource" OnModeChanging="Grid_ModeChanging" OnDataBound="Grid_DataBound">
                                    <EditItemTemplate>
                                        <table>
                                            <tr>
                                                <td colspan="4" style="height: 40px" valign="bottom">
                                                    <asp:Label ID="Label1" runat="server" Text='<%# GetUIString("PRMCLAIMSTITLE") %>' Font-Size="Larger" Font-Bold="True"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label2" runat="server" Text='<%# GetUIString("PRMCLAIMSSHAREPOINT") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtClaimsDisplayMode" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("ClaimsDisplayMode") %>' DataSourceID="DropSourceClaimsDisplayMode" DataValueField="Value" DataTextField="Text" /> 
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label3" runat="server" Text='<%# GetUIString("PRMCLAIMSPEOPLEPICKER") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtPeoplePickerDisplayMode" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("PeoplePickerDisplayMode") %>' DataSourceID="DropSourceClaimsDisplayMode" DataValueField="Value" DataTextField="Text"/> 
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label4" runat="server" Text='<%# GetUIString("PRMCLAIMSIDENTITYVALUE") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:Textbox ID="txtClaimIdentityValue" runat="server" ValidationGroup="B" Width="500px" Text='<%# Bind("ClaimIdentity") %>' /> 
                                                </td>
                                                <td style="width: 10px">
					                                <asp:RequiredFieldValidator ID="ReqClaimIdentityValue" runat="server" ErrorMessage='*' ControlToValidate="txtClaimIdentityValue" ForeColor="Red" ValidationGroup="B"/>
						                        </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2" >
                                                    <asp:Label ID="Label5" runat="server" Text='<%# GetUIString("PRMCLAIMSIDENTITY") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtClaimIdentityMode" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("ClaimIdentityMode") %>' DataSourceID="DropSourceClaimIdentityMode" DataTextField="Text" DataValueField="Value" /> 
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label6" runat="server" Text='<%# GetUIString("PRMCLAIMSIDENTITYROLESVALUE") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:Textbox ID="txtClaimRoleValue" runat="server" ValidationGroup="B" Width="500px" Text='<%# Bind("ClaimRole") %>' /> 
                                                </td>
						                        <td style="width: 10px">
						                            <asp:RequiredFieldValidator ID="ReqClaimRoleValue" runat="server" ErrorMessage='*' ControlToValidate="txtClaimRoleValue" ForeColor="Red" ValidationGroup="B" />
						                        </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label7" runat="server" Text='<%# GetUIString("PRMCLAIMSIDENTITYROLES") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtClaimRoleMode" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("ClaimRoleMode") %>' DataSourceID="DropSourceClaimRoleMode" DataTextField="Text" DataValueField="Value" /> 
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 15px" ></td>
                                                <td style="width: 350px" >
                                                    <asp:CheckBox ID="CheckBoxUserkey" runat="server" Text='<%# GetUIString("PRMSUPPORTSUSERKEY") %>'  ValidationGroup="B" Width="350px" Checked='<%# Bind("SupportsUserKey") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                </td>
                                                <td>
                                                </td>
                                            </tr>                                            
                                            <tr>
                                                <td colspan="4" style="height: 40px" valign="bottom">
                                                    <asp:Label ID="Label8" runat="server" Text='<%# GetUIString("PRMREPOSITORYDESC") %>' Font-Bold="True" Font-Size="Larger"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label9" runat="server" Text='<%# GetUIString("PRMCACHEDURATION") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:Textbox ID="txtTimeout" runat="server" ValidationGroup="B" Width="50px" Text='<%# Bind("CacheDuration") %>' /> 
                                                    <asp:RequiredFieldValidator ID="RequiredValidatorTimeOut" runat="server" ControlToValidate="txtTimeout" ErrorMessage='<%# GetUIString("PRMERRORTIMEOUT") %>' BorderStyle="None" ForeColor="Red" ValidationGroup="B" />
                                                    <asp:RangeValidator ID="RangeValidatorTimeOut" runat="server" ErrorMessage='<%# GetUIString("PRMERRORTIMEOUT") %>' MaximumValue="1440" MinimumValue="0" BorderStyle="None" ForeColor="Red" ValidationGroup="B" ControlToValidate="txtTimeout" Type="Integer"></asp:RangeValidator>
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label10" runat="server" Text='<%# GetUIString("PRMQUERIESMODE") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtSmoothRequestor" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("SmoothRequestor") %>' DataSourceID="DropSourceSmoothRequestor" DataTextField="Text" DataValueField="Value" /> 
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 15px" ></td>
                                                <td style="width: 350px" >
                                                    <asp:CheckBox ID="chkPeoplePickerImages" runat="server" Text='<%# GetUIString("PRMSHOWPEOPLEPICKERIMG") %>'  ValidationGroup="B" Width="350px" Checked='<%# Bind("PeoplePickerImages") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                    
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 15px" ></td>
                                                <td style="width: 350px" >
                                                    <asp:CheckBox ID="chkShowSystemNodes" runat="server" Text='<%# GetUIString("PRMSHOWSYSTEMACCOUNTS") %>' ValidationGroup="B" Width="350px" Checked='<%# Bind("ShowSystemNodes") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">

                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 15px" ></td>
                                                <td style="width: 350px" >
                                                    <asp:CheckBox ID="chkSearchByDisplayName" runat="server" Text='<%# GetUIString("PRMSEARCHDISPLAYNAME") %>' ValidationGroup="B" Width="375px" Checked='<%# Bind("SearchByDisplayName") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                    
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 15px" ></td>
                                                <td style="width: 350px" >
                                                    <asp:CheckBox ID="chkSearchByMail" runat="server" Text='<%# GetUIString("PRMSEARCHEMAILS") %>' ValidationGroup="B" Width="350px" Checked='<%# Bind("SearchByMail") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">

                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td colspan="4" style="height: 40px" valign="bottom">
                                                    <asp:Label ID="Label11" runat="server" Text='<%# GetUIString("PRMCLAIMPROVIDERTITLE") %>' Font-Bold="True" Font-Size="Larger" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label12" runat="server" Text='<%# GetUIString("PRMCLAIMPROVIDERNAME") %>' ></asp:Label>
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:TextBox  ID="txtClaimProviderName" runat="server" Text='<%# Bind("ClaimProviderName") %>' ValidationGroup="B" Width="350px" ReadOnly="True" BorderStyle="None"/>
                                                    <asp:RequiredFieldValidator ID="RequiredClaimProviderName" runat="server" ErrorMessage='*' Enabled="false" ControlToValidate="txtClaimProviderName" ForeColor="Red" ValidationGroup="B"></asp:RequiredFieldValidator>
                                                </td>
						                        <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label13" runat="server" Text='<%# GetUIString("PRMCLAIMPROVIDERDESC") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:TextBox  ID="txtClaimDisplayName" runat="server" Text='<%# Bind("ClaimDisplayName") %>' ValidationGroup="B" Width="350px" />
						                            <asp:RequiredFieldValidator ID="RequiredClaimsDisplayname" runat="server" ErrorMessage='*' ControlToValidate="txtClaimDisplayName" ForeColor="Red" ValidationGroup="B" />
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label14" runat="server" Text='<%# GetUIString("PRMCLAIMPROVIDERTYPE") %>' ></asp:Label>
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtClaimsMode" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("ClaimsMode") %>' Enabled="false" ReadOnly="True" DataSourceID="DropSourceClaimsMode" DataTextField="Text" DataValueField="Value" /> 
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="4" style="height: 40px" valign="bottom">
                                                    <asp:Label ID="Label15" runat="server" Text='<%# GetUIString("PRMIDENTITYPROVIDERTITLE") %>' Font-Bold="True" Font-Size="Larger" ></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label16" runat="server" Text='<%# GetUIString("PRMIDENTITYPROVIDERDESC") %>' ></asp:Label>
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:TextBox  ID="txtTrustedLoginProviderName" runat="server" Text='<%# Bind("TrustedLoginProviderName") %>' ValidationGroup="B" Width="350px" ReadOnly="True" BorderStyle="None"/>
                                                </td>
                                                <td></td>
                                            </tr>
                                        </table>
                                        <br />
                                        <asp:ImageButton CommandName="Update" Text="Update" ID="btnUpdate" Runat="server" ImageUrl="/_layouts/15/images/saveitem.gif" CausesValidation="true" ValidationGroup="B"/>&nbsp;
                                        <asp:ImageButton CommandName="Cancel" Text="Cancel" ID="btnCancel" Runat="server" ImageUrl="/_layouts/15/images/back.gif" CausesValidation="false" ValidationGroup="B"/>
                                    </EditItemTemplate>

                                    <ItemTemplate>
                                        <table>
                                            <tr>
                                                <td colspan="4" style="height: 40px" valign="bottom">
                                                    <asp:Label ID="Label17" runat="server" Text='<%# GetUIString("PRMCLAIMSTITLE") %>' Font-Size="Larger" Font-Bold="True"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label18" runat="server" Text='<%# GetUIString("PRMCLAIMSSHAREPOINT") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtClaimsDisplayMode" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("ClaimsDisplayMode") %>' Enabled="False" DataSourceID="DropSourceClaimsDisplayMode" DataValueField="Value" DataTextField="Text" /> 
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label19" runat="server" Text='<%# GetUIString("PRMCLAIMSPEOPLEPICKER") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtPeoplePickerDisplayMode" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("PeoplePickerDisplayMode") %>' Enabled="False" DataSourceID="DropSourceClaimsDisplayMode" DataValueField="Value" DataTextField="Text" /> 
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label20" runat="server" Text='<%# GetUIString("PRMCLAIMSIDENTITYVALUE") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:Textbox ID="txtClaimIdentityValue" runat="server" ValidationGroup="B" Width="500px" Text='<%# Bind("ClaimIdentity") %>' ReadOnly="True" BorderStyle="None" /> 
                                                </td>
						                        <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label21" runat="server" Text='<%# GetUIString("PRMCLAIMSIDENTITY") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtClaimIdentityMode" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("ClaimIdentityMode") %>' Enabled="False" DataSourceID="DropSourceClaimIdentityMode" DataTextField="Text" DataValueField="Value" /> 
                                                </td>
                                            </tr>
                                            <tr id="idrolesclaimblock">
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label22" runat="server" Text='<%# GetUIString("PRMCLAIMSIDENTITYROLESVALUE") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:Textbox ID="txtClaimRoleValue" runat="server" ValidationGroup="B" Width="500px" Text='<%# Bind("ClaimRole") %>' ReadOnly="True" BorderStyle="None" /> 
                                                </td>
						                        <td></td>
                                            </tr>
                                            <tr id="idrolesclaimvalue">
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label23" runat="server" Text='<%# GetUIString("PRMCLAIMSIDENTITYROLES") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtClaimRoleMode" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("ClaimRoleMode") %>' Enabled="false" DataSourceID="DropSourceClaimRoleMode" DataTextField="Text" DataValueField="Value" /> 
                                                </td>
                                            </tr>
                                            <tr id="iduserkeycb">
                                                <td style="width: 15px" ></td>
                                                <td style="width: 350px" >
                                                    <asp:CheckBox ID="CheckBoxUserkey" runat="server" Text='<%# GetUIString("PRMSUPPORTSUSERKEY") %>'  ValidationGroup="B" Width="350px" Checked='<%# Bind("SupportsUserKey") %>' Enabled="False"/>
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                </td>
                                                <td>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="4" style="height: 40px" valign="bottom">
                                                    <asp:Label ID="Label24" runat="server" Text='<%# GetUIString("PRMREPOSITORYDESC") %>' Font-Size="Larger" Font-Bold="True"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label25" runat="server" Text='<%# GetUIString("PRMCACHEDURATION") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:Textbox ID="txtTimeout" runat="server" ValidationGroup="B" Width="50px" Text='<%# Bind("CacheDuration") %>' ReadOnly="True" BorderStyle="None"/> 
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label26" runat="server" Text='<%# GetUIString("PRMQUERIESMODE") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtSmoothRequestor" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("SmoothRequestor") %>' Enabled="False" DataSourceID="DropSourceSmoothRequestor" DataTextField="Text" DataValueField="Value" /> 
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 15px" ></td>
                                                <td style="width: 350px" >
                                                    <asp:CheckBox ID="chkPeoplePickerImages" runat="server" Text='<%# GetUIString("PRMSHOWPEOPLEPICKERIMG") %>' ValidationGroup="B" Width="350px" Checked='<%# Bind("PeoplePickerImages") %>' Enabled="False" />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                    
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 15px" ></td>
                                                <td style="width: 350px" >
                                                    <asp:CheckBox ID="chkShowSystemNodes" runat="server" Text='<%# GetUIString("PRMSHOWSYSTEMACCOUNTS") %>' ValidationGroup="B" Width="350px" Checked='<%# Bind("ShowSystemNodes") %>' Enabled="False" />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">

                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 15px" ></td>
                                                <td style="width: 350px" >
                                                    <asp:CheckBox ID="chkSearchByDisplayName" runat="server" Text='<%# GetUIString("PRMSEARCHDISPLAYNAME") %>' ValidationGroup="B" Width="375px" Checked='<%# Bind("SearchByDisplayName") %>' Enabled="False" />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                    
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 15px" ></td>
                                                <td style="width: 350px" >
                                                    <asp:CheckBox ID="chkSearchByMail" runat="server" Text='<%# GetUIString("PRMSEARCHEMAILS") %>' ValidationGroup="B" Width="350px" Checked='<%# Bind("SearchByMail") %>' Enabled="False" />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">

                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td colspan="4" style="height: 40px" valign="bottom">
                                                    <asp:Label ID="Label27" runat="server" Text='<%# GetUIString("PRMCLAIMPROVIDERTITLE") %>' Font-Size="Larger" Font-Bold="True"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label28" runat="server" Text='<%# GetUIString("PRMCLAIMPROVIDERNAME") %>' ></asp:Label>
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:TextBox  ID="txtClaimProviderName" runat="server" Text='<%# Bind("ClaimProviderName") %>' ValidationGroup="B" Width="350px" ReadOnly="True" BorderStyle="None"/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label29" runat="server" Text='<%# GetUIString("PRMCLAIMPROVIDERDESC") %>' />
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:TextBox  ID="txtClaimDisplayName" runat="server" Text='<%# Bind("ClaimDisplayName") %>' ValidationGroup="B" Width="350px" ReadOnly="True" BorderStyle="None"/>
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label30" runat="server" Text='<%# GetUIString("PRMCLAIMPROVIDERTYPE") %>' ></asp:Label>
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:DropDownList ID="txtClaimsMode" runat="server" ValidationGroup="B" Width="350px" SelectedValue='<%# Bind("ClaimsMode") %>' Enabled="False" ReadOnly="True" BorderStyle="None" DataSourceID="DropSourceClaimsMode" DataTextField="Text" DataValueField="Value" /> 
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="4" style="height: 40px" valign="bottom">
                                                    <asp:Label ID="Label31" runat="server" Text='<%# GetUIString("PRMIDENTITYPROVIDERTITLE") %>' Font-Size="Larger" Font-Bold="True"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="width: 365px" colspan="2">
                                                    <asp:Label ID="Label32" runat="server" Text='<%# GetUIString("PRMIDENTITYPROVIDERDESC") %>' ></asp:Label>
                                                </td>
                                                <td style="width: 10px"></td>
                                                <td style="width: 350px">
                                                    <asp:TextBox  ID="txtTrustedLoginProviderName" runat="server" Text='<%# Bind("TrustedLoginProviderName") %>' ValidationGroup="B" Width="350px" ReadOnly="True" BorderStyle="None" />
                                                </td>
                                                <td></td>
                                            </tr>
                                        </table>
                                        <br />
                                        <asp:ImageButton CommandName="Edit"   Text="Edit" ID="btnEdit" Runat="server" ImageUrl="/_layouts/15/images/edit.gif" CausesValidation="false" ValidationGroup="B"/>&nbsp;
                                    </ItemTemplate>
                        </asp:FormView>
                    </Template_control>
		        </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>   
    </table>
    <asp:HyperLink ID="RETURNBACK" Text='<%# GetUIString("MANAGERETURN") %>' runat="server"/>
    <SharePoint:FormDigest ID="FormDigest1" runat="server"/>
    <asp:ObjectDataSource ID="ServiceDataSource" runat="server" />
    <asp:ObjectDataSource ID="DropSourceClaimsDisplayMode" runat="server" />
    <asp:ObjectDataSource ID="DropSourceClaimIdentityMode" runat="server" />
    <asp:ObjectDataSource ID="DropSourceClaimRoleMode" runat="server" />
    <asp:ObjectDataSource ID="DropSourceSmoothRequestor" runat="server" />
    <asp:ObjectDataSource ID="DropSourceClaimsMode" runat="server" />
</asp:Content>




