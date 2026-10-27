# Git workflow

How Team D works on the code in parallel.

## Rules

- `main` is the stable branch. It always builds. Nobody pushes to it directly.
- **One branch per Jira story**, named `feature/CD-<number>-<short-name>`,
  for example `feature/CD-5-cd-command`.
- Changes reach `main` only through a **pull request** that **one teammate**
  reviews and approves.
- **CD-14 (foundation) is built first.** The other stories start from `main`
  after CD-14 is merged, because every command depends on the shell, the parser
  and the factory.
- Keep a pull request small: one story, one purpose.

## Step by step

```
git switch main
git pull
git switch -c feature/CD-5-cd-command

# ... work, then commit in small steps ...
git add .
git commit -m "CD-5: add CD command"

git push -u origin feature/CD-5-cd-command
```

Then on GitHub:

1. Open a pull request from your branch into `main`.
2. Put the Jira key in the title, for example `CD-5: CD command`.
3. Ask one teammate to review it.
4. After approval, merge it and delete the branch.
5. Update the story status in Jira.

Before you start the next story, run `git switch main` and `git pull` again.

## Unit tests

Write the unit tests of your story in the same pull request, see
[testing.md](testing.md). `dotnet test` must pass before you open the pull request,
and the reviewer checks that every acceptance criterion has a test.

## Commit messages

Start with the Jira key, then a short description in the imperative:

```
CD-2: list directories and files in DIR
CD-1: end the shell on EXIT
```

## Merge conflicts

Every command has its own file, so conflicts should be rare. The place where two
stories can touch the same lines is `FileSystemService` and `ConsoleService`, where
every story implements its own methods. Pull often, keep your change to your own
methods, and resolve conflicts together if two stories touch the same lines.

## Line endings

Windows and macOS use different line endings. The repository contains a
`.gitattributes` file that handles this, so there is nothing to configure on your
machine.

## Optional: protect `main` on GitHub

In the repository settings under Branches you can require a pull request and one
approval before merging, if your GitHub plan offers it for private repositories.
If it does not, the rule above is a team agreement.
