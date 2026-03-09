# SAR Form Responses - GitHub Copilot Challenge Leaderboard

---

## 1. Describe what the system does and what is its purpose?

The GitHub Copilot Adoption Challenge Leaderboard is a non-production web application designed to gamify and track employee participation in an internal GitHub Copilot skills development challenge. The system serves as an engagement platform to encourage adoption of GitHub Copilot AI coding assistant across RAC's development teams.

**Core Functionality:**
- **Challenge Management:** Hosts 50+ coding challenges covering GitHub Copilot features including code generation, test creation, documentation, refactoring, and workspace (agent mode) capabilities. Each challenge includes instructional content, completion criteria, and point values.
- **Participant Registration:** Employees self-register with their work email, name, and optional GitHub/MS Learn profile handles to participate in challenges.
- **Team Competition:** Participants are organized into teams (e.g., by department, location, or project group) to foster collaborative learning and friendly competition. Team scores aggregate individual member achievements.
- **Progress Tracking:** Real-time leaderboard displays individual and team rankings based on challenge completion points. Participants can view their completed challenges, current rank, and team standing.
- **GitHub Integration:** Reads GitHub Copilot usage metrics via API to verify active Copilot usage and pull participant activity timestamps (e.g., last commit, PR creation). Does not access source code or repository contents.
- **MS Learn Integration (Optional):** Verifies completion of Microsoft Learn modules related to GitHub Copilot certification if participants opt in.
- **Administrative Controls:** Designated admins can create/edit challenges, manage teams, adjust scoring, and monitor participation metrics through an admin dashboard.

**Purpose:**
This system is a temporary deployment (approximately 4-8 weeks) to support RAC's GitHub Copilot adoption initiative led by Developer Advocacy. It aims to increase developer productivity by providing structured, hands-on learning experiences with immediate feedback and social recognition. The gamification approach using leaderboards and team competition has proven effective in previous internal training programs. Upon challenge conclusion, participant data will be deleted and the application decommissioned.

**Non-Production Context:**
This is explicitly NOT a production business system. It does not handle member/customer data, payment processing, policy administration, or any critical business operations. It is an internal learning and development tool with a limited lifespan.

---

## 3. Describe how the system is used?

**Access Method:**
The system is a web-based portal accessible through any modern web browser (Chrome, Edge, Firefox, Safari) by navigating to a RAC-internal URL (e.g., https://copilot-challenge.internal.rac.com.au or similar). The application is hosted on Azure App Service in RAC's Azure cloud subscription, not installed locally on user devices.

**Authentication Requirements:**
- **Mandatory Single Sign-On:** All access requires authentication via Microsoft Entra ID (Azure AD). Users must sign in with their RAC work credentials (@rac.com.au email and password). Multi-factor authentication (MFA) is enforced per RAC's existing Entra ID policies.
- **No Anonymous Access:** The application cannot be accessed without valid RAC employee authentication. There is no guest access, federated login from external organizations, or public-facing components.

**User Roles and Privileges:**
1. **Participant (Standard User):** Default role for all authenticated RAC employees. Can view challenges, mark challenges as complete, update their profile (name, GitHub handle, MS Learn ID), view leaderboards, and see team standings. Cannot modify other users' data, create challenges, or access admin functions.

2. **Administrator:** Assigned to Developer Advocacy team members (approximately 2-3 people). Can perform all participant actions plus: create/edit/delete challenges, manage teams, manually adjust participant scores, view analytics dashboards, sync GitHub team memberships, and export participation data. Admin privileges are granted through role-based access control in the application code (not Entra ID roles).

**Typical User Workflow:**
1. User navigates to application URL → redirected to Entra ID login if not authenticated
2. After successful authentication, user lands on dashboard showing leaderboard, their current rank, and team standing
3. User browses available challenges by category (e.g., "Code Generation", "Testing", "Refactoring")
4. User clicks on a challenge to view instructions and completion criteria
5. User attempts the challenge in their development environment (VS Code, Visual Studio, GitHub Codespaces)
6. User returns to application and marks challenge as complete (honor system + optional GitHub metric verification)
7. Points are awarded, leaderboard updates in real-time
8. Process repeats for additional challenges throughout the competition period

**Network Access:**
The application is deployed with Azure VNet integration and can optionally be restricted to RAC's internal network only (accessible via VPN or on-premises network). Alternatively, it can be internet-accessible but still requires Entra ID authentication. Final network posture will be determined during deployment configuration based on security team recommendations.

**Browser Requirements:**
Modern browser with JavaScript enabled. No special plugins, extensions, or client-side software required. Mobile-responsive design allows access from phones/tablets, though desktop is recommended for optimal experience.

---

## 4A. Describe where and how data is stored and transmitted to and from the system?

**Data Storage:**

**Primary Database - Azure SQL Database:**
- All participant data, challenge definitions, team assignments, and completion records are stored in a single-tenant Azure SQL Database hosted in the Australia East region (Sydney datacenter).
- Database uses Standard/Premium tier with Transparent Data Encryption (TDE) enabled by default, encrypting all data at rest using AES-256 encryption.
- Database is secured with private endpoint configuration, meaning it is NOT accessible from the public internet. Access is restricted to the application's VNet and specific authorized IP addresses (administrative access only).
- Connection strings contain username/password credentials stored in Azure Key Vault (see below), never hardcoded in application configuration files.

**Tables include:**
- `Participants` (ID, Name, Email, GitHub handle, team assignment, registration date)
- `Challenges` (ID, Title, Description/Content, Category, Point value, active status)
- `Completions` (Participant ID, Challenge ID, Completion timestamp)
- `Teams` (ID, Name, GitHub team ID)
- `Scores` (Calculated views/tables aggregating challenge completions)

**Secrets Storage - Azure Key Vault:**
- Sensitive configuration values are stored in Azure Key Vault in the same Australia East region:
  - GitHub Personal Access Token (PAT) for API calls
  - Microsoft Entra ID Client Secret for authentication
  - Azure SQL Database connection string (admin credentials)
  - SMTP credentials for email notifications (if implemented)
- The application accesses Key Vault using Managed Identity (no static credentials required for Key Vault access itself).
- Key Vault uses private endpoint, accessible only from the application's VNet.
- Secrets are NOT stored in application configuration files, environment variables, or source code repositories.

**Logging/Telemetry - Azure Application Insights:**
- Application logs, performance metrics, and error traces are sent to Azure Application Insights (Azure Monitor).
- Logs contain user actions (challenge completions, profile updates, admin operations) for audit and troubleshooting.
- PII is minimized in logs (email addresses logged, but no passwords, credit cards, or sensitive content).
- Retention follows RAC's standard monitoring data retention policies (typically 30-90 days).

**No Local Storage:**
- The application does not store data on user devices (beyond standard browser cookies for session management).
- No data is written to local file systems, shared drives, or external media.

---

**Data Transmission:**

**External API Calls (Outbound from RAC):**

1. **GitHub REST API (api.github.com):**
   - **Purpose:** Fetch Copilot usage metrics for RAC's GitHub organization, manage team memberships, read user activity timestamps.
   - **Protocol:** HTTPS (TLS 1.2+) RESTful API calls
   - **Authentication:** Bearer token using GitHub Personal Access Token (PAT) with limited scope:
     - `read:org` - Read organization data and teams
     - `admin:org` - Manage team memberships (add/remove users from challenge teams)
     - `copilot:read` - Read Copilot usage metrics (aggregated statistics only, no code access)
   - **Data Transmitted TO GitHub:** GitHub usernames (e.g., @employee-gh-handle) when creating teams or adding members. No RAC member data, no business-sensitive information.
   - **Data Received FROM GitHub:** Copilot usage statistics (number of suggestions, acceptances, languages used), user activity timestamps (last active date), team membership lists. GitHub API does NOT return source code, commit contents, or repository data.
   - **Frequency:** On-demand when admins sync teams or when leaderboard refreshes metrics (approximately hourly or on user request).

2. **Microsoft Learn API (learn.microsoft.com):**
   - **Purpose:** Verify completion of GitHub Copilot certification modules if participants provide their MS Learn profile ID.
   - **Protocol:** HTTPS RESTful API
   - **Authentication:** Public API using profile IDs (no credentials required for read-only access to public profile data)
   - **Data Transmitted:** MS Learn username/profile ID
   - **Data Received:** List of completed modules, certification status
   - **Frequency:** Optional, on-demand when user requests verification
   - **Note:** This integration is optional and may not be implemented in initial release.

3. **Microsoft Entra ID (Azure AD) - login.microsoftonline.com:**
   - **Purpose:** Authentication via OpenID Connect (OIDC) protocol
   - **Protocol:** HTTPS, OAuth 2.0/OIDC standard flow
   - **Authentication:** Client ID + Client Secret (stored in Key Vault)
   - **Data Transmitted TO Entra ID:** Authentication requests, redirect URIs
   - **Data Received FROM Entra ID:** ID tokens containing email, name, object ID (standard OIDC claims for authenticated user)
   - **Frequency:** Every user login session (tokens valid for 1 hour by default, then refresh)

**User Browser to Application (Inbound to RAC):**
- **Protocol:** HTTPS only (TLS 1.3/1.2), HTTP traffic automatically redirected to HTTPS
- **Data Transmitted:** User input (challenge submissions, profile updates, team assignments) sent as HTTPS POST/PUT requests
- **Authentication Cookies:** Encrypted session cookies issued after Entra ID authentication (HttpOnly, Secure, SameSite=Lax flags)
- **No Cleartext Transmission:** All data encrypted in transit using TLS. Application rejects non-HTTPS connections.

**Application to Database (Internal within Azure):**
- **Protocol:** TDS (Tabular Data Stream) over TLS, standard SQL Server protocol
- **Network Path:** Communication occurs over Azure private network, not public internet (private endpoint ensures traffic stays within Microsoft's backbone)
- **Authentication:** SQL authentication using credentials from Key Vault, or Azure AD authentication using Managed Identity
- **Data Transmitted:** SQL queries, result sets

**Application to Key Vault (Internal within Azure):**
- **Protocol:** HTTPS REST API
- **Network Path:** Private endpoint ensures communication stays within Azure VNet
- **Authentication:** Managed Identity (no static credentials)

**Data Retention and Disposal:**
- Database will be deleted 30 days after challenge completion (Azure resource group deletion)
- Soft-deleted secrets in Key Vault will be purged
- Application Insights logs retained per standard RAC policy, then auto-deleted
- No long-term archival of participant data

**No Data Sharing:**
- No participant data is sold, shared, or transmitted to third parties
- GitHub receives only GitHub usernames for team management (public information)
- Microsoft receives only authentication requests (standard Entra ID login telemetry)
- No integration with marketing platforms, analytics services, or external data warehouses

---

## 4B. Data Flow / Architectural Diagram

```
┌───────────────────────────────────────────────────────────────────────┐
│                       User's Browser (Employee)                       │
│                                                                       │
│  HTTPS (TLS 1.2+)                                                     │
│  - Authentication via Entra ID redirect                               │
│  - Encrypted session cookies                                          │
│  - Challenge submissions, profile updates                             │
└────────────────────────────────┬──────────────────────────────────────┘
                                 │
                                 │ HTTPS only
                                 │ (HTTP → HTTPS redirect)
                                 ▼
         ┌───────────────────────────────────────┐
         │   Azure Front Door (Optional)          │
         │   - DDoS protection                    │
         │   - IP allowlist (if internal-only)    │
         └───────────────┬───────────────────────┘
                         │
                         │ VNet Integration
                         ▼
┌────────────────────────────────────────────────────────────────────┐
│                      Azure App Service (Linux)                     │
│                        .NET 8.0 Web Application                    │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │         Authentication Middleware (Entra ID)              │    │
│  │  - OpenID Connect authentication                          │    │
│  │  - Token validation                                       │    │
│  │  - Session cookie issuance                                │    │
│  └──────────────────────────────────────────────────────────┘    │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────┐    │
│  │         Application Logic (ASP.NET Core MVC)              │    │
│  │  - Challenge management                                   │    │
│  │  - Leaderboard calculation                                │    │
│  │  - Team scoring                                           │    │
│  │  - Admin controls                                         │    │
│  └──────────────────────────────────────────────────────────┘    │
│                                                                    │
└───┬──────────────┬─────────────────┬─────────────────────────────┘
    │              │                 │
    │              │                 │ All connections use
    │              │                 │ private endpoints or HTTPS
    │              │                 │
    ▼              ▼                 ▼
┌─────────┐  ┌──────────┐    ┌──────────────────────┐
│  Azure  │  │  Azure   │    │  External APIs       │
│   SQL   │  │   Key    │    │  (Public Internet)   │
│Database │  │  Vault   │    │                      │
│         │  │          │    │  ┌────────────────┐  │
│ Private │  │ Private  │    │  │ GitHub REST    │  │
│Endpoint │  │Endpoint  │    │  │ api.github.com │  │
│         │  │          │    │  │                │  │
│Australia│  │Australia │    │  │ HTTPS/TLS 1.2+ │  │
│  East   │  │  East    │    │  │ Bearer Token   │  │
│         │  │          │    │  │ (PAT from KV)  │  │
│  TDE    │  │Secrets:  │    │  └────────────────┘  │
│Encrypted│  │- GH PAT  │    │                      │
│         │  │- ClientID│    │  ┌────────────────┐  │
│         │  │- DB Conn │    │  │ MS Learn API   │  │
│         │  │- SMTP    │    │  │ (Optional)     │  │
└─────────┘  └──────────┘    │  └────────────────┘  │
                              │                      │
                              │  ┌────────────────┐  │
                              │  │ Entra ID OIDC  │  │
                              │  │ login.ms...    │  │
                              │  └────────────────┘  │
                              └──────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│             Azure Monitor / Application Insights            │
│  - Application logs                                         │
│  - Performance metrics                                      │
│  - Error traces                                             │
│  - Audit logs (user actions)                                │
└─────────────────────────────────────────────────────────────┘

Legend:
───►  HTTPS/TLS encrypted traffic
═══►  Private endpoint (Azure internal network)
```

**Data Flow Steps:**

1. **User Authentication:**
   - User navigates to app URL → App redirects to login.microsoftonline.com
   - User enters RAC credentials → Entra ID validates → Returns ID token
   - App validates token, creates encrypted session cookie, grants access

2. **Challenge Browsing:**
   - User requests challenge list → App queries Azure SQL via private endpoint
   - Challenge data returned → Rendered in browser

3. **Challenge Completion:**
   - User submits completion → App writes to Azure SQL (Completions table)
   - Leaderboard recalculated → Updated scores displayed

4. **GitHub Metrics Sync (Admin Action):**
   - Admin clicks "Sync GitHub Teams" → App retrieves PAT from Key Vault
   - App sends HTTPS request to api.github.com with Bearer token
   - GitHub returns metrics → App updates database → Logs action to App Insights

5. **Secrets Access:**
   - App needs database connection → Requests from Key Vault using Managed Identity
   - Key Vault validates identity → Returns secret → App connects to database

---

## 10B. Please describe the types of records which will be stored in this system / solution.

**Employee Records (Participants):**
- Employee first name, last name, work email address (@rac.com.au)
- Optional: preferred nickname/display name for leaderboard
- Optional: public GitHub username (e.g., @john-smith-gh)
- Optional: public Microsoft Learn profile ID
- Team assignment (which challenge team the employee joined)
- Registration date/timestamp
- Last login timestamp
- **Estimated Volume:** 50-150 records (one per participating employee)

**Challenge Activity Records:**
- Challenge completion events (which participant completed which challenge and when)
- Timestamps of completion
- Point values awarded
- **Estimated Volume:** 2,500-7,500 records (50 challenges × 50-150 participants)

**Challenge Definition Records:**
- Challenge title, description content (markdown text with instructions)
- Challenge category (e.g., "Code Generation", "Testing", "Workspace")
- Point value, difficulty level
- Active/inactive status
- Creation and modification timestamps
- **Estimated Volume:** 50-60 records (fixed set of challenges)

**Team Records:**
- Team name (e.g., "Web Dev Squad", "Backend Team", "CloudOps")
- Team description
- GitHub organization team ID (if synced with GitHub teams)
- **Estimated Volume:** 5-15 records (teams organized by department/project)

**Score/Leaderboard Records:**
- Calculated aggregations of challenge completions
- Individual scores (total points per participant)
- Team scores (sum of team member points)
- Rankings (position in leaderboard)
- **Estimated Volume:** Derived from above data, no separate storage beyond cache

**GitHub Copilot Usage Metrics (Cached):**
- Aggregated statistics only: number of suggestions, acceptance rate, languages used
- User activity timestamps (last GitHub commit date, last PR creation)
- NO source code, NO repository contents, NO commit messages, NO file paths
- **Estimated Volume:** Minimal, fetched on-demand from GitHub API, not permanently stored

**Audit Logs (Application Insights):**
- User login events (email, timestamp, IP address)
- Challenge completion actions (who completed what, when)
- Admin actions (challenge creation, team management, score adjustments)
- Error logs and performance traces
- **Estimated Volume:** 10,000-50,000 log entries over challenge duration
- **Retention:** Per RAC standard (typically 30-90 days), then auto-deleted

**Records NOT Stored:**
- No member or customer PII
- No credit card or payment information
- No employee HR data (salary, performance reviews, medical, etc.)
- No network topology or infrastructure configurations
- No production system credentials or API keys (except GitHub PAT in Key Vault, not in database)
- No source code or intellectual property
- No incident management data

**Record Categories Summary:**
- ✅ Employee records (basic work info only)
- ✅ Analytics / Logging (application usage, audit trail)
- ❌ Member / customer records
- ❌ Vehicle records
- ❌ Credit card
- ❌ Network topology
- ❌ Incidents

---

## 15B. Details on interfaces / API of the solution that will be used by the system / solution

**Outbound API Calls (From Leaderboard App to External Systems):**

**1. GitHub REST API (api.github.com)**

**Purpose:** Read GitHub Copilot usage metrics and manage challenge team memberships

**API Endpoints Used:**
- `GET /orgs/{org}/copilot/usage` - Retrieve organizational Copilot usage statistics (aggregated metrics: suggestions, acceptances, languages)
- `GET /orgs/{org}/teams` - List existing teams in RAC's GitHub organization
- `POST /orgs/{org}/teams` - Create new team for challenge (e.g., "Copilot Challenge - Web Team")
- `PUT /orgs/{org}/teams/{team}/memberships/{username}` - Add participant to team
- `DELETE /orgs/{org}/teams/{team}/memberships/{username}` - Remove participant from team
- `GET /users/{username}/events` - Fetch user activity timeline (last commit, PR timestamps)

**Authentication Method:**
- GitHub Personal Access Token (PAT) sent as `Authorization: Bearer <token>` header
- Token stored in Azure Key Vault, retrieved via Managed Identity
- PAT has limited scope permissions:
  - `read:org` - Required to read organization data and teams
  - `admin:org` - Required to manage team memberships (add/remove users)
  - `copilot:read` - Required to access Copilot usage metrics
- **PAT CANNOT access:** Repository contents, source code, commit data, pull request code, organization billing, or administrative settings

**Data Transmitted TO GitHub:**
- RAC GitHub organization name (e.g., "rac-wa-org")
- GitHub usernames of participants (public information, e.g., "@john-smith")
- Team names for challenge teams

**Data Received FROM GitHub:**
- Copilot usage statistics (numbers only: suggestion count, acceptance rate, active days)
- Team membership lists (GitHub usernames)
- User activity timestamps (last active date, last event timestamp)
- NO source code, NO file contents, NO commit messages, NO repository data

**Protocol:** HTTPS (TLS 1.2+), JSON responses

**Frequency:** 
- Usage metrics: Hourly or on-demand (admin refresh)
- Team management: On-demand when admin creates teams or adds participants

**Error Handling:** If GitHub API unavailable, team sync features disabled but core leaderboard functionality continues (metrics fetching is non-blocking)

**Rate Limiting:** GitHub API rate limits (5,000 requests/hour for authenticated apps). App implements caching to stay well below limits.

---

**2. Microsoft Learn API (learn.microsoft.com/api) - OPTIONAL**

**Purpose:** Verify completion of GitHub Copilot certification modules for bonus points

**API Endpoints Used:**
- `GET /users/{profile-id}/achievements` - Fetch completed modules and certifications

**Authentication Method:**
- Public API using user-provided MS Learn profile ID (no authentication token required for public profile data)
- Rate limited by IP address

**Data Transmitted:**
- MS Learn profile ID (public username, e.g., "john-smith-123456")

**Data Received:**
- List of completed Microsoft Learn modules
- Certification status (e.g., "GitHub Copilot Fundamentals" completed)
- Completion dates

**Protocol:** HTTPS (TLS 1.2+), JSON responses

**Frequency:** On-demand when participant requests verification (typically once per user)

**Note:** This integration may not be implemented in initial release. If omitted, admin will manually verify certifications.

---

**3. Microsoft Entra ID / Azure AD (login.microsoftonline.com)**

**Purpose:** User authentication via OpenID Connect (OIDC)

**API Endpoints Used:**
- `GET /.well-known/openid-configuration` - Discover OIDC endpoints
- `POST /oauth2/v2.0/token` - Exchange authorization code for ID token
- `GET /oauth2/v2.0/authorize` - Redirect user for login

**Authentication Method:**
- OAuth 2.0 / OpenID Connect standard flow
- Client ID (app registration ID) + Client Secret (stored in Key Vault)
- Redirect URIs configured in Entra ID app registration

**Data Transmitted TO Entra ID:**
- Client ID, redirect URI, requested scopes (openid, profile, email)

**Data Received FROM Entra ID:**
- ID Token (JWT) containing:
  - User email (e.g., john.smith@rac.com.au)
  - Display name
  - Object ID (Entra ID user GUID)
  - Token expiration timestamp
- NO admin roles, NO group memberships beyond standard claims

**Protocol:** HTTPS (TLS 1.2+), OAuth 2.0 standard

**Frequency:** Every user login session (tokens expire after 1 hour, refresh as needed)

**Permissions Required:** 
- `User.Read` (delegated) - Read signed-in user's profile. This is the ONLY Microsoft Graph permission requested.
- No admin consent required
- No access to other users' data, mailboxes, calendars, or OneDrive

---

**Inbound API / Public Endpoints (Exposed by Leaderboard App):**

**NOTE:** The application exposes standard web endpoints (HTML pages + REST API), NOT intended for consumption by external systems. These are for internal browser-based access only.

**Web Pages (HTML served to browsers):**
- `GET /` - Home page / leaderboard
- `GET /challenges` - Browse challenges
- `GET /challenges/{id}` - View challenge details
- `GET /profile` - User profile management
- `GET /teams` - Team standings
- `GET /admin/dashboard` - Admin control panel (requires admin role)

**REST API Endpoints (JSON, for AJAX calls from browser):**
- `POST /api/participants` - Register new participant
- `PUT /api/participants/{id}` - Update participant profile
- `POST /api/completions` - Mark challenge as complete
- `GET /api/leaderboard` - Fetch current rankings (JSON)
- `GET /api/teams/{id}/score` - Get team score
- `POST /api/admin/challenges` - Create challenge (admin only)
- `POST /api/admin/teams/sync-github` - Sync with GitHub teams (admin only)

**Authentication for API:** All endpoints require Entra ID authentication cookie. No public/anonymous API access. No API keys issued to external systems.

**No Third-Party Integrations:** The application does NOT expose webhooks, does NOT publish events to message queues, and does NOT integrate with other RAC systems (HR, CRM, ERP, etc.). It is a standalone application.

---

## Additional Information

**Deployment Timeline:**
- **Pre-Deployment (2 days):** Remediation of missing `[Authorize]` attributes on API controllers (SEC-001 security finding). This is a blocking security issue that must be fixed before production deployment.
- **Deployment:** Infrastructure provisioned via Azure Developer CLI (azd) and Bicep templates. Automated deployment from GitHub repository.
- **Challenge Duration:** Approximately 4-8 weeks (exact dates TBD by Developer Advocacy team)
- **Decommissioning:** Application and all data deleted 30 days after challenge end date

**Temporary Nature:**
This is explicitly a short-term, non-production system. It is NOT intended for long-term operational use. After the challenge concludes, the Azure resource group will be deleted, removing all infrastructure, database, and stored data. Participants will be notified of the decommissioning date in advance per GDPR transparency requirements.

**Known Security Findings (Pre-Deployment):**
1. **HIGH Priority (Blocking):** Missing authorization checks on API controllers. Remediation in progress (2 business days). Deployment will not proceed until fixed and tested.
2. **MEDIUM Priority (Accepted for now):** XSS potential in challenge content views. Requires database compromise to exploit (admin credentials). Mitigation: Content is admin-controlled, HTML sanitization library to be added post-launch.
3. **MEDIUM Priority (Accepted for now):** No rate limiting on API endpoints. Risk mitigated by internal-only access and Entra ID authentication requirement. Post-launch enhancement planned.

**Positive Security Assurances:**
- All secrets (GitHub PAT, Client Secret, database credentials) stored in Azure Key Vault, never in source code or configuration files
- GitHub PAT has minimal scope (read metrics + manage teams only), cannot access repositories or source code
- Database and Key Vault use private endpoints (not accessible from public internet)
- Transparent Data Encryption (TDE) enabled on Azure SQL
- Comprehensive audit logging to Application Insights
- Entra ID SSO with MFA enforcement per organizational policy
- HTTPS-only traffic enforced (TLS 1.2+)

**Data Privacy:**
- Privacy statement will be displayed on registration page explaining data collection, purpose, retention, and deletion process
- Participants can request data deletion by contacting admin (manual process, or self-service profile deletion if implemented)
- No data shared with third parties (GitHub receives only public usernames for team management)
- GDPR-compliant (employee data minimization, right to access, right to erasure)

**Business Justification:**
This system supports RAC's AI adoption strategy and developer productivity initiatives. GitHub Copilot enterprise licenses have been purchased, and this challenge aims to maximize ROI through structured onboarding and engagement. Previous gamification programs (e.g., Azure certification challenges) showed 3x higher completion rates vs. passive learning materials.

**Success Metrics:**
- Challenge participation rate (target: 40%+ of eligible developers)
- Average challenges completed per participant (target: 25+)
- GitHub Copilot active usage growth (measured via GitHub API metrics)
- Post-challenge survey: developer satisfaction and productivity self-assessment

**Alternatives Considered:**
- **Manual tracking (Excel spreadsheet):** Rejected due to high admin overhead and poor user experience
- **Third-party gamification platform (e.g., Badgeville):** Rejected due to cost, data residency concerns, and lack of GitHub/Entra ID integration
- **Extend existing LMS (Learning Management System):** Rejected due to LMS complexity and inability to integrate with GitHub metrics

**Contact Information:**
- **Business Owner:** [Developer Advocacy & Experience Lead name]
- **Technical Lead:** [Developer/Engineer name]
- **Security Reviewer:** [InfoSec team contact]

**Post-Launch Monitoring:**
- Daily log review for first 48 hours
- Weekly participation metrics
- Security alert monitoring via Application Insights
- Incident response plan documented (disable app via Azure Portal, rotate secrets if compromised)
