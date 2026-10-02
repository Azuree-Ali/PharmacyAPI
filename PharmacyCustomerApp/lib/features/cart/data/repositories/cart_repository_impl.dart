import '../../domain/entities/shopping_cart.dart';
import '../../domain/repositories/cart_repository.dart';
import '../data_sources/cart_remote_data_source.dart';

class CartRepositoryImpl implements CartRepository {
  const CartRepositoryImpl(this._remote);
  final CartRemoteDataSource _remote;

  @override
  Future<ShoppingCart> getCart() => _remote.getCart();
  @override
  Future<void> addItem({required int productId, required int quantity}) =>
      _remote.addItem(productId: productId, quantity: quantity);
  @override
  Future<void> updateQuantity({required int itemId, required int quantity}) =>
      _remote.updateQuantity(itemId: itemId, quantity: quantity);
  @override
  Future<void> removeItem(int itemId) => _remote.removeItem(itemId);
  @override
  Future<void> clear() => _remote.clear();
}
