using ADO.Access;
using ADO.Access.Access;
using ADO.Access.DataTest;
using ADO.BL.Helper;
using ADO.BL.Interfaces;
using ADO.BL.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 50 * 1024 * 1024);

// Add services to the container.
builder.Services.AddControllersWithViews();

// signalR
builder.Services.AddSignalR();

// Cors
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowLocalhost",
//        builder => builder
//            .WithOrigins("http://127.0.0.1:5500", "http://localhost:4200", "https://localhost:7155/", "https://localhost:7189/") // Dirección de tu frontend
//            .AllowAnyMethod()
//            .AllowAnyHeader());
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSignalR",
        policy => policy
            .WithOrigins("https://localhost:7189", "https://127.0.0.1:7155", "http://127.0.0.1:4200", "http://127.0.0.1:5500")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // Importante para SignalR
});

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new GlobalMapper());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

// interfaces
builder.Services.AddTransient<IFileAssetServices, FileAssetServices>();
builder.Services.AddTransient<IFileAssetDataAccess, FileAssetDataAccess>();
builder.Services.AddTransient<ITC1Services, TC1Services>();
builder.Services.AddTransient<IFileIdeamValidationServices, FileIdeamValidationServices>();
builder.Services.AddTransient<IFileDataAccess, FileDataAccess>();
builder.Services.AddTransient<IAllAssetOracleServices, AllAssetOracleServices>();
builder.Services.AddTransient<IAllAssetOracleDataAccess, AllAssetOracleDataAccess>();
builder.Services.AddTransient<IFileTT2ValidationServices, FileTT2ValidationServices>();
builder.Services.AddTransient<ITokenServices, TokenServices>();
builder.Services.AddTransient<IFileRayosValidationServices, FileRayosValidationServices>();
builder.Services.AddTransient<IFileRayosProcessingServices, FileRayosProcessingServices>();
builder.Services.AddTransient<IRayosCSVDataAccess, RayosCSVDataAccess>();
builder.Services.AddTransient<IRamalesServices, RamalesServices>();
builder.Services.AddTransient<IRamalesDataAccess, RamalesDataAccess>();
builder.Services.AddTransient<IFileIOServices, FileIOServices>();
builder.Services.AddTransient<IFileIODataAccess, FileIODataAccess>();

builder.Services.AddTransient<ITT2GlobalServices, TT2GlobalServices>();
builder.Services.AddTransient<ILacsGlobalEssaServices, LacsGlobalEssaServices>();
builder.Services.AddTransient<ISSPDGlobalServices, SSPDGlobalServices>();

builder.Services.AddTransient<ILACValidationEssaServices, LACValidationEssaServices>();
builder.Services.AddTransient<ISSPDValidationEepServices, SSPDValidationEepServices>();
builder.Services.AddTransient<ITC1ValidationServices, TC1ValidationServices>();
builder.Services.AddTransient<ITT2ValidationServices, TT2ValidationServices>();

builder.Services.AddTransient<ILacsFileValidationServices, LacsFileValidationServices>();
builder.Services.AddTransient<ILacsFileProcessServices, LacsFileProcessServices>();

builder.Services.AddTransient<ISSPDFileValidationServices, SSPDFileValidationServices>();
builder.Services.AddTransient<ISSPDFileProcessingServices, SSPDFileProcessingServices>();

builder.Services.AddTransient<ITC1FileValidationServices, TC1FileValidationServices>();
builder.Services.AddTransient<ITC1FileProcessingServices, TC1FileProcessingServices>();

builder.Services.AddTransient<ITT2FileValidationServices, TT2FileValidationServices>();
builder.Services.AddTransient<ITT2FileProcessingServices, TT2FileProcessingServices>();

builder.Services.AddTransient<IFileIOValidationServices, FileIOValidationServices>();
builder.Services.AddTransient<IFileIOProcessingServices, FileIOProcessingServices>();
builder.Services.AddTransient<IIoCommentsDataAccess, IoCommentsDataAccess>();

builder.Services.AddTransient<IFileAssetValidationServices, FileAssetValidationServices>();
builder.Services.AddTransient<IFileAssetProcessingServices, FileAssetProcessingServices>();

builder.Services.AddTransient<IQueueGlobalServices, QueueGlobalServices>();

builder.Services.AddTransient<IStatusFileDataAccess, StatusFileDataEssaAccess>();

builder.Services.AddTransient<IFilePolesValidationServices, FilePolesValidationServices>();
builder.Services.AddTransient<IFilePolesProcessingServices, FilePolesProcessingServices>();
builder.Services.AddTransient<IPolesDataAccess, PolesDataAccess>();

builder.Services.AddTransient<IPolesEssaServices, PolesEssaServices>();
builder.Services.AddTransient<IPolesEssaDataAccess, PolesEssaDataAccess>();

builder.Services.AddTransient<IPodasEssaServices, PodasEssaServices>();
builder.Services.AddTransient<IPodasEssaDataAccess, PodasEssaDataAccess>();

builder.Services.AddTransient<IFileAssetCierreServices, FileAssetCierreServices>();

builder.Services.AddTransient<IFileAssetModifiedServices, FileAssetModifiedServices>();
builder.Services.AddTransient<IFileAssetModifiedDataAccess, FileAssetModifiedDataAccess>();

// bd connection
builder.Services.AddDbContext<DannteTestingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PgDbTestingConnection")));

// swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DannteADOAPI v1", Version = "v1" });
    var filePath = Path.Combine(System.AppContext.BaseDirectory, "DannteADOAPI.xml");
    c.IncludeXmlComments(filePath);
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insert Token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

//jwt
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWT:Key").Value)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Home/Error");
    ////The default HSTS value is 30 days.You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DannteADOAPI v1"));
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DannteADOAPI v1"));
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DannteADOAPI v1"));
app.UseHttpsRedirection();
app.UseStaticFiles();



app.UseRouting();

app.UseCors("AllowSignalR");

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/notiHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
