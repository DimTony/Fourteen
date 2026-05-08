using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Application.Configurations
{
    public static class FeatureFlags
    {
        public const string ClassifyName = "Features:ClassifyName";
        public const string CreateProfile = "Features:CreateProfile";
        public const string GetProfileById = "Features:GetProfileById";
        public const string GetAllProfiles = "Features:GetAllProfiles";
        public const string GetDashboardStats = "Features:GetDashboardStats";
        public const string DeleteProfile = "Features:DeleteProfile";
        public const string GetProfiles = "Features:GetProfiles";
        public const string SearchProfiles = "Features:SearchProfiles";
        public const string GoogleAuth = "Features:GoogleAuth";
        public const string UpdateUser = "Features:UpdateUser";
        public const string RegisterUser = "Features:RegisterUser";
        public const string LoginUser = "Features:LoginUser";
        public const string GetDomains = "Features:GetDomains";
        public const string GetDomainById = "Features:GetDomainById";
        public const string DeleteDomain = "Features:DeleteDomain";        
        public const string VerifyDomain = "Features:VerifyDomain";    
        public const string StartScan = "Features:StartScan";    
        public const string ProcessScanResult = "Features:ProcessScanResult";    
            
        public const string AddDomain = "Features:AddDomain";
    }
}
