import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/providers.dart';
import '../../data/data_sources/catalog_remote_data_source.dart';
import '../../data/repositories/catalog_repository_impl.dart';
import '../../domain/entities/customer_home.dart';
import '../../domain/entities/product.dart';
import '../../domain/repositories/catalog_repository.dart';
import '../../domain/use_cases/get_customer_home.dart';
import '../../domain/use_cases/get_products.dart';

final catalogRepositoryProvider = Provider<CatalogRepository>((ref) {
  return CatalogRepositoryImpl(
    CatalogRemoteDataSource(ref.watch(apiClientProvider)),
  );
});

final getCustomerHomeProvider = Provider<GetCustomerHome>(
  (ref) => GetCustomerHome(ref.watch(catalogRepositoryProvider)),
);

final getProductsProvider = Provider<GetProducts>(
  (ref) => GetProducts(ref.watch(catalogRepositoryProvider)),
);

final customerHomeProvider = FutureProvider<CustomerHome>(
  (ref) => ref.watch(getCustomerHomeProvider)(),
);

final productsProvider = FutureProvider<List<Product>>(
  (ref) => ref.watch(getProductsProvider)(),
);
