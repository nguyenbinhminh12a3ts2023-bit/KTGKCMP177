import pymupdf
doc = pymupdf.open('Reading v07.pdf')

for pno in range(4, 8):
    print(f"\n==================== Page {pno+1} ====================")
    page = doc[pno]
    rect = page.rect
    # Let's inspect text blocks
    blocks = page.get_text("blocks")
    for b in blocks:
        # filter small header/footer
        if b[4].strip() in ["TEST 7", str(207 + (pno-4))]:
            continue
        print(f"[{round(b[0])},{round(b[1])},{round(b[2])},{round(b[3])}]:")
        print(b[4].strip())
