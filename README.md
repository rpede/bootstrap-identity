# Bootstrap Identity

Create a new web api project using controllers

```sh
dotnet new webapi -controllers
```

Optionally create a different project for data access.
Then add these dependencies:

```sh
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

Create a DbContext, like this:

```cs
using Microsoft.EntityFrameworkCore;

namespace bootstrap_identity;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
}
```

In `Program.cs`, add the following before `builder.Build()`:

```cs
var connectionString = builder.Configuration.GetConnectionString("AppDb");
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
);

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();
```

And this after `builder.Build()`:

```cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

app.MapIdentityApi<IdentityUser>();
```

For Scalar, add dependency:

```sh
dotnet add package Scalar.AspNetCore
```

Then on the line right below `app.MapOpenApi()` add:

```cs
app.MapScalarApiReference();
```

We need a database to try it out.
Add to `appsettings.Development`:

```json
  "ConnectionStrings": {
    "AppDb": "HOST=localhost;DB=postgres;UID=postgres;PWD=mysecret;PORT=5432;"
  }
```

And this to `docker-compose.yml`:

```yml
services:
  db:
    image: postgres:17-alpine
    restart: always
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: mysecret
    ports:
      - "5432:5432"
```

To run do:

```sh
docker compose up -d
dotnet run
```

Now go to `/scalar` to try it out.
