namespace DinnerTable;
public record Ingredient(string Id, string Name, string Category, string Unit, decimal PackSize, decimal PackPrice);
public record Portion(string IngredientId, decimal Quantity);
public record Recipe(string Id, string Name, string Description, string Category, int Minutes, int Servings, List<Portion> Ingredients, string[] Steps) { public decimal ProteinPerServing => Nutrition.Protein(this); public bool HasMeat => Nutrition.HasMeat(this); }
public class PlanState {
 public decimal MinProteinPerServing { get; set; } = 30;
 public decimal MeatDinnerFraction { get; set; } = .8m;
 public int People { get; set; } = 4;
 public int Nights { get; set; } = 5;
 public decimal Budget { get; set; } = 75;
 public List<string> Meals { get; set; } = [];
 public HashSet<string> Favorites { get; set; } = [];
 public HashSet<string> Dislikes { get; set; } = [];
 public HashSet<string> Checked { get; set; } = [];
}
public record ActionRequest(string Type, int? People, int? Nights, decimal? Budget, string? Id, int? Index);
public record GroceryLine(string Id, string Name, string Category, string Unit, decimal Quantity, int Packs, decimal PackSize, decimal Cost, bool Checked);
// Retailer-specific matching, pricing and future cart preparation remain outside the meal engine.
public interface IRetailerAdapter {
 string Name { get; }
 Task<IReadOnlyList<ProductMatch>> MatchAsync(Ingredient ingredient, CancellationToken cancellationToken);
 Task<CartPreparation> PrepareCartAsync(IReadOnlyList<ProductMatch> products, CancellationToken cancellationToken);
}
public record ProductMatch(string ProductId, string Name, decimal PackageQuantity, string Unit, decimal Price, DateTimeOffset QuotedAt, Uri? ProductUrl);
public record CartPreparation(bool Available, string Message, Uri? ReviewUrl);
public sealed class WalmartAdapter : IRetailerAdapter {
 public string Name => "Walmart";
 public Task<IReadOnlyList<ProductMatch>> MatchAsync(Ingredient ingredient, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ProductMatch>>([]);
 public Task<CartPreparation> PrepareCartAsync(IReadOnlyList<ProductMatch> products, CancellationToken cancellationToken) => Task.FromResult(new CartPreparation(false, "Walmart is not connected. No consumer cart API is assumed. Future browser integration must require checkout approval.", null));
}
