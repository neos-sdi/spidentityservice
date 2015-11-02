<%@ Assembly Name="SharePoint.IdentityService.Application, Version=15.0.0.0, Culture=neutral, PublicKeyToken=5f2cd3262c7b6db4" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SPSWC" Namespace="Microsoft.SharePoint.Portal.WebControls" Assembly="Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="~/_controltemplates/15/InputFormSection.ascx" %> 
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/15/InputFormControl.ascx" %> 
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="managedlls.aspx.cs" Inherits="SharePoint.IdentityService.AdminLayoutPages.managedlls" MasterPageFile="~/_admin/admin.master" %>

<asp:Content ID="PlaceHolderPageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
SharePoint Identity Services
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
<%=GetFormattedTitle("MANAGETITLE") %>
</asp:Content>

<asp:content ID="PageDescription" contentplaceholderid="PlaceHolderPageDescription" runat="server">
<%=GetFormattedTitle("MANAGEDLLDESC") %>
</asp:content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <SPSWC:PageLevelError runat="server" Id="pageLevelError" />
    <table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet" width="500px">
                <wssuc:InputFormSection runat="server" Title=<%# GetUIString("DLLFORMSECTIONTITLE") %> id="AttributesSection" >
                    <Template_Description>
                        <p>
                            <%# GetUIString("DLLFORMSECTIONDESC") %>
                        </p>
                    </Template_Description>

                    <Template_InputFormControls>
                        <wssuc:InputFormControl runat="server" LabelText=<%# GetUIString("DLLINPUTCONTROLTITLE") %> > 
                            <Template_Control>
                                <SharePoint:SPGridView ID="Grid" AllowPaging="True" PageSize="7" AutoGenerateColumns="false" runat="server" DataSourceID="ServiceDataSource" ShowFooter="true" OnRowCommand="Grid_RowCommand" OnRowDataBound="Grid_RowDataBound" OnRowUpdating="Grid_RowUpdating" OnPageIndexChanging="Grid_PageIndexChanging" FooterStyle-VerticalAlign="Top" DataKeyNames="AssemblyFulldescription, AssemblyTypeDescription">
                                    <Columns>
                                        <asp:TemplateField HeaderText="Assembly" ControlStyle-Width="400px">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="txtAssemblyFulldescription" runat="server" Text='<%# Bind("AssemblyFulldescription") %>' ValidationGroup="B"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="ReqAssemblyFulldescription" runat="server" ErrorMessage='<%# GetUIString("DLLVALIDATORASSEMBLYMESSAGE") %>' ControlToValidate="txtAssemblyFulldescription" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="B" ></asp:RequiredFieldValidator>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="lblAssemblyFulldescription" runat="server" Text='<%# Bind("AssemblyFulldescription") %>' BorderStyle="None" Enabled="false" ValidationGroup="B"></asp:TextBox>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <br />
                                                <asp:TextBox ID="newAssemblyFulldescription" runat="server" Width="400px" ValidationGroup="A"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="valAssemblyFulldescription" runat="server" ErrorMessage='<%# GetUIString("DLLVALIDATORASSEMBLYMESSAGE") %>' ControlToValidate="newAssemblyFulldescription" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="A" ></asp:RequiredFieldValidator>
                                            </FooterTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Type" ControlStyle-Width="300px">
                                            <EditItemTemplate>
                                                <asp:TextBox ID="txtAssemblyTypeDescription" runat="server" Text='<%# Bind("AssemblyTypeDescription") %>' Wrap="true" ValidationGroup="B"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="ReqAssemblyTypeDescription" runat="server" ErrorMessage='<%# GetUIString("DLLVALIDATORTYPEMESSAGE") %>' ControlToValidate="txtAssemblyTypeDescription" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="B"></asp:RequiredFieldValidator>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox  ID="lblAssemblyTypeDescription" runat="server" Text='<%# Bind("AssemblyTypeDescription") %>' Wrap="true" BorderStyle="None" Enabled="false" ValidationGroup="B"></asp:TextBox>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <br />
                                                <asp:TextBox ID="newAssemblyTypeDescription" runat="server" Width="300px" ValidationGroup="A"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="valAssemblyTypeDescription" runat="server" ErrorMessage='<%# GetUIString("DLLVALIDATORTYPEMESSAGE") %>' ControlToValidate="newAssemblyTypeDescription" SetFocusOnError="True" Display="Dynamic" ForeColor="Red" ValidationGroup="A" ></asp:RequiredFieldValidator>
                                            </FooterTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Selected" FooterStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                            <EditItemTemplate>
                                                <asp:CheckBox ID="cbaSelected" runat="server" Checked='<%# Bind("Selected") %>' ValidationGroup="B"/>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="cbbSelected" runat="server" Checked='<%# Bind("Selected") %>' Enabled="false" ValidationGroup="B"/>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <br />
                                                <asp:CheckBox ID="cbnSelected" runat="server" Checked="false" ValidationGroup="A"/>
                                            </FooterTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Trace" FooterStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                            <EditItemTemplate>
                                                <asp:CheckBox ID="cbaTrace" runat="server" Checked='<%# Bind("TraceResolve") %>' ValidationGroup="B"/>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="cbbTrace" runat="server" Checked='<%# Bind("TraceResolve") %>' Enabled="false" ValidationGroup="B"/>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <br />
                                                <asp:CheckBox ID="cbnTrace" runat="server" Checked="false" ValidationGroup="A"/>
                                            </FooterTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Claims +" FooterStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                            <EditItemTemplate>
                                                <asp:CheckBox ID="cbaClaims" runat="server" Checked='<%# Bind("ClaimsExt") %>' ValidationGroup="B"/>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="cbbClaims" runat="server" Checked='<%# Bind("ClaimsExt") %>' Enabled="false" ValidationGroup="B"/>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <br />
                                                <asp:CheckBox ID="cbnClaims" runat="server" Checked="false" ValidationGroup="A"/>
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
</asp:Content>
