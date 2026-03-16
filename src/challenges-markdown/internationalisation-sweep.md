---
Title: Internationalisation sweep
ActivityId: 12
---

### Summary

In this localisation exercise you will use **GitHub Copilot Chat in EDIT mode** to internationalise a React view. Copilot will wrap hard coded user facing strings in `t()` calls and create an `en.json` translation file. You will then verify that the application still renders correctly. Plan about twenty minutes.

### What you will learn

- Finding hard coded strings in a React component.

- Prompting Copilot Chat to apply `t()` wrappers automatically.

- Generating an `en.json` translation catalogue.

- Running and visually verifying the app after internationalisation.

### Before you start

Confirm Copilot Chat is enabled in **your IDE** such as VS Code or JetBrains. Ensure your React project uses a library like `react-i18next` and runs successfully with `npm start` or `yarn dev`.

### Steps

- **Step 1.** Open a React component that contains visible hard coded strings for example headings button labels or error messages.

- **Step 2.** Select the whole component or the JSX section with text and open Copilot Chat in EDIT mode.

- **Step 3.** Type `Wrap all user facing strings in t() and generate an en.json file` then press Enter.

- **Step 4.** Copilot lists edits that replace each literal with `{t('key')}` and proposes an `en.json` object. Accept the edits and save the translation file under `src/locales/en.json`.

- **Step 5.** Run `npm start` or your usual dev server and load the page in a browser. Check that every string appears and no translation key is missing.

- **Step 6.** Commit the changes including the updated component and `en.json` with a descriptive message.

### Checkpoint

1. Did Copilot replace every hard coded string with a `t()` call?

- [ ] Yes
- [ ] No

2. Did the generated `en.json` include keys for each wrapped string?

- [ ] Yes
- [ ] No

3. Did the application render correctly after the change?

- [ ] Yes
- [ ] No

### Explore more

- [react-i18next documentation](https://react.i18next.com/)

- [Copilot Edits VS 2022 overview](https://learn.microsoft.com/en-us/visualstudio/ide/copilot-edits)

- [Copilot Edits VS Code overview](https://code.visualstudio.com/docs/copilot/chat/copilot-edits)

- [Copilot Edits Jetbrains overview](https://github.blog/changelog/2025-03-20-enhance-your-productivity-with-copilot-edits-in-jetbrains-ides/)
