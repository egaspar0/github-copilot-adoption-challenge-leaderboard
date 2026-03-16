---
Title: PRD - Stories - Code (voice-first)
ActivityId: 12
---

### Summary

In this end-to-end workflow you will start with a lightweight Product Requirements Document named `product-x.prd.md`. Using **GitHub Copilot Chat in Agent mode with voice input**you will:

- Extract epics user stories and acceptance criteria.
- Draft a Gherkin specification.
- Scaffold initial code stubs and tests on a new branch.
- Review the output, commit, and open a pull request.
- Plan about thirty minutes.

### What you will learn

- Writing a concise PRD in markdown.

- Using Copilot Chat voice input to issue multi step prompts.

- Converting product language into epics user stories and Gherkin specs.

- Generating starter code and tests directly from requirements.

### Before you start

Install the [VS Code Speech](https://marketplace.visualstudio.com/items?itemName=ms-vscode.vscode-speech) extension and enable microphone permissions for your browser or VS Code. Confirm you have write access to the repository and an available test runner (Jest PyTest JUnit or similar). Create `docs/product-x.prd.md` with a brief description of the new feature.

### Steps

- **Step 1.** Create and check out a branch called `feature-product-x` using `git checkout -b feature-product-x`.

- **Step 2.** Open Copilot Chat and click the microphone icon (or press the assigned shortcut) then dictate: `Extract epics user stories and acceptance criteria from docs/product-x.prd.md`. Wait for Copilot to return structured lists.

- **Step 3.** With voice still active ask: `Draft a Gherkin specification for these stories`. Accept the generated `product-x.feature` file into `tests/features/`.

- **Step 4.** Speak: `Scaffold initial code stubs and unit tests in src/productX/`. Copilot adds empty classes functions and matching test files.

- **Step 5.** Run your test suite with `npm test` `pytest` or equivalent. All tests should fail gracefully indicating todo stubs that await implementation.

- **Step 6.** Stage all new files with `git add docs/product-x.prd.md src/ tests/` then commit. Let Copilot draft the commit message.

- **Step 7.** Push the branch and open a pull request titled `feat: scaffold Product X epics stories Gherkin and stubs`.

### Checkpoint

1. Did Copilot generate epics user stories and acceptance criteria from the PRD?

- [ ] Yes
- [ ] No

2. Did the Gherkin spec file save under `tests/features`?

- [ ] Yes
- [ ] No

3. Did you commit and push the scaffold branch and open a pull request?

- [ ] Yes
- [ ] No

### Explore more

- [Copilot Agents overview](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode)

- [VS Code Speech extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.vscode-speech)

- [Personalizing Copilot Voice](https://code.visualstudio.com/docs/configure/accessibility/voice)

- [Gherkin syntax guide](https://cucumber.io/docs/gherkin/)
