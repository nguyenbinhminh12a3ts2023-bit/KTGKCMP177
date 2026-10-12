class ExamListItem {
  final int id;
  final String title;
  final String code;
  final int durationMinutes;
  final int totalQuestions;
  final String description;

  ExamListItem({
    required this.id,
    required this.title,
    required this.code,
    required this.durationMinutes,
    required this.totalQuestions,
    required this.description,
  });

  factory ExamListItem.fromJson(Map<String, dynamic> json) {
    return ExamListItem(
      id: json['id'] ?? 0,
      title: json['title'] ?? '',
      code: json['code'] ?? '',
      durationMinutes: json['durationMinutes'] ?? 75,
      totalQuestions: json['totalQuestions'] ?? 100,
      description: json['description'] ?? '',
    );
  }
}

class ExamDetail {
  final int id;
  final String title;
  final String code;
  final int durationMinutes;
  final int totalQuestions;
  final String description;
  final List<ExamPart> parts;

  ExamDetail({
    required this.id,
    required this.title,
    required this.code,
    required this.durationMinutes,
    required this.totalQuestions,
    required this.description,
    required this.parts,
  });

  factory ExamDetail.fromJson(Map<String, dynamic> json) {
    var partsList = (json['parts'] as List<dynamic>? ?? [])
        .map((e) => ExamPart.fromJson(e))
        .toList();

    return ExamDetail(
      id: json['id'] ?? 0,
      title: json['title'] ?? '',
      code: json['code'] ?? '',
      durationMinutes: json['durationMinutes'] ?? 75,
      totalQuestions: json['totalQuestions'] ?? 100,
      description: json['description'] ?? '',
      parts: partsList,
    );
  }
}

class ExamPart {
  final int id;
  final int partNumber;
  final String title;
  final String description;
  final int questionCount;
  final List<PassageGroup> passageGroups;
  final List<QuestionItem> questions;

  ExamPart({
    required this.id,
    required this.partNumber,
    required this.title,
    required this.description,
    required this.questionCount,
    required this.passageGroups,
    required this.questions,
  });

  factory ExamPart.fromJson(Map<String, dynamic> json) {
    var rawGroups = json['groups'] ?? json['passageGroups'];
    var pGroups = (rawGroups as List<dynamic>? ?? [])
        .map((e) => PassageGroup.fromJson(e))
        .toList();
    var qItems = (json['questions'] as List<dynamic>? ?? [])
        .map((e) => QuestionItem.fromJson(e))
        .toList();

    return ExamPart(
      id: json['id'] ?? 0,
      partNumber: json['partNumber'] ?? 0,
      title: json['title'] ?? '',
      description: json['description'] ?? '',
      questionCount: json['questionCount'] ?? 0,
      passageGroups: pGroups,
      questions: qItems,
    );
  }
}

class PassageGroup {
  final int id;
  final String groupRange;
  final String title;
  final String passageText;
  final String? imageUrl;
  final List<QuestionItem> questions;

  PassageGroup({
    required this.id,
    required this.groupRange,
    required this.title,
    required this.passageText,
    this.imageUrl,
    required this.questions,
  });

  factory PassageGroup.fromJson(Map<String, dynamic> json) {
    var qItems = (json['questions'] as List<dynamic>? ?? [])
        .map((e) => QuestionItem.fromJson(e))
        .toList();

    return PassageGroup(
      id: json['id'] ?? 0,
      groupRange: json['groupRange'] ?? '',
      title: json['title'] ?? '',
      passageText: json['passageText'] ?? '',
      imageUrl: json['imageUrl'],
      questions: qItems,
    );
  }
}

class QuestionItem {
  final int id;
  final int questionNumber;
  final String? questionText;
  final int? blankNumber;
  final String? explanation;
  final List<AnswerOption> options;

  QuestionItem({
    required this.id,
    required this.questionNumber,
    this.questionText,
    this.blankNumber,
    this.explanation,
    required this.options,
  });

  factory QuestionItem.fromJson(Map<String, dynamic> json) {
    var opts = (json['options'] as List<dynamic>? ?? [])
        .map((e) => AnswerOption.fromJson(e))
        .toList();

    return QuestionItem(
      id: json['id'] ?? 0,
      questionNumber: json['questionNumber'] ?? 0,
      questionText: json['questionText'],
      blankNumber: json['blankNumber'],
      explanation: json['explanation'],
      options: opts,
    );
  }
}

class AnswerOption {
  final int id;
  final String label;
  final String text;

  AnswerOption({
    required this.id,
    required this.label,
    required this.text,
  });

  factory AnswerOption.fromJson(Map<String, dynamic> json) {
    return AnswerOption(
      id: json['id'] ?? 0,
      label: json['label'] ?? '',
      text: json['text'] ?? '',
    );
  }
}

class ExamResultDto {
  final int submissionId;
  final String examTitle;
  final String studentName;
  final int totalQuestions;
  final int correctCount;
  final int incorrectCount;
  final int skippedCount;
  final int toeicScore;
  final double scorePercentage;
  final int timeSpentSeconds;
  final DateTime submissionTime;
  final List<QuestionDetailDto> details;

  ExamResultDto({
    required this.submissionId,
    required this.examTitle,
    required this.studentName,
    required this.totalQuestions,
    required this.correctCount,
    required this.incorrectCount,
    required this.skippedCount,
    required this.toeicScore,
    required this.scorePercentage,
    required this.timeSpentSeconds,
    required this.submissionTime,
    required this.details,
  });

  factory ExamResultDto.fromJson(Map<String, dynamic> json) {
    var rawList = json['details'] ?? json['questionReviews'] ?? [];
    var dtls = (rawList as List<dynamic>? ?? [])
        .map((e) => QuestionDetailDto.fromJson(e))
        .toList();

    final correct = json['correctCount'] ?? json['totalCorrect'] ?? 0;
    final total = json['totalQuestions'] ?? 100;
    final score = json['toeicScore'] ?? json['estimatedScore'] ?? 0;

    return ExamResultDto(
      submissionId: json['submissionId'] ?? 0,
      examTitle: json['examTitle'] ?? '',
      studentName: json['studentName'] != null && json['studentName'].toString().isNotEmpty
          ? json['studentName'].toString()
          : 'Thí sinh',
      totalQuestions: total,
      correctCount: correct,
      incorrectCount: json['incorrectCount'] ?? json['totalIncorrect'] ?? 0,
      skippedCount: json['skippedCount'] ?? json['totalSkipped'] ?? 0,
      toeicScore: score,
      scorePercentage: json['scorePercentage'] != null
          ? (json['scorePercentage'] as num).toDouble()
          : (total > 0 ? (correct / total) * 100 : 0.0),
      timeSpentSeconds: json['timeSpentSeconds'] ?? 0,
      submissionTime: DateTime.tryParse(json['submissionTime'] ?? '') ?? DateTime.now(),
      details: dtls,
    );
  }
}

class QuestionDetailDto {
  final int questionNumber;
  final String? userAnswer;
  final String correctAnswer;
  final bool isCorrect;
  final bool isSkipped;

  QuestionDetailDto({
    required this.questionNumber,
    this.userAnswer,
    required this.correctAnswer,
    required this.isCorrect,
    required this.isSkipped,
  });

  factory QuestionDetailDto.fromJson(Map<String, dynamic> json) {
    return QuestionDetailDto(
      questionNumber: json['questionNumber'] ?? 0,
      userAnswer: json['userAnswer'],
      correctAnswer: json['correctAnswer'] ?? '',
      isCorrect: json['isCorrect'] ?? false,
      isSkipped: json['isSkipped'] ?? false,
    );
  }
}
