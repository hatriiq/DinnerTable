namespace DinnerTable;
public static class CalendarPlan {
 public static string Monday(DateOnly day)=>day.AddDays(-(((int)day.DayOfWeek+6)%7)).ToString("yyyy-MM-dd");
 public static string ParseWeek(string? date){if(!DateOnly.TryParseExact(date,"yyyy-MM-dd",out var day)||day.Year<2020||day.Year>2100)throw new ArgumentException("Choose a valid calendar date between 2020 and 2100.");return Monday(day);}
 public static void Normalize(PlanState s){
  s.WeekStart=ParseWeek(s.WeekStart);
  if(s.MealDates.Count!=s.Meals.Count)s.MealDates=Enumerable.Range(0,s.Meals.Count).Select(i=>DateOnly.Parse(s.WeekStart).AddDays(i).ToString("yyyy-MM-dd")).ToList();
 }
 public static void SetMeal(PlanState s,string? date,string? recipeId,IReadOnlyList<Recipe> recipes){
  Normalize(s);if(!DateOnly.TryParseExact(date,"yyyy-MM-dd",out var day)||Monday(day)!=s.WeekStart)throw new ArgumentException("Choose a day in the selected week.");
  var r=recipes.SingleOrDefault(r=>r.Id==recipeId)??throw new ArgumentException("Recipe not found.");
  if(s.Dislikes.Contains(r.Id))throw new ArgumentException("Allow this recipe again before adding it.");
  int index=s.MealDates.IndexOf(date!);
  if(index>=0)s.Meals[index]=r.Id;else{s.Meals.Add(r.Id);s.MealDates.Add(date!);}
  var sorted=s.MealDates.Zip(s.Meals).OrderBy(x=>x.First).ToList();s.MealDates=sorted.Select(x=>x.First).ToList();s.Meals=sorted.Select(x=>x.Second).ToList();s.Nights=Math.Max(s.Nights,s.Meals.Count);s.Checked.Clear();
 }
}
