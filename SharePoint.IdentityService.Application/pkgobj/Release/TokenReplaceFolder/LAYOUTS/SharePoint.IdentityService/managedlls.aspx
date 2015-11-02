<%@ Assembly Name="SharePoint.IdentityService.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ad9787278992c174" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SPSWC" Namespace="Microsoft.SharePoint.Portal.WebControls" Assembly="Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="~/_controltemplates/InputFormSection.ascx" %> 
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/InputFormControl.ascx" %> 
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
                <wssuc:InputFormSection runat="server" Title="Extension du magasin d'attributs" id="AttributesSection" >
                    <Template_Description>
                        <p>
                            Vous devez indiquer l'extension du magasin d'attributs associée à votre fournisseur de revendications. 
                        </p>
                    </Template_Description>

                    <Template_InputFormControls>
                        <wssuc:InputFormControl runat="server" LabelText="Magasins d'attributs" >
                            <Template_Control>
                                <SharePoint:SPGridView ID="Grid" AllowPaging="True" PageSize="7" AutoGenerateColumns="false" runat="server"  >
                                    <Columns>
                                        <asp:CommandField ButtonType="Image" ShowEditButton="true" EditImageUrl="/_layouts/images/edit.gif" UpdateImageUrl="/_layouts/images/saveitem.gif" DeleteImageUrl="/_layouts/images/delitem.gif" ShowDeleteButton="True"  ShowInsertButton="True" NewImageUrl="/_layouts/images/newitem.gif" />
                                        <asp:TemplateField HeaderText="Assembly Description" >
                                            <ItemTemplate>
                                                <asp:Label ID="_assemblyLabel" runat="server" Width="70%" Text='<%# Bind("AssemblyDesc") %>'  />
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="_assemblyText" runat="server" Width="70%" Text='<%# Bind("AssemblyDesc") %>' />
                                            </EditItemTemplate>
                                            <InsertItemTemplate>
                                                <asp:TextBox ID="_assemblyIns" runat="server" Width="70%" Text='<%# Bind("AssemblyDesc") %>' />
                                            </InsertItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Type Description">
                                            <ItemTemplate>
                                                <asp:Label ID="_typeLabel" runat="server" Width="30%" Text='<%# Bind("TypeDesc") %>' />
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="_typeText" runat="server" Width="30%" Text='<%# Bind("TypeDesc") %>' />
                                            </EditItemTemplate>
                                            <InsertItemTemplate>
                                                <asp:TextBox ID="_typeIns" runat="server" Width="30%" Text='<%# Bind("TypeDesc") %>' />
                                            </InsertItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Trace">
                                            <ItemTemplate>
                                                <asp:Checkbox ID="_traceLabel" runat="server" Checked='<%# Convert.ToBoolean(Eval("TraceDesc")) %>' Enabled="false"/>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <asp:Checkbox ID="_traceText" runat="server" Checked='<%# Convert.ToBoolean(Eval("TraceDesc")) %>' Enabled="true"/>
                                            </EditItemTemplate>
                                            <InsertItemTemplate>
                                                <asp:Checkbox ID="_traceIns" runat="server" Checked='<%# Convert.ToBoolean(Eval("TraceDesc")) %>' Enabled="true"/>
                                            </InsertItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Selected">
                                            <ItemTemplate>
                                                <asp:Checkbox ID="_selectedLabel" runat="server" Checked='<%# Convert.ToBoolean(Eval("SelectedDesc")) %>' Enabled="false"/>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <asp:Checkbox ID="_selectedText" runat="server" Checked='<%# Convert.ToBoolean(Eval("SelectedDesc")) %>' Enabled="true"/>
                                            </EditItemTemplate>
                                            <InsertItemTemplate>
                                                <asp:Checkbox ID="_selectedIns" runat="server" Checked='<%# Convert.ToBoolean(Eval("SelectedDesc")) %>' Enabled="true"/>
                                            </InsertItemTemplate>

                                        </asp:TemplateField>        
                                    </Columns>
                                </SharePoint:SPGridView>
			                </Template_control>
		                </wssuc:InputFormControl>
                    </Template_InputFormControls>
                </wssuc:InputFormSection>   
    </table>
    <SharePoint:FormDigest ID="FormDigest1" runat="server"/>
</asp:Content>
