---
Title: Spec Kit
ActivityId: 12
---

### Summary

In this exercise you will use **Spec Kit** (GitHub's spec-driven development toolkit) to go from a natural-language idea to a fully implemented feature using structured specifications. Spec Kit bridges the gap between what you want to build and what AI agents implement. Expect to spend thirty to forty minutes.

### What you will learn

- Installing the `specify-cli` tool using `uv`.
- Initializing a Spec Kit project with `specify init`.
- Using slash commands to define a constitution, write specifications, generate plans, create tasks, and implement code.
- Following the spec-driven development workflow from idea to implementation.

### Before you start

Ensure you have **Python 3.11 or later** and **uv** installed. You also need **Git** and a code editor with GitHub Copilot (VS Code recommended). Have a repository ready where you want to build a new feature.

### Steps

- **Step 1.** Install the Spec Kit CLI by running: `uv tool install specify-cli --from git+https://github.com/github/spec-kit.git`

- **Step 2.** Navigate to your repository and initialize Spec Kit: `specify init my-feature --ai copilot`. This creates a `.specs/` directory with the initial structure.

- **Step 3.** Open Copilot Chat and run `/speckit.constitution` to define the project's guiding principles and constraints. Review the generated constitution file in `.specs/`.

- **Step 4.** Run `/speckit.specify` and describe the feature you want to build in natural language. Spec Kit converts your description into a structured specification with acceptance criteria.

- **Step 5.** Run `/speckit.plan` to generate an implementation plan from the specification. Review the plan which breaks the work into ordered steps.

- **Step 6.** Run `/speckit.tasks` to convert the plan into individual, actionable tasks that the agent can execute.

- **Step 7.** Run `/speckit.implement` to have the agent execute each task and write the code. Review the generated code against the original specification to confirm it meets acceptance criteria.

### Checkpoint

1. Did the specify init command create a .specs directory with the initial structure?

- [ ] Yes
- [ ] No

2. Did the /speckit.specify command produce a structured specification with acceptance criteria?

- [ ] Yes
- [ ] No

3. Did /speckit.implement generate code that matched the planned specification?

- [ ] Yes
- [ ] No

### Explore more

- [Spec Kit on GitHub](https://github.com/github/spec-kit)

- [Spec-driven development overview](https://github.com/github/spec-kit/blob/main/docs/spec-driven.md)

