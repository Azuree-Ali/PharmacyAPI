import '../entities/product.dart';
import '../repositories/catalog_repository.dart';

class GetProducts {
  const GetProducts(this._repository);

  final CatalogRepository _repository;

  Future<List<Product>> call() => _repository.getProducts();
}
