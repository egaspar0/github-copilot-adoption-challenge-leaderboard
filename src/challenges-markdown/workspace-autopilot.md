---
Title: Workspace autopilot
ActivityId: 12
---

### Summary

In this activity you will use **GitHub Copilot coding agent** to fix a real issue in your repository. You will assign Copilot to a GitHub Issue, and it will autonomously create a branch, implement the fix, and open a pull request for your review. Expect to spend twenty to twenty-five minutes.

### What you will learn

- Assigning Copilot coding agent to a GitHub Issue.

- Reviewing the pull request that Copilot creates autonomously.

- Providing feedback via pull request comments to iterate on the solution.

- Merging the pull request after verifying the fix.

### Before you start

Copilot coding agent is available with **GitHub Copilot Pro, Pro+, Business, and Enterprise** plans. Confirm that Copilot coding agent is enabled for your repository (an organization admin may need to enable the policy). Ensure there is a GitHub Issue in your repository that describes a reproducible bug.

### Steps

- **Step 1.** Open the Issue you want to fix on GitHub.com and click **Assignees** in the right sidebar.

- **Step 2.** Select **Copilot** from the assignees list. In the dialog that appears, optionally add specific guidance in the **Optional prompt** field (for example, coding patterns to follow or files to modify).

- **Step 3.** Confirm the target repository and base branch, then click to assign. Copilot coding agent begins working in the background using a GitHub Actions-powered environment.

- **Step 4.** Monitor progress from the **Agents** tab in your repository or from the [agents page](https://github.com/copilot/agents). Copilot will create a `copilot/` branch, commit changes, and open a draft pull request.

- **Step 5.** When Copilot finishes, it requests a review from you. Open the pull request, review the diff, and run tests locally or via CI to confirm the bug is resolved.

- **Step 6.** If changes are needed, leave pull request comments — Copilot will read them and push follow-up commits. Iterate until you are satisfied.

- **Step 7.** Approve and merge the pull request. Close the original Issue if it did not close automatically.

### Checkpoint

1. Did Copilot coding agent create a pull request from the assigned Issue

- [ ] Yes
- [ ] No

2. Did the automated branch and pull request compile and pass tests

- [ ] Yes
- [ ] No

3. Did you merge the pull request and close the Issue

- [ ] Yes
- [ ] No

### Explore more

- [About GitHub Copilot coding agent](https://docs.github.com/en/copilot/concepts/agents/coding-agent/about-coding-agent)

- [Asking GitHub Copilot to create a pull request](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-a-pr)
