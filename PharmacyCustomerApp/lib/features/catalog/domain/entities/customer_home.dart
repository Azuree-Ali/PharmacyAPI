import 'category.dart';
import 'product.dart';

class CustomerHome {
  const CustomerHome({required this.categories, required this.products});

  final List<ProductCategory> categories;
  final List<Product> products;
}
