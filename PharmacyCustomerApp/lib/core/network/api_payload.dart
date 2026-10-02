import '../errors/app_exception.dart';

abstract final class ApiPayload {
  static dynamic unwrap(dynamic response) {
    if (response is Map && response.containsKey('data')) {
      final isSuccess = response['isSuccess'] ?? response['IsSuccess'];
      if (isSuccess == false) {
        final message = response['message'] ?? response['Message'];
        throw AppException(message is String ? message : 'The request failed.');
      }
      return response['data'] ?? response['Data'];
    }
    if (response is Map && response.containsKey('Data')) {
      final isSuccess = response['IsSuccess'];
      if (isSuccess == false) {
        final message = response['Message'];
        throw AppException(message is String ? message : 'The request failed.');
      }
      return response['Data'];
    }
    return response;
  }

  static Map<String, dynamic> asMap(dynamic value) {
    if (value is Map<String, dynamic>) return value;
    if (value is Map) return Map<String, dynamic>.from(value);
    throw const FormatException('Unexpected API response format.');
  }

  static List<dynamic> asList(dynamic value) {
    if (value is List) return value;
    throw const FormatException('Expected a list in the API response.');
  }
}
