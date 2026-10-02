enum CustomerOrderStatus { pending, processing, completed, cancelled, unknown }

class OrderLine {
  const OrderLine({
    required this.productId,
    required this.productName,
    required this.quantity,
    required this.unitPrice,
    required this.totalPrice,
  });
  final int productId;
  final String productName;
  final int quantity;
  final double unitPrice;
  final double totalPrice;
}

class CustomerOrder {
  const CustomerOrder({
    required this.id,
    required this.orderNumber,
    required this.orderDate,
    required this.status,
    required this.isPaid,
    required this.netAmount,
    this.totalAmount = 0,
    this.discount = 0,
    this.deliveryFees = 0,
    this.paymentMethod,
    this.deliveryAddress,
    this.notes,
    this.deliveryConfirmationRequestedAt,
    this.items = const [],
  });

  final int id;
  final String orderNumber;
  final DateTime orderDate;
  final CustomerOrderStatus status;
  final bool isPaid;
  final double totalAmount;
  final double discount;
  final double deliveryFees;
  final double netAmount;
  final int? paymentMethod;
  final String? deliveryAddress;
  final String? notes;
  final DateTime? deliveryConfirmationRequestedAt;
  final List<OrderLine> items;

  bool get canConfirmDelivery =>
      status == CustomerOrderStatus.pending &&
      !isPaid &&
      deliveryConfirmationRequestedAt != null;
}

class CheckoutReceipt {
  const CheckoutReceipt({
    required this.orderId,
    required this.orderNumber,
    required this.netAmount,
    required this.status,
    required this.isPaid,
  });
  final int orderId;
  final String orderNumber;
  final double netAmount;
  final CustomerOrderStatus status;
  final bool isPaid;
}
