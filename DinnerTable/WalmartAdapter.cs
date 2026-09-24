using System.Text.RegularExpressions;
namespace DinnerTable;
public interface IRetailerAdapter { RetailerBasket BuildBasket(PlanState state,IReadOnlyList<GroceryLine> groceries); }
public record RetailerSettings(string Zip="",string StoreId="",string StoreName="",string Fulfillment="Choose at Walmart");
public record RetailerMatch(string IngredientId,string ProductId,string Name,decimal PackageQuantity,string Unit,decimal? Price=null,string PriceScope="Not checked",DateTimeOffset? ObservedAt=null,string Note="",bool Confirmed=false);
public record RetailerLine(GroceryLine Ingredient,RetailerMatch? Match,int Quantity,decimal? Cost,string SearchUrl);
public record RetailerBasket(RetailerSettings Settings,List<RetailerLine> Lines,int Unmatched,int NeedsReview,decimal KnownSubtotal,bool CompletePricing,string? CartUrl);
public sealed class WalmartAdapter(Store db):IRetailerAdapter {
 public RetailerBasket BuildBasket(PlanState state,IReadOnlyList<GroceryLine> groceries){
  var settings=db.ReadDocument("walmart-settings",new RetailerSettings());
  var matches=db.ReadDocument("walmart-matches",WalmartProducts.Seeds);
  var lines=groceries.Select(g=>{var match=matches.FirstOrDefault(m=>m.IngredientId==g.Id&&m.Unit==g.Unit&&m.PackageQuantity>0);var n=match==null?0:(int)Math.Ceiling(g.Quantity/match.PackageQuantity);return new RetailerLine(g,match,n,match?.Price*n,"https://www.walmart.com/search?q="+Uri.EscapeDataString(g.Name));}).ToList();
  int unmatched=lines.Count(l=>l.Match==null);int review=lines.Count(l=>l.Match is {Confirmed:false});
  var grouped=lines.Where(l=>l.Match!=null).GroupBy(l=>l.Match!.ProductId).Select(g=>g.Key+"_"+g.Sum(l=>l.Quantity));
  var url=unmatched==0&&lines.Count>0?"https://www.walmart.com/sc/cart/addToCart?items="+Uri.EscapeDataString(string.Join(',',grouped))+(string.IsNullOrWhiteSpace(settings.StoreId)?"":"&storeId="+settings.StoreId):null;
  return new(settings,lines,unmatched,review,lines.Sum(l=>l.Cost??0),lines.Count>0&&lines.All(l=>l.Cost!=null),url);
 }
 public static void ValidateSettings(RetailerSettings s){if(!Regex.IsMatch(s.Zip??"",@"^\d{5}$")||(!string.IsNullOrEmpty(s.StoreId)&&!Regex.IsMatch(s.StoreId,@"^\d{1,8}$"))||s.StoreName.Length>150||s.Fulfillment is not ("Pickup" or "Delivery" or "Choose at Walmart"))throw new ArgumentException("Enter a five-digit ZIP, valid store number and pickup/delivery preference.");}
 public static void ValidateMatch(RetailerMatch m,IReadOnlyList<Ingredient> ingredients){var ingredient=ingredients.SingleOrDefault(i=>i.Id==m.IngredientId);if(ingredient==null||!Regex.IsMatch(m.ProductId??"",@"^\d{1,20}$")||string.IsNullOrWhiteSpace(m.Name)||m.Name.Length>200||m.PackageQuantity<=0||m.PackageQuantity>100000||m.Unit!=ingredient.Unit||m.Price is <=0 or >10000||m.Note.Length>500||m.PriceScope.Length>150)throw new ArgumentException("Check product ID, package quantity, matching unit and optional package price.");}
}
public static class WalmartProducts {
 // Item identities/package labels checked on Walmart product pages. These are suggested
 // matches, not inventory promises. No price is invented or treated as a live local quote.
 private static RetailerMatch M(string ingredient,string id,string name,decimal amount,string unit="g",string note="")=>new(ingredient,id,name,amount,unit,Note:note);
 public static List<RetailerMatch> Seeds => [
 M("rice","10315394","Great Value Long Grain Enriched Rice, 32 oz",907),
 M("beans","10534038","Great Value Black Beans, 15 oz",255,note:"255 g estimated drained yield per can; check drained quantity."),
 M("cheese","10452370","Great Value Mild Cheddar Finely Shredded Cheese, 8 oz",226),
 M("chicken","124782654","Freshness Guaranteed Boneless Skinless Chicken Thighs, family pack",1179,note:"Variable 2.6–6.9 lb tray: quantity uses the advertised minimum 2.6 lb. Review actual weight and final price in Walmart."),
 M("egg","145051970","Great Value Large White Eggs, 12 Count",12,"each"),
 M("veg","817042496","Great Value Mixed Vegetables, 12 oz",340),
 M("tortilla","953466267","Great Value Medium Flour Tortillas, 10 Count",10,"each"),
 M("carrot","44391515","Fresh Whole Carrots, 1 lb",454),
 M("onion","10447842","Fresh Yellow Onions, 3 lb",1361),
 M("potato","10447837","Fresh Russet Potatoes, 5 lb",2268),
 M("oil","10451002","Great Value Vegetable Oil, 48 fl oz",1420,"ml"),
 M("soy","10315653","Great Value Naturally Brewed Soy Sauce, 15 fl oz",444,"ml"),
 M("spice","168168071","Great Value Seasoned Salt, 16 oz",454,note:"Salt-based all-purpose blend; season to taste, especially alongside soy sauce."),
 M("corn","659879040","Great Value Whole Kernel Corn, 12 oz",340),
 M("pasta","10534084","Great Value Penne Pasta, 16 oz",454),
 M("tomato","10450990","Great Value Diced Tomatoes, 14.5 oz",411),
 M("garlic","367014931","Great Value Minced Garlic in Water, 8 oz",226),
 M("broccoli","967902497","Great Value Broccoli Cuts, 12 oz",340),
 M("pepper","47770124","Fresh Bell Peppers, 3 Count",450,note:"Estimated 450 g usable weight per three-pack; check size."),
 M("cabbage","44391042","Fresh Green Cabbage, Each",900,note:"Estimated 900 g usable weight; final price and weight vary."),
 M("greenbeans","576704015","Great Value Cut Green Beans, 12 oz",340),
 M("salsa","1959652540","Great Value Thick and Chunky Mild Salsa, 16 oz",454),
 M("bbq","593927194","Great Value Original BBQ Sauce, 18 oz",510),
 M("mustard","10315545","Great Value Dijon Mustard, 12 oz",340),
 M("honey","20647992","Great Value Honey, 12 oz",340),
 M("yogurt","26559565","Great Value Plain Nonfat Greek Yogurt, 32 oz",907),
 M("milk","10450118","Great Value Whole Milk, Half Gallon",1893,"ml"),
 M("butter","132893363","Great Value Salted Butter, 16 oz",454),
 M("breadcrumb","10315089","Great Value Plain Bread Crumbs, 15 oz",425),
 M("lemon","1056763979","Great Value Lemon Juice, 15 fl oz",443,"ml"),
 M("lentil","10314942","Great Value Lentils, 16 oz",454),
 M("chickpea","10534041","Great Value Garbanzos Chick Peas, 15.5 oz",255,note:"Estimated drained yield per can; verify after draining."),
 M("turkey","167794433","Jennie-O Ground Turkey 93/7, 1 lb",454),
 M("beef","18892154161","90% Lean Ground Beef Sirloin, 1 lb",454),
 M("pork","20934757","Smithfield Boneless Pork Center Cut Loin, 3–6.25 lb",1360,note:"Variable-weight roast; quantity uses minimum advertised 3 lb. Review final weight and price.")
 ];
}
