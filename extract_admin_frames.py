import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open(r'C:/Users/BinhMinh/.gemini/antigravity/brain/fb785398-0065-41ab-b0fe-8a08eba98762/.system_generated/steps/45/output.txt', 'r', encoding='utf-8') as f:
    text = f.read()

# Let's inspect sections between frames
frames = [
    ("Quản lý đề thi", 9787),
    ("Nhập đề từ Word", 9943),
    ("Lỗi định dạng", 10071),
    ("Kiểm tra thành công", 10191),
    ("Xem trước đề thi", 10337),
    ("Chỉnh sửa đề thi", 10505),
]

for title, fid in frames:
    print(f"\n==================== FRAME: {title} (#{fid}) ====================")
    # search where #4:{fid} starts in NODES
    pattern = rf'\[FRAME\] "{re.escape(title)}" #4:{fid}(.*?)(?=\n  \[FRAME\]|\Z)'
    match = re.search(pattern, text, re.DOTALL)
    if match:
        frame_content = match.group(1)
        # extract all [TEXT] contents
        texts = re.findall(r'\[TEXT\] .*? text="(.*?)"', frame_content)
        for t in texts:
            print(f" - {t}")
    else:
        print("Not found with regex, searching line by line")
