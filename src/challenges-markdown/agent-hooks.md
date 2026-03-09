---
Title: Agent Hooks
ActivityId: 12
---

### Summary

In this exercise you will create **agent hooks** that run custom scripts at specific points during an agent session lifecycle. Hooks let you enforce policies, inject context, or run automations whenever the agent starts, stops, or uses a tool. Expect to spend twenty to thirty minutes.

### What you will learn

- Understanding the eight agent lifecycle events available for hooks.
- Creating a hook configuration file in `.github/hooks/`.
- Writing a PreToolUse hook to block or gate specific tool calls.
- Writing a PostToolUse hook to run formatting or validation after edits.
- Using the `/create-hook` slash command to generate hooks with AI assistance.

### Before you start

Ensure VS Code has the latest GitHub Copilot Chat extension with agent mode enabled. Have a repository where you can add files to `.github/hooks/`. Shell scripting basics will be helpful.

### Steps

- **Step 1.** Create the folder `.github/hooks/` in your repository.

- **Step 2.** Create a file `.github/hooks/security-check.json` with the following structure: set the `event` to `PreToolUse`, specify a `command` that runs a shell script, and add a `description` explaining the hook blocks writes to files matching sensitive patterns like `.env` or `secrets`.

- **Step 3.** Create the corresponding shell script referenced in the hook. The script should check the tool arguments for sensitive file paths and exit with a non-zero code to block the operation, or exit zero to allow it.

- **Step 4.** Start an Agent mode session and ask Copilot to edit a `.env` file. Verify the hook blocks the edit and the agent reports the rejection.

- **Step 5.** Create a second hook file `.github/hooks/format-on-save.json` with `event` set to `PostToolUse`. Configure it to run a formatter (for example `prettier --write` or `dotnet format`) on any file the agent just modified.

- **Step 6.** Test the PostToolUse hook by asking the agent to create or edit a source file. Verify the formatter runs automatically after the edit.

- **Step 7.** Use the `/create-hook` slash command in Copilot Chat to generate a `SessionStart` hook that logs the session start time and injects project-specific context.

### Checkpoint

1. Did the PreToolUse hook successfully block an edit to a sensitive file

- [ ] Yes
- [ ] No

2. Did the PostToolUse hook run the formatter automatically after an agent edit

- [ ] Yes
- [ ] No

3. Were you able to create a SessionStart hook using the /create-hook command

- [ ] Yes
- [ ] No

### Explore more

- [Agent hooks in VS Code](https://code.visualstudio.com/docs/copilot/customization/agent-hooks)

- [Agent tools and approval](https://code.visualstudio.com/docs/copilot/agents/agent-tools)

