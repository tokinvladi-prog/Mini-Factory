# AI Usage

## Tools

- **Claude (Anthropic)** — main coding assistant used throughout the project.
  Worked in a chat interface with full source context; not an IDE-integrated
  agent, so every suggestion was applied by hand and compiled in Unity.
- **Unity Editor + IntelliSense** — used as the ground truth for API
  signatures. Whenever the AI guessed at a Unity IAP v5 field name, the editor
  caught it and the fix was driven by the actual compile error.

## Tasks delegated to AI

- Boilerplate for `ScriptableObject` configs, DTOs, and event plumbing.
- First draft of the economy model (`MachineModel`, `FactoryModel`) and the
  offline/boost formula from the spec.
- Draft of the save layer (`SaveService`, `SaveMapper`).
- Draft of the IAP abstraction (`IIapService`, `UnityIapService`,
  `IapCoordinator`) and its two-phase purchase flow.
- Draft of the analytics layer (`IAnalyticsProvider`, `AnalyticsService`,
  `FactoryAnalyticsTracker`, `ConsoleAnalyticsProvider`).
- Draft of the EditMode test suite.
- The HTML/CSS mockup of the main screen.
- Updating this README and AI_USAGE.md.

## Key prompts

- "Implement Stage 1: configs, models, and FactoryManager with a tick loop.
  Pure C# economy, no UI."
- "Add a save system on PlayerPrefs + JsonUtility. Save on pause and quit,
  plus autosave every 12 seconds. Don't couple models to persistence."
- "Implement offline progress and boost. Boost is consumed first, then regular
  time up to `maxOfflineSeconds`. Formula from the spec is [formula]."
- "Wrap Unity IAP so gameplay code never sees `UnityEngine.Purchasing` types.
  One consumable product, Fake Store in Editor."
- "Add a multi-provider analytics service with a console provider. Gameplay
  must not depend on any analytics SDK."
- "Write EditMode unit tests for the offline formula and transaction
  boundaries."

## AI-assisted parts

Almost all of the initial draft code was AI-generated, then reviewed and
edited line-by-line. The overall shape of the project — `Core` assembly,
event-driven model, coordinator pattern for IAP, fan-out analytics — came
from the AI, but the reasoning behind each choice was validated before
keeping it.

## Done by hand

- All Unity Editor wiring: scene hierarchy, prefab setup, asmdef placement,
  inspector references, TextMeshPro components.
- All manual testing in Play Mode: clicking through upgrades, verifying save
  round-trips, watching the offline popup, running the Fake Store purchase.
- Debugging the IAP v5 API churn. The AI confidently produced
  `IStoreListener`, `IDetailedStoreListener`, and `FailedFetchProductsInfo`
  across three iterations. Each of these was wrong for the installed package
  version; the correct names (`StoreController`, `ProductFetchFailed`,
  `FailureReason`) were found by reading the actual package sources in
  `Library/PackageCache/com.unity.purchasing@<version>/`.
- Deciding on `double` vs `float` for money, and enforcing it consistently.
- Choosing to make `ProcessPurchase` return `Pending` and confirm only after
  the currency is granted and saved.
- Fixing the asmdef layout after the AI's first suggestion put `IapCatalog`
  into `Factory.Core` and produced a cascade of "type could not be found"
  errors.
- Replacing `OfflineResult?` with an explicit `bool` flag after a nullable
  mismatch, which was cleaner and avoided further `Nullable<T>` surprises.

## AI suggestions changed or rejected

- **Nullable `OfflineResult?`** — AI's initial proposal. Rejected in favor of
  an explicit `_hasPendingOfflineResult` flag. Fewer edge cases, works on any
  C# version, and no hidden `default(OfflineResult)` semantics.
- **Batching / retry queue for analytics** — proposed "for robustness".
  Rejected: a console-only provider doesn't need it, and real SDKs handle
  their own delivery.
- **`ISaveService` interface** — proposed "to keep the option open". Rejected
  as overkill for a single PlayerPrefs key.
- **Full asmdef modularity** (six assemblies) — proposed. Rejected in favor
  of `Factory.Core` + `Factory.Tests.EditMode`, which is the minimum that
  allows pure logic to be tested.
- **Boost window not clamped by `maxOfflineSeconds`** — AI followed the
  formula from the spec literally. Changed to clamp the boost window by the
  offline cap, so a long boost doesn't pay 2× income for time beyond the
  offline limit. Documented in a comment.
- **Analytics as a static singleton** — proposed in the first draft.
  Replaced with a scene object passed through the inspector, consistent with
  the rest of the project's no-singleton stance.

## AI mistakes and questionable decisions caught in review

- **IAP v5 API guesses.** Three separate iterations were wrong:
  `IDetailedStoreListener` (obsolete in v5), `IStoreController` (obsolete
  spelling of `StoreController`), and `ProductFetchFailed.Message`
  (field name doesn't exist). None of these were "fixable" by the AI without
  seeing the actual package source. Compiler errors plus reading
  `com.unity.purchasing@<version>/Runtime/` were the only way forward.
- **asmdef placement.** AI put `IapCatalog.cs` under `Factory.Core/Configs/`,
  which broke compilation because `IapProductDefinition` lives in the
  predefined `Assembly-CSharp`. Moved `IapCatalog.cs` back to the IAP folder.
- **`float` vs `double` in `OnPurchaseCompleted`.** AI produced
  `Action<IapPurchaseInfo, float>` while the rest of the project used
  `double` for money. Standardized on `double`.
- **Missing events on `IapCoordinator`.** AI's Stage 6 tracker subscribed to
  `OnProductFetched` and `OnPurchaseFailed` on `IapCoordinator`, but those
  events were never declared in the Stage 5 draft of the coordinator. Added
  them and widened `OnPurchaseCompleted` to carry the reward amount.
- **Nullable field signature drift.** AI wrote `OfflineResult?` in one place
  and `OfflineResult` in another, which caused a compile error that looked
  like a missing extension method.

## Overall assessment

AI was a productive first-draft generator, especially for boilerplate-heavy
code (DTOs, event wiring, test scaffolding) and for the offline/boost formula
which was given precisely. It was unreliable on library APIs whose exact
signatures change between minor versions — every IAP 5.x issue was caught by
the compiler, not by the AI. The parts of the project that carry the most
risk (transaction semantics, offline cap, two-phase purchase, asmdef
boundaries) were all reviewed and adjusted by hand, and every adjustment is
documented in code comments or in this file.
