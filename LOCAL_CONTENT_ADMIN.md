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

The first start creates the database and seeds the seven initial practices: K2/Nintex, Odoo, networking and infrastructure, Business Central AL extensions, cybersecurity/resilience, CompTIA A+ education, and Access & Office automation.

## Operating notes

- The password is never written to source code, configuration files, or SQLite.
- The sign-in route is rate-limited; authenticated edits use an anti-forgery token and a secure, HTTP-only session cookie. The cookie-protection keys live beside the database in the Git-excluded local app folder.
- To back up content, stop the local app and copy `flowbridge.db` to a secure backup location.
- The static public host cannot run a local SQLite database. Publish a static refresh for baseline content; use the local .NET app for live local content editing.

## Digital product and coloring-book series workflow

The first digital product is **FlowBridge Funny Tech Coloring Book: Cosmic Desk Mayhem (Series 1)**. It is a 20-page printable PDF (cover, 18 original coloring pages, and a final FlowBridge services guide) priced at **$5.99 USD** and fulfilled manually.

### Odoo CRM manual fulfillment

The Odoo CRM configuration includes these order tags:

- `Digital product`
- `Funny Tech Coloring Book`
- `Series 1`

It also includes the **Manual payment pending** pipeline stage. When a customer submits the website product form and sends the generated email, Odoo creates the request as an opportunity through the existing email gateway.

1. Open the opportunity, verify the requested product and delivery email, then apply the relevant product and series tags.
2. Move it to **Manual payment pending** and send the customer payment instructions outside the public site. Do not request or store card details in the website form.
3. After payment is confirmed, email the product PDF manually to the delivery email and move the opportunity to **Won**.
4. Keep the attached source PDF in a secure owner-controlled location; it is intentionally not published as a public website download.

### Adding a new series

1. In Odoo CRM, open **Configuration → Tags** and create a new tag such as `Series 2`.
2. Create the new original artwork and printable PDF, then give it a title, price, cover image, and delivery instructions.
3. Add a matching product card and order form to the public site, pointing to the existing Odoo CRM email gateway.
4. When the first order arrives, tag it with `Digital product`, the product name, and the new series tag; use the same manual-payment and fulfillment stage.

The current free Odoo CRM setup is intentionally a manual-order workflow. A native Odoo product catalog, sales orders, and automatic digital delivery would require adding Odoo Sales/eCommerce, which changes the one-app-free setup and should be evaluated before enabling it.
