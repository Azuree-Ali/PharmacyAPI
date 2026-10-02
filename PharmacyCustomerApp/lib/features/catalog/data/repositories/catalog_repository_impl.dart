import '../../domain/entities/customer_home.dart';
import '../../domain/entities/product.dart';
import '../../domain/repositories/catalog_repository.dart';
import '../data_sources/catalog_remote_data_source.dart';

class CatalogRepositoryImpl implements CatalogRepository {
  const CatalogRepositoryImpl(this._remote);

  final CatalogRemoteDataSource _remote;

  @override
  Future<CustomerHome> getHome() => _remote.getHome();

  @override
  Future<List<Product>> getProducts() => _remote.getProducts();

  @override
  Future<Product> getProduct(int productId) => _remote.getProduct(productId);
}
