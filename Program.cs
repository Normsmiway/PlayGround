using Geo.Api.Code;
using Geo.Api.Code.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.MapGet("/diatnce", (double x1, double y1, double x2, double y2) =>
{
    return CoordinateMap.GetDistance(new(x1, y1), new(x2, y2));
})
.WithName("GetDistance");

app.MapGet("/nearMe", (double latitude, double longitude, double radius, int take, int pageNumber) =>
{
    return CoordinateMap.GetCoordinatesWithRadius(new(latitude, longitude), radius, take, pageNumber);
})
.WithName("locationWithin");


app.MapGet("/rating", (string agentId,string ratingsMagicString) =>
{
    return RatingProvider.GetRatingSummary(agentId, ratingsMagicString);
})
.WithName("rating");

app.MapGet("/get-rating", (string agentId) =>
{
    return RatingProvider.GetRating(agentId);
})
.WithName("getrating");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}