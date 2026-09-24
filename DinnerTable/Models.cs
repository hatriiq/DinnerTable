namespace DinnerTable;
public record Ingredient(string Id, string Name, string Category, string Unit, decimal PackSize, decimal PackPrice);
public record Portion(string IngredientId, decimal Quantity);
public record Recipe(string Id, string Name, string Description, string Category, int Minutes, int Servings, List<Portion> Ingredients, string[] Steps) { public decimal ProteinPerServing => Nutrition.Protein(this); public bool HasMeat => Nutrition.HasMeat(this); }
public class PlanState {
 public string WeekStart { get; set; } = CalendarPlan.Monday(DateOnly.FromDateTime(DateTime.Today));
 public List<string> MealDates { get; set; } = [];
 public decimal MinProteinPerServing { get; set; } = 30;
 public decimal MeatDinnerFraction { get; set; } = .94m;
 public int People { get; set; } = 4;
 public int Nights { get; set; } = 5;
 public decimal Budget { get; set; } = 75;
 public List<string> Meals { get; set; } = [];
 public HashSet<string> Favorites { get; set; } = [];
 public HashSet<string> Dislikes { get; set; } = [];
 public HashSet<string> Checked { get; set; } = [];
}
public record ActionRequest(string Type, int? People, int? Nights, decimal? Budget, string? Id, int? Index, string? Date = null);
public record GroceryLine(string Id, string Name, string Category, string Unit, decimal Quantity, int Packs, decimal PackSize, decimal Cost, bool Checked);
