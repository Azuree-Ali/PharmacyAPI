# Pharmacy Platform

This repository contains two separate applications that communicate over HTTP:

```text
PharmacyAPI/             ASP.NET Core API and SQL Server data layer
PharmacyCustomerApp/     Flutter customer mobile app
PharmacyAPI.slnx         .NET solution (API only)
```

The Flutter app is a sibling of the API project, not nested within it. The admin interface is planned as a separate web client.

## API

Open `PharmacyAPI/PharmacyAPI.csproj` or the solution file `PharmacyAPI.slnx` to work on the API. The current API design is documented in `PharmacyAPI/SYSTEM_DESIGN.md`.

## Customer mobile app

Open `PharmacyCustomerApp/` as its own Flutter project. It uses feature-oriented clean architecture: each feature separates presentation, domain, and data. Setup and API connection instructions are in `PharmacyCustomerApp/README.md`.

The app currently covers registration, sign-in, catalog browsing, cart, cash checkout, and customer orders. Online payment, notifications, and chat are later slices.
