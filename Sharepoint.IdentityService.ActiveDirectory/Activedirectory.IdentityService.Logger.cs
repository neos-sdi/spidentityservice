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
using System;
using System.Diagnostics;
using Microsoft.SharePoint;
using System.Threading;
using Microsoft.SharePoint.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace SharePoint.IdentityService.ActiveDirectory
{
	public static class LogEvent 
    {
        const string _eventlogsource = "ActiveDirectory Identity Service";

        /// <summary>
        /// Constructor
        /// </summary>
        static LogEvent()
        {
			try 
            {
                // using (Identity impersonate = Identity.ImpersonateAdmin()) 
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
					if (!EventLog.SourceExists(_eventlogsource))
                        System.Diagnostics.EventLog.CreateEventSource(_eventlogsource, "Application");
				}
                );
			}
			catch 
            {
			}
		}

        /// <summary>
        /// Log method implementation
        /// </summary>
		public static void Log(Exception ex, string message, EventLogEntryType eventLogEntryType, int eventid = 0 ) 
        {
			try 
            {
                // using (Identity impersonate = Identity.ImpersonateAdmin()) 
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    EventLog evtL = new EventLog("Application");
                    evtL.Source = _eventlogsource;

                    string contents = String.Format("{0}\r\n{1}\r\n{2}", message, ex.Message, ex.StackTrace);
                    while ((ex = ex.InnerException) != null)
                    {
                        contents = String.Format("{3}\r\n\r\n{0}\r\n{1}\r\n{2}", message, ex.Message, ex.StackTrace, contents);
                    }
                    evtL.WriteEntry(contents, eventLogEntryType, eventid);
                }
                );
			}
			catch 
            {
			}
		}

        /// <summary>
        /// Trace method implementation
        /// </summary>
        public static void Trace(string message, EventLogEntryType eventLogEntryType, int eventid = 0)
        {
            try
            {
               //using (Identity impersonate = Identity.ImpersonateAdmin()) 
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    EventLog evtL = new EventLog("Application");
                    evtL.Source = _eventlogsource;
                    string contents = String.Format("{0}", message);
                    evtL.WriteEntry(contents, eventLogEntryType, eventid);
                }
                );
            }
            catch
            {
            }
        }
	}

    internal static class ResourcesValues
    {
        internal const string resfilename = "SharePoint.IdentityService.ActiveDirectory";
        internal static uint cultureid = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        static ResourcesValues()
        {
            cultureid = Convert.ToUInt32(System.Globalization.CultureInfo.InstalledUICulture.LCID);
        }

        /// <summary>
        /// GetString method implementation
        /// </summary>
        public static string GetString(string value)
        {
            return SPUtility.GetLocalizedString("$Resources:" + value, resfilename, cultureid);
           // return SPUtility.GetLocalizedString("$Resources:" + value, resfilename, Convert.ToUInt32(Thread.CurrentThread.CurrentCulture.LCID));
        }

        /// <summary>
        /// GetString method implementation
        /// </summary>
        public static string GetUIString(string value)
        {
            return SPUtility.GetLocalizedString("$Resources:" + value, resfilename, Convert.ToUInt32(Thread.CurrentThread.CurrentUICulture.LCID));
        }
    }
}
