import '../../domain/entities/customer_session.dart';

class CustomerSessionModel extends CustomerSession {
  const CustomerSessionModel({required super.accessToken});

  factory CustomerSessionModel.fromJson(Map<String, dynamic> json) {
    final token = json['accessToken'] ?? json['AccessToken'];
    if (token is! String || token.isEmpty) {
      throw const FormatException(
        'Login response did not contain an access token.',
      );
    }
    return CustomerSessionModel(accessToken: token);
  }
}
