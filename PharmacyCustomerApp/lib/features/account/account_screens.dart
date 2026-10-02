import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'account_api.dart';

dynamic _field(Map<String, dynamic> data, String name) =>
    data[name] ?? data[name[0].toUpperCase() + name.substring(1)];

class ProfileScreen extends ConsumerStatefulWidget {
  const ProfileScreen({super.key});
  @override
  ConsumerState<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends ConsumerState<ProfileScreen> {
  final _form = GlobalKey<FormState>();
  final _first = TextEditingController(),
      _last = TextEditingController(),
      _phone = TextEditingController(),
      _address = TextEditingController();
  String _email = '';
  bool _loaded = false, _saving = false;

  @override
  void dispose() {
    _first.dispose();
    _last.dispose();
    _phone.dispose();
    _address.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final profile = ref.watch(profileProvider);
    return Scaffold(
      appBar: AppBar(title: const Text('Profile')),
      body: profile.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (e, _) =>
            _retry(e.toString(), () => ref.invalidate(profileProvider)),
        data: (data) {
          if (!_loaded) {
            _first.text = '${_field(data, 'firstName') ?? ''}';
            _last.text = '${_field(data, 'lastName') ?? ''}';
            _phone.text = '${_field(data, 'phoneNumber') ?? ''}';
            _address.text = '${_field(data, 'adresse') ?? ''}';
            _email = '${_field(data, 'email') ?? ''}';
            _loaded = true;
          }
          return Form(
            key: _form,
            child: ListView(
              padding: const EdgeInsets.all(20),
              children: [
                _input(_first, 'First name'),
                const SizedBox(height: 12),
                _input(_last, 'Last name'),
                const SizedBox(height: 12),
                _input(_phone, 'Phone number', optional: true),
                const SizedBox(height: 12),
                _input(_address, 'Address', optional: true),
                const SizedBox(height: 20),
                FilledButton(
                  onPressed: _saving ? null : _save,
                  child: Text(_saving ? 'Saving…' : 'Save profile'),
                ),
                const SizedBox(height: 24),
                const Divider(),
                const SizedBox(height: 10),
                const Text(
                  'Change password',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600),
                ),
                const SizedBox(height: 12),
                const _PasswordForm(),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _input(
    TextEditingController controller,
    String label, {
    bool optional = false,
  }) => TextFormField(
    controller: controller,
    decoration: InputDecoration(labelText: label),
    validator: (v) =>
        !optional && (v == null || v.trim().isEmpty) ? 'Enter $label' : null,
  );

  Future<void> _save() async {
    if (!_form.currentState!.validate()) return;
    setState(() => _saving = true);
    try {
      await ref.read(accountApiProvider).updateProfile({
        'firstName': _first.text.trim(),
        'lastName': _last.text.trim(),
        'phoneNumber': _phone.text.trim(),
        'adresse': _address.text.trim(),
        'email': _email,
      });
      ref.invalidate(profileProvider);
      if (mounted) _toast('Profile saved');
    } catch (e) {
      if (mounted) _toast(e.toString());
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  void _toast(String value) => ScaffoldMessenger.of(
    context,
  ).showSnackBar(SnackBar(content: Text(value)));
}

class _PasswordForm extends ConsumerStatefulWidget {
  const _PasswordForm();
  @override
  ConsumerState<_PasswordForm> createState() => _PasswordFormState();
}

class _PasswordFormState extends ConsumerState<_PasswordForm> {
  final _current = TextEditingController(), _next = TextEditingController();
  bool _saving = false;
  @override
  void dispose() {
    _current.dispose();
    _next.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => Column(
    children: [
      TextField(
        controller: _current,
        obscureText: true,
        decoration: const InputDecoration(labelText: 'Current password'),
      ),
      const SizedBox(height: 10),
      TextField(
        controller: _next,
        obscureText: true,
        decoration: const InputDecoration(labelText: 'New password'),
      ),
      const SizedBox(height: 12),
      OutlinedButton(
        onPressed: _saving
            ? null
            : () async {
                if (_current.text.isEmpty || _next.text.isEmpty) return;
                setState(() => _saving = true);
                try {
                  await ref
                      .read(accountApiProvider)
                      .updatePassword(_current.text, _next.text);
                  _current.clear();
                  _next.clear();
                  if (context.mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text('Password updated')),
                    );
                  }
                } catch (e) {
                  if (context.mounted) {
                    ScaffoldMessenger.of(
                      context,
                    ).showSnackBar(SnackBar(content: Text(e.toString())));
                  }
                } finally {
                  if (mounted) setState(() => _saving = false);
                }
              },
        child: Text(_saving ? 'Updating…' : 'Update password'),
      ),
    ],
  );
}

class NotificationsScreen extends ConsumerWidget {
  const NotificationsScreen({super.key});
  @override
  Widget build(BuildContext context, WidgetRef ref) => Scaffold(
    appBar: AppBar(
      title: const Text('Notifications'),
      actions: [
        TextButton(
          onPressed: () async {
            try {
              await ref
                  .read(accountApiProvider)
                  .notificationAction('/api/Customer/Notifications/read-all');
              ref.invalidate(notificationsProvider);
            } catch (e) {
              if (!context.mounted) return;
              ScaffoldMessenger.of(
                context,
              ).showSnackBar(SnackBar(content: Text(e.toString())));
            }
          },
          child: const Text('Read all'),
        ),
      ],
    ),
    body: ref
        .watch(notificationsProvider)
        .when(
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (e, _) =>
              _retry(e.toString(), () => ref.invalidate(notificationsProvider)),
          data: (items) => items.isEmpty
              ? const _Empty(
                  icon: Icons.notifications_none,
                  text: 'You are all caught up.',
                )
              : RefreshIndicator(
                  onRefresh: () => ref.refresh(notificationsProvider.future),
                  child: ListView.builder(
                    itemCount: items.length,
                    itemBuilder: (context, i) {
                      final item = items[i];
                      final id = _field(item, 'id');
                      final read = _field(item, 'isRead') == true;
                      return Card(
                        margin: const EdgeInsets.symmetric(
                          horizontal: 16,
                          vertical: 5,
                        ),
                        child: ListTile(
                          leading: Icon(
                            read
                                ? Icons.notifications_none
                                : Icons.notifications_active,
                            color: read
                                ? null
                                : Theme.of(context).colorScheme.primary,
                          ),
                          title: Text('${_field(item, 'message') ?? ''}'),
                          subtitle: Text('${_field(item, 'createdAt') ?? ''}'),
                          onTap: () async {
                            if (id == null || read) return;
                            try {
                              await ref
                                  .read(accountApiProvider)
                                  .notificationAction(
                                    '/api/Customer/Notifications/$id/read',
                                  );
                              ref.invalidate(notificationsProvider);
                            } catch (e) {
                              if (context.mounted) {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  SnackBar(content: Text(e.toString())),
                                );
                              }
                            }
                          },
                          trailing: IconButton(
                            icon: const Icon(Icons.delete_outline),
                            onPressed: () async {
                              if (id == null) return;
                              try {
                                await ref
                                    .read(accountApiProvider)
                                    .notificationAction(
                                      '/api/Customer/Notifications/$id',
                                      delete: true,
                                    );
                                ref.invalidate(notificationsProvider);
                              } catch (e) {
                                if (context.mounted) {
                                  ScaffoldMessenger.of(context).showSnackBar(
                                    SnackBar(content: Text(e.toString())),
                                  );
                                }
                              }
                            },
                          ),
                        ),
                      );
                    },
                  ),
                ),
        ),
  );
}

class SupportChatScreen extends ConsumerStatefulWidget {
  const SupportChatScreen({super.key});
  @override
  ConsumerState<SupportChatScreen> createState() => _SupportChatScreenState();
}

class _SupportChatScreenState extends ConsumerState<SupportChatScreen> {
  final _message = TextEditingController();
  bool _sending = false;
  @override
  void dispose() {
    _message.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(
      title: const Text('Support chat'),
      actions: [
        IconButton(
          tooltip: 'Refresh conversation',
          onPressed: () => ref.invalidate(supportChatProvider),
          icon: const Icon(Icons.refresh),
        ),
      ],
    ),
    body: ref
        .watch(supportChatProvider)
        .when(
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (e, _) =>
              _retry(e.toString(), () => ref.invalidate(supportChatProvider)),
          data: (chat) {
            final messages = (_field(chat, 'messages') as List? ?? const [])
                .whereType<Map>()
                .map((m) => Map<String, dynamic>.from(m))
                .toList();
            final chatId = (_field(chat, 'id') as num?)?.toInt() ?? 0;
            return Column(
              children: [
                Expanded(
                  child: messages.isEmpty
                      ? const _Empty(
                          icon: Icons.forum_outlined,
                          text: 'Send a message to reach our support team.',
                        )
                      : ListView.builder(
                          reverse: false,
                          padding: const EdgeInsets.all(16),
                          itemCount: messages.length,
                          itemBuilder: (context, i) {
                            final msg = messages[i];
                            return Align(
                              alignment: Alignment.centerLeft,
                              child: Container(
                                margin: const EdgeInsets.only(bottom: 10),
                                padding: const EdgeInsets.all(14),
                                constraints: const BoxConstraints(
                                  maxWidth: 320,
                                ),
                                decoration: BoxDecoration(
                                  color: Theme.of(
                                    context,
                                  ).colorScheme.primaryContainer,
                                  borderRadius: BorderRadius.circular(16),
                                ),
                                child: Text('${_field(msg, 'message') ?? ''}'),
                              ),
                            );
                          },
                        ),
                ),
                SafeArea(
                  top: false,
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(12, 8, 12, 12),
                    child: Row(
                      children: [
                        Expanded(
                          child: TextField(
                            controller: _message,
                            onChanged: (_) => setState(() {}),
                            minLines: 1,
                            maxLines: 4,
                            decoration: const InputDecoration(
                              hintText: 'Write a message',
                            ),
                          ),
                        ),
                        const SizedBox(width: 8),
                        IconButton.filled(
                          onPressed: _sending || _message.text.trim().isEmpty
                              ? null
                              : () async {
                                  final text = _message.text.trim();
                                  setState(() => _sending = true);
                                  try {
                                    await ref
                                        .read(accountApiProvider)
                                        .sendMessage(chatId, text);
                                    _message.clear();
                                    ref.invalidate(supportChatProvider);
                                  } catch (e) {
                                    if (context.mounted) {
                                      ScaffoldMessenger.of(
                                        context,
                                      ).showSnackBar(
                                        SnackBar(content: Text(e.toString())),
                                      );
                                    }
                                  } finally {
                                    if (mounted) {
                                      setState(() => _sending = false);
                                    }
                                  }
                                },
                          icon: const Icon(Icons.send),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            );
          },
        ),
  );
}

Widget _retry(String message, VoidCallback retry) => Center(
  child: Padding(
    padding: const EdgeInsets.all(24),
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(message, textAlign: TextAlign.center),
        const SizedBox(height: 12),
        OutlinedButton(onPressed: retry, child: const Text('Try again')),
      ],
    ),
  ),
);

class _Empty extends StatelessWidget {
  const _Empty({required this.icon, required this.text});
  final IconData icon;
  final String text;
  @override
  Widget build(BuildContext context) => Center(
    child: Padding(
      padding: const EdgeInsets.all(28),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 46, color: Theme.of(context).colorScheme.primary),
          const SizedBox(height: 12),
          Text(text, textAlign: TextAlign.center),
        ],
      ),
    ),
  );
}
