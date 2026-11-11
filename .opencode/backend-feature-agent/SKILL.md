---
name: backend-feature-agent
description: Implement or refactor FastEndpoints feature slices in the .NET backend, including routes, validation, EF access, and domain behavior.
---

# Backend Feature Agent

Implement backend API behavior within the existing Vertical Slice Architecture. Apply `repository-implementation-conventions` for shared architecture, mapping, validation, data-integrity, and quality rules.

## Feature boundaries

- Feature domains are under `backend/Features/{Admin,AssetManagement,Common,HRIS,My,User}`.
- Place each operation with its endpoint, request/response models, and validator under the owning feature/action directory. Match adjacent FastEndpoints patterns.
- A new action directory holds `Endpoint.cs` (one endpoint class, named `Endpoint`), `Validator.cs` (class `Validator`, when the request has field-level rules), and `Models.cs` (when the action has request/response DTOs). Shared models of a feature live in its parent `Models.cs`, not in an action file; never declare DTOs inside `Endpoint.cs`.
- Use `backend/Database`, `backend/Entities`, `backend/Policies`, and `backend/Enums` for the shared persistence and domain definitions they own. Add a shared service only when logic is genuinely reused or needs a clear cross-slice boundary.

## Task-specific responsibilities

- Preserve route, verb, permission, and response behavior unless the request calls for a contract change.
- Enforce authorization and user-context rules on the server.
- Keep handlers cohesive and domain behavior explicit; avoid generic CRUD or service layers that obscure feature behavior.
- If schema changes are needed, coordinate entity/configuration and migration implications with the data-integrity conventions.

## Delivery notes

- Identify feature paths changed and any route/request/response contract changes.
- List the slices touched and confirm file-name compliance: `Endpoint.cs` / `Validator.cs` / `Models.cs` only, no endpoint file at domain level, no endpoint with more than one endpoint class.
- Call out authorization implications and required migration or seed steps.
- Suggest focused endpoint smoke checks for the modified behavior.
