using WarehouseApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost\\SQLEXPRESS; Database=WarehouseDb; Trusted_Connection=True; TrustServerCertificate=True;";

// Регистрируем инфраструктуру (DbContext и репозитории)
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
