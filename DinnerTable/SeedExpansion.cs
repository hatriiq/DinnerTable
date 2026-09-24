namespace DinnerTable;
public static class SeedExpansion
{
    private static Portion P(string id, decimal quantity) => new(id, quantity);
    public static List<Ingredient> ExtraIngredients => [
        new("garlic","Garlic","Produce","g",85,0.68m), new("pepper","Bell peppers","Produce","g",450,2.48m),
        new("broccoli","Frozen broccoli","Frozen","g",340,1.16m), new("cabbage","Green cabbage","Produce","g",900,1.98m),
        new("greenbeans","Frozen green beans","Frozen","g",340,0.98m), new("salsa","Salsa","Grains & pantry","g",454,1.98m),
        new("bbq","Barbecue sauce","Grains & pantry","g",510,1.48m),new("mustard","Dijon mustard","Grains & pantry","g",340,1.98m),
        new("honey","Honey","Grains & pantry","g",340,3.48m),new("yogurt","Plain Greek yogurt","Dairy","g",907,3.54m),
        new("milk","Milk","Dairy","ml",1893,2.48m),new("butter","Butter","Dairy","g",454,3.98m),
        new("breadcrumb","Breadcrumbs","Grains & pantry","g",425,1.98m),new("lemon","Lemon juice","Grains & pantry","ml",443,1.98m)
    ];
    public static List<Recipe> Build(List<Recipe> original)
    {
        var result=original.ToList();
        void Revise(string id,string name,string description,Portion[] additions,string? step=null) {
            var index=result.FindIndex(r=>r.Id==id);var old=result[index];
            result[index]=old with{Name=name,Description=description,Ingredients=old.Ingredients.Concat(additions).ToList(),Steps=step==null?old.Steps:old.Steps.Prepend(step).ToArray()};
        }
        Revise("bean-bowls","Black bean, egg & cheddar bowls","One of three meatless choices: beans, eggs and cheddar over rice.",[P("egg",4),P("cheese",200)],"Scramble eggs until set and divide with the cheese among the finished bowls.");
        Revise("quesadilla","Extra-protein black bean quesadillas","A meatless option with generous beans and melted cheddar.",[P("beans",255),P("cheese",46)]);
        Revise("tomato-pasta","Cheesy tomato & lentil pasta","A meatless pasta dinner with lentils and cheddar.",[P("lentil",80),P("cheese",100)],"Reserve the cheddar and stir into the finished hot pasta.");
        Revise("fried-rice","Chicken & vegetable fried rice","Tender chicken, egg and vegetables tossed with savory rice.",[P("chicken",650)],"Dice and cook chicken in a large skillet to 165°F / 74°C. Set aside, then stir into the rice at the end.");
        Revise("chickpea-stew","Chicken & chickpea tomato stew","A hearty chicken dinner with chickpeas and rice.",[P("chicken",650)],"Dice and cook chicken to 165°F / 74°C. Add it to the stew during the final simmer.");
        Revise("loaded-potato","Beef & cheese loaded potatoes","Baked potatoes loaded with ground beef, beans and cheddar.",[P("beef",650)],"Brown ground beef until it reaches 160°F / 71°C. Drain excess fat and fold into the bean topping.");
        Revise("lentil-soup","Turkey & lentil soup","A filling pot of ground turkey, lentils and vegetables.",[P("turkey",650)],"Brown ground turkey to 165°F / 74°C in the soup pot before adding vegetables and the remaining ingredients.");
        void Add(string id,string name,string meat,string starch,string veg,string flavor,int minutes=30) {
            var ingredients=new List<Portion>{P(meat,700),P(starch,starch=="tortilla"?8:starch=="potato"?800:300),P(veg,340),P("onion",150),P("oil",15),P("spice",8)};
            string sauce;
            switch(flavor){
                case "tomato": ingredients.Add(P("tomato",411));ingredients.Add(P("garlic",10));sauce="Add diced tomatoes and minced garlic; simmer 10 minutes.";break;
                case "salsa": ingredients.Add(P("salsa",200));ingredients.Add(P("cheese",100));sauce="Stir in salsa, simmer 5 minutes and finish with cheddar.";break;
                case "bbq": ingredients.Add(P("bbq",140));sauce="Stir in barbecue sauce with a splash of water and heat gently.";break;
                case "soy": ingredients.Add(P("soy",40));ingredients.Add(P("garlic",10));sauce="Toss with soy sauce and minced garlic; stir-fry for 2 minutes.";break;
                case "honey": ingredients.Add(P("honey",40));ingredients.Add(P("soy",30));ingredients.Add(P("garlic",10));sauce="Stir in honey, soy sauce and minced garlic with a splash of water. Simmer 3 minutes.";break;
                case "mustard": ingredients.Add(P("mustard",50));ingredients.Add(P("yogurt",200));sauce="Remove the pan from heat and stir in mustard and yogurt, loosening with warm water as needed.";break;
                case "lemon": ingredients.Add(P("lemon",40));ingredients.Add(P("garlic",10));sauce="Stir in lemon juice and minced garlic; simmer 2 minutes with a splash of water.";break;
                default: ingredients.Add(P("milk",200));ingredients.Add(P("cheese",150));sauce="Lower heat, stir in milk and cheddar, and heat gently until melted.";break;
            }
            string carb=starch switch {"rice"=>"Cook rice according to its package.","pasta"=>"Cook pasta according to its package; reserve a little cooking water.","potato"=>"Cut potatoes into 2 cm cubes, toss with half the oil and roast at 425°F / 220°C for 25–35 minutes until tender.",_=>"Warm tortillas just before serving."};
            string prep=meat is "beef" or "turkey"?"Break up the ground meat":"Cut the boneless meat into bite-size pieces";
            string temp=meat is "chicken" or "turkey"?"165°F / 74°C":meat=="beef"?"160°F / 71°C":"145°F / 63°C, then allow a 3-minute rest";
            string finish=starch=="tortilla"?"Spoon the filling into warm tortillas.":starch=="pasta"?"Toss with the cooked pasta, loosening with reserved cooking water.":"Serve the meat and vegetables over the cooked side.";
            result.Add(new(id,name,$"A generous serving of {meat} with {veg switch{"veg"=>"mixed vegetables","greenbeans"=>"green beans",_=>veg}} and {starch switch{"tortilla"=>"warm tortillas",_=>starch}}.","Protein focused",minutes,4,ingredients,[carb,$"{prep}. Cook in oil with diced onion and seasoning until the meat reaches {temp}. Set aside.",$"Chop fresh vegetables or use frozen straight from the bag. Cook in the same pan with a splash of water until tender and hot throughout. Return meat to the pan. {sauce}",finish]));
        }
        Add("chicken-broccoli-rice","Garlic chicken & broccoli rice","chicken","rice","broccoli","soy");
        Add("chicken-salsa-bowls","Salsa chicken rice bowls","chicken","rice","corn","salsa");
        Add("chicken-bbq-potato","BBQ chicken & potato plates","chicken","potato","greenbeans","bbq",40);
        Add("chicken-lemon-pasta","Lemon garlic chicken pasta","chicken","pasta","broccoli","lemon");
        Add("chicken-cheddar-pasta","Cheddar chicken & vegetable pasta","chicken","pasta","veg","cheese");
        Add("chicken-honey-rice","Honey garlic chicken bowls","chicken","rice","carrot","honey");
        Add("chicken-mustard-potato","Creamy mustard chicken & potatoes","chicken","potato","greenbeans","mustard",40);
        Add("chicken-pepper-wrap","Salsa chicken & pepper wraps","chicken","tortilla","pepper","salsa");
        Add("chicken-tomato-rice","Tomato garlic chicken rice","chicken","rice","pepper","tomato");
        Add("chicken-bbq-wrap","BBQ chicken & cabbage wraps","chicken","tortilla","cabbage","bbq");
        Add("beef-broccoli","Beef & broccoli rice bowls","beef","rice","broccoli","soy");
        Add("beef-salsa","Beef salsa rice skillet","beef","rice","corn","salsa");
        Add("beef-tacos","Cheesy beef & pepper tacos","beef","tortilla","pepper","salsa");
        Add("beef-cabbage","Beef & cabbage tomato skillet","beef","rice","cabbage","tomato");
        Add("beef-cheddar-pasta","Cheeseburger-style vegetable pasta","beef","pasta","veg","cheese");
        Add("beef-bbq-potatoes","BBQ beef & roasted potatoes","beef","potato","carrot","bbq",40);
        Add("beef-greenbean-pasta","Beef & green bean tomato pasta","beef","pasta","greenbeans","tomato");
        Add("beef-honey-bowls","Sweet garlic beef & carrot bowls","beef","rice","carrot","honey");
        Add("beef-mustard-potatoes","Mustard beef & potato skillet","beef","potato","broccoli","mustard",40);
        Add("beef-bbq-wraps","BBQ beef & corn wraps","beef","tortilla","corn","bbq");
        Add("turkey-taco-bowls","Turkey taco rice bowls","turkey","rice","corn","salsa");
        Add("turkey-broccoli-pasta","Cheesy turkey & broccoli pasta","turkey","pasta","broccoli","cheese");
        Add("turkey-cabbage","Turkey & cabbage stir-fry bowls","turkey","rice","cabbage","soy");
        Add("turkey-bbq-wraps","BBQ turkey & pepper wraps","turkey","tortilla","pepper","bbq");
        Add("turkey-tomato-potatoes","Tomato turkey & potato plates","turkey","potato","greenbeans","tomato",40);
        Add("turkey-lemon-rice","Lemon garlic turkey & vegetable rice","turkey","rice","veg","lemon");
        Add("turkey-mustard-pasta","Creamy mustard turkey pasta","turkey","pasta","greenbeans","mustard");
        Add("turkey-honey-rice","Honey garlic turkey & broccoli bowls","turkey","rice","broccoli","honey");
        Add("pork-broccoli","Pork & broccoli stir-fry rice","pork","rice","broccoli","soy");
        Add("pork-bbq-wraps","BBQ pork & cabbage wraps","pork","tortilla","cabbage","bbq");
        Add("pork-honey","Honey garlic pork rice bowls","pork","rice","carrot","honey");
        Add("pork-mustard","Mustard pork & roasted potatoes","pork","potato","greenbeans","mustard",40);
        Add("pork-salsa","Salsa pork & pepper tacos","pork","tortilla","pepper","salsa");
        Add("pork-tomato-pasta","Tomato garlic pork pasta","pork","pasta","veg","tomato");
        Add("pork-lemon","Lemon pork & broccoli potatoes","pork","potato","broccoli","lemon",40);
        Add("pork-cheddar-rice","Cheddar pork & corn rice skillet","pork","rice","corn","cheese");
        return result;
    }
}
