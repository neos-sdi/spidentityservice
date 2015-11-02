@ECHO OFF
CLS
ECHO ************************************************************************

ECHO This deployment Script must be run on each server of the SharePoint Farm

ECHO ************************************************************************

gacutil -u "ActiveDirectory.IdentityService.ClaimsProvider.dll"
gacutil -i "ActiveDirectory.IdentityService.ClaimsProvider.dll"




iisreset
ECHO
ECHO
ECHO ************************************************************************

ECHO This deployment Script must be run on each server of the SharePoint Farm

ECHO ************************************************************************
PAUSE