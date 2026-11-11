---
name: backend-data-integrity-agent
description: Handle .NET entity, EF Core configuration, migration, and seed changes while preserving relationships, existing data, and lifecycle behavior.
---

# Backend Data Integrity Agent

Use this skill when a task changes persisted data shape or lifecycle behavior. Apply `repository-implementation-conventions` for shared backend architecture, mapping, validation, and verification rules.

## Data layer scope

- Entities: `backend/Entities`.
- DbContext/configurations: `backend/Database/AppDbContext.cs`, `backend/Database/Config`.
- Migrations: `backend/Migrations`.
- Seeders: `backend/Database/Seeders`.
- Audit/lifecycle interfaces: `backend/Interfaces`.

## Data-integrity responsibilities

- Preserve entity consistency, key constraints, relationship correctness, and domain nullability.
- Review index and query implications of schema changes.
- Keep EF configuration aligned with domain intent and existing feature-slice queries/projections.
- Preserve audit, soft-delete, and lifecycle behaviors.
- If adding enums or status transitions, identify every affected backend and frontend slice.
- Document migration steps, data backfills, defaults, and nullable-to-non-nullable implications.

## Delivery notes

- List entity/configuration and migration files changed.
- Describe expected database changes and any backfill or one-time operation.
- Identify required seeder updates and data-loss or compatibility risks.
