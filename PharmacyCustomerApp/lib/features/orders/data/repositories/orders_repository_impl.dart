import '../../domain/entities/customer_order.dart';
import '../../domain/repositories/orders_repository.dart';
import '../data_sources/orders_remote_data_source.dart';

class OrdersRepositoryImpl implements OrdersRepository {
  const OrdersRepositoryImpl(this._remote);
  final OrdersRemoteDataSource _remote;

  @override
  Future<List<CustomerOrder>> getOrders() async => _remote.getOrders();
  @override
  Future<CustomerOrder> getOrder(int id) => _remote.getOrder(id);
  @override
  Future<CheckoutReceipt> checkout({
    required String deliveryAddress,
    String? notes,
  }) => _remote.checkout(deliveryAddress: deliveryAddress, notes: notes);
  @override
  Future<void> cancelOrder(int id) => _remote.cancelOrder(id);
  @override
  Future<void> confirmDelivery(int id) => _remote.confirmDelivery(id);
}
