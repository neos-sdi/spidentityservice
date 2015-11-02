SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO

IF COL_LENGTH('DomainConfiguration','DisplayPosition') IS NULL
BEGIN
   ALTER TABLE [dbo].[DomainConfiguration]	ADD DisplayPosition int NULL
END
GO

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_DomainConfiguration_DisplayPosition]') AND type = 'D')
BEGIN
   ALTER TABLE [dbo].[DomainConfiguration] ADD  CONSTRAINT [DF_DomainConfiguration_DisplayPosition]  DEFAULT ((0)) FOR [DisplayPosition]
   UPDATE [dbo].[DomainConfiguration] SET [DisplayPosition]=0
END   
GO

IF COL_LENGTH('ConnectionConfiguration','ConnectString') IS NULL
BEGIN
	ALTER TABLE [dbo].[ConnectionConfiguration]	ADD ConnectString [nvarchar](1024) NULL  
END
GO

IF COL_LENGTH('AssemblyConfiguration','Selected') IS NULL
BEGIN
   ALTER TABLE [dbo].[AssemblyConfiguration] ADD Selected [bit] NOT NULL  
END
GO

IF COL_LENGTH('AssemblyConfiguration','ClaimsExt') IS NULL
BEGIN
   ALTER TABLE [dbo].[AssemblyConfiguration] ADD ClaimsExt [bit] NOT NULL  
END
GO

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AssemblyConfiguration_Selected]') AND type = 'D')
BEGIN
   ALTER TABLE [dbo].[AssemblyConfiguration] ADD  CONSTRAINT [DF_AssemblyConfiguration_Selected]  DEFAULT ((0)) FOR [Selected]
   UPDATE [dbo].[AssemblyConfiguration] SET [Selected]=1 WHERE [AssemblyTypeDescription] = 'SharePoint.IdentityService.ActiveDirectory.ActiveDirectoryWrapper' AND [AssemblyFulldescription] = 'SharePoint.IdentityService.ActiveDirectory, Version=1.0.0.1, Culture=neutral, PublicKeyToken=ad9787278992c174'   
END
GO

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AssemblyConfiguration_ClaimsExt]') AND type = 'D')
BEGIN
   ALTER TABLE [dbo].[AssemblyConfiguration] ADD  CONSTRAINT [DF_AssemblyConfiguration_ClaimsExt]  DEFAULT ((0)) FOR [ClaimsExt]
   UPDATE [dbo].[AssemblyConfiguration] SET [ClaimsExt]=1 WHERE [AssemblyTypeDescription] = 'SharePoint.IdentityService.ClaimsAugmenter' AND [AssemblyFulldescription] = 'SharePoint.IdentityService.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ad9787278992c174'   
END
GO


IF NOT EXISTS (SELECT * FROM [dbo].[AssemblyConfiguration] WHERE [AssemblyTypeDescription] = 'SharePoint.IdentityService.ActiveDirectory.ActiveDirectoryWrapper' AND [AssemblyFulldescription] = 'SharePoint.IdentityService.ActiveDirectory, Version=1.0.0.1, Culture=neutral, PublicKeyToken=ad9787278992c174')   
BEGIN
    UPDATE [dbo].[AssemblyConfiguration] SET [Selected]=0 WHERE [AssemblyTypeDescription] = 'SharePoint.IdentityService.ActiveDirectory.ClaimsAugmenter'    
	INSERT INTO [dbo].[AssemblyConfiguration] ([AssemblyFulldescription], [AssemblyTypeDescription], [TraceResolve], [Selected], [ClaimsExt]) 
	VALUES ('SharePoint.IdentityService.ActiveDirectory, Version=1.0.0.1, Culture=neutral, PublicKeyToken=ad9787278992c174','SharePoint.IdentityService.ActiveDirectory.ActiveDirectoryWrapper ', 0, 1, 0)
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[AssemblyConfiguration] WHERE [AssemblyTypeDescription] = 'SharePoint.IdentityService.ClaimsAugmenter' AND [AssemblyFulldescription] = 'SharePoint.IdentityService.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ad9787278992c174')   
BEGIN
    UPDATE [dbo].[AssemblyConfiguration] SET [Selected]=0 WHERE [AssemblyTypeDescription] = 'SharePoint.IdentityService.ClaimsAugmenter'    
	INSERT INTO [dbo].[AssemblyConfiguration] ([AssemblyFulldescription], [AssemblyTypeDescription], [TraceResolve], [Selected], [ClaimsExt]) 
	VALUES ('SharePoint.IdentityService.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ad9787278992c174','SharePoint.IdentityService.ClaimsAugmenter', 1, 1, 1)
END
GO


IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_AssemblyConfiguration_TraceResolve]') AND type = 'D')
BEGIN
   ALTER TABLE [dbo].[AssemblyConfiguration] ADD  CONSTRAINT [DF_AssemblyConfiguration_TraceResolve]  DEFAULT ((0)) FOR [TraceResolve]
END   
GO

IF NOT EXISTS (SELECT [ParamName] FROM [dbo].[GeneralParameters] WHERE [ParamName] = 'ShowSystemNodes')
BEGIN
	INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ShowSystemNodes', 'false')
END
GO

IF NOT EXISTS (SELECT [ParamName] FROM [dbo].[GeneralParameters] WHERE [ParamName] = 'CacheDuration')
BEGIN
	INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('CacheDuration', '15')
END
GO

IF NOT EXISTS (SELECT [ParamName] FROM [dbo].[GeneralParameters] WHERE [ParamName] = 'ClaimIdentityMode')
BEGIN
	INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ClaimIdentityMode', 'UserPrincipalName')
END
GO

IF NOT EXISTS (SELECT [ParamName] FROM [dbo].[GeneralParameters] WHERE [ParamName] = 'ClaimRoleMode')
BEGIN
	INSERT INTO [dbo].[GeneralParameters] ([ParamName], [ParamValue]) VALUES ('ClaimRoleMode', 'SID')
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CacheData]') AND type in (N'U'))
BEGIN
   CREATE TABLE [dbo].[CacheData](
		[MachineName] [nvarchar](50) NOT NULL,
		[TimeStamp] [datetime] NOT NULL,
		[Version] [int] IDENTITY(1,1) NOT NULL,
		[Data] [nvarchar](max) NOT NULL,
		[IsInprocess] [bit] NOT NULL,
		[IsLoaded] [bit] NOT NULL,
		CONSTRAINT [PK_CacheData] PRIMARY KEY CLUSTERED ([Version] ASC)) 
END
GO

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_CacheData_IsInprocess]') AND type = 'D')
BEGIN
   ALTER TABLE [dbo].[CacheData] ADD  CONSTRAINT [DF_CacheData_IsInprocess]  DEFAULT ((0)) FOR [IsInprocess]
END   
GO

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_CacheData_IsLoaded]') AND type = 'D')
BEGIN
   ALTER TABLE [dbo].[CacheData] ADD  CONSTRAINT [DF_CacheData_IsLoaded]  DEFAULT ((0)) FOR [IsLoaded]
END   
GO

SET QUOTED_IDENTIFIER OFF
GO
SET ANSI_NULLS ON
GO


