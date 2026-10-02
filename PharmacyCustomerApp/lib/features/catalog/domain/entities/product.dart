class Product {
  const Product({
    required this.id,
    required this.name,
    required this.price,
    required this.categoryId,
    this.genericName,
    this.categoryName,
    this.requiresPrescription = false,
  });

  final int id;
  final String name;
  final String? genericName;
  final double price;
  final int categoryId;
  final String? categoryName;
  final bool requiresPrescription;
}
