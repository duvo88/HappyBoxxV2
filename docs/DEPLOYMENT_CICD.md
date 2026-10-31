# Backend CI/CD Deployment (Azure App Service)

This repository includes a backend deployment workflow:

- [.github/workflows/backend-deploy.yml](../.github/workflows/backend-deploy.yml)

It builds and deploys the 5 backend apps in this order:

1. Identity API
2. Catalog API, Inventory API, Billing API (in parallel)
3. Gateway
4. EF migrations (Identity, Catalog, Inventory, Billing)

## 1) Configure GitHub Actions OIDC authentication

Create one Microsoft Entra app registration for GitHub Actions deployment and configure a federated credential:

- **Issuer**: `https://token.actions.githubusercontent.com`
- **Audience**: `api://AzureADTokenExchange`
- **Subject**: `repo:danielvo-lsnsw/HappyBoxx:environment:production`

Grant the app registration (service principal) access to the Azure resource group that hosts the web apps (Contributor role is sufficient).

## 2) Add GitHub repository secrets

In GitHub repo settings, add:

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `AZURE_SQL_CONNECTION_STRING`

`AZURE_SQL_CONNECTION_STRING` is used by the migration job and should point to the shared Azure SQL database.

Example:

`Server=tcp:<sql-server>.database.windows.net,1433;Initial Catalog=HappyBoxx;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Service Principal;User ID=<client-id>;Password=<client-secret>;`

## 3) Add GitHub repository variables

In GitHub repo settings, add:

- `AZURE_WEBAPP_IDENTITY` (example: `happyboxx-identity-api`)
- `AZURE_WEBAPP_CATALOG` (example: `happyboxx-catalog-api`)
- `AZURE_WEBAPP_INVENTORY` (example: `happyboxx-inventory-api`)
- `AZURE_WEBAPP_BILLING` (example: `happyboxx-billing-api`)
- `AZURE_WEBAPP_GATEWAY` (example: `happyboxx-gateway`)

## 4) Create the `production` environment

In GitHub repo settings, create environment:

- `production`

Then scope the secrets/variables above to this environment if you want stricter control.

## 5) Run the deployment

Choose either:

- Push to `main` (for backend paths), or
- Manually run **Backend Deploy** via **Actions** → **Backend Deploy** → **Run workflow**.

## 6) Post-deploy

After backend deployment succeeds and migration job passes:

1. Validate health/startup logs for each app.
2. Verify gateway routes and auth flow end-to-end.
