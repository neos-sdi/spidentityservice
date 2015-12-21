<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="SPSWC" Namespace="Microsoft.SharePoint.Portal.WebControls" Assembly="Microsoft.SharePoint.Portal, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="/_controltemplates/InputFormSection.ascx" %> 
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="/_controltemplates/InputFormControl.ascx" %> 
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="serviceappproxy.aspx.cs" Inherits="MyCorp.SP.ServiceApplication.AdminPages.ServiceAppProxyPage" MasterPageFile="~/_layouts/dialog.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderDialogHeaderPageTitle" runat="server">
    <asp:Literal ID="litServiceApplicationProxyTitle" Text="MyCorp Service Application Proxy" runat="server" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderDialogDescription" runat="server">
    <asp:Literal ID="litServiceApplicationProxyDescription" Text="Specify the name and settings for this MyCorp Service Application Proxy." runat="server" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
    <SharePoint:FormDigest ID="FormDigest" runat="server" />
    <script type="text/javascript">
        function ULSpupu() { var o = new Object; o.ULSTeamName = "SharePoint Portal Server"; o.ULSFileName = "serviceappproxy.aspx"; return o; }
        // <![CDATA[
        _spBodyOnLoadFunctionNames.push("SetFocus");
        function SetFocus() {
            ULSpupu: ;
            var txtServiceName = (document.getElementById("<%= txtServiceApplicationProxyName.ClientID %>"));
            if (!txtServiceName.disabled) {
                txtServiceName.focus();
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
            id="ServiceAppProxySection" >
            <Template_Description>
                <p>
                    Give your MyCorp Service Application Proxy a name. The name entered here will be displayed in the list of Service Applications on the Manage Service Applications page.
                </p>
            </Template_Description>
            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="">
                    <Template_Control>
                        <SharePoint:InputFormTextBox title="Service Application Proxy Name" class="ms-input" ID="txtServiceApplicationProxyName" Columns="35" Runat="server" MaxLength="256" />
                        <SharePoint:InputFormRequiredFieldValidator id="valServiceApplicationProxyName" 
                            ControlToValidate="txtServiceApplicationProxyName" 
                            ErrorMessage="Required field" 
                            Runat="server"/>
                        <SharePoint:InputFormCustomValidator ID="valServiceApplicationProxyNameIsUnique" 
                            ControlToValidate="txtServiceApplicationProxyName" 
                            OnServerValidate="ValidateUniqueName" 
                            ErrorMessage="Name already in use"
                            runat="server" />
                    </Template_Control>
                </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>
        
        <wssuc:InputFormSection runat="server" 
            Title="WCF Proxy Channel Settings"
            id="ServiceAppProxyChannelSettings" >
            <Template_Description>
                <p>
                    Specify the default WCF proxy communication channel settings for this service application proxy.
                </p>
            </Template_Description>
            <Template_InputFormControls>
                <wssuc:InputFormControl runat="server" LabelText="">
                    <Template_Control>
                        <p>
                            Open Timeout (in seconds):<br/>
                            <SharePoint:InputFormTextBox title="Open Timeout" class="ms-input" ID="txtOpenChannelTimeout" Columns="35" Runat="server" MaxLength="20" />
                            <SharePoint:InputFormRequiredFieldValidator id="valOpenChannelTimeout" 
                                ControlToValidate="txtOpenChannelTimeout" 
                                ErrorMessage="Required field" 
                                Runat="server"/>
                            <SharePoint:InputFormRegularExpressionValidator id="valOpenChannelTimeout2" ValidationExpression="^[0-9]+$" controlToValidate="txtOpenChannelTimeout" ErrorMessage="Must be an integer" runat="server" />
                        </p>
                        <p>
                            Send Timeout (in seconds):<br/>
                            <SharePoint:InputFormTextBox title="Send Timeout" class="ms-input" ID="txtSendChannelTimeout" Columns="35" Runat="server" MaxLength="20" />
                            <SharePoint:InputFormRequiredFieldValidator id="valSendChannelTimeout" 
                                ControlToValidate="txtSendChannelTimeout" 
                                ErrorMessage="Required field" 
                                Runat="server"/>
                            <SharePoint:InputFormRegularExpressionValidator id="valSendChannelTimeout2" ValidationExpression="^[0-9]+$" controlToValidate="txtSendChannelTimeout" ErrorMessage="Must be an integer" runat="server" />
                        </p>
                        <p>
                            Receive Timeout (in seconds):<br/>
                            <SharePoint:InputFormTextBox title="Receive Timeout" class="ms-input" ID="txtReceiveChannelTimeout" Columns="35" Runat="server" MaxLength="20" />
                            <SharePoint:InputFormRequiredFieldValidator id="valReceiveChannelTimeout" 
                                ControlToValidate="txtReceiveChannelTimeout" 
                                ErrorMessage="Required field" 
                                Runat="server"/>
                            <SharePoint:InputFormRegularExpressionValidator id="valReceiveChannelTimeout2" ValidationExpression="^[0-9]+$" controlToValidate="txtReceiveChannelTimeout" ErrorMessage="Must be an integer" runat="server" />
                        </p>
                        <p>
                            Close Timeout (in seconds):<br/>
                            <SharePoint:InputFormTextBox title="Close Timeout" class="ms-input" ID="txtCloseChannelTimeout" Columns="35" Runat="server" MaxLength="20" />
                            <SharePoint:InputFormRequiredFieldValidator id="valCloseChannelTimeout" 
                                ControlToValidate="txtCloseChannelTimeout" 
                                ErrorMessage="Required field" 
                                Runat="server"/>
                            <SharePoint:InputFormRegularExpressionValidator id="valCloseChannelTimeout2" ValidationExpression="^[0-9]+$" controlToValidate="txtCloseChannelTimeout" ErrorMessage="Must be an integer" runat="server" />
                        </p>
                        <p>
                            Maximum Execution Time (in seconds):<br/>
                            <SharePoint:InputFormTextBox title="Maximum Execution Time" class="ms-input" ID="txtMaximumExecutionTime" Columns="35" Runat="server" MaxLength="20" />
                            <SharePoint:InputFormRequiredFieldValidator id="valMaximumExecutionTime" 
                                ControlToValidate="txtMaximumExecutionTime" 
                                ErrorMessage="Required field" 
                                Runat="server"/>
                            <SharePoint:InputFormRegularExpressionValidator id="valMaximumExecutionTime2" ValidationExpression="^[0-9]+$" controlToValidate="txtMaximumExecutionTime" ErrorMessage="Must be an integer" runat="server" />
                        </p>
                    </Template_Control>
                </wssuc:InputFormControl>
            </Template_InputFormControls>
        </wssuc:InputFormSection>

    </table>
</asp:Content>