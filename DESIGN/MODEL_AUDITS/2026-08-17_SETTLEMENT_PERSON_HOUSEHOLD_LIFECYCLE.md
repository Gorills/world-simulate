# Audit — Settlement Person, Household and Life-Cycle

Audit date: **2026-08-17**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/SETTLEMENT_PERSON_HOUSEHOLD_LIFECYCLE.md`

Reviewed research/model SHA: `e583d5c24a7174190f39991f7871def9fb9dba9c`

Acceptance/status-change SHA: `33fb127f003dffa64cb6d178f87c11f57350efb9`

## Scope

Independent audit of the Person / Household / life-cycle foundation only. No production simulation code was accepted or changed by this model audit. Quantitative fertility, mortality, disease, marriage-age and household-size distributions remained outside the accepted scope as `MODEL_UNDERDEFINED`.

## Load-bearing fact re-check

The audit independently reopened the evidence behind the claims that materially determine the model topology.

### Household is not identical to kinship

Re-checked Christopher Dyer, _Peasants Making History: Living in an English Region 1200–1540_, chapter 5, “Family and household” (Oxford University Press, 2022):

https://academic.oup.com/book/43934/chapter/370549926

The source supports a household commonly centred on parents and children while also documenting other relatives and unrelated servants. Audit conclusion: separating `HouseholdMembership` from kinship is supported; the source does not justify a universal household-size distribution.

### Servanthood can be a life-cycle/economic relationship

Re-checked P. J. P. Goldberg, _Women, Work, and Life Cycle in a Medieval Economy: Women in York and Yorkshire c.1300–1520_, chapter 4, “Servants and Servanthood” (Oxford University Press, 1992):

https://academic.oup.com/book/7906/chapter/153160760

The source supports treating service as a labour/life-cycle relationship rather than kinship or a permanent person class. Audit conclusion: a servant may participate in another household's domestic/economic life without making `servant` a fundamental simulation species. No universal age range or participation probability was accepted.

### Marriage does not justify `age -> new household`

Re-checked L. R. Poos, _A Rural Society after the Black Death_, chapter 7, “Marriage and household formation” (Cambridge University Press):

https://www.cambridge.org/core/books/abs/rural-society-after-the-black-death/marriage-and-household-formation/0070217F3BFE1064F1AB089223A13AB1

The source supports treating marriage/household formation as socially, legally and materially consequential processes rather than a single age-triggered state transition. Audit conclusion: the contract correctly requires additional ordinary-world causes such as residence/resources/tenure instead of accepting `age -> household` as law.

### Migration is ordinary possible behavior, not a special player/adventurer rule

Re-checked Matt Raven, “Servile migration and seigniorial reaction in England: the serfs of Great Waltham and High Easter (Essex), c.1336–1361,” _Continuity and Change_:

https://www.cambridge.org/core/product/FEE7266397005ACF06410A59D9CB33AD/core-reader

The manorial-court-roll evidence supports real migration even among people subject to legal constraints. Audit conclusion: migration belongs in the ordinary Person life-cycle model, but the source does not justify unrestricted movement or one universal mobility rate.

### Local tenure/custom variation must remain explicit

Re-checked The National Archives guidance on manors/manorial documents:

https://www.nationalarchives.gov.uk/help-with-your-research/research-guides/manors

https://www.nationalarchives.gov.uk/archives-sector/finding-records-in-discovery-and-other-databases/manorial-documents-register/a-guide-to-manorial-documents/

The institutional guidance supports explicit modeling of tenants, holdings, rents, services, property transfer and local custom and warns against treating one manor's customs as universal. Audit conclusion: household membership must not silently confer universal ownership/residence rights.

### Numerical demography remains underdefined

Re-checked the Razi / Poos-Smith methodological debate on demographic inference from manorial court rolls:

https://www.cambridge.org/core/journals/law-and-history-review/article/abs/demographic-transparency-of-manorial-court-rolls/C2740B4535B6B657EAFB6E553DE97555

https://www.cambridge.org/core/journals/law-and-history-review/article/use-of-manorial-court-rolls-in-demographic-analysis-a-reconsideration/653C47B9F4C520541E4E9DCD6F341E25

Audit conclusion: the records are valuable but selection/observability issues make it unsafe to promote one mortality, replacement, marriage or fertility table into a universal medieval constant without a separate calibration model. Keeping these quantities `MODEL_UNDERDEFINED` is correct.

## Model-dimension verdicts

- **Causal logic: PASS.** Household/life-cycle transitions arise from person state, relationships, household pressures, rights/obligations, residence/work opportunities and life events; age/time are constraints rather than sufficient causes.
- **Historical grounding: PASS for structural topology.** Multiple independent academic/institutional sources support the accepted structural distinctions. Numeric demographic calibration is explicitly excluded.
- **Player/NPC symmetry: PASS.** The accepted model uses ordinary `Person` state under either AI or human controller; controller identity does not grant household, property, migration or interaction powers.
- **Ownership/rights/obligations: PASS at foundation scope.** Household membership is explicitly not universal ownership; detailed tenure/common-right schemas remain a later contract.
- **Uncertainty/fixture boundary: PASS.** Two-person prototype households, `Farmer/Cook/Forager`, fixed workplaces/hours and separate player species are explicitly noncanonical fixtures.
- **Long horizon: NOT_APPLICABLE to quantitative plausibility in this contract.** The contract defines structural lifecycle topology but deliberately does not define demographic rates. A future implementation must support >=10-year identity/membership integrity, while demographic trajectory plausibility awaits an accepted demographic model.

## Deferred / still underdefined

The following were deliberately not accepted by this audit:

- fertility and mortality rates;
- disease incidence/effects;
- marriage-age distributions;
- household-size distributions;
- detailed marriage/inheritance law;
- exact service participation by age/sex/region;
- detailed tenure/common rights;
- household authority/governance rules.

Later contracts must research these independently when they become load-bearing.

## CI evidence

For reviewed model SHA `e583d5c24a7174190f39991f7871def9fb9dba9c`:

- `ci #122` — success;
- `playable-prototype-gate #126` — success;
- `proof-a-measure #117` — success.

For acceptance/status SHA `33fb127f003dffa64cb6d178f87c11f57350efb9`:

- `ci #123` — success;
- `playable-prototype-gate #128` — success;
- `proof-a-measure #118` — success.

## Final audit conclusion

`SETTLEMENT_PERSON_HOUSEHOLD_LIFECYCLE` is accepted only for the structural Person/Household/life-cycle model and declared reference context. This PASS does not authorize filling its explicitly deferred demographic, tenure, inheritance or labour-contract gaps with convenient constants.
