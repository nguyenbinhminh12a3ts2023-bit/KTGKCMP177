import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open(r'C:/Users/BinhMinh/.gemini/antigravity/brain/fb785398-0065-41ab-b0fe-8a08eba98762/.system_generated/steps/45/output.txt', 'r', encoding='utf-8') as f:
    text = f.read()

frames = [
    ("Đề thi · TOEIC Reading", 5713),
    ("Chi tiết · Reading Test 01", 5786),
    ("Làm bài · Part 5", 5844),
    ("Làm bài · Part 6", 5907),
    ("Làm bài · Part 7", 5978),
    ("Tổng quan · Câu 101–200", 6071),
    ("Xác nhận · Nộp bài thi", 6426),
    ("Cảnh báo · Còn 5 phút", 6504),
    ("Hết giờ · Tự động nộp bài", 6572),
    ("Kết quả · Reading Test 01", 6634)
]

for title, fid in frames:
    print(f"\n==================== FRAME: {title} (#{fid}) ====================")
    pattern = rf'\[FRAME\] "{re.escape(title)}" #4:{fid}(.*?)(?=\n  \[FRAME\]|\Z)'
    match = re.search(pattern, text, re.DOTALL)
    if match:
        frame_content = match.group(1)
        texts = re.findall(r'\[TEXT\] .*? text="(.*?)"', frame_content)
        for t in texts:
            print(f" - {t}")
