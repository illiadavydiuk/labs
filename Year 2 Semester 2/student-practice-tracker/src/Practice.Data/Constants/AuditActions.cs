namespace Practice.Data.Constants
{
    public static class AuditActions
    {// === АВТОРИЗАЦІЯ ===
        public const string LoginSuccess = "Auth.Login.Success";
        public const string LoginFailed = "Auth.Login.Failed";
        public const string Logout = "Auth.Logout";

        // === СТУДЕНТ ===
        public const string TopicSelected = "Student.Topic.Selected";
        public const string ReportSubmitted = "Student.Report.Submitted";
        public const string ReportUpdated = "Student.Report.Updated";

        // === ВИКЛАДАЧ ===
        public const string AssessmentUpdated = "Supervisor.Assessment.Updated"; // Оцінка/Статус

        // === АДМІНІСТРАТОР ===
        public const string UserCreated = "Admin.User.Created";
        public const string UserUpdated = "Admin.User.Updated";
        public const string UserDeleted = "Admin.User.Deleted";

        public const string CourseCreated = "Admin.Course.Created";
        public const string CourseUpdated = "Admin.Course.Updated";
        public const string CourseDeleted = "Admin.Course.Deleted";

        // === ЗАГАЛЬНЕ ===
        public const string SystemError = "System.Error";
    }
}