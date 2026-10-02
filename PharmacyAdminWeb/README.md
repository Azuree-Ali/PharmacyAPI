# Pharmacy Admin Web

React dashboard for the existing Pharmacy API. This is a web-only admin surface; the Flutter application remains customer-only.

## Run locally

1. Start the API and its configured SQL Server database.
2. Install the web dependencies with `npm install`.
3. Run `npm run dev` and open `http://localhost:5173`.

For a different API address, set `PHARMACY_API_URL` before starting Vite (default: `http://localhost:5114`). Vite proxies `/api` to the API so browser CORS configuration is not required. Build with `npm run build`.

Sign in using an existing `SuperAdmin`, `Admin`, or `Pharmacist` account. Tokens are kept in tab-scoped `sessionStorage`; authorization is enforced by the API. Only privileged roles see management forms; the API remains authoritative for every operation. Never put signing keys or database credentials in this project.

## Connected modules

- Dashboard metrics and order status summary.
- Orders list, details, and status update. API business rules may reject invalid transitions.
- Products, categories, batches, customers, and users (including role assignment and account lock/unlock where allowed).
- Sales invoice list and details.
- Support chat list and conversation history, with replies through the existing authenticated SignalR hub.
- Sales invoice creation with a customer or order reference and a product batch line.

The API does not provide search pagination; list screens use the current list endpoints and client-side filtering. Invoice creation currently accepts one batch line per UI submission, matching the API's batch-based stock contract.
