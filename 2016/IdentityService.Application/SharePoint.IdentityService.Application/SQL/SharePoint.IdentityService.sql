SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS OFF
GO

CREATE TABLE [dbo].AssemblyConfiguration
(
[AssemblyFulldescription] varchar(256) NOT NULL ,
[AssemblyTypeDescription] varchar(256) NOT NULL ,
[TraceResolve] [bit] NOT NULL,
[Selected] [bit] NOT NULL,
[ClaimsExt] [bit] NOT NULL,
CONSTRAINT [PK_AssemblyConfiguration] PRIMARY KEY CLUSTERED ( [AssemblyFulldescription] )
)
GO

ALTER TABLE [dbo].[AssemblyConfiguration] ADD  CONSTRAINT [DF_AssemblyConfiguration_TraceResolve]  DEFAULT ((0)) FOR [TraceResolve]
GO

ALTER TABLE [dbo].[AssemblyConfiguration] ADD  CONSTRAINT [DF_AssemblyConfiguration_Selected]  DEFAULT ((0)) FOR [Selected]
GO

ALTER TABLE [dbo].[AssemblyConfiguration] ADD  CONSTRAINT [DF_AssemblyConfiguration_ClaimsExt]  DEFAULT ((0)) FOR [ClaimsExt]
GO

INSERT INTO [dbo].[AssemblyConfiguration] ([AssemblyFulldescription], [AssemblyTypeDescription], [TraceResolve], [Selected], [ClaimsExt])
VALUES ('SharePoint.IdentityService.ActiveDirectory, Version=16.0.0.0, Culture=neutral, PublicKeyToken=$SharePoint.Project.AssemblyPublicKeyToken$','SharePoint.IdentityService.ActiveDirectory.ActiveDirectoryWrapper', 1, 1, 0)
GO

INSERT INTO [dbo].[AssemblyConfiguration] ([AssemblyFulldescription], [AssemblyTypeDescription], [TraceResolve], [Selected], [ClaimsExt]) 
VALUES ('SharePoint.IdentityService.Application, Version=16.0.0.0, Culture=neutral, PublicKeyToken=$SharePoint.Project.AssemblyPublicKeyToken$','SharePoint.IdentityService.ClaimsAugmenter', 1, 1, 1)
GO

CREATE TABLE [dbo].[ConnectionConfiguration]
(
[ConnectionName] [nchar](18) NOT NULL,
[UserName] [varchar](50) NOT NULL,
[Password] [varchar](1024) NOT NULL,
[TimeOut] [smallint] NULL,
[Secure] [bit] NULL,
[MaxRows] [int] NULL,
[ConnectString] [varchar](1024) NULL,
CONSTRAINT [PK_ConnectionConfiguration] PRIMARY KEY CLUSTERED ([ConnectionName])
)
GO

ALTER TABLE [dbo].[ConnectionConfiguration] ADD  CONSTRAINT [DF_ConnectionConfiguration_TimeOut]  DEFAULT ((30)) FOR [TimeOut]
GO

ALTER TABLE [dbo].[ConnectionConfiguration] ADD  CONSTRAINT [DF_ConnectionConfiguration_Secure]  DEFAULT ((1)) FOR [Secure]
GO

ALTER TABLE [dbo].[ConnectionConfiguration] ADD  CONSTRAINT [DF_ConnectionConfiguration_MaxRows]  DEFAULT ((200)) FOR [MaxRows]
GO

INSERT INTO [dbo].[ConnectionConfiguration] ([ConnectionName], [UserName],	[Password],	[TimeOut], [Secure], [MaxRows])
VALUES ('default','yourdomain\youruser', 'password', 30, 1, 200)
GO

CREATE TABLE [dbo].[DomainConfiguration](
[DnsName] [varchar](100) NOT NULL,
[DisplayName] [varchar](100) NOT NULL,
[Enabled] [bit] NULL,
[Connection] [nchar](18) NOT NULL,
[DisplayPosition] int NULL,
CONSTRAINT [PK_DomainConfiguration] PRIMARY KEY CLUSTERED ([DisplayName])
)

ALTER TABLE [dbo].[DomainConfiguration]  WITH NOCHECK ADD  CONSTRAINT [FK_DomainConfiguration_ConnectionConfiguration] FOREIGN KEY([Connection]) REFERENCES [dbo].[ConnectionConfiguration] ([ConnectionName])
GO

ALTER TABLE [dbo].[DomainConfiguration] CHECK CONSTRAINT [FK_DomainConfiguration_ConnectionConfiguration]
GO

ALTER TABLE [dbo].[DomainConfiguration] ADD  CONSTRAINT [DF_DomainConfiguration_Enabled]  DEFAULT ((1)) FOR [Enabled]
GO

ALTER TABLE [dbo].[DomainConfiguration] ADD  CONSTRAINT [DF_DomainConfiguration_DisplayPosition]  DEFAULT ((1)) FOR [DisplayPosition]
GO


CREATE TABLE [dbo].[GeneralParameters](
[ParamName] [varchar](50) NOT NULL,
[ParamValue] [varchar](512) NOT NULL,
CONSTRAINT [PK_GeneralParameters] PRIMARY KEY CLUSTERED ([ParamName]))
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('SmoothRequestor', 'Smooth')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ClaimsMode', 'Federated')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ClaimDisplayMode', 'DisplayName')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ClaimIdentityMode', 'UserPrincipalName')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ClaimIdentity', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ClaimRoleMode', 'SID')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ClaimRole', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('PeoplePickerDisplayMode', 'DisplayNameAndEmail')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('PeoplePickerImages', 'false')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('SearchByMail', 'true')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('SearchByDisplayName', 'true')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('TrustedLoginProviderName', 'yourtrustedidentityprovider')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ClaimProviderName', 'yourclaimprovidername')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ClaimDisplayName', 'yourclaimproviderdisplayname')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ShowSystemNodes', 'false')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('SupportsUserKey', 'true')
GO

INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('CacheDuration', '1440')
GO


CREATE TABLE [dbo].[CacheData](
[MachineName] [varchar](50) NOT NULL,
[TimeStamp] [datetime] NOT NULL,
[Version] [int] IDENTITY(1,1) NOT NULL,
[Data] [nvarchar](max) NOT NULL,
[IsInprocess] [bit] NOT NULL,
[IsLoaded] [bit] NOT NULL,
CONSTRAINT [PK_CacheData] PRIMARY KEY CLUSTERED ([Version] ASC))
GO

ALTER TABLE [dbo].[CacheData] ADD  CONSTRAINT [DF_CacheData_IsInprocess]  DEFAULT ((0)) FOR [IsInprocess]
GO

ALTER TABLE [dbo].[CacheData] ADD  CONSTRAINT [DF_CacheData_IsLoaded]  DEFAULT ((0)) FOR [IsLoaded]
GO

SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO