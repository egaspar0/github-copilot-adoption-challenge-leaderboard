---
Title: CLI : Explain a scary command
ActivityId: 12
---

### Summary

In this exercise you will use **GitHub Copilot CLI** to understand a complex shell command from your own project. You will ask Copilot to explain the command `git diff --name-only | head -n 1` through the `gh copilot explain` feature. Plan for about ten to fifteen minutes.

### What you will learn

- Installing and using the `gh copilot` extension.

- Running `gh copilot explain` on a shell command in your terminal.

- Reading Copilot output to confirm understanding of each command part.

- Applying the same method to any unfamiliar command in future.

### Prerequisites

Confirm that the GitHub CLI is installed by running `gh --version`. Install the Copilot extension with `gh extension install github/gh-copilot` if it is not yet present. Authenticate with `gh auth login` and stay in the root folder of a repository you can modify.

### Steps

- **Step 1.** Open a terminal in the root folder of your project.

- **Step 2.** Copy and paste the command below then press Enter.

`gh copilot explain "$(git diff --name-only | head -n 1)"`

- **Step 3.** Wait for Copilot to produce a line by line explanation of the pipeline.

- **Step 4.** Compare each part of the explanation with your own understanding. If something is not clear ask a follow up question such as `gh copilot explain "head -n 1"`.

- **Step 5.** Run the original command without the Copilot prefix and view the output so you can link explanation to real data.

- **Step 6.** Use `gh copilot explain` on a different command from your shell history to reinforce the technique.

### Checkpoint

1. Did Copilot return an explanation that listed every flag and sub command?

- [ ] Yes
- [ ] No

2. Did you confirm the explanation by running the original command yourself?

- [ ] Yes
- [ ] No

3. Did you try `gh copilot explain` on at least one more command from your project?

- [ ] Yes
- [ ] No

### Explore more

- [GitHub Copilot CLI project page](https://githubnext.com/projects/copilot-cli)

- [gh copilot command reference](https://docs.github.com/en/copilot/using-github-copilot/using-github-copilot-in-the-command-line)

- [Getting started with GitHub Copilot](https://docs.github.com/en/copilot/getting-started-with-github-copilot)
