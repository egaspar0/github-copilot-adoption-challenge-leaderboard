---
Title: CLI suggest & execute
ActivityId: 12
---

### Summary

In this exercise you will use **GitHub Copilot CLI** to suggest and run a command that cleans up local branches already merged into main. You will review the agent's plan, approve the command, then verify the result with `git branch`. The task should take about ten minutes.

### What you will learn

- Launching the `copilot` CLI interactive agent to request shell commands.

- Reviewing and approving the agent's suggested command before execution.

- Verifying local branch cleanup with `git branch`.

- Using Copilot CLI for other repository maintenance tasks.

### Prerequisites

Confirm that the Copilot CLI is installed with `copilot --version`. Authenticate with `copilot login` and open a terminal in the root of a repository that has multiple merged branches.

### Steps

- **Step 1.** In the project root launch the interactive Copilot CLI agent.

`copilot`

- **Step 2.** In the interactive session type: `Delete all local branches that have already been merged into main`. The agent will propose a shell command — it should resemble `git branch --merged | grep -v "main" | xargs -n 1 git branch -d`.

- **Step 3.** When the agent asks for permission to execute the command, review it and approve. If the agent asks clarifying questions, confirm you want to clean up merged branches.

- **Step 4.** After execution run `git branch` to list local branches and confirm that only active branches remain.

- **Step 5.** Explore other cleanup ideas by typing `List the largest files in this repo` or a task of your choice. Exit the CLI with `/exit` when done.

- **Step 6.** Commit any auxiliary changes if applicable and push to remote.

### Checkpoint

1. Did Copilot suggest a safe branch cleanup command?

- [ ] Yes
- [ ] No

2. Did the command remove local branches that are already merged?

- [ ] Yes
- [ ] No

3. Did you confirm the result with `git branch`?

- [ ] Yes
- [ ] No

### Explore more

- [GitHub Copilot in the CLI](https://docs.github.com/en/copilot/github-copilot-in-the-cli/about-github-copilot-in-the-cli)
