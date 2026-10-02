import '../entities/customer_home.dart';
import '../entities/product.dart';

abstract interface class CatalogRepository {
  Future<CustomerHome> getHome();
  Future<List<Product>> getProducts();
  Future<Product> getProduct(int productId);
}
