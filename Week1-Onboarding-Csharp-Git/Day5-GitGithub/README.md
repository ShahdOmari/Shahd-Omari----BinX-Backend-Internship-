# Day 5 — Git & GitHub Workflow; Week 1 Synthesis

**8 hours**

## Learning Objectives

- Use Git's core commands to track and manage changes to a codebase
- Follow a feature-branch workflow matching how the Phase 3 sprints
  will operate
- Open a pull request with a clear description

## What I Did

- Set up `.gitignore` (`bin/`, `obj/`, `.vs/`) and connected the
  local repository to GitHub via `git remote add origin`
- Created a feature branch (`feature/week1-day5-git-workflow`)
- Committed the Day 5 practice project and README updates with clear,
  descriptive commit messages
- Pushed the feature branch and opened a Pull Request from it into
  `main`, with a title and description summarizing the week's work
- Added the mentor as a collaborator on the repository to request
  their review

## Key Commands

```bash
git checkout -b feature/week1-day5-git-workflow
git add .
git commit -m "Add Day 5 practice project and update README"
git push -u origin feature/week1-day5-git-workflow
```

## What I Learned

Working on a feature branch instead of pushing straight to `main`
made the purpose of the workflow concrete rather than theoretical —
`main` stays stable and reviewable at all times, and the Pull Request
is what actually documents "what changed and why" in one place
instead of scattering that across individual commits.

## Project

[`Day5Practice/`](Day5Practice/) — small practice project committed
through the full feature-branch workflow.

**Pull Request:** https://github.com/ShahdOmari/Shahd-Omari----BinX-Backend-Internship-/pull/1