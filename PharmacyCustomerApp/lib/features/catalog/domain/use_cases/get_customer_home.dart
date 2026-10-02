import '../entities/customer_home.dart';
import '../repositories/catalog_repository.dart';

class GetCustomerHome {
  const GetCustomerHome(this._repository);

  final CatalogRepository _repository;

  Future<CustomerHome> call() => _repository.getHome();
}
