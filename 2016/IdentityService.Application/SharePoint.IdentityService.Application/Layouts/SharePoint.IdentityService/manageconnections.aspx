<%@ Assembly Name="SharePoint.IdentityService.Application, Version=16.0.0.0, Culture=neutral, PublicKeyToken=$SharePoint.Project.AssemblyPublicKeyToken$" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SPSWC" Namespace="Microsoft.SharePoint.Portal.WebControls" Assembly="Microsoft.SharePoint.Portal, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="~/_controltemplates/15/InputFormSection.ascx" %> 
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/15/InputFormControl.ascx" %> 
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="manageconnections.aspx.cs" Inherits="SharePoint.IdentityService.AdminLayoutPages.manageconnections" MasterPageFile="~/_admin/admin.master" %>

<asp:Content ID="PlaceHolderPageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
SharePoint Identity Services
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
<%=GetFormattedTitle("MANAGETITLE") %>
</asp:Content>

<asp:content ID="PageDescription" contentplaceholderid="PlaceHolderPageDescription" runat="server">
<%=GetFormattedTitle("MANAGECONNECTIONSDESC") %>   
</asp:content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

<SPSWC:PageLevelError runat="server" Id="pageLevelError" />
    <table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet" width="500px">
        <wssuc:InputFormSection runat="server" Title=<%# GetUIString("CNXTFORMSECTIONTITLE") %> id="AttributesSection" >
            <Template_Description>
                <p>
                    <%# GetUIString("CNXFORMSECTIONDESC") %>
                </p>
            </Template_Description>

            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText=<%# GetUIString("CNXINPUTCONTROLTITLE") %> > 
                    <Template_Control>
                        <SharePoint:SPGridView ID="Grid" AllowPaging="True" PageSize="7" AutoGenerateColumns="false" runat="server" DataSourceID="ServiceDataSource" ShowFooter="true" OnRowCommand="Grid_RowCommand" OnRowDataBound="Grid_RowDataBound" OnPageIndexChanging="Grid_PageIndexChanging" FooterStyle-VerticalAlign="Top" DataKeyNames="ConnectionName">
                            <Columns>
                                <asp:TemplateField HeaderText="Connection Name" ControlStyle-Width="150px">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtConnectionName" runat="server" Text='<%# Bind("ConnectionName") %>' ValidationGroup="B"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="ReqConnectionName" runat="server" ErrorMessage='<%# GetUIString("CNXVALIDATORNAMEMESSAGE") %>' ControlToValidate="txtConnectionName" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="B" ></asp:RequiredFieldValidator>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox ID="lblConnectionName" runat="server" Text='<%# Bind("ConnectionName") %>' BorderStyle="None" Enabled="false" ValidationGroup="B"></asp:TextBox>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:TextBox ID="newConnectionName" runat="server" Width="150px" ValidationGroup="A"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="valConnectionName" runat="server" ErrorMessage='<%# GetUIString("CNXVALIDATORNAMEMESSAGE") %>' ControlToValidate="newConnectionName" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="A" ></asp:RequiredFieldValidator>
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="UserName" ControlStyle-Width="150px">
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtUserName" runat="server" Text='<%# Bind("UserName") %>' Wrap="true" ValidationGroup="B"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="ReqUserName" runat="server" ErrorMessage='<%# GetUIString("CNXVALIDATORUSERMESSAGE") %>' ControlToValidate="txtUserName" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="B"></asp:RequiredFieldValidator>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox  ID="lblUserName" runat="server" Text='<%# Bind("UserName") %>' Wrap="true" BorderStyle="None" Enabled="false" ValidationGroup="B"></asp:TextBox>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:TextBox ID="newUserName" runat="server" Width="150px" ValidationGroup="A"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="valUserName" runat="server" ErrorMessage='<%# GetUIString("CNXVALIDATORUSERMESSAGE") %>' ControlToValidate="newUserName" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="A" ></asp:RequiredFieldValidator>
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Password" ControlStyle-Width="150px" >
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtPassword" runat="server" Text='<%# Bind("Password") %>' ValidationGroup="B" />
                                        <asp:RequiredFieldValidator ID="reqPassword" runat="server" ErrorMessage='<%# GetUIString("CNXVALIDATORPWDMESSAGE") %>' ControlToValidate="txtPassword" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="B" ></asp:RequiredFieldValidator>
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox ID="lblPassword" runat="server" Text='<%# Bind("Password") %>' Enabled="false" BorderStyle="None" ValidationGroup="B" />
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:TextBox ID="newPassword" runat="server" Width="150px" ValidationGroup="A" TextMode="Password" />
                                        <asp:RequiredFieldValidator ID="valPassword" runat="server" ErrorMessage='<%# GetUIString("CNXVALIDATORPWDMESSAGE") %>' ControlToValidate="newPassword" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="A" ></asp:RequiredFieldValidator>
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Timeout" ControlStyle-Width="30px" >
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtTimeout" runat="server" Text='<%# Bind("Timeout") %>' ValidationGroup="B" MaxLength="2" Width="30px"/>
                                        <asp:RangeValidator ID="reqTimeout" runat="server" ErrorMessage='<%# GetUIString("CNXVALIDATORTIMEOUTPOSMESSAGE") %>' MinimumValue="1" MaximumValue="90" ControlToValidate="txtTimeout" ValidationGroup="B" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" Type="Integer" />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox ID="lblTimeout" runat="server" Text='<%# Bind("Timeout") %>' Enabled="false" BorderStyle="None" ValidationGroup="B" Width="30px"/>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:TextBox ID="newTimeout" runat="server" Width="30px"  ValidationGroup="A" Text="30" MaxLength="2" />
                                        <asp:RangeValidator ID="valTimeout" runat="server" ErrorMessage='<%# GetUIString("CNXVALIDATORTIMEOUTMESSAGE") %>' MinimumValue="1" MaximumValue="90" ValidationGroup="A" ControlToValidate="newTimeout" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" Type="Integer" />
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Secure" FooterStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                    <EditItemTemplate>
                                        <asp:CheckBox ID="txtSecure" runat="server" Checked='<%# Bind("Secure") %>' ValidationGroup="B" />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="lblSecure" runat="server" Checked='<%# Bind("Secure") %>' Enabled="false" BorderStyle="None" ValidationGroup="B"/>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:CheckBox ID="newSecure" runat="server" Checked="false" ValidationGroup="A"/>
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="MaxRows" ControlStyle-Width="30px" >
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtMaxRows" runat="server" Text='<%# Bind("MaxRows") %>' ValidationGroup="B" MaxLength="4" Width="30px"/>
                                        <asp:RangeValidator ID="reqMaxRows" runat="server" ErrorMessage='<%# GetUIString("CNXVALIDATORROWSMESSAGE") %>' MinimumValue="1" MaximumValue="9999" ControlToValidate="txtMaxRows" ValidationGroup="B" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" Type="Integer" />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox ID="lblMaxRows" runat="server" Text='<%# Bind("MaxRows") %>' Enabled="false" BorderStyle="None" ValidationGroup="B" Width="30px"/>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:TextBox ID="newMaxRows" runat="server" Width="30px"  ValidationGroup="A" Text="720" MaxLength="4" />
                                        <asp:RangeValidator ID="valMaxRows" runat="server" ErrorMessage='<%# GetUIString("CNXVALIDATORROWSMESSAGE") %>' MinimumValue="1" MaximumValue="9999" ValidationGroup="A" ControlToValidate="newMaxRows" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" Type="Integer" />
                                    </FooterTemplate>
                                </asp:TemplateField>

                                <asp:TemplateField HeaderText="ConnectString" ControlStyle-Width="200px" >
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtConnectString" runat="server" Text='<%# Bind("ConnectString") %>' ValidationGroup="B" Width="200px" />
                                    </EditItemTemplate>
                                    <ItemTemplate>
                                        <asp:TextBox ID="lblConnectString" runat="server" Text='<%# Bind("ConnectString") %>' Enabled="false" BorderStyle="None" ValidationGroup="B" Width="200px"/>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:TextBox ID="newConnectString" runat="server" Width="200px"  ValidationGroup="A" />
                                    </FooterTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions" FooterStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <asp:ImageButton CommandName="Edit"   ID="btnEdit" Runat="server" ImageUrl="/_layouts/15/images/edit.gif" CausesValidation="false" ValidationGroup="B"/>&nbsp;
                                        <asp:ImageButton CommandName="Delete" ID="btnDel"  Runat="server" ImageUrl="/_layouts/15/images/delitem.gif" CausesValidation="true" ValidationGroup="B"/>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:ImageButton CommandName="Update" ID="btnUpdate" Runat="server" ImageUrl="/_layouts/15/images/saveitem.gif" CausesValidation="true" ValidationGroup="B"/>&nbsp;
                                        <asp:ImageButton CommandName="Cancel" ID="btnCancel" Runat="server" ImageUrl="/_layouts/15/images/back.gif" CausesValidation="false" ValidationGroup="B"/>
                                    </EditItemTemplate>
                                    <FooterTemplate>
                                        <br />
                                        <asp:ImageButton CommandName="New" ID="btnAdd" runat="server" ImageUrl="/_layouts/15/images/newitem.gif" CausesValidation="true" ValidationGroup="A"/>
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
</asp:Content>

