import 'cart_item.dart';

class ShoppingCart {
  const ShoppingCart({
    required this.id,
    required this.items,
    required this.totalAmount,
  });

  final int id;
  final List<CartItem> items;
  final double totalAmount;
}
