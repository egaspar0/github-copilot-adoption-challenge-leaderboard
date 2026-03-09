---
Title: Cloud Agent
ActivityId: 12
---

### Summary

In this exercise you will use the **GitHub Copilot coding agent** (cloud agent) to perform a non-trivial code change entirely in the cloud. The cloud agent creates a remote session, implements changes autonomously, and opens a pull request for your review. Expect to spend twenty to thirty minutes.

### What you will learn

- Starting a cloud agent session from VS Code or GitHub.com.
- Providing a well-structured prompt that the cloud agent can execute autonomously.
- Monitoring the cloud agent progress and reviewing its generated pull request.
- Handing off a local Plan session to the cloud agent for execution.

### Before you start

Ensure your GitHub organization has **Copilot coding agent** enabled. You need write access to a repository hosted on GitHub. Have the latest VS Code with the GitHub Copilot Chat extension installed.

### Steps

- **Step 1.** Open Copilot Chat in VS Code and select **Cloud** from the session type dropdown at the top of the panel.

- **Step 2.** Enter a refactoring prompt that affects multiple files, for example: `Refactor the logging module to use structured logging with JSON output across all services.`

- **Step 3.** The cloud agent provisions a remote environment. Watch the status bar for progress. You can continue working locally while it runs.

- **Step 4.** When the cloud agent finishes, it posts a notification with a link to a new pull request. Open the pull request on GitHub.

- **Step 5.** Review the diff, inspect the commit messages the agent wrote, and verify tests pass in CI.

- **Step 6.** Alternatively, start a **Plan** session locally by switching to Plan mode. After the plan is generated, click **Hand off to cloud agent** to let the coding agent execute the plan remotely.

- **Step 7.** Merge the pull request once you are satisfied with the changes.

### Checkpoint

1. Did the cloud agent successfully create a remote session and begin working on your prompt

- [ ] Yes
- [ ] No

2. Did the cloud agent open a pull request with the implemented changes

- [ ] Yes
- [ ] No

3. Were you able to review the diff and verify the changes before merging

- [ ] Yes
- [ ] No

### Explore more

- [Cloud agents in VS Code](https://code.visualstudio.com/docs/copilot/chat/cloud-agents)

- [Copilot coding agent on GitHub](https://docs.github.com/en/copilot/using-github-copilot/using-the-copilot-coding-agent)

