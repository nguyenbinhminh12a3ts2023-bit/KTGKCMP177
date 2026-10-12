import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open(r'C:/Users/BinhMinh/.gemini/antigravity/brain/fb785398-0065-41ab-b0fe-8a08eba98762/.system_generated/steps/45/output.txt', 'r', encoding='utf-8') as f:
    text = f.read()

# Find frames and their titles
frame_matches = re.findall(r'\[FRAME\] "(.*?)" #(.*?)\s', text)
print("=== FRAMES FOUND ===")
for title, node_id in frame_matches:
    print(f"{node_id}: {title}")

# Also find tag formats mentioned in the Figma file
print("\n=== TAG FORMATS OR EXAMPLES IN FIGMA ===")
for line in text.splitlines():
    if any(k in line for k in ['[PART', '[QUESTION', '[CHOICE', '[PASSAGE', '[IMAGE', '[TAG', 'TAG', 'tag', '.docx', 'Word']):
        print(line.strip())
