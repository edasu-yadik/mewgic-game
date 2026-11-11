"""Pack approved-reference costume renders. Input filenames match ShopItem IDs."""
import json
import sys
from pathlib import Path
import numpy as np
from PIL import Image
from scipy.ndimage import binary_dilation, label

src, dst = (Path(arg) for arg in sys.argv[1:])
dst.mkdir(parents=True, exist_ok=True)
cell_size = 400
pad = 4

def trim(cell):
    pixels = np.array(cell)
    alpha = pixels[:, :, 3]
    labels, count = label(alpha >= 120)
    if count == 0:
        raise ValueError("Missing opaque costume silhouette")
    area = np.bincount(labels.ravel())
    area[0] = 0
    core = labels == area.argmax()
    near = binary_dilation(core, iterations=7)
    connected = np.unique(labels[near])
    core |= np.isin(labels, connected[connected != 0])
    near = binary_dilation(core, iterations=5)
    pixels[:, :, 3] = np.where(near, np.clip((alpha.astype(float) - 9)*255/244, 0, 255), 0)
    pixels[pixels[:, :, 3] == 0, :3] = 0
    cleaned = Image.fromarray(pixels)
    box = cleaned.getbbox()
    if box is None or (box[2]-box[0])*(box[3]-box[1]) < 20000:
        raise ValueError("Pose too small")
    return cleaned.crop(box)

for source in sorted(src.glob("*Cat.png")):
    sheet = Image.open(source)
    if sheet.mode != "RGBA" or sheet.width != sheet.height:
        raise ValueError(f"Expected transparent square: {source}")
    cell = sheet.width / 3
    poses = [
        trim(sheet.crop((round(c*cell), round(r*cell), round((c+1)*cell), round((r+1)*cell))))
        for r in range(3) for c in range(3)
    ]
    # Poses in a short animation cycle share a canvas and a common paw baseline.
    groups = ((0,1,8), (2,3,7), (4,5), (6,))
    output = Image.new("RGBA", (cell_size*3, cell_size*3))
    frames = [None]*9
    for group in groups:
        width = max(poses[i].width for i in group)
        height = max(poses[i].height for i in group)
        scale = min(1, (cell_size - 2*pad) / max(width, height))
        frame_w, frame_h = round(width*scale)+2*pad, round(height*scale)+2*pad
        for i in group:
            image = poses[i]
            if scale < 1:
                image = image.resize((round(image.width*scale), round(image.height*scale)), Image.Resampling.LANCZOS)
            x = i % 3 * cell_size
            y = i // 3 * cell_size
            output.paste(image, (x+(frame_w-image.width)//2, y+frame_h-pad-image.height))
            frames[i] = dict(x=x, y=y, width=frame_w, height=frame_h)
    output.save(dst / source.name, optimize=True)
    (dst / (source.stem+"Frames.json")).write_text(json.dumps({"frames": frames}, indent=2)+"\n", encoding="utf-8")
    print(source.stem, [tuple(i.size) for i in poses])
