"""
make_contact_sheet.py
Create a single contact-sheet PNG showing all 12 rendered views in a grid,
so the user can quickly verify the test data looks correct.
"""
import os
import json
import math
import numpy as np
from PIL import Image

OUT_DIR = "/home/z/my-project/download/test_data"
NUM_VIEWS = 12
COLS = 4
ROWS = 3
PAD = 8
LABEL_H = 18

with open(os.path.join(OUT_DIR, "cameras.json")) as f:
    cams = json.load(f)
intr = cams["intrinsics"]
cams = cams["cameras"]

W, H = intr["width"], intr["height"]
sheet_w = COLS * W + (COLS + 1) * PAD
sheet_h = ROWS * (H + LABEL_H) + (ROWS + 1) * PAD

sheet = Image.new("RGB", (sheet_w, sheet_h), (32, 32, 32))

from PIL import ImageDraw, ImageFont
draw = ImageDraw.Draw(sheet)
try:
    font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", 12)
except Exception:
    font = ImageFont.load_default()

for i in range(NUM_VIEWS):
    img = Image.open(os.path.join(OUT_DIR, f"view_{i:02d}.png")).convert("RGB")
    r, c = divmod(i, COLS)
    x = PAD + c * (W + PAD)
    y = PAD + r * (H + LABEL_H + PAD)
    cam = cams[i]
    label = f"view_{i:02d}  angle={cam['angle_deg']:.0f}  eye=({cam['eye'][0]:+.1f},{cam['eye'][1]:+.1f},{cam['eye'][2]:+.1f})"
    draw.text((x, y), label, fill=(255, 255, 255), font=font)
    sheet.paste(img, (x, y + LABEL_H))

out_path = os.path.join(OUT_DIR, "contact_sheet.png")
sheet.save(out_path)
print(f"Contact sheet saved to: {out_path}")
print(f"  Size: {sheet_w} x {sheet_h}")
