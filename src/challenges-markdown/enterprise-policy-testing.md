---
Title: Enterprise policy testing
ActivityId: 12
---

### Summary

In this governance exercise you will configure **Copilot content exclusions** at the repository level to prevent GitHub Copilot from accessing files that contain secrets or sensitive configuration. You will add exclusion patterns through the GitHub.com settings UI, then verify that Copilot no longer provides completions or chat responses using the excluded files. Plan about fifteen minutes.

### What you will learn

- Configuring content exclusion patterns at the repository level.

- Testing that Copilot ignores excluded files for completions, chat, and inline suggestions.

- Understanding the scope and limitations of content exclusions across different Copilot features.

### Before you start

Content exclusions require a **GitHub Copilot Business or Enterprise** plan. You need **admin access** to the repository where you want to configure exclusions. Open the repository on GitHub.com and confirm you can access **Settings > Copilot**.

### Steps

- **Step 1.** Navigate to your repository on GitHub.com. Click **Settings**, then in the left sidebar under *Code & automation* click **Copilot**.

- **Step 2.** Under **Content exclusion**, click **New rule**. Give the rule a descriptive name such as `Block secret files`.

- **Step 3.** In the **Paths to exclude** field add one pattern per line. For example:

  ```text
  **/.env
  **/.env.*
  **/secrets/**
  **/config/credentials.*
  ```

- **Step 4.** Click **Save** to apply the exclusion rule. The patterns use fnmatch syntax relative to the repository root.

- **Step 5.** Open VS Code and navigate to a file that matches one of your exclusion patterns (for example `.env`). Confirm that Copilot does **not** offer code completions or inline suggestions for that file. You should see a Copilot status icon indicating the file is excluded.

- **Step 6.** Open Copilot Chat and ask a question that references an excluded file, for example `Summarize the contents of .env`. Confirm that Copilot does not use the excluded file context in its response.

- **Step 7.** Open a non-excluded file and confirm Copilot still works normally, providing completions and chat responses as expected.

### Checkpoint

1. Did you add content exclusion patterns covering your secret files?

- [ ] Yes
- [ ] No

2. Does Copilot stop offering suggestions in excluded files?

- [ ] Yes
- [ ] No

3. Does Copilot still work normally on non-excluded files?

- [ ] Yes
- [ ] No

### Explore more

- [Excluding content from GitHub Copilot](https://docs.github.com/en/copilot/managing-copilot/configuring-and-auditing-content-exclusion/excluding-content-from-github-copilot)

- [Copilot policies overview](https://docs.github.com/en/copilot/concepts/policies)
