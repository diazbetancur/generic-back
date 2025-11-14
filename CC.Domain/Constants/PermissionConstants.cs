namespace CC.Domain.Constants
{
    /// <summary>
    /// Constantes de permisos del sistema
    /// </summary>
    public static class PermissionConstants
    {
        /// <summary>
        /// Módulo de Solicitudes (Requests)
        /// </summary>
        public static class Requests
        {
            public const string Module = "Requests";
            public const string View = "Requests.View";
            public const string Create = "Requests.Create";
            public const string Update = "Requests.Update";
            public const string Delete = "Requests.Delete";
            public const string Assign = "Requests.Assign";
            public const string ChangeState = "Requests.ChangeState";
        }

        /// <summary>
        /// Módulo de Usuarios (Users)
        /// </summary>
        public static class Users
        {
            public const string Module = "Users";
            public const string View = "Users.View";
            public const string Create = "Users.Create";
            public const string Update = "Users.Update";
            public const string Delete = "Users.Delete";
            public const string AssignRoles = "Users.AssignRoles";
        }

        /// <summary>
        /// Módulo de Roles (Roles)
        /// </summary>
        public static class Roles
        {
            public const string Module = "Roles";
            public const string View = "Roles.View";
            public const string Create = "Roles.Create";
            public const string Update = "Roles.Update";
            public const string Delete = "Roles.Delete";
            public const string ManagePermissions = "Roles.ManagePermissions";
        }

        /// <summary>
        /// Módulo de Reportes (Reports)
        /// </summary>
        public static class Reports
        {
            public const string Module = "Reports";
            public const string View = "Reports.View";
            public const string Export = "Reports.Export";
            public const string ViewAll = "Reports.ViewAll";
        }

        /// <summary>
        /// Módulo de NilRead (Imágenes Diagnósticas)
        /// </summary>
        public static class NilRead
        {
            public const string Module = "NilRead";
            public const string ViewExams = "NilRead.ViewExams";
            public const string ViewReports = "NilRead.ViewReports";
            public const string ViewImages = "NilRead.ViewImages";
        }

        /// <summary>
        /// Módulo de Configuración (Configuration)
        /// </summary>
        public static class Configuration
        {
            public const string Module = "Configuration";
            public const string View = "Config.View";
            public const string Update = "Config.Update";
            public const string ViewAuditLog = "Config.ViewAuditLog";
        }

        /// <summary>
        /// Módulo de Contenido (Content Management)
        /// </summary>
        public static class Content
        {
            public const string Module = "Content";
            public const string View = "Content.View";
            public const string Create = "Content.Create";
            public const string Update = "Content.Update";
            public const string Delete = "Content.Delete";
            public const string Publish = "Content.Publish";
        }

        /// <summary>
        /// Módulo de Estados (States)
        /// </summary>
        public static class States
        {
            public const string Module = "States";
            public const string View = "States.View";
            public const string Manage = "States.Manage";
        }

        /// <summary>
        /// Módulo de Telemetría (Telemetry)
        /// </summary>
        public static class Telemetry
        {
            public const string Module = "Telemetry";
            public const string View = "Telemetry.View";
            public const string ViewAll = "Telemetry.ViewAll";
            public const string Export = "Telemetry.Export";
        }

        /// <summary>
        /// Nombres de políticas de autorización
        /// </summary>
        public static class Policies
        {
            // Políticas por tipo de usuario
            public const string AdminOnly = "AdminOnly";
            public const string PatientOnly = "PatientOnly";

            // Políticas de Requests
            public const string CanViewRequests = "CanViewRequests";
            public const string CanCreateRequests = "CanCreateRequests";
            public const string CanUpdateRequests = "CanUpdateRequests";
            public const string CanDeleteRequests = "CanDeleteRequests";
            public const string CanAssignRequests = "CanAssignRequests";
            public const string CanChangeRequestState = "CanChangeRequestState";

            // Políticas de Users
            public const string CanViewUsers = "CanViewUsers";
            public const string CanManageUsers = "CanManageUsers";
            public const string CanAssignRoles = "CanAssignRoles";

            // Políticas de Roles
            public const string CanViewRoles = "CanViewRoles";
            public const string CanManageRoles = "CanManageRoles";
            public const string CanManagePermissions = "CanManagePermissions";

            // Políticas de Reports
            public const string CanViewReports = "CanViewReports";
            public const string CanExportReports = "CanExportReports";
            public const string CanViewAllReports = "CanViewAllReports";

            // Políticas de NilRead
            public const string CanViewExams = "CanViewExams";
            public const string CanViewMedicalReports = "CanViewMedicalReports";
            public const string CanViewMedicalImages = "CanViewMedicalImages";

            // Políticas de Configuration
            public const string CanViewConfig = "CanViewConfig";
            public const string CanUpdateConfig = "CanUpdateConfig";
            public const string CanViewAuditLog = "CanViewAuditLog";

            // Políticas de Content
            public const string CanViewContent = "CanViewContent";
            public const string CanManageContent = "CanManageContent";
            public const string CanPublishContent = "CanPublishContent";

            // Políticas de States
            public const string CanViewStates = "CanViewStates";
            public const string CanManageStates = "CanManageStates";

            // Políticas de Telemetry
            public const string CanViewTelemetry = "CanViewTelemetry";
            public const string CanViewAllTelemetry = "CanViewAllTelemetry";
            public const string CanExportTelemetry = "CanExportTelemetry";
        }

        /// <summary>
        /// Obtiene todos los permisos del sistema
        /// </summary>
        /// <returns>Array de todos los permisos</returns>
        public static string[] GetAllPermissions()
        {
            return new[]
            {
                // Requests
                Requests.View, Requests.Create, Requests.Update, Requests.Delete, Requests.Assign, Requests.ChangeState,
                // Users
                Users.View, Users.Create, Users.Update, Users.Delete, Users.AssignRoles,
                // Roles
                Roles.View, Roles.Create, Roles.Update, Roles.Delete, Roles.ManagePermissions,
                // Reports
                Reports.View, Reports.Export, Reports.ViewAll,
                // NilRead
                NilRead.ViewExams, NilRead.ViewReports, NilRead.ViewImages,
                // Configuration
                Configuration.View, Configuration.Update, Configuration.ViewAuditLog,
                // Content
                Content.View, Content.Create, Content.Update, Content.Delete, Content.Publish,
                // States
                States.View, States.Manage,
                // Telemetry
                Telemetry.View, Telemetry.ViewAll, Telemetry.Export
            };
        }

        /// <summary>
        /// Obtiene los permisos por módulo
        /// </summary>
        /// <param name="module">Nombre del módulo</param>
        /// <returns>Array de permisos del módulo</returns>
        public static string[] GetPermissionsByModule(string module)
        {
            return module switch
            {
                "Requests" => new[] { Requests.View, Requests.Create, Requests.Update, Requests.Delete, Requests.Assign, Requests.ChangeState },
                "Users" => new[] { Users.View, Users.Create, Users.Update, Users.Delete, Users.AssignRoles },
                "Roles" => new[] { Roles.View, Roles.Create, Roles.Update, Roles.Delete, Roles.ManagePermissions },
                "Reports" => new[] { Reports.View, Reports.Export, Reports.ViewAll },
                "NilRead" => new[] { NilRead.ViewExams, NilRead.ViewReports, NilRead.ViewImages },
                "Configuration" => new[] { Configuration.View, Configuration.Update, Configuration.ViewAuditLog },
                "Content" => new[] { Content.View, Content.Create, Content.Update, Content.Delete, Content.Publish },
                "States" => new[] { States.View, States.Manage },
                "Telemetry" => new[] { Telemetry.View, Telemetry.ViewAll, Telemetry.Export },
                _ => Array.Empty<string>()
            };
        }
    }
}
