namespace CC.Domain.Constants
{
    /// <summary>
    /// System permission constants
    /// </summary>
    public static class PermissionConstants
    {
        /// <summary>
        /// Users Module
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
        /// Roles Module
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
        /// Configuration Module
        /// </summary>
        public static class Configuration
        {
            public const string Module = "Configuration";
            public const string View = "Config.View";
            public const string Update = "Config.Update";
            public const string ViewAuditLog = "Config.ViewAuditLog";
        }


        /// <summary>
        /// Gets all system permissions
        /// </summary>
        /// <returns>Array of all permissions</returns>
        public static string[] GetAllPermissions()
        {
            return new[]
            {
                // Users
                Users.View, Users.Create, Users.Update, Users.Delete, Users.AssignRoles,
                // Roles
                Roles.View, Roles.Create, Roles.Update, Roles.Delete, Roles.ManagePermissions,
                    // Configuration
                Configuration.View, Configuration.Update, Configuration.ViewAuditLog,
            };
        }

        /// <summary>
        /// Gets permissions by module
        /// </summary>
        /// <param name="module">Module name</param>
        /// <returns>Array of module permissions</returns>
        public static string[] GetPermissionsByModule(string module)
        {
            return module switch
            {
                "Users" => new[] { Users.View, Users.Create, Users.Update, Users.Delete, Users.AssignRoles },
                "Roles" => new[] { Roles.View, Roles.Create, Roles.Update, Roles.Delete, Roles.ManagePermissions },
                "Configuration" => new[] { Configuration.View, Configuration.Update, Configuration.ViewAuditLog },
                _ => Array.Empty<string>()
            };
        }
    }
}
