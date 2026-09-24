using DinnerTable;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:5188");
builder.Services.AddSingleton<Store>();
builder.Services.AddSingleton<MealEngine>();
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
app.Run();
