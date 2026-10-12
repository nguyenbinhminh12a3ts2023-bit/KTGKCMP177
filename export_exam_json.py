import json
from create_toeic_docx import QUESTIONS_DATA, PART6_PASSAGES, PART7_PASSAGES

exam_data = {
    "title": "Reading Test 07",
    "description": "Bài thi TOEIC Reading chuẩn gồm 100 câu hỏi chia làm 3 phần (Part 5, Part 6, Part 7) theo format quốc tế ETS.",
    "durationMinutes": 75,
    "totalQuestions": 100,
    "parts": [
        {
            "partNumber": 5,
            "title": "Part 5 · Incomplete Sentences",
            "description": "Directions: A word or phrase is missing in each of the sentences below. Four answer choices are given below each sentence. Select the best answer to complete the sentence.",
            "questionRange": "101–130",
            "questionCount": 30,
            "questions": [
                {
                    "number": qnum,
                    "text": QUESTIONS_DATA[qnum]["text"],
                    "options": {
                        "A": QUESTIONS_DATA[qnum]["A"],
                        "B": QUESTIONS_DATA[qnum]["B"],
                        "C": QUESTIONS_DATA[qnum]["C"],
                        "D": QUESTIONS_DATA[qnum]["D"]
                    },
                    "correctAnswer": QUESTIONS_DATA[qnum]["ans"]
                }
                for qnum in range(101, 131)
            ]
        },
        {
            "partNumber": 6,
            "title": "Part 6 · Text Completion",
            "description": "Directions: Read the texts that follow. A word, phrase, or sentence is missing in parts of each text. Four answer choices for each question are given below the text. Select the best answer to complete the text.",
            "questionRange": "131–146",
            "questionCount": 16,
            "groups": [
                {
                    "groupRange": grp_key,
                    "title": PART6_PASSAGES[grp_key]["title"],
                    "passage": PART6_PASSAGES[grp_key]["text"],
                    "questions": [
                        {
                            "number": qnum,
                            "text": f"Chỗ trống [{qnum}]",
                            "options": {
                                "A": QUESTIONS_DATA[qnum]["A"],
                                "B": QUESTIONS_DATA[qnum]["B"],
                                "C": QUESTIONS_DATA[qnum]["C"],
                                "D": QUESTIONS_DATA[qnum]["D"]
                            },
                            "correctAnswer": QUESTIONS_DATA[qnum]["ans"]
                        }
                        for qnum in range(int(grp_key.split("-")[0]), int(grp_key.split("-")[1]) + 1)
                    ]
                }
                for grp_key in ["131-134", "135-138", "139-142", "143-146"]
            ]
        },
        {
            "partNumber": 7,
            "title": "Part 7 · Reading Comprehension",
            "description": "Directions: In this part you will read a selection of texts, such as magazine and newspaper articles, e-mails, and instant messages. Each text or set of texts is followed by several questions. Select the best answer for each question.",
            "questionRange": "147–200",
            "questionCount": 54,
            "groups": [
                {
                    "groupRange": grp_key,
                    "title": PART7_PASSAGES[grp_key]["title"],
                    "passage": PART7_PASSAGES[grp_key]["text"],
                    "questions": [
                        {
                            "number": qnum,
                            "text": QUESTIONS_DATA[qnum]["text"],
                            "options": {
                                "A": QUESTIONS_DATA[qnum]["A"],
                                "B": QUESTIONS_DATA[qnum]["B"],
                                "C": QUESTIONS_DATA[qnum]["C"],
                                "D": QUESTIONS_DATA[qnum]["D"]
                            },
                            "correctAnswer": QUESTIONS_DATA[qnum]["ans"]
                        }
                        for qnum in range(int(grp_key.split("-")[0]), int(grp_key.split("-")[1]) + 1)
                    ]
                }
                for grp_key in [
                    "147-148", "149-150", "151-152", "153-154", "155-157",
                    "158-160", "161-163", "164-167", "168-171", "172-175",
                    "176-180", "181-185", "186-190", "191-195", "196-200"
                ]
            ]
        }
    ]
}

with open("toeic_reading_test_07.json", "w", encoding="utf-8") as f:
    json.dump(exam_data, f, ensure_ascii=False, indent=2)

print("Saved toeic_reading_test_07.json successfully!")
