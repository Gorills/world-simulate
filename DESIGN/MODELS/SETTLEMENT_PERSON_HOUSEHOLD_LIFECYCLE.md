# Settlement Person, Household and Life-Cycle — Model Contract

Status: **ACCEPTED**

This contract defines the minimum causal structure for ordinary people and households. It does **not** define numerical fertility, mortality, marriage-age, disease or household-size distributions; those remain `MODEL_UNDERDEFINED` and require separate evidence before demographic behavior becomes canonical.

## Mechanic

Represent an ordinary person as a persistent world actor who can belong to, leave, join or help form a household through ordinary life processes rather than through a fixed NPC class or player-only rule.

## Intended feeling

A person should feel like a member of a living society with changing dependencies, work, residence, kinship and economic ties. The player should be able to take control of the same kind of person that AI controls, without receiving a different simulation ontology.

## Real-world process

Late-medieval rural people lived in domestic/economic groups that commonly combined family life, consumption, labour, residence and succession, but household membership was not identical to kinship. Households could include children, older relatives, siblings and unrelated servants; people could also migrate, enter service, marry, inherit, leave a household, join another one or establish a new domestic unit.

The simulation therefore needs to distinguish:

- a **Person** as a persistent individual;
- **kinship/social relationships** between people;
- **HouseholdMembership** as a time-bounded domestic/economic relationship;
- **residence, property, access and maintenance rights** as separate claims rather than consequences of identity alone;
- **labour/employment/service obligations** as relationships that may cross household boundaries;
- life events and decisions that can change these relationships over time.

## Reference context

Baseline: rural England, approximately 1270–1450, with evidence drawn from the West Midlands, Essex and Yorkshire and from English manorial records more generally.

This is a causal reference model, not a claim that one English household pattern was universal across all regions, classes or centuries. The period spans the Black Death and major changes in land/labour conditions, so pre- and post-1348 behavior must not be collapsed into one set of constants.

## Evidence and sources

1. **Christopher Dyer, _Peasants Making History: Living in an English Region 1200–1540_, chapter 5, “Family and household” (Oxford University Press, 2022).** The chapter describes households commonly centred on parents and children, often with roughly four to six members, while also documenting variants including siblings, older-generation relatives and unrelated servants. It also connects household organization to labour and succession. This supports treating household composition as variable and economically meaningful, not as a fixed two-person fixture.
   - https://academic.oup.com/book/43934/chapter/370549926

2. **P. J. P. Goldberg, _Women, Work, and Life Cycle in a Medieval Economy: Women in York and Yorkshire c.1300–1520_, chapter 4, “Servants and Servanthood” (Oxford University Press, 1992).** Goldberg treats servanthood in life-cycle terms and as part of the labour force, supporting the distinction between kinship and household/economic membership and the possibility that a person can enter service rather than immediately form an independent household.
   - https://academic.oup.com/book/7906/chapter/153160760

3. **L. R. Poos, _A Rural Society after the Black Death_, chapter 7, “Marriage and household formation” (Cambridge University Press, 1991; Cambridge Core online edition 2009).** For late-medieval Essex, marriage is described as a process with legal, property and demographic consequences rather than a single instantaneous state change. This supports modeling household formation as a process affected by ordinary world relationships and resources instead of `age -> household`.
   - https://www.cambridge.org/core/books/abs/rural-society-after-the-black-death/marriage-and-household-formation/0070217F3BFE1064F1AB089223A13AB1

4. **Matt Raven, “Servile migration and seigniorial reaction in England: the serfs of Great Waltham and High Easter (Essex), c.1336–1361,” _Continuity and Change_ (2025).** Based on manorial court rolls, the article documents migration among people whose legal status theoretically constrained mobility. This supports migration as an ordinary process that may change household, work and residence relationships; it does not justify unrestricted movement for every person.
   - https://www.cambridge.org/core/product/FEE7266397005ACF06410A59D9CB33AD/core-reader

5. **The National Archives, “Manors and manorial records” and “A guide to manorial documents.”** These institutional guides explain that court rolls, surveys, rentals and related records contain information about household divisions, tenants, holdings, rents, services, property transfer and local social/economic structure. They also emphasize variation in tenure and custom between manors. This supports using explicit relationships/claims and treating regional rules as model context rather than universal constants.
   - https://www.nationalarchives.gov.uk/help-with-your-research/research-guides/manors
   - https://www.nationalarchives.gov.uk/archives-sector/finding-records-in-discovery-and-other-databases/manorial-documents-register/a-guide-to-manorial-documents/

6. **Zvi Razi, “The Demographic Transparency of Manorial Court Rolls,” _Law and History Review_ 5(2), and the Razi/Poos-Smith debate on demographic use of court rolls.** The debate demonstrates that manorial records are selective evidence and that demographic quantities such as population, mortality, replacement and marriage cannot safely be copied into the simulation as universal rates without a separate calibration method.
   - https://www.cambridge.org/core/journals/law-and-history-review/article/abs/demographic-transparency-of-manorial-court-rolls/C2740B4535B6B657EAFB6E553DE97555
   - https://www.cambridge.org/core/journals/law-and-history-review/article/use-of-manorial-court-rolls-in-demographic-analysis-a-reconsideration/653C47B9F4C520541E4E9DCD6F341E25

### Evidence limits and disagreement

- Household size and composition varied by region, wealth, life stage and historical moment. Dyer's reported common pattern is evidence for variability and plausible scale, not a spawn-table distribution.
- Service was important but its frequency, sex balance, age profile and rural/urban incidence varied. No fixed “everyone serves between ages X and Y” rule is accepted.
- Marriage and household formation were related but not identical. Marriage must not automatically create a new household if residence, resources or existing household arrangements imply otherwise.
- Manorial court rolls observe some people and events better than others. Numerical demography remains outside this contract.
- Serfdom, tenure and local custom affected mobility differently between communities. Migration is possible world behavior, not an unconditional right to relocate.

## Causal model

Stable shape:

`person state + existing relationships + household pressures + rights/obligations + available residence + work/service opportunities + life events + social decisions -> household/life-cycle intention -> feasible transition/action -> changed membership/residence/obligations/resources/history`

Examples:

- a household needs labour and can support another resident -> it may seek a servant or accept a dependant;
- a young person has an employment/service opportunity elsewhere and the required freedom/permission -> they may leave their current household and migrate;
- partners decide to marry, but formation of a separate household additionally depends on a viable residence/economic arrangement;
- death or incapacity removes labour/maintenance capacity and may trigger dependency, succession, migration, household merger or dissolution;
- inheritance or tenancy access may make independent household formation feasible;
- loss of residence, work or maintenance may make remaining in a household infeasible.

Clock time and age can constrain these processes, but neither is by itself a motive or sufficient cause.

## Player/NPC symmetry

The authoritative actor is `Person`, regardless of controller.

Conceptual direction:

`AIController -> Person`

or

`HumanController -> Person`

Changing controller must not change the person's:

- identity;
- age/health;
- household membership;
- kinship/social relationships;
- property or access rights;
- possessions;
- obligations/contracts/debts;
- location;
- history.

A human-controlled person may choose among feasible actions differently from AI, but may not join a household, take property, occupy a home, abandon an obligation or command another person solely because they are player-controlled.

## Ownership, rights and obligations

Household membership is not universal ownership.

The model must be able to represent at least these distinctions, even if a later rights contract defines their exact schema:

- a person may own or hold a claim to property individually;
- multiple people may have claims or use rights over the same subject;
- a household may have shared-use rules for specified resources;
- membership may justify access only where an explicit household/resource rule says members share access;
- residence requires an ownership, tenancy, household, service, permission or comparable ordinary-world basis;
- kinship alone does not automatically grant possession or residence rights;
- employment/service can create maintenance, lodging, labour or wage obligations without creating kinship.

## Player decision

The player may face ordinary life choices such as:

- remain with the current household or seek service/work elsewhere;
- accept or refuse an employment/service arrangement;
- contribute labour/resources to household needs;
- seek permission, tenancy or lodging;
- form a new household when relationships and material conditions make it feasible;
- invite or accept another person into an existing household if authority/resources permit;
- migrate when expected opportunities justify the costs and constraints.

These choices must compete with needs, obligations, risks and opportunities; they are not menu verbs that succeed automatically.

## Rules

### Person

A canonical person eventually requires, at minimum:

- stable identity;
- age/birth information;
- health/capability state;
- skills or learned capabilities;
- current household membership, if any;
- kinship/social relationship references;
- property/right/obligation references;
- possessions/resource claims;
- current intention/task;
- authoritative location;
- persistent life history/events.

`farmer`, `servant`, `miller`, `brewer`, `carpenter` or similar labels are not fundamental mutually-exclusive person types. They may be derived from sustained activity, skills, contracts, tenure, office or social status.

### Household

A household is a persistent domestic/economic coordination entity, not merely a residence pointer. It needs to support:

- current and historical memberships;
- responsibility for daily maintenance/consumption of members or dependants;
- a labour pool composed of available members, without owning those people;
- access to residence and resources through explicit rights/claims;
- household-level pressures such as food, care, rent/debt and labour needs;
- formation, split, merge and dissolution events;
- continuity when individual members die, leave or join.

### Household membership

`HouseholdMembership` is conceptually separate from `Kinship`.

V1 may use one **primary domestic/economic household** per person at a time for routine maintenance/residence coordination. Kinship, employment, debt, partnership and other obligations can connect that person to other households. This one-primary-household simplification is a review target, not a universal historical claim.

A membership transition must record a cause/event and effective time. Membership must not silently change because a person travelled to another settlement.

### Formation and dissolution

No fixed `age -> form household` rule is accepted.

Potential causes that can make formation feasible include marriage/partnership, access to residence, tenancy/inheritance, sufficient income/resources, leaving service, or deliberate separation from an existing household. None is individually universal or sufficient.

A household may dissolve when it no longer has continuing members or when its members deliberately merge into other households. Death of one member does not automatically destroy the household.

## Long-horizon behavior

This contract defines lifecycle structure but not demographic event rates, so it cannot by itself satisfy a demographic `long_horizon` PASS.

When implemented, a structural >=10-year proof should at minimum demonstrate that externally supplied or separately modeled births, deaths, marriages/partnerships, service episodes and migrations can occur without:

- losing person identity;
- orphaning property/rights/obligations;
- silently granting household property to new members;
- leaving dead/departed people in the active labour pool;
- forcing every adult into a household of identical shape;
- making player-controlled people exempt from lifecycle transitions.

Population growth/decline, fertility, mortality and marriage frequencies require a separate accepted demographic model before their trajectories are judged historically plausible.

## Assumptions and uncertainty

- Exact fertility, mortality, disease and marriage-age distributions: **MODEL_UNDERDEFINED**.
- Exact historical probability of entering service by age/sex/region: **MODEL_UNDERDEFINED**.
- Exact household-size distribution for the reference world: **MODEL_UNDERDEFINED**.
- Exact authority structure inside a household: deferred; medieval English households were often hierarchical, but a single universal `HeadOfHousehold` power set would over-generalize social, gender and tenure variation.
- One primary domestic/economic household per person is a deliberate V1 simplification and must be revised if later evidence or gameplay requires concurrent primary membership.
- This contract does not yet define marriage law, inheritance rules, tenancy, common rights or detailed employment contracts; those require linked model contracts.

## Fixture boundary

The following current prototype facts are not evidence for this model and must not become canonical constraints:

- six households of exactly two residents;
- household identity derived from a named cottage;
- one fixed home per household for all time;
- `Farmer/Cook/Forager` as exhaustive person types;
- one fixed workplace per person;
- migration meaning only “remove old HouseholdId and WorkplaceId”;
- a separate authoritative player species with different inventory/interaction powers;
- fixed hourly routines used to infer household presence.

## Falsifiers

Revise this model if evidence or implementation shows that:

- household membership must be identical to kinship to reproduce the reference context;
- ordinary service/migration cannot be represented without a special human species;
- household formation can be explained adequately by age alone across the reference context;
- concurrent domestic/economic memberships are common enough that the V1 one-primary-household simplification breaks important causal behavior;
- resource ownership or residence can only be modeled by granting implicit universal rights to all household members;
- long-horizon runs systematically create impossible household transitions even when demographic events are externally valid.

## Feedback

Immediate presentation may show a person's current household, residence basis, dependants, current work/service relationship and active household pressures when known to the observer.

History/trace should preserve important lifecycle transitions such as joining/leaving a household, migration, service start/end, partnership/marriage linkage, household formation/merge/dissolution and death.

Visibility of private relationships or obligations is a later information-model question; UI projection must not imply omniscient player knowledge by default.

## Persistence

Persist enough authoritative state to reconstruct exactly:

- person identity and lifecycle state;
- household identity;
- current membership relations and their effective history where required;
- kinship/social links required by accepted mechanics;
- linked rights/obligations/contracts;
- location and current intention/task;
- lifecycle events needed for deterministic replay.

Do not reconstruct household membership, residence or person activity from clock hour.

## Input flow

Not defined in this foundation contract. Controllers submit ordinary-world intentions/actions against a `Person`; input mapping belongs to the relevant gameplay mechanic.

## Projection/UI

Godot may receive projections of person identity, household membership, lifecycle state, current task/location and observable relationships. It must not own authoritative membership, rights, lifecycle transitions or controller privilege.

## Acceptance scenario

A future implementation acceptance scenario should use the same person through a sequence such as:

1. Person A begins as a member/dependant of Household H1.
2. An ordinary service/work opportunity with lodging becomes available in another household or settlement.
3. AI or human controller selects that feasible opportunity.
4. Person A travels using ordinary semantic travel rules, leaves H1 at the effective transition point and enters the new domestic/economic arrangement without losing identity or kinship links.
5. Later, an accepted ordinary-world cause (for example partnership plus viable residence/tenure) makes a new household feasible.
6. Person A forms or joins H2; property and access are transferred only through explicit accepted rights/actions, not through player status or membership magic.
7. Save/load/replay reproduces the same memberships, rights references, location and lifecycle history.

The scenario proves structural causality and player/NPC symmetry. It does not validate demographic rates.

## Deferred complexity

Deliberately deferred to separate bounded contracts:

- land tenure, ownership bundles and common rights;
- detailed marriage law and inheritance/succession;
- numerical demography, disease and mortality;
- agricultural household production and food allocation;
- hired labour, servant contract terms and wage formation;
- detailed social relationship/reputation models;
- household authority/governance beyond the minimum access/obligation requirements.

Deferring these does not invalidate this contract because this step only establishes the identity, membership and lifecycle topology that those later models must attach to.
