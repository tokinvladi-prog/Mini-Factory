# Factory Idle

A mobile idle game: the factory generates money, the player buys and upgrades
machines, activates boosts, collects offline income, and buys currency via IAP.

The project was implemented as a test assignment in 7 stages — from pure
economy to analytics and unit tests.

## Stack

- Unity 2022.3 LTS+ (URP not required)
- TextMeshPro (UGUI)
- Unity In-App Purchasing 5.x
- NUnit (EditMode tests)

## Architecture

Three principles drive the entire structure:

1. **Gameplay logic knows nothing about Unity.** Economy, upgrades, offline,
   boost — pure C# in the `Factory.Core` assembly. Testable without a scene.
2. **Gameplay logic knows nothing about SDKs.** IAP and analytics live behind
   interfaces (`IIapService`, `IAnalyticsProvider`). `FactoryManager` contains
   no `using UnityEngine.Purchasing` and no `using Factory.Analytics`.
3. **The model is the source of truth.** UI subscribes to events — no `Update()`
   in UI scripts. The only `Update` in the project lives in `FactoryManager`.

Data flow, top to bottom: `FactoryModel` (pure economy) emits events; the
`FactoryManager` is the only consumer that runs `Update` and owns autosave,
offline, and boost; from there events go to UI (`FactoryUI`, `ShopUI`) and to
`IapCoordinator`. The coordinator drives `UnityIapService` on one side and
feeds `FactoryAnalyticsTracker` on the other, which fans events into
`AnalyticsService` and its providers.

## Features

### Economy
- Machines: `Unlock` → `Level 1`, income = `baseIncome × Level`.
- Upgrade cost scales geometrically: `baseUpgradeCost × multiplier^(Level-1)`.
- `FactoryModel.Tick(dt)` accumulates balance; all transactions are atomic.
- Events: `OnBalanceChanged / OnIncomeChanged / OnMachineChanged`.

### Persistence
- `PlayerPrefs` + `JsonUtility`, key `factory.save.v1`.
- Saved on `OnApplicationPause(true)`, `OnApplicationQuit`, plus an autosave
  every 12 seconds (only when state is dirty — tracked with a `_dirty` flag).
- Restored by machine `id`, resilient to reordering in the config.

### Offline and Boost
- Formula from the spec: boost window is consumed first, regular time is the
  remainder capped by `maxOfflineSeconds`. The long-boost edge case is handled
  explicitly — `regularEffectiveTime` never goes negative.
- Reward popup with a "Collect" button.
- `BoostController` is a pure timer with no dependencies, emits an event once
  per second.
- Boost survives a restart: remaining time is stored in the save file.

### IAP
- One consumable product: `coins_pack_small` (1000 currency).
- Fake Store in Editor — no real Google/Apple keys required.
- Two-phase purchase: `OnPurchasePending` → grant currency → `SaveNow()` →
  `ConfirmPurchase`. If the app crashes between steps, the purchase is
  replayed on next launch.

### Analytics
- 8 required events plus 5 IAP events.
- Multiple providers simultaneously (`AnalyticsService` is a fan-out).
- A `ConsoleAnalyticsProvider` is included — colored logs in Editor.
- A crashed provider never breaks the others.

### Tests
- 13 EditMode tests covering the most expensive failure modes: exponential
  upgrade cost, transaction boundaries, offline formula with boost.

## Quick Start

1. Open the `Main` scene.
2. Verify references in the Inspector:
   - `FactoryManager.config` → `FactoryConfig`
   - `IapCoordinator.catalog` → `IapCatalog` (with product `coins_pack_small`)
   - `IapCoordinator.factory` → `FactoryManager`
   - `FactoryAnalyticsTracker` → references to `FactoryManager`, `IapCoordinator`, `AnalyticsService`
3. Press Play.

In Editor, IAP works through Fake Store with no extra setup.
Right-click on `FactoryManager` → **Delete Save (debug)** to wipe progress.

## Console Output Sample

On launch, analytics logs look like this:
[Analytics] game_started { is_first_session=true, balance=0.00, income_per_second=0.00 }
[Analytics] iap_initialized { product_count=1 }
[Analytics] machine_unlocked { machine_id=press, level=1, cost=10.00 }
[Analytics] offline_income_applied { elapsed_seconds=180.00, earned=42.50, ... }
[Analytics] purchase_succeeded { product_id=coins_pack_small, reward_amount=1000.00 }

## Deliberately Not Done

- **DI framework.** Overkill at this scale. References are passed via the
  Inspector and `Bind(...)`.
- **An interface over `SaveService`.** A single `PlayerPrefs` key doesn't
  warrant an abstraction. If cloud sync or encryption is added later, an
  interface goes in then.
- **Batching/queues for analytics.** Real SDKs (Firebase, Amplitude) handle
  delivery themselves; duplicating their buffer locally buys nothing.
- **Six asmdefs for full modularity.** One `Factory.Core` + one `Tests` is the
  minimum that lets pure logic be tested.

## Versions and Gotchas

- **IAP 5.x** renames fields between minor releases (`StoreController`,
  `ProductFetchFailed`, etc.). The package version is pinned in
  `Packages/manifest.json`. All SDK work is confined to
  `Iap/UnityIapService.cs` — when the next rename lands, one file changes.
- **`double` for money everywhere.** `float` is used only for `costMultiplier`
  and `boostMultiplier` — precision is sufficient there.
