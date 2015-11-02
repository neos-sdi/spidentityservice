<%@ Assembly Name="SharePoint.IdentityService.Application, Version=15.0.0.0, Culture=neutral, PublicKeyToken=5f2cd3262c7b6db4" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SPSWC" Namespace="Microsoft.SharePoint.Portal.WebControls" Assembly="Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="~/_controltemplates/15/InputFormSection.ascx" %> 
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/15/InputFormControl.ascx" %> 
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="manageentities.aspx.cs" Inherits="SharePoint.IdentityService.AdminLayoutPages.manageentities" MasterPageFile="~/_admin/admin.master" %>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
SharePoint Identity Services
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
<%=GetFormattedTitle("MANAGETITLE") %>
</asp:Content>

<asp:content ID="PageDescription" contentplaceholderid="PlaceHolderPageDescription" runat="server">
<%=GetFormattedTitle("MANAGEENTITIESDESC") %>
</asp:content>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <SPSWC:PageLevelError runat="server" Id="pageLevelError" />
    <table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet" width="500px">
        <wssuc:InputFormSection runat="server" Title=<%# GetUIString("ENTFORMSECTIONTITLE") %> id="AttributesSection" >
            <Template_Description>
                <p>
                    <%# GetUIString("ENTFORMSECTIONDESC") %>
                </p>
            </Template_Description>

            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText=<%# GetUIString("ENTINPUTCONTROLTITLE") %> > 
                    <Template_Control>
                        <SharePoint:SPGridView ID="Grid" AllowPaging="True" PageSize="7" AutoGenerateColumns="false" runat="server" DataSourceID="ServiceDataSource" ShowFooter="true" OnRowCommand="Grid_RowCommand" OnRowDataBound="Grid_RowDataBound" FooterStyle-VerticalAlign ="Top" ShowHeaderWhenEmpty="True" DataKeyNames="DisplayName,DnsName" OnPageIndexChanging="Grid_PageIndexChanging" >
                            <Columns>
                                <asp:TemplateField HeaderText="Display Name" ControlStyle-Width="250px">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtDisplayName" runat="server" Text='<%# Bind("DisplayName") %>' ValidationGroup="B"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="ReqDisplayName" runat="server" ErrorMessage='<%# GetUIString("ENTVALIDATORDISPLAYMESSAGE") %>' ControlToValidate="txtDisplayName" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="B" ></asp:RequiredFieldValidator>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox ID="lblDisplayName" runat="server" Text='<%# Bind("DisplayName") %>' BorderStyle="None" Enabled="false" ValidationGroup="B"></asp:TextBox>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:TextBox ID="newDisplayName" runat="server" Width="250px" ValidationGroup="A"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="valDisplayName" runat="server" ErrorMessage='<%# GetUIString("ENTVALIDATORDISPLAYMESSAGE") %>' ControlToValidate="newDisplayName" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="A" ></asp:RequiredFieldValidator>
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Dns Name" ControlStyle-Width="250px">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtDnsName" runat="server" Text='<%# Bind("DnsName") %>' Wrap="true" ValidationGroup="B"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="ReqDnsName" runat="server" ErrorMessage='<%# GetUIString("ENTVALIDATORDNSMESSAGE") %>' ControlToValidate="txtDnsName" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="B"></asp:RequiredFieldValidator>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox  ID="lblDnsName" runat="server" Text='<%# Bind("DnsName") %>' Wrap="true" BorderStyle="None" Enabled="false" ValidationGroup="B"></asp:TextBox>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:TextBox ID="newDnsName" runat="server" Width="250px" ValidationGroup="A"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="valDnsName" runat="server" ErrorMessage='<%# GetUIString("ENTVALIDATORDNSMESSAGE") %>' ControlToValidate="newDnsName" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="A" ></asp:RequiredFieldValidator>
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Connection" ControlStyle-Width="150px" >
                                    <EditItemTemplate>
                                        <asp:DropDownList ID="txtConnection" runat="server" ValidationGroup="B" DataSourceID="LookupDataSource" DataTextField="ConnectionName" DataValueField="ConnectionName" SelectedValue='<%# Bind("Connection") %>'/>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox ID="lblConnection" runat="server" Text='<%# Bind("Connection") %>' Enabled="false" BorderStyle="None" ValidationGroup="B"/>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:DropDownList ID="newConnection" runat="server" ValidationGroup="A" Width="150px" DataSourceID="LookupDataSource" DataTextField="ConnectionName" DataValueField="ConnectionName" SelectedValue='<%# Bind("Connection") %>'/>
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Enabled" FooterStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                    <EditItemTemplate>
                                        <asp:CheckBox ID="txtEnabled" runat="server" Checked='<%# Bind("Enabled") %>' ValidationGroup="B"/>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="lblEnabled" runat="server" Checked='<%# Bind("Enabled") %>' Enabled="false" BorderStyle="None" ValidationGroup="B"/>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:CheckBox ID="newEnabled" runat="server" Checked="false" ValidationGroup="A"/>
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Position" ControlStyle-Width="30px" >
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtPosition" runat="server" Text='<%# Bind("DisplayPosition") %>' ValidationGroup="B" MaxLength="2" Width="30px"/>
                                        <asp:RangeValidator ID="reqPosition" runat="server" ErrorMessage='<%# GetUIString("ENTVALIDATORPOSMESSAGE") %>' MinimumValue="1" MaximumValue="99" ControlToValidate="txtPosition" ValidationGroup="B" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" Type="Integer" />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox ID="lblPosition" runat="server" Text='<%# Bind("DisplayPosition") %>' Enabled="false" BorderStyle="None" ValidationGroup="B" Width="30px"/>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:TextBox ID="newPosition" runat="server" Width="30px"  ValidationGroup="A" Text="99" MaxLength="2" />
                                        <asp:RangeValidator ID="valPosition" runat="server" ErrorMessage='<%# GetUIString("ENTVALIDATORPOSMESSAGE") %>' MinimumValue="1" MaximumValue="99" ValidationGroup="A" ControlToValidate="newPosition" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" />
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions" FooterStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <asp:ImageButton CommandName="Edit"   Text="Edit"   ID="btnEdit" Runat="server" ImageUrl="/_layouts/15/images/edit.gif" CausesValidation="false" ValidationGroup="B"/>&nbsp;
                                        <asp:ImageButton CommandName="Delete" Text="Delete" ID="btnDel"  Runat="server" ImageUrl="/_layouts/15/images/delitem.gif" CausesValidation="true" ValidationGroup="B"/>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:ImageButton CommandName="Update" Text="Update" ID="btnUpdate" Runat="server" ImageUrl="/_layouts/15/images/saveitem.gif" CausesValidation="true" ValidationGroup="B"/>&nbsp;
                                        <asp:ImageButton CommandName="Cancel" Text="Cancel" ID="btnCancel" Runat="server" ImageUrl="/_layouts/15/images/back.gif" CausesValidation="false" ValidationGroup="B"/>
                                    </EditItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:ImageButton CommandName="New" Text="Add"  ID="btnAdd" runat="server" ImageUrl="/_layouts/15/images/newitem.gif" CausesValidation="true" ValidationGroup="A"/>
                                    </FooterTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </SharePoint:SPGridView>
                    </Template_control>
		        </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>   
    </table>
    <asp:HyperLink ID="RETURNBACK" Text='<%# GetUIString("MANAGERETURN") %>' runat="server"/>
    <SharePoint:FormDigest ID="FormDigest1" runat="server"/>
    <asp:ObjectDataSource ID="ServiceDataSource" runat="server" />
    <asp:ObjectDataSource ID="LookupDataSource" runat="server" />
</asp:Content>

