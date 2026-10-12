import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../models/exam_models.dart';
import '../services/exam_provider.dart';
import '../theme/app_theme.dart';
import '../widgets/five_min_warning_dialog.dart';
import '../widgets/overview_grid_sheet.dart';
import '../widgets/submit_confirm_dialog.dart';
import '../widgets/time_up_dialog.dart';
import 'exam_result_screen.dart';

class ExamTakingScreen extends StatefulWidget {
  final String studentName;

  const ExamTakingScreen({super.key, required this.studentName});

  @override
  State<ExamTakingScreen> createState() => _ExamTakingScreenState();
}

class _ExamTakingScreenState extends State<ExamTakingScreen> {
  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      final provider = context.read<ExamProvider>();
      provider.startTimer(
        on5MinWarning: () {
          if (mounted) {
            showDialog(
              context: context,
              barrierDismissible: false,
              builder: (_) => const FiveMinWarningDialog(),
            );
          }
        },
        onTimeUp: () {
          if (mounted) {
            showDialog(
              context: context,
              barrierDismissible: false,
              builder: (_) => TimeUpDialog(
                onAutoSubmit: _executeSubmit,
              ),
            );
          }
        },
      );
    });
  }

  void _executeSubmit() async {
    final provider = context.read<ExamProvider>();
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (_) => const Center(
        child: CircularProgressIndicator(color: AppColors.primary),
      ),
    );

    try {
      final result = await provider.submitExam(studentName: widget.studentName);
      if (mounted) {
        Navigator.of(context).pop(); // dismiss loading
        Navigator.of(context).pushReplacement(
          MaterialPageRoute(
            builder: (_) => ExamResultScreen(result: result),
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        Navigator.of(context).pop(); // dismiss loading
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Lỗi nộp bài: $e')),
        );
      }
    }
  }

  void _openOverviewSheet() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => FractionallySizedBox(
        heightFactor: 0.85,
        child: OverviewGridSheet(
          onSubmitRequested: () {
            Navigator.of(context).pop(); // close sheet
            _confirmSubmit();
          },
        ),
      ),
    );
  }

  void _confirmSubmit() {
    final provider = context.read<ExamProvider>();
    showDialog(
      context: context,
      builder: (_) => SubmitConfirmDialog(
        answeredCount: provider.answeredCount,
        totalCount: provider.totalQuestionsCount,
        onConfirm: _executeSubmit,
      ),
    );
  }

  String _formatTime(int totalSeconds) {
    final minutes = totalSeconds ~/ 60;
    final seconds = totalSeconds % 60;
    return '${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<ExamProvider>(
      builder: (context, provider, child) {
        final currentQ = provider.currentQuestion;
        final currentIndex = provider.currentIndex;
        final totalQ = provider.totalQuestionsCount;
        final remainingTime = provider.remainingSeconds;
        final passageGroup = currentQ != null ? provider.getPassageGroupForQuestion(currentQ) : null;
        final partNumber = currentQ != null ? provider.getPartNumberForQuestion(currentQ.questionNumber) : 5;

        return Scaffold(
          backgroundColor: AppColors.background,
          appBar: AppBar(
            backgroundColor: AppColors.primary,
            elevation: 0,
            leading: IconButton(
              icon: const Icon(Icons.grid_view_rounded, color: Colors.white),
              onPressed: _openOverviewSheet,
            ),
            title: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Part $partNumber · Câu ${currentQ?.questionNumber ?? (currentIndex + 101)}/200',
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w700,
                    color: Colors.white,
                  ),
                ),
                Text(
                  'Đã làm ${provider.answeredCount}/$totalQ câu',
                  style: TextStyle(
                    fontSize: 12,
                    color: Colors.white.withOpacity(0.85),
                  ),
                ),
              ],
            ),
            actions: [
              // Timer Badge
              Container(
                margin: const EdgeInsets.symmetric(vertical: 10, horizontal: 8),
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: remainingTime <= 300
                      ? AppColors.danger
                      : Colors.white.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Row(
                  children: [
                    const Icon(Icons.timer_outlined, size: 16, color: Colors.white),
                    const SizedBox(width: 4),
                    Text(
                      _formatTime(remainingTime),
                      style: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.w700,
                        fontSize: 13,
                      ),
                    ),
                  ],
                ),
              ),
              TextButton(
                onPressed: _confirmSubmit,
                child: const Text(
                  'Nộp bài',
                  style: TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.w700,
                    fontSize: 14,
                  ),
                ),
              ),
            ],
          ),
          body: currentQ == null
              ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
              : Column(
                  children: [
                    // Top Progress Bar
                    LinearProgressIndicator(
                      value: totalQ > 0 ? provider.answeredCount / totalQ : 0,
                      backgroundColor: AppColors.borderLight,
                      valueColor: const AlwaysStoppedAnimation<Color>(AppColors.primary),
                      minHeight: 4,
                    ),

                    // Main Content: Split layout if Part 6 or Part 7 (Passage + Question)
                    Expanded(
                      child: SingleChildScrollView(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            // If Part 6 or Part 7, show Passage Group card
                            if (passageGroup != null) ...[
                              _buildPassageCard(passageGroup, partNumber, currentQ),
                              const SizedBox(height: 16),
                            ],

                            // Question card
                            _buildQuestionCard(context, provider, currentQ, partNumber),
                          ],
                        ),
                      ),
                    ),

                    // Bottom Navigation Bar
                    _buildBottomNavigationBar(provider, currentIndex, totalQ),
                  ],
                ),
        );
      },
    );
  }

  Widget _buildPassageCard(PassageGroup group, int partNumber, QuestionItem currentQ) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.border),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.02),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      padding: const EdgeInsets.all(18),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: AppColors.primaryLight,
                  borderRadius: BorderRadius.circular(6),
                ),
                child: Text(
                  'Questions ${group.groupRange}',
                  style: const TextStyle(
                    color: AppColors.primary,
                    fontWeight: FontWeight.w700,
                    fontSize: 12,
                  ),
                ),
              ),
              if (group.title.isNotEmpty)
                Expanded(
                  child: Padding(
                    padding: const EdgeInsets.only(left: 10),
                    child: Text(
                      group.title,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        fontSize: 12,
                        color: AppColors.textSecondary,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ),
            ],
          ),
          const SizedBox(height: 14),
          const Divider(height: 1),
          const SizedBox(height: 14),

          // Passage Text with high readability
          SelectableText(
            group.passageText,
            style: const TextStyle(
              fontSize: 13.5,
              color: Color(0xFF1E293B),
              height: 1.65,
              letterSpacing: 0.1,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildQuestionCard(
    BuildContext context,
    ExamProvider provider,
    QuestionItem q,
    int partNumber,
  ) {
    final selectedAnswer = provider.getAnswerFor(q.questionNumber);

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: AppColors.border),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.02),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      padding: const EdgeInsets.all(18),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Question header
          Row(
            children: [
              Container(
                width: 32,
                height: 32,
                decoration: BoxDecoration(
                  color: AppColors.navy,
                  borderRadius: BorderRadius.circular(8),
                ),
                alignment: Alignment.center,
                child: Text(
                  '${q.questionNumber}',
                  style: const TextStyle(
                    color: Colors.white,
                    fontWeight: FontWeight.w700,
                    fontSize: 14,
                  ),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  q.blankNumber != null
                      ? 'Chọn đáp án đúng nhất cho vị trí [${q.blankNumber}] trong đoạn văn:'
                      : (q.questionText ?? 'Chọn đáp án đúng nhất:'),
                  style: const TextStyle(
                    fontSize: 14.5,
                    fontWeight: FontWeight.w600,
                    color: AppColors.navy,
                    height: 1.4,
                  ),
                ),
              ),
            ],
          ),

          const SizedBox(height: 18),

          // Options A, B, C, D
          Column(
            children: q.options.map((option) {
              final isSelected = selectedAnswer == option.label;

              return Padding(
                padding: const EdgeInsets.only(bottom: 10),
                child: InkWell(
                  onTap: () {
                    provider.selectAnswer(q.questionNumber, option.label);
                  },
                  borderRadius: BorderRadius.circular(12),
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
                    decoration: BoxDecoration(
                      color: isSelected ? AppColors.primaryLight : AppColors.background,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(
                        color: isSelected ? AppColors.primary : AppColors.border,
                        width: isSelected ? 1.5 : 1.0,
                      ),
                    ),
                    child: Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Container(
                          width: 28,
                          height: 28,
                          decoration: BoxDecoration(
                            color: isSelected ? AppColors.primary : Colors.white,
                            shape: BoxShape.circle,
                            border: Border.all(
                              color: isSelected ? AppColors.primary : AppColors.border,
                              width: 1.5,
                            ),
                          ),
                          alignment: Alignment.center,
                          child: Text(
                            option.label,
                            style: TextStyle(
                              color: isSelected ? Colors.white : AppColors.navy,
                              fontWeight: FontWeight.w700,
                              fontSize: 12.5,
                            ),
                          ),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Padding(
                            padding: const EdgeInsets.only(top: 4),
                            child: Text(
                              option.text,
                              style: TextStyle(
                                fontSize: 14,
                                color: isSelected ? AppColors.primaryDark : AppColors.textPrimary,
                                fontWeight: isSelected ? FontWeight.w600 : FontWeight.normal,
                                height: 1.4,
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              );
            }).toList(),
          ),
        ],
      ),
    );
  }

  Widget _buildBottomNavigationBar(ExamProvider provider, int currentIndex, int totalQ) {
    final hasPrev = currentIndex > 0;
    final hasNext = currentIndex < totalQ - 1;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        border: const Border(top: BorderSide(color: AppColors.border)),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 8,
            offset: const Offset(0, -2),
          ),
        ],
      ),
      child: SafeArea(
        child: Row(
          children: [
            Expanded(
              child: OutlinedButton.icon(
                onPressed: hasPrev ? () => provider.previousQuestion() : null,
                icon: const Icon(Icons.arrow_back_ios, size: 14),
                label: const Text('Câu trước'),
                style: OutlinedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  side: const BorderSide(color: AppColors.border),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
              ),
            ),
            const SizedBox(width: 10),
            IconButton(
              icon: const Icon(Icons.apps_rounded, color: AppColors.primary),
              tooltip: 'Danh sách 100 câu',
              onPressed: _openOverviewSheet,
            ),
            const SizedBox(width: 10),
            Expanded(
              child: ElevatedButton.icon(
                onPressed: hasNext ? () => provider.nextQuestion() : null,
                icon: const Icon(Icons.arrow_forward_ios, size: 14),
                label: const Text('Câu tiếp'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                  elevation: 0,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
