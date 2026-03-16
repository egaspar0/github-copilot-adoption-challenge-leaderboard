---
Title: Cross file rename
ActivityId: 12
---

### Summary

In this refactor exercise you will use **GitHub Copilot Chat** to rename every occurrence of the class `User` to `Member` across your repository. Copilot will propose bulk edits update imports and you will finish by running your build or test suite. Allocate around twenty minutes.

### What you will learn

- Prompting Copilot Chat for a project wide rename.

- Reviewing bulk edit suggestions before applying them.

- Running build or test commands to verify a large refactor.

- Committing refactor changes with confidence.

### Before you start

Ensure Copilot Chat is enabled in **your IDE** for example VS Code JetBrains or Visual Studio. If you are using a compatible editor you may switch to EDIT mode. See references section below for instructions. Confirm your project builds cleanly and that you have a working test runner such as Jest PyTest JUnit or xUnit.

### Steps

- **Step 1.** Open any file that defines or references the class `User`.

- **Step 2.** Launch Copilot Chat and type `Rename class User to Member across the repo update imports` then press Enter.

- **Step 3.** Copilot displays a list of bulk edits. Review each change to confirm file paths and renamed symbols are correct.

- **Step 4.** Approve the bulk edits. Copilot will commit the changes to your working tree.

- **Step 5.** Run your normal build or test command such as `npm test` `pytest` or `dotnet build`. Ensure everything compiles and all tests pass.

- **Step 6.** If errors occur ask Copilot Chat follow up questions for example `Fix remaining User references` then re run the build.

- **Step 7.** Commit the refactor with a message like `refactor: rename User to Member across codebase`. You may allow Copilot to draft the commit message at the prompt.

### Checkpoint

1. Did Copilot list every file requiring a rename?

- [ ] Yes
- [ ] No

2. Did the project build or all tests pass after the rename?

- [ ] Yes
- [ ] No

3. Did you commit the refactor with a descriptive message?

- [ ] Yes
- [ ] No

### Explore more

- [Copilot Edits VS 2022 overview](https://learn.microsoft.com/en-us/visualstudio/ide/copilot-edits)

- [Copilot Edits VS Code overview](https://code.visualstudio.com/docs/copilot/chat/copilot-edits)

- [Copilot Edits Jetbrains overview](https://github.blog/changelog/2025-03-20-enhance-your-productivity-with-copilot-edits-in-jetbrains-ides/)

- [Managing files in a repository](https://docs.github.com/en/repositories/working-with-files/managing-files)
