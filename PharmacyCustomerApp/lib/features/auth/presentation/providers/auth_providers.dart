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
  final token = await ref.watch(tokenStorageProvider).read();
  return token != null && token.isNotEmpty;
});
