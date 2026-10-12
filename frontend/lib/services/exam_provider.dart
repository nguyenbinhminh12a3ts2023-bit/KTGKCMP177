import 'dart:async';
import 'package:flutter/foundation.dart';
import '../models/exam_models.dart';
import '../services/api_service.dart';

class ExamProvider with ChangeNotifier {
  ExamDetail? _exam;
  bool _isLoading = false;
  String? _errorMessage;

  // Answers map: QuestionNumber (int) -> OptionLabel (String: 'A','B','C','D')
  final Map<int, String> _answers = {};

  // Flattened list of 100 questions for quick index lookup
  final List<QuestionItem> _allQuestions = [];
  int _currentIndex = 0; // 0 to 99

  // Timer: 75 minutes = 4500 seconds
  int _remainingSeconds = 75 * 60;
  Timer? _timer;
  bool _isFinished = false;
  bool _warnedAt5Min = false;

  // Exam result after submission
  ExamResultDto? _result;

  // Getters
  ExamDetail? get exam => _exam;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;
  Map<int, String> get answers => _answers;
  List<QuestionItem> get allQuestions => _allQuestions;
  int get currentIndex => _currentIndex;
  QuestionItem? get currentQuestion =>
      _allQuestions.isNotEmpty && _currentIndex < _allQuestions.length
          ? _allQuestions[_currentIndex]
          : null;
  int get remainingSeconds => _remainingSeconds;
  bool get isFinished => _isFinished;
  bool get warnedAt5Min => _warnedAt5Min;
  ExamResultDto? get result => _result;

  int get answeredCount => _answers.length;
  int get totalQuestionsCount => _allQuestions.isNotEmpty ? _allQuestions.length : 100;

  // Question status helpers
  bool isQuestionAnswered(int qNumber) => _answers.containsKey(qNumber);
  String? getAnswerFor(int qNumber) => _answers[qNumber];

  Future<void> loadExam(int examId) async {
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();

    try {
      _exam = await ApiService.getExamDetail(examId);
      _buildAllQuestionsList();
      _remainingSeconds = (_exam?.durationMinutes ?? 75) * 60;
      _answers.clear();
      _currentIndex = 0;
      _isFinished = false;
      _warnedAt5Min = false;
      _isLoading = false;
      notifyListeners();
    } catch (e) {
      _isLoading = false;
      _errorMessage = e.toString();
      notifyListeners();
    }
  }

  void _buildAllQuestionsList() {
    _allQuestions.clear();
    if (_exam == null) return;

    for (var part in _exam!.parts) {
      if (part.partNumber == 5) {
        _allQuestions.addAll(part.questions);
      } else {
        for (var group in part.passageGroups) {
          _allQuestions.addAll(group.questions);
        }
      }
    }
    _allQuestions.sort((a, b) => a.questionNumber.compareTo(b.questionNumber));
  }

  void startTimer({VoidCallback? on5MinWarning, VoidCallback? onTimeUp}) {
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_remainingSeconds > 0) {
        _remainingSeconds--;
        if (_remainingSeconds == 5 * 60 && !_warnedAt5Min) {
          _warnedAt5Min = true;
          on5MinWarning?.call();
        }
        notifyListeners();
      } else {
        _timer?.cancel();
        _isFinished = true;
        notifyListeners();
        onTimeUp?.call();
      }
    });
  }

  void selectAnswer(int questionNumber, String optionLabel) {
    if (_isFinished) return;
    _answers[questionNumber] = optionLabel;
    notifyListeners();
  }

  void setCurrentIndex(int index) {
    if (index >= 0 && index < _allQuestions.length) {
      _currentIndex = index;
      notifyListeners();
    }
  }

  void goToQuestionNumber(int questionNumber) {
    final idx = _allQuestions.indexWhere((q) => q.questionNumber == questionNumber);
    if (idx != -1) {
      _currentIndex = idx;
      notifyListeners();
    }
  }

  void nextQuestion() {
    if (_currentIndex < _allQuestions.length - 1) {
      _currentIndex++;
      notifyListeners();
    }
  }

  void previousQuestion() {
    if (_currentIndex > 0) {
      _currentIndex--;
      notifyListeners();
    }
  }

  PassageGroup? getPassageGroupForQuestion(QuestionItem q) {
    if (_exam == null) return null;
    for (var part in _exam!.parts) {
      for (var group in part.passageGroups) {
        if (group.questions.any((item) => item.id == q.id || item.questionNumber == q.questionNumber)) {
          return group;
        }
      }
    }
    return null;
  }

  int getPartNumberForQuestion(int qNumber) {
    if (qNumber <= 130) return 5;
    if (qNumber <= 146) return 6;
    return 7;
  }

  Future<ExamResultDto> submitExam({String studentName = 'Thí sinh'}) async {
    _timer?.cancel();
    _isFinished = true;
    _isLoading = true;
    notifyListeners();

    try {
      final examId = _exam?.id ?? 1;
      final timeSpent = (_exam?.durationMinutes ?? 75) * 60 - _remainingSeconds;

      final answersMap = <String, String>{};
      _answers.forEach((k, v) {
        answersMap[k.toString()] = v;
      });

      _result = await ApiService.submitExam(
        examId: examId,
        studentName: studentName,
        timeSpentSeconds: timeSpent,
        answers: answersMap,
      );
      _isLoading = false;
      notifyListeners();
      return _result!;
    } catch (e) {
      _isLoading = false;
      _errorMessage = e.toString();
      notifyListeners();
      rethrow;
    }
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }
}
