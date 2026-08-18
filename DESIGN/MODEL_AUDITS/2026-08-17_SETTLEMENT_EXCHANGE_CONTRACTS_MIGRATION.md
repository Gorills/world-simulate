# Audit — Settlement Exchange, Contracts and Migration

Audit date: **2026-08-17**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/SETTLEMENT_EXCHANGE_CONTRACTS_MIGRATION.md`

Reviewed research/model SHA: `5fd63f3a5aff3566a8d411e22595d31d7ac36f8a`

## Scope

Independent audit of the structural exchange / contracts / labour-service / migration model only. No production simulation code was reviewed or changed. Exact commodity prices, price formation, wages, board/lodging values, credit access/default rates, market frequency, enforcement procedure/costs, migration rates/distances, travel durations and newcomer-integration probabilities remain `MODEL_UNDERDEFINED` as declared by the contract.

The accepted Person/Household, Property/Tenure/Common Rights and Agricultural Year contracts were treated as dependencies. `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md` remains `MODEL_UNDERDEFINED`; this audit accepts only its stable intention -> destination -> travel seam, not its prototype one-hour resolution or schedule fixtures.

## Repository and CI

The exact reviewed SHA was branch HEAD `5fd63f3a5aff3566a8d411e22595d31d7ac36f8a` at audit time.

Required GitHub Actions on that SHA all passed:

- `ci #136` — success;
- `playable-prototype-gate #154` — success;
- `proof-a-measure #131` — success.

## Load-bearing fact re-check

### Rural exchange is ordinary but market participation is not universal

Re-checked Scott L. Waugh, “Markets and towns,” in _England in the Reign of Edward III_ (Cambridge University Press):

https://www.cambridge.org/core/books/abs/england-in-the-reign-of-edward-iii/markets-and-towns/EDF5E40731E54A08C9399CD636F6EAE2

The chapter supports a countryside connected by trade: tenants needed money for rents and commodities, smallholders could need to buy food, and village grain/goods/land were linked to towns and cities. Audit conclusion: exchange requires real counterparties and cross-settlement links rather than a closed settlement inventory.

Re-checked Kathleen Biddick, “Medieval English Peasants and Market Involvement,” _Journal of Economic History_ 45(4):

https://www.cambridge.org/core/journals/journal-of-economic-history/article/abs/medieval-english-peasants-and-market-involvement/B2E3B095E79207FF0AC83A08B5D46563

Biddick's taxation evidence shows selective/regional market involvement and commodity-specific cash-cropping patterns. Audit conclusion: the contract is correct to reject `every surplus -> market` and any universal household market-participation probability.

### Credit creates future claims and cannot be collapsed into simultaneous payment

Re-checked Chris Briggs, _Credit and Village Society in Fourteenth-Century England_, “The forms of credit and their uses” and “The credit supply” (British Academy/Oxford University Press):

https://academic.oup.com/british-academy-scholarship-online/book/21558/chapter-abstract/181401376

https://academic.oup.com/british-academy-scholarship-online/book/21558/chapter-abstract/181407204

Briggs describes medieval rural credit broadly as present benefit/service generating a future claim and identifies deferred payment and cash loans used to obtain goods/services, discharge obligations and maintain production/consumption. Audit conclusion: persistent obligations/debts are load-bearing world state. Credit availability, terms, interest/premia and default frequencies remain calibration questions and are not implied by this structural result.

### Contract formation/performance/enforcement are distinct causal states

Re-checked Chris Briggs, “Introduction: law courts, contracts and rural society in Europe, 1200–1600,” _Continuity and Change_ 29(1):

https://www.cambridge.org/core/journals/continuity-and-change/article/introduction-law-courts-contracts-and-rural-society-in-europe-12001600/187EBB92881E44508C2F1D0CD5E5C2F5

The source describes private contracts of many kinds as central to rural economies and institutions—especially courts—that facilitated registration and enforcement. Audit conclusion: the contract is justified in separating agreement formation, obligations, performance, default/dispute and optional enforcement rather than representing every contract as an instant transfer.

The source does **not** imply that every exchange was written, formally enrolled or litigated. The model correctly treats `Contract` as a simulation abstraction and leaves documentary/enforcement procedure context-specific.

Re-checked Briggs, “Seigniorial control of villagers’ litigation beyond the manor in later medieval England,” _Historical Research_ 81(213):

https://academic.oup.com/histres/article-abstract/81/213/399/5581660

The article explicitly concerns villagers enforcing contracts and recovering debts from residents of other villages through courts beyond their home manor. Audit conclusion: debt/contract relationships must not disappear when a person changes settlement, and enforcement cannot be assumed to be purely settlement-local. Exact jurisdiction choice and remedies remain underdefined.

### Labour/service agreements cannot be replaced by permanent profession classes

Re-checked Mark Bailey, “The regulation of the rural market in waged labour in fourteenth-century England,” _Continuity and Change_ (2023):

https://www.cambridge.org/core/journals/continuity-and-change/article/regulation-of-the-rural-market-in-waged-labour-in-fourteenthcentury-england/C7726EC8A2D0C628ACFF49428FDA95A7

Bailey distinguishes live-in servants paid partly with board/lodging and cash from wage labourers paid by day or piece rates. The court evidence also contains wage-debt and breach-of-service disputes. Audit conclusion: labour/service agreement, payment/maintenance obligations and finite labour commitment are separate from occupation labels. The contract correctly refuses to make `Profession` the source of employment authority, wages or work output.

The evidence does not establish one universal wage, board value, contract duration or labour-market share. Those remain `MODEL_UNDERDEFINED` and period-dependent.

### Migration is ordinary, often short-distance/rural, but constrained and not automatic integration

Re-checked Christopher Dyer, “Migration in Rural England in the Later Middle Ages,” in _Migrants in Medieval England, c.500–c.1500_ (British Academy/Oxford University Press):

https://academic.oup.com/british-academy-scholarship-online/book/37959/chapter-abstract/332491570

Dyer describes rural migration as normal/commonplace, mostly short-distance in the West Midlands evidence but with longer movements, and connects migration with mechanisms, motives and reception in new settlements. The distance observation is regional evidence, not a universal distribution.

Re-checked Christopher Dyer, _Peasants Making History_, “Peasants changing society”:

https://academic.oup.com/book/43934/chapter/370549741

The chapter connects migration with quests for land, employment and marriage and with attempts at betterment. Audit conclusion: destination choice can be driven by concrete opportunities rather than a generic wander trigger, while success is not guaranteed.

Re-checked Matt Raven, “Servile migration and seigniorial reaction in England: the serfs of Great Waltham and High Easter (Essex), c.1336–1361,” _Continuity and Change_:

https://www.cambridge.org/core/journals/continuity-and-change/article/servile-migration-and-seigniorial-reaction-in-england-the-serfs-of-great-waltham-and-high-easter-essex-c-13361361/FEE7266397005ACF06410A59D9CB33AD

Raven shows migration by villeins despite formal restrictions, with almost half of recorded migrants in the studied manors within ten miles and a sizeable majority choosing rural destinations; attempted seigniorial restriction in the 1350s was real but practically limited. Audit conclusion: status/custom may constrain departure and create consequences without becoming a universal physical immobility rule. The Essex percentages are not accepted as global simulation constants.

`arrival -> household/job/home/property` is a **design inference from accepted relationship/rights models**, not a quoted historical rule. Physical arrival can coexist with unresolved residence, employment, household or tenure relationships, so automatic integration would violate the already accepted Person/Household and Property/Tenure contracts.

## Causal model review

PASS.

Exchange/contract topology preserves cause and state:

`need/opportunity/surplus + rights + counterparties + information/context + terms -> feasible agreement/transaction -> acceptance/refusal -> immediate transfers and/or obligations -> performance/partial performance/renegotiation/default/dispute -> optional enforcement/discharge -> changed resources/claims/history`

Formation is distinct from performance. Deferred performance creates explicit obligations. Default/dispute is valid world state. A market does not synthesize liquidity or ownership.

Migration topology likewise preserves cause:

`pressures/opportunities + destination knowledge + expected social/economic basis + legal/social constraints + travel feasibility -> intention -> destination -> travel -> arrival -> ordinary relationship transitions`

Arrival alone does not fabricate household membership, employment, residence or property rights.

## Player/NPC symmetry review

PASS.

HumanController and AIController operate through the same `Person`, transfer authorization, contracts, obligations, labour capacity and migration process. Controller type grants no special prices, free inventory, unlimited credit, automatic enforcement, free housing/employment on arrival or debt erasure.

## Rights and obligations review

PASS.

The accepted Property/Tenure/Common Rights contract remains authoritative for transfer/use authority. Possession and market presence do not imply a right to sell. A contract may create a future claim without transferring current possession. Employment can authorize limited resource use without changing ownership. Debt is a claim/obligation, not negative inventory.

## Uncertainty and fixture-boundary review

PASS.

Quantitative and institution-specific gaps remain explicit `MODEL_UNDERDEFINED`: prices/price formation, market matching/frequency, credit availability/default/interest/security, enforcement costs/remedies/jurisdiction, wages/board/lodging values, migration rates/distances/profiles, travel costs/durations and newcomer integration probabilities.

Prototype settlement-global inventory, player-only trade authority, fixed `ShareRation`, guaranteed fixed prices/liquidity, automatic credit, profession-derived employment and atomic migration reset are explicitly noncanonical.

## Long-horizon review

PASS for **structural contract acceptance**, not for implemented economic/demographic balance.

Because exchange, debt, labour contracting and migration change resources, productive capacity and population distribution, implementation remains blocked from economic/demographic PASS until a future >=10 simulated-year proof exists. Required invariants include resource/money conservation at the chosen abstraction, no duplicated transfers or labour, persistent debt/default state, no person cloning on migration, no automatic integration and deterministic save/load/replay of contracts/obligations/migration state.

## Final verdict

**PASS.**

No remaining evidence, causal, symmetry, rights, uncertainty or long-horizon blocker prevents this structural contract from becoming `ACCEPTED` in its declared rural-lowland-English reference context.

`ACCEPTED` does not approve any currently underdefined price, wage, credit, enforcement, migration-rate or travel-duration calibration, does not promote P3 travel from `MODEL_UNDERDEFINED`, and does not authorize production implementation that depends materially on those unresolved values.