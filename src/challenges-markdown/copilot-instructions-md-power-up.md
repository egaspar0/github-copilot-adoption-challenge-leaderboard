---
Title: Copilot-instructions.md power-up
ActivityId: 12
---

### Summary

In this personalization exercise you will add a `copilot-instructions.md` file to your repository root (or use the VS Code Personalize panel) and write three sentences that describe your preferred coding style, frameworks, and docstring format. After saving the file you will ask GitHub Copilot to draft a helper utility and observe how it follows your new instructions. The task should take about fifteen minutes.

### What you will learn

- Creating or editing `copilot-instructions.md` to guide Copilot output.

- Using the Personalize panel in VS Code to manage Copilot instructions.

- Requesting Copilot to generate code that follows custom guidelines.

- Committing repository level instructions for team visibility.

### Before you start

Ensure GitHub Copilot is enabled in **your IDE** such as VS Code JetBrains or Visual Studio. Have at least one branch where you can commit the new file without affecting production code.

### Steps

- **Step 1.** Create and check out a branch named `copilot-instructions` using `git checkout -b copilot-instructions` or the source control panel.

- **Step 2.**Create a new file `.github/copilot-instructions.md` in the repository root.

- **Step 3.** Add exactly three sentences that describe:

- Your preferred indentation and naming style.

- Your main framework or library choices.

- Your docstring or comment format.

Example: `I use four space indentation and snake_case for functions. All helpers target FastAPI. Docstrings must follow the Google style guide.`

- **Step 4.** Save the file or panel and close it.

- **Step 5.** Open any source file and prompt Copilot Chat with `Draft a helper utility to validate email addresses`. Accept the suggestion.

- **Step 6.** Confirm that the generated code matches your stated style, framework import, and docstring format.

- **Step 7.** Stage and commit `.github/copilot-instructions.md` with a clear message then push the branch and open a pull request.

### Checkpoint

1. Did you add three clear sentences to `copilot-instructions.md`?

- [ ] Yes
- [ ] No

2. Did Copilot generate a helper utility that follows your specified style?

- [ ] Yes
- [ ] No

3. Did you commit the new instruction file to the repository?

- [ ] Yes
- [ ] No

### Explore more

- [Custom instructions for GitHub Copilot](https://docs.github.com/en/copilot/customizing-copilot/adding-repository-custom-instructions-for-github-copilot)

- [Getting started with Copilot](https://docs.github.com/en/copilot/getting-started-with-github-copilot)
