import '../repositories/cart_repository.dart';

class CartActions {
  const CartActions(this._repository);

  final CartRepository _repository;

  Future<void> add(int productId, {int quantity = 1}) =>
      _repository.addItem(productId: productId, quantity: quantity);
  Future<void> updateQuantity(int itemId, int quantity) =>
      _repository.updateQuantity(itemId: itemId, quantity: quantity);
  Future<void> remove(int itemId) => _repository.removeItem(itemId);
  Future<void> clear() => _repository.clear();
}
