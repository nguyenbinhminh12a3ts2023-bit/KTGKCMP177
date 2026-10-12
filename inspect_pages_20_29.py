import pymupdf

doc = pymupdf.open('Reading v07.pdf')

for pno in range(19, len(doc)):
    print(f"\n==================== Page {pno+1} ====================")
    print(doc[pno].get_text())
