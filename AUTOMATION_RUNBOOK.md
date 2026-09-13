# FlowBridge Systems — Automation Runbook

## Goal

Operate the consulting firm from one source of truth: **Odoo**. Use its CRM, Project, Helpdesk, Timesheets, Sales, Invoicing, Documents, and Accounting capabilities as the operating spine. Keep the system simple until paid work proves a need for more tools.

This runbook specifies the desired automations; it does not connect to an Odoo instance because that requires your own tenant, admin account, users, payment setup, and final security decisions.

## Before configuration

1. Create a separate Odoo company for FlowBridge Systems after the legal name and tax treatment are confirmed.
2. Use a sandbox or duplicate database for automation testing. Do not test mail, invoices, or scheduled actions against real clients.
3. Create a shared business mailbox and use it only after the domain is finalized.
4. Enable only the apps needed for the first 90 days: CRM, Sales, Project, Timesheets, Invoicing/Accounting, Documents, and Helpdesk.
5. Define a data owner. For launch, that is the founder; no automation may silently change a commercial record without an audit trail.

## Core records

| Odoo area | Record | Required fields | Purpose |
|---|---|---|---|
| CRM | Opportunity | Organization, contact, platform, problem, source, next action | Controls the sales funnel |
| Sales | Quotation | Offer, scope assumptions, price, expiration, acceptance criterion | Controls commercial commitment |
| Project | Client project | Client, SOW, budget, owner, status, target date | Controls delivery |
| Helpdesk | Ticket | Client, severity, component, requester, SLA, related project | Controls support requests |
| Timesheets | Entry | Client/project, task, billable flag, work note | Supports billing and capacity |
| Accounting | Invoice | Client, product/retainer, due date, payment terms | Controls cash collection |
| Documents | Engagement folder | Signed documents, audit deliverables, release notes, access record | Keeps evidence together |

### CRM pipeline

`New lead → Qualified → Fit call booked → Audit proposed → Audit sold → Audit delivered → Sprint proposed → Active client → Retainer → Closed / not now`

Create lost reasons: **Not in scope**, **No budget**, **Timing**, **Vendor route**, **No response**, and **No decision owner**. These make pipeline reviews useful.

## Automation recipes

Configure these using Odoo automated actions/server actions, activity rules, email templates, and scheduled actions available in your licensed version. Field labels differ by Odoo release; preserve the business behavior rather than copying labels blindly.

### A. New inquiry acknowledgement

**Trigger:** New CRM opportunity created from website form, email alias, LinkedIn message entered manually, or referral.  
**Actions:**

1. Set stage to `New lead`.
2. Create a same-day activity: “Review inquiry and decide if it is in scope.”
3. Send a short acknowledgement only when the person submitted a web form or business email; do not automate messages to LinkedIn connections.
4. Assign lead source and platform: `K2`, `Nintex`, `Odoo`, or `Other`.
5. If untouched after one business day, notify the founder.

**Safety:** Never promise availability, pricing, or a migration outcome in the automatic message.

### B. Qualified-lead follow-up

**Trigger:** Opportunity moved to `Qualified`.  
**Actions:**

1. Create a task due within two business days: “Offer fit call times.”
2. Attach the fit-call agenda from `CLIENT_ASSETS.md`.
3. Add the opportunity to the weekly pipeline review activity.

**Trigger:** No client response after five business days.  
**Action:** Draft or schedule one value-based follow-up. Mark `No response` only after the second unanswered follow-up.

### C. Paid-audit conversion

**Trigger:** Opportunity moved to `Audit proposed`.  
**Actions:**

1. Generate a quotation from the `Stability Audit` service product.
2. Apply the audit scope, exclusions, payment-in-advance term, and 10-hour target from the SOW template.
3. Create a follow-up activity for two business days before quote expiration.

**Trigger:** Quotation accepted and payment confirmed.  
**Actions:**

1. Move opportunity to `Audit sold`.
2. Create a client project from the `Stability Audit` template.
3. Create tasks: kickoff, access request, stakeholder interview, architecture review, risk register, readout, and next-step proposal.
4. Create a client document folder with `01 Commercial`, `02 Discovery`, `03 Deliverables`, and `04 Release Evidence`.
5. Send the welcome message and kickoff scheduling request.

### D. Project health

**Trigger:** Every Friday at 2:00 p.m. local time, for active projects.  
**Actions:**

1. Create an internal activity: update progress, risks, client decisions needed, next milestone, and budget consumed.
2. If a project has no logged time or update in seven days, flag it `Watch` for review.
3. Prepare, but do not automatically send, a client status update. The founder reviews it before it goes out.

**Why review is required:** A client-facing project status is a commercial communication; automation may collect the facts, but a person should approve the wording.

### E. Support ticket routing

**Trigger:** Helpdesk ticket received.  
**Actions:**

1. Acknowledge receipt with stated business-hour response expectation.
2. Use platform plus severity to apply SLA: `Critical`, `High`, `Normal`, or `Planned`.
3. Create a related project task when the ticket needs more than 30 minutes, code, a release, or change approval.
4. Escalate Critical tickets to a founder activity immediately; do not guarantee 24/7 coverage unless a specific contract provides it.
5. On closure, require a resolution note and whether a knowledge-base article or preventative task is needed.

### F. Retainer invoicing and capacity

**Trigger:** First day of each retainer month.  
**Actions:**

1. Create and send the advance invoice from the retainer subscription/product.
2. Create the month’s reserved-capacity task budget (10 or 20 hours).
3. At 70% consumed, notify the founder to review scope with the client.
4. At 90% consumed, create a renewal/overage decision activity; do not bill overage automatically without contract terms and client approval.

### G. Collections hygiene

**Trigger:** Invoice reaches due date.  
**Actions:**

1. Send a friendly payment reminder from the finance mailbox.
2. Three business days later, create a founder follow-up activity.
3. Seven business days later, flag the client `Payment watch` and pause new non-emergency work only in accordance with the signed agreement.

## Templates to configure as service products

| Product | Sale type | Invoice policy | Default price | Notes |
|---|---|---|---:|---|
| Stability Audit | Service | Ordered quantity / advance | $1,500 | Fixed 10-hour target; no production changes in base scope |
| Recovery Sprint | Service | Milestone | From $4,500 | Quote each acceptance criterion separately |
| Core Continuity Retainer | Service | Prepaid monthly | $1,800 | 10 hours, 3-month initial term |
| Priority Continuity Retainer | Service | Prepaid monthly | $3,400 | 20 hours, agreed priority response |
| Standard consulting | Service | Delivered quantity | $150/hour | 8-hour minimum per approved work item |
| Urgent response | Service | Delivered quantity | $195/hour | Availability and scope subject to contract |

Validate pricing, taxes, payment terms, and billing policy with your accountant before enabling invoices.

## Weekly operating cadence

| When | Review | Decision |
|---|---|---|
| Monday, 20 minutes | Pipeline and promised follow-ups | What must move this week? |
| Daily, 10 minutes | New leads, support tickets, approvals | What risks a client promise? |
| Friday, 30 minutes | Project health, timesheets, invoices due | What needs executive attention? |
| Month end, 45 minutes | Revenue, collections, capacity, retainer renewal | What should change next month? |

## Test protocol before going live

1. Create a dummy lead; confirm the owner activity and acknowledgment behavior.
2. Move it through each CRM stage; confirm tasks appear once, not repeatedly.
3. Accept a test audit quote; confirm project, folder, and kickoff tasks are created.
4. Create a test helpdesk ticket at each severity; confirm correct SLA and escalation.
5. Generate a test retainer invoice; verify company identity, tax setup, payment terms, and email recipient.
6. Record and reverse a dummy payment; make sure reminders stop correctly.
7. Review access rights: only authorized staff can see client documents, invoices, and credentials.
8. Disable or correct any action that sends unintended emails, invoices, or client notifications.

## External connections to add only after the Odoo core works

- Website form → CRM opportunity
- Calendar booking → CRM opportunity/activity
- Accounting bank feed → reconciliation workflow
- E-signature → signed SOW stored in Documents
- Cloud file storage → client deliverable archive

Use service accounts, least-privilege access, audit logs, and a written owner for each connection. No automation should copy client credentials or sensitive production data into an unapproved tool.
