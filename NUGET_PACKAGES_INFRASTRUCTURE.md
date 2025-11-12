# Referencias NuGet Faltantes - CC.Infrastructure

## INSTALAR EN CC.Infrastructure.csproj

El proyecto CC.Infrastructure necesita referencias a paquetes de ASP.NET Core para usar Authorization y HttpContext.

### Comandos de Instalación

```bash
# Package Manager Console (Visual Studio)
Install-Package Microsoft.AspNetCore.Authorization -ProjectName CC.Infrastructure
Install-Package Microsoft.AspNetCore.Http.Abstractions -ProjectName CC.Infrastructure

# o .NET CLI
dotnet add .\CC.Infrastructure\CC.Infrastructure.csproj package Microsoft.AspNetCore.Authorization
dotnet add .\CC.Infrastructure\CC.Infrastructure.csproj package Microsoft.AspNetCore.Http.Abstractions
```

### Verificación

Después de instalar, el archivo `CC.Infrastructure.csproj` debe contener:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.Authorization" Version="8.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
  <!-- Otros paquetes existentes -->
</ItemGroup>
```

---

## Alternativa: Editar .csproj Manualmente

Si prefieres editar directamente el archivo:

1. Abrir `CC.Infrastructure\CC.Infrastructure.csproj`
2. Buscar el `<ItemGroup>` de PackageReferences
3. Agregar:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authorization" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version="2.2.0" />
```

4. Guardar y ejecutar `dotnet restore`

---

## ? Verificación

Ejecutar build nuevamente. Los errores de `PermissionHandler` y `PermissionRequirement` deben desaparecer.

```bash
dotnet build .\CC.Infrastructure\CC.Infrastructure.csproj
```
