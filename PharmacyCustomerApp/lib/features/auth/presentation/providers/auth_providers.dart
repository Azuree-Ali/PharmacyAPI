import 'dart:convert';

import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';
import '../../data/data_sources/auth_remote_data_source.dart';
import '../../data/repositories/auth_repository_impl.dart';
import '../../domain/repositories/auth_repository.dart';
import '../../domain/use_cases/register_customer.dart';
import '../../domain/use_cases/sign_in.dart';

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  return AuthRepositoryImpl(
    AuthRemoteDataSource(ref.watch(apiClientProvider)),
    ref.watch(tokenStorageProvider),
  );
});

final signInProvider = Provider<SignIn>(
  (ref) => SignIn(ref.watch(authRepositoryProvider)),
);

final registerCustomerProvider = Provider<RegisterCustomer>(
  (ref) => RegisterCustomer(ref.watch(authRepositoryProvider)),
);

final sessionProvider = FutureProvider<bool>((ref) async {
  final storage = ref.watch(tokenStorageProvider);
  final token = await storage.read();
  if (token == null || token.isEmpty) return false;

  try {
    final payload = token.split('.')[1];
    final claims = jsonDecode(utf8.decode(base64Url.decode(base64Url.normalize(payload))))
        as Map<String, dynamic>;
    final role = claims['role'] ??
        claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    final roles = role is List ? role : [role];
    if (roles.contains('Customer')) return true;
  } on FormatException {
    // An unreadable saved token cannot be used to enter the customer area.
  } on TypeError {
    // A malformed token payload is treated as an expired customer session.
  }

  await storage.clear();
  return false;
});
