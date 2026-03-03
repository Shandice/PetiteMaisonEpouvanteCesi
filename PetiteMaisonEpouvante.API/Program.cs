using PetiteMaisonEpouvante.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration des Logs (Serilog + Seq)
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.Seq("http://seq:5341")); // Envoi les logs vers le conteneur Seq

// 2. Configuration Base de données
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<StoreContext>(options => options.UseSqlServer(connectionString));

// 3. Configuration de l'Authentification (JWT)
var secretKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Important car pas de HTTPS dans Docker interne
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 4. Configuration Swagger pour tester l'authentification
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Entrez 'Bearer' [espace] puis votre token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 5. CORS : Autorise ton Frontend à faire des requêtes complexes (POST avec JSON et Token)
// Il n'y a plus qu'une seule définition de politique ici
builder.Services.AddCors(options => 
{
    options.AddPolicy("StrictPolicy", p => p
        .SetIsOriginAllowed(_ => true) // Autorise n'importe quelle origine pour le POC
        .AllowAnyMethod()              // Autorise GET, POST, OPTIONS, etc.
        .AllowAnyHeader());            // Autorise les en-têtes Authorization et Content-Type
});

var app = builder.Build();

// Création de la DB au démarrage
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var db = services.GetRequiredService<StoreContext>();
        
        // On détruit et on recrée la base proprement pour le POC (assure que les nouvelles colonnes sont là)
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated(); 
        
        Log.Information("Base de données initialisée avec succès pour le POC.");
    } 
    catch (Exception ex) 
    { 
        Log.Fatal(ex, "Erreur fatale base de données.");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

// ----------------------------------------------------
// ORDRE STRICT DES MIDDLEWARES : TRÈS IMPORTANT EN .NET
// ----------------------------------------------------
app.UseRouting();             // 1. Gère la route demandée

app.UseCors("StrictPolicy");  // 2. Autorise la requête selon le CORS défini en haut

app.UseAuthentication();      // 3. Vérifie qui est l'utilisateur (lit le Token JWT)
app.UseAuthorization();       // 4. Vérifie si l'utilisateur a le droit d'accéder (le [Authorize])

app.MapControllers();         // 5. Envoie la requête au bon Controller

app.Run();

public partial class Program { }