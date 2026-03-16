---
Title: Docstring on demand
ActivityId: 12
---

### Summary

This exercise shows how to let **GitHub Copilot** write documentation for your own code. You will insert a docstring marker above an undocumented function or class and accept the generated text. The task should take about fifteen minutes.

### What you will learn

- Triggering docstring generation with `/**` in languages such as JavaScript Java and C#.

- Triggering docstring generation with `"""` in Python.

- Accepting or cycling through Copilot suggestions to document code quickly.

- Reviewing and editing generated documentation for clarity and accuracy.

### Prerequisites

Make sure GitHub Copilot is active in **your IDE** (VS Code JetBrains Visual Studio or Neovim). Open a repository you own or maintain so you can save changes.

### Steps

- **Step 1.** Find a function or class in your code that has no documentation.

- **Step 2.** Place the cursor on the line directly above that definition.

- **Step 3.** Type `/**` if you are in JavaScript Java or C# or type `"""` if you are in Python then press Enter.

- **Step 4.** Wait for Copilot to suggest a complete docstring. Press **Tab** to accept or use **Alt ]** or **Option ]** to view other versions.

- **Step 5.** Read the generated text. Add or correct details so that parameters return values and side effects are clear.

- **Step 6.** Repeat Steps 1 to 5 on one more function in another language file if possible.

- **Step 7.** Commit your changes with a message that mentions added documentation. You may let Copilot draft the commit message at the prompt.

### Checkpoint

1. Did Copilot generate a docstring without manual typing?

- [ ] Yes
- [ ] No

2. Did you cycle through more than one suggestion before accepting?

- [ ] Yes
- [ ] No

3. Did you edit the generated docstring to better fit your code style?

- [ ] Yes
- [ ] No

### Explore more

- [Getting started with GitHub Copilot](https://docs.github.com/en/copilot/getting-started-with-github-copilot)

- [Quickstart guide for common Copilot tasks](https://docs.github.com/en/copilot/quickstart)

- [Using Copilot in VS Code](https://github.blog/ai-and-ml/github-copilot/documenting-and-explaining-legacy-code-with-github-copilot-tips-and-examples/)
