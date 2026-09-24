using Microsoft.Data.Sqlite;
using System.Text.Json;
namespace DinnerTable;
public sealed class Store {
 private readonly string connection;
 public List<Recipe> Recipes { get; }
 public List<Ingredient> Ingredients { get; }
 public Store(IWebHostEnvironment environment) {
  var folder=Path.Combine(environment.ContentRootPath,"Data"); Directory.CreateDirectory(folder);
  connection=new SqliteConnectionStringBuilder{DataSource=Path.Combine(folder,"dinners.db")}.ToString();
  using var db=Open(); using var cmd=db.CreateCommand();
  cmd.CommandText="PRAGMA journal_mode=WAL; CREATE TABLE IF NOT EXISTS documents (id TEXT PRIMARY KEY, json TEXT NOT NULL); CREATE TABLE IF NOT EXISTS history (id INTEGER PRIMARY KEY, created TEXT NOT NULL, json TEXT NOT NULL);";cmd.ExecuteNonQuery();
  Insert("recipes",Seed.Recipes); Insert("ingredients",Seed.Ingredients); Insert("state",new PlanState());
  Recipes=Get<List<Recipe>>("recipes"); Ingredients=Get<List<Ingredient>>("ingredients");
  // Versioned catalog update: retain the old catalog for recovery and preserve all user state.
  using(var migration=Open())using(var check=migration.CreateCommand()){
   check.CommandText="SELECT COUNT(*) FROM documents WHERE id='catalog-v2'";
   if(Convert.ToInt32(check.ExecuteScalar())==0){
    Insert("recipes-before-v2",Recipes);
    var seeds=Seed.Recipes.ToDictionary(r=>r.Id);
    for(int i=0;i<Recipes.Count;i++)if(seeds.TryGetValue(Recipes[i].Id,out var replacement))Recipes[i]=replacement;
    var preferences=Read();preferences.MeatDinnerFraction=.94m;Save(preferences,false);
    UpdateCatalog("recipes",Recipes);Insert("catalog-v2",true);
   }
  }
  Recipes.AddRange(Seed.Recipes.Where(seed=>Recipes.All(r=>r.Id!=seed.Id)));
  Ingredients.AddRange(Seed.Ingredients.Where(seed=>Ingredients.All(i=>i.Id!=seed.Id)));
  UpdateCatalog("recipes",Recipes); UpdateCatalog("ingredients",Ingredients);
 }
 private void UpdateCatalog<T>(string id,T value){using var db=Open();using var cmd=db.CreateCommand();cmd.CommandText="UPDATE documents SET json=$json WHERE id=$id";cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$json",JsonSerializer.Serialize(value));cmd.ExecuteNonQuery();}
 private SqliteConnection Open(){var db=new SqliteConnection(connection);db.Open();return db;}
 private void Insert<T>(string id,T value){using var db=Open();using var cmd=db.CreateCommand();cmd.CommandText="INSERT OR IGNORE INTO documents VALUES ($id,$json)";cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$json",JsonSerializer.Serialize(value));cmd.ExecuteNonQuery();}
 private T Get<T>(string id){using var db=Open();using var cmd=db.CreateCommand();cmd.CommandText="SELECT json FROM documents WHERE id=$id";cmd.Parameters.AddWithValue("$id",id);return JsonSerializer.Deserialize<T>((string)cmd.ExecuteScalar()!)!;}
 public PlanState Read(){var s=Get<PlanState>("state");CalendarPlan.Normalize(s);return s;} public T ReadDocument<T>(string key,T fallback){Insert(key,fallback);return Get<T>(key);}
 public void SaveDocument<T>(string key,T value){Insert(key,value);UpdateCatalog(key,value);}
 public PlanState SwitchWeek(string date){
  var current=Read();Save(current,false);var week=CalendarPlan.ParseWeek(date);
  var target=ReadDocument("week:"+week,new PlanState{WeekStart=week,People=current.People,Nights=current.Nights,Budget=current.Budget});
  target.Favorites=current.Favorites;target.Dislikes=current.Dislikes;target.MinProteinPerServing=current.MinProteinPerServing;target.MeatDinnerFraction=current.MeatDinnerFraction;
  CalendarPlan.Normalize(target);Save(target,false);return target;
 }
 public void Save(PlanState state,bool history){using var db=Open();using var tx=db.BeginTransaction();using var cmd=db.CreateCommand();cmd.Transaction=tx;cmd.CommandText="UPDATE documents SET json=$json WHERE id='state'";cmd.Parameters.AddWithValue("$json",JsonSerializer.Serialize(state));cmd.ExecuteNonQuery();if(history){cmd.CommandText="INSERT INTO history(created,json) VALUES ($time,$json)";cmd.Parameters.AddWithValue("$time",DateTimeOffset.Now.ToString("O"));cmd.ExecuteNonQuery();}cmd.CommandText="INSERT INTO documents(id,json) VALUES ($week,$json) ON CONFLICT(id) DO UPDATE SET json=excluded.json";cmd.Parameters.AddWithValue("$week","week:"+state.WeekStart);cmd.ExecuteNonQuery();tx.Commit();}
 public object[] History(){using var db=Open();using var cmd=db.CreateCommand();cmd.CommandText="SELECT created,json FROM history ORDER BY id DESC LIMIT 20";using var reader=cmd.ExecuteReader();var result=new List<object>();while(reader.Read())result.Add(new{created=reader.GetString(0),plan=JsonSerializer.Deserialize<PlanState>(reader.GetString(1))});return result.ToArray();}
}
