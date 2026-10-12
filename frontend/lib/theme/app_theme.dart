import 'package:flutter/material.dart';

class AppColors {
  // Brand / Primary Palette (from Figma)
  static const Color primary = Color(0xFF087F82); // Teal
  static const Color primaryLight = Color(0xFFE7F4F3);
  static const Color primaryDark = Color(0xFF066365);

  // Secondary / Dark Slate (from Figma)
  static const Color navy = Color(0xFF18324F); // Dark Navy
  static const Color navyDark = Color(0xFF23364B);
  static const Color navyLight = Color(0xFF334B68);

  // Neutrals & Backgrounds
  static const Color background = Color(0xFFF8FAFC);
  static const Color surface = Color(0xFFFFFFFF);
  static const Color card = Color(0xFFFFFFFF);
  static const Color border = Color(0xFFE2E8F0);
  static const Color borderLight = Color(0xFFEDF2F7);

  // Typography
  static const Color textPrimary = Color(0xFF1E293B);
  static const Color textSecondary = Color(0xFF64748B);
  static const Color textMuted = Color(0xFF94A3B8);

  // Accents / States
  static const Color success = Color(0xFF10B981);
  static const Color successLight = Color(0xFFD1FAE5);
  static const Color warning = Color(0xFFF59E0B);
  static const Color warningLight = Color(0xFFFEF3C7);
  static const Color danger = Color(0xFFEF4444);
  static const Color dangerLight = Color(0xFFFEE2E2);

  // Bubble Status Colors (Figma 100-Question Grid)
  static const Color bubbleAnswered = Color(0xFF087F82);
  static const Color bubbleUnanswered = Color(0xFFF1F5F9);
  static const Color bubbleCurrent = Color(0xFFF59E0B);
}

class AppTextStyles {
  static const TextStyle headerTitle = TextStyle(
    fontSize: 20,
    fontWeight: FontWeight.w700,
    color: AppColors.navy,
    letterSpacing: -0.3,
  );

  static const TextStyle sectionTitle = TextStyle(
    fontSize: 16,
    fontWeight: FontWeight.w700,
    color: AppColors.navy,
  );

  static const TextStyle body = TextStyle(
    fontSize: 14,
    color: AppColors.textPrimary,
    height: 1.5,
  );

  static const TextStyle bodyMuted = TextStyle(
    fontSize: 13,
    color: AppColors.textSecondary,
    height: 1.4,
  );

  static const TextStyle passage = TextStyle(
    fontSize: 13.5,
    color: Color(0xFF334155),
    height: 1.6,
  );
}
