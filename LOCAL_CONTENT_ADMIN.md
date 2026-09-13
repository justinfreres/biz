# Local content administration

The local ASP.NET Core app stores editable public service content in SQLite at:

`src/FlowBridge.Web/App_Data/flowbridge.db`

That database is intentionally excluded from Git. The public, free static site keeps a safe authored fallback for visitors. When the .NET app runs locally, its public service cards load from SQLite and `/admin/` lets the owner manage them.

## First-time use

1. In PowerShell, choose a strong, unique password for this local session:

   ```powershell
   $env:FLOWBRIDGE_ADMIN_PASSWORD = "replace-with-a-long-unique-password"
   ```

2. Start the app:

   ```powershell
   .\scripts\Run-LocalAdmin.ps1
   ```

3. Open [http://127.0.0.1:5010/admin/](http://127.0.0.1:5010/admin/) and sign in with that password.

The first start creates the database and seeds the six initial practices: K2/Nintex, Odoo, networking and infrastructure, Business Central AL extensions, cybersecurity/resilience, and CompTIA A+ education.

## Operating notes

- The password is never written to source code, configuration files, or SQLite.
- The sign-in route is rate-limited; authenticated edits use an anti-forgery token and a secure, HTTP-only session cookie. The cookie-protection keys live beside the database in the Git-excluded local app folder.
- To back up content, stop the local app and copy `flowbridge.db` to a secure backup location.
- The static public host cannot run a local SQLite database. Publish a static refresh for baseline content; use the local .NET app for live local content editing.

