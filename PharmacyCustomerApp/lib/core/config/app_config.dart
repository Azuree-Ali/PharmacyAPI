class AppConfig {
  const AppConfig._();

  static const apiBaseUrl = String.fromEnvironment('API_BASE_URL');

  static Uri get apiBaseUri {
    if (apiBaseUrl.isEmpty) {
      throw StateError(
        'Set API_BASE_URL with --dart-define=API_BASE_URL=https://your-api-host',
      );
    }

    final uri = Uri.tryParse(apiBaseUrl);
    if (uri == null || !uri.hasScheme || uri.host.isEmpty) {
      throw StateError('API_BASE_URL must be an absolute URL.');
    }
    return uri;
  }
}
