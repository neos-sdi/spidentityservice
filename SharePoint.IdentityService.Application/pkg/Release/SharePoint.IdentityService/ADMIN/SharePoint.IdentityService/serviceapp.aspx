<%@ Assembly Name="SharePoint.IdentityService.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ad9787278992c174" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SPSWC" Namespace="Microsoft.SharePoint.Portal.WebControls" Assembly="Microsoft.SharePoint.Portal, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="/_controltemplates/InputFormSection.ascx" %> 
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="/_controltemplates/InputFormControl.ascx" %> 
<%@ Register TagPrefix="wssuc" TagName="ContentDatabaseSection" src="~/_admin/ContentDatabaseSection.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="IisWebServiceApplicationPoolSection" src="~/_admin/IisWebServiceApplicationPoolSection.ascx" %>
<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="serviceapp.aspx.cs" Inherits="SharePoint.IdentityService.AdminPages.ServiceAppPage" MasterPageFile="~/_layouts/dialog.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderDialogHeaderPageTitle" runat="server">
    <asp:Literal ID="litServiceApplicationTitle" Text="SharePoint Identity Service Application" runat="server" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderDialogDescription" runat="server">
    <asp:Literal ID="litServiceApplicationDescription" Text="Spécifier le nom, la base de données, le pool d’applications pour cette appliction Identity Service " runat="server" />
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
                    Nom de l'Application de Service.
                </p>
            </Template_Description>
            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Nom de l'application de Service">
                    <Template_Control>
                        <SharePoint:InputFormTextBox title="Nom de l'application de Service" class="ms-input" ID="txtServiceApplicationName" Columns="80" Runat="server" MaxLength="256" />
                        <SharePoint:InputFormRequiredFieldValidator id="valServiceApplicationName" 
                            ControlToValidate="txtServiceApplicationName" 
                            ErrorMessage="Le nom de l'application est requis !" 
                            Runat="server"/>
                        <SharePoint:InputFormCustomValidator ID="valServiceApplicationNameIsUnique" 
                            ControlToValidate="txtServiceApplicationName" 
                            OnServerValidate="ValidateUniqueName" 
                            ErrorMessage="Le nom de cette application est déjà utilisé !"
                            runat="server" />
                    </Template_Control>
                </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>
        <wssuc:InputFormSection runat="server" Title="Fournisseur de Revendications" id="ClaimsAppSection">
            <Template_Description>
                <p>
                    Vous devez indiquer le Nom, la description et la visibilité de votre fournisseur de revendications. 
                </p>
            </Template_Description>
            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Nom du fournisseur de revendications" >
                    <Template_Control>
                        <SharePoint:InputFormTextBox title="Nom du fournisseur de revendications (Claim Provider)" class="ms-input" ID="txtInputFormTextClaimName" Columns="35" Runat="server" MaxLength="35" size="25" />
                        <SharePoint:InputFormRequiredFieldValidator id="ClaimsNameRequiredFieldValidator" 
                            ControlToValidate="txtInputFormTextClaimName" 
                            ErrorMessage="Le nom du fournisseur de revendications est requis !" 
                            Runat="server"/>
                        <SharePoint:InputFormCustomValidator ID="ClaimsNameCustomValidator" 
                            ControlToValidate="txtInputFormTextClaimName" 
                            OnServerValidate="ValidateUniqueClaimName" 
                            ErrorMessage="nom du fournisseur de revendications est déjà utilisé !"
                            runat="server" /> 
			         </Template_control>
		        </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText="Libéllé d'affichage du fournisseur de revendications" >
                    <Template_Control>
                        <SharePoint:InputFormTextBox title="Libéllé d'affichage du fournisseur de revendications" class="ms-input" ID="txtInputFormDisplayClaimName" Columns="80" Runat="server" MaxLength="256" size="25" />
                        <SharePoint:InputFormRequiredFieldValidator id="InputFormRequiredFieldValidator2" 
                            ControlToValidate="txtInputFormDisplayClaimName" 
                            ErrorMessage="Le libéllé du fournisseur de revendications est requis !" 
                            Runat="server"/> 
			         </Template_control>
		        </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText="Description du fournisseur de revendications" >
                    <Template_Control>
                        <SharePoint:InputFormTextBox title="Description du fournisseur de revendications (Claim Provider)" class="ms-input" ID="txtInputFormTextClaimDesc" Columns="80" Runat="server" MaxLength="256" size="25" />
                        <SharePoint:InputFormRequiredFieldValidator id="InputFormRequiredFieldValidator1" 
                            ControlToValidate="txtInputFormTextClaimDesc" 
                            ErrorMessage="La description du fournisseur de revendications est requise !" 
                            Runat="server"/> 
			         </Template_control>
		        </wssuc:InputFormControl>
                <wssuc:InputFormControl runat="server" LabelText="Visibilité du fournisseur de revendications" >
                    <Template_Control>
                        <asp:CheckBox runat="server" ID="visibilityCB" Text="Toujours visible (IsUsedByDefault)" /> 
                    </Template_Control>
                </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>
        <wssuc:InputFormSection runat="server" Title="Fournisseur d'identité" id="IdentityAppSection" >
            <Template_Description>
                <p>
                    indiquez avec quel fournisseur d'identité (Windows, Fédéré (ADFS, ACS,...)) sera associé votre fournisseur de revendications.
                </p>
            </Template_Description>
            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="Type du fournisseur d'identités" >
                    <Template_Control>
                        <asp:DropDownList title="Type du fournisseur d'identités (Identity Token Issuer)" class="ms-input" ID="InputClaimProviderDropBox" Runat="server" Width="500px" CausesValidation="false" AutoPostBack="True">
                        </asp:DropDownList>
                    </Template_Control>
                </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>        

        <wssuc:ContentDatabaseSection Id="DatabaseSection"
            Title="Base de données"
            IncludeSearchServer="false"
            IncludeFailoverDatabaseServer="true"
            runat="server">
        </wssuc:ContentDatabaseSection>
        <wssuc:InputFormSection runat="server" Title="Database Options" id="ReplaceDBAppSection" >
            <Template_Description>
                <p>
                    indiquez si vous souhaitez réutiliser la base de données existante.
                </p>
            </Template_Description>
            <Template_InputFormControls>
                <wssuc:InputFormControl ID="InputFormReplaceDB" runat="server" LabelText="Réutiliser la base de données existante" >
                    <Template_Control>
                        <asp:CheckBox runat="server" ID="CBReplaceDB" Text="Réutiliser la base de données" /> 
                    </Template_Control>
                </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>
        <wssuc:IisWebServiceApplicationPoolSection 
            id="ApplicationPoolSection" 
            runat="server" />
    </table>
</asp:Content>
