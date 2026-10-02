import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_payload.dart';
import '../models/shopping_cart_model.dart';

class CartRemoteDataSource {
  const CartRemoteDataSource(this._apiClient);

  final ApiClient _apiClient;

  Future<ShoppingCartModel> getCart() async => ShoppingCartModel.fromResponse(
    await _apiClient.get('/api/Customer/Cart'),
  );

  Future<void> addItem({required int productId, required int quantity}) async {
    ApiPayload.unwrap(
      await _apiClient.post(
        '/api/Customer/Cart/items',
        data: {'productId': productId, 'quantity': quantity},
      ),
    );
  }

  Future<void> updateQuantity({
    required int itemId,
    required int quantity,
  }) async {
    ApiPayload.unwrap(
      await _apiClient.put(
        '/api/Customer/Cart/items/$itemId',
        data: {'quantity': quantity},
      ),
    );
  }

  Future<void> removeItem(int itemId) async {
    ApiPayload.unwrap(
      await _apiClient.delete('/api/Customer/Cart/items/$itemId'),
    );
  }

  Future<void> clear() async {
    ApiPayload.unwrap(await _apiClient.delete('/api/Customer/Cart'));
  }
}
