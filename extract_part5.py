import pymupdf
import re
import json

doc = pymupdf.open('Reading v07.pdf')

# Part 5: Pages 2, 3, 4 (0-indexed: 1, 2, 3)
part5_text = ""
for pno in [1, 2, 3]:
    page = doc[pno]
    rect = page.rect
    words = page.get_text("words")
    if pno == 1:
        # Page 2 has header directions at the top
        # Questions start below y = 290
        q_words = [w for w in words if w[1] > 280]
    else:
        q_words = [w for w in words if w[1] < 740] # avoid footer
        
    left_words = [w for w in q_words if w[0] < rect.width * 0.48]
    right_words = [w for w in q_words if w[0] >= rect.width * 0.48]
    
    left_words.sort(key=lambda w: (round(w[1] / 12), w[0]))
    right_words.sort(key=lambda w: (round(w[1] / 12), w[0]))
    
    part5_text += " ".join([w[4] for w in left_words]) + "\n"
    part5_text += " ".join([w[4] for w in right_words]) + "\n"

# Fix common OCR spacing like "11 5." -> "115."
part5_text = re.sub(r'\b(\d)\s+(\d)\s+(\d)\.', r'\1\2\3.', part5_text)
part5_text = re.sub(r'\b(\d)\s+(\d)\.', r'\1\2.', part5_text)

# Let's see all found question numbers in part 5
q_nums = re.findall(r'(\d{3})\.', part5_text)
print("Part 5 extracted question numbers:", q_nums)

with open('part5_extracted.txt', 'w', encoding='utf-8') as f:
    f.write(part5_text)
