# Audit — Settlement Agricultural Year, Seed, Labour and Livestock

Audit date: **2026-08-17**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/SETTLEMENT_AGRICULTURAL_YEAR_SEED_LABOUR_LIVESTOCK.md`

Reviewed research/model SHA: `d92800481c9ba023f829a5ad81d44e712895dbdf`

Repair SHA reviewed after blockers: `2a9439329a41a631ce83d484d2ae1b0f4e1a95ef`

## Scope

Independent audit of the structural agricultural-year / seed / labour / livestock model only. No production simulation code was reviewed or changed. Exact crop mixes, rotations, sowing/harvest windows, seed rates, yields, labour coefficients, herd distributions, livestock reproduction/productivity, fodder yields, grazing capacities and storage-loss rates remain `MODEL_UNDERDEFINED` as declared by the contract.

## Repository and CI

The reviewed repair was the exact branch HEAD `2a9439329a41a631ce83d484d2ae1b0f4e1a95ef` at audit time. The repair changed only two lines in the agricultural model contract: it corrected the Templar-estates source attribution and narrowed the seed requirement from all harvests to sown arable crops.

Required GitHub Actions on the repair SHA all passed:

- `ci #132` — success;
- `playable-prototype-gate #146` — success;
- `proof-a-measure #127` — success.

## Load-bearing fact re-check

### Seasonal/rotation state constrains tasks; it does not assign personal activity

Re-checked Helena Hamerow, Amy Bogaard, Michael Charles and Richard Thomas, _Feeding Medieval England: A Long ‘Agricultural Revolution’, 700–1300_, chapter “Crop Rotation and Seasonal Sowing” (Oxford University Press, 2025):

https://academic.oup.com/book/61548/chapter/537298429

The source supports structured seasonal sowing and fallow/rotation states while also stressing uncertainty and local variation in the spread and organization of systematic rotation. Audit conclusion: the model is justified in representing season/calendar as task feasibility/urgency context and in rejecting a universal `date -> Farmer.Work()` or mandatory national three-field script.

The source does **not** establish one crop calendar, field arrangement or rotation for every settlement. Those remain calibration questions.

### Seed is a material input to sown cereal production, not automatic replenishment

Re-checked the same Oxford synthesis, including its discussion of yield per grain sown and seed as an input to cereal production:

https://academic.oup.com/book/61548/chapter/537305187

Audit conclusion: explicit seed continuity is a load-bearing requirement for sown arable crops. Gross cereal harvest cannot be equated to freely disposable household surplus when future sowing requires retained or acquired seed. The contract correctly refuses to turn the cited yield ratios into a universal numerical multiplier.

### Repair check: seed requirement must not be universalized to meadow hay

Re-checked Hamerow et al., “Agricultural Land Use, c.AD 300–1500,” which distinguishes pasture managed by grazing from hay meadow managed by mowing and describes meadows as sources of harvested hay/fodder:

https://academic.oup.com/book/61548/chapter/537304152

Also re-checked Christopher Dyer, “Partnership among peasants: rural England, 1270–1520,” which discusses meadow management and grazing after hay had been cut and carried:

https://www.cambridge.org/core/journals/continuity-and-change/article/partnership-among-peasants-rural-england-12701520/F8DB6A2A76E46C44687718E4FDEA8CC8

Audit conclusion: the repair is correct. The seed rule belongs to sown arable crop processes; other outputs such as meadow hay require their own causal land/resource process and must not inherit a seed requirement merely because they are harvested.

### Repair check: Templar-estates attribution and draft/feed coupling

Re-checked Cambridge Core for _The Templar Estates in Lincolnshire, 1185–1565_. Cambridge identifies **J. Michael Jefferson** as the author of the book and its chapters, including the 1308–13 former Templar-estates material:

https://www.cambridge.org/core/books/templar-estates-in-lincolnshire-11851565/941EFC2AB177ABA0A6A3EEB94988BD4D

https://www.cambridge.org/core/books/templar-estates-in-lincolnshire-11851565/lincolnshire-preceptories-and-the-former-templar-estates-130813/452C3BC73E14538219288DD6CF461B45

The chapter on livestock, excluding sheep, remains the evidence used by the contract for ox/horse draft and haulage roles and the dependence of livestock maintenance on provender:

https://www.cambridge.org/core/books/abs/templar-estates-in-lincolnshire-11851565/livestock-excluding-sheep-on-the-former-templar-estates-130813/9DBFCFCF9A1985BF298DD8673558C0BF

Audit conclusion: changing the attribution from J. R. S. Phillips to J. Michael Jefferson fixes the bibliographic blocker without changing the supported causal claim. The estate evidence does not establish universal peasant ownership rates, team sizes, ox/horse ratios or feed quantities; the contract preserves those limits.

### Livestock, pasture/fodder and arable fertility are coupled but regionally variable

Re-checked Hamerow et al., “The Intensity of Cultivation: Soil Fertility and the Expansion of Arable”:

https://academic.oup.com/book/61548/chapter/537294899

The source supports hay/fodder feeding, grazing, manure and the maintenance needs of working animals as coupled husbandry processes. The contract correctly uses this as structural evidence rather than a quantitative manure/fodder function.

The accepted Property/Tenure/Common Rights contract remains authoritative for lawful pasture/common access; ecological forage availability is a separate constraint from having a legal right.

### Labour regime is not a fixed profession table and changes across the Black Death

Re-checked Mark Bailey, “The regulation of the rural market in waged labour in fourteenth-century England,” _Continuity and Change_ (2023):

https://www.cambridge.org/core/journals/continuity-and-change/article/regulation-of-the-rural-market-in-waged-labour-in-fourteenthcentury-england/C7726EC8A2D0C628ACFF49428FDA95A7

The source supports a significant hired rural labour market before the Black Death and a substantial post-plague change in labour scarcity, wages, mobility and regulation. Audit conclusion: labour must be allocatable across household, hired/service/obligation sources rather than generated by a `Farmer` class, and 1270–1348 should not be averaged into 1350–1450 as one timeless coefficient.

## Causal model review

PASS.

The accepted topology is:

`household/person pressures + rights + parcel/crop state + season/weather + seed + labour + skills + tools/draft capacity + obligations/opportunities -> feasible tasks -> controller choice/allocation -> action/process -> parcel/crop/livestock/resource consequences -> storage/reservations/obligations -> next-cycle options`

Cause precedes state. Calendar changes feasibility and urgency rather than fabricating motives. Crop output requires process provenance. Sown arable crops consume real seed. Labour and draft capacity are finite. Livestock consume maintenance resources. Failure/shortfall is a valid outcome rather than a hidden balancing error.

## Player/NPC symmetry review

PASS.

HumanController and AIController operate through the same ordinary `Person`, rights, task requirements and resource constraints. Player control does not create seed, labour, tools, draft power, land/common access or guaranteed crop success.

## Rights and obligations review

PASS.

The model depends on the accepted Property/Tenure/Common Rights contract. Cultivation, harvest/removal and grazing authorization remain explicit and action-specific. Agricultural output is not automatically routed into settlement-global ownership.

## Uncertainty and fixture-boundary review

PASS.

All load-bearing quantitative calibration gaps remain explicit `MODEL_UNDERDEFINED`, including crop/rotation configuration, yields, seed rates, sowing/harvest windows, labour coefficients, draft prevalence, herd demography, fodder/carrying capacity and storage loss.

Prototype fixtures such as `Profession.Farmer -> Grain`, `08:00 -> Working`, infinite seed, fixed settlement stock and universal common pasture are explicitly noncanonical.

## Long-horizon review

PASS for **structural contract acceptance**, not for implemented economic balance.

Because the model changes productive capacity and resource balance, implementation remains blocked from economic PASS until the Reality Modeling Policy's >=10 simulated-year proof exists. The contract correctly requires seed conservation, crop provenance, finite labour/draft capacity, livestock maintenance, rights checks and persistent save/load/replay state over that horizon.

## Final verdict

**PASS.**

The two blockers found in the first audit pass are repaired and independently re-checked. No remaining structural or evidence blocker prevents this contract from becoming `ACCEPTED` in its declared rural-lowland-English reference context.

`ACCEPTED` does not approve any currently underdefined numerical agricultural calibration and does not authorize production implementation that would need those unresolved numbers.