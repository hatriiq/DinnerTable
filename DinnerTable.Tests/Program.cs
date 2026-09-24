using DinnerTable;
using Microsoft.AspNetCore.Builder;
var builder=WebApplication.CreateBuilder();
builder.Environment.ContentRootPath=Path.Combine(Path.GetTempPath(),"DinnerTable-tests-"+Guid.NewGuid());
var db=new Store(builder.Environment);var engine=new MealEngine(db);var s=db.Read();int count=0;
void Check(bool condition,string name){if(!condition)throw new Exception(name);Console.WriteLine("PASS "+name);count++;}
ActionRequest A(string type,int? people=null,int? nights=null,decimal? budget=null,string id=null,int? index=null)=>new(type,people,nights,budget,id,index);
engine.Apply(s,A("generate"));Check(s.Meals.Count==5,"requested dinner count");Check(s.Meals.Distinct().Count()==5,"variety without repeated dinners");
Check(s.Meals.Count(id=>db.Recipes.Single(r=>r.Id==id).HasMeat)>=4,"at least four meat dinners in five-night plan");
Check(s.Meals.All(id=>db.Recipes.Single(r=>r.Id==id).ProteinPerServing>=30),"every dinner meets protein target");
var original=engine.Grocery(s);Check(original.Select(g=>g.Id).Distinct().Count()==original.Count,"duplicate ingredients consolidated");Check(original.Sum(x=>x.Cost)<=s.Budget,"default plan within budget");
engine.Apply(s,A("settings",people:8));var doubled=engine.Grocery(s);Check(doubled.All(x=>x.Quantity==original.Single(g=>g.Id==x.Id).Quantity*2),"serving quantities double");Check(doubled.All(x=>x.Packs*x.PackSize>=x.Quantity),"whole packages cover ingredient needs");
var old=s.Meals[0];engine.Apply(s,A("swap",index:0));Check(s.Meals[0]!=old&&s.Meals.Count==5,"swap changes one dinner");Check(s.Meals.Count(id=>db.Recipes.Single(r=>r.Id==id).HasMeat)>=4&&s.Meals.All(id=>db.Recipes.Single(r=>r.Id==id).ProteinPerServing>=30),"swaps preserve meat and protein rules");engine.Apply(s,A("remove",index:0));Check(s.Meals.Count==4,"remove recalculates plan");
engine.Apply(s,A("favorite",id:old));engine.Apply(s,A("dislike",id:s.Meals[0]));var excluded=s.Meals[0];engine.Apply(s,A("generate",people:4,budget:1));Check(!s.Meals.Contains(excluded),"dislikes excluded from generation");Check(engine.Grocery(s).Sum(x=>x.Cost)>s.Budget,"impossible budget remains visibly over budget");
engine.Apply(s,A("check",id:engine.Grocery(s)[0].Id));db.Save(s,true);var reopened=new Store(builder.Environment);var restored=reopened.Read();Check(restored.People==s.People&&restored.Budget==s.Budget&&restored.Meals.SequenceEqual(s.Meals)&&restored.Favorites.SetEquals(s.Favorites)&&restored.Dislikes.SetEquals(s.Dislikes)&&restored.Checked.SetEquals(s.Checked),"SQLite survives store restart with all preferences");Check(reopened.History().Length==1,"history snapshot persisted");
try{engine.Apply(s,A("generate",people:0));throw new Exception("invalid accepted");}catch(ArgumentException){Check(true,"invalid input rejected");}
foreach(var r in db.Recipes)if(!s.Dislikes.Contains(r.Id))engine.Apply(s,A("dislike",id:r.Id));try{engine.Apply(s,A("generate"));throw new Exception("all dislikes accepted");}catch(ArgumentException){Check(true,"all-disliked collection handled");}
var sparse=new PlanState{Nights=7,Dislikes=db.Recipes.Where(r=>r.Id!="chicken-fried-rice").Select(r=>r.Id).ToHashSet()};engine.Apply(sparse,A("generate"));Check(sparse.Meals.Count==7&&sparse.Meals.All(id=>id=="chicken-fried-rice"),"sparse collection repeats eligible meat instead of silently dropping preferences");
Check(restored.MinProteinPerServing==30&&restored.MeatDinnerFraction==.8m,"protein preferences survive restart");
Console.WriteLine($"{count} checks passed.");
