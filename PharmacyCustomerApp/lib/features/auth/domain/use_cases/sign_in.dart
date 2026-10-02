import '../entities/customer_session.dart';
import '../repositories/auth_repository.dart';

class SignIn {
  const SignIn(this._repository);

  final AuthRepository _repository;

  Future<CustomerSession> call({
    required String usernameOrEmail,
    required String password,
  }) =>
      _repository.signIn(usernameOrEmail: usernameOrEmail, password: password);
}
