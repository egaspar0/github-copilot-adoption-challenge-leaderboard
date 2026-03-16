---
Title: Ghost Text 101
ActivityId: 12
---

### Summary

In this micro-exercise you will practise Copilot’s “ghost text” inline completion on real code in your own repository. You will (a) finish a half-implemented function in your primary language, then (b) repeat the experiment in a different language file that already lives in the same repo, all in 15-20 minutes.

### What you will learn

- Triggering and accepting Copilot’s inline suggestions (“ghost text”).

- Prompting better suggestions with meaningful names and comments.

- Cycling through alternative completions with the keyboard.

- Comparing Copilot behaviour across two different programming languages.

### Prerequisites

Make sure GitHub Copilot is enabled in your IDE of choice (VS Code, JetBrains, Visual Studio, Neovim…). Open a repository you worked on recently, no starter project needed.

### Step-by-step tutorial

- 1. Pick a partially written function
Locate a function or method you began last week but never finished. Tip: search for “TODO” comments or empty method bodies. If you don’t have one, create a new function and leave it partly stubbed to simulate a work-in-progress.

- 2. Set the stage for Copilot
Add a one-line comment describing the intent (e.g., `// Return the sum of all positive numbers`). Good context helps Copilot suggest higher-quality code.

- 3. Start typing... accept the ghost text
Begin the first line of the implementation and pause. When the grey suggestion appears:

- Press Tab (? ] / Alt ] to cycle variants) to accept.

- Optionally reject (Esc) and keep typing to refine the prompt.

- 4. Review & adjust
Read the generated code. If it compiles and meets your style, keep it; otherwise edit freely.

- 5. Try again in another language file
Open a file in a different language that already exists in the repo (e.g., Python if you started in JS). Repeat steps 2-4 and observe how suggestions adapt.

- 6. Commit your work
Run tests or build as usual, then commit with a Copilot-generated commit message (optional).

### Mini-quiz

1. Did Copilot suggest a complete, runnable function body?

- [ ] Yes
- [ ] No

2. Did you use the keyboard shortcut to cycle through alternative suggestions?

- [ ] Yes
- [ ] No

3. Did you repeat the exercise in a second language file and notice any differences?

- [ ] Yes
- [ ] No

### Explore more

- [Copilot Quickstart docs](https://docs.github.com/en/copilot/getting-started-with-github-copilot)

- [VS Code inline suggestion keys](https://code.visualstudio.com/docs/editor/intellisense#_inline-suggestions)

- [Copilot CLI (ghost text on the command line)](https://githubnext.com/projects/copilot-cli)
