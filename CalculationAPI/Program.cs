using CalculationAPI.Services;
 
var builder = WebApplication.CreateBuilder(args);

// Connection of Services
builder.Services.AddScoped<ExpressionEvaluator>();
builder.Services.AddControllers();

var app = builder.Build();

// HTTP request  
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
