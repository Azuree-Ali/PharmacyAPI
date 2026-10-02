import '../entities/customer_order.dart';

abstract interface class OrdersRepository {
  Future<List<CustomerOrder>> getOrders();
  Future<CustomerOrder> getOrder(int id);
  Future<CheckoutReceipt> checkout({
    required String deliveryAddress,
    String? notes,
  });
  Future<void> cancelOrder(int id);
  Future<void> confirmDelivery(int id);
}
