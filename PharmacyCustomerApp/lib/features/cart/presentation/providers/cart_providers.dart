import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/providers.dart';
import '../../data/data_sources/cart_remote_data_source.dart';
import '../../data/repositories/cart_repository_impl.dart';
import '../../domain/entities/shopping_cart.dart';
import '../../domain/repositories/cart_repository.dart';
import '../../domain/use_cases/cart_actions.dart';

final cartRepositoryProvider = Provider<CartRepository>(
  (ref) =>
      CartRepositoryImpl(CartRemoteDataSource(ref.watch(apiClientProvider))),
);
final cartActionsProvider = Provider<CartActions>(
  (ref) => CartActions(ref.watch(cartRepositoryProvider)),
);
final cartProvider = FutureProvider<ShoppingCart>(
  (ref) => ref.watch(cartRepositoryProvider).getCart(),
);
