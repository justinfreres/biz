# Development, Staging, and Production Delivery

## What is already configured

- **ASP.NET Core 10 / .NET 10.0.401** is the source platform. The production site is an ASP.NET Core project that serves the same static client assets locally and produces a static deployment package for free edge hosting.
- **`dev`** is the integration branch. Every push and pull request runs restore, build, tests, and static-site validation.
- **`stage`** is the release-candidate branch. A successful push runs verification and creates a Cloudflare Pages preview when the optional Cloudflare secrets are present.
- **`main`** is production. A successful push runs the same verification sequence and publishes the verified static site to free GitHub Pages.
- GitHub Actions workflows, .NET dependency locks, health endpoint, test project, static-output checks, Dependabot updates, and release scripts are in the repository.

## Branch policy

| Branch | Purpose | How it changes | Deployment |
|---|---|---|---|
| `dev` | Day-to-day development | Feature branches merge here through pull requests | Verification only |
| `stage` | Release candidate | Pull request from `dev`; require passing verification | Optional Cloudflare Pages preview |
| `main` | Production | Pull request from `stage`; require passing verification and manual approval | GitHub Pages production |

Never edit `main` directly. Protect all three branches in the Git host. Require at least one approval for `stage` and `main`, require the **Verify** status check, and dismiss stale approvals after new commits.

## One-time owner connection steps

The GitHub repository, `dev`, `stage`, and `main` branches, Actions workflows, and free GitHub Pages delivery are already configured. GitHub Actions will create the Pages deployment on the first successful production run.

1. In GitHub, enable Actions if the account has globally disabled it, then configure the branch protections above.
2. Create GitHub Environments named `staging` and `production`; require your approval for production.
3. Optionally create a free Cloudflare account and a Pages project named `flowbridge-systems` for a staging preview and alternate edge host.
4. For that optional Cloudflare deployment, save a Pages-edit API token as `CLOUDFLARE_API_TOKEN` and its account identifier as `CLOUDFLARE_ACCOUNT_ID` in the GitHub repository secrets. Until then, the Cloudflare workflow reports a configuration notice and finishes successfully.
5. Promote work through `dev`, `stage`, and `main`. The main workflow publishes the verified static output to GitHub Pages automatically.

## Why the deployed output is static

The public site uses a verified static package. ASP.NET Core 10 is used for the application source, local serving, health endpoint, compilation, tests, local SQLite-backed content administration, and release output; GitHub Pages serves the verified static publish output for free. The local content database is intentionally not published to a static host.

If the site later needs client portals, Odoo APIs, authentication, or private data, move the ASP.NET Core runtime to a suitable server host and keep this branch/CI/CD process unchanged.

