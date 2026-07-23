//Created by TungDPL
//Create at 7/21/2026
//Last update: 7/21/2026
using System.Collections.Generic;
using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.ViewModels.Admin
{
    public class UserManagementViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public UserStatsViewModel Stats { get; set; } = new UserStatsViewModel();
    }
}
