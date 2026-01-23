# Development at Luxoria

## Development Guidelines

To contribute to this project, please follow the guidelines below to ensure consistency and quality in our development process.

### Gitflow Workflow
We follow the Gitflow branching model for managing development. This involves using separate branches for features, bugfixes, hotfixes, and releases.

1. **Main Branches**:
   - `main`: Contains the production-ready code.
   - `develop`: Used as an integration branch for features, bugfixes, and hotfixes before releases.

2. **Supporting Branches**:
   - `feat/*`: For new features.
   - `bugfix/*`: For resolving bugs found during development.
   - `hotfix/*`: For urgent fixes in the production code.

### Branch Naming Convention
Use the following prefixes for your branch names to maintain consistency:
- `feat/FEATURE_NAME`: For new features.
- `bugfix/BUG_NAME`: For bug fixes during development.
- `hotfix/HOTFIX_NAME`: For critical fixes applied directly to `main`.

### Commit Messages
Use the following structure for writing clear, informative commit messages:
- **Format**: `<type>: <description>`
- **Types**:
  - `feat`: For new features (e.g., `feat: Add image editing tool`).
  - `bugfix`: For bug fixes (e.g., `bugfix: Fix alignment issue`).
  - `hotfix`: For critical bug fixes (e.g., `hotfix: Fix crash on startup`).

Add a brief description after the type, and make sure your message is concise and descriptive.

### Pull Requests
Each feature, bugfix, or hotfix should be accompanied by a pull request (PR). The steps are:
1. Open a PR targeting the `develop` branch.
2. Assign a reviewer for code review.
3. Ensure the PR passes all automated checks and tests.
4. Provide a detailed description of what was changed and why.

### Code Reviews
Every PR must undergo a code review before merging:
- Reviewers should carefully check the changes for quality, readability, and correctness.
- Provide constructive feedback.
- Approve the PR if it meets the necessary standards.

### Merging
Once your pull request has been reviewed and approved:
1. **Ensure all checks pass** before merging.
2. Merge into `develop` (or `main` in the case of hotfixes).
3. **Delete the branch** after merging to keep the repository clean.

### Versioning
We use **Semantic Versioning** for releases:
- **Major**: Incompatible API changes or large-scale changes.
- **Minor**: New features or functionality.
- **Patch**: Bug fixes or minor updates.

Follow this versioning structure when preparing release branches.

### Documentation
Make sure the documentation is updated for each new feature, bugfix, or hotfix:
- Update **`docs`** for in-depth documentation.
- Update **`README.md`** for general project information.
- Use **`DEVELOPMENT.md`** for developer guidelines.
- Use **`CONTRIBUTING.md`** for contribution guidelines.
- Update **`CHANGELOG.md`** with the history of changes.
- Keep the **`LICENSE.md`** up-to-date with any changes in licensing.

### Testing Policy
All code changes must adhere to the **[Luxoria Unit Testing Policy](./desktop/TESTING-POLICY.md)** to ensure high coverage, quality, and maintainability of the codebase.

### Additional Guidelines
- **Testing**: Ensure all code changes are properly tested.
- **Code Style**: Follow project-specific coding standards to maintain uniformity.

By following these guidelines, you help maintain a smooth and efficient development process at Luxoria.
