---
Title: Agent driven repo clean up
ActivityId: 12
---

### Summary

In this exercise you will use **GitHub Copilot Agent mode** to automate a repository clean up. You will prompt the Agent to find dead code paths across the project and create a pull request that removes them. After reviewing the changes and running tests you will merge the pull request. Set aside twenty minutes.

### What you will learn

- Prompting Copilot Agents for repository wide analysis.

- Letting the Agent generate a branch and pull request automatically.

- Reviewing deletions to confirm only dead code is removed.

- Running tests before merging automated clean-up changes.

### Before you start

Install the latest **GitHub Copilot** extension in VS Code and enable Agent mode under **Settings > GitHub Copilot > Agents**. Ensure your repository builds cleanly and that all tests pass with `npm test` `pytest` `dotnet test` or your framework's command.

### Steps

- **Step 1.** Open the repository in VS Code and switch to the Copilot panel. Click **Agent** to enter Agent mode.

- **Step 2.** In the Agent prompt type `Find dead code paths in this repo and open a PR deleting them` then press Enter.

- **Step 3.** Wait while the Agent performs static analysis, creates a new branch for example `agent-dead-code-cleanup` and opens a pull request.

- **Step 4.** Open the pull request in GitHub. Review the diff to ensure only unused code paths are removed and no critical logic is affected.

- **Step 5.** Run your full test suite `npm test` `pytest` `dotnet test` or rely on continuous integration checks. Confirm all tests pass.

- **Step 6.** If any test fails ask the Agent a follow-up prompt such as `Undo removal of misidentified function validateUser` apply the patch, then rerun tests.

- **Step 7.** Once tests pass click **Merge pull request**. Delete the branch if prompted.

### Checkpoint

1. Did the Agent open a pull request that deleted only dead code?

- [ ] Yes
- [ ] No

2. Did all tests pass after the clean up?

- [ ] Yes
- [ ] No

3. Did you merge the pull request and delete the clean up branch?

- [ ] Yes
- [ ] No

### Explore more

- [Copilot Agents overview](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode)

- [Setting up tests in GitHub Actions](https://docs.github.com/en/actions/using-workflows)
