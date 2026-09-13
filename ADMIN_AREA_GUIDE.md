# Admin area operations

## Scope

The content administration area is available only when the local ASP.NET Core application is running. It manages the service cards stored in the local SQLite database.

- Local address: `http://127.0.0.1:5010/admin/`
- Public static site: `https://justinfreres.github.io/biz/`

GitHub Pages is a static host. It does not run the SQLite database or expose the local administrator area.

## Sign-in and credential handling

There is no default username and no default password.

Before starting the local application, the administrator must set a strong, unique value for the `FLOWBRIDGE_ADMIN_PASSWORD` environment variable on the local machine. Sign in with that value at the local address above.

Never commit a password or other login secret to this repository, including in source code, configuration files, documentation examples, `.env` files, or the SQLite database. Keep the actual password in an approved password manager or another secure local secret store.

## Starting the local administrator

1. Set `FLOWBRIDGE_ADMIN_PASSWORD` locally using your secure credential-handling process.
2. Run `./scripts/Run-LocalAdmin.ps1` from the repository root.
3. Open `http://127.0.0.1:5010/admin/` and enter the locally configured password.

The first local startup creates the database and seeds the initial service options.

## Managing service content

The dashboard can add, edit, publish, unpublish, reorder, and delete service options. Each service includes:

- Practice label
- Title
- Summary
- Bullet points, one per line
- Intended audience
- Published status
- Sort order

Required text fields have length limits. Enter at least one bullet point, use no more than eight bullets, and keep sort order between 0 and 999. Unpublished services remain in the local database but are omitted from the local public service list.

Use **Sign out** when finished. Changes take effect in the local application immediately; they do not update the GitHub Pages site automatically.

## Data protection and recovery

The local database and cookie-protection keys live under `src/FlowBridge.Web/App_Data/`. This folder is ignored by Git and must remain local.

To back up content, stop the local application and copy `flowbridge.db` to a secure backup location. Do not attach database backups or credential files to pull requests.

The sign-in endpoint is rate limited to five attempts per five minutes. Authenticated changes use anti-forgery protection and an HTTP-only session cookie.
