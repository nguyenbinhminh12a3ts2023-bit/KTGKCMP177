with open('part7_full_dump.txt', 'r', encoding='utf-8') as f:
    text = f.read()

for target in ['151', '155', '168', '200']:
    print(f"\n--- Context for {target} ---")
    pos = 0
    while True:
        idx = text.find(target, pos)
        if idx == -1:
            break
        print(repr(text[max(0, idx-40):min(len(text), idx+60)]))
        pos = idx + len(target)
