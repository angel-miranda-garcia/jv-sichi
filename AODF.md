# AI-Orchestrated Delivery Framework (AODF)

## AI-Assisted Development Model Based on Layers and Nodes

**Version 1.3**

---

## 1. Purpose and Principles

The AODF organizes the software development lifecycle assisted by AI through a network of **specialized nodes** (dedicated chat instances with a scoped purpose), grouped into **responsibility layers**.

The core operating premise: **strategy is debated in the strategic layer, systems are designed in the engineering layer, and agents execute in the delivery layer.** The underlying goal is to maximize precision, minimize context contamination between conversations, and enable operational scalability through specialization.

Each node has an explicit purpose, a delimited scope, defined inputs, expected outputs, a declared agentic integration, and a mandatory exit criterion (Gate).

**Final governing principle: no specialized node is the source of truth. The only official source of truth for the project is Node 0 — Project Context Authority.**

---

## 2. General Architecture

The framework operates on two levels of abstraction:

- **Layers** (organizational level): group nodes by nature of work.
- **Nodes** (execution level): specialized work units, instantiated as dedicated AI conversations.

| Layer | Nodes | Nature |
|-------|-------|--------|
| **Strategic Layer** | Node 0, Node 1, Node 2 | Defines the *what* and *why*; maintains global project state |
| **Engineering Layer** | Node 3, Node 4 | Defines and executes the *how* |
| **Delivery Layer** | Node 5, Node 6 | Ensures quality and materializes value in production environments |

---

## 3. Agentic Capabilities

Two technical protocols run across all nodes and give them autonomy. The distinction is fundamental: one **reads**, the other **executes**.

### MCP (Model Context Protocol) — Reading capability

The "plug" for reading. Allows the AI to connect live to repositories, databases, schemas, or documents (Confluence, SharePoint, OneDrive, ADO) without manually copying and pasting context.

Examples: Filesystem MCP, GitHub MCP, Azure DevOps MCP, Figma MCP, Postman MCP.

### Powers (Tool Use) — Execution capability

The "hands" of the system. Allows agents (Kiro IDE, Cursor) to interact autonomously with external tools: write code, create branches, run tests, invoke APIs, trigger pipelines.

Each node explicitly declares which protocol it activates and for what purpose.

---

## 4. State Governance (framework core)

All information flowing between nodes has a lifecycle state. The decision lifecycle is:

```
PROPOSED → REVIEWED → APPROVED → COMMITTED
```

| State | Meaning |
|-------|---------|
| `PROPOSED` | Idea or decision raised, still under debate |
| `REVIEWED` | Discussed and refined, pending approval |
| `APPROVED` | Approved by the responsible party; enters as operative truth |
| `COMMITTED` | Materialized (code, infrastructure, versioned artifact) |

**Golden rule of states:** Only information in `APPROVED` or `COMMITTED` state may enter Node 0. Nothing in `PROPOSED` or `REVIEWED` contaminates the source of truth.

---

## 5. Strategic Layer

### Node 0 — Project Context Authority (PCA)

**Alias:** Master Context Hub / Memory AI

**Purpose.** To be the single source of operative truth and the persistent memory of the project — and to **actively generate tailored seed contexts** that enable other nodes and external tools to start working immediately, with exactly the information they need and nothing more.

Node 0 has three core functions, in order:

```
1. CONSOLIDATE  →  receive APPROVED/COMMITTED decisions from all nodes
2. MAINTAIN     →  keep project state clean, versioned, and unambiguous
3. GENERATE     →  produce tailored seed contexts per target node or tool
```

Node 0 does **not** design, debate, program, experiment, interpret, or infer. It consolidates and serves.

**Agentic integration.** Acts as an **MCP server**: all other nodes connect to it to read the current project state. Optionally backed by **Qdrant** (vector database) for semantic search across accumulated decisions — see Section 9.

**Responsibilities.**
- Consolidate approved decisions from Nodes 1–6
- Update project roadmap and delivery status
- Maintain current business rules and engineering standards
- Consolidate approved ADRs
- **Generate seed context documents per target node or tool**

**Inputs.** Exclusively `APPROVED` or `COMMITTED` outputs from Nodes 1–6 and business-approved changes.

**Seed context generation — target map.**

Node 0 generates a tailored `n0-out-context-{target}.md` for each consumer. The package contains only what that target needs. N4.x seed contexts are generated based on the partition defined in Node 3 — there is no fixed list:

| Target | Seed context includes |
|--------|----------------------|
| `n0-out-context-1.md` | Prior decisions, known constraints, macro state |
| `n0-out-context-3.md` | Confirmed stack, existing ADRs, architectural constraints |
| `n0-out-context-3-1.md` | UI-related HUs, visual references, screen inventory, design guidelines |
| `n0-out-context-4-x.md` | Defined by Node 3 partition: business rules, contracts, interfaces, steering files relevant to that construction unit |
| `n0-out-context-5.md` | Acceptance criteria per HU, Node 3 contracts, edge cases |
| `n0-out-context-6.md` | Deployment strategy, environment variables, rollback plan |
| External (Figma, Uizard, v0.dev) | UX flows, palette, typography, key components, screen inventory |
| Leadership report | Delivery status, sprint progress, open blockers |
| New collaborator onboarding | What is done, in progress, conventions to follow, which node to connect to |

**Deliverables.**

- `context_project_master.md`
- `context_business_rules.md`
- `context_architecture.md`
- `context_delivery_status.md` — includes explicit tracking of consumed and pending steering files
- `context_{target}.md` — generated on demand per target node or tool

---

### Node 1 — Business Discovery

**Alias:** Discovery Node

**Purpose.** Deeply understand the problem and align the business **before** designing. This is the node for exploration, discussion, argumentation, and alignment. Extensive debate is expected and allowed here.

**Agentic integration.** Uses **MCP** to read transcripts, meeting minutes, workshops, raw requirements (e.g. planning spreadsheets), and initial documents directly.

**Activities.** Problem understanding, functional analysis, objective refinement, stakeholder identification, risk detection, macro planning, preliminary estimation, preliminary technology stack definition.

**Deliverable.** `context_discovery.md` (macro plan, preliminary scope, risks, objectives, preliminary stack, functional gaps).

**Gate 1 — Discovery Approved.** To advance to Node 2: preliminary scope, clear objectives, identified stakeholders, preliminary stack, and identified risks must all exist.

---

### Node 2 — Delivery Planning & Sprint Design

**Alias:** Planning Node

**Purpose.** Translate business understanding into an executable plan and define delivery governance.

**Inputs.** `context_discovery.md`, refined requirements, new business definitions.

**Activities.** Creation of user stories, epics and features; detailed roadmap; sprint planning; complete definition of Sprint 1; functional refinement; gap identification.

**Delivery governance.** Defines ADO structure (Epics, Features, Tasks), backlog hierarchy, naming conventions, deliverable definitions, and initial Definition of Done.

**Rule.** Sprint 1 must be **fully closed**; subsequent sprints are adjustable.

**Deliverables.** `context_planning.md`, `delivery_plan.md`, `ado_structure.md`.

#### Subnode 2.x — Change Review

Manages business changes, refinements, new rules, and scope re-evaluation. Mandatory questions for any change:

1. Is this a bug or an enhancement?
2. Does it impact the sprint?
3. Does it impact the architecture?
4. Does it require an ADR?
5. Does it require a redesign?

**Gate 2 — Planning Approved.** Must exist: Sprint 1 closed, initial backlog, refined user stories, and identified external dependencies.

---

## 6. Engineering Layer

### Node 3 — Design

**Alias:** Design Node

**Purpose.** Define the **how** of the system and design the solution before any construction begins. No code is written here — only decisions, architecture, and validated designs.

**Agentic integration.** Uses **MCP** to scan existing repositories, read database schemas, and consume existing API contracts.

**Activities.** Technical architecture, ADRs, folder structure, modular design, contracts and interfaces, conventions, **steering files** (agent contracts), engineering standards, CI/CD strategy, toolchain decisions.

**CI/CD Strategy.** Branch strategy (trunk/gitflow), environments, pipeline architecture, deployment strategy, repositories, base observability.

**Toolchain Architecture.** Decides which MCPs, tools, integrations, and automations are adopted (Figma, Postman, GitHub, Azure DevOps, Filesystem, Browser MCP).

**N4.x partition decision.** Node 3 defines how construction work will be split across N4 subnodes — by domain, by functional area, or by any other criteria that fits the project. This partition is documented in the steering files and consumed by Node 0 to generate the corresponding seed contexts.

```
Node 3 defines: "construction will be split into N parts"
  → documented in steering files → APPROVED → Node 0
Node 0 generates:
  n0-out-context-4-1.md
  n0-out-context-4-2.md
  ...
  n0-out-context-4-n.md
```

**Subnodes.**

- **3.1 — Design UI** `[FIXED]` — generates a navigable HTML prototype with real user sign-off before any code is written. Output is the approved prototype that N4 implements from.
- **3.2 — ADR** `[OPTIONAL]` — formal evaluation and documentation of a technical decision when alternatives need to be compared explicitly (e.g. REST vs GraphQL, database choice).
- **3.3 — Spike** `[OPTIONAL]` — proof of concept to validate technical feasibility before committing to a construction approach.

Subnodes 3.2 and 3.3 are opened only when the topic is large enough to justify an isolated conversation. Otherwise the activity happens inside the main N3 chat.

**Deliverables (committed to repo).**

- `docs/architecture.md`
- `docs/engineering_standards.md`
- `docs/adr/`
- `steering/` (or `.kiro/steering/`)
- `prototype/` — navigable HTML from N3.1 (if applicable)

**Gate 3 — Design Approved.** Must exist: validated architecture, steering files, conventions, minimum ADRs, pipeline strategy, N4 partition defined, and UI prototype approved (if applicable).

---

### Node 4 — Execute

**Alias:** Construction Node

**Purpose.** Build what was designed. No architecture debate or redefinition here — only implementation under contracts. Node 4 is technology and domain agnostic: it applies equally to data pipelines, APIs, UIs, migrations, or any other type of construction.

**Core rule.** If architectural ambiguity arises → **return to Node 3**. Node 4 never redefines architecture.

**Agentic integration.** Intensive activation of **Powers / Tool Use** (Kiro IDE, Cursor). Agents write code, manage git, and execute steering files.

**Form of construction.** By modules, bounded contexts, functional areas, or any partition defined in Node 3. **Never by isolated files.**

**Subnodes — how to partition.**

Node 3 decides how to split construction work. The split can be by domain, by functional area, by technical layer, or by any criteria that fits the project. There is no fixed list of N4 subnodes — they are defined per project:

```
Example — data engineering project:
  N4.1 - Execute scoring-engine
  N4.2 - Execute data-loaders
  N4.3 - Execute control-log

Example — product with UI:
  N4.1 - Execute ui
  N4.2 - Execute api
  N4.3 - Execute notifications

Example — infrastructure migration:
  N4.1 - Execute pipeline
  N4.2 - Execute infra
```

Each N4.x subnode:
- Receives its seed context from Node 0 (`n0-out-context-4-x.md`)
- Receives the approved prototype from N3.1 if applicable
- Follows the steering files defined in Node 3
- Produces committed code that returns to Node 0

**Multi-session work.** If a subnode's work is too large for a single chat, it continues via snapshots — see Section 10 (Node Lifecycle).

**Inputs.**
- Seed context generated by Node 0 (`n0-out-context-4-x.md`)
- Steering files from Node 3
- Approved HTML prototype from N3.1 (if applicable)

**Commit convention.** Every commit must follow steering conventions, include rationale, and map to a user story.

**Deliverables.** Functional code, atomic semantic commits, traceable PRs.

---

## 7. Delivery Layer

### Node 5 — Quality Engineering

**Alias:** Quality Node

**Purpose.** Validate technical and functional quality by cross-checking code against Node 3 design contracts.

**Agentic integration.** Uses **Powers** to autonomously execute testing suites (pytest, BDD/Gherkin), validation engines (Great Expectations), linters, and request collections (Postman).

**Inputs.** Node 4 outputs, Node 3 contracts, `context_5.md` from Node 0.

**Activities.** Unit testing, integration testing, BDD/Gherkin, UAT, regression testing, coverage reporting, hardening.

**Deliverables.** `test_strategy.md`, `uat_results.md`, `hardening_report.md`.

**Rule.** On failure → return correction context to the corresponding Node 4.

---

### Node 6 — Release & Operations

**Alias:** Release Node

**Purpose.** Production deployment and lifecycle observability.

**Agentic integration.** Uses **Powers** to interact with pipelines (Azure Pipelines), trigger CI/CD, or connect to on-premise servers under controlled roles.

**Activities.** Environment management, release pipelines, variable configuration, rollout, rollback, monitoring, runbooks, training/handover.

**Deliverables.** `deployment_strategy.md`, `runbook.md`, `release_checklist.md`.

---

## 8. Cross-Cutting Capabilities

### A. Context Engineering

Responsible for contextual continuity, snapshots, context propagation, and operative memory. Distinguishes three document types:

| Type | What it is | Generated by | Committed |
|------|-----------|--------------|-----------|
| **Seed Context** (`context_{node}.md`) | Tailored input to start a node session | Node 0 | No — consumed on start |
| **Node Snapshot** (`snapshot_{node}_s{n}.md`) | Distilled state when pausing or closing a long session | The node itself | No — operational only |
| **Project Source Document (PSD)** | Permanent approved output | Any node | **Yes** — versioned in repo |

**Seed Context** is generated by Node 0 and contains only what the target node needs to begin: no noise, no intermediate decisions, no discarded attempts.

**Node Snapshot** solves the multi-session problem. A long Node 4.1 conversation may span 40+ messages with explorations, failed attempts, and intermediate decisions. The snapshot captures only the current useful state so a new session can continue without inheriting the full conversation history.

Snapshot structure:
```
snapshot_{node}_s{n}.md
  - What was built / decided
  - Current state of outputs
  - Pending work
  - Open blockers
  - Decisions made during this session (pre-APPROVED)
```

**How sessions chain together:**
```
Node 0 generates context_4.1.md
   ↓
Node 4.1 — session 1 (N messages)
   ↓ closes / pauses
generates snapshot_4.1_s1.md
   ↓
Node 4.1 — session 2 starts with:
   context_4.1.md  +  snapshot_4.1_s1.md
   ↓ closes / pauses
generates snapshot_4.1_s2.md
   ↓
... and so on until COMMITTED output exits to Node 0
```

**What a node never receives:** the raw prior conversation. Conversations are discardable. Only their distilled state — the snapshot — carries forward.

### B. MCP & AI Integrations

Connects AI to external tools to automate repetitive tasks and reduce operational friction (Figma, Postman, GitHub, Azure DevOps, Filesystem MCP).

### C. AI Toolchain Orchestration

Industrializes execution: IDE agents, Kiro, scripts, PowerShell, scaffolding, CLI automation, code generation.

### D. Knowledge Base

Reusable repository of prompts, ADRs, patterns, steering files, and standards. Goal: avoid reinventing decisions.

---

## 9. Qdrant Integration (Semantic Memory Layer)

### What it is

Qdrant is a vector database that acts as the **semantic memory engine of Node 0**. Instead of searching context by exact keywords, it searches by *semantic similarity*: "give me everything related to authentication" retrieves decisions, ADRs, user stories, and conversations even if they don't use the exact word.

### When to activate

| Scenario | Recommendation |
|----------|---------------|
| Small project, few ADRs, manageable `.md` files | Not needed — flat files are sufficient |
| Project grows beyond 2 sprints, many ADRs and HUs | **Activate Qdrant** |
| Multiple parallel Node 4.x subnodes running simultaneously | **Activate Qdrant** |
| Node 0 needs to generate contexts across many domains | **Activate Qdrant** |

### How it integrates

```
APPROVED / COMMITTED decisions
        ↓
   embedded (OpenAI / Cohere / local model)
        ↓
   stored in Qdrant with metadata
   { node_origin, state, date, type, project }
        ↓
Node 0 queries Qdrant semantically by target node
        ↓
generates context_{target}.md with only
the most relevant fragments for that node
```

### Architecture

```
Node 0
  ├── Qdrant collection: decisions
  ├── Qdrant collection: business_rules
  ├── Qdrant collection: adr
  ├── Qdrant collection: user_stories
  └── Qdrant collection: standards
```

### Query example

```
Node 0 receives request: "generate context_4.1.md for UI module"
  → semantic query: "UI visual design screens components"
  → Qdrant returns: relevant HUs + design tokens + visual standards + Figma brief
  → Node 0 packages and emits: context_4.1.md
```

### Metadata schema per entry

```json
{
  "id": "uuid",
  "content": "decision or rule text",
  "source_node": "Node 2",
  "state": "APPROVED",
  "type": "user_story | adr | business_rule | standard | contract",
  "project": "project-name",
  "sprint": "sprint-01",
  "date": "2026-05-30"
}
```

---

## 10. Node Lifecycle

A node is not a document — it is a **complete conversation**, from the moment it receives its seed context to the moment it produces its committed output. Understanding the lifecycle prevents context contamination and enables multi-session work.

### The three artifacts of a node

```
Seed Context (input)  →  [Node conversation]  →  Committed output (to Node 0)
                               ↓
                         Node Snapshot (if session pauses)
```

### Node lifecycle states

| State | Meaning |
|-------|---------|
| **Initialized** | Session started, seed context loaded |
| **Active** | Work in progress |
| **Snapshotted** | Session paused, snapshot generated, conversation discardable |
| **Closed** | Output committed, result returned to Node 0 |

### What enters Node 0 vs what stays local

| Artifact | Goes to Node 0 | Stays local |
|----------|---------------|-------------|
| Seed context (`context_{node}.md`) | — | ✓ consumed, then discardable |
| Node snapshot (`snapshot_{node}_s{n}.md`) | — | ✓ operational only |
| Committed output (code, ADR, decision) | ✓ COMMITTED | — |
| The conversation itself | Never | Discardable once snapshotted |

### Rule

Node 0 never receives raw conversations. It only receives distilled, state-tagged outputs. The conversation is the workshop; the snapshot is the workbench state; the committed output is what leaves the workshop.

---

## 11. Naming Conventions

### Claude Project name

```
AODF · {project}
```

Examples: `AODF · scoring-migration`, `AODF · ga-diners`, `AODF · blu2`

The `AODF ·` prefix signals that the project follows this framework.

---

### Chat names (inside a Claude Project)

```
[{TRG}] N{n} - {domain}
```

| Segment | Description | Example |
|---------|-------------|---------|
| `[{TRG}]` | 3-letter project trigram, defined when creating the project | `[SCH]` |
| `N{n}` | Node number | `N0`, `N3`, `N4.1` |
| `{domain}` | Node domain name | `Master`, `Discovery`, `Planning`, `Design`, `Execute`, `Testing`, `Deploy` |

The trigram is necessary because Claude's **Recents** section shows all chats without the project container — without it, multiple `N0 - Master` entries become indistinguishable.

**Full chat name map:**

```
[SCH] N0 - Master
[SCH] N1 - Discovery
[SCH] N2 - Planning
[SCH] N3 - Design
[SCH] N3.1 - Design UI
[SCH] N4.1 - Execute {part-name}
[SCH] N4.2 - Execute {part-name}
[SCH] N5 - Testing
[SCH] N6 - Deploy
```

**Multi-session chats** (when context saturates and work continues in a new chat):

```
[SCH] N4.1 - Execute scoring-engine S2
[SCH] N4.1 - Execute scoring-engine S3
```

No session suffix for single-session nodes. Add `S2`, `S3`... only when a new chat is needed to continue the same node's work.

---

### File names

```
{origin}-{direction}-{content}.md
```

| Segment | Values | Example |
|---------|--------|---------|
| `origin` | `n0`, `n1`, `n2`, `n3`, `n3-1`, `n4-1`, `n4-2` | `n0` |
| `direction` | `out` (output), `snap` (snapshot) | `out` |
| `content` | short descriptive noun in kebab-case | `context-4-1` |

**The origin is always who produces the file, not who receives it.** If Node 0 generates a seed context for Node 2, the file is `n0-out-delivery-plan.md`, not `n2-in-delivery-plan.md`.

**File map by node:**

```
Node 0 — master documents
  n0-out-project-master.md
  n0-out-business-rules.md
  n0-out-architecture.md
  n0-out-delivery-status.md

Node 0 — seed contexts (generated on demand)
  n0-out-context-1.md
  n0-out-context-3.md
  n0-out-context-3-1.md
  n0-out-context-4-1.md
  n0-out-context-4-2.md
  n0-out-context-5.md
  n0-out-context-6.md

Node 1
  n1-out-context-discovery.md

Node 2
  n2-out-delivery-plan-tasks.md
  n2-out-ado-structure.md

Node 3
  n3-out-architecture.md
  n3-out-engineering-standards.md

Node 4.x — snapshots (when session saturates)
  n4-1-snap-s1.md
  n4-1-snap-s2.md
  n4-2-snap-s1.md

Node 5
  n5-out-test-strategy.md
  n5-out-uat-results.md

Node 6
  n6-out-runbook.md
  n6-out-release-checklist.md
```

---

## 13. Official Operating Flow

Agile waterfall model with continuous feedback to the central brain:

```
Node 1 — Discovery
   ↓   (Gate 1: Discovery Approved)
Node 2 — Planning
   ↓   (Gate 2: Planning Approved)
Node 3 — Design
   ↓   N3.1 Design UI → navigable prototype → user sign-off
   ↓   (Gate 3: Design Approved, N4 partition defined)
Node 0 generates n0-out-context-4-x.md per N4 subnode
   ↓
Node 4 — Execute (N4.1, N4.2 ... N4.n)  ←────────────┐
   ↓                                                   │
Node 5 — Testing  ──── (failure) ─────────────────────┘
   ↓
Node 6 — Deploy

Every APPROVED/COMMITTED result from any layer
   ↓
Node 0 — Project Context Authority (operative truth)
```

**Golden rule.** Every closed result in any layer returns immediately to Node 0 to keep the project's operative truth updated.

---

## 14. Summary Reference

| Node | Alias | Fixed | MCP | Powers | Key output |
|------|-------|-------|-----|--------|------------|
| 0 | Master | ✓ | ✓ serves as MCP server | — | `n0-out-context-{target}.md` |
| 1 | Discovery | ✓ | ✓ reads docs | — | `n1-out-context-discovery.md` |
| 2 | Planning | ✓ | — | — | `n2-out-delivery-plan-tasks.md` |
| 3 | Design | ✓ | ✓ scans repos | — | `steering/`, `docs/adr/` |
| 3.1 | Design UI | ✓ | — | — | Navigable HTML prototype |
| 3.2 | ADR | optional | — | — | `docs/adr/{decision}.md` |
| 3.3 | Spike | optional | — | — | Spike report |
| 4.x | Execute | per project | — | ✓ Kiro/Cursor | Code, commits, PRs |
| 5 | Testing | ✓ | — | ✓ pytest/BDD | `n5-out-uat-results.md` |
| 6 | Deploy | ✓ | — | ✓ CI/CD | `n6-out-runbook.md` |

---

## Final Principle

**No specialized node is the source of truth. The only official source of the project is Node 0 — Project Context Authority.**
