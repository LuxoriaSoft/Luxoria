# Development at Luxoria

## Development guidelines
To contribute to this documentation, please follow the steps below:

### Gitflow workflow
- Use the Gitflow workflow to manage the development process.

### Branch naming
- Use the following naming convention for branches:
  - `feat/FEATURE_NAME`
  - `bugfix/BUG_NAME`
  - `hotfix/HOTFIX_NAME`

### Commit messages
- Use the following format for commit messages:
  - `feat: Add new feature`
  - `bugfix: Fix bug`
  - `hotfix: Fix critical bug`

### Pull requests
- Create a pull request for each feature, bugfix, or hotfix.
- Assign a reviewer to the pull request.
- Make sure the pull request passes all checks before merging.
- Target the `develop` branch for the pull request.

### Code reviews
- Review the code changes in the pull request.
- Provide feedback on the code changes.
- Approve the pull request if the code changes are acceptable.

### Merging
- Merge the pull request once it has been approved.
- Delete the branch after merging.

### Versioning
- Use semantic versioning for releases.
- Increment the version number based on the type of changes:
  - Major version for breaking changes
  - Minor version for new features
  - Patch version for bugfixes

### Documentation
- Update the documentation for each feature, bugfix, or hotfix.
- Make sure the documentation is up-to-date with the code changes.
- Use the `docs` folder for documentation files.
- Use the `README.md` file for the main documentation.
- Use the `DEVELOPMENT.md` file for development guidelines.
- Use the `CONTRIBUTING.md` file for contribution guidelines.
- Use the `CHANGELOG.md` file for version history.
- Use the `LICENSE.md` file for licensing information.
