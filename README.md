# DinnerTable

A local, free dinner planner built with C# / ASP.NET Core 10, SQLite, and plain HTML/CSS/JavaScript. No account, subscriptions, cloud database, remote fonts or hosted services. The packaged app requires the .NET 10 ASP.NET Core runtime. A fresh GitHub clone requires the .NET 10 SDK; the launcher builds the app on first use.

## Open the app
Double-click **Start-DinnerTable.cmd**, then use http://127.0.0.1:5188. The launcher starts the app quietly, or opens the existing instance. Keep the entire outputs folder together.

Enter people (1–20), nights (1–7), and budget ($1–$2,000). Build a plan, open meal titles for instructions, swap or remove meals, check shopping items, or export the list. Changing the people or budget immediately recalculates the existing plan. Changing nights sets the next generation target; choose Build to fill it. Recipes lets you favorite or exclude dinners. History preserves the last 20 generated-plan snapshots in the interface.

## Estimates and planning
Fourteen seeded recipes, including seven meat dinners. New plans require at least 80% meat dinners (rounded up) and at least 30 g estimated protein per serving. Swaps preserve these rules. These are initial product defaults, not personalized dietary recommendations. Protein estimates use rounded generic ingredient coefficients in Nutrition.cs and vary by product and preparation. Reference: https://www.nal.usda.gov/sites/default/files/page-files/Protein.pdf. All quantities scale from four servings using canonical ingredient IDs and units. The grocery list consolidates duplicates and rounds up to whole packages. Meal costs show only the quantities consumed; the basket includes leftover package contents. All staples are included; water is excluded. Canned bean package quantities are drained weights. Costs are illustrative USD estimates, not researched or live Walmart quotes; taxes and fees are excluded.

A bounded beam search (180 candidates per depth) aims for inexpensive combined baskets with unique dinners when enough recipes are available. Favorites receive a small preference. This is a heuristic, not a proof of the cheapest possible basket. If no affordable plan is found, the amount over budget is shown; the app never promises a plan is affordable when its estimate exceeds the limit. With fewer eligible recipes or meat options than required, repeats are permitted. If every eligible meat recipe is disliked, generation reports a clear error rather than silently substituting vegetarian meals. Swaps choose a cheap alternative and can put a plan over budget, which remains visible.

## Local ownership and backup
Source: DinnerTable/. Published executable libraries: DinnerTable.App/. The launcher uses DinnerTable/ as its working directory, so **DinnerTable/Data/dinners.db** holds your recipes, ingredient catalog, preferences, current plan, grocery checks, and generated history. SQLite uses transactions and WAL. To back up, stop the app and copy the Data folder. The app listens only on the local loopback address. It does not start automatically with Windows.

## Retailer boundary
Models.cs defines IRetailerAdapter, ProductMatch, and CartPreparation. WalmartAdapter is an explicit unconnected stub; it supplies no invented product or price data. The meal engine depends on the local ingredient catalog, not Walmart. A future service can normalize retailer quotes into the catalog, track timestamps and product identifiers, then add browser/cart preparation separately. There is no assumed consumer Walmart cart API and no automatic checkout.

## Development and verification
Run `dotnet run --project DinnerTable --no-launch-profile` from this folder (stop the running app first). Build with `dotnet build DinnerTable`. Publish with `dotnet publish DinnerTable -c Release -o DinnerTable.App` while the app is stopped.

Run the dependency-free integration harness with `dotnet run --project DinnerTable.Tests`. Nineteen checks cover meat/protein constraints, sparse collections, dinner count, variety, consolidation, affordability, serving scaling, package rounding, swaps, removal, dislikes, impossible budgets, persistence, history, and invalid inputs. Tests create an isolated temporary SQLite database. The live browser was also checked for generation, swaps, removal, recipe details, desktop layout, and narrow-screen layout.

V1 does not include recipe editing/import, configurable nutritional targets, pantry deductions, live retail pricing, or ordering. The SQLite recipe catalog is persisted and can be extended through future editing features.
