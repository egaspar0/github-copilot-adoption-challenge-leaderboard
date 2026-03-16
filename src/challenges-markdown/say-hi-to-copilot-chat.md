---
Title: Say hi to Copilot Chat
ActivityId: 12
---

### Summary

In this exercise you will use **GitHub Copilot Chat** to understand and improve code that already exists in your repository. You will select a confusing block, ask Copilot Chat to explain it, then request a simpler rewrite. The entire task should take about twenty minutes.

### What you will learn

- Opening Copilot Chat inside your IDE.

- Sending prompts that refer to selected code.

- Asking follow up questions to refine Copilot responses.

- Evaluating and applying a suggested rewrite to your own code.

### Prerequisites

Confirm that Copilot Chat is enabled in **your IDE** (use the Copilot sidebar in VS Code or the Copilot Chat tool window in JetBrains and Visual Studio). Work inside a repository you own or have permission to change.

### Steps

- **Step 1.** Select a block of code that you find hard to read or overly complex.

- **Step 2.** With the code still highlighted open the Copilot Chat panel. In VS Code choose **Copilot Chat** from the Activity Bar. In JetBrains use **View > Tool Windows > Copilot Chat**.

- **Step 3.** Type `Explain what this does` and press Enter. Read Copilot’s explanation and make sure it matches your intention.

- **Step 4.** In the same conversation type `Can you suggest a simpler rewrite`. Copilot will return a refactored version of the selected code.

- **Step 5.** Compare the suggested code with the original. If you prefer the new version paste it over the old block then run your tests or build command.

- **Step 6.** Commit the change with a clear message. You can pause at the commit prompt and let Copilot propose a message if you wish.

### Checkpoint

1. Did Copilot Chat provide an explanation that aligned with your understanding of the code?

- [ ] Yes
- [ ] No

2. Did you ask for and receive a simpler rewrite?

- [ ] Yes
- [ ] No

3. Did the rewritten code compile or pass tests after you applied it?

- [ ] Yes
- [ ] No

### Explore more

- [Getting started with Copilot Chat](https://code.visualstudio.com/docs/copilot/chat/getting-started-chat)

- [Using Copilot Chat in VS Code](https://learn.microsoft.com/en-us/visualstudio/ide/visual-studio-github-copilot-chat?view=vs-2022)

- [Copilot Chat for JetBrains IDEs](https://plugins.jetbrains.com/plugin/17718-github-copilot)
