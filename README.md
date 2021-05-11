# spidentityservice
# SharePoint ClaimProvider (SPClaimProvider) Application for SharePoint

Publishing SharePoint on the internet, using cloud services, Office 365 has become commonplace.

However, companies needs to master the identity management of their employees and partners while providing unified access.

Since the 2010 version, SharePoint is able to integrate with identity federation systems for identification (WSFED, SAML, OpenID-Connect (2016)) or authorization protocols like OAuth 2 (Notably used for model Apps (SP 2013))

In this context, based on standards, no implementation has been provided for managing federated identification, this is widely open. 

To make SharePoint working fine with Microsoft ADFS federation server or other IDP,  you must implement, develop a "Claim Provider" component. The Claim Provider component provide the necessary access to corporate directories, Build of the Security Token, and an efficient Peoplepicker.

You can find many examples on the internet for a "Custom Claim Provider". So, is not new ! 
And, our component too is not new ! he is powerful, generic, extensible, "industrial". 
The first version was build in 2010 and was designed to handle an infrastructure of fifty Active Directory forests, spread worldwide for tens of thousands users. Since, almost all of our SharePoint farm projects use this component, either for Windows Authentication, but especially in projects where the SSO takes place. Today many of our customers use "SharePoint Identity Service" in production.
