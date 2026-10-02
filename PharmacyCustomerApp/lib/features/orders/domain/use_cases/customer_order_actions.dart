import '../entities/customer_order.dart';
import '../repositories/orders_repository.dart';

class CustomerOrderActions {
  const CustomerOrderActions(this._repository);
  final OrdersRepository _repository;

  Future<List<CustomerOrder>> list() => _repository.getOrders();
  Future<CustomerOrder> details(int id) => _repository.getOrder(id);
  Future<CheckoutReceipt> checkout({required String address, String? notes}) =>
      _repository.checkout(deliveryAddress: address, notes: notes);
  Future<void> cancel(int id) => _repository.cancelOrder(id);
  Future<void> confirmDelivery(int id) => _repository.confirmDelivery(id);
}
