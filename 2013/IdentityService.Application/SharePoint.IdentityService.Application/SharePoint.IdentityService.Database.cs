//******************************************************************************************************************************************************************************************//
// Copyright (c) 2015 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
namespace SharePoint.IdentityService
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Security.Principal;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;
    using System.Data;
    using System.Text;
    using System.IO;
    using System.Diagnostics.CodeAnalysis;
    using SharePoint.IdentityService.Core;

    [System.Runtime.InteropServices.Guid("1AE44DE8-B658-404C-82FE-01287C82EE6A")]
    public sealed class ActiveDirectoryIdentityServiceDatabase : SPDatabase
    {
        #region SPDatabase
        /// <summary>
        /// ActiveDirectoryIdentityServiceDatabase constructor
        /// </summary>
        public ActiveDirectoryIdentityServiceDatabase()
        {
        
        }

        /// <summary>
        /// ConnectString property
        /// </summary>
        public string ConnectString()
        {
            return DatabaseConnectionString; 
        }

        /// <summary>
        /// ActiveDirectoryIdentityServiceDatabase constructor
        /// </summary>
        internal ActiveDirectoryIdentityServiceDatabase(SPDatabaseParameters databaseParameters): base(databaseParameters)
        {
            this.Status = SPObjectStatus.Disabled;
        }

        /// <summary>
        /// Provision method override
        /// </summary>
        public override void Provision()
        {
            if (SPObjectStatus.Online == this.Status)
            {
                return;
            }
            this.Status = SPObjectStatus.Provisioning;
            this.Update();

            Dictionary<DatabaseOptions, bool> options = new Dictionary<DatabaseOptions, bool>(1);
            options.Add(DatabaseOptions.AutoClose, false);

            if (!this.Exists)
                SPDatabase.Provision(ConnectString(), SPUtility.GetVersionedGenericSetupPath(@"Template\sql\SharePoint.IdentityService.sql", 15), options);

            this.Status = SPObjectStatus.Online;
            this.Update();
        }

        /// <summary>
        /// Unprovision method override
        /// </summary>
        public override void Unprovision()
        {
            this.Status = SPObjectStatus.Unprovisioning;
            this.Update();
            try
            {
                base.Unprovision();
                this.Status = SPObjectStatus.Offline;
                this.Update();
            }
            finally
            {

            }
        }


        /// <summary>
        /// Upgrade method implementation
        /// </summary>
        public override void Upgrade()
        {
            var builder = new SqlConnectionStringBuilder(ConnectString()) { Pooling = false };
            var connectionString = builder.ToString();

            var minimumCommandTimeout = 0;
            if (SPFarm.Local != null)
            {
                var service = SPFarm.Local.Services.GetValue<SPDatabaseService>();
                minimumCommandTimeout = service.CommandTimeout;
            }
            string fileName = SPUtility.GetVersionedGenericSetupPath(@"Template\sql\SharePoint.IdentityService.Upgrade.sql", 15);
            SqlConnection sqlConnection = null;
            SqlTransaction sqlTransaction = null;
            try
            {
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();
                sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.ReadUncommitted, "DbScripts");

                var commands = ParseCommands(fileName, false);
                foreach (string commandText in commands)
                {
                    sqlConnection.Execute(commandText, null, sqlTransaction, minimumCommandTimeout, CommandType.Text);
                }
                sqlTransaction.Commit();
            }
            catch (Exception)
            {
                if (sqlTransaction != null)
                    sqlTransaction.Rollback();
                throw;
            }
            finally
            {
                if (sqlTransaction != null)
                    sqlTransaction.Dispose();
                if (sqlConnection != null)
                {
                    if (sqlConnection.State != ConnectionState.Closed)
                        sqlConnection.Close();
                    sqlConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// GrantApplicationPoolAccess method implementation
        /// </summary>
        internal void GrantApplicationPoolAccess(SecurityIdentifier processSecurityIdentifier)
        {
            this.GrantAccess(processSecurityIdentifier, "db_owner");
        }

        /// <summary>
        /// IsParametersMatch method implementation
        /// </summary>
        internal bool IsParametersMatch(SPDatabaseParameters databaseParameters)
        {
            return (((!(databaseParameters.Database != base.Name) && !(databaseParameters.Server != base.NormalizedDataSource)) && (!(databaseParameters.Username != base.Username) && !(databaseParameters.Password != base.Password))));
        }
        #endregion

        #region Helpers
        /// <summary>
        /// ParseCommands method implementation
        /// </summary>
        private static IEnumerable<string> ParseCommands(string filePath, bool throwExceptionIfNonExists)
        {
            if (!File.Exists(filePath))
            {
                if (throwExceptionIfNonExists)
                    throw new FileNotFoundException("File not found", filePath);
                else
                    return new string[0];
            }
            var statements = new List<string>();
            using (var stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                string statement = "";
                while ((statement = ReadNextStatementFromStream(reader)) != null)
                {
                    statements.Add(statement);
                }
            }
            return statements.ToArray();
        }

        /// <summary>
        /// ReadNextStatementFromStream method implementation
        /// </summary>
        private static string ReadNextStatementFromStream(TextReader reader)
        {
            var sb = new StringBuilder();
            while (true)
            {
                string lineOfText = reader.ReadLine();
                if (lineOfText == null)
                {
                    if (sb.Length > 0)
                        return sb.ToString();
                    else
                        return null;
                }
                if (lineOfText.TrimEnd().ToUpper() == "GO")
                    break;
                sb.Append(lineOfText + Environment.NewLine);
            }
            return sb.ToString();
        }
        #endregion

        #region Table AssemblyConfiguration
        /// <summary>
        /// GetAssemblyConfigurationList method implmentation
        /// </summary>
        public IEnumerable<AssemblyConfiguration> GetAssemblyConfigurationList()
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT AssemblyFulldescription, AssemblyTypeDescription, TraceResolve, Selected, ClaimsExt FROM dbo.AssemblyConfiguration ORDER BY AssemblyFulldescription";
                cnx.Open();
                return cnx.Query<AssemblyConfiguration>(qry, null);
            } 
        }

        /// <summary>
        /// GetAssemblyConfiguration method implementation
        /// </summary>
        public AssemblyConfiguration GetAssemblyConfiguration()
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT AssemblyFulldescription, AssemblyTypeDescription, TraceResolve, Selected, ClaimsExt FROM dbo.AssemblyConfiguration WHERE Selected=1 AND ClaimsExt=0";
                cnx.Open();
                return cnx.Query<AssemblyConfiguration>(qry, null).First();
            }                
        }

        /// <summary>
        /// GetAssemblyAugmenter method implementation
        /// </summary>
        public AssemblyConfiguration GetAssemblyAugmenter()
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT AssemblyFulldescription, AssemblyTypeDescription, TraceResolve, Selected, ClaimsExt FROM dbo.AssemblyConfiguration WHERE Selected=1 AND ClaimsExt=1";
                cnx.Open();
                return cnx.Query<AssemblyConfiguration>(qry, null).First();
            }
        }

        /// <summary>
        /// AssemblyConfiguration method implementation
        /// </summary>
        public bool SetAssemblyConfiguration(AssemblyConfiguration cfg, AssemblyConfiguration newcfg)
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string upd = "UPDATE dbo.AssemblyConfiguration SET AssemblyFulldescription = @AssemblyFulldescription, AssemblyTypeDescription = @AssemblyTypeDescription, TraceResolve = @TraceResolve, Selected = @Selected, ClaimsExt=@ClaimsExt WHERE AssemblyFulldescription = @OldAssemblyFulldescription AND AssemblyTypeDescription = @OldAssemblyTypeDescription";
                string ins = "INSERT INTO dbo.AssemblyConfiguration (AssemblyFulldescription, AssemblyTypeDescription, TraceResolve, Selected, ClaimsExt) VALUES (@AssemblyFulldescription, @AssemblyTypeDescription, @TraceResolve, @Selected, @ClaimsExt)";
                cnx.Open();
                if (cfg != null)  // Update
                {
                    if (cnx.Execute(upd, new { AssemblyFulldescription = newcfg.AssemblyFulldescription, AssemblyTypeDescription = newcfg.AssemblyTypeDescription, TraceResolve = newcfg.TraceResolve, Selected = newcfg.Selected, ClaimsExt = newcfg.ClaimsExt, OldAssemblyFulldescription = cfg.AssemblyFulldescription, OldAssemblyTypeDescription = cfg.AssemblyTypeDescription }) >= 1)
                        return true;
                    else  // do insert not probable
                        return (cnx.Execute(ins, new { AssemblyFulldescription = newcfg.AssemblyFulldescription, AssemblyTypeDescription = newcfg.AssemblyTypeDescription, TraceResolve = newcfg.TraceResolve, Selected = newcfg.Selected, ClaimsExt = newcfg.ClaimsExt }) == 1);
                }
                else // Insert
                {
                    if (cnx.Execute(upd, new { AssemblyFulldescription = newcfg.AssemblyFulldescription, AssemblyTypeDescription = newcfg.AssemblyTypeDescription, TraceResolve = newcfg.TraceResolve, Selected = newcfg.Selected, ClaimsExt = newcfg.ClaimsExt, OldAssemblyFulldescription = newcfg.AssemblyFulldescription, OldAssemblyTypeDescription = newcfg.AssemblyTypeDescription }) >= 1)
                        return true;
                    else // do insert
                        return (cnx.Execute(ins, new { AssemblyFulldescription = newcfg.AssemblyFulldescription, AssemblyTypeDescription = newcfg.AssemblyTypeDescription, TraceResolve = newcfg.TraceResolve, Selected = newcfg.Selected, ClaimsExt = newcfg.ClaimsExt }) == 1);
                }
            }                
        }

        /// <summary>
        /// DeleteDomainConfiguration method implementation
        /// </summary>
        public bool DeleteAssemblyConfiguration(AssemblyConfiguration cfg)
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string del = "DELETE FROM dbo.AssemblyConfiguration WHERE AssemblyFulldescription = @AssemblyFulldescription AND AssemblyTypeDescription = @AssemblyTypeDescription";
                cnx.Open();
                if (cnx.Execute(del, new { AssemblyFulldescription = cfg.AssemblyFulldescription, AssemblyTypeDescription = cfg.AssemblyTypeDescription }) >= 1)
                    return true;
                else
                    return false;
            }
        }
        #endregion

        #region Table ConnectionConfiguration
        /// <summary>
        /// GetConnectionConfigurationList method implementation
        /// </summary>
        public IEnumerable<ConnectionConfiguration> GetConnectionConfigurationList()
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT ConnectionName, UserName, Password, Timeout, Secure, Maxrows, ConnectString FROM dbo.ConnectionConfiguration ORDER BY ConnectionName";
                cnx.Open();
                return cnx.Query<ConnectionConfiguration>(qry, null);
            }
        }
        /// <summary>
        /// GetConnectionConfiguration method implementation
        /// </summary>
        public ConnectionConfiguration GetConnectionConfiguration(string name)
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT ConnectionName, UserName, Password, Timeout, Secure, Maxrows, ConnectString FROM dbo.ConnectionConfiguration WHERE ConnectionName=@ConnectionName";
                cnx.Open();
                return cnx.Query<ConnectionConfiguration>(qry, new { ConnectionName = name } ).First();
            }
        }

        /// <summary>
        /// SetConnectionConfiguration method implementation
        /// </summary>
        public bool SetConnectionConfiguration(ConnectionConfiguration cfg, ConnectionConfiguration newcfg)
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string upd = "UPDATE dbo.ConnectionConfiguration SET UserName=@UserName, Password=@Password, Timeout=@Timeout, Secure=@Secure, Maxrows=@Maxrows, ConnectString=@ConnectString WHERE ConnectionName=@ConnectionName";
                string ins = "INSERT INTO dbo.ConnectionConfiguration (ConnectionName, UserName, Password, Timeout, Secure, Maxrows, ConnectString) VALUES (@ConnectionName, @UserName, @Password, @Timeout, @Secure, @Maxrows, @ConnectString)";

                cnx.Open();
                if (cfg != null)  // Update
                {
                    if (cnx.Execute(upd, new { UserName = newcfg.Username, Password = newcfg.Password, Timeout = newcfg.Timeout, Secure = newcfg.Secure, Maxrows = newcfg.Maxrows, ConnectString = newcfg.ConnectString, ConnectionName = cfg.ConnectionName }) >= 1)
                        return true;
                    else  // do insert not probable
                        return (cnx.Execute(ins, new { ConnectionName = newcfg.ConnectionName, UserName = newcfg.Username, Password = newcfg.Password, Timeout = newcfg.Timeout, Secure = newcfg.Secure, Maxrows = newcfg.Maxrows, ConnectString = newcfg.ConnectString }) == 1);
                }
                else // Insert
                {
                    if (cnx.Execute(upd, new { UserName = newcfg.Username, Password = newcfg.Password, Timeout = newcfg.Timeout, Secure = newcfg.Secure, Maxrows = newcfg.Maxrows, ConnectString = newcfg.ConnectString, ConnectionName = newcfg.ConnectionName }) >= 1)
                        return true;
                    else // do insert
                        return (cnx.Execute(ins, new { ConnectionName = newcfg.ConnectionName, UserName = newcfg.Username, Password = newcfg.Password, Timeout = newcfg.Timeout, Secure = newcfg.Secure, Maxrows = newcfg.Maxrows, ConnectString = newcfg.ConnectString }) == 1);
                }
            }
        }

        /// <summary>
        /// DeleteConnectionConfiguration method implementation
        /// </summary>
        public bool DeleteConnectionConfiguration(ConnectionConfiguration cfg)
        {
            if (cfg.ConnectionName.ToLower().Trim().Equals("default"))
                throw new InvalidOperationException("Cannot delete the default connection parameters !");
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string del = "DELETE FROM dbo.ConnectionConfiguration WHERE ConnectionName=@ConnectionName";
                cnx.Open();
                if (cnx.Execute(del, new { ConnectionName = cfg.ConnectionName }) >= 1)
                    return true;
                else
                    return false;
            }
        }

        #endregion

        #region Table DomainConfiguration
        /// <summary>
        /// GetDomainConfigurationList method implementation
        /// </summary>
        public IEnumerable<DomainConfiguration> GetDomainConfigurationList()
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT DnsName, DisplayName, Enabled, Connection, DisplayPosition FROM dbo.DomainConfiguration ORDER BY DisplayPosition";
                cnx.Open();
                return cnx.Query<DomainConfiguration>(qry, null);
            }
        }

        /// <summary>
        /// GetDomainConfiguration method implementation
        /// </summary>
        public DomainConfiguration GetDomainConfiguration(string name)
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT DnsName, DisplayName, Enabled, Connection, DisplayPosition FROM dbo.DomainConfiguration WHERE DisplayName=@DisplayName";
                cnx.Open();
                return cnx.Query<DomainConfiguration>(qry, new { DisplayName = name }).First();
            }
        }

        /// <summary>
        /// SetDomainConfiguration method implementation
        /// </summary>
        public bool SetDomainConfiguration(DomainConfiguration cfg, DomainConfiguration newcfg)
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                if ((cfg!=null) && (string.IsNullOrEmpty(cfg.Connection)))
                    cfg.Connection = "default";
                if ((newcfg != null) && (string.IsNullOrEmpty(newcfg.Connection)))
                    newcfg.Connection = "default";

                string upd = "UPDATE dbo.DomainConfiguration SET DnsName=@DnsName, DisplayName=@DisplayName, Enabled=@Enabled, Connection=@Connection, DisplayPosition=@DisplayPosition WHERE DisplayName=@oldDisplayName";
                string ins = "INSERT INTO dbo.DomainConfiguration (DnsName, DisplayName, Enabled, Connection, DisplayPosition) VALUES (@DnsName, @DisplayName, @Enabled, @Connection, @DisplayPosition)";
                cnx.Open();
                if (cfg != null)  // Update
                {
                    if (cnx.Execute(upd, new { DnsName = newcfg.DnsName, DisplayName = newcfg.DisplayName, Enabled = newcfg.Enabled, Connection = newcfg.Connection, DisplayPosition = newcfg.DisplayPosition, OldDisplayName = cfg.DisplayName }) >= 1)
                        return true;
                    else  // do insert not probable
                        return (cnx.Execute(ins, new { DnsName = cfg.DnsName, DisplayName = cfg.DisplayName, Enabled = cfg.Enabled, Connection = cfg.Connection, DisplayPosition = cfg.DisplayPosition }) == 1);
                }
                else // Insert
                {   // do Update not probable
                    if (cnx.Execute(upd, new { DnsName = newcfg.DnsName, DisplayName = newcfg.DisplayName, Enabled = newcfg.Enabled, Connection = newcfg.Connection, DisplayPosition = newcfg.DisplayPosition, OldDisplayName = newcfg.DisplayName }) >= 1)
                        return true;
                    else // do insert
                        return (cnx.Execute(ins, new { DnsName = newcfg.DnsName, DisplayName = newcfg.DisplayName, Enabled = newcfg.Enabled, Connection = newcfg.Connection, DisplayPosition = newcfg.DisplayPosition }) == 1);
                }
            }
        }

        /// <summary>
        /// DeleteDomainConfiguration method implementation
        /// </summary>
        public bool DeleteDomainConfiguration(DomainConfiguration cfg)
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string del = "DELETE FROM dbo.DomainConfiguration WHERE DisplayName=@DisplayName";
                cnx.Open();
                if (cnx.Execute(del, new { DisplayName = cfg.DisplayName }) >= 1)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// GetFullConfigurations method implementation
        /// </summary>
        public IEnumerable<FullConfiguration> GetFullConfigurations()
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT a.DnsName, a.DisplayName, a.Enabled, a.Connection, a.DisplayPosition, b.UserName, b.Password, b.Timeout, b.Secure, b.Maxrows, b.ConnectString FROM dbo.DomainConfiguration a inner join dbo.ConnectionConfiguration b on a.Connection = b.ConnectionName where a.Enabled=1";
                cnx.Open();
                return cnx.Query<FullConfiguration>(qry, null);
            }
        }
        #endregion

        #region Table GeneralParameters
        /// <summary>
        /// GetGeneralParameters method implementation
        /// </summary>
        public IEnumerable<GeneralParameter> GetGeneralParameters()
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT ParamName, ParamValue FROM dbo.GeneralParameters";
                cnx.Open();
                return cnx.Query<GeneralParameter>(qry, null);
            }
        }

        /// <summary>
        /// GetGeneralParameter method implementation
        /// </summary>
        public string GetGeneralParameter(string paramname)
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT ParamValue FROM dbo.GeneralParameters WHERE ParamName=@ParamName";
                cnx.Open();
                return cnx.Query<string>(qry, new { ParamName = paramname }).First();
            }
        }

        /// <summary>
        /// SetGeneralParameter method implementation
        /// </summary>
        public bool SetGeneralParameter(string paramname, string paramvalue)
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string upd = "UPDATE dbo.GeneralParameters SET ParamValue=@paramvalue WHERE ParamName=@paramname";
                cnx.Open();
                if (cnx.Execute(upd, new { ParamValue = paramvalue, ParamName = paramname }) >= 1)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// GetAccessTocache method implementation
        /// </summary>
        internal bool GetAccessTocache(out bool dotrueload)
        {
            dotrueload = false;
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT Version FROM dbo.CacheData WHERE IsInProcess=@IsInProcess And IsLoaded=@IsLoaded";
                cnx.Open();
                int? tmp = cnx.Query<int?>(qry, new { IsInProcess = true, IsLoaded = false }).Max();
                if (tmp == null)
                {
                    string qry2 = "SELECT Version FROM dbo.CacheData WHERE IsInProcess=@IsInProcess And IsLoaded=@IsLoaded";
                    int? tmp2 = cnx.Query<int?>(qry2, new { IsInProcess = false, IsLoaded = true }).Max();
                    if (tmp2 == null)
                    {
                        string ins = "INSERT INTO dbo.CacheData (MachineName, TimeStamp, Data, IsInprocess, IsLoaded) VALUES (@MachineName, @TimeStamp, @Data, @IsInprocess, @IsLoaded)";
                        if (cnx.Execute(ins, new { MachineName = Environment.MachineName, TimeStamp = DateTime.Now, Data = "TEMPORARY", IsInProcess = true, IsLoaded = false }) == 1)
                        {
                            dotrueload = true;
                            return true;  // get from database
                        }
                        else
                        {
                            dotrueload = false;
                            return false;  // Wait for lock
                        }
                    }
                    else
                    {
                        dotrueload = false;
                        return true;  // get from cache
                    }
                }
                else
                {
                    dotrueload = false;
                    return false;  // Wait for lock
                }
            }
        }

        /// <summary>
        /// SetDataToCache method implementation
        /// </summary>
        internal void SetDataToCache(string data)
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string upd = "UPDATE dbo.CacheData SET Data=@data WHERE IsInprocess=@IsInprocess And IsLoaded=@IsLoaded";
                cnx.Open();
                cnx.Execute(upd, new { Data = data, IsInprocess=true, IsLoaded=false });
            }
        }

        /// <summary>
        /// GetDataFromCache method implementation
        /// </summary>
        internal string GetDataFromCache()
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT Data FROM dbo.CacheData WHERE IsInprocess=@IsInprocess And IsLoaded=@IsLoaded";
                cnx.Open();
                return cnx.Query<string>(qry, new { IsInprocess = false, IsLoaded = true }).First<string>();
            }
        }

        /// <summary>
        /// ClearAccessToCache method implementation
        /// </summary>
        internal void ClearAccessToCache()
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string upd = "UPDATE dbo.CacheData SET IsInprocess=@IsInprocess, IsLoaded=@IsLoaded WHERE IsInprocess=@oldIsInprocess And IsLoaded=@oldIsLoaded";
                cnx.Open();
                cnx.Execute(upd, new { IsInprocess = false, IsLoaded = true, oldIsInprocess = true, oldIsLoaded = false });
            }
        }

        /// <summary>
        /// ResetAccessCache method implementation
        /// </summary>
        internal void ResetAccessCache(double minutes)
        {
            DateTime dt = DateTime.Now.AddMinutes(-minutes);
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string del = "DELETE FROM dbo.CacheData WHERE TimeStamp<@timestamp";
                cnx.Open();
                cnx.Execute(del, new { timestamp = dt });
            }
        }

        /// <summary>
        /// ResetAccessCache method implementation
        /// </summary>
        internal void ZapCache()
        {
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string del = "DELETE FROM dbo.CacheData";
                cnx.Open();
                cnx.Execute(del, null);
            }
        }
        #endregion

        #region Table GlobalParameters
        /// <summary>
        /// GetGlobalParameterList method implementation
        /// </summary>
        public IEnumerable<GlobalParameter> GetGlobalParameterList()
        {
            List<GeneralParameter> dbres = null;
            List<GlobalParameter> lsres = new List<GlobalParameter>();
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string qry = "SELECT ParamName, ParamValue FROM dbo.GeneralParameters";
                cnx.Open();
                dbres = cnx.Query<GeneralParameter>(qry, null).ToList();
            }
            GlobalParameter glb = new GlobalParameter();
            foreach (GeneralParameter p in dbres)
            {
                if (p.ParamName.ToLower().Equals("cacheduration"))
                    glb.CacheDuration = Convert.ToInt32(p.ParamValue);
                else if (p.ParamName.ToLower().Equals("claimdisplaymode"))
                    glb.ClaimsDisplayMode = (ProxyClaimsDisplayMode)Enum.Parse(typeof(ProxyClaimsDisplayMode), p.ParamValue, true);
                else if (p.ParamName.ToLower().Equals("claimdisplayname"))
                    glb.ClaimDisplayName = p.ParamValue;
                else if (p.ParamName.ToLower().Equals("claimidentitymode"))
                    glb.ClaimIdentityMode = (ProxyClaimsIdentityMode)Enum.Parse(typeof(ProxyClaimsIdentityMode), p.ParamValue, true);
                else if (p.ParamName.ToLower().Equals("claimidentity"))
                    glb.ClaimIdentity = p.ParamValue;
                else if (p.ParamName.ToLower().Equals("claimprovidername"))
                    glb.ClaimProviderName = p.ParamValue;
                else if (p.ParamName.ToLower().Equals("claimrolemode"))
                    glb.ClaimRoleMode = (ProxyClaimsRoleMode)Enum.Parse(typeof(ProxyClaimsRoleMode), p.ParamValue, true);
                else if (p.ParamName.ToLower().Equals("claimrole"))
                    glb.ClaimRole = p.ParamValue;
                else if (p.ParamName.ToLower().Equals("claimsmode"))
                    glb.ClaimsMode = (ProxyClaimsMode)Enum.Parse(typeof(ProxyClaimsMode), p.ParamValue, true);
                else if (p.ParamName.ToLower().Equals("peoplepickerdisplaymode"))
                    glb.PeoplePickerDisplayMode = (ProxyClaimsDisplayMode)Enum.Parse(typeof(ProxyClaimsDisplayMode), p.ParamValue, true);
                else if (p.ParamName.ToLower().Equals("peoplepickerimages"))
                    glb.PeoplePickerImages = bool.Parse(p.ParamValue);
                else if (p.ParamName.ToLower().Equals("searchbydisplayname"))
                    glb.SearchByDisplayName = bool.Parse(p.ParamValue);
                else if (p.ParamName.ToLower().Equals("searchbymail"))
                    glb.SearchByMail = bool.Parse(p.ParamValue);
                else if (p.ParamName.ToLower().Equals("showsystemnodes"))
                    glb.ShowSystemNodes = bool.Parse(p.ParamValue);
                else if (p.ParamName.ToLower().Equals("smoothrequestor"))
                    glb.SmoothRequestor = (ProxySmoothRequest)Enum.Parse(typeof(ProxySmoothRequest), p.ParamValue, true);
                else if (p.ParamName.ToLower().Equals("trustedloginprovidername"))
                    glb.TrustedLoginProviderName = p.ParamValue;
            }
            lsres.Add(glb); // after flattening return only one row
            return lsres;
        }

        /// <summary>
        /// SetGlobalParameter method implementation
        /// </summary>
        public bool SetGlobalParameter(GlobalParameter prms, GlobalParameter newprms)
        {
            bool result = true;
            using (var cnx = new SqlConnection(ConnectString()))
            {
                string upd = "UPDATE dbo.GeneralParameters SET ParamValue=@paramvalue WHERE ParamName=@paramname";
                cnx.Open();
                SqlTransaction tr = cnx.BeginTransaction();
                try
                {
                    if (cnx.Execute(upd, new { ParamValue = newprms.CacheDuration.ToString(), ParamName = "CacheDuration" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.ClaimDisplayName.ToString(), ParamName = "ClaimDisplayName" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.ClaimIdentityMode.ToString(), ParamName = "ClaimIdentityMode" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.ClaimIdentity.ToString(), ParamName = "ClaimIdentity" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.ClaimProviderName.ToString(), ParamName = "ClaimProviderName" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.ClaimRoleMode.ToString(), ParamName = "ClaimRoleMode" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.ClaimRole.ToString(), ParamName = "ClaimRole" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.ClaimsDisplayMode.ToString(), ParamName = "ClaimDisplayMode" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.ClaimsMode.ToString(), ParamName = "ClaimsMode" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.PeoplePickerDisplayMode.ToString(), ParamName = "PeoplePickerDisplayMode" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.PeoplePickerImages.ToString(), ParamName = "PeoplePickerImages" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.SearchByDisplayName.ToString(), ParamName = "SearchByDisplayName" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.SearchByMail.ToString(), ParamName = "SearchByMail" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.ShowSystemNodes.ToString(), ParamName = "ShowSystemNodes" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.SmoothRequestor.ToString(), ParamName = "SmoothRequestor" }, tr) < 1)
                        result = false;
                    if (cnx.Execute(upd, new { ParamValue = newprms.TrustedLoginProviderName.ToString(), ParamName = "TrustedLoginProviderName" }, tr) < 1)
                        result = false;
                }
                catch (Exception e)
                {
                    tr.Rollback();
                    throw e;
                }
                if (result)
                    tr.Commit();
                else
                    tr.Rollback();
            }
            return result;
        }


        #endregion
    }

    #region Database mappings Classes
    /// <summary>
    /// GlobalParameter class
    /// </summary>
    public class GlobalParameter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GlobalParameter()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GlobalParameter(int cacheduration, ProxyClaimsDisplayMode claimsdisplaymode, string claimdisplayname, ProxyClaimsIdentityMode claimidentitymode, string claimidentity, string claimprovidername, 
                               ProxyClaimsRoleMode claimrolemode, string claimrole, ProxyClaimsMode claimsmode, ProxyClaimsDisplayMode peoplepickerdisplaymode, bool peoplepickerimages, bool searchbydisplayname,
                               bool searchbymail, bool showsystemnodes, ProxySmoothRequest smoothrequestor, string trustedloginprovidername)
        {
            this.CacheDuration = cacheduration;
            this.ClaimsDisplayMode = claimsdisplaymode;
            this.ClaimDisplayName = claimdisplayname;
            this.ClaimIdentityMode = claimidentitymode;
            this.ClaimIdentity = claimidentity;
            this.ClaimProviderName = claimprovidername;
            this.ClaimRoleMode = claimrolemode;
            this.ClaimRole = claimrole;
            this.ClaimsMode = claimsmode;
            this.PeoplePickerDisplayMode = peoplepickerdisplaymode;
            this.PeoplePickerImages = peoplepickerimages;
            this.SearchByDisplayName = searchbydisplayname;
            this.SearchByMail = searchbymail;
            this.ShowSystemNodes = showsystemnodes; 
            this.SmoothRequestor = smoothrequestor;
            this.TrustedLoginProviderName = trustedloginprovidername;
        }

        public int CacheDuration { get; set; }
        public ProxyClaimsDisplayMode ClaimsDisplayMode { get; set; }
        public string ClaimDisplayName { get; set; }
        public ProxyClaimsIdentityMode ClaimIdentityMode { get; set; }
        public string ClaimIdentity { get; set; }
        public string ClaimProviderName { get; set; }
        public ProxyClaimsRoleMode ClaimRoleMode { get; set; }
        public string ClaimRole { get; set; }
        public ProxyClaimsMode ClaimsMode { get; set; }
        public ProxyClaimsDisplayMode PeoplePickerDisplayMode { get; set; }
        public bool PeoplePickerImages { get; set; }
        public bool SearchByDisplayName { get; set; }
        public bool SearchByMail { get; set; }
        public bool ShowSystemNodes { get; set; }
        public ProxySmoothRequest SmoothRequestor { get; set; }
        public string TrustedLoginProviderName { get; set; }
    }

    /// <summary>
    /// AssemblyConfiguration Class
    /// </summary>
    public class AssemblyConfiguration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AssemblyConfiguration()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AssemblyConfiguration(string assemblydesc, string typedesc, bool selected = false, bool trace = false, bool augment = false)
        {
            this.AssemblyFulldescription = assemblydesc;
            this.AssemblyTypeDescription = typedesc;
            this.Selected = selected;
            this.TraceResolve = trace;
            this.ClaimsExt = augment;
        }

        public string AssemblyFulldescription { get; set; }
        public string AssemblyTypeDescription { get; set; }
        public bool Selected { get; set; }
        public bool TraceResolve { get; set; }
        public bool ClaimsExt { get; set; }
    }

    /// <summary>
    /// FullConfiguration Class
    /// </summary>
    public class FullConfiguration
    {
        public string DisplayName { get; set; }
        public string DnsName { get; set; }
        public bool Enabled { get; set; }
        public string Connection { get; set; }
        public int DisplayPosition { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Int16 Timeout { get; set; }
        public bool Secure { get; set; }
        public int Maxrows { get; set; }
        public string ConnectString { get; set; }
    }

    /// <summary>
    /// ConnectionConfiguration Class
    /// </summary>
    public class ConnectionConfiguration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectionConfiguration()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectionConfiguration(string connectionname, string username, string password, Int16 timeout, bool secure, int maxrows, string connectstring)
        {
            this.ConnectionName = connectionname;
            this.Username = username;
            this.Password = password;
            this.Timeout = timeout;
            this.Secure = secure;
            this.Maxrows = maxrows;
            this.ConnectString = connectstring;
        }

        public string ConnectionName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Int16 Timeout { get; set; }
        public bool Secure { get; set; }
        public int Maxrows { get; set; }
        public string ConnectString { get; set; }
    }

    /// <summary>
    /// DomainConfiguration Class
    /// </summary>
    public class DomainConfiguration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DomainConfiguration()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DomainConfiguration(string displayname, string dnsname, string connection, bool enabled, int position)
        {
            this.DisplayName = displayname;
            this.DnsName = dnsname;
            this.Enabled = enabled;
            this.Connection = connection;
            this.DisplayPosition = position;
        }

        public string DisplayName { get; set; }
        public string DnsName { get; set; }
        public bool Enabled { get; set; }
        public string Connection { get; set; }
        public int DisplayPosition { get; set; }
    }

    /// <summary>
    /// GeneralParameter class
    /// </summary>
    public class GeneralParameter
    {
        public string ParamName { get; set; }
        public string ParamValue { get; set; }
    }
    #endregion
}