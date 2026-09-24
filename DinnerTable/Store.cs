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
  // Add new seeds without replacing saved recipes, preferences, or history.
  Recipes.AddRange(Seed.Recipes.Where(seed=>Recipes.All(r=>r.Id!=seed.Id)));
  Ingredients.AddRange(Seed.Ingredients.Where(seed=>Ingredients.All(i=>i.Id!=seed.Id)));
  UpdateCatalog("recipes",Recipes); UpdateCatalog("ingredients",Ingredients);
 }
 private void UpdateCatalog<T>(string id,T value){using var db=Open();using var cmd=db.CreateCommand();cmd.CommandText="UPDATE documents SET json=$json WHERE id=$id";cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$json",JsonSerializer.Serialize(value));cmd.ExecuteNonQuery();}
 private SqliteConnection Open(){var db=new SqliteConnection(connection);db.Open();return db;}
 private void Insert<T>(string id,T value){using var db=Open();using var cmd=db.CreateCommand();cmd.CommandText="INSERT OR IGNORE INTO documents VALUES ($id,$json)";cmd.Parameters.AddWithValue("$id",id);cmd.Parameters.AddWithValue("$json",JsonSerializer.Serialize(value));cmd.ExecuteNonQuery();}
 private T Get<T>(string id){using var db=Open();using var cmd=db.CreateCommand();cmd.CommandText="SELECT json FROM documents WHERE id=$id";cmd.Parameters.AddWithValue("$id",id);return JsonSerializer.Deserialize<T>((string)cmd.ExecuteScalar()!)!;}
 public PlanState Read()=>Get<PlanState>("state");
 public void Save(PlanState state,bool history){using var db=Open();using var tx=db.BeginTransaction();using var cmd=db.CreateCommand();cmd.Transaction=tx;cmd.CommandText="UPDATE documents SET json=$json WHERE id='state'";cmd.Parameters.AddWithValue("$json",JsonSerializer.Serialize(state));cmd.ExecuteNonQuery();if(history){cmd.CommandText="INSERT INTO history(created,json) VALUES ($time,$json)";cmd.Parameters.AddWithValue("$time",DateTimeOffset.Now.ToString("O"));cmd.ExecuteNonQuery();}tx.Commit();}
 public object[] History(){using var db=Open();using var cmd=db.CreateCommand();cmd.CommandText="SELECT created,json FROM history ORDER BY id DESC LIMIT 20";using var reader=cmd.ExecuteReader();var result=new List<object>();while(reader.Read())result.Add(new{created=reader.GetString(0),plan=JsonSerializer.Deserialize<PlanState>(reader.GetString(1))});return result.ToArray();}
}
