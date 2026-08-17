# Audit — Settlement Property, Tenure and Common Rights

Audit date: **2026-08-17**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/SETTLEMENT_PROPERTY_TENURE_COMMON_RIGHTS.md`

Reviewed research/model SHA: `1dc690d694b7537dd05b0d64d601f9ac5c4cc6fd`

## Scope

Independent audit of the structural property / tenure / common-rights model only. No production simulation code was reviewed or changed by this audit. Exact manor-specific inheritance algorithms, rents, labour services, heriots, entry fines, transfer fees, grazing stints/carrying capacities and dispute procedures remain outside the accepted scope and remain `MODEL_UNDERDEFINED` or deferred as stated by the contract.

## Load-bearing fact re-check

The audit independently reopened the evidence behind the claims that materially determine the rights/holding topology.

### Manorial records distinguish holding, transfer, local regulation and obligations

Re-checked The National Archives, “Manors and manorial records”:

https://www.nationalarchives.gov.uk/help-with-your-research/research-guides/manors/

The guide states that manorial court rolls record transfers of property rights, occupation of land, agricultural regulation, bye-laws concerning common land and enforcement of labour services. It also notes that the manorial system did not cover the whole country.

Audit conclusion: a single `OwnerId` is not sufficient as the universal authority model for the selected manorial/customary reference context. The structural claim is accepted only for contexts where such overlapping holding, use, transfer and obligation relations matter; it is not asserted as a universal description of all medieval English land.

### Common land is not unrestricted settlement/public ownership

Re-checked The National Archives, “Land ownership, use and rights: common lands”:

https://www.nationalarchives.gov.uk/help-with-your-research/research-guides/common-lands/

The guide defines common land as land owned by someone else over which one or more persons enjoy rights to take/use part of the land or its produce, and explicitly rejects the misconception that common land is simply public land with unrestricted access.

Audit conclusion: commoning must be represented as a recognized use right with an eligible holder/basis and possible scope. `SettlementMember -> unrestricted common access` is rejected.

### Manor-specific custom and inheritance variation are real

Re-checked The National Archives, “A guide to manorial documents”:

https://www.nationalarchives.gov.uk/archives-sector/finding-records-in-discovery-and-other-databases/manorial-documents-register/a-guide-to-manorial-documents/

The guide states that manor courts handled common/waste rights, property transfer, tenancy conditions and bylaws. It also states that population density, tenancy types and customs varied from manor to manor, and gives differing inheritance patterns including succession to the oldest child, youngest child, or division among offspring. Surveys/rentals record tenants, holdings, rents, labour services and tenure types.

Audit conclusion: the contract is correct to require local `Custom/RuleSet` context and to leave exact inheritance and due/service algorithms `MODEL_UNDERDEFINED` until a reference manor or calibrated rule set is selected.

### Transfer can require recognized institutional procedure

Re-checked The National Archives, “Manorial documents and lordships and how to use the Manorial Documents Register”:

https://www.nationalarchives.gov.uk/help-with-your-research/research-guides/manorial-documents-lordships-how-to-use-manorial-document-register/

Its “Surrender and Admission” glossary entry describes copyhold transfer through surrender/admission in manorial court and states that copyhold tenants could require lordly permission/process to buy, sell, inherit, sublet, exchange or mortgage land.

Audit conclusion: the structural rule `transfer is a process governed by applicable conditions`, rather than `possession changes -> OwnerId changes`, is supported. The audit does **not** back-project one later copyhold procedure unchanged across every manor in 1270–1450; the contract already contains this limitation.

### Customary law cannot be reduced to one national common-law table

Re-checked Lloyd Bonfield, “What Did English Villagers Mean by ‘Customary Law’?”, in _Medieval Society and the Manor Court_ (Oxford University Press, 1996):

https://academic.oup.com/book/3251/chapter/144206775

The chapter’s abstract rejects treating manorial customary law as a clone of the common law.

Audit conclusion: a locality/institution-specific custom context is a justified simulation requirement. This does not imply local custom was arbitrary or disconnected from wider legal institutions.

### Succession/disposition offered structured options, not one universal hard-coded heir transition

Re-checked Lloyd Bonfield and L. R. Poos, “The Development of the Deathbed Transfers in Medieval English Manor Courts”:

https://academic.oup.com/book/3251/chapter-abstract/144207305

The chapter examines customary tenants’ inheritance/disposition strategies, their transmission options and their interaction with lordly interests.

Audit conclusion: succession/transfer should be represented as a process that resolves applicable rights and conditions. The source does not establish a universal succession ordering or probability distribution.

### Having a common right and exceeding its scope are different states

Re-checked Spike Gibbs, “Manorial Officeholding and Village Governance: Misconduct and Landscape Control”:

https://www.cambridge.org/core/books/lordship-state-formation-and-local-authority-in-late-medieval-and-early-modern-england/manorial-officeholding-and-village-governance-misconduct-and-landscape-control/B54CAEE9A3B92A11CF7629BC90EC129A

The chapter records outsiders presented in 1366 and 1445 for commoning sheep where they had no common rights. It separately describes a 1426 case in which men from Ely were accused of overburdening a common, with the distinction indicating that they were exceeding legitimately held rights rather than commoning without any right.

Audit conclusion: `right exists` and `action remains within right scope` must be distinct checks. The contract’s explicit `scope` field and authorization logic are supported.

### Private claims can coexist with collective/institutional resource management

Re-checked Jordan Claridge and Spike Gibbs, “Waifs and Strays: Property Rights in Late Medieval England,” _Journal of British Studies_ 61(1) (2022):

https://www.cambridge.org/core/journals/journal-of-british-studies/article/waifs-and-strays-property-rights-in-late-medieval-england/148ADDD32647806A4793D0AB2933F888

The article uses court-roll evidence from 1274–1453 and describes a stray-livestock institution that protected owners’ claims to livestock while also protecting shared arable interests from wandering animals.

Audit conclusion: the contract is justified in separating private claims from institutional/common-resource governance. The stray system is evidence for coexistence of interests, not a universal dispute mechanism for every asset.

### `Tenure` is a simulation abstraction, not a literal universal medieval ontology

Re-checked Susan Reynolds, “Tenure and property in medieval England,” _Historical Research_ 88(242) (2015):

https://academic.oup.com/histres/article/88/242/563/5603024

Reynolds argues that later use of the word “tenure” can impede understanding of medieval English property law and derives partly from later historiography rather than medieval legal vocabulary/content.

Audit conclusion: `TenureHolding` is accepted only as an implementation abstraction for a holding relationship with claims/obligations. It must not become a claim that all property is one feudal hierarchy or that medieval actors used the simulation’s categories.

## Causal-model review

**PASS.**

The model separates:

`physical possibility/access -> recognized claim/right + applicable conditions -> authorization/violation -> action/consequence`

This prevents co-location, possession or UI/controller status from fabricating legal/social authority. Transfers, succession and permissions are modeled as state-changing processes with provenance rather than direct owner replacement.

The audit specifically accepts the distinction between `CanAttempt(action)` and `IsAuthorized(action)`. An unauthorized action may remain physically possible and later produce theft/trespass/breach/dispute consequences; authorization is not required to act as an invisible physics wall.

## Player/NPC symmetry review

**PASS.**

Controller type is not an input to physical possibility or authorization. Human-controlled and AI-controlled `Person` entities use the same rights, permissions, contracts, office, custom and physical-access rules.

## Ownership, rights and obligations review

**PASS.**

The contract correctly rejects household membership, settlement membership, profession/workplace assignment and co-location as universal rights. It permits multiple recognized interests over one subject and keeps obligations explicit rather than attaching them magically to person/profession classes.

The term “bundle of rights” is an explanatory simulation interpretation, not a claim about medieval terminology. The evidence supports the underlying separations—holding/occupation, transfer conditions, common-use rights, obligations and institutional enforcement—from which the multi-claim simulation model is inferred.

## Uncertainty / fixture-boundary review

**PASS.**

The following remain explicitly noncanonical or `MODEL_UNDERDEFINED`:

- exact free/customary/copyhold/lease world-generation classification;
- manor-specific inheritance and transfer algorithms;
- rents, labour services, heriots, entry fines and other dues;
- common-right quantitative limits/carrying capacity;
- detailed courts, evidence, sanctions and priority between competing claims.

Current prototype `_settlementOwnerId`, `ItemStack.OwnerId`, `HouseholdId`, `HomeId`, workplace/profession permissions and settlement-wide common access are correctly marked as fixture seams rather than historical model constraints.

## Long-horizon review

**PASS for structural scope; quantitative economy remains deferred.**

The contract does not claim to calibrate economic balance. Its future >=10-year requirement is structural: rights/holdings must survive births/deaths, household changes, migration, tenancy/transfer and save/load/replay without silent duplication, deletion, player privilege or settlement-wide ownership collapse. Historical rates and economic consequences remain dependencies of later contracts.

## CI on reviewed SHA

Reviewed SHA `1dc690d694b7537dd05b0d64d601f9ac5c4cc6fd`:

- `ci #127` — **success**
- `playable-prototype-gate #136` — **success**
- `proof-a-measure #122` — **success**

## Final verdict

**PASS.**

The structural Property / Tenure / Common Rights model is sufficiently evidenced and causally defined for its declared scope. It may be promoted from `REVIEW_REQUIRED` to `ACCEPTED`.

Acceptance does not authorize production implementation by itself and does not resolve the explicitly deferred manor-specific or quantitative models.