namespace DinnerTable;
// Rounded generic estimates, not brand-specific nutrition labels. Quantities match the
// ingredient catalog: raw/dry grams, drained canned grams, eggs/tortillas by each.
// Reference: https://www.nal.usda.gov/sites/default/files/page-files/Protein.pdf
public static class Nutrition {
 private static readonly Dictionary<string,decimal> ProteinPerUnit = new() {
  ["garlic"]=.06m,["pepper"]=.01m,["broccoli"]=.03m,["cabbage"]=.01m,["greenbeans"]=.02m,["salsa"]=.01m,["bbq"]=0m,["mustard"]=.04m,["honey"]=0m,["yogurt"]=.10m,["milk"]=.033m,["butter"]=0m,["breadcrumb"]=.13m,["lemon"]=0m,
  ["chicken"]=.19m,["beef"]=.21m,["turkey"]=.19m,["pork"]=.21m,
  ["rice"]=.07m,["pasta"]=.13m,["tortilla"]=4m,["beans"]=.07m,
  ["tomato"]=.01m,["chickpea"]=.07m,["lentil"]=.24m,["egg"]=6m,
  ["cheese"]=.25m,["onion"]=.01m,["potato"]=.02m,["carrot"]=.01m,
  ["veg"]=.03m,["corn"]=.03m,["oil"]=0m,["spice"]=0m,["soy"]=.05m
 };
 public static decimal Protein(Recipe r)=>r.Ingredients.Sum(p=>p.Quantity*ProteinPerUnit.GetValueOrDefault(p.IngredientId))/r.Servings;
 public static bool HasMeat(Recipe r)=>r.Ingredients.Any(p=>p.IngredientId is "chicken" or "beef" or "turkey" or "pork");
}
