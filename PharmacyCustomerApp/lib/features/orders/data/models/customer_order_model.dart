import '../../../../core/network/api_payload.dart';
import '../../domain/entities/customer_order.dart';

class CustomerOrderModel extends CustomerOrder {
  const CustomerOrderModel({
    required super.id,
    required super.orderNumber,
    required super.orderDate,
    required super.status,
    required super.isPaid,
    required super.netAmount,
    super.totalAmount,
    super.discount,
    super.deliveryFees,
    super.paymentMethod,
    super.deliveryAddress,
    super.notes,
    super.deliveryConfirmationRequestedAt,
    super.items,
  });

  factory CustomerOrderModel.fromJson(Map<String, dynamic> json) {
    final linesValue = json['orderItems'] ?? json['OrderItems'] ?? const [];
    final lines = ApiPayload.asList(linesValue).map((value) {
      final line = ApiPayload.asMap(value);
      return OrderLine(
        productId: _int(line, 'productId', 'ProductId'),
        productName:
            (line['productName'] ?? line['ProductName']) as String? ??
            'Product',
        quantity: _int(line, 'quantity', 'Quantity'),
        unitPrice: _double(line, 'unitPrice', 'UnitPrice'),
        totalPrice: _double(line, 'totalPrice', 'TotalPrice'),
      );
    }).toList();
    return CustomerOrderModel(
      id: _int(json, 'id', 'Id'),
      orderNumber: _string(json, 'orderNumber', 'OrderNumber'),
      orderDate:
          DateTime.tryParse(_string(json, 'orderDate', 'OrderDate')) ??
          DateTime.now(),
      status: parseStatus(json['status'] ?? json['Status']),
      isPaid: (json['isPaid'] ?? json['IsPaid']) as bool? ?? false,
      totalAmount: _double(json, 'totalAmount', 'TotalAmount'),
      discount: _double(json, 'discount', 'Discount'),
      deliveryFees: _double(json, 'deliveryFees', 'DeliveryFees'),
      netAmount: _double(json, 'netAmount', 'NetAmount'),
      paymentMethod: _optionalInt(
        json['paymentMethod'] ?? json['PaymentMethod'],
      ),
      deliveryAddress:
          (json['deliveryAddress'] ?? json['DeliveryAddress']) as String?,
      notes: (json['notes'] ?? json['Notes']) as String?,
      deliveryConfirmationRequestedAt: _optionalDate(
        json['deliveryConfirmationRequestedAt'] ??
            json['DeliveryConfirmationRequestedAt'],
      ),
      items: lines,
    );
  }

  static int _int(Map<String, dynamic> json, String lower, String upper) =>
      ((json[lower] ?? json[upper]) as num?)?.toInt() ?? 0;
  static double _double(
    Map<String, dynamic> json,
    String lower,
    String upper,
  ) => ((json[lower] ?? json[upper]) as num?)?.toDouble() ?? 0;
  static String _string(
    Map<String, dynamic> json,
    String lower,
    String upper,
  ) => (json[lower] ?? json[upper]) as String? ?? '';
  static int? _optionalInt(dynamic value) =>
      value is num ? value.toInt() : null;
  static DateTime? _optionalDate(dynamic value) =>
      value is String ? DateTime.tryParse(value) : null;

  static CustomerOrderStatus parseStatus(dynamic value) {
    if (value is num) {
      return switch (value.toInt()) {
        0 => CustomerOrderStatus.pending,
        1 => CustomerOrderStatus.processing,
        2 => CustomerOrderStatus.completed,
        3 => CustomerOrderStatus.cancelled,
        _ => CustomerOrderStatus.unknown,
      };
    }
    return switch (value?.toString().toLowerCase()) {
      'pending' => CustomerOrderStatus.pending,
      'processing' => CustomerOrderStatus.processing,
      'completed' => CustomerOrderStatus.completed,
      'cancelled' => CustomerOrderStatus.cancelled,
      _ => CustomerOrderStatus.unknown,
    };
  }
}
