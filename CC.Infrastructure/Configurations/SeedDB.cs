namespace CC.Infrastructure.Configurations
{
    public class SeedDB
    {
        private readonly DBContext _dbContext;

        public SeedDB(DBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task SeedAsync()
        {
            await _dbContext.Database.EnsureCreatedAsync();
            await EnsureDocType();
            await EnsureGeneralSettings();
        }

        private async Task EnsureDocType()
        {
            if (!_dbContext.DocTypes.Any())
            {
                _dbContext.DocTypes.Add(new Domain.Entities.DocType
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Code = "CC",
                    Description = "Cédula de Ciudadanía",
                    IsActive = true,
                }
                );

                _dbContext.DocTypes.Add(new Domain.Entities.DocType
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Code = "CE",
                    Description = "Cédula de Extranjería",
                    IsActive = true,
                }
                );

                _dbContext.DocTypes.Add(new Domain.Entities.DocType
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Code = "TI",
                    Description = "Tarjeta de Identidad",
                    IsActive = true,
                }
                );

                _dbContext.DocTypes.Add(new Domain.Entities.DocType
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Code = "RC",
                    Description = "Registro Civil",
                    IsActive = true,
                }
                );
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task EnsureGeneralSettings()
        {
            if (!_dbContext.GeneralSettings.Any())
            {
                _dbContext.GeneralSettings.Add(new Domain.Entities.GeneralSettings
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Key = "TokenLifetimeMinutes",
                    Value = "30",
                    Description = "Tiempo de vida del token en minutos",
                }
                );

                _dbContext.GeneralSettings.Add(new Domain.Entities.GeneralSettings
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Key = "OtpTtlSeconds",
                    Value = "300",
                    Description = "Tiempo de vida del OTP en segundos",
                }
                );

                _dbContext.GeneralSettings.Add(new Domain.Entities.GeneralSettings
                {
                    DateCreated = DateTime.Now,
                    Id = Guid.NewGuid(),
                    Key = "MensajeSinContacto",
                    Value = "No se encontraron medios de contacto registrados. Por favor comuníquese al WhatsApp +57 300 123 4567 para actualizar sus datos.",
                    Description = "Mensaje mostrado cuando el usuario no tiene email ni celular registrado",
                }
                );

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}