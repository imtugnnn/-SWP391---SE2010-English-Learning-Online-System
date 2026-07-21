using System;

namespace EnglishLearningOnlineSystem.ViewModels.Admin
{
    public class UserStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int StudentCount { get; set; }
        public int TeacherCount { get; set; }
        public int ParentCount { get; set; }
        public int ContentManagerCount { get; set; }

        public int NewThisMonth { get; set; }
        public int NewLastMonth { get; set; }

        public int ActiveThisMonth { get; set; }
        public int ActiveLastMonth { get; set; }

        public int StudentsThisMonth { get; set; }
        public int StudentsLastMonth { get; set; }

        public int TeachersThisMonth { get; set; }
        public int TeachersLastMonth { get; set; }

        public int ParentsThisMonth { get; set; }
        public int ParentsLastMonth { get; set; }

        public int ContentThisMonth { get; set; }
        public int ContentLastMonth { get; set; }
    }
}
