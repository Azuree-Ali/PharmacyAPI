import '../../../../core/network/api_payload.dart';
import '../../domain/entities/cart_item.dart';
import '../../domain/entities/shopping_cart.dart';

class ShoppingCartModel extends ShoppingCart {
  const ShoppingCartModel({
    required super.id,
    required super.items,
    required super.totalAmount,
  });

  factory ShoppingCartModel.fromResponse(dynamic response) {
    final json = ApiPayload.asMap(ApiPayload.unwrap(response));
    final items = ApiPayload.asList(json['items'] ?? json['Items'] ?? const [])
        .map((item) {
          final map = ApiPayload.asMap(item);
          return CartItem(
            id: _int(map, 'id', 'Id'),
            productId: _int(map, 'productId', 'ProductId'),
            productName:
                (map['productName'] ?? map['ProductName']) as String? ?? '',
            unitPrice: _double(map, 'unitPrice', 'UnitPrice'),
            quantity: _int(map, 'quantity', 'Quantity'),
            totalPrice: _double(map, 'totalPrice', 'TotalPrice'),
          );
        })
        .toList();
    return ShoppingCartModel(
      id: _int(json, 'id', 'Id'),
      items: items,
      totalAmount: _double(json, 'totalAmount', 'TotalAmount'),
    );
  }

  static int _int(Map<String, dynamic> json, String lower, String upper) =>
      ((json[lower] ?? json[upper]) as num?)?.toInt() ?? 0;
  static double _double(
    Map<String, dynamic> json,
    String lower,
    String upper,
  ) => ((json[lower] ?? json[upper]) as num?)?.toDouble() ?? 0;
}
