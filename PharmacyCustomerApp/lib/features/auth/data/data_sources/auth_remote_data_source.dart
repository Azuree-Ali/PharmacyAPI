import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_payload.dart';
import '../models/customer_session_model.dart';

class AuthRemoteDataSource {
  const AuthRemoteDataSource(this._apiClient);

  final ApiClient _apiClient;

  Future<void> register({
    required String firstName,
    required String lastName,
    required String username,
    required String email,
    required String password,
    required String confirmPassword,
  }) async {
    final response = await _apiClient.post(
      '/api/Identity/Auth/Register',
      data: {
        'firstName': firstName,
        'lastName': lastName,
        'username': username,
        'email': email,
        'password': password,
        'confirmPassword': confirmPassword,
      },
    );
    ApiPayload.unwrap(response);
  }

  Future<CustomerSessionModel> signIn({
    required String usernameOrEmail,
    required String password,
  }) async {
    final response = await _apiClient.post(
      '/api/Identity/Auth/Login',
      data: {
        'usernameOrEmail': usernameOrEmail,
        'password': password,
        'rememberMe': true,
      },
    );
    if (response is! Map) {
      throw const FormatException('Unexpected login response.');
    }
    return CustomerSessionModel.fromJson(Map<String, dynamic>.from(response));
  }
}
