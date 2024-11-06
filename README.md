# Bootstrap Identity

## How to run

```sh
docker compose -d
dotnet run
```

## Initial project setup

This section shows how the project was created.

```sh
dotnet new gitignore
dotnet new webapi --use-controllers --use-program-main
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.10
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.10
```

`appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "AppDb": "HOST=localhost;DB=postgres;UID=postgres;PWD=mysecret;PORT=5432;"
  }
}
```

`docker-compose.yml`

```yml
services:
  db:
    image: postgres:16-alpine
    restart: always
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: mysecret
    ports:
      - "5432:5432"
```

`AppDbContext.cs`

```cs
using Microsoft.EntityFrameworkCore;

namespace bootstrap_identity;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
}
```

`Program.cs`

```cs
var connectionString = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);

// ...

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}
```

## How to add Identity

This section shows you how add Identity to the project.

```sh
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 8.0.10
```

Change `AppDbContext.cs` to:

```cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace bootstrap_identity;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
}
```

`Program.cs`

```cs
builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        // Globally require users to be authenticated
        .RequireAuthenticatedUser()
        .Build();
});
builder
    .Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

//...

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<IdentityUser>().AllowAnonymous();
```
