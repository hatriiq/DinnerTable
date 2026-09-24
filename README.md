# DinnerTable

A locally owned dinner planner built with C# / ASP.NET Core 10, SQLite and plain HTML/CSS/JavaScript. No paid application services, cloud database, or account needed for planning.

## Start
Double-click **Start-DinnerTable.cmd** on Windows, or run `dotnet run --project DinnerTable --no-launch-profile`, then open http://127.0.0.1:5188. A fresh clone requires the .NET 10 SDK; the Windows launcher publishes the app on first use. Published builds require the .NET 10 ASP.NET Core runtime.

## Plan by calendar week
Choose a date or use Previous/Next week. Click a day to search the recipe collection, filter meat or meatless options, and assign dinner. Every week saves its own menu, serving count, budget and grocery checks. Adding or replacing a dinner immediately recalculates ingredients and cost. Open a dinner card for scaled ingredients and directions.

Build my dinner plan generates the requested number of dinners. If the current menu already has that many dates assigned, those dates are preserved; otherwise the generated plan fills consecutive days from Monday. Manual calendar choices are honored, including meatless meals. Favorites/dislikes follow you across weeks. History contains generated-plan snapshots; the calendar stores subsequent edits to each week.

## Recipes and budget
Exactly **50 recipes: 47 meat dinners and 3 meatless options**. The collection includes chicken, beef, turkey and pork. All seeded recipes have at least 30 g of estimated protein per serving. Automatic generation uses a 94% meat preference rounded up for short weeks, so a 1–7-night automatic plan is all meat; use the calendar for an occasional meatless choice.

Protein estimates use rounded generic ingredient values in Nutrition.cs, not verified retail nutrition labels. Reference: [USDA protein table](https://www.nal.usda.gov/sites/default/files/page-files/Protein.pdf). Product brands and preparation can change these values.

The grocery engine scales quantities from four servings, consolidates ingredients and rounds up to whole packages. Meal costs count quantities consumed. The planning basket includes whole packages and leftovers. Oil and seasoning are included; water is not. Canned-bean quantities use estimated drained weights.

Planning prices are illustrative USD estimates. The bounded search favors inexpensive combined baskets, with a small favorite preference; it is not guaranteed to find a global minimum. Over-budget results remain clearly labeled. Too few eligible recipes permit repeats. Dislikes are excluded. Estimated prices exclude taxes, fees and tips.

## Walmart
The **Walmart** tab contains suggested product matches for every catalog ingredient, package quantities, editable store settings, product links and a price editor. Product IDs/package labels were checked against Walmart pages in September 2026. Suggestions are not live inventory guarantees. Review variable-weight meat and produce, and drained-weight assumptions, before adding to your cart.

After reviewing all products, **Add list to Walmart cart** opens Walmart's documented public [Add To Cart service](https://walmart.io/docs/atc/v1/add-to-cart), passing item IDs, calculated quantities and an optional store ID. It does not purchase anything. Walmart may require sign-in, report unavailable items, or ask you to confirm fulfillment. Reopening the link can add quantities again; use Open existing Walmart cart after the first handoff.

This is a browser handoff, not an authenticated consumer ordering API. DinnerTable cannot automatically read your Walmart cart, guarantee stock, or retrieve live local prices. Save observed package prices in the app with their store/context; unknown prices are never treated as a complete quote. The Walmart subtotal is separate from the planner's illustrative estimate. Final price, inventory, substitutions, pickup/delivery, fees and checkout approval stay with Walmart.

IRetailerAdapter separates retailer matching and cart construction from MealEngine. Future authenticated pricing/availability services can implement the interface without changing recipe calculations. No Walmart passwords, cookies or payment credentials are stored.

## Data and backup
DinnerTable/Data/dinners.db stores recipes, preferences, dated weekly plans, generated history, retailer settings and saved product matches/prices. The catalog migration preserves a copy of the previous catalog and retains existing menus. Stop the app and copy the Data folder to back it up. The application listens only on local loopback.

Git ignores the database, compiled output, logs, process IDs and development artifacts. Source is in DinnerTable/; the Windows launcher runs DinnerTable.App/ with DinnerTable/ as its working directory. Updates require rebuilding the published app while it is stopped.

## Development
- Build: `dotnet build DinnerTable`
- Run checks: `dotnet run --project DinnerTable.Tests`
- Publish: `dotnet publish DinnerTable -c Release -o DinnerTable.App`

The integration harness uses an isolated temporary SQLite database and checks recipe composition, ingredient integrity, protein rules, scaling, package rounding, swaps/removal, dislikes, invalid inputs, persistence, weekly isolation, calendar choices and Walmart handoff construction. Browser checks cover the calendar picker, week switching, mobile layout and Walmart review.

Not included: pantry deductions, custom recipe editing/import, automatic local price refresh, automatic availability verification, or checkout automation.
