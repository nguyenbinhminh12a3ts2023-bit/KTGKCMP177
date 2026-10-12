import pymupdf, re
doc = pymupdf.open('Reading v07.pdf')

print("=== PART 7 INSPECTION ===")
for pno in range(8, len(doc)):
    page = doc[pno]
    text = page.get_text()
    matches = re.findall(r'Questions?\s+(\d+)\s*[-–]\s*(\d+)\s+refer to the following\s+([^\.\n]+)', text, re.IGNORECASE)
    if matches:
        for m in matches:
            print(f"Page {pno+1}: Q{m[0]}-{m[1]} -> {m[2].strip()}")
    else:
        # Check single question or continuing
        q_single = re.findall(r'(\d{3})\.\s+([A-Z][^\n]+)', text)
        print(f"Page {pno+1}: (No header match) Found Qs: {[q[0] for q in q_single]}")
