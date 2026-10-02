import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/api_payload.dart';
import '../../../../core/providers.dart';

class PasswordRecoveryScreen extends ConsumerStatefulWidget {
  const PasswordRecoveryScreen({super.key});
  @override
  ConsumerState<PasswordRecoveryScreen> createState() =>
      _PasswordRecoveryScreenState();
}

class _PasswordRecoveryScreenState
    extends ConsumerState<PasswordRecoveryScreen> {
  final _identity = TextEditingController(),
      _otp = TextEditingController(),
      _password = TextEditingController(),
      _confirm = TextEditingController();
  String? _userId, _token;
  bool _busy = false;
  @override
  void dispose() {
    _identity.dispose();
    _otp.dispose();
    _password.dispose();
    _confirm.dispose();
    super.dispose();
  }

  Future<void> _run(Future<void> Function() action) async {
    setState(() => _busy = true);
    try {
      await action();
    } catch (e) {
      if (mounted) _message(e.toString());
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  void _message(String text) =>
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(text)));
  @override
  Widget build(BuildContext context) => Scaffold(
    appBar: AppBar(title: const Text('Reset password')),
    body: ListView(
      padding: const EdgeInsets.all(20),
      children: [
        const Text('We will email you a one-time code to verify your request.'),
        const SizedBox(height: 18),
        TextField(
          controller: _identity,
          keyboardType: TextInputType.emailAddress,
          decoration: const InputDecoration(labelText: 'Username or email'),
        ),
        const SizedBox(height: 10),
        OutlinedButton(
          onPressed: _busy
              ? null
              : () => _run(() async {
                  ApiPayload.unwrap(
                    await ref
                        .read(apiClientProvider)
                        .post(
                          '/api/Identity/Auth/ForgetPassword',
                          data: {'userNameOrEmail': _identity.text.trim()},
                        ),
                  );
                  if (mounted) {
                    _message(
                      'If the account can receive email, a code has been sent.',
                    );
                  }
                }),
          child: const Text('Send code'),
        ),
        const SizedBox(height: 20),
        TextField(
          controller: _otp,
          keyboardType: TextInputType.number,
          decoration: const InputDecoration(labelText: 'Email code'),
        ),
        const SizedBox(height: 10),
        OutlinedButton(
          onPressed: _busy
              ? null
              : () => _run(() async {
                  final response = ApiPayload.asMap(
                    ApiPayload.unwrap(
                      await ref
                          .read(apiClientProvider)
                          .post(
                            '/api/Identity/Auth/VerifyOTP',
                            data: {
                              'userNameOrEmail': _identity.text.trim(),
                              'otp': _otp.text.trim(),
                            },
                          ),
                    ),
                  );
                  _token = '${response['token'] ?? response['Token'] ?? ''}';
                  _userId = '${response['userId'] ?? response['UserId'] ?? ''}';
                  if (_token!.isEmpty || _userId!.isEmpty) {
                    throw const FormatException(
                      'The reset response was incomplete.',
                    );
                  }
                  if (mounted) {
                    _message('Code verified. Choose a new password.');
                  }
                }),
          child: const Text('Verify code'),
        ),
        const SizedBox(height: 20),
        TextField(
          controller: _password,
          obscureText: true,
          decoration: const InputDecoration(labelText: 'New password'),
        ),
        const SizedBox(height: 10),
        TextField(
          controller: _confirm,
          obscureText: true,
          decoration: const InputDecoration(labelText: 'Confirm new password'),
        ),
        const SizedBox(height: 14),
        FilledButton(
          onPressed: _busy || _token == null
              ? null
              : () {
                  if (_password.text != _confirm.text) {
                    _message('Passwords do not match.');
                    return;
                  }
                  _run(() async {
                    ApiPayload.unwrap(
                      await ref
                          .read(apiClientProvider)
                          .post(
                            '/api/Identity/Auth/ResetPassword',
                            data: {
                              'userId': _userId,
                              'token': _token,
                              'newPassword': _password.text,
                              'confirmNewPassword': _confirm.text,
                            },
                          ),
                    );
                    if (!mounted || !context.mounted) return;
                    _message('Password reset. You can sign in now.');
                    Navigator.pop(context);
                  });
                },
          child: Text(_busy ? 'Working…' : 'Set new password'),
        ),
      ],
    ),
  );
}
