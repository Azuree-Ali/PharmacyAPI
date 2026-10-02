import '../../domain/entities/product.dart';

class ProductModel extends Product {
  const ProductModel({
    required super.id,
    required super.name,
    required super.price,
    required super.categoryId,
    super.genericName,
    super.categoryName,
    super.requiresPrescription,
  });

  factory ProductModel.fromJson(Map<String, dynamic> json) => ProductModel(
    id: _int(json, 'id', 'Id'),
    name: _string(json, 'name', 'Name'),
    genericName: _nullableString(json, 'genericName', 'GenericName'),
    price: _double(json, 'price', 'Price'),
    categoryId: _int(json, 'categoryId', 'CategoryId'),
    categoryName: _nullableString(json, 'categoryName', 'CategoryName'),
    requiresPrescription: _bool(
      json,
      'requiresPrescription',
      'RequiresPrescription',
    ),
  );

  static dynamic _value(
    Map<String, dynamic> json,
    String lower,
    String upper,
  ) => json[lower] ?? json[upper];

  static int _int(Map<String, dynamic> json, String lower, String upper) =>
      (_value(json, lower, upper) as num?)?.toInt() ?? 0;

  static double _double(
    Map<String, dynamic> json,
    String lower,
    String upper,
  ) => (_value(json, lower, upper) as num?)?.toDouble() ?? 0;

  static String _string(
    Map<String, dynamic> json,
    String lower,
    String upper,
  ) => _value(json, lower, upper) as String? ?? '';

  static String? _nullableString(
    Map<String, dynamic> json,
    String lower,
    String upper,
  ) => _value(json, lower, upper) as String?;

  static bool _bool(Map<String, dynamic> json, String lower, String upper) =>
      _value(json, lower, upper) as bool? ?? false;
}
