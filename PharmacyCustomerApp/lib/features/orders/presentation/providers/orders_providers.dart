import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';
import '../../data/data_sources/orders_remote_data_source.dart';
import '../../data/repositories/orders_repository_impl.dart';
import '../../domain/entities/customer_order.dart';
import '../../domain/repositories/orders_repository.dart';
import '../../domain/use_cases/customer_order_actions.dart';

final ordersRepositoryProvider = Provider<OrdersRepository>(
  (ref) => OrdersRepositoryImpl(
    OrdersRemoteDataSource(ref.watch(apiClientProvider)),
  ),
);
final customerOrderActionsProvider = Provider<CustomerOrderActions>(
  (ref) => CustomerOrderActions(ref.watch(ordersRepositoryProvider)),
);
final ordersProvider = FutureProvider<List<CustomerOrder>>(
  (ref) => ref.watch(customerOrderActionsProvider).list(),
);
final orderDetailsProvider = FutureProvider.family<CustomerOrder, int>(
  (ref, id) => ref.watch(customerOrderActionsProvider).details(id),
);
