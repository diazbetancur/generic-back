using CC.Domain.Constants;
using CC.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Api_Portar_Paciente.Configuration
{
    /// <summary>
    /// Configuración centralizada de políticas de autorización
    /// </summary>
    public static class AuthorizationPoliciesConfiguration
    {
        /// <summary>
        /// Configura todas las políticas de autorización del sistema
        /// </summary>
        /// <param name="options">Opciones de autorización</param>
        public static void ConfigurePolicies(AuthorizationOptions options)
        {
            // ===== POLÍTICAS POR TIPO DE USUARIO =====
            
            options.AddPolicy(PermissionConstants.Policies.PatientOnly, policy =>
                policy.RequireClaim("UserType", "Patient"));

            options.AddPolicy(PermissionConstants.Policies.AdminOnly, policy =>
                policy.RequireClaim("UserType", "Admin"));

            // ===== MÓDULO REQUESTS =====

            options.AddPolicy(PermissionConstants.Policies.CanViewRequests, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Requests.View)));

            options.AddPolicy(PermissionConstants.Policies.CanCreateRequests, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Requests.Create)));

            options.AddPolicy(PermissionConstants.Policies.CanUpdateRequests, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Requests.Update)));

            options.AddPolicy(PermissionConstants.Policies.CanDeleteRequests, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Requests.Delete)));

            options.AddPolicy(PermissionConstants.Policies.CanAssignRequests, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Requests.Assign)));

            options.AddPolicy(PermissionConstants.Policies.CanChangeRequestState, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Requests.ChangeState)));

            // ===== MÓDULO USERS =====

            options.AddPolicy(PermissionConstants.Policies.CanViewUsers, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Users.View)));

            options.AddPolicy(PermissionConstants.Policies.CanManageUsers, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Users.Create)));

            options.AddPolicy(PermissionConstants.Policies.CanAssignRoles, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Users.AssignRoles)));

            // ===== MÓDULO ROLES =====

            options.AddPolicy(PermissionConstants.Policies.CanViewRoles, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Roles.View)));

            options.AddPolicy(PermissionConstants.Policies.CanManageRoles, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Roles.Create)));

            options.AddPolicy(PermissionConstants.Policies.CanManagePermissions, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Roles.ManagePermissions)));

            // ===== MÓDULO REPORTS =====

            options.AddPolicy(PermissionConstants.Policies.CanViewReports, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Reports.View)));

            options.AddPolicy(PermissionConstants.Policies.CanExportReports, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Reports.Export)));

            options.AddPolicy(PermissionConstants.Policies.CanViewAllReports, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Reports.ViewAll)));

            // ===== MÓDULO NILREAD =====

            options.AddPolicy(PermissionConstants.Policies.CanViewExams, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.NilRead.ViewExams)));

            options.AddPolicy(PermissionConstants.Policies.CanViewMedicalReports, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.NilRead.ViewReports)));

            options.AddPolicy(PermissionConstants.Policies.CanViewMedicalImages, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.NilRead.ViewImages)));

            // ===== MÓDULO CONFIGURATION =====

            options.AddPolicy(PermissionConstants.Policies.CanViewConfig, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Configuration.View)));

            options.AddPolicy(PermissionConstants.Policies.CanUpdateConfig, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Configuration.Update)));

            options.AddPolicy(PermissionConstants.Policies.CanViewAuditLog, policy =>
                policy.AddRequirements(new PermissionRequirement(PermissionConstants.Configuration.ViewAuditLog)));
        }
    }
}
