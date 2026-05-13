using ApiDescargaSriV9.CDescarga;
using ApiDescargaSriV9.Context;
using ApiDescargaSriV9.Dto;
using ApiDescargaSriV9.Helpers;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<ViewDatosRecibidosDto>("ViewDatosRecibidosDto");
    return builder.GetEdmModel();
}
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// Learn more about

builder.Services.AddCors();

builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddHttpContextAccessor();

// builder.Services.AddDbContext<AplicationDbContext>
//     (options => options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers().AddOData(options =>
{
    options.Count().Filter().OrderBy().Expand().Select().SetMaxTop(100);
    options.AddRouteComponents("odata", GetEdmModel());
});



builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(FiltrosErrores));
}).AddNewtonsoftJson();



// configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<CDescarga>();


builder.Services.AddSwaggerGen(c =>
{

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseRouting();
app.UseStaticFiles();


app.UseAuthorization();

app.MapControllers();

app.Run();
