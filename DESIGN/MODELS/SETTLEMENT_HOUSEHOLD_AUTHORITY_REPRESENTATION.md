# Settlement Household Authority and Representation — Model Contract

Status: **ACCEPTED**

This contract defines the minimum causal structure for authority, representation and household-level coordination. It answers who may act for themselves, who may act on behalf of another person or a household-scoped interest, and when a household pressure or plan may legitimately create obligations, resource commitments or labour expectations.

It does **not** define one universal medieval `HeadOfHousehold` office, one gender/age hierarchy, universal marital powers, child/guardian law, exact intra-household bargaining, inheritance procedure, domestic violence/coercion mechanics, detailed court procedure or quantitative household decision weights. Those remain separate context/model questions and are `MODEL_UNDERDEFINED` where material.

## Mechanic

Represent a household as a persistent domestic/economic coordination entity whose members, resources, obligations and plans may overlap, but whose identity does not automatically grant one member power over every other member or every associated asset.

The canonical authority seam is:

`household/person need or plan + applicable relationship/status/custom + rights over the subject + explicit/delegated/role authority where applicable -> scoped authority to act or represent -> proposal/decision/action -> resource/labour/contract consequence -> obligation/claim attaches to the correct principal(s) -> history`

There is no canonical transition of the form:

`HouseholdId -> HeadOfHousehold -> may command all members and dispose of all household assets`.

## Intended feeling

A household should feel coordinated without becoming a hive mind or a corporation with magical powers:

- food or rent pressure can be a household-level fact;
- one member may ordinarily procure necessities or manage a specified store under an accepted household rule;
- a tenant/holder may have authority over a holding because of tenure, not merely because they belong to the household;
- a spouse may transact, litigate, provision or manage within historically/contextually appropriate constraints rather than being universally powerless or universally autonomous;
- a servant can owe work under a service agreement without becoming owned by the employer or household;
- a parent/guardian relationship may later create special authority over dependants, but only through an accepted age/dependency/legal-capacity model;
- a person cannot promise another competent member's labour, sell their individually held asset or incur debt for them merely because both share a household;
- HumanController and AIController use the same authority and representation rules.

## Dependencies

This contract reuses the accepted foundations:

- `DESIGN/MODELS/SETTLEMENT_PERSON_HOUSEHOLD_LIFECYCLE.md` — household membership is time-bounded, separate from kinship and ownership; household is a domestic/economic coordination entity, not a residence pointer;
- `DESIGN/MODELS/SETTLEMENT_PROPERTY_TENURE_COMMON_RIGHTS.md` — possession, use, transfer and management rights are explicit and action-specific;
- `DESIGN/MODELS/SETTLEMENT_EXCHANGE_CONTRACTS_MIGRATION.md` — contracts identify parties and represented principals where applicable; obligations persist and attach to explicit holders;
- `DESIGN/MODELS/SETTLEMENT_INTENTION_TASK_SELECTION.md` — household requests/expectations may become task reasons only when their authority/basis is accepted.

The cross-model audit `DESIGN/MODEL_AUDITS/2026-08-17_SETTLEMENT_FOUNDATIONS_CROSS_MODEL.md` identified household authority/representation as a production blocker.

### Boundary with task selection

This contract determines whether a household-originated request, expectation, representation or commitment has an accepted basis and scope.

It does **not** force performance. Once a valid expectation/obligation/request becomes a reason, the accepted Intention/Task Selection contract governs candidate generation and controller choice. A person may still delay, refuse, breach or neglect when physically/socially possible, with ordinary consequences.

### Boundary with property and contracts

This contract does not replace rights or contract law.

Authority to represent a principal answers **who may validly attempt to act for whom**. The Property/Tenure contract still answers whether the principal has the relevant right over the subject. The Exchange/Contracts contract still answers what obligations are formed and how they are performed/disputed.

For example, being authorized to represent Household H does not create a transfer right over Person P's individually held cow.

## Reference context

Baseline: rural lowland England, approximately **1270–1348** for first calibration, with **1350–1450** retained as a separate stress/validation regime. Evidence also uses late-medieval English urban/borough material where it illuminates legal agency and marital representation; those urban examples are used structurally, not as direct rural rule tables.

The evidence supports a household world with hierarchy, interdependence, provisioning roles and legally/socially constrained agency. It does **not** support one universal household constitution or one immutable male-head API across all status, marital and tenure contexts.

## Evidence ledger

### 1. Household hierarchy and labour expectations were real, but household form varied

**Christopher Dyer, _Peasants Making History: Living in an English Region 1200–1540_, chapter 5, “Family and household” (Oxford University Press, 2022).** Dyer describes households typically centred on parents and children but also including other relatives and unrelated servants. He notes the role of adult male household heads and expectations of discipline/hierarchy intended in part to secure household labour and orderly succession.

- https://academic.oup.com/book/43934/chapter/370549926
- Supports: household hierarchy, labour expectations and household-level coordination can be real causal inputs.
- Supports: household structure is variable rather than one fixed pair/cottage form.
- Does **not** establish one universal legal power set for a `HeadOfHousehold`, identical authority in every household, or a rule that every household member's labour/property is disposable by one actor.
- Simulation consequence: hierarchy/expectation requires an explicit basis/scope; it is not inferred solely from `HouseholdId`.

### 2. Married women's contractual/economic capacity cannot be reduced to either full independence or zero agency

**Cordelia Beattie, “Married Women, Contracts and Coverture in Late Medieval England,” in _Married Women and the Law in Premodern Northwest Europe_ (Boydell & Brewer, 2013).** Beattie's chapter examines married women's contracting under coverture and explicitly frames practical questions around household provisioning, credit and liability.

- https://www.cambridge.org/core/books/abs/married-women-and-the-law-in-premodern-northwest-europe/married-women-contracts-and-coverture-in-late-medieval-england/31E09DF2ABD29B9AB3FBFF9E71DC2EC9
- Supports: household provisioning and external transactions could be performed by wives, while legal liability/contract capacity depended on marital/legal context.
- Supports: `married woman -> no economic action` is unsafe, as is `married person -> fully interchangeable household representative`.
- Does **not** provide a universal transaction threshold, one coverture implementation for every jurisdiction or one rule for all rural households.

### 3. Marital partnership and court representation were context-dependent and inconsistently expressed

**Teresa Phipps, “Coverture and the Marital Partnership in Late Medieval Nottingham: Women's Litigation at the Borough Court, ca. 1300–ca.1500,” _Journal of British Studies_ 58(4) (2019).** Phipps documents women as plaintiffs and defendants in matters including trade and household provisioning and emphasizes that marital status/coverture shaped legal experience. The article also highlights inconsistency in how marital partnership was represented in litigation.

- https://www.cambridge.org/core/journals/journal-of-british-studies/article/abs/coverture-and-the-marital-partnership-in-late-medieval-nottingham-womens-litigation-at-the-borough-court-ca-1300ca1500/5DE10A526354BDA9FAD10B58AC005663
- Supports: representation/liability cannot be derived from sex or marriage alone; legal/institutional context and the particular transaction matter.
- Supports: spouses can have intertwined interests without becoming one simple simulation actor.
- Does **not** establish Nottingham borough procedure as the rule for lowland rural manors.

### 4. Peasant women could hold economic claims and litigate debts in manorial contexts

**Miriam Müller, “Peasant Women, Agency and Status in Mid-Thirteenth- to Late Fourteenth-Century England: Some Reconsiderations,” in _Married Women and the Law in Premodern Northwest Europe_ (2013).** Using manorial court rolls, Müller reconstructs women participating in economic activity and debt litigation, including the case of Agnes de Schonedon, who sold ale and pursued multiple debt claims.

- https://www.cambridge.org/core/books/abs/married-women-and-the-law-in-premodern-northwest-europe/peasant-women-agency-and-status-in-midthirteenth-to-late-fourteenthcentury-england-some-reconsiderations/80EE86C10BCB0A6ACE10A5C60DFE4463
- Supports: rural female economic/legal agency existed and can be visible as individual claims/actions; a universal male-only external representative would erase documented behaviour.
- Does **not** establish full autonomy for every married woman, one household-property regime or one probability of female market/legal action.

### 5. Legal agency was structured by overlapping status, procedure, property and relationships

**Alexandra Shepard and Tim Stretton, “Women Negotiating the Boundaries of Justice in Britain, 1300–1700: An Introduction,” _Journal of British Studies_ 58(4) (2019).** The synthesis emphasizes procedural, jurisdictional, marital-property and status boundaries on legal agency and warns against simple generalizations about autonomous versus dependent action.

- https://www.cambridge.org/core/journals/journal-of-british-studies/article/women-negotiating-the-boundaries-of-justice-in-britain-13001700-an-introduction/733BE4EA5A17E8C209B2CF78B8FD7740
- Supports: authority/representation should be resolved from multiple explicit relationships/context rather than one demographic flag.
- Supports: people can act individually, as part of family interests, or through constrained/represented positions.
- Later-period material in the special issue is not back-projected as a rural 1270 rule table; only the structural warning against one-dimensional agency is reused.

### Evidence limits and disagreement

- Household history and legal history use different evidence and vocabularies. A domestic hierarchy does not automatically equal a legal power to alienate property or bind every member to a contract.
- Surviving court records overrepresent disputes, enforceable claims and formal appearances; informal household cooperation is less visible.
- Coverture was real but historically complex in operation. The simulation must not encode either `wife has no agency` or `marriage changes nothing` as a universal law.
- Adult male headship was an important norm in Dyer's reference material, but widowhood, female tenancy/economic action, servants, multi-generational households and local custom make a single universal power set unsafe.
- Authority over children/dependants changes with age, capacity, guardianship and local/legal context. This contract does not invent those rules before an accepted dependency/capacity model exists.
- Internal household bargaining, affection, coercion and informal delegation are poorly suited to one universal historical algorithm. V1 preserves explicit authority provenance without claiming to reconstruct private negotiations.

## Causal model

Stable shape:

`household/person pressure or plan + relevant members/principals + rights/claims over affected subjects + relationship/status/custom + explicit role/delegation/holding authority -> scoped authority/representation options -> controller/person proposal or decision -> consent/acceptance where required -> action/contract/resource or labour commitment -> obligations/claims attach to correct principal(s) -> consequences/history -> updated authority/relationships`

Household state can therefore generate needs and coordination pressure without directly executing world actions.

## Core distinctions

### Household coordination is not universal legal/person authority

The `Household` may own simulation-level coordination state such as:

- maintenance/consumption pressure;
- known shared needs;
- planned household expenditure or resource reservation;
- known member availability;
- residence coordination;
- household-scoped obligations when an accepted basis creates them;
- requests/expectations issued through accepted relationships.

This does not mean the Household object itself may automatically:

- transfer every associated asset;
- contract every member's labour;
- incur debt for every member;
- surrender an individual's tenancy/right;
- admit/remove members without applicable authority/consent;
- waive another person's claim;
- authorize trespass/use merely because it benefits the household.

### Principal

A `Principal` is the person, holding, institution or explicitly household-scoped interest whose right/obligation is being affected.

A transaction/contract/decision must identify its actual principal(s) when representation matters.

Examples:

- Person A buys food for themselves -> A is principal.
- Person A buys food under an accepted household provisioning authority using a household-scoped fund -> household-scoped interest/recognized holders are principal according to the accepted resource rule; A is representative/actor.
- Tenant T agrees to a holding obligation within T's tenure authority -> T/holding is principal.
- Person A cannot make Person B principal to a labour contract merely because A and B share a household.

Exact storage schema for principal identity is implementation design; correct attachment of claims/obligations is canonical.

### RepresentationAuthority / AuthorityGrant

A representation/authority relation needs enough state to answer:

- actor/representative;
- principal(s) represented;
- source/basis;
- permitted action kinds;
- subjects/resources/holdings covered;
- quantity/value/scope limits where applicable;
- effective period/termination condition;
- whether further delegation is permitted where later modeled;
- whether consent/countersignature/other participant is required;
- applicable custom/institution/legal context;
- history/provenance.

Possible authority bases, only when established by accepted context, include:

- direct individual ownership/right;
- explicit permission/delegation;
- tenure/holding role;
- service/employment role;
- office/institutional role;
- marital/customary rule;
- guardianship/dependency rule after separately accepted modeling;
- explicit household sharing/provisioning rule.

The existence of these categories does not claim that every category applied uniformly in medieval England.

### Household plan/request

A household-level plan/request is coordination state, not an order with automatic force.

It may include:

- acquire food/seed/fuel;
- reserve grain;
- meet rent/debt;
- perform household agricultural work;
- provide care;
- repair residence/tool;
- seek paid work or credit;
- relocate/adjust household membership.

A request becomes a particular person's accepted obligation only through a valid relationship/rule/contract. Otherwise it is a pressure/request/reason that the controller may accept or reject with ordinary consequences.

## Resource authority

### Individually held resources

If Person P is the recognized holder with transfer authority over Resource R, another household member cannot sell, pledge, gift or consume R beyond any explicit shared-use/agency authorization.

Co-location, marriage, kinship and household membership are insufficient by themselves.

### Household-shared resources

A household sharing rule may authorize defined actions over specified resources, for example routine consumption or provisioning.

Such authorization must specify enough scope to distinguish:

- consume/use for maintenance;
- allocate/reserve;
- move/store;
- exchange/sell;
- pledge/use as security;
- permanently transfer title/claim.

Permission to consume household food does not automatically imply authority to sell the seed reserve or pledge a dwelling/holding.

### Resource acquisition

A representative may acquire a resource for a household-scoped purpose only when:

- the counterparty exists;
- consideration/credit authority is valid;
- the principal that will own/hold the acquired subject is defined;
- the resulting payment/debt obligation attaches to the correct principal(s).

There is no generic `HouseholdMoney` spending power merely because the actor is a member.

## Labour authority and expectations

A household has a labour pool in the sense that members may contribute labour, not in the sense that the household owns their time.

Possible bases for a person's household-related work reason include:

- an accepted dependency/maintenance relationship;
- an accepted service/employment agreement;
- a tenure/service obligation;
- a valid household expectation under the selected context;
- a voluntary request/cooperation agreement;
- an existing task/plan the person previously accepted.

Rules:

1. Membership alone does not permit one competent adult to create an external labour contract that binds another competent adult.
2. A representative can communicate a request or plan without automatically creating an obligation.
3. Where a valid authority/dependency rule allows assignment of work, its scope and consequence of refusal must be explicit.
4. Servanthood/service authority comes from the service relationship, not generic household ownership of the servant.
5. Labour already committed elsewhere remains finite; household expectations do not duplicate time.
6. Player control cannot make another member's labour directly commandable without the same accepted basis an AI-controlled person would need.

Exact authority over children/dependants is `MODEL_UNDERDEFINED` pending an accepted capacity/dependency model.

## Contracts, debt and representation

When Person A negotiates or signs/accepts a contract while representing another principal, the contract must preserve:

- actual acting person;
- represented principal(s);
- authority basis/scope;
- counterparties;
- obligations created and their obligors/beneficiaries;
- whether personal liability also arises where the accepted context says so;
- dispute state if authority is challenged.

A contract does not bind every household member merely because one member formed it.

If representation authority is absent, exceeded or disputed, the world should preserve the attempted action/dispute rather than silently treating the household as universally bound.

Exact medieval doctrines of marital liability, necessaries, agency and jurisdiction are deferred to context-specific contract/legal work where required.

## Household membership changes

Joining a household does not automatically grant all authority held by existing members.

Leaving a household does not automatically erase:

- individually held rights;
- contracts/debts;
- previously completed transfers;
- obligations whose basis continues;
- valid authority that has an independent continuing basis.

Authority based specifically on membership/residence/service may terminate when its basis ends; termination must be an event with provenance.

Death/incapacity of an authorized representative or principal requires ordinary succession/representation handling rather than silently transferring all authority to another household member.

## Player/NPC symmetry

HumanController and AIController operate on the same `Person`, household, rights, representation and contract structures.

Controller type cannot grant:

- automatic household leadership;
- authority over another member's labour;
- access to every household-associated asset;
- power to bind a spouse/servant/dependant outside accepted rules;
- authority to incur debt for the household without a valid basis;
- ability to override tenure/custom/property restrictions;
- immunity from disputes over exceeded authority.

A human-controlled person can make household proposals, accept responsibilities, decline requests, exceed authority or misuse resources when physically possible; ordinary world consequences apply.

## Rules

1. **Household coordination is real; household omnipotence is not.** Household needs/plans can generate pressure without directly executing actions.
2. **Membership does not equal representation.** `HouseholdId` alone never grants authority to bind another member or dispose of all associated assets.
3. **Authority is scoped and provenance-bearing.** Actor, principal, basis, subject/action scope and duration must be resolvable where authority matters.
4. **Rights remain authoritative.** Representation cannot grant a principal a right the principal does not have.
5. **Labour is not household property.** Household work expectations require an accepted relationship/basis and still consume finite person time.
6. **Contracts bind explicit principals.** One member's agreement does not silently create debt/obligation for every member.
7. **Provisioning authority is not universal alienation authority.** Routine access/consumption can be broader than sale, pledge or permanent transfer.
8. **Marital status is context, not a universal permission table.** Coverture/partnership constraints require the selected legal/custom model.
9. **Household plans are not forced AI orders.** The task-selection controller still chooses/executes through an ordinary person unless a valid obligation makes performance required with consequences for breach.
10. **Authority changes are events.** Delegation, role changes, membership changes, marriage, service start/end, death or succession must not silently rewrite authority.
11. **Disputed/exceeded authority is valid state.** The engine may represent unauthorized representation, challenge or breach rather than auto-normalizing it.
12. **Controller type never changes authority.** Human/AI differences are decision-policy differences only.
13. **No universal household head constant.** A default simulation field may exist for projection/content migration only if it is not treated as complete authority law.
14. **No unsupported quantitative/legal constants.** Spending limits, consent thresholds, age/capacity rules and liability allocations require accepted context/calibration.

## Knowledge and observability boundary

A person may know some household needs, shared resources and accepted authority relations through ordinary participation, but the simulation must not assume perfect household omniscience.

At minimum:

- a person knows authority they personally hold unless a modeled exception exists;
- known requests/obligations need provenance;
- private assets/claims of another member are not automatically visible merely through membership;
- a counterparty may rely on, question or misunderstand representation only through later accepted knowledge/legal mechanics;
- UI/debug projection must distinguish `can physically act`, `appears authorized`, `represents principal`, and `would affect another member's claim` where material.

Detailed belief/reputation/apparent-authority mechanics are deferred.

## Long-horizon requirement

This foundation can materially affect household economy, labour and debt. When implemented together with economic mechanics it must participate in the Reality Modeling Policy's **>=10 simulated-year proof**.

Long-horizon invariants must eventually include:

- household join/leave does not clone or erase resource authority;
- no person becomes universal owner/representative after death of another member;
- debts attach to the correct obligor(s) and survive unrelated membership changes;
- household-shared consumption does not silently convert into transfer title;
- no duplicate labour through simultaneous household/external commitments;
- representation authority terminates or transfers only through explicit accepted events;
- spouses/servants/dependants do not acquire or lose all agency through one boolean status flag;
- save/load/replay preserves authority basis, principal identity and resulting obligations;
- household viability does not depend on hidden player-only access to all resources/members.

A structurally accepted authority model is not itself evidence of historically calibrated household bargaining or economic balance.

## Assumptions and uncertainty

- Exact intra-household authority by sex, age, marital status, wealth and tenure: **MODEL_UNDERDEFINED** at calibration/context level.
- Detailed coverture and marital liability rules by jurisdiction: **MODEL_UNDERDEFINED** until a concrete applicable legal/custom context is selected for the mechanic.
- Authority/guardianship over minors and incapacitated persons: **MODEL_UNDERDEFINED** pending capacity/dependency modeling.
- Domestic coercion, violence and enforcement: deferred; household hierarchy must not be implemented as supernatural command authority.
- Exact household sharing rules for food, money, tools and livestock: require resource/context-specific acceptance.
- Whether the simulation stores a household as a direct contract principal for some household-scoped obligations is an implementation abstraction; if used, represented holders/authority and obligation incidence must remain explicit.
- Apparent authority, estoppel/reliance-like effects, witnessing and third-party good-faith rules: deferred to detailed legal/institution modeling if needed.
- Internal bargaining/consent protocol between spouses/adult members: **MODEL_UNDERDEFINED**.
- Exact spending/credit thresholds and who may pledge major assets: **MODEL_UNDERDEFINED**.

These are blockers only for mechanics that materially require the missing rule. They are not permission to choose convenient RPG/game constants.

## Fixture boundary

The following patterns are explicitly noncanonical:

- `Household.HeadId` automatically owning every household asset;
- `HeadOfHousehold.Command(member, task)` without an accepted authority/relationship basis;
- all household members sharing identical consume/sell/pledge permissions;
- marriage automatically merging all property and contracts into one inventory;
- wife universally unable to transact, litigate or hold claims;
- spouse universally able to bind the other spouse to any debt/contract;
- parent relationship automatically granting permanent control over an adult person's labour/property;
- service/servant status implying ownership of the servant;
- joining a household granting access to settlement/household money and stock;
- player-controlled household member being able to command AI members because they are the player;
- migration/household change wiping or granting representation rights without an event;
- household pressure directly setting member activity without controller/task-selection logic.

Existing fixture fields may remain as migration/projection seams but must not constrain canonical authority semantics.

## Falsifiers

Revise this model if evidence or implementation shows that:

- one universal household-head power set accurately covers the selected reference context without erasing documented individual claims/agency;
- household membership alone is sufficient to determine transfer, contract and labour authority;
- distinguishing principal from representative produces no meaningful difference for debts, property or labour commitments;
- household-level planning cannot coexist with individual controller choice and finite labour;
- marital/legal context can be represented accurately by a single unrestricted spouse-authority boolean;
- long-horizon runs require silent reassignment of all household authority on marriage, migration or death to remain coherent.

## Feedback and UI

Authorized debug/audit tooling should be able to expose:

- household need/plan that motivated an action;
- acting person;
- represented principal(s);
- authority basis and relevant scope;
- affected resource/right/contract;
- whether another member's consent/obligation is required;
- who becomes obligor/beneficiary after the action;
- authority termination/change events;
- disputes over exceeded or absent authority.

Player-facing UI should not show a generic `Command Household` power. It may present ordinary actions such as request, propose, authorize, spend, sell, hire, accept work or contract only when the controlled person's world-state authority supports them.

## Persistence

Persist enough authoritative state to reconstruct exactly:

- relevant household identities/memberships;
- authority/representation relations and basis;
- principal/representative linkage;
- resource-sharing permissions where material;
- authority effective period/termination;
- household-scoped plans/requests that affect current decisions;
- contracts/obligations created through representation;
- disputed/exceeded-authority history where it affects claims;
- controller-independent person identity.

Do not reconstruct authority from sex, `HouseholdId`, `HomeId`, profession, controller type or current location.

## Acceptance scenario

A future structural implementation should demonstrate:

1. Household H faces a food shortage and has a known household-level acquisition plan.
2. H contains Persons A and B. A has an accepted provisioning authority over a specified shared money/resource scope; B has an individually held asset outside that scope.
3. The household plan becomes a decision reason for A and/or a request to B; it does not directly assign either person's activity.
4. A may negotiate a food purchase using only authority and resources within A's representation scope. The resulting food holder and payment/debt principal are explicit.
5. A cannot sell or pledge B's individual asset merely because both belong to H.
6. If B voluntarily agrees to contribute/sell the asset, that consent/transfer is an explicit event and later consequences attach to the correct parties.
7. If H needs harvest labour, an existing valid service/household expectation may create a reason/obligation for a member, but one member cannot create an external labour contract binding another competent member without accepted authority/consent.
8. A contract formed by A as representative records A as actor, the represented principal(s), authority basis and resulting obligations. Exceeding authority can remain disputed world state.
9. HumanController may control A or B without changing any authority, ownership, labour or contract rule.
10. Save/load/replay preserves the household plan, authority grants, principals, resource rights and resulting obligations exactly.
11. Marriage, household join/leave or travel does not silently merge assets or recreate authority.

This proves the structural household-authority seam. It does not prove a universal medieval household constitution, calibrated gender/age authority rules or detailed marital liability law.

## Deferred complexity

Separate bounded work may still be required for:

- context-specific marital/coverture/property/liability rules;
- dependency, minority, guardianship and legal capacity;
- household formation/marriage consent and property transition detail;
- internal bargaining, coercion and conflict;
- apparent authority/reputation/third-party reliance;
- detailed court/dispute enforcement;
- calibrated household consumption/resource-sharing policy;
- integration with P3 travel once travel itself is accepted;
- >=10-year integrated economic validation for P5/P6.

Completing this structural contract does not authorize production code to invent any deferred authority rule.