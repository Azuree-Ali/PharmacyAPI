import '../../../../core/storage/token_storage.dart';
import '../../domain/entities/customer_session.dart';
import '../../domain/repositories/auth_repository.dart';
import '../data_sources/auth_remote_data_source.dart';

class AuthRepositoryImpl implements AuthRepository {
  const AuthRepositoryImpl(this._remote, this._tokenStorage);

  final AuthRemoteDataSource _remote;
  final TokenStorage _tokenStorage;

  @override
  Future<void> register({
    required String firstName,
    required String lastName,
    required String username,
    required String email,
    required String password,
    required String confirmPassword,
  }) => _remote.register(
    firstName: firstName,
    lastName: lastName,
    username: username,
    email: email,
    password: password,
    confirmPassword: confirmPassword,
  );

  @override
  Future<CustomerSession> signIn({
    required String usernameOrEmail,
    required String password,
  }) async {
    final session = await _remote.signIn(
      usernameOrEmail: usernameOrEmail,
      password: password,
    );
    await _tokenStorage.save(session.accessToken);
    return session;
  }

  @override
  Future<void> signOut() => _tokenStorage.clear();
}
