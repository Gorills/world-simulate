# Audit — Settlement Household Authority and Representation

Audit date: **2026-08-17**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/SETTLEMENT_HOUSEHOLD_AUTHORITY_REPRESENTATION.md`

Reviewed research/model SHA: `24d4ce2ca70454eab17b9012c2b4cbf3889d9f27`

## Scope

Independent audit of the structural household authority / representation model only. No production simulation code was reviewed or changed. This audit does not accept one universal medieval `HeadOfHousehold` office, one gender/age authority table, universal marital liability rules, guardianship/minority rules, detailed court procedure, internal bargaining algorithm, spending thresholds or household decision weights.

The accepted Person/Household, Property/Tenure/Common Rights, Exchange/Contracts/Migration and Intention/Task Selection contracts are treated as dependencies.

## Repository and CI

The exact reviewed SHA was branch HEAD `24d4ce2ca70454eab17b9012c2b4cbf3889d9f27` at audit time. The commit added only `DESIGN/MODELS/SETTLEMENT_HOUSEHOLD_AUTHORITY_REPRESENTATION.md`.

Required GitHub Actions on the reviewed SHA all passed:

- `ci #141` — success;
- `playable-prototype-gate #164` — success;
- `proof-a-measure #136` — success.

## Load-bearing fact re-check

### Household hierarchy and labour expectations existed, but do not establish universal legal power

Re-checked Christopher Dyer, _Peasants Making History: Living in an English Region 1200–1540_, chapter 5, “Family and household” (Oxford University Press, 2022):

https://academic.oup.com/book/43934/chapter/370549926

The chapter describes households usually centred on parents and children but with variants including siblings, older relatives and unrelated servants. Its abstract explicitly states that norms and adult male household heads mattered to family life and that expectations of discipline and hierarchy were intended to secure household labour and orderly succession.

Audit conclusion: household hierarchy and labour expectation are supported as real causal inputs. The source does **not** establish a universal legal right of one household head to alienate every associated asset, bind every competent adult to contracts or dispose of all member labour. The contract correctly separates household coordination from scoped authority.

### Coverture and married-woman contracting require contextual resolution

Re-checked Cordelia Beattie, “Married Women, Contracts and Coverture in Late Medieval England,” in _Married Women and the Law in Premodern Northwest Europe_ (2013):

https://www.cambridge.org/core/books/abs/married-women-and-the-law-in-premodern-northwest-europe/married-women-contracts-and-coverture-in-late-medieval-england/31E09DF2ABD29B9AB3FBFF9E71DC2EC9

The accessible chapter summary frames married women's market activity, household provisioning, credit and the question of who is liable when a married woman contracts. The wider volume explicitly treats variation in how coverture operated in practice.

Audit conclusion: this source is suitable for the **structural** claim that marital status/coverture and liability cannot be represented by either `wife has no agency` or `spouse may bind spouse universally`. The accessible summary alone is not used to derive a universal provisioning-authority or debt-liability rule. Exact marital liability remains deferred.

### Late-medieval women appeared as litigants in trade and household-provisioning matters

Re-checked Teresa Phipps, “Coverture and the Marital Partnership in Late Medieval Nottingham: Women's Litigation at the Borough Court, ca. 1300–ca.1500,” _Journal of British Studies_ 58(4) (2019):

https://www.cambridge.org/core/journals/journal-of-british-studies/article/abs/coverture-and-the-marital-partnership-in-late-medieval-nottingham-womens-litigation-at-the-borough-court-ca-1300ca1500/5DE10A526354BDA9FAD10B58AC005663

The article abstract states that women appeared as both plaintiffs and defendants for reasons including trade and household provisioning, and that coverture and marital status shaped their legal experience.

Audit conclusion: married/female legal-economic action existed while its form and liability depended on marital/institutional context. This supports distinguishing actor, principal, representation basis and resulting obligations rather than collapsing spouses into one actor or making sex/marriage a complete permission table. Nottingham borough procedure is not promoted to a universal rural rule.

### Peasant women could hold economic claims and pursue debt litigation in manorial contexts

Re-checked Miriam Müller, “Peasant Women, Agency and Status in Mid-Thirteenth- to Late Fourteenth-Century England: Some Reconsiderations,” in _Married Women and the Law in Premodern Northwest Europe_ (2013):

https://www.cambridge.org/core/books/abs/married-women-and-the-law-in-premodern-northwest-europe/peasant-women-agency-and-status-in-midthirteenth-to-late-fourteenthcentury-england-some-reconsiderations/80EE86C10BCB0A6ACE10A5C60DFE4463

The chapter summary reconstructs Agnes de Schonedon from Heacham manorial court rolls. Agnes pursued multiple debt claims, including payment owed for ale she had sold, and the records show continuing brewing/economic activity.

Audit conclusion: a universal male-only external representative would erase documented rural female economic/legal agency. The evidence does **not** establish unrestricted autonomy for every married woman or one household-property regime.

## Causal model review

**PASS.**

The accepted topology is:

`household/person pressure or plan + relevant principals + rights/claims + relationship/status/custom + explicit role/delegation/holding authority -> scoped authority/representation options -> proposal/decision -> consent/acceptance where required -> action/contract/resource or labour commitment -> obligations/claims attach to correct principal(s) -> consequences/history`

The model correctly distinguishes:

- household coordination from execution authority;
- actor/representative from principal;
- representation authority from the principal's underlying property/right;
- request/expectation from enforceable obligation;
- routine provisioning/use authority from sale/pledge/permanent transfer authority;
- service/household labour expectations from ownership of a person's time.

No hidden `HouseholdId -> universal powers` transition is required.

## Player/NPC symmetry review

**PASS.**

HumanController and AIController act through the same `Person`, rights, representation grants, principals and contract consequences. Player control cannot grant household leadership, command another member's labour, expose all household assets, incur debt for others or override tenure/custom restrictions.

## Rights, obligations and representation review

**PASS.**

The contract preserves the accepted Property/Tenure distinction that representation cannot create a right the principal does not have. Exchange/Contracts remains authoritative for obligations and explicit counterparties. An actor representing another principal must preserve the authority basis and resulting obligor/beneficiary identities.

Membership, marriage, co-location and kinship remain insufficient by themselves to authorize sale, pledge, debt creation or external labour commitment for another competent adult.

## Uncertainty and fixture-boundary review

**PASS.**

The following remain explicit `MODEL_UNDERDEFINED` or deferred rather than being filled with universal constants:

- exact intra-household authority by sex, age, wealth, marital status and tenure;
- detailed coverture/marital liability by jurisdiction;
- minority, dependency, guardianship and capacity rules;
- internal bargaining/consent protocols;
- domestic coercion/enforcement;
- household sharing rules by resource type;
- spending/credit thresholds;
- apparent authority and third-party reliance.

Prototype patterns such as `Household.HeadId -> owns/commands all`, universal spouse authority, universal female incapacity, servant ownership or player-only household command remain noncanonical.

## Long-horizon review

**PASS for structural contract acceptance, not for implemented economic viability.**

Because household authority can materially alter resources, labour, contracts and debt, any implemented economic system using this contract remains subject to the Reality Modeling Policy's >=10 simulated-year proof. Required future invariants include correct debt incidence, no cloning/erasure of authority on membership changes, finite labour, explicit authority termination, and persistence of principal/representation provenance.

This audit does not claim calibrated household bargaining, marital authority or long-run economic balance.

## Remaining blockers outside this contract

- context-specific marital/coverture/property/liability rules where a mechanic needs them;
- dependency/minority/guardianship/legal capacity;
- detailed information/apparent-authority mechanics;
- P3 semantic travel remains separately underdefined until its own model is repaired and audited;
- quantitative economy/demography and >=10-year integrated evidence remain later gates.

## Final verdict

**PASS.**

The structural household-authority model is sufficiently grounded and causally bounded for its declared scope. No evidence blocker requires a repair before promotion to `ACCEPTED`.

`ACCEPTED` does not authorize production code to invent the deferred legal, marital, dependency or quantitative rules.