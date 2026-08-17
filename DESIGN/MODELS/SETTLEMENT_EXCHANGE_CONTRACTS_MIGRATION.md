# Settlement Exchange, Contracts and Migration — Model Contract

Status: **REVIEW_REQUIRED**

This contract defines the minimum causal structure for exchange, payment, credit/debt, private contracts, labour/service agreements and migration between settlements/households. It does **not** define universal prices, wage levels, interest rates, credit limits, contract-form probabilities, market-day frequencies, migration rates, travel speeds or settlement-specific legal procedure; those remain calibration/context questions and are `MODEL_UNDERDEFINED` where material.

## Mechanic

Represent exchange as transfers and obligations between explicit counterparties who hold recognized rights to what they offer, rather than as a magical settlement stockpile or a special player barter screen.

Represent a contract as a persistent agreement that creates explicit obligations which may be performed, partially performed, discharged, breached/defaulted, disputed, renegotiated or enforced through an applicable institution.

Represent migration as an ordinary `Person` changing residence/work/household/tenure relationships because pressures and opportunities make relocation worthwhile and feasible. Migration is not `SettlementId = destination`, and arrival alone grants no residence, household, employment, inventory or property rights.

## Intended feeling

The settlement economy should feel like people and households dealing with other people, households and institutions under real constraints:

- a household short of seed can seek a seller, lender or creditor rather than receiving automatic replenishment;
- a worker can accept day work, piece work or service under explicit terms rather than being permanently typed as `Farmer`;
- a buyer may receive goods now and owe payment later;
- failure to perform can create a debt/dispute rather than silently deleting the obligation;
- an individual may move because work, land, marriage, service or other prospects are better elsewhere, but must still secure ordinary residence/work/access arrangements;
- the human-controlled person faces exactly the same counterparties, prices/terms, obligations, legal constraints and migration consequences as AI-controlled people.

## Dependencies and boundary with accepted models

This contract builds on three accepted foundations:

- `DESIGN/MODELS/SETTLEMENT_PERSON_HOUSEHOLD_LIFECYCLE.md` — persistent ordinary `Person`, household membership, service/life-cycle transitions and controller symmetry;
- `DESIGN/MODELS/SETTLEMENT_PROPERTY_TENURE_COMMON_RIGHTS.md` — explicit possession/use/transfer rights, holdings and obligations;
- `DESIGN/MODELS/SETTLEMENT_AGRICULTURAL_YEAR_SEED_LABOUR_LIVESTOCK.md` — finite resources, labour allocation, seed continuity and the need to acquire shortages through ordinary exchange/contracts.

`DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md` remains **MODEL_UNDERDEFINED**. This contract may rely on its stable infrastructure idea that people occupy semantic places and travel because an intention creates a destination, but it does **not** accept its temporary one-hour travel resolution or current schedule fixtures as migration law.

Migration implementation must therefore preserve an explicit travel seam and must not use this contract as permission to teleport a person or reset their relationships.

## Reference context

Baseline: rural lowland England, approximately **1270–1348** for first calibration, with **1350–1450** retained as a separate stress/validation regime because labour supply, bargaining conditions, land availability, mobility and regulation changed materially after the Black Death.

The structural contract draws on evidence from English villages, manorial jurisdictions, small towns, the West Midlands, Essex and Yorkshire. It does not claim that one market institution, court, servile status, contract form or migration pattern applied uniformly across England.

## Evidence ledger

### 1. Rural households participated in markets, but participation was selective and geographically structured

**Scott L. Waugh, “Markets and towns,” in _England in the Reign of Edward III_ (Cambridge University Press).** The chapter describes a marketing system connecting village grain, goods and land with market towns and larger cities. It notes that tenants needed money for rents and commodities and that smallholders could need to buy food.

- https://www.cambridge.org/core/books/abs/england-in-the-reign-of-edward-iii/markets-and-towns/EDF5E40731E54A08C9399CD636F6EAE2
- Supports: ordinary rural production/consumption can create reasons to buy, sell and obtain money; trade connects settlements rather than being a closed village inventory.
- Does **not** support: every household trading every commodity, one national market price, or all exchange occurring in formal market towns.

**Kathleen Biddick, “Medieval English Peasants and Market Involvement,” _Journal of Economic History_ 45(4) (1985).** Taxation evidence shows regional/selective market involvement and cash-cropping patterns for grains and livestock.

- https://www.cambridge.org/core/journals/journal-of-economic-history/article/abs/medieval-english-peasants-and-market-involvement/B2E3B095E79207FF0AC83A08B5D46563
- Supports: market participation varies by household/resource/region; `every producer automatically sells surplus` is not a safe universal rule.
- Does **not** provide a universal household participation probability for the simulation baseline.

### 2. Credit is ordinary exchange structure, not a rare emergency-only subsystem

**Chris Briggs, _Credit and Village Society in Fourteenth-Century England_, chapter “The forms of credit and their uses” (British Academy/Oxford University Press, 2009).** Briggs treats credit broadly as benefits/services provided with a future claim and examines multiple forms of rural credit.

- https://academic.oup.com/british-academy-scholarship-online/book/21558/chapter-abstract/181401376
- Supports: exchange may create a future counter-obligation rather than requiring simultaneous payment.
- Does **not** establish one universal contract schema, interest rate or credit limit.

**Chris Briggs, chapter “The credit supply.”** The chapter identifies deferred payment and cash loans and connects them to obtaining goods/services, discharging obligations, maintaining production/consumption and investment.

- https://academic.oup.com/british-academy-scholarship-online/book/21558/chapter-abstract/181407204
- Supports: `goods now -> payment later`, loans and other delayed-performance relationships belong in the ordinary economy.
- Does **not** justify automatic credit availability or zero-risk lending.

### 3. Contracts need formation, obligations and enforcement states

**Chris Briggs, “Introduction: law courts, contracts and rural society in Europe, 1200–1600,” _Continuity and Change_ 29(1) (2014).** The article argues that private contracts of many kinds were central to rural economies and that courts and related public-order institutions facilitated registration and enforcement.

- https://www.cambridge.org/core/journals/continuity-and-change/article/introduction-law-courts-contracts-and-rural-society-in-europe-12001600/187EBB92881E44508C2F1D0CD5E5C2F5
- Supports: a contract cannot be represented adequately as one instant transfer; persistent obligations and institutional enforcement/dispute paths matter.
- Does **not** mean every small exchange required formal written registration or court involvement.

**Chris Briggs, “Seigniorial control of villagers’ litigation beyond the manor in later medieval England,” _Historical Research_ 81(213) (2008).** Villagers used legal structures to enforce contracts and recover debts from residents of other villages, including litigation outside their home manor.

- https://academic.oup.com/histres/article-abstract/81/213/399/5581660
- Supports: debts/contracts may survive cross-settlement relationships and enforcement need not be settlement-local.
- Does **not** establish unrestricted access to every jurisdiction or one universal enforcement procedure.

### 4. Formal markets can have institutional rules without being the only place exchange occurs

**James Davis, “Market Regulation in Fifteenth-Century England,” in _Commercial Activity, Markets and Entrepreneurs in the Middle Ages_ (Boydell & Brewer, 2011; Cambridge Core edition).** The chapter describes regulation of price, quality, weights, measures and market conduct in English markets.

- https://www.cambridge.org/core/books/abs/commercial-activity-markets-and-entrepreneurs-in-the-middle-ages/market-regulation-in-fifteenthcentury-england/ACEE135FE1AEF9F794C3F048999BF3C1
- Supports: a market venue/institution may supply applicable rules, standards, tolls or restrictions rather than being only a coordinate where inventories merge.
- Fifteenth-century evidence is used structurally only; exact regulations are **not** back-projected into every 1270–1348 settlement.

### 5. Labour can be exchanged under different arrangements

**Mark Bailey, “The regulation of the rural market in waged labour in fourteenth-century England,” _Continuity and Change_ (2023).** Bailey reconstructs a substantial rural hired-labour market. The evidence distinguishes live-in service with board/lodging and cash from irregular day/task/piece wage labour, and shows major changes in regulation and bargaining after the Black Death.

- https://www.cambridge.org/core/journals/continuity-and-change/article/regulation-of-the-rural-market-in-waged-labour-in-fourteenthcentury-england/C7726EC8A2D0C628ACFF49428FDA95A7
- Supports: labour/service agreements can create combinations of wage, maintenance/lodging and work obligations; one permanent occupation relationship is insufficient.
- Supports keeping pre- and post-1348 labour contexts separate.
- Does **not** justify one fixed wage, contract duration, board value or household/hired-labour share.

### 6. Rural migration was normal, often short-distance, and not simply movement to towns

**Christopher Dyer, “Migration in Rural England in the Later Middle Ages,” in _Migrants in Medieval England, c.500–c.1500_ (British Academy/Oxford University Press, 2020).** Using tax and manorial evidence from the West Midlands, Dyer describes rural migration as normal/commonplace, often within roughly ten miles but with longer movements, and discusses mechanisms, motives and reception in new settlements.

- https://academic.oup.com/british-academy-scholarship-online/book/37959/chapter-abstract/332491570
- Supports: migration is ordinary life behavior; short-distance movement and rural destinations must remain possible; opportunity/betterment can motivate relocation.
- The reported distance pattern is **not** accepted as a universal spawn/migration distribution.

**Christopher Dyer, _Peasants Making History_, chapter “Peasants changing society” (Oxford University Press, 2022).** The chapter connects migration with searches for land, employment and marriage and presents such choices as attempts to improve circumstances.

- https://academic.oup.com/book/43934/chapter-abstract/370549741
- Supports: destination choice can arise from concrete social/economic opportunities rather than a generic `wander` urge.
- Does **not** imply every migration succeeds or improves welfare.

### 7. Legal/tenurial constraints can matter without making servile people immobile

**Matt Raven, “Servile migration and seigniorial reaction in England: the serfs of Great Waltham and High Easter (Essex), c.1336–1361,” _Continuity and Change_ (2025).** Court-roll evidence shows migration by villeins despite formal servile restrictions; almost half of recorded migrants in the studied manors stayed within ten miles and a majority chose rural destinations. Raven emphasizes regional/institutional context and opportunities in landholding, wage work and service.

- https://www.cambridge.org/core/journals/continuity-and-change/article/servile-migration-and-seigniorial-reaction-in-england-the-serfs-of-great-waltham-and-high-easter-essex-c-13361361/FEE7266397005ACF06410A59D9CB33AD
- Supports: legal status/custom may affect migration feasibility/consequences but must not be converted into a universal physical immobility rule.
- Does **not** justify using the Essex distance/destination proportions for every region or social status.

### Evidence limits and disagreement

- Manorial court records reveal debts and disputes better than fully performed informal transactions; absence of litigation is not absence of exchange.
- Market participation differed by region, wealth, landholding, commodity and household strategy. No universal `surplus -> market` probability is accepted.
- Recorded debts reveal credit relationships selectively. Exact default rates, interest/implicit price premia and credit access remain underdefined.
- A contract may be oral, written, witnessed, court-enrolled or otherwise recognized depending on context. The simulation concept `Contract` is a causal abstraction, not a claim that all agreements used one documentary form.
- Formal market regulation varied by place and period. A `MarketInstitution` is optional context, not a mandatory wrapper around every transaction.
- Migration evidence is selective and regional. No universal annual migration rate or distance distribution is accepted.
- Pre- and post-Black Death labour, land and mobility conditions must not be averaged into one timeless parameter set.

## Causal model

Stable exchange/contract shape:

`need/opportunity/surplus + rights to offered resources/labour + available counterparties + information/relationships + market/institution context + proposed terms -> feasible agreement/transaction -> acceptance or refusal -> immediate transfers and/or explicit obligations -> performance / partial performance / renegotiation / default / dispute -> enforcement or discharge -> changed resources/claims/relationships/history`

Stable migration shape:

`person/household pressures + opportunities elsewhere + destination knowledge + expected work/service/land/residence/social basis + legal/social constraints + travel feasibility/cost -> migration intention -> destination -> travel -> arrival -> obtain/activate ordinary residence/work/household/tenure relationships -> changed location and relationships`

Arrival is not itself the final relationship transition.

There is no canonical transition of the form:

`arrive at settlement -> resident + household member + job + inventory access`

and no canonical transaction of the form:

`player clicks trade -> settlement stock transfers item at fixed global price`.

## Core concepts

### Transaction / Exchange event

A completed or partially completed exchange needs enough state/provenance to identify:

- parties/counterparties;
- transferred subjects and quantities;
- recognized rights/authority for each transfer;
- consideration received/promised, if any;
- whether consideration is immediate or deferred;
- location/venue/institution context when relevant;
- linked contract/obligation references;
- effective time and history/provenance.

A transaction may transfer money, resource lots, livestock, tools, rights or another accepted transferable subject. Labour itself is not a storable commodity lot; a labour/service agreement creates obligations for a person to perform work under accepted terms.

A gift or unilateral transfer can use the same transfer machinery without inventing fictitious consideration.

### Contract

A `Contract` is a persistent recognized agreement between parties creating one or more obligations/claims.

Minimum conceptual state:

- stable identity;
- parties and represented principals where applicable;
- formation/effective time;
- applicable institutional/custom context;
- obligations created for each party;
- conditions/contingencies where relevant;
- due date/window or termination condition where relevant;
- security/pledge/guarantor references where later accepted;
- status such as proposed/active/performed/discharged/breached/defaulted/disputed/terminated;
- performance history;
- dispute/enforcement references.

Exact enum/schema is deferred. The persistence of obligations and their causal lifecycle is canonical.

### Obligation / Debt / Claim

An obligation represents something a party owes to another party or recognized holder.

Possible accepted obligation kinds include:

- pay money;
- deliver a resource/animal/object;
- perform labour/service;
- provide lodging/maintenance/board;
- return a borrowed/leased subject;
- satisfy a rent/fee/other obligation already created by a holding or institution.

An obligation requires:

- obligor;
- beneficiary/claim holder;
- subject/performance kind;
- quantity/scope;
- due condition/time;
- source/basis contract or holding;
- current satisfied/outstanding/disputed state;
- history.

Debt is not erased because a person travels or joins another household. It ends only through an accepted discharge/performance/settlement/legal process.

### Money / payment

V1 may represent money at an abstract denomination/accounting layer, but payment must still have:

- a holder;
- finite amount/value;
- transfer provenance;
- no player-only creation or settlement-wide free balance.

Exact coin denominations, debasement, clipping, minting, exchange rates and monetary shortages are deferred unless a later mechanic needs them.

A price is a term of an agreement/offer/market context, not a timeless intrinsic property of an item type.

### Market / trading venue

A market is an optional institution/venue that can improve counterparty discovery and apply accepted rules such as standards, tolls or restrictions.

It is **not**:

- a global merged inventory;
- an omniscient price oracle;
- guaranteed liquidity;
- permission to sell property the actor has no right to transfer.

Private exchange, work agreements and credit relations may also occur outside a formal market.

### Labour / service agreement

A labour/service contract can combine multiple reciprocal terms, for example:

- specified/expected labour or availability;
- wage/payment now or later;
- board, lodging or maintenance;
- duration/termination condition;
- place/work scope;
- other accepted duties.

The employment/service relationship is separate from household membership and kinship even when lodging places the worker in the employer's household.

No `Profession` label may synthesize the agreement, wage, lodging, work authority or right to output.

### Migration episode

Migration is a process/event history around one persistent `Person`; it is not a new actor instance.

Minimum conceptual state/process references:

- person;
- origin and intended destination;
- motivating pressures/opportunities as decision inputs, not a single universal reason code;
- expected basis for residence/work/household/tenure where known;
- applicable permissions/constraints where relevant;
- travel intention/route state delegated to the accepted travel model;
- departure/arrival times;
- post-arrival relationship transitions;
- outcome/history.

A person may migrate temporarily, seasonally or permanently when later models support those distinctions. V1 must not hard-code every relocation as permanent household replacement.

## Exchange feasibility and choice

A proposed transaction is feasible only when the relevant parties can actually perform or credibly promise what is required under the accepted model.

Examples:

- Household H needs seed and Person S lawfully holds transferable grain -> H and S may agree on immediate payment, deferred payment, loan or another accepted arrangement -> actual grain transfers only under the agreed rights/terms.
- H has no money but expects harvest income -> S may refuse credit, accept deferred payment, require security/guarantee when later modeled, or offer different terms. Credit is a decision/relationship, not an automatic fallback.
- Person W offers labour during harvest -> a counterparty can agree day/task/piece work -> W's finite labour time becomes committed to the resulting task/obligation; the contract does not duplicate W's labour capacity.
- A player reaches a market with grain owned by another household -> physical possession/location alone does not authorize sale.

Counterparty selection, bargaining, trust/reputation and price formation need later decision/calibration work. This contract only requires that they be causal inputs rather than hidden global constants.

## Contract lifecycle

Minimum topology:

`proposal/negotiation -> accepted formation -> obligations become active -> performance events update obligations -> fully satisfied/discharged OR outstanding/renegotiated/defaulted/disputed -> optional institutional enforcement/settlement -> terminal history`

Rules:

1. Formation and performance are separate.
2. A promised transfer does not move the resource until performance occurs.
3. Partial performance must be representable where material.
4. Default/breach is valid world state, not an engine error.
5. A dispute does not silently choose a winner; unresolved claims can persist until an accepted process resolves them.
6. Contract termination does not automatically reverse already completed performance unless the applicable agreement/process says so.
7. Enforcement depends on applicable institution/jurisdiction/context, not controller type.

## Migration and integration

Migration changes physical/social context but preserves person identity and pre-existing relationships unless ordinary world events change them.

Potential causal drivers include:

- employment/service opportunities;
- access to land/tenancy or other productive resources;
- marriage/partnership/household opportunities;
- loss of work, residence or maintenance;
- household labour/consumption pressure;
- debt/obligations or conflict;
- wages/market opportunities;
- kin/social connections;
- shocks or institutional pressure.

No single driver is universally required or sufficient.

### Arrival is not integration

On arrival a person may need to obtain one or more of:

- lawful lodging/residence basis;
- household membership or service lodging arrangement;
- employment/work contract;
- tenancy/land/resource rights;
- market/custom/institutional permissions;
- social recognition/relationships where later modeled.

The person remains an ordinary world actor while these are unresolved. The engine must be able to represent a newcomer who is present but not yet securely housed/employed/integrated.

### Mobility constraints

Rights/custom/legal status may affect:

- whether departure is authorized;
- obligations that continue after departure;
- fines/disputes or enforcement risk;
- which opportunities are realistically available.

Such constraints do not become an invisible physical wall unless the movement is physically prevented by an accepted world mechanism.

## Player/NPC symmetry

HumanController and AIController operate on the same `Person`, `Transaction`, `Contract`, `Obligation`, rights and migration rules.

Controller type cannot grant:

- special prices;
- free goods or money;
- unlimited credit;
- ability to sell another holder's property;
- automatic contract enforcement;
- exemption from debt/default;
- automatic employment or housing on arrival;
- free migration/teleportation;
- deletion of pre-existing obligations when changing settlement.

A human controller may choose worse terms, intentionally default, reject work, sell reserved seed, or migrate without a secure destination when those actions are physically/socially possible. AI faces the same consequence topology even if its policy differs.

## Rights, claims and authorization

The accepted Property/Tenure/Common Rights contract is authoritative for whether a party may transfer/use a subject.

Important distinctions:

- possessing an item does not necessarily authorize its sale;
- market presence does not authorize trade in a subject;
- accepting money does not automatically make an unauthorized transfer legitimate;
- an employment contract may authorize use of specified employer tools/resources without transferring ownership;
- a contract can create a claim to future performance without transferring current possession;
- debt is a claim/obligation relationship, not negative inventory;
- migration does not detach rights/obligations from their accepted basis.

## Rules

1. **Exchange has counterparties.** No world stockpile buys/sells by itself unless an explicit institution is the counterparty.
2. **Transfers require authority.** The offering party must have an accepted right/authorization to transfer or promise the subject/performance.
3. **Consideration is general.** Money, goods, services and deferred obligations may participate; there is no special canonical `barter mode`.
4. **Credit is explicit.** Deferred performance creates an obligation/claim that persists until resolved.
5. **Contracts survive time and movement.** Save/load, travel and household changes do not silently erase them.
6. **Performance is causal.** Resources/labour move or are consumed only when the relevant action/performance occurs.
7. **No guaranteed liquidity.** A willing/able counterparty must exist; a market venue does not synthesize one.
8. **Price is contextual.** No universal item-price table is accepted as historical law.
9. **Default and dispute are valid.** The simulation must preserve outstanding obligations and unresolved claims.
10. **Labour is finite.** Contracting work reserves/commits person time/capacity; it does not clone labour.
11. **Migration preserves identity.** Relocation never despawns one person and spawns an unrelated replacement.
12. **Arrival grants no automatic rights.** Residence, household, work and resource access require their ordinary accepted bases.
13. **Migration uses travel.** It must pass through the accepted semantic-location/travel process rather than direct settlement reassignment.
14. **Local institutions matter.** Market/court/custom context can affect terms, authorization and enforcement without becoming one global rule table.
15. **No unsupported quantitative constants.** Prices, wages, interest, default, migration and market-frequency parameters require separate evidence/calibration.

## Long-horizon requirement

Because exchange, debt, labour contracting and migration can alter settlement resources, productive capacity and population distribution, an implemented economic/demographic system based on this contract cannot PASS without a future **>=10 simulated-year proof** under the Reality Modeling Policy.

That proof must eventually track at least:

- major transactions and transfer provenance;
- money/payment conservation at the chosen abstraction;
- active contracts and obligations;
- debt creation, performance, default/dispute and discharge;
- household resource shortages/acquisitions;
- labour contracts and finite labour allocation;
- agricultural acquisition needs such as seed/tools/animals where applicable;
- persons moving between settlements/households;
- residence/work/tenure transitions after migration;
- rights and debts that cross settlement boundaries;
- births/deaths from a separately accepted demographic model when population trajectories are evaluated.

Required invariants include:

- no resource or money creation from exchange itself;
- no duplicate transferred asset/lot;
- no debt disappearance merely because due time passed or debtor moved;
- no duplicate person after migration;
- no automatic household/job/property assignment on arrival;
- no labour double-booking across contracts/tasks;
- save/load/replay preserving contract, obligation, transaction and migration history;
- both solvent/viable and distressed/defaulting/collapsing outcomes accepted when causally explained.

This structural contract does not claim that any current settlement has historically calibrated prices, incomes, credit availability or migration balance.

## Assumptions and uncertainty

- Exact commodity prices and price distributions: **MODEL_UNDERDEFINED**.
- Price formation/bargaining model: **MODEL_UNDERDEFINED**.
- Market-day frequency, catchment and counterparty matching rates: **MODEL_UNDERDEFINED**.
- Exact use of formal markets versus private/local exchange: **MODEL_UNDERDEFINED**.
- Medieval monetary denomination/coin-quality detail: deferred until causally required.
- Credit access, loan/deferred-payment frequencies and default rates: **MODEL_UNDERDEFINED**.
- Interest/implicit credit-price premia/security/guarantor frequencies: **MODEL_UNDERDEFINED**.
- Contract documentation/witnessing/enrolment procedures by institution: **MODEL_UNDERDEFINED**.
- Exact enforcement costs, delays, remedies and jurisdiction choice: **MODEL_UNDERDEFINED**.
- Wage levels, board/lodging valuation and labour-contract durations: **MODEL_UNDERDEFINED** and period-dependent.
- Migration rate, distance distribution, sex/age profile and destination mix: **MODEL_UNDERDEFINED**.
- Costs/risks of travel and exact mobility permissions: blocked on accepted travel/local-status calibration.
- Newcomer integration probability/timing and social reception: **MODEL_UNDERDEFINED**.

These gaps are explicit blockers for production mechanics that materially require the missing numbers. They are not permission to choose convenient gameplay constants.

## Fixture boundary

The following current/prototype patterns are explicitly noncanonical:

- a settlement-wide inventory acting as seller/buyer for all residents;
- player-only inventory authority over settlement goods;
- `ShareRation` or another fixed interaction verb as the general exchange model;
- universal fixed item prices with guaranteed buyers/sellers;
- a separate `barter` subsystem that bypasses generic transfers/contracts/rights;
- automatic credit when money is insufficient;
- debt represented only as a negative inventory count;
- contracts that vanish on travel/save/load/household change;
- `Profession` as the source of wage, work authority or employment relationship;
- migration implemented as only resetting `HouseholdId`/`WorkplaceId`;
- arrival automatically assigning a new home, household or job;
- all migration directed toward towns;
- migration as unrestricted teleportation;
- player status exempting an actor from market/custom/legal rules.

Existing command/inventory/migration fixtures may remain temporarily as regression seams, but they must not constrain canonical implementation.

## Falsifiers

Revise this model if evidence or implementation shows that:

- deferred obligations/debt add no causal distinction over instant transfer for the selected context;
- rural exchange can be represented without explicit counterparties while still preserving ownership/rights and scarcity;
- contract performance/default/dispute states produce no meaningful world consequences;
- labour agreements can be collapsed into permanent profession labels without losing accepted historical distinctions;
- migration can be modeled as an atomic settlement reassignment without losing residence/work/rights/obligation causality;
- cross-settlement debts/claims never matter to accepted gameplay/history;
- ten-year proofs require hidden buyers, free liquidity, silent debt forgiveness or population cloning to remain stable.

## Feedback

Presentation may expose, subject to later knowledge/visibility rules:

- known offers/opportunities and counterparties;
- agreed price/consideration and whether payment/performance is due now or later;
- active obligations/debts and known due conditions;
- contract performance/default/dispute status;
- known market rules/tolls/restrictions;
- work/service terms and committed time;
- migration intention/destination and known expected opportunity;
- whether residence/work/tenure after arrival is secured or unresolved.

UI must not reveal every hidden buyer, future price, private contract, creditworthiness judgment, court outcome or migration opportunity merely because the player opens a trade/world screen.

## Persistence

Persist enough authoritative state to reconstruct exactly:

- transaction history/provenance needed for current claims;
- active and recently relevant contracts;
- obligations, beneficiaries, due conditions and performance history;
- outstanding debt/dispute/enforcement references;
- money/resource holder changes;
- labour/service commitments that remain active;
- migration intention/process when interrupted;
- person's origin/destination/current semantic location;
- pre-existing and post-arrival household/residence/work/tenure relationships;
- applicable institution/custom references;
- controller-independent actor identity.

Do not reconstruct contracts, debts, employment, market rights or migration outcome from profession, settlement membership, current location, controller type or clock hour.

## Acceptance scenario

A future structural implementation should be able to demonstrate:

1. Household H needs seed but owns no freely disposable grain sufficient for sowing.
2. Seller S lawfully holds transferable grain; H and S negotiate an accepted transaction rather than drawing from settlement stock.
3. H can pay immediately, promise deferred payment or enter another accepted contract; if deferred, grain transfer creates a persistent payment obligation instead of free grain.
4. Save/load preserves the debt. If H later pays, the obligation is discharged by an explicit performance event; if H cannot pay, default/dispute remains represented.
5. Person W can accept finite harvest work under a day/task/service agreement; the agreement creates work/payment obligations and prevents the same labour time from being simultaneously allocated elsewhere.
6. Person M identifies a credible opportunity in another settlement and chooses migration because expected benefits/pressures outweigh available alternatives under the later decision model.
7. M retains identity, possessions, debts, rights and relationships while travelling through the semantic travel layer.
8. On arrival M is physically present but receives no automatic household/job/home/property. Ordinary agreements, membership and rights establish those relationships when feasible.
9. A debt or contract counterparty in the origin settlement remains valid after M moves and can later be performed/disputed/enforced under an accepted jurisdiction process.
10. HumanController may take over H, S, W or M without changing any exchange, contract, debt, labour, migration or authorization rule.
11. Save/load/replay reproduces the same contracts, obligations, resource transfers, person identity and migration state.

This scenario proves topology, not calibrated prices, wages, credit risk or migration frequencies.

## Deferred complexity

Separate bounded work is still required for:

- calibrated price formation and market matching;
- money/coinage detail if economically necessary;
- bargaining, trust/reputation and information models;
- secured credit, pledges, guarantors and insolvency procedures;
- institution-specific court/enforcement procedures;
- detailed labour-contract law/custom and wage regulation;
- merchant/intermediary and transport-cost models;
- accepted semantic travel duration/routing model;
- migration decision calibration and newcomer social integration;
- demographic rates and marriage formation;
- cross-settlement world-economy calibration.

Deferring those does not invalidate this structural contract, but production mechanics that require the missing quantitative or institutional detail must remain blocked until the relevant model/calibration is independently accepted.
