<%@ Assembly Name="SharePoint.IdentityService.Application, Version=15.0.0.0, Culture=neutral, PublicKeyToken=5f2cd3262c7b6db4" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SPSWC" Namespace="Microsoft.SharePoint.Portal.WebControls" Assembly="Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="/_controltemplates/15/InputFormSection.ascx" %> 
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="/_controltemplates/15/InputFormControl.ascx" %> 
<%@ Register TagPrefix="wssuc" TagName="ContentDatabaseSection" src="~/_admin/ContentDatabaseSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="IisWebServiceApplicationPoolSection" src="~/_admin/IisWebServiceApplicationPoolSection.ascx" %>
<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="serviceapp.aspx.cs" Inherits="SharePoint.IdentityService.AdminPages.ServiceAppPage" MasterPageFile="~/_layouts/15/dialog.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderDialogHeaderPageTitle" runat="server">
    <asp:Literal ID="litServiceApplicationTitle" Text="SharePoint Identity Service Application" runat="server" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderDialogDescription" runat="server">
    <asp:Literal ID="litServiceApplicationDescription" Text='<%# GetUIString("SVCAPPDESC") %>' runat="server" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <SharePoint:FormDigest ID="FormDigest" runat="server" />
    <script type="text/javascript">
        function ULSpupu() { var o = new Object; o.ULSTeamName = "SharePoint Portal Server"; o.ULSFileName = "serviceapp.aspx"; return o; }
        // <![CDATA[
        _spBodyOnLoadFunctionNames.push("SetFocus");
        function SetFocus() {
            ULSpupu: ;
            var txtServiceName = (document.getElementById("<%= txtServiceApplicationName.ClientID %>"));
            if (!txtServiceName.disabled) {
                txtServiceName.focus();
            }
            else {

                var inputElements = document.getElementsByTagName("input");
                if (inputElements != null && inputElements.length > 0) {
                    var len = inputElements.length;
                    var i = 0;
                    for (i = 0; i < len; i++) {
                        if (inputElements[i].name.indexOf("TxtDatabaseServer") > -1) {
                            inputElements[i].focus();
                            return true;
                        }
                    }
                }
            }
            return true;
        }
        // ]]>
    </script>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="PlaceHolderDialogBodyMainSection" runat="server">
    <SPSWC:PageLevelError runat="server" Id="pageLevelError" />
    <table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet">

        <wssuc:InputFormSection runat="server" 
            Title="Name"
            id="ServiceAppSection" >
            <Template_Description>
                <p>
                    <%# GetUIString("SVCFORMSECTIONDESC") %>.
                </p>
            </Template_Description>
            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText=<%# GetUIString("SVCFORMSECTIONDESC") %> >
                    <Template_Control>
                        <SharePoint:InputFormTextBox title='<%# GetUIString("SVCFORMSECTIONDESC") %>' class="ms-input" ID="txtServiceApplicationName" Columns="50" Runat="server" MaxLength="80" />
                        <SharePoint:InputFormRequiredFieldValidator id="valServiceApplicationName" 
                            ControlToValidate="txtServiceApplicationName" 
                            ErrorMessage='<%# GetUIString("SVCFORMSECTIONREQUIRED") %>'   
                            Runat="server"/>
                        <SharePoint:InputFormCustomValidator ID="valServiceApplicationNameIsUnique" 
                            ControlToValidate="txtServiceApplicationName" 
                            OnServerValidate="ValidateUniqueName" 
                            ErrorMessage='<%# GetUIString("SVCFORMSECTIONUSED") %>'    
                            runat="server" />
                    </Template_Control>
                </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>
        <wssuc:InputFormSection runat="server" Title='<%# GetUIString("SVCCPFORMSECTIONTITLE") %>'  id="ClaimsAppSection">
            <Template_Description>
                <p>
                    <%# GetUIString("SVCCPFORMSECTIONDESC") %> 
                </p>
            </Template_Description>   
            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText='<%# GetUIString("SVCCPFORMSECTIONLABEL") %>' >
                    <Template_Control>
                        <SharePoint:InputFormTextBox title='<%# GetUIString("SVCCPFORMSECTIONLABEL") %>' class="ms-input" ID="txtInputFormDisplayClaimName" Columns="35" Runat="server" MaxLength="80" />
                        <SharePoint:InputFormRequiredFieldValidator id="InputFormRequiredFieldValidator2" 
                            ControlToValidate="txtInputFormDisplayClaimName" 
                            ErrorMessage='<%# GetUIString("SVCCPFORMSECTIONLABELREQUIRED") %>'  
                            Runat="server"/> 
			         </Template_control>
		        </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText=<%# GetUIString("SVCCPFORMSECTIONDESC2") %> >
                    <Template_Control>
                        <SharePoint:InputFormTextBox title='<%# GetUIString("SVCCPFORMSECTIONLABEL2") %>' class="ms-input" ID="txtInputFormTextClaimDesc" Columns="35" Runat="server" MaxLength="80" />
                        <SharePoint:InputFormRequiredFieldValidator id="InputFormRequiredFieldValidator1" 
                            ControlToValidate="txtInputFormTextClaimDesc" 
                            ErrorMessage= '<%# GetUIString("SVCCPFORMSECTIONDESCREQUIRED") %>' 
                            Runat="server"/> 
			         </Template_control>
		        </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText='<%# GetUIString("SVCCPFORMSECTIONVISIBILITY") %>' >
                    <Template_Control>
                        <asp:CheckBox runat="server" ID="visibilityCB" Text='<%# GetUIString("SVCCPFORMSECTIONALWAYS") %>' /> 
                    </Template_Control>
                </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>
        <wssuc:InputFormSection runat="server" Title='<%# GetUIString("SVCIDPECTIONTITLE") %>' id="IdentityAppSection" >
            <Template_Description>
                <p>
                    <%# GetUIString("SVCIDPECTIONTITLEDESC") %>
                </p>
            </Template_Description>
            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText='<%# GetUIString("SVCIDPECTIONTYPE") %>' >
                    <Template_Control>
                        <asp:DropDownList title="Type du fournisseur d'identités (Identity Token Issuer)" class="ms-input" ID="InputClaimProviderDropBox" Runat="server" Width="470px" CausesValidation="false" AutoPostBack="True">
                        </asp:DropDownList>
                    </Template_Control>
                </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>        

        <wssuc:ContentDatabaseSection Id="DatabaseSection"
            Title='<%# GetUIString("SVCDBTITLE") %>' 
            IncludeSearchServer="false"
            IncludeFailoverDatabaseServer="true"
            runat="server">
        </wssuc:ContentDatabaseSection>
        <wssuc:InputFormSection runat="server" Title='<%# GetUIString("SVCDBOPTTITLE") %>' id="ReplaceDBAppSection" >
            <Template_Description>
                <p>
                    <%# GetUIString("SVCDBSUBTITLE") %>
                </p>
            </Template_Description>
            <Template_InputFormControls>
                <wssuc:InputFormControl ID="InputFormReplaceDB" runat="server" LabelText='<%# GetUIString("SVCDBREUSELABEL") %>' >
                    <Template_Control>
                        <asp:CheckBox runat="server" ID="CBReplaceDB" Text='<%# GetUIString("SVCDBREUSEDESC") %>' /> 
                    </Template_Control>
                </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>
        <wssuc:IisWebServiceApplicationPoolSection 
            id="ApplicationPoolSection" 
            runat="server" />
    </table>
</asp:Content>
