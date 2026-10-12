import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/exam_models.dart';

class ApiService {
  // Use http://localhost:5000 for Chrome web & desktop to avoid self-signed SSL errors
  static String baseUrl = 'http://localhost:5000/api/toeicapi';

  static void setBaseUrl(String url) {
    baseUrl = url;
  }

  static Future<List<ExamListItem>> getExams() async {
    try {
      final response = await http.get(Uri.parse('$baseUrl/exams'));
      if (response.statusCode == 200) {
        final body = jsonDecode(response.body);
        final list = (body['result']?['data'] ?? body['data']) as List<dynamic>;
        return list.map((e) => ExamListItem.fromJson(e)).toList();
      } else {
        throw Exception('Failed to load exams: ${response.statusCode}');
      }
    } catch (e) {
      // Return offline mock or rethrow
      rethrow;
    }
  }

  static Future<ExamDetail> getExamDetail(int examId) async {
    final response = await http.get(Uri.parse('$baseUrl/exam/$examId'));
    if (response.statusCode == 200) {
      final body = jsonDecode(response.body);
      final data = body['result']?['data'] ?? body['data'];
      return ExamDetail.fromJson(data);
    } else {
      throw Exception('Failed to load exam detail: ${response.statusCode}');
    }
  }

  static Future<ExamResultDto> submitExam({
    required int examId,
    required String studentName,
    required int timeSpentSeconds,
    required Map<String, String> answers,
  }) async {
    final payload = {
      'examId': examId,
      'studentName': studentName,
      'timeSpentSeconds': timeSpentSeconds,
      'answers': answers,
    };

    final response = await http.post(
      Uri.parse('$baseUrl/submit'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(payload),
    );

    if (response.statusCode == 200) {
      final body = jsonDecode(response.body);
      final data = body['result']?['data'] ?? body['data'];
      return ExamResultDto.fromJson(data);
    } else {
      throw Exception('Failed to submit exam: ${response.statusCode}');
    }
  }

  static Future<ExamResultDto> getResult(int submissionId) async {
    final response = await http.get(Uri.parse('$baseUrl/result/$submissionId'));
    if (response.statusCode == 200) {
      final body = jsonDecode(response.body);
      final data = body['result']?['data'] ?? body['data'];
      return ExamResultDto.fromJson(data);
    } else {
      throw Exception('Failed to load result: ${response.statusCode}');
    }
  }
}
