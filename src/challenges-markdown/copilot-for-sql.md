---
Title: Copilot for SQL
ActivityId: 12
---

### Summary

In this activity you will let **GitHub Copilot** write SQL for you by adding a plain English comment to a data access file. Copilot will turn the comment `-- top 10 customers by spend last year` into a full query. You will run the query against your database and review the results. Allocate about fifteen minutes.

### What you will learn

- Prompting Copilot with natural language in SQL files.

- Accepting inline SQL suggestions with the keyboard.

- Executing generated queries in your database client.

- Validating query accuracy against sample data.

### Before you start

Confirm Copilot is active in **your IDE** such as VS Code or JetBrains. Ensure you have a database connection configured and working query runner for example psql mysql cli or the built in database panel.

### Steps

- **Step 1.** Open the file in your data layer where SQL queries are stored for example `customerQueries.sql` or a repository migration file.

- **Step 2.** On a new line type the comment `-- top 10 customers by spend last year` and press Enter.

- **Step 3.** Pause. Copilot shows a complete SQL query suggestion. Press **Tab** to accept or use **Alt ]** or **Option ]** to view other versions.

- **Step 4.** Save the file then copy the generated query or run it directly in your database client connected to the correct schema.

- **Step 5.** Verify that the query returns ten rows ordered by spend for the previous calendar year. Adjust date filters if required then rerun.

- **Step 6.** Commit the new query file or update with a descriptive message possibly drafted by Copilot.

### Checkpoint

1. Did Copilot produce a syntactically correct SQL query?

- [ ] Yes
- [ ] No

2. Did the query return the expected ten rows sorted by spend?

- [ ] Yes
- [ ] No

3. Did you commit the query or save it in the data layer?

- [ ] Yes
- [ ] No

### Explore more

- [Generating SQL with GitHub Copilot](https://dev.to/karenpayneoregon/github-copilot-generate-sql-pd3)

- [Unleashing SQL Sorcery with GitHub Copilot](https://techcommunity.microsoft.com/blog/azuresqlblog/unleashing-sql-sorcery-increasing-performance-and-complexity-with-github-copilot/3898909)

- [Best Practices for Using GitHub Copilot](https://docs.github.com/en/copilot/using-github-copilot/best-practices-for-using-github-copilot)

- [Turbocharge Your SQL Development Workflow](https://techcommunity.microsoft.com/blog/azuresqlblog/github-copilot-for-sql-developers-turbocharge-your-sql-development-workflow/3875915)
