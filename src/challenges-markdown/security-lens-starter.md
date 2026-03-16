---
Title: Security lens starter
ActivityId: 12
---

### Summary

In this security focused exercise you will copy a data access function from your repository that may contain a vulnerability. You will ask **GitHub Copilot Chat** to identify potential OWASP risks and provide a safer version of the code. Finally you will write a quick unit test to confirm the fix works. Allocate about twenty minutes.

### What you will learn

- Sending real code snippets to Copilot Chat for security analysis.

- Mapping identified issues to OWASP categories.

- Generating a safer refactor that mitigates the risk.

- Writing a simple unit test to validate the secure behaviour.

### Prerequisites

Ensure Copilot Chat is enabled in **your IDE** for example VS Code JetBrains or Visual Studio. Confirm your project has a working test runner such as Jest PyTest JUnit or xUnit.

### Steps

- **Step 1.** In your code, select a data access function that may be vulnerable for example one that concatenates user input into a SQL string.

- **Step 2.** Open Copilot Chat, note how the selected line numbers are automatically included and type `Identify OWASP risks and provide a safer version` then press Enter.

- **Step 3.** Review the risks Copilot lists such as SQL Injection or Insecure Deserialization. Examine the safer code suggestion.

- **Step 4.** Accept the secure refactor or copy it into a new branch for example `security-fix`.

- **Step 5.** Ask Copilot Chat `Write a unit test that proves the fix blocks the vulnerability` then save the test file in your test directory.

- **Step 6.** Run your tests with `npm test` `pytest` or `dotnet test`. Confirm they pass.

- **Step 7.** Commit the secure code and tests with a descriptive message drafted by you or suggested by Copilot.

### Checkpoint

1. Did Copilot correctly highlight at least one OWASP risk in your original code?

- [ ] Yes
- [ ] No

2. Did you implement the safer version without breaking existing functionality?

- [ ] Yes
- [ ] No

3. Did the new unit test pass confirming the vulnerability is mitigated?

- [ ] Yes
- [ ] No

### Explore more

- [Finding existing vulnerabilities in code](https://docs.github.com/en/copilot/copilot-chat-cookbook/security-analysis/finding-existing-vulnerabilities-in-code)

- [OWASP Top Ten reference](https://owasp.org/www-project-top-ten)
