using DinnerTable;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:5188");
builder.Services.AddSingleton<Store>();
builder.Services.AddSingleton<MealEngine>();
builder.Services.AddSingleton<IRetailerAdapter, WalmartAdapter>();
var app = builder.Build();
app.Use(async (ctx, next) => {
    if (ctx.Request.Method == "POST" && ctx.Request.Headers.Origin.Count > 0 && ctx.Request.Headers.Origin != "http://127.0.0.1:5188" && ctx.Request.Headers.Origin != "http://localhost:5188") { ctx.Response.StatusCode = 403; return; }
    try { await next(); } catch (ArgumentException ex) { ctx.Response.StatusCode = 400; await ctx.Response.WriteAsJsonAsync(new { error = ex.Message }); }
});
app.UseDefaultFiles(); app.UseStaticFiles();
app.MapGet("/api/state", (Store db, MealEngine engine) => engine.View(db.Read()));
app.MapPost("/api/action", (ActionRequest request, Store db, MealEngine engine) => {
    lock(db) { var state = db.Read(); engine.Apply(state, request); db.Save(state, request.Type == "generate"); return engine.View(state); }
});
app.MapPost("/api/week",(WeekRequest request,Store db,MealEngine engine)=>{lock(db){return engine.View(db.SwitchWeek(request.Date));}});
app.MapGet("/api/retailer",(Store db,MealEngine engine,IRetailerAdapter retailer)=>retailer.BuildBasket(db.Read(),engine.Grocery(db.Read())));
app.MapPost("/api/retailer/settings",(RetailerSettings settings,Store db)=>{lock(db){WalmartAdapter.ValidateSettings(settings);db.SaveDocument("walmart-settings",settings);return Results.Ok();}});
app.MapPost("/api/retailer/match",(RetailerMatch match,Store db)=>{lock(db){WalmartAdapter.ValidateMatch(match,db.Ingredients);var matches=db.ReadDocument("walmart-matches",WalmartProducts.Seeds);matches.RemoveAll(m=>m.IngredientId==match.IngredientId);matches.Add(match with{ObservedAt=DateTimeOffset.UtcNow});db.SaveDocument("walmart-matches",matches);return Results.Ok();}});
app.Run();
public record WeekRequest(string Date);
