import re

with open('part7_full_dump.txt', 'r', encoding='utf-8') as f:
    text = f.read()

# Normalize spacing like "15 3." or "1 53."
text = re.sub(r'(\d+)\s+(\d+)\.', r'\1\2.', text)

q_nums = re.findall(r'\b(1[4-9]\d|200)\.', text)
print(f"Found {len(set(q_nums))} unique Part 7 question numbers:", sorted([int(x) for x in set(q_nums)]))

missing = [n for n in range(147, 201) if str(n) not in q_nums]
print("Missing questions:", missing)
