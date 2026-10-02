import '../entities/customer_session.dart';

abstract interface class AuthRepository {
  Future<void> register({
    required String firstName,
    required String lastName,
    required String username,
    required String email,
    required String password,
    required String confirmPassword,
  });

  Future<CustomerSession> signIn({
    required String usernameOrEmail,
    required String password,
  });

  Future<void> signOut();
}
