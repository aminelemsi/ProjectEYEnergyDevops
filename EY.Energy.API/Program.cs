using DinkToPdf.Contracts;
using DinkToPdf;
using EY.Energy.API.Controllers.Forms;
using EY.Energy.Application.EmailConfiguration;
using EY.Energy.Application.Services.Answers;
using EY.Energy.Application.Services.Forms;
using EY.Energy.Application.Services.Users;
using EY.Energy.Infrastructure.Configuration;
using EY.Energy.Infrastructure.Configuration.Validation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using EY.Energy.Application.Services.Claim;
using EY.Energy.Application.Services.Publications;
using EY.Energy.API.Hub;
using EY.Energy.Application.DTO.Chat;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(nameof(MongoDbSettings)));

builder.Services.AddSingleton<MongoDBContext>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoDBContext(settings.ConnectionString, settings.DatabaseName);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "EnergyManagementAppCookiehsisMysecretKeyHMACsha512ForErnstAndYoung1209AA2837";
        options.Cookie.HttpOnly = false;

    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:4200", "http://localhost:4600")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<AuthenticationServices>();
builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<CustomerServices>();
builder.Services.AddScoped<InvoiceServices>();
builder.Services.AddScoped<CompanyServices>();
builder.Services.AddScoped<ValidationServices>();
builder.Services.AddScoped<FormService>();
builder.Services.AddScoped<ClientResponseService>();
builder.Services.AddScoped<ClaimServices>();
builder.Services.AddScoped<PublicationService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<EnergyReportService>();
builder.Services.AddScoped<PublicationStatisticsService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<FilesController>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IDictionary<string, UserRoomConnection>>(opt=>
    new Dictionary<string, UserRoomConnection>());

var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libs", "libwkhtmltox.dll"));

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddTransient<PdfService>();

var app = builder.Build();
builder.Services.AddControllers();


app.UseRouting();

// Dans la methode Configure de Startup.cs
app.UseAuthentication();

app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("MyAllowSpecificOrigins");

app.UseAuthorization();

app.MapControllers();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chat");
});


app.Run();


