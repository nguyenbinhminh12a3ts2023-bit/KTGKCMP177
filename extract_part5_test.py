import pymupdf
import re
import json

doc = pymupdf.open('Reading v07.pdf')

print("Total pages in Reading v07.pdf:", len(doc))

# Let's inspect pages by parts:
# Part 5: Page 2, 3, 4 (Questions 101 to 130)
# Part 6: Page 5, 6, 7, 8 (Questions 131 to 146)
# Part 7: Page 9 to 29 (Questions 147 to 200)

for pno in range(1, 4):
    print(f"--- Part 5 Page {pno+1} ---")
    page = doc[pno]
    rect = page.rect
    # PyMuPDF column extraction
    # Left column: x < rect.width / 2
    # Right column: x >= rect.width / 2
    words = page.get_text("words")
    left_words = [w for w in words if w[0] < rect.width * 0.48]
    right_words = [w for w in words if w[0] >= rect.width * 0.48]
    
    # Sort top to bottom
    left_words.sort(key=lambda w: (round(w[1] / 10), w[0]))
    right_words.sort(key=lambda w: (round(w[1] / 10), w[0]))
    
    left_text = " ".join([w[4] for w in left_words])
    right_text = " ".join([w[4] for w in right_words])
    
    print("LEFT:", left_text[:200])
    print("RIGHT:", right_text[:200])
