import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../services/exam_provider.dart';
import '../theme/app_theme.dart';

class OverviewGridSheet extends StatelessWidget {
  final VoidCallback onSubmitRequested;

  const OverviewGridSheet({
    super.key,
    required this.onSubmitRequested,
  });

  @override
  Widget build(BuildContext context) {
    return Consumer<ExamProvider>(
      builder: (context, provider, child) {
        final answered = provider.answeredCount;
        final total = provider.totalQuestionsCount;

        return Container(
          decoration: const BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Drag handle
              Center(
                child: Container(
                  margin: const EdgeInsets.only(top: 12, bottom: 8),
                  width: 44,
                  height: 4,
                  decoration: BoxDecoration(
                    color: Colors.grey.shade300,
                    borderRadius: BorderRadius.circular(2),
                  ),
                ),
              ),

              // Title and status counters
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Tổng quan câu hỏi',
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.w700,
                            color: AppColors.navy,
                          ),
                        ),
                        const SizedBox(height: 4),
                        Text(
                          'Đã hoàn thành $answered / $total câu',
                          style: const TextStyle(
                            fontSize: 13,
                            color: AppColors.textSecondary,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
                      ],
                    ),
                    ElevatedButton(
                      onPressed: onSubmitRequested,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 10),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(10),
                        ),
                        elevation: 0,
                      ),
                      child: const Text(
                        'Nộp bài',
                        style: TextStyle(fontWeight: FontWeight.w700),
                      ),
                    ),
                  ],
                ),
              ),

              // Legend
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.start,
                  children: [
                    _buildLegendItem(AppColors.primary, 'Đã làm ($answered)'),
                    const SizedBox(width: 16),
                    _buildLegendItem(AppColors.warning, 'Đang xem'),
                    const SizedBox(width: 16),
                    _buildLegendItem(Colors.grey.shade200, 'Chưa làm (${total - answered})', textColor: Colors.grey.shade700),
                  ],
                ),
              ),

              const Divider(height: 1),

              // 100 Bubbles Grid
              Flexible(
                child: ListView(
                  shrinkWrap: true,
                  padding: const EdgeInsets.all(20),
                  children: [
                    _buildPartSection(context, provider, 'Part 5: Incomplete Sentences (101 - 130)', 101, 130),
                    const SizedBox(height: 16),
                    _buildPartSection(context, provider, 'Part 6: Text Completion (131 - 146)', 131, 146),
                    const SizedBox(height: 16),
                    _buildPartSection(context, provider, 'Part 7: Reading Comprehension (147 - 200)', 147, 200),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildLegendItem(Color color, String label, {Color? textColor}) {
    return Row(
      children: [
        Container(
          width: 14,
          height: 14,
          decoration: BoxDecoration(
            color: color,
            borderRadius: BorderRadius.circular(4),
          ),
        ),
        const SizedBox(width: 6),
        Text(
          label,
          style: TextStyle(
            fontSize: 12,
            fontWeight: FontWeight.w500,
            color: textColor ?? AppColors.textPrimary,
          ),
        ),
      ],
    );
  }

  Widget _buildPartSection(
    BuildContext context,
    ExamProvider provider,
    String title,
    int startQ,
    int endQ,
  ) {
    final questions = provider.allQuestions
        .where((q) => q.questionNumber >= startQ && q.questionNumber <= endQ)
        .toList();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: const TextStyle(
            fontSize: 13.5,
            fontWeight: FontWeight.w700,
            color: AppColors.navy,
          ),
        ),
        const SizedBox(height: 10),
        GridView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: questions.length,
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 6,
            mainAxisSpacing: 10,
            crossAxisSpacing: 10,
            childAspectRatio: 1.1,
          ),
          itemBuilder: (ctx, idx) {
            final q = questions[idx];
            final qNum = q.questionNumber;
            final isAnswered = provider.isQuestionAnswered(qNum);
            final isCurrent = provider.currentQuestion?.questionNumber == qNum;

            Color bgColor = Colors.grey.shade100;
            Color textColor = AppColors.textPrimary;
            Border? border;

            if (isCurrent) {
              bgColor = AppColors.warning;
              textColor = Colors.white;
            } else if (isAnswered) {
              bgColor = AppColors.primary;
              textColor = Colors.white;
            }

            return InkWell(
              onTap: () {
                provider.goToQuestionNumber(qNum);
                Navigator.of(context).pop();
              },
              borderRadius: BorderRadius.circular(10),
              child: Container(
                decoration: BoxDecoration(
                  color: bgColor,
                  borderRadius: BorderRadius.circular(10),
                  border: border,
                ),
                alignment: Alignment.center,
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      '$qNum',
                      style: TextStyle(
                        fontSize: 13,
                        fontWeight: FontWeight.w700,
                        color: textColor,
                      ),
                    ),
                    if (isAnswered)
                      Text(
                        provider.getAnswerFor(qNum) ?? '',
                        style: TextStyle(
                          fontSize: 10,
                          fontWeight: FontWeight.w600,
                          color: textColor.withOpacity(0.9),
                        ),
                      ),
                  ],
                ),
              ),
            );
          },
        ),
      ],
    );
  }
}
