---
Title: Doc to spec converter
ActivityId: 12
---

### Summary

In this exercise you will use **GitHub Copilot Agents** inside VS Code to summarise a set of commits that modified your module (eg:Authentication module). You will ask the Agent `Summarise auth module changes across commits a1…a6 in this repo` then review the generated overview. Plan about fifteen minutes.

### What you will learn

- Enabling Copilot Agent mode in VS Code.

- Prompting an Agent to analyse multiple commits.

- Reading a concise natural language diff summary.

- Sharing the summary with your team or attaching it to a pull request.

### Before you start

Install the latest **GitHub Copilot** extension in VS Code and enable Agent mode under **Settings > GitHub Copilot > Agents**. Ensure you have at least six sequential commits that touched the module (ex: authentication) and know their hashes (for example `a1` through `a6`).

### Steps

- **Step 1.** Open your repository in VS Code and switch to the Copilot panel. Click **Agent** to enter Agent mode.

- **Step 2.** In the Agent prompt type `Summarise auth module changes across commits a1…a6 in this repo` then press Enter.

- **Step 3.** Wait while the Agent calls the repo search tool and analyses each commit. A structured summary appears describing added files removed functions and key behaviour changes.

- **Step 4.** Copy the summary to your clipboard or click **Insert to editor** to save it in `docs/auth-change-log.md`.

- **Step 5.** If any detail is unclear ask a follow up question such as `Show code diff for the login validator refactor` and review the answer.

- **Step 6.** Commit the summary file on a new branch and push for review.

### Checkpoint

1. Did the Agent return a clear description of all six commits?

- [ ] Yes
- [ ] No

2. Did you ask at least one follow up question to clarify a change?

- [ ] Yes
- [ ] No

3. Did you save or share the summary with your team?

- [ ] Yes
- [ ] No

### Explore more

- [Copilot Agents overview](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode)
