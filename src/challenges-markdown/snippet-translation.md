---
Title: Snippet translation
ActivityId: 12
---

### Summary

This exercise demonstrates how to use **GitHub Copilot Chat** to convert code from one language to another. You will copy a snippet from your repository for example Java ask Copilot Chat to translate it into Go idioms then place the result in a scrapbook branch for review. Plan about twenty minutes.

### What you will learn

- Sending context aware prompts to Copilot Chat.

- Generating idiomatic code in a target language.

- Reviewing translated code for correctness and style.

- Isolating experiments in a scrapbook or draft branch.

### Prerequisites

Confirm Copilot Chat is active in **your IDE** such as VS Code JetBrains Visual Studio or Neovim. Ensure you have permissions to create a new branch in the repository you are using.

### Steps

- **Step 1.** Open a file that contains a non trivial snippet you want to translate for example a Java method.

- **Step 2.** Highlight the entire snippet including any import statements that matter.

- **Step 3.** Open Copilot Chat and type `Translate to Go idioms` then press Enter.

- **Step 4.** Review the generated Go code. Use **Tab** to accept or **Esc** to ask follow up questions such as performance considerations.

- **Step 5.** Create a scrapbook branch with `git checkout -b translate-experiment` or use your IDE branch controls.

- **Step 6.** Add a new file for example `snippet.go` and paste the translated code. Run `go vet` or another linter to verify compilation.

- **Step 7.** Commit the file with a descriptive message such as Add Go translation of example snippet.

### Checkpoint

1. Did Copilot produce syntactically correct code in the target language?

- [ ] Yes
- [ ] No

2. Did the translated snippet compile or pass linting tools?

- [ ] Yes
- [ ] No

3. Did you create and commit the translation in a separate scrapbook branch?

- [ ] Yes
- [ ] No

### Explore more

- [Translating code across languages with Copilot](https://docs.github.com/en/copilot/copilot-chat-cookbook/refactoring-code/translating-code-to-a-different-programming-language)

- [Copilot in VS Code documentation](https://code.visualstudio.com/docs/copilot/overview)
