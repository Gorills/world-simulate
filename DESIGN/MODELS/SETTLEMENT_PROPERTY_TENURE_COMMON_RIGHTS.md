# Settlement Property, Tenure and Common Rights — Model Contract

Status: **ACCEPTED**

This contract defines the minimum causal structure for possession, recognized claims, tenure, use rights, common rights and associated obligations. It does **not** define a universal medieval English land-law code, exact inheritance algorithms, rent/service quantities, grazing stints or manor-specific customs; those remain context-specific or `MODEL_UNDERDEFINED` until separately calibrated.

## Mechanic

Represent who may possess, occupy, use, exclude, transfer, inherit, collect from or otherwise act upon land, buildings and resources through explicit world-state rights/claims and obligations rather than through player status, household membership, co-location or a single magical `OwnerId`.

## Intended feeling

Land, homes, stored food, tools, livestock and commons should feel socially and materially situated. A person can physically reach something without automatically being entitled to use it, while legitimate rights can arise from tenure, custom, contract, office, household sharing rules, grant, inheritance or permission.

The player should encounter ordinary constraints and opportunities that AI-controlled people also face: rent, service, permission, shared access, disputed claims, transfer conditions, limited commoning rights and consequences for unauthorized use.

## Real-world process

Late-medieval rural English property relations cannot be represented adequately as one modern absolute ownership flag. Manorial records repeatedly distinguish occupation/holding, transfers, local tenure conditions, rents/services, rights over commons and wasteland, and court/custom-based enforcement.

For simulation purposes the important causal distinction is:

`physical possession/control` != `recognized right/claim` != `authorization for a specific action`

A person may hold and cultivate land while transfer or succession is constrained by local custom; a person may own livestock while another institution regulates how strays are handled; a commoner may have a legitimate pasture or resource-use right over land held by someone else; a household member may have permission to consume specified household food without gaining universal ownership of every household-associated asset.

## Reference context

Baseline: rural England, approximately 1270–1450, especially communities governed through manorial/customary institutions.

This contract defines structural affordances required to represent documented variation. It does not assert that all English land was customary/manorial, that every manor used identical rules, or that one hierarchy of lord/tenant rights explains every property relationship. Freehold, customary tenure, leases, ecclesiastical holdings, royal jurisdictions and local customs could differ materially.

Later examples may be cited only when they clarify a durable institutional distinction; they are not used to back-project later numerical rules into the 1270–1450 baseline.

## Evidence and sources

1. **The National Archives, “Manors and manorial records.”** Manorial court rolls record transfers of property rights, occupation of land, agricultural regulation, common-land bylaws and labour services. This supports explicit state for claims/holding, transfer events, obligations and common-resource rules instead of inferring authority from presence.
   - https://www.nationalarchives.gov.uk/help-with-your-research/research-guides/manors/
   - Does **not** establish one universal tenure type or one set of rents/services across England.

2. **The National Archives, “A guide to manorial documents.”** The guide states that manor courts dealt with rights to common/wasteland, property transfer, conditions of tenancy and bylaws; surveys and rentals identify tenants, holdings, rents, labour services and tenure types. It also explicitly notes manor-to-manor variation in tenancy and inheritance customs, including different succession patterns. This supports local `Custom/RuleSet` context rather than a global inheritance/tenure algorithm.
   - https://www.nationalarchives.gov.uk/archives-sector/finding-records-in-discovery-and-other-databases/manorial-documents-register/a-guide-to-manorial-documents/
   - Does **not** justify treating every recorded later copyhold procedure as unchanged in the thirteenth century.

3. **The National Archives, “Manorial documents and lordships and how to use the Manorial Documents Register.”** The guide describes surrender/admission as the recorded process for transfers of copyhold land and notes restrictions on buying, selling, inheriting, subletting, exchanging or mortgaging such holdings without the required manorial process/permission. This is evidence that `transfer possession -> new owner` is not an adequate generic land transition.
   - https://www.nationalarchives.gov.uk/help-with-your-research/research-guides/manorial-documents-lordships-how-to-use-manorial-document-register/
   - The terminology “copyhold” spans later periods; V1 uses the structural lesson—transfers can require a recognized procedure and conditions—without assuming one procedure everywhere in 1270–1450.

4. **The National Archives, “Land ownership, use and rights: common lands.”** The guide explains the core distinction that common land is land owned/held by someone while specified persons enjoy rights to take or use part of it or its produce; common land is not simply unrestricted public land. This supports modeling common rights as explicit use claims over a subject rather than settlement-wide ownership.
   - https://www.nationalarchives.gov.uk/help-with-your-research/research-guides/common-lands/
   - The guide covers a long chronology and later legislation, so this contract uses only the structural distinction, not modern access law.

5. **Lloyd Bonfield, “What Did English Villagers Mean by ‘Customary Law’?”, in Zvi Razi and Richard Smith (eds.), _Medieval Society and the Manor Court_ (Oxford University Press, 1996).** Bonfield argues that manorial customary law should not be treated as merely a clone of royal/common law. This supports representing a locality/institution-specific custom context rather than assuming one national rule table for all village property relations.
   - https://academic.oup.com/book/3251/chapter/144206775
   - This does not imply that local custom was arbitrary or isolated from wider law; it only blocks a single universalized village-law fixture.

6. **Lloyd Bonfield and L. R. Poos, “The Development of the Deathbed Transfers in Medieval English Manor Courts,” in _Medieval Society and the Manor Court_ (Oxford University Press, 1996).** The chapter examines how customary tenants developed inheritance/disposition strategies within manorial institutions and lordly interests. This supports succession/transfer as world processes with recognized options and constraints rather than automatic inheritance by one hard-coded relative.
   - https://academic.oup.com/book/3251/chapter-abstract/144207305
   - It does not establish one succession order or probability distribution suitable for all manors.

7. **Jordan Claridge and Spike Gibbs, “Waifs and Strays: Property Rights in Late Medieval England,” _Journal of British Studies_ 61(1) (2022).** The article shows a manorial institution simultaneously protecting owners’ claims to livestock and managing risks to collective arable resources. It also describes courts as venues for enforceable contracts and bylaws concerning common land/grazing. This supports separate private claims, collective-resource governance and institutional enforcement rather than collapsing them into one settlement owner.
   - https://www.cambridge.org/core/journals/journal-of-british-studies/article/waifs-and-strays-property-rights-in-late-medieval-england/148ADDD32647806A4793D0AB2933F888
   - The stray system is an example of institutional coordination, not a universal template for every asset dispute.

8. **Spike Gibbs, _Lordship, State Formation and Local Authority in Late Medieval and Early Modern England_, chapter 4, “Manorial Officeholding and Village Governance: Misconduct and Landscape Control” (Cambridge University Press, 2023).** Fourteenth- and fifteenth-century examples include outsiders presented for using commons where they lacked rights and commoners accused of overburdening resources. The chapter also shows substantial variation between communities in landscape, governance and common-right practices. This supports rights having eligibility/scope and supports local variation; it does not justify importing later sixteenth-century stints into the medieval baseline.
   - https://www.cambridge.org/core/books/lordship-state-formation-and-local-authority-in-late-medieval-and-early-modern-england/manorial-officeholding-and-village-governance-misconduct-and-landscape-control/B54CAEE9A3B92A11CF7629BC90EC129A

9. **Susan Reynolds, “Tenure and property in medieval England,” _Historical Research_ 88(242) (2015).** Reynolds argues that later historiographical use of “tenure” can obscure medieval English property law and comparison with other systems. This is a terminology warning for the simulation: `Tenure` should model a relationship/holding with associated claims and obligations, not act as a synonym for “the lord absolutely owns everything and the tenant owns nothing.”
   - https://academic.oup.com/histres/article/88/242/563/5603024
   - This contract therefore uses “right/claim/holding” language where possible and treats tenure as one relationship among several.

### Evidence limits and disagreement

- Medieval English property vocabulary and legal classification are contested by historians; simulation names are implementation abstractions, not claims that medieval actors used identical categories.
- Manorial custom varied geographically and temporally. An inheritance or transfer rule accepted for one manor must not silently become national simulation law.
- The Black Death and later land/labour changes altered bargaining conditions and land markets. Pre- and post-1348 parameterization may differ even where the structural rights model remains useful.
- Commons could be governed at manor, vill/hamlet or other local levels; `SettlementId -> common rights` is not accepted as a universal mapping.
- Rights could be attached to a holding/tenement, granted to a person, arise from custom or be otherwise conditioned. V1 must support more than one basis rather than selecting a single universal attachment rule.
- Exact fines, rents, labour services, heriots, entry payments, transfer fees, grazing capacities and succession orders are outside this structural contract.

## Causal model

Stable shape:

`actor + subject + physical access/control + recognized claims/rights + applicable tenure/custom/contract + obligations + requested action -> authorization/violation determination -> action or dispute/enforcement -> changed possession/claims/resources/obligations/history`

Examples:

- Person A reaches a grain store -> co-location makes taking physically possible -> a household sharing rule, ownership claim, employment authority or explicit permission may authorize taking a quantity -> absent such basis, taking can become unauthorized appropriation rather than magically succeeding because A is the player.
- Tenant A holds Parcel P under a recognized holding -> cultivation is permitted within that holding -> proposed transfer invokes the applicable custom/procedure -> transfer succeeds only if its required conditions are satisfied.
- Commoner A has pasture rights over Common C -> the right authorizes specified commoning under applicable limits -> exceeding scope can become overburdening even though A is a legitimate commoner.
- Outsider B is physically present on Common C -> presence alone supplies no use right -> grazing/cutting may be unauthorized unless a separate grant, contract or custom applies.
- Person A dies -> physical assets and rights do not silently become household property -> succession/transfer procedures resolve each relevant claim according to the applicable accepted model.

## Player/NPC symmetry

The same authorization function applies to AI-controlled and human-controlled `Person` actors.

Conceptually:

`CanAttempt(action)` depends on physical capability/access.

`IsAuthorized(action)` depends on rights, claims, contracts, office, permission and applicable custom.

Controller type is not an input to either legal/world authorization rule.

A human-controlled person may still choose an unauthorized action when the action is physically possible; the consequence should be represented as trespass, theft, breach, debt, dispute or another ordinary-world violation when the relevant later mechanic exists. Player status must not convert an unauthorized action into an authorized one.

## Ownership, rights and obligations

### Core distinction

V1 must not use a single universal `OwnerId` as the complete authority model.

A simple owner field may remain as an optimization/projection for uncomplicated movable goods, but authoritative decisions must be expressible through explicit recognized claims when multiple interests or conditions exist.

### Right / Claim

Minimum conceptual fields:

- stable identity;
- `holder` — a recognized world entity or holding to which the claim belongs;
- `subject` — land parcel, building/space, resource lot, livestock group, tool/object or other rights-capable subject;
- `right_kind` — what action/benefit is recognized;
- `scope` — quantity, area, species/resource type, duration or other limit when relevant;
- `basis` — tenure, custom, contract, grant, inheritance, office, household sharing rule or other accepted origin;
- `effective_from` and optional end/termination condition;
- `conditions` / reference to the applicable rule set;
- provenance/history sufficient for deterministic replay and dispute resolution.

V1 should support at least these conceptual right kinds without claiming that they are medieval legal terms:

- possess/control;
- occupy/reside;
- cultivate/use;
- consume/remove a specified resource;
- pasture/common or collect a specified common resource;
- exclude others within recognized scope;
- receive rent/service/payment;
- transfer/surrender/assign subject to applicable procedure;
- inherit/succeed when the applicable process recognizes it;
- manage/authorize on behalf of another holder or institution.

Exact enum/schema is deferred to implementation design; the causal distinctions are canonical.

### Tenure / Holding

A `TenureHolding` is a persistent relationship tying a holder to land/buildings through an applicable institutional/custom context and associated claims/obligations.

Minimum conceptual fields:

- holder;
- subject parcel(s)/premises;
- superior/lord/institution or other counterparty when applicable;
- applicable local custom/rule context;
- claims granted/recognized by the holding;
- rents, services, dues or other obligations as references rather than baked constants;
- transfer/succession conditions;
- effective period and history.

This abstraction must also permit simpler holdings with no relevant manorial superior. Not every possession relationship is forced into one feudal ladder.

### Common rights

A common resource is not “owned by everyone.”

Represent:

- the land/resource subject and its underlying holder/control relation;
- each recognized common/use right separately or via a rule that resolves an eligible holding/person to such a right;
- scope/limits when the accepted local model requires them;
- the local institution/custom responsible for regulation/enforcement when relevant.

A common right may be attached to a person, a holding/tenement or another accepted legal basis. The exact attachment must come from the selected reference context; household or settlement membership alone is insufficient.

### Household sharing

The accepted Person/Household contract remains authoritative: membership is not universal ownership.

A household can coordinate access to specified resources through explicit sharing/maintenance rules. Such a rule creates authorization for the covered action/resource; it does not grant every member transferable title to all assets associated with the household.

## Player decision

Rights create gameplay choices without special RPG verbs:

- seek permission to use a field, house, tool or stored resource;
- accept a holding with rent/service obligations;
- surrender, transfer, sublet or exchange a holding when the applicable model allows it;
- exercise or defend a common right;
- stay within a common-right limit or intentionally overburden it;
- lend, gift or authorize use of movable property;
- challenge or negotiate a disputed claim through an applicable institution;
- use something without authorization and accept ordinary-world consequences.

These are consequences of world relationships, not menu powers granted because an actor is the player.

## Rules

1. **Physical access is necessary for many physical actions but never sufficient legal/social authorization.**
2. **Household membership, settlement membership and controller type never imply universal resource rights.**
3. **Rights are action-specific.** A cultivation right does not automatically imply a transfer right; a pasture right does not imply timber/fuel collection; residence does not imply disposal of the building.
4. **Rights may coexist.** Multiple recognized interests over one subject are expected, especially for land/common resources.
5. **Obligations are explicit counterparts.** A holding may create rent/service/maintenance duties without encoding those duties as properties of the person class.
6. **Transfers are events/processes.** They preserve provenance, apply the relevant custom/contract and alter claims only when the required conditions are satisfied.
7. **Unauthorized does not always mean physically impossible.** Where the world permits the physical act, the simulation should be able to represent violation rather than using authorization as an invisible force field.
8. **Local custom is data/model context, not a global singleton law table.** A later world generator must select or define applicable customs deliberately.
9. **No unsupported numerical constants.** Rent, service days, heriots, fines, entry fees, grazing limits or inheritance shares require separate evidence/calibration.

## Long-horizon behavior

This structural rights contract does not by itself establish economic balance, but a future >=10-year settlement proof must demonstrate that rights/holdings survive ordinary lifecycle and economic change without state corruption.

At minimum, long-run tests should be able to include births/deaths, household changes, tenancy changes, debts, migrations, land transfers and resource depletion while checking that:

- rights do not disappear or multiply merely because actors move;
- deceased/departed holders do not remain active claimants indefinitely;
- successor claims arise only through an accepted succession/transfer process;
- household membership changes do not silently transfer unrelated assets;
- common resources cannot be consumed by every resident without rights/scope checks;
- obligations remain attached to their accepted basis after save/load/replay;
- conflicting or unresolved claims remain visible rather than being silently collapsed to one `OwnerId`.

Historical rates and economic consequences of tenure change remain for later agriculture/exchange/demography contracts.

## Assumptions and uncertainty

- Exact free/customary/copyhold/lease classification across the reference world: **MODEL_UNDERDEFINED** at world-generation/calibration level.
- Exact manor-specific inheritance and transfer rules: **MODEL_UNDERDEFINED** until a concrete reference manor/custom set is selected.
- Exact rents, labour services, heriots, entry fines and other dues: **MODEL_UNDERDEFINED**.
- Exact common-right stinting/carrying capacities and seasonal restrictions: **MODEL_UNDERDEFINED** and must connect to later livestock/ecology modeling.
- Exact dispute procedure, evidentiary burden and sanctions: deferred to an institutions/dispute contract.
- V1 uses explicit right kinds as simulation abstractions; it does not claim medieval legal categories map one-to-one onto these names.
- V1 may initially omit sophisticated priority/ranking between competing claims. It must not resolve that omission by arbitrarily choosing the player or settlement as owner.

## Fixture boundary

The following current prototype assumptions are explicitly noncanonical:

- one `_settlementOwnerId` that effectively makes settlement stock available for player-facing commands;
- `ItemStack.OwnerId` alone being sufficient for all land/resource authorization semantics;
- `HouseholdId` implying ownership or unrestricted access to household-associated resources;
- `HomeId` implying that every household member has identical residence, exclusion or transfer rights;
- co-location with a person/place implying authority to give, feed, take or share resources;
- “common” meaning settlement/public ownership or free access by every resident;
- profession/workplace assignment granting implicit authority over all inputs/outputs at that workplace;
- migration automatically clearing or granting rights without a transfer/termination event.

Existing `OwnerId`/household/home seams may be useful migration points in code, but their current fixture semantics must not constrain the canonical model.

## Falsifiers

Revise this model if evidence or implementation shows that:

- one absolute owner relation is sufficient to explain documented land/common-resource behavior in the selected reference context;
- household membership historically and causally grants universal transferable title to household-associated assets;
- common rights can be represented accurately as unrestricted settlement membership;
- transfer/succession can be modeled without any local/custom/institutional conditions while preserving the selected historical context;
- separating physical possibility from authorization produces no meaningful or historically grounded state distinction;
- long-horizon simulations systematically create irresolvable claim proliferation because the V1 claim/holding distinction is too weak.

## Feedback

Immediate presentation may expose, subject to the later knowledge/visibility model:

- who currently possesses/controls a subject;
- the actor's known right or permission basis;
- required permission/obligation for a proposed action;
- whether an action is physically possible but apparently unauthorized;
- relevant due rent/service or common-right limit when known.

History/trace should preserve grants, admissions, surrenders/transfers, permissions, right termination, dues fulfilled/defaulted and material disputes/violations.

UI must avoid asserting omniscient legal truth to the player when the character could not know it; information visibility is deferred.

## Persistence

Persist enough authoritative state to reconstruct exactly:

- stable rights/claim identities;
- holder and subject identities;
- basis/context and scope;
- tenure/holding identities and counterparties;
- linked obligations;
- effective/termination state;
- transfer/succession provenance;
- common-resource eligibility/scope state required by accepted local rules.

Do not reconstruct rights from current location, household membership, profession, controller type or clock time.

## Input flow

Not defined at this foundation layer. Human and AI controllers submit ordinary intentions/actions; authorization is resolved by authoritative simulation state before consequences are applied.

## Projection/UI

Godot may receive observable projections of possession, known rights/permissions, obligations and proposed-action failure reasons. Godot must not own claims, tenure, common-right scope or authorization logic.

## Acceptance scenario

A future implementation acceptance scenario should prove several different interests over the same world:

1. Person A is an ordinary `Person` holding Parcel P through Tenure H with a recognized cultivation/residence basis and a rent/service obligation.
2. Person B is a member of A's household but has no automatic transfer right over P merely from membership.
3. Person A and another eligible holder possess recognized pasture rights over Common C; outsider Person D does not.
4. HumanController takes control of D. D can physically travel onto C but controller status does not create a common right.
5. If D attempts grazing, the simulation classifies the action through the same authorization rules used for AI and records an unauthorized use/violation rather than silently allowing it as player privilege.
6. A legitimate commoner can graze within accepted scope; exceeding scope is distinguishable from having no right at all.
7. Person A later attempts to transfer H. The applicable accepted custom/process determines whether the transfer is valid and what linked claims/obligations change; no direct `OwnerId = B` shortcut bypasses it.
8. Save/load/replay reproduces the same claims, holding, obligations, common rights and violations.

This scenario proves the structural model. Exact rent levels, inheritance order and grazing numbers are not part of this contract's acceptance.

## Deferred complexity

Deliberately deferred to separate bounded contracts/model work:

- manor-specific tenure taxonomies and calibrated customs;
- inheritance/succession law in detail;
- courts, disputes, evidence and sanctions;
- exchange, debt, credit and contract formation;
- agricultural parcel use and crop-cycle decisions;
- commons ecology/carrying capacity and livestock herd dynamics;
- officeholding/governance and collective bylaw formation;
- information/knowledge of rights and concealment/fraud.

Deferring these does not invalidate this contract because this step only establishes the minimum rights/holding/authorization topology that later economic and agricultural models require.