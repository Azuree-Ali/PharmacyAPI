import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/network/api_client.dart';
import '../../core/network/api_payload.dart';
import '../../core/providers.dart';

final accountApiProvider = Provider<AccountApi>(
  (ref) => AccountApi(ref.watch(apiClientProvider)),
);

class AccountApi {
  const AccountApi(this._client);
  final ApiClient _client;

  Future<Map<String, dynamic>> profile() async => ApiPayload.asMap(
    ApiPayload.unwrap(await _client.get('/api/Identity/Profile')),
  );

  Future<void> updateProfile(Map<String, dynamic> value) async =>
      ApiPayload.unwrap(
        await _client.put('/api/Identity/Profile/update', data: value),
      );

  Future<void> updatePassword(String current, String next) async =>
      ApiPayload.unwrap(
        await _client.put(
          '/api/Identity/Profile/update-password',
          data: {'currentPassword': current, 'newPassword': next},
        ),
      );

  Future<List<Map<String, dynamic>>> notifications() async => ApiPayload.asList(
    ApiPayload.unwrap(await _client.get('/api/Customer/Notifications')),
  ).map((item) => ApiPayload.asMap(item)).toList();

  Future<void> notificationAction(String path, {bool delete = false}) async =>
      ApiPayload.unwrap(
        delete ? await _client.delete(path) : await _client.put(path),
      );

  Future<Map<String, dynamic>> chat() async => ApiPayload.asMap(
    ApiPayload.unwrap(await _client.get('/api/Customer/Chat')),
  );

  Future<void> sendMessage(int chatId, String message) async =>
      ApiPayload.unwrap(
        await _client.post(
          '/api/Customer/Chat/$chatId/messages',
          data: {'message': message},
        ),
      );

  Future<void> performDeliveryAction({
    required String method,
    required String href,
  }) async {
    final action = RegExp(
      r'^/api/Customer/Orders/\d+/(arrived|cancel)$',
    ).firstMatch(href);
    if (method.toUpperCase() != 'POST' || action == null) {
      throw const FormatException('Unsupported delivery action.');
    }
    ApiPayload.unwrap(await _client.post(href));
  }
}

final profileProvider = FutureProvider<Map<String, dynamic>>(
  (ref) => ref.watch(accountApiProvider).profile(),
);
final notificationsProvider = FutureProvider<List<Map<String, dynamic>>>(
  (ref) => ref.watch(accountApiProvider).notifications(),
);
final supportChatProvider = FutureProvider<Map<String, dynamic>>(
  (ref) => ref.watch(accountApiProvider).chat(),
);
