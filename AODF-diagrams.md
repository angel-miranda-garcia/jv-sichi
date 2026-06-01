# AODF — Diagrams

Mermaid diagrams for the **AI-Orchestrated Delivery Framework**. Render natively in GitHub/GitLab inside ` ```mermaid ` blocks.

---

## Diagram 1 — Overview: layers, nodes and flow

Waterfall flow with three mandatory gates, testing correction loop, Node 0 seed context generation, and universal return to Node 0.

```mermaid
flowchart TB
    classDef strategic fill:#1e3a5f,stroke:#0d1b2a,color:#fff,stroke-width:2px
    classDef engineering fill:#2d5016,stroke:#1a2e0a,color:#fff,stroke-width:2px
    classDef delivery fill:#5c2d0a,stroke:#331a05,color:#fff,stroke-width:2px
    classDef authority fill:#4a148c,stroke:#2a0a4f,color:#fff,stroke-width:3px
    classDef gate fill:#f4c430,stroke:#8a6d00,color:#1a1a1a,stroke-width:1px

    subgraph STRAT[" STRATEGIC LAYER "]
        direction TB
        N1["<b>Node 1 — Discovery</b><br/>Understand what it is<br/><i>MCP: reads minutes, requirements</i>"]
        N2["<b>Node 2 — Planning</b><br/>Plan how to do it<br/><i>User stories, backlog, Sprint 1 closed</i>"]
    end

    subgraph ENG[" ENGINEERING LAYER "]
        direction TB
        N3["<b>Node 3 — Design</b><br/>Design the solution<br/><i>MCP: scans repos · N3.1 HTML prototype</i>"]
        N4["<b>Node 4 — Execute</b><br/>Build it · partition defined in N3<br/><i>Powers: Kiro / Cursor execute</i>"]
    end

    subgraph DEL[" DELIVERY LAYER "]
        direction TB
        N5["<b>Node 5 — Testing</b><br/>Prove it works<br/><i>Powers: pytest, BDD, hardening</i>"]
        N6["<b>Node 6 — Deploy</b><br/>Deliver it<br/><i>Powers: CI/CD, observability</i>"]
    end

    N0["<b>NODE 0 — Project Context Authority</b><br/>Single source of truth · Memory AI<br/><i>Consolidate · Maintain · Generate seed contexts</i><br/>Only accepts: APPROVED / COMMITTED"]

    G1{{"Gate 1<br/>Discovery Approved"}}
    G2{{"Gate 2<br/>Planning Approved"}}
    G3{{"Gate 3<br/>Design Approved"}}

    N1 --> G1 --> N2
    N2 --> G2 --> N3
    N3 --> G3 --> N4
    N4 --> N5
    N5 -->|" failure "| N4
    N5 --> N6

    N1 -.->|APPROVED| N0
    N2 -.->|APPROVED| N0
    N3 -.->|APPROVED| N0
    N4 -.->|COMMITTED| N0
    N5 -.->|COMMITTED| N0
    N6 -.->|COMMITTED| N0

    N0 ==>|"seed context (MCP)"| STRAT
    N0 ==>|"seed context (MCP)"| ENG
    N0 ==>|"seed context (MCP)"| DEL

    class N1,N2 strategic
    class N3,N4 engineering
    class N5,N6 delivery
    class N0 authority
    class G1,G2,G3 gate
```

---

## Diagram 2 — Subnodes per node

Detailed view of specialized subnodes. N4.x partition is defined in Node 3 per project — examples shown are illustrative.

```mermaid
flowchart LR
    classDef parent fill:#2c3e50,stroke:#1a252f,color:#fff,stroke-width:2px
    classDef fixed fill:#ecf0f1,stroke:#7f8c8d,color:#2c3e50,stroke-width:1px
    classDef optional fill:#fdebd0,stroke:#d35400,color:#2c3e50,stroke-width:1px,stroke-dasharray:4 2
    classDef perproject fill:#d5f5e3,stroke:#1e8449,color:#2c3e50,stroke-width:1px

    N2["<b>Node 2</b><br/>Planning"]
    N2X["2.x — Change Review<br/><i>bug/enhancement? sprint impact?<br/>architecture impact? ADR? redesign?</i>"]
    N2 --> N2X

    N3["<b>Node 3</b><br/>Design"]
    N31["3.1 — Design UI ✓ FIXED<br/><i>navigable HTML prototype<br/>user sign-off required</i>"]
    N32["3.2 — ADR ◌ OPTIONAL<br/><i>formal technical decision</i>"]
    N33["3.3 — Spike ◌ OPTIONAL<br/><i>proof of concept</i>"]
    N3 --> N31
    N3 --> N32
    N3 --> N33

    N4["<b>Node 4</b><br/>Execute<br/><i>partition defined in N3</i>"]
    N41["4.1 — Execute {part 1}<br/><i>domain / functional area</i>"]
    N42["4.2 — Execute {part 2}<br/><i>domain / functional area</i>"]
    N4N["4.n — Execute {part n}<br/><i>as many as needed</i>"]
    N4 --> N41
    N4 --> N42
    N4 --> N4N

    class N2,N3,N4 parent
    class N2X,N31 fixed
    class N32,N33 optional
    class N41,N42,N4N perproject
```

---

## Diagram 3 — State governance

Decision lifecycle and which states are allowed into Node 0.

```mermaid
flowchart LR
    classDef open fill:#bdc3c7,stroke:#7f8c8d,color:#2c3e50,stroke-width:1px
    classDef closed fill:#27ae60,stroke:#145a32,color:#fff,stroke-width:2px
    classDef authority fill:#4a148c,stroke:#2a0a4f,color:#fff,stroke-width:2px
    classDef blocked fill:#e74c3c,stroke:#7b241c,color:#fff,stroke-width:1px

    P["PROPOSED<br/><i>raised, under debate</i>"]
    R["REVIEWED<br/><i>refined, pending approval</i>"]
    A["APPROVED<br/><i>operative truth</i>"]
    C["COMMITTED<br/><i>materialized</i>"]

    P --> R --> A --> C

    N0["<b>NODE 0</b><br/>Project Context Authority"]
    A -.->|" ✓ enters "| N0
    C -.->|" ✓ enters "| N0
    P -.->|" ✗ blocked "| X1["⛔"]
    R -.->|" ✗ blocked "| X2["⛔"]

    class P,R open
    class A,C closed
    class N0 authority
    class X1,X2 blocked
```

---

## Diagram 4 — Agentic capabilities: MCP vs Powers

Which protocol each node activates. MCP = reading ("the plug"); Powers = execution ("the hands").

```mermaid
flowchart TB
    classDef mcp fill:#2980b9,stroke:#1a5276,color:#fff,stroke-width:2px
    classDef powers fill:#c0392b,stroke:#7b241c,color:#fff,stroke-width:2px

    MCP["<b>MCP — Reading</b><br/>'the plug'<br/><i>connects to repos, DBs, docs live</i>"]
    POW["<b>Powers / Tool Use — Execution</b><br/>'the hands'<br/><i>writes code, creates branches, triggers pipelines</i>"]

    MCP --- N0M["Node 0 — serves context (acts as MCP server)"]
    MCP --- N1M["Node 1 — reads minutes / requirements"]
    MCP --- N3M["Node 3 — scans repos and schemas"]

    POW --- N4P["Node 4 — Kiro / Cursor code"]
    POW --- N5P["Node 5 — pytest, BDD, Great Expectations"]
    POW --- N6P["Node 6 — Azure Pipelines, CI/CD"]

    class MCP,N0M,N1M,N3M mcp
    class POW,N4P,N5P,N6P powers
```

---

## Diagram 5 — Node 0: seed context generation

Node 0 active role — consolidates state and generates tailored seed contexts per target.

```mermaid
flowchart LR
    classDef authority fill:#4a148c,stroke:#2a0a4f,color:#fff,stroke-width:3px
    classDef input fill:#1e3a5f,stroke:#0d1b2a,color:#fff,stroke-width:1px
    classDef output fill:#2d5016,stroke:#1a2e0a,color:#fff,stroke-width:1px
    classDef external fill:#5c2d0a,stroke:#331a05,color:#fff,stroke-width:1px

    N0["<b>NODE 0</b><br/>Project Context Authority<br/><i>consolidate · maintain · generate</i>"]

    N1I["Node 1 APPROVED"] --> N0
    N2I["Node 2 APPROVED"] --> N0
    N3I["Node 3 APPROVED"] --> N0
    N4I["Node 4 COMMITTED"] --> N0
    N5I["Node 5 COMMITTED"] --> N0
    N6I["Node 6 COMMITTED"] --> N0

    N0 --> C41["context_4.1.md<br/><i>UI: HUs + design tokens<br/>+ Figma brief</i>"]
    N0 --> C42["context_4.2.md<br/><i>Infra: pipeline strategy<br/>+ env config</i>"]
    N0 --> C43["context_4.3.md<br/><i>Core: business rules<br/>+ domain contracts</i>"]
    N0 --> C44["context_4.4.md<br/><i>Integrations: API specs<br/>+ queue definitions</i>"]
    N0 --> C5["context_5.md<br/><i>QA: acceptance criteria<br/>+ edge cases</i>"]
    N0 --> CEX["External tools<br/><i>Figma, Uizard, v0.dev<br/>UX flows + palette</i>"]
    N0 --> CREP["Leadership report<br/><i>delivery status<br/>+ open blockers</i>"]

    class N0 authority
    class N1I,N2I,N3I,N4I,N5I,N6I input
    class C41,C42,C43,C44,C5 output
    class CEX,CREP external
```

---

## Diagram 6 — Qdrant semantic memory layer

How Qdrant powers Node 0's semantic context generation.

```mermaid
flowchart TB
    classDef authority fill:#4a148c,stroke:#2a0a4f,color:#fff,stroke-width:3px
    classDef qdrant fill:#16a085,stroke:#0b5345,color:#fff,stroke-width:2px
    classDef embed fill:#d35400,stroke:#7e5109,color:#fff,stroke-width:1px
    classDef output fill:#2d5016,stroke:#1a2e0a,color:#fff,stroke-width:1px

    IN["APPROVED / COMMITTED decisions<br/><i>ADRs, business rules, HUs, standards</i>"]
    EMB["Embedding model<br/><i>OpenAI / Cohere / local</i>"]

    IN --> EMB --> QD

    subgraph QD["Qdrant Collections"]
        direction LR
        Q1["decisions"]
        Q2["business_rules"]
        Q3["adr"]
        Q4["user_stories"]
        Q5["standards"]
    end

    QD --> N0

    N0["<b>NODE 0</b><br/>semantic query by target node"]

    N0 -->|"query: UI visual components"| C41["context_4.1.md"]
    N0 -->|"query: domain rules contracts"| C43["context_4.3.md"]
    N0 -->|"query: acceptance criteria HU"| C5["context_5.md"]

    class N0 authority
    class QD,Q1,Q2,Q3,Q4,Q5 qdrant
    class EMB embed
    class C41,C43,C5 output
```

---

## Diagram 8 — Node Lifecycle

A node is a complete conversation. This diagram shows how sessions chain together using seed contexts and snapshots, and what exits to Node 0.

```mermaid
flowchart TB
    classDef authority fill:#4a148c,stroke:#2a0a4f,color:#fff,stroke-width:3px
    classDef seed fill:#2980b9,stroke:#1a5276,color:#fff,stroke-width:2px
    classDef session fill:#2d5016,stroke:#1a2e0a,color:#fff,stroke-width:2px
    classDef snapshot fill:#d35400,stroke:#7e5109,color:#fff,stroke-width:1px
    classDef committed fill:#27ae60,stroke:#145a32,color:#fff,stroke-width:2px
    classDef discard fill:#7f8c8d,stroke:#566573,color:#fff,stroke-width:1px

    N0["<b>NODE 0</b><br/>generates seed context"]

    N0 --> SC["context_4.1.md<br/><i>seed context</i>"]

    SC --> S1["<b>Session 1</b><br/>Node 4.1 — active conversation"]
    S1 -->|"pauses"| SN1["snapshot_4.1_s1.md<br/><i>what was built · pending · blockers</i>"]
    S1 -->|"conversation"| D1["raw chat — discardable ✗"]

    SN1 --> S2["<b>Session 2</b><br/>Node 4.1 — continues<br/><i>loads: context_4.1.md + snapshot_4.1_s1.md</i>"]
    S2 -->|"pauses"| SN2["snapshot_4.1_s2.md"]
    S2 -->|"conversation"| D2["raw chat — discardable ✗"]

    SN2 --> S3["<b>Session N</b><br/>Node 4.1 — final session"]
    S3 -->|"closes"| CO["<b>Committed output</b><br/>code · ADR · decision<br/>state: COMMITTED"]
    S3 -->|"conversation"| D3["raw chat — discardable ✗"]

    CO -->|"returns to"| N0B["<b>NODE 0</b><br/>receives COMMITTED result"]

    class N0,N0B authority
    class SC seed
    class S1,S2,S3 session
    class SN1,SN2 snapshot
    class CO committed
    class D1,D2,D3 discard
```

Four capabilities that run across all nodes.

```mermaid
flowchart TB
    classDef cross fill:#16a085,stroke:#0b5345,color:#fff,stroke-width:2px
    classDef doc fill:#ecf0f1,stroke:#7f8c8d,color:#2c3e50,stroke-width:1px

    CE["<b>A. Context Engineering</b><br/>continuity, snapshots, operative memory"]
    WCD["WCD — temporary<br/><i>not committed</i>"]
    PSD["PSD — permanent<br/><i>versioned: docs/adr, docs/architecture</i>"]
    CE --> WCD
    CE --> PSD

    MCP2["<b>B. MCP &amp; AI Integrations</b><br/>Figma, Postman, GitHub, ADO, Filesystem"]
    TOOL["<b>C. AI Toolchain Orchestration</b><br/>IDE agents, Kiro, scripts, scaffolding, CLI"]
    KB["<b>D. Knowledge Base</b><br/>prompts, ADRs, patterns, steering files, standards<br/><i>avoid reinventing decisions</i>"]

    class CE,MCP2,TOOL,KB cross
    class WCD,PSD doc
```

---

## Diagram 9 — Cross-cutting capabilities

Four capabilities that run across all nodes.

```mermaid
flowchart TB
    classDef cross fill:#16a085,stroke:#0b5345,color:#fff,stroke-width:2px
    classDef doc fill:#ecf0f1,stroke:#7f8c8d,color:#2c3e50,stroke-width:1px

    CE["<b>A. Context Engineering</b><br/>seed contexts · snapshots · operative memory"]
    SC["Seed Context<br/><i>generated by Node 0, consumed on start</i>"]
    SN["Node Snapshot<br/><i>session state, not committed</i>"]
    PSD["PSD — permanent<br/><i>versioned: docs/adr, docs/architecture</i>"]
    CE --> SC
    CE --> SN
    CE --> PSD

    MCP2["<b>B. MCP &amp; AI Integrations</b><br/>Figma, Postman, GitHub, ADO, Filesystem"]
    TOOL["<b>C. AI Toolchain Orchestration</b><br/>IDE agents, Kiro, scripts, scaffolding, CLI"]
    KB["<b>D. Knowledge Base</b><br/>prompts, ADRs, patterns, steering files, standards<br/><i>avoid reinventing decisions</i>"]

    class CE,MCP2,TOOL,KB cross
    class SC,SN,PSD doc
```
