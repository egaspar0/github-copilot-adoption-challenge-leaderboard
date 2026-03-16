---
Title: Refactor with confidence
ActivityId: 12
---

### Summary

In this activity you will use **GitHub Copilot Chat** to refactor a long method by extracting smaller helper methods while keeping behaviour unchanged. Copilot will also create unit tests first so you can confirm nothing breaks. The exercise should take about twenty minutes.

### What you will learn

- Selecting a candidate for refactor in your own code.

- Prompting Copilot Chat to extract helper methods and generate tests up front.

- Running the new tests to verify behaviour before and after refactor.

- Committing refactor changes with confidence.

### Prerequisites

Ensure Copilot Chat is enabled in **your IDE** such as VS Code JetBrains or Visual Studio. Confirm your project has a working test runner like Jest PyTest JUnit or xUnit.

### Steps

- **Step 1.** Locate a method longer than about thirty lines that mixes multiple responsibilities.

- **Step 2.** Highlight the entire method then open Copilot Chat.

- **Step 3.** Type `Extract smaller methods write tests first keep behaviour` and press Enter.

- **Step 4.** Copilot will propose unit tests followed by the refactor plan and code. Accept the tests first, save them in your test folder (if not create one) and then copy the refactored code.

- **Step 5.** Run your test suite for example `npm test` `pytest` or `dotnet test`. All tests should pass confirming behaviour is intact.

- **Step 6.** Review helper method names and visibility. Rename or adjust signatures if needed then rerun tests.

- **Step 7.** Commit the changes with a descriptive message. You may let Copilot draft the commit message once your staged diff is ready.

### Checkpoint

1. Did Copilot create unit tests before performing the refactor?

- [ ] Yes
- [ ] No

2. Did all tests pass after the new helper methods were added?

- [ ] Yes
- [ ] No

3. Did you confirm that the refactored code reads more clearly than before?

- [ ] Yes
- [ ] No

### Explore more

- [Refactoring code with GitHub Copilot](https://docs.github.com/en/copilot/using-github-copilot/guides-on-using-github-copilot/refactoring-code-with-github-copilot)

- [How to refactor code with GitHub Copilot](https://github.blog/ai-and-ml/github-copilot/how-to-refactor-code-with-github-copilot/)
