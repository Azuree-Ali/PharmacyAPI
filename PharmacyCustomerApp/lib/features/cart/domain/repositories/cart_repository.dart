import '../entities/shopping_cart.dart';

abstract interface class CartRepository {
  Future<ShoppingCart> getCart();
  Future<void> addItem({required int productId, required int quantity});
  Future<void> updateQuantity({required int itemId, required int quantity});
  Future<void> removeItem(int itemId);
  Future<void> clear();
}
