import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_payload.dart';
import '../../domain/entities/customer_order.dart';
import '../models/customer_order_model.dart';

class OrdersRemoteDataSource {
  const OrdersRemoteDataSource(this._apiClient);
  final ApiClient _apiClient;

  Future<List<CustomerOrderModel>> getOrders() async {
    final data = ApiPayload.unwrap(
      await _apiClient.get('/api/Customer/Orders'),
    );
    return ApiPayload.asList(data)
        .map((item) => CustomerOrderModel.fromJson(ApiPayload.asMap(item)))
        .toList();
  }

  Future<CustomerOrderModel> getOrder(int id) async =>
      CustomerOrderModel.fromJson(
        ApiPayload.asMap(
          ApiPayload.unwrap(await _apiClient.get('/api/Customer/Orders/$id')),
        ),
      );

  Future<CheckoutReceipt> checkout({
    required String deliveryAddress,
    String? notes,
  }) async {
    final response = ApiPayload.asMap(
      ApiPayload.unwrap(
        await _apiClient.post(
          '/api/Customer/Checkout',
          data: {
            'paymentMethod': 0,
            'deliveryAddress': deliveryAddress,
            if (notes != null && notes.isNotEmpty) 'notes': notes,
          },
        ),
      ),
    );
    return CheckoutReceipt(
      orderId: _int(response, 'id', 'Id'),
      orderNumber: _string(response, 'orderNumber', 'OrderNumber'),
      netAmount: _double(response, 'netAmount', 'NetAmount'),
      status: CustomerOrderModel.parseStatus(
        response['status'] ?? response['Status'],
      ),
      isPaid: (response['isPaid'] ?? response['IsPaid']) as bool? ?? false,
    );
  }

  Future<void> cancelOrder(int id) async {
    ApiPayload.unwrap(await _apiClient.post('/api/Customer/Orders/$id/cancel'));
  }

  Future<void> confirmDelivery(int id) async {
    ApiPayload.unwrap(
      await _apiClient.post('/api/Customer/Orders/$id/arrived'),
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
}
