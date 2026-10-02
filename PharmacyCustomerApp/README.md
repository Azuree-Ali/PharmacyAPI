# Pharmacy Customer Flutter App

This is a standalone Flutter mobile project beside the ASP.NET API project:

```text
PharmacyAPI/             ASP.NET Core API
PharmacyCustomerApp/     Flutter customer mobile app
```

## Architecture

Feature-oriented clean architecture keeps UI, business contracts, and API/storage implementation independent:

```text
lib/
  core/                  API config/client, errors, token storage, theme
  features/
    auth/                registration, sign-in, session
    catalog/             customer home, products, product details
    cart/                server-backed cart and cart actions
    orders/              cash checkout, order history, details/actions
  shared/                customer navigation and reusable widgets
```

Riverpod provides dependency injection and async UI state. Dio sends REST requests. `flutter_secure_storage` keeps the JWT out of ordinary app preferences.

## Current app slice

Register / sign in → home → product catalog → product details → cart → cash checkout → order list/details. Online payment, profile, notifications, and chat remain future slices.

## Flutter setup

Install Flutter for your platform and make sure `flutter` is available in your terminal. From this directory run:

```powershell
flutter pub get
flutter analyze
flutter run --dart-define=API_BASE_URL=https://YOUR-API-HOST
```

For a USB-connected Android phone with the API running on this computer, forward the API port and launch against the local development server:

```powershell
adb reverse tcp:5114 tcp:5114
flutter run -d YOUR-DEVICE-ID --dart-define=API_BASE_URL=http://127.0.0.1:5114
```

The Android debug manifest permits cleartext traffic for this local HTTP development connection. Release builds do not enable that setting.

The Android and iOS runner folders have been generated. Android deployment also needs Android SDK tooling and a device/emulator. Building iOS apps requires macOS and Xcode.

Use an API host reachable from the target device or emulator, with HTTPS trusted by that device. Android emulators normally reach the development host through `10.0.2.2`; iOS simulators can generally use `localhost`. Do not disable certificate validation in the app.

## Current API constraints

The API requires a customer JWT for Home, Product, Cart, Orders, and Checkout routes. Checkout currently supports cash only. Product responses do not include an image URL, so the UI uses a neutral placeholder. Registration and resend-confirmation email links now target the API's `AuthController/ConfirmEmail` route.
