import re
import os
from rapidfuzz import fuzz
# pip install rapidfuzz

# -----------------------------
# SETTINGS
# -----------------------------
THRESHOLD = 75  # match percent (0-100)
MAX_TEXT_LEN = 120  # cliloc string max length

# -----------------------------
# TEXT CLEANING
# -----------------------------
def clean_text(text):
    text = text.lower()
    text = re.sub(r"%[sd]", "", text)  # %s %d remove
    text = re.sub(r"[^\w\s]", "", text)  # dot remove
    text = re.sub(r"\s+", " ", text).strip()
    return text


# -----------------------------
# CLILOC PARSE
# -----------------------------
def parse_cliloc(file_path):
    cliloc_dict = {}

    with open(file_path, "r", encoding="utf-8", errors="ignore") as f:
        content = f.read()

    blocks = content.split("*********************")

    for block in blocks:
        id_match = re.search(r"ID:\s*(\d+)", block)
        text_match = re.search(r"ID:\s*\d+\s*\n\n(.+)", block, re.DOTALL)

        if id_match and text_match:
            cid = id_match.group(1)
            text = text_match.group(1).strip()
            cliloc_dict[cid] = text

    return cliloc_dict


# -----------------------------
# BEST MATCH
# -----------------------------
def find_best_match(msg, cliloc_dict):
    best_id = None
    best_score = 0

    clean_msg = clean_text(msg)

    for cid, text in cliloc_dict.items():
        score = fuzz.ratio(clean_msg, clean_text(text))

        if score > best_score:
            best_score = score
            best_id = cid

    return best_id, best_score


# -----------------------------
# SPHERE_MSGS process
# -----------------------------
def process_sphere_msgs(input_file, output_file, cliloc_dict):
    inside_block = False
    new_lines = []

    with open(input_file, "r", encoding="utf-8", errors="ignore") as f:
        lines = f.readlines()

    for line in lines:
        stripped = line.strip()

        # block start
        if stripped.lower() == "[defmessage messages]":
            inside_block = True
            new_lines.append(line)
            continue

        # Exit when another block appears.
        if stripped.startswith("[") and inside_block and stripped.lower() != "[defmessage messages]":
            inside_block = False

        if inside_block and stripped.startswith("//") and '"' in line:
            msg_match = re.search(r'"(.+?)"', line)
            if msg_match:
                msg = msg_match.group(1)

                best_id, score = find_best_match(msg, cliloc_dict)

                if best_id and score >= THRESHOLD:
                    matched_text = cliloc_dict[best_id]

                    # uzun metni kısalt
                    if len(matched_text) > MAX_TEXT_LEN:
                        matched_text = matched_text[:MAX_TEXT_LEN] + "..."

                    line = line.rstrip() + f" // {best_id} // {matched_text}\n"

        new_lines.append(line)

    with open(output_file, "w", encoding="utf-8") as f:
        f.writelines(new_lines)


# -----------------------------
# MAIN
# -----------------------------
if __name__ == "__main__":
    base_dir = os.path.dirname(os.path.abspath(__file__))

    cliloc_file = os.path.join(base_dir, "cliloc.enu.txt")
    sphere_file = os.path.join(base_dir, "sphere_msgs.scp")
    output_file = os.path.join(base_dir, "sphere_msgs_out.scp")

    print("Working directory:", base_dir)

    print("Install cliloc...")
    cliloc_dict = parse_cliloc(cliloc_file)
    print(f"{len(cliloc_dict)} cliloc added.")

    print("Making matches...")
    process_sphere_msgs(sphere_file, output_file, cliloc_dict)

    print("Done!")
    print("Output:", output_file)