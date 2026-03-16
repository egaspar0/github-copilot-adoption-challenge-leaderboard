---
Title: Commit message concierge
ActivityId: 12
---

### Summary

This exercise shows how to let **GitHub Copilot** write commit messages for you. You will stage today’s changes invoke Copilot’s commit message helper accept the draft then amend it with extra context if required. The task should take ten to fifteen minutes.

### What you will learn

- Staging file changes with `git add` or your IDE source control panel.

- Triggering Copilot to draft a commit message from the staged diff.

- Editing the AI generated message to add project specific context.

- Committing and pushing the change to your remote repository.

### Prerequisites

Confirm that GitHub Copilot is enabled in **your IDE** for example VS Code JetBrains or Visual Studio. Make sure you have several modified files ready to commit in your repository.

### Steps

- **Step 1.** Stage all changes by running `git add -A` in the terminal or selecting files in the IDE source control view.

- **Step 2.** Start the commit process.

- In VS Code place the cursor in the commit message box.

- In JetBrains open the Commit tool window.

- **Step 3.** Wait a moment. Copilot detects the staged diff and inserts a suggested message.

- **Step 4.** Read the draft. Use the keyboard to append extra context such as ticket numbers or motivations.

- **Step 5.** Save the commit.

- In the terminal type `git commit` then accept the message.

- In the IDE click Commit or press the shortcut.

- **Step 6.** Push the commit to your remote with `git push` or the IDE push command.

### Checkpoint

1. Did Copilot generate a commit message automatically from your staged changes?

- [ ] Yes
- [ ] No

2. Did you add additional detail such as a ticket reference or rationale?

- [ ] Yes
- [ ] No

3. Did the commit push successfully to the remote repository?

- [ ] Yes
- [ ] No

### Explore more

- [Generating commit messages with Copilot](https://docs.github.com/en/copilot/getting-started-with-github-copilot#generate-commit-messages)
