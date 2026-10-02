import '../../../../core/network/api_client.dart';
import '../../../../core/network/api_payload.dart';
import '../models/customer_home_model.dart';
import '../models/product_model.dart';

class CatalogRemoteDataSource {
  const CatalogRemoteDataSource(this._apiClient);

  final ApiClient _apiClient;

  Future<CustomerHomeModel> getHome() async => CustomerHomeModel.fromResponse(
    await _apiClient.get('/api/Customer/Home'),
  );

  Future<List<ProductModel>> getProducts() async {
    final response = ApiPayload.unwrap(
      await _apiClient.get('/api/Customer/Product'),
    );
    return ApiPayload.asList(
      response,
    ).map((item) => ProductModel.fromJson(ApiPayload.asMap(item))).toList();
  }

  Future<ProductModel> getProduct(int id) async => ProductModel.fromJson(
    ApiPayload.asMap(
      ApiPayload.unwrap(await _apiClient.get('/api/Customer/Product/$id')),
    ),
  );
}
