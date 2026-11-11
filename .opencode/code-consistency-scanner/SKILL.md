---
name: code-consistency-scanner
description: Audit this repository for duplicated code, inconsistent patterns, and missed shared utilities; return evidence-based refactoring recommendations without editing files unless asked.
---

# Code Quality and Consistency Scanner

Review the repository for duplication and consistency issues. Use `repository-implementation-conventions` as the source of truth for expected architecture and implementation patterns. This skill is an audit and recommendation workflow, not a second implementation guide.

## Review scope

- Detect copy-paste logic, near-duplicate functions, scattered constants/configuration, and unnecessary conditional duplication.
- Find existing utilities/composables/services that are underused or functionality that has been reimplemented.
- Check naming, error handling, module boundaries, and idiom consistency against repository conventions.
- Identify likely feature-boundary violations, including backend behavior placed in generic layers or Nuxt feature logic scattered unnecessarily across global folders.
- Do not classify intentional domain separation as a defect merely because code looks similar.

## Method

1. Inventory existing helpers, composables, services, and representative mature feature slices.
2. Compare similar features and flows for repeated logic and diverging behavior.
3. Verify each finding against actual implementations and line references.
4. Recommend the smallest useful refactor, including when duplication should remain to preserve domain boundaries.
5. Do not modify files unless the user explicitly asks for implementation.

## Report format

```markdown
## Code Quality and Consistency Report

### Summary
- Files analyzed: [count]
- Issues found: [count by severity]
- Utility reuse opportunities: [count]
- Consistency violations: [count]

### Findings
#### [Severity] [file:line range] — [issue]
- **Type:** duplicate code | missed utility | inconsistency | boundary issue
- **Evidence:** [related locations and concrete behavior]
- **Existing solution:** [utility/pattern, if one exists]
- **Suggestion:** [specific, minimal refactor]
- **Impact:** [why this improves the codebase]

### Consistency notes
- Backend feature-slice adherence: [assessment]
- Nuxt feature/routing conventions: [assessment]
- Error handling and shared utility reuse: [assessment]
```

Report only substantiated findings. Avoid line references unless verified against the current files.
