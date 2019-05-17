//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //
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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Administration.Backup;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace SharePoint.IdentityService
{
    [Guid("948E1B2F-9002-404C-852F-656893CC391F")]
    public class IdentityServiceApplicationJobDefinition : SPServiceJobDefinition
    {
        [Persisted] private string _data;
        [Persisted] private Guid _serviceApplicationId;

        protected SPJobState JobState { get; private set; }
        private SPMinuteSchedule _defaultschedule;
             
        /// <summary>
        /// Constructor
        /// </summary>
        public IdentityServiceApplicationJobDefinition()
        {
            _defaultschedule = new SPMinuteSchedule();
            _defaultschedule.Interval = 2;
            _defaultschedule.BeginSecond = 1;
            _defaultschedule.EndSecond = 59;
            this.Schedule = _defaultschedule;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public IdentityServiceApplicationJobDefinition(IdentityServiceApplication serviceApplication, string name): base(GenerateJobName(serviceApplication, name), serviceApplication.Service)
        {
            ArgumentValidator.IsNotNull(serviceApplication, "IdentityServiceApplication");
            _serviceApplicationId = serviceApplication.Id;
            _defaultschedule = new SPMinuteSchedule();
            _defaultschedule.Interval = 2;
            _defaultschedule.BeginSecond = 1;
            _defaultschedule.EndSecond = 59;
            this.Schedule = _defaultschedule;

        }

        /// <summary>
        /// ServiceApplicationId property implementation
        /// </summary>
        public Guid ServiceApplicationId
        {
            get { return _serviceApplicationId; }
            private set { _serviceApplicationId = value; }
        }

        /// <summary>
        /// ServiceApplication property implementation
        /// </summary>
        public IdentityServiceApplication ServiceApplication
        {
            get
            {
                IdentityServiceApplication app = Utilities.GetApplicationById(_serviceApplicationId);
                if (app == null)
                {
                    throw new InvalidOperationException();
                }
                return app;
            }
        }

        /// <summary>
        /// Description property
        /// </summary>
        public override sealed string Description
        {
            get { return JobDescription; }
        }

        /// <summary>
        /// DisplayName property
        /// </summary>
        public override sealed string DisplayName
        {
            get
            {
                if (ServiceApplication == null)
                    return string.Format("{0} : {1}", JobDisplayName, this._serviceApplicationId);
                else
                    return string.Format("{0} : {1}", JobDisplayName, ServiceApplication.Name);
            }
        }

        /// <summary>
        /// EnableBackup property
        /// </summary>
        public override bool EnableBackup
        {
            get { return true; }
        }

        /// <summary>
        /// Types property
        /// </summary>
        internal static IEnumerable<Type> Types
        {
            get { return new Type[] {typeof(IdentityServiceApplicationJobDefinition)}; }
        }

        /// <summary>
        /// Data property
        /// </summary>
        protected string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// DefaultSchedule 
        /// </summary>
        public SPSchedule DefaultSchedule
        {
            get 
            {
                if (_defaultschedule == null)
                {
                    _defaultschedule = new SPMinuteSchedule();
                    _defaultschedule.Interval = 2;
                    _defaultschedule.BeginSecond = 1;
                    _defaultschedule.EndSecond = 59;
                }
                return _defaultschedule; 
            }
        }

        /// <summary>
        /// JobDescription property
        /// </summary>
        public string JobDescription
        {
                    get { return ResourcesValues.GetString("E20101"); }
        }

        /// <summary>
        /// JobDisplayName property
        /// </summary>
        public string JobDisplayName
        {
            get { return "SharePoint Identity Service Application Job "; }
        }


        /// <summary>
        /// Execute method implementation
        /// </summary>
        public override void Execute(SPJobState jobState)
        {
            ArgumentValidator.IsNotNull(jobState, "JobState");
            JobState = jobState;
            if (ServiceApplication.Status == SPObjectStatus.Online)
            {
                if (!jobState.ShouldStop)
                    DoExecute();
            }
        }

        /// <summary>
        /// Execute method implementation
        /// </summary>
        public void DoExecute()
        {
            try
            {
                SPFarm farm = SPFarm.Local;
                IdentityServiceProxy serviceProxy = farm.ServiceProxies.GetValue<IdentityServiceProxy>();
                if (null != serviceProxy)
                {
                    foreach(SPServiceApplicationProxy prxy in serviceProxy.ApplicationProxies)
                    {
                        if (prxy is IdentityServiceApplicationProxy)
                        {
                            if (CheckApplicationProxy(ServiceApplication, prxy as IdentityServiceApplicationProxy))
                            {
                                ((IdentityServiceApplicationProxy)prxy).LaunchStartCommand(Environment.MachineName);
                            }
                        }
                    }
                }
            }
            catch 
            {
                // Do Nothing
            }
        }

        /// <summary>
        /// CheckApplicationProxy metho implementation
        /// </summary>
        private bool CheckApplicationProxy(IdentityServiceApplication app, IdentityServiceApplicationProxy prxy)
        {
            bool result = false;
            try
            {
                string path = app.IisVirtualDirectoryPath;
                string[] xpath = path.Split('\\');
                result = (prxy.ServiceEndpointUri.ToString().ToLower().Contains(xpath[1]));
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// OnPostRestore method implementation
        /// </summary>
        public override bool OnPostRestore(object sender, SPRestoreInformation info)
        {
            ArgumentValidator.IsNotNull(info, "RestoreInformation");
            info.ChangePersistedObjectParentId(Utilities.GetAdminService(true).Id);
            Update(true);
            info.CurrentProgress = 100;
            return true;
        }

        /// <summary>
        /// GenerateJobName method implementation
        /// </summary>
        private static string GenerateJobName(IdentityServiceApplication app, string jobName)
        {
            ArgumentValidator.IsNotNull(app, "IdentityServiceApplication");
            ArgumentValidator.IsNotEmpty(jobName, "JobName");
            return (app.Name + "_" + jobName);
        }
    }

    [Guid("948E1B2F-9002-404C-852F-656893CC392F")]
    public class IdentityServiceApplicationReloadJobDefinition : SPServiceJobDefinition
    {
        [Persisted]
        private string _data;
        [Persisted]
        private Guid _serviceApplicationId;

        protected SPJobState JobState { get; private set; }
        private SPDailySchedule _defaultschedule;

        /// <summary>
        /// Constructor
        /// </summary>
        public IdentityServiceApplicationReloadJobDefinition()
        {
            _defaultschedule = new SPDailySchedule();
            _defaultschedule.BeginHour = 23;
            _defaultschedule.EndHour = 23;
            _defaultschedule.BeginMinute = 0;
            _defaultschedule.EndMinute = 5;
            _defaultschedule.BeginSecond = 1;
            _defaultschedule.EndSecond = 59;
            this.Schedule = _defaultschedule;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public IdentityServiceApplicationReloadJobDefinition(IdentityServiceApplication serviceApplication, string name): base(GenerateJobName(serviceApplication, name), serviceApplication.Service)
        {
            ArgumentValidator.IsNotNull(serviceApplication, "IdentityServiceApplication");
            _serviceApplicationId = serviceApplication.Id;
            _defaultschedule = new SPDailySchedule();
            _defaultschedule.BeginHour = 23;
            _defaultschedule.EndHour = 23;
            _defaultschedule.BeginMinute = 0;
            _defaultschedule.EndMinute = 5;
            _defaultschedule.BeginSecond = 1;
            _defaultschedule.EndSecond = 59;
            this.Schedule = _defaultschedule;
        }

        /// <summary>
        /// ServiceApplicationId property implmentation
        /// </summary>
        public Guid ServiceApplicationId
        {
            get { return _serviceApplicationId; }
            private set { _serviceApplicationId = value; }
        }

        /// <summary>
        /// ServiceApplication property implmentation
        /// </summary>
        public IdentityServiceApplication ServiceApplication
        {
            get
            {
                IdentityServiceApplication app = Utilities.GetApplicationById(_serviceApplicationId);
                if (app == null)
                {
                    throw new InvalidOperationException();
                }
                return app;
            }
        }

        /// <summary>
        /// Description property
        /// </summary>
        public override sealed string Description
        {
            get { return JobDescription; }
        }

        /// <summary>
        /// DisplayName property
        /// </summary>
        public override sealed string DisplayName
        {
            get 
            { 
                if (ServiceApplication==null)
                    return string.Format("{0} : {1}", JobDisplayName, this._serviceApplicationId); 
                else
                    return string.Format("{0} : {1}", JobDisplayName, ServiceApplication.Name); 
            }
        }

        /// <summary>
        /// EnableBackup property
        /// </summary>
        public override bool EnableBackup
        {
            get { return true; }
        }

        /// <summary>
        /// Types property
        /// </summary>
        internal static IEnumerable<Type> Types
        {
            get { return new Type[] { typeof(IdentityServiceApplicationReloadJobDefinition) }; }
        }

        /// <summary>
        /// Data property
        /// </summary>
        protected string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// DefaultSchedule 
        /// </summary>
        public SPSchedule DefaultSchedule
        {
            get
            {
                if (_defaultschedule == null)
                {
                    _defaultschedule = new SPDailySchedule();
                    _defaultschedule.BeginHour = 23;
                    _defaultschedule.EndHour = 23;
                    _defaultschedule.BeginMinute = 0;
                    _defaultschedule.EndMinute = 5;
                    _defaultschedule.BeginSecond = 1;
                    _defaultschedule.EndSecond = 59;
                }
                return _defaultschedule;
            }
        }

        /// <summary>
        /// JobDescription property
        /// </summary>
        public string JobDescription
        {
            get { return ResourcesValues.GetString("E20102"); }
        }

        /// <summary>
        /// JobDisplayName property
        /// </summary>
        public string JobDisplayName
        {
            get { return "SharePoint Identity Service Application Reload Job "; }
        }


        /// <summary>
        /// Execute method implementation
        /// </summary>
        public override void Execute(SPJobState jobState)
        {
            ArgumentValidator.IsNotNull(jobState, "JobState");
            JobState = jobState;
            if (ServiceApplication.Status == SPObjectStatus.Online)
            {
                if (!jobState.ShouldStop)
                    DoExecute();
            }
        }

        /// <summary>
        /// Execute method implementation
        /// </summary>
        public void DoExecute()
        {
            try
            {
                SPFarm farm = SPFarm.Local;
                IdentityServiceProxy serviceProxy = farm.ServiceProxies.GetValue<IdentityServiceProxy>();
                if (null != serviceProxy)
                {
                    foreach (SPServiceApplicationProxy prxy in serviceProxy.ApplicationProxies)
                    {
                        if (prxy is IdentityServiceApplicationProxy)
                        {
                            if (CheckApplicationProxy(ServiceApplication, prxy as IdentityServiceApplicationProxy))
                            {
                                ((IdentityServiceApplicationProxy)prxy).LaunchReloadCommand(Environment.MachineName);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Do Nothing
            }
        }

        /// <summary>
        /// CheckApplicationProxy metho implementation
        /// </summary>
        private bool CheckApplicationProxy(IdentityServiceApplication app, IdentityServiceApplicationProxy prxy)
        {
            bool result = false;
            try
            {
                string path = app.IisVirtualDirectoryPath;
                string[] xpath = path.Split('\\');
                result = (prxy.ServiceEndpointUri.ToString().ToLower().Contains(xpath[1]));
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// OnPostRestore method implementation
        /// </summary>
        public override bool OnPostRestore(object sender, SPRestoreInformation info)
        {
            ArgumentValidator.IsNotNull(info, "RestoreInformation");
            info.ChangePersistedObjectParentId(Utilities.GetAdminService(true).Id);
            Update(true);
            info.CurrentProgress = 100;
            return true;
        }

        /// <summary>
        /// GenerateJobName method implementation
        /// </summary>
        private static string GenerateJobName(IdentityServiceApplication app, string jobName)
        {
            ArgumentValidator.IsNotNull(app, "IdentityServiceApplication");
            ArgumentValidator.IsNotEmpty(jobName, "JobName");
            return (app.Name + "_" + jobName);
        }
    }

}