import docx
from docx.shared import Pt, RGBColor
import shutil

# Make a copy and introduce the 3 specific errors
doc = docx.Document()
doc.add_paragraph("[TEST]")
doc.add_paragraph("[TITLE: Reading Test 04]")
doc.add_paragraph("[TIME: 75]")
doc.add_paragraph("[TOTAL_QUESTIONS: 100]")
doc.add_paragraph("")

doc.add_paragraph("[PART: 5]")
doc.add_paragraph("[PART_TITLE: Incomplete Sentences]")

# Câu 101 - 111 normal
doc.add_paragraph("[QUESTION: 101]")
doc.add_paragraph("If ------- are not satisfied with an item, return it for a full refund within 30 days of purchase.")
doc.add_paragraph("[A] you")
doc.add_paragraph("[B] your")
doc.add_paragraph("[C] yours")
doc.add_paragraph("[D] yourself")
doc.add_paragraph("[ANSWER: A]")

# Lỗi 2: Câu 112 thiếu lựa chọn D
doc.add_paragraph("[QUESTION: 112]")
doc.add_paragraph("Ms. Valdez' sales numbers are good ------- for her to be considered for the employee-of-the-month award.")
doc.add_paragraph("[A] forward")
doc.add_paragraph("[B] even")
doc.add_paragraph("[C] ahead")
# Missing [D]
doc.add_paragraph("[ANSWER: D]")

doc.add_paragraph("[PART: 6]")
# Lỗi 1: Nhóm 131-134 thiếu thẻ [PASSAGE]
doc.add_paragraph("[GROUP: 131-134]")
doc.add_paragraph("[PASSAGE_TITLE: Global Strength Gym]")
# Missing [PASSAGE] tag
doc.add_paragraph("With Global Strength Gym's 30-day trial period, you get the opportunity to try our classes, equipment, and facilities. [BLANK: 131] It's completely risk-free!")
doc.add_paragraph("[QUESTION: 131]")
doc.add_paragraph("[A] Throughout the trial, you pay nothing and sign no contract.")
doc.add_paragraph("[B] Weight-lifting classes are not currently available.")
doc.add_paragraph("[C] A cash deposit is required when you sign up for membership.")
doc.add_paragraph("[D] All questions should be e-mailed to customerservice@gsgym.com.")
doc.add_paragraph("[ANSWER: A]")

doc.add_paragraph("[PART: 7]")
# Lỗi 3: Trùng số câu 147 (thiếu 148)
doc.add_paragraph("[GROUP: 147-148]")
doc.add_paragraph("[PASSAGE_TITLE: Sedwick Electronics]")
doc.add_paragraph("[PASSAGE]")
doc.add_paragraph("Sedwick Electronics is opening a new manufacturing facility...")
doc.add_paragraph("[/PASSAGE]")
doc.add_paragraph("[QUESTION: 147]")
doc.add_paragraph("For whom is the advertisement intended?")
doc.add_paragraph("[A] Recruiters")
doc.add_paragraph("[B] Job seekers")
doc.add_paragraph("[C] Local business owners")
doc.add_paragraph("[D] Current Sedwick Electronics employees")
doc.add_paragraph("[ANSWER: B]")

# Trùng câu 147 thay vì 148
doc.add_paragraph("[QUESTION: 147]")
doc.add_paragraph("What is stated about Sedwick Electronics?")
doc.add_paragraph("[A] It is moving its headquarters.")
doc.add_paragraph("[B] It offers a training program for new employees.")
doc.add_paragraph("[C] It requires employees to wear uniforms.")
doc.add_paragraph("[D] It will have more than one location.")
doc.add_paragraph("[ANSWER: D]")

doc.add_paragraph("[END_TEST]")
doc.save("DeThi_LoiDinhDang_Mau.docx")
print("Saved DeThi_LoiDinhDang_Mau.docx with sample errors matching Figma screen!")
