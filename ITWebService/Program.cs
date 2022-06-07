using ITWebService.Core.Config;

ConfigCore.AddConfig<DutyConfig>();
ConfigCore.ConfigInit().Wait();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var myallow = "_myallow";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myallow, policy => { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(myallow);
app.UseAuthorization();

app.MapControllers();

app.Run();

