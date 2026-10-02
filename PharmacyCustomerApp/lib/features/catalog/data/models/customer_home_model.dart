import '../../../../core/network/api_payload.dart';
import '../../domain/entities/category.dart';
import '../../domain/entities/customer_home.dart';
import 'product_model.dart';

class CustomerHomeModel extends CustomerHome {
  const CustomerHomeModel({required super.categories, required super.products});

  factory CustomerHomeModel.fromResponse(dynamic response) {
    final json = ApiPayload.asMap(ApiPayload.unwrap(response));
    final categories =
        ApiPayload.asList(
          json['categories'] ?? json['Categories'] ?? const [],
        ).map((item) {
          final map = ApiPayload.asMap(item);
          return ProductCategory(
            id: ((map['id'] ?? map['Id']) as num).toInt(),
            name: (map['name'] ?? map['Name']) as String,
          );
        }).toList();
    final products = ApiPayload.asList(
      json['products'] ?? json['Products'] ?? const [],
    ).map((item) => ProductModel.fromJson(ApiPayload.asMap(item))).toList();
    return CustomerHomeModel(categories: categories, products: products);
  }
}
