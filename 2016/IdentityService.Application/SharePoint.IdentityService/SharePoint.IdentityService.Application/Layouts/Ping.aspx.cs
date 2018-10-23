using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace SharePoint.IdentityService.AdminLayoutPages
{
    public partial class PingPage : UnsecuredLayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override bool AllowAnonymousAccess 
        { 
            get 
            {
                return true;
            }
        }
    }
}
