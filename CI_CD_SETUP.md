# Development, Staging, and Production Delivery

## What is already configured

- **ASP.NET Core 10 / .NET 10.0.401** is the source platform. The production site is an ASP.NET Core project that serves the same static client assets locally and produces a static deployment package for free edge hosting.
- **`dev`** is the integration branch. Every push and pull request runs restore, build, tests, and static-site validation.
- **`stage`** is the release-candidate branch. A successful push deploys a Cloudflare Pages preview after a GitHub connection and secrets are added.
- **`main`** is production. A successful push runs the same verification sequence and deploys the production Pages site.
- GitHub Actions workflows, .NET dependency locks, health endpoint, test project, static-output checks, Dependabot updates, and release scripts are in the repository.

## Branch policy

| Branch | Purpose | How it changes | Deployment |
|---|---|---|---|
| `dev` | Day-to-day development | Feature branches merge here through pull requests | Verification only |
| `stage` | Release candidate | Pull request from `dev`; require passing verification | Cloudflare Pages preview |
| `main` | Production | Pull request from `stage`; require passing verification and manual approval | Cloudflare Pages production |

Never edit `main` directly. Protect all three branches in the Git host. Require at least one approval for `stage` and `main`, require the **Verify** status check, and dismiss stale approvals after new commits.

## One-time owner connection steps

These actions need the owner’s GitHub and Cloudflare credentials; they cannot be safely performed from a local checkout.

1. Create or choose a GitHub repository named `flowbridge-systems` and push this repository to it.
2. In GitHub, enable Actions and configure the branch protections above.
3. Create a free Cloudflare account and a Pages project named `flowbridge-systems`.
4. Create a Cloudflare API token limited to Pages edit access for that account. In the GitHub repository, save it as `CLOUDFLARE_API_TOKEN` and save the account identifier as `CLOUDFLARE_ACCOUNT_ID`.
5. Create GitHub Environments named `staging` and `production`; require your approval for production.
6. Push `dev`, then promote it to `stage`, then promote it to `main`. The workflow will publish the verified static output automatically.

## Why the deployed output is static

The site currently has no server-side data, authentication, uploads, or database. ASP.NET Core 10 is used for the application source, local serving, health endpoint, compilation, tests, and release output; Cloudflare Pages serves the verified static publish output at the edge. This provides a free production host without pretending a static business site needs an always-on paid .NET server.

If the site later needs client portals, Odoo APIs, authentication, or private data, move the ASP.NET Core runtime to a suitable server host and keep this branch/CI/CD process unchanged.
