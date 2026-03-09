---
Title: Agent Hooks
ActivityId: 12
---

### Summary

In this exercise you will create **agent hooks** — small shell scripts that run automatically at specific points during a GitHub Copilot agent session. You will build four hooks that cover real-world scenarios: blocking hardcoded secrets, auto-running tests, injecting project context, and logging prompts. Expect to spend thirty to forty-five minutes.

### What you will learn

- Understanding the eight agent lifecycle events available for hooks.
- Creating hook configuration files in `.github/hooks/`.
- Writing a `PreToolUse` hook that scans for hardcoded secrets and blocks the write if any are found.
- Writing a `PostToolUse` hook that automatically runs unit tests whenever the agent modifies a source file.
- Using a `SessionStart` hook to inject project context from a `CONTEXT.md` file into every agent session.
- Using the `/create-hook` slash command to generate a `UserPromptSubmit` hook that logs every agent prompt to a local file.

### Before you start

- Ensure VS Code has the latest GitHub Copilot Chat extension installed and **agent mode is enabled**.
- You need a terminal (bash or zsh on Mac/Linux, or Git Bash on Windows).
- No prior shell scripting experience is required — all script content is provided in the steps below.

### Steps

---

#### 🔐 Hook 1 — Block hardcoded secrets (PreToolUse)

- **Step 1.** Open your terminal and navigate to the root of your repository. Run the following command to create the hooks folder:

  ```bash
  mkdir -p .github/hooks
  ```

- **Step 2.** Create the hook configuration file by running this command in your terminal:

  ```bash
  cat > .github/hooks/secret-scanner.json << 'EOF'
  {
    "event": "PreToolUse",
    "command": "bash .github/hooks/secret-scanner.sh",
    "description": "Blocks any file write that contains a hardcoded secret pattern such as GitHub tokens, AWS keys, or passwords."
  }
  EOF
  ```

- **Step 3.** Create the shell script that does the actual scanning by running this command in your terminal:

  ```bash
  cat > .github/hooks/secret-scanner.sh << 'EOF'
  #!/bin/bash
  # Read the file content being written from stdin
  content=$(cat)

  # Check for common secret patterns
  if echo "$content" | grep -qE 'ghp_[A-Za-z0-9]+|AKIA[0-9A-Z]{16}|sk-[A-Za-z0-9]+|password\s*=\s*\S+'; then
    echo "❌ SECRET DETECTED: This file contains a hardcoded secret and cannot be written."
    exit 1
  fi

  exit 0
  EOF
  chmod +x .github/hooks/secret-scanner.sh
  ```

- **Step 4.** Open VS Code, switch to **Agent mode** in the Copilot Chat panel, and send this exact prompt:

  > Create a file called `test-credentials.txt` with the following content: `token=ghp_fakeABCDEFGH1234567890`

  ✅ **Expected result:** Copilot reports the write was blocked and the file is NOT created in your repository.

---

#### 🧪 Hook 2 — Auto-run tests after edits (PostToolUse)

- **Step 5.** Create the hook configuration file by running this command in your terminal:

  ```bash
  cat > .github/hooks/test-runner.json << 'EOF'
  {
    "event": "PostToolUse",
    "command": "bash .github/hooks/test-runner.sh",
    "description": "Automatically runs the relevant test suite whenever the agent modifies a .js, .ts, or .py source file."
  }
  EOF
  ```

- **Step 6.** Create the shell script by running this command in your terminal:

  ```bash
  cat > .github/hooks/test-runner.sh << 'EOF'
  #!/bin/bash
  # The modified file path is passed as the first argument
  MODIFIED_FILE="$1"

  if [[ "$MODIFIED_FILE" == *.js || "$MODIFIED_FILE" == *.ts ]]; then
    echo "🧪 Running JavaScript/TypeScript tests..."
    npm test
  elif [[ "$MODIFIED_FILE" == *.py ]]; then
    echo "🧪 Running Python tests..."
    pytest
  else
    echo "ℹ️ No test runner configured for this file type. Skipping."
    exit 0
  fi
  EOF
  chmod +x .github/hooks/test-runner.sh
  ```

- **Step 7.** In Agent mode, send this exact prompt:

  > Add a comment to the top of any existing `.js`, `.ts`, or `.py` file in this project saying `# Updated by agent`.

  ✅ **Expected result:** After the agent edits the file, you see test output appear automatically in the terminal.

---

#### 🧠 Hook 3 — Inject project context at session start (SessionStart)

- **Step 8.** Create a `CONTEXT.md` file in the root of your repository by running this command in your terminal:

  ```bash
  cat > CONTEXT.md << 'EOF'
  # Project Context

  ## Current Sprint Goal
  Improve test coverage to 80% and fix all critical security linting issues.

  ## Architecture
  This is a Node.js REST API using Express. All routes live in `/src/routes`. Database models are in `/src/models`.

  ## Coding Standards
  - Use `async/await` instead of callbacks.
  - All functions must have JSDoc comments.
  - Never hardcode secrets — use environment variables via `process.env`.
  EOF
  ```

- **Step 9.** Create the hook configuration file by running this command in your terminal:

  ```bash
  cat > .github/hooks/context-injector.json << 'EOF'
  {
    "event": "SessionStart",
    "command": "bash .github/hooks/context-injector.sh",
    "description": "Injects the contents of CONTEXT.md as project context at the start of every agent session."
  }
  EOF
  ```

- **Step 10.** Create the shell script by running this command in your terminal:

  ```bash
  cat > .github/hooks/context-injector.sh << 'EOF'
  #!/bin/bash
  CONTEXT_FILE="CONTEXT.md"

  if [ -f "$CONTEXT_FILE" ]; then
    echo "📋 Injecting project context from $CONTEXT_FILE..."
    cat "$CONTEXT_FILE"
  else
    echo "⚠️ No CONTEXT.md found. Skipping context injection."
  fi
  EOF
  chmod +x .github/hooks/context-injector.sh
  ```

- **Step 11.** Close and reopen the Copilot Chat panel to start a **new agent session**. Then send this exact prompt:

  > What are the coding standards for this project?

  ✅ **Expected result:** Copilot answers using the content from your `CONTEXT.md` file without you having to paste it manually.

---

#### 📋 Hook 4 — Log all prompts (UserPromptSubmit)

- **Step 12.** In the Copilot Chat panel (in Agent mode), type the following slash command:

  > `/create-hook`

  When Copilot asks you to describe the hook, enter this:

  > A `UserPromptSubmit` hook that appends every prompt I send to the agent — along with the current date and time — to a file called `agent-prompt-log.txt` in the repository root.

- **Step 13.** Copilot will generate the hook JSON and shell script for you. Save both files into `.github/hooks/` exactly as Copilot instructs.

- **Step 14.** Send a few test prompts to the agent in Agent mode (for example: `What files are in this repo?`). Then open `agent-prompt-log.txt` in the VS Code file explorer (it will be in the root of your repository).

  ✅ **Expected result:** Each prompt you sent appears as a timestamped line in `agent-prompt-log.txt`, for example:
  ```
  [2026-03-09 10:32:01] What files are in this repo?
  ```

---

### Checkpoint

1. Did the PreToolUse secret scanner hook block the write of `test-credentials.txt` containing `ghp_fakeABCDEFGH1234567890`?

- [ ] Yes
- [ ] No

2. Did the PostToolUse hook automatically run tests in the terminal after the agent edited a source file?

- [ ] Yes
- [ ] No

3. Did the SessionStart hook inject context from `CONTEXT.md` when you asked about coding standards?

- [ ] Yes
- [ ] No

4. Were you able to create a `UserPromptSubmit` logging hook using `/create-hook` and verify timestamped entries in `agent-prompt-log.txt`?

- [ ] Yes
- [ ] No

### Explore more

- [Agent hooks in VS Code](https://code.visualstudio.com/docs/copilot/customization/hooks)

- [Agent tools and approval](https://code.visualstudio.com/docs/copilot/agents/agent-tools)

- [Custom instructions in VS Code](https://code.visualstudio.com/docs/copilot/customization/custom-instructions)