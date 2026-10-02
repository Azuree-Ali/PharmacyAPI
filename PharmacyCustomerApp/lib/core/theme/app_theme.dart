import 'package:flutter/material.dart';

abstract final class AppTheme {
  static const _primary = Color(0xFF087E74);

  static ThemeData get light => ThemeData(
    useMaterial3: true,
    colorScheme: ColorScheme.fromSeed(
      seedColor: _primary,
      primary: _primary,
      surface: const Color(0xFFF8FAF9),
    ),
    scaffoldBackgroundColor: const Color(0xFFF8FAF9),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: Colors.white,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(14),
        borderSide: BorderSide.none,
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(14),
        borderSide: const BorderSide(color: Color(0xFFE1E8E6)),
      ),
    ),
  );
}
