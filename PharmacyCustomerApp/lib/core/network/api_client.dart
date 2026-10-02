import 'package:dio/dio.dart';

import '../config/app_config.dart';
import '../errors/app_exception.dart';
import '../storage/token_storage.dart';

class ApiClient {
  ApiClient(this._tokenStorage)
    : dio = Dio(
        BaseOptions(
          baseUrl: AppConfig.apiBaseUri.toString().replaceFirst(
            RegExp(r'/$'),
            '',
          ),
          connectTimeout: const Duration(seconds: 15),
          receiveTimeout: const Duration(seconds: 20),
          headers: const {'Accept': 'application/json'},
        ),
      ) {
    dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final isLogin = options.path.toLowerCase().endsWith('/auth/login');
          final token = isLogin ? null : await _tokenStorage.read();
          if (token != null && token.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
      ),
    );
  }

  final TokenStorage _tokenStorage;
  final Dio dio;

  Future<dynamic> get(String path) async {
    try {
      return (await dio.get<dynamic>(path)).data;
    } on DioException catch (error) {
      throw _toAppException(error);
    }
  }

  Future<dynamic> post(String path, {Object? data}) async {
    try {
      return (await dio.post<dynamic>(path, data: data)).data;
    } on DioException catch (error) {
      throw _toAppException(error);
    }
  }

  Future<dynamic> put(String path, {Object? data}) async {
    try {
      return (await dio.put<dynamic>(path, data: data)).data;
    } on DioException catch (error) {
      throw _toAppException(error);
    }
  }

  Future<dynamic> delete(String path) async {
    try {
      return (await dio.delete<dynamic>(path)).data;
    } on DioException catch (error) {
      throw _toAppException(error);
    }
  }

  AppException _toAppException(DioException error) {
    final body = error.response?.data;
    if (body is Map) {
      final message =
          body['error'] ?? body['Error'] ?? body['message'] ?? body['Message'];
      if (message is String && message.isNotEmpty) {
        return AppException(message, statusCode: error.response?.statusCode);
      }
    }

    final message = switch (error.type) {
      DioExceptionType.connectionTimeout ||
      DioExceptionType.sendTimeout ||
      DioExceptionType.receiveTimeout =>
        'The request timed out. Please try again.',
      DioExceptionType.connectionError =>
        'Could not reach the pharmacy service. Check your connection.',
      _ => 'Something went wrong. Please try again.',
    };
    return AppException(message, statusCode: error.response?.statusCode);
  }
}
