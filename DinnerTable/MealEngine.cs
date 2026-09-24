namespace DinnerTable;
public sealed class MealEngine(Store db) {
 public List<GroceryLine> Grocery(PlanState state,IEnumerable<string>? meals=null) => (meals??state.Meals).SelectMany(id=>db.Recipes.Single(r=>r.Id==id).Ingredients.Select(p=>new {p.IngredientId,Quantity=p.Quantity*state.People/db.Recipes.Single(r=>r.Id==id).Servings})).GroupBy(p=>p.IngredientId).Select(g=>{var i=db.Ingredients.Single(x=>x.Id==g.Key);var q=g.Sum(x=>x.Quantity);var packs=(int)Math.Ceiling(q/i.PackSize);return new GroceryLine(i.Id,i.Name,i.Category,i.Unit,q,packs,i.PackSize,packs*i.PackPrice,state.Checked.Contains(i.Id));}).OrderBy(x=>x.Category).ThenBy(x=>x.Name).ToList();
 private decimal Cost(PlanState s,IEnumerable<string> ids)=>Grocery(s,ids).Sum(x=>x.Cost);
 public object View(PlanState s){CalendarPlan.Normalize(s);var groceries=Grocery(s);var total=groceries.Sum(x=>x.Cost);return new {state=s,recipes=db.Recipes,history=db.History(),groceries,total,remaining=s.Budget-total,meals=s.Meals.Select(id=>{var r=db.Recipes.Single(x=>x.Id==id);var cost=r.Ingredients.Sum(p=>{var i=db.Ingredients.Single(x=>x.Id==p.IngredientId);return p.Quantity*s.People/r.Servings/i.PackSize*i.PackPrice;});return new{recipe=r,cost,ingredients=r.Ingredients.Select(p=>new{name=db.Ingredients.Single(i=>i.Id==p.IngredientId).Name,unit=db.Ingredients.Single(i=>i.Id==p.IngredientId).Unit,quantity=p.Quantity*s.People/r.Servings})};})};}
 public void Apply(PlanState s,ActionRequest a){CalendarPlan.Normalize(s);
  if(a.Type is "settings" or "generate") {if(a.People is <1 or >20 || a.Nights is <1 or >7 || a.Budget is <1 or >2000)throw new ArgumentException("Use 1–20 people, 1–7 nights and a budget of $1–$2,000.");s.People=a.People??s.People;s.Nights=a.Nights??s.Nights;s.Budget=a.Budget??s.Budget;}
  if(a.Type=="generate"){
   var eligible=db.Recipes.Where(r=>!s.Dislikes.Contains(r.Id)&&r.ProteinPerServing>=s.MinProteinPerServing).ToList();if(eligible.Count==0)throw new ArgumentException("Allow a recipe with at least 30 g estimated protein per serving before generating a plan.");
   var requiredMeat=(int)Math.Ceiling(s.Nights*s.MeatDinnerFraction);
   var meatIds=eligible.Where(r=>r.HasMeat).Select(r=>r.Id).ToHashSet();
   if(requiredMeat>0&&meatIds.Count==0)throw new ArgumentException("Allow at least one meat recipe to build your mostly-meat plan.");
   var allowRepeats=eligible.Count<s.Nights||meatIds.Count<requiredMeat;
   // Bounded beam search ranks consolidated package baskets, not individual recipe costs.
   var beam=new List<List<string>>{new()};
   for(int n=0;n<s.Nights;n++)beam=beam.SelectMany(p=>eligible.Where(r=>(allowRepeats||!p.Contains(r.Id))&&(r.HasMeat||p.Count(id=>!meatIds.Contains(id))<s.Nights-requiredMeat)).Select(r=>p.Append(r.Id).Order().ToList())).DistinctBy(p=>string.Join(',',p)).OrderBy(p=>Cost(s,p)-p.Count(s.Favorites.Contains)*0.35m+p.Count(id=>s.Meals.Contains(id))*0.15m).Take(180).ToList();
   s.Meals=beam.OrderBy(p=>Cost(s,p)>s.Budget?1:0).ThenBy(p=>Cost(s,p)-p.Count(s.Favorites.Contains)*0.35m).First();if(s.MealDates.Count!=s.Meals.Count)s.MealDates=Enumerable.Range(0,s.Meals.Count).Select(i=>DateOnly.Parse(s.WeekStart).AddDays(i).ToString("yyyy-MM-dd")).ToList();s.Checked.Clear();
  }else if(a.Type is "remove" or "swap"){
   if(a.Index is null || a.Index<0 || a.Index>=s.Meals.Count)throw new ArgumentException("That dinner is no longer in the plan.");
   if(a.Type=="remove"){s.Meals.RemoveAt(a.Index.Value);s.MealDates.RemoveAt(a.Index.Value);}
   else {var meatNeeded=(int)Math.Ceiling(s.Meals.Count*s.MeatDinnerFraction);var remainingMeat=s.Meals.Where((_,i)=>i!=a.Index.Value).Count(id=>db.Recipes.Single(r=>r.Id==id).HasMeat);var options=db.Recipes.Where(r=>!s.Dislikes.Contains(r.Id)&&!s.Meals.Contains(r.Id)&&r.ProteinPerServing>=s.MinProteinPerServing&&(remainingMeat>=meatNeeded||r.HasMeat)).ToList();if(options.Count==0)throw new ArgumentException("No alternative meets your meat and protein preferences. Allow more meat recipes in your recipe collection.");var index=a.Index.Value;var best=options.OrderBy(r=>Cost(s,s.Meals.Where((_,i)=>i!=index).Append(r.Id))- (s.Favorites.Contains(r.Id)?0.35m:0)).First();s.Meals[index]=best.Id;}
   s.Checked.Clear();
  }else if(a.Type=="setMeal"){CalendarPlan.SetMeal(s,a.Date,a.Id,db.Recipes);}else if(a.Type is "favorite" or "dislike"){
   if(!db.Recipes.Any(r=>r.Id==a.Id))throw new ArgumentException("Recipe not found.");var set=a.Type=="favorite"?s.Favorites:s.Dislikes;if(!set.Add(a.Id!))set.Remove(a.Id!);if(a.Type=="dislike")s.Favorites.Remove(a.Id!);else s.Dislikes.Remove(a.Id!);
  }else if(a.Type=="check"){if(!db.Ingredients.Any(i=>i.Id==a.Id))throw new ArgumentException("Ingredient not found.");if(!s.Checked.Add(a.Id!))s.Checked.Remove(a.Id!);}
  else if(a.Type!="settings")throw new ArgumentException("Unknown action.");
 }
}
