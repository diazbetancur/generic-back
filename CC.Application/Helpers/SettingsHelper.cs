using CC.Domain.Interfaces.Repositories;

namespace CC.Aplication.Helpers
{
    /// <summary>
    /// Helper para operaciones comunes con GeneralSettings
    /// </summary>
    public static class SettingsHelper
    {
        /// <summary>
        /// Obtiene un valor de configuración como entero, con valor por defecto si no existe o no es válido
        /// </summary>
        public static async Task<int> GetIntSettingAsync(
            IGeneralSettingsRepository settingsRepo, 
            string key, 
            int defaultValue)
        {
            var setting = await settingsRepo.FindByAlternateKeyAsync(s => s.Key == key).ConfigureAwait(false);
            return setting != null && int.TryParse(setting.Value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Obtiene un valor de configuración como string, con valor por defecto si no existe
        /// </summary>
        public static async Task<string> GetStringSettingAsync(
            IGeneralSettingsRepository settingsRepo, 
            string key, 
            string defaultValue)
        {
            var setting = await settingsRepo.FindByAlternateKeyAsync(s => s.Key == key).ConfigureAwait(false);
            return setting?.Value ?? defaultValue;
        }

        /// <summary>
        /// Obtiene un valor de configuración como booleano, con valor por defecto si no existe o no es válido
        /// </summary>
        public static async Task<bool> GetBoolSettingAsync(
            IGeneralSettingsRepository settingsRepo, 
            string key, 
            bool defaultValue)
        {
            var setting = await settingsRepo.FindByAlternateKeyAsync(s => s.Key == key).ConfigureAwait(false);
            return setting != null && bool.TryParse(setting.Value, out var result) ? result : defaultValue;
        }
    }
}
