using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
/*builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);  
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps(); 
    });

    ,
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  }
});*/
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

/*app.Use(async (context, next) => debugTool
{
    if (context.User.Identity.IsAuthenticated)
    {
        foreach (var claim in context.User.Claims)
        {
            app.Logger.LogInformation($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
        }

        var roleClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        var role = roleClaim?.Value;
        app.Logger.LogInformation($"User is authenticated with role: {role}");
    }
    else
    {
        app.Logger.LogInformation("User is not authenticated");
    }
    await next.Invoke();
});*/

app.MapReverseProxy();

await app.RunAsync();