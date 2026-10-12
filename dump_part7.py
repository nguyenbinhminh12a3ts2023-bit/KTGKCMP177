import pymupdf, re

doc = pymupdf.open('Reading v07.pdf')

with open('part7_full_dump.txt', 'w', encoding='utf-8') as f:
    for pno in range(8, len(doc)):
        f.write(f"\n==================== PAGE {pno+1} ====================\n")
        f.write(doc[pno].get_text())

print("Part 7 dumped successfully.")
