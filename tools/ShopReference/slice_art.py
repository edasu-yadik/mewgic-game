"""Slice decorative art from the four approved references and build a Unity UI atlas."""
import json
import math
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter

source, target = map(Path, sys.argv[1:])
references = [Image.open(next(source.glob(f"*20_0*-{n}.png"))).convert("RGBA") for n in range(1, 5)]
pieces = {}

def crop(name, ref, bounds, feather=0):
    image = references[ref - 1].crop(bounds)
    if feather:
        mask = Image.new("L", image.size)
        w, h = image.size
        inset = feather * 2
        ImageDraw.Draw(mask).rectangle((inset, inset, w - inset - 1, h - inset - 1), fill=255)
        image.putalpha(mask.filter(ImageFilter.GaussianBlur(feather)))
    pieces[name] = image

# Upper and lower decorative scenery, sampled without lettering or controls.
header = references[0].crop((0, 0, 941, 343))
mask = Image.new("L", header.size, 255)
cut = ImageDraw.Draw(mask)
for bounds in ((337, 8, 581, 128), (655, 25, 918, 93), (261, 148, 753, 246), (256, 253, 753, 328)):
    cut.rectangle(bounds, fill=0)
for y in range(331, 343):
    cut.line((0, y, 941, y), fill=int((342-y)*19))
header.putalpha(mask.filter(ImageFilter.GaussianBlur(3)))
pieces["scenery"] = header
crop("garden", 1, (0, 1548, 941, 1672))
garden_alpha = Image.new("L", pieces["garden"].size, 255)
for y in range(18):
    ImageDraw.Draw(garden_alpha).line((0, y, 941, y), fill=round(255*y/18))
pieces["garden"].putalpha(garden_alpha)
# Stretch only the light in front of the horizon on tall phones.
horizon = pieces["garden"].crop((0, 12, 941, 32)).resize((235, 1)).filter(ImageFilter.GaussianBlur(8))
mist = Image.new("RGBA", (235, 96))
for y in range(mist.height):
    for x in range(mist.width):
        r, g, b, _ = horizon.getpixel((x,0))
        mist.putpixel((x,y), (r,g,b,round(246*(y/95)**1.65)))
pieces["gardenMist"] = mist
crop("logo", 1, (327, 2, 599, 141), 6)

def label_free_plate(name, bounds, label):
    plate = references[0].crop(bounds)
    left, top, right, bottom = label
    inset = Image.new("RGBA", plate.size)
    pixels = inset.load()
    for y in range(top, bottom):
        a = plate.getpixel((left - 1, y))
        b = plate.getpixel((right, y))
        for x in range(left, right):
            t = (x - left) / (right - left - 1)
            pixels[x,y] = tuple(round(start*(1-t)+end*t) for start,end in zip(a,b))
    alpha = Image.new("L", plate.size)
    ImageDraw.Draw(alpha).rectangle((left + 3, top + 2, right - 4, bottom - 3), fill=255)
    inset.putalpha(alpha.filter(ImageFilter.GaussianBlur(3)))
    plate.alpha_composite(inset)
    pieces[name] = plate

label_free_plate("heading", (257,143,757,248), (130,17,471,77))
label_free_plate("tagline", (250,248,755,333), (76,12,433,74))
crop("laurelLeft", 3, (410,549,442,605), 2)
crop("laurelRight", 3, (645,549,677,605), 2)
crop("coin", 1, (401, 575, 456, 626), 1)
crop("paw", 1, (277, 269, 322, 319), 1)
crop("crown", 1, (111, 350, 182, 399), 1)
crop("stack", 1, (341, 350, 411, 401), 1)
crop("potion", 1, (566, 346, 621, 399), 1)
crop("shirt", 1, (775, 349, 840, 400), 1)
crop("starter", 1, (37, 589, 384, 824), 6)
crop("powerpack", 1, (27, 878, 360, 1153), 5)
crop("gold100", 1, (68, 1275, 270, 1401), 5)
crop("gold250", 1, (359, 1293, 574, 1401), 4)
crop("gold1000", 1, (663, 1293, 890, 1401), 4)
crop("gold2500", 2, (136, 1209, 343, 1288), 4)
crop("gold5000", 2, (581, 1209, 823, 1289), 4)
crop("gold10000", 2, (38, 581, 457, 832), 6)
crop("wizard", 3, (30, 479, 394, 753), 5)
crop("diamond", 3, (89, 1035, 143, 1077))
def silhouette(name, points):
    image = pieces[name]
    mask = Image.new("L", image.size)
    ImageDraw.Draw(mask).polygon(points, fill=255)
    image.putalpha(mask.filter(ImageFilter.GaussianBlur(.85)))

silhouette("diamond", [(2, 17), (12, 3), (42, 3), (52, 17), (26, 41)])
silhouette("coin", [(9, 10), (22, 3), (38, 5), (47, 15), (49, 29), (42, 42),
                    (30, 47), (15, 44), (7, 37), (3, 24)])
silhouette("crown", [(6,9),(14,5),(23,15),(29,11),(31,1),(43,2),(46,14),
                     (55,13),(61,6),(69,10),(63,23),(60,43),(15,46),(12,32)])
silhouette("stack", [(4,31),(18,20),(17,11),(23,3),(44,2),(59,10),
                     (59,24),(66,29),(69,40),(61,48),(35,48),(16,51),(3,44)])
silhouette("potion", [(15,2),(36,2),(39,12),(36,19),(48,27),(50,39),
                       (43,49),(29,52),(14,50),(5,43),(4,30),(15,21),(19,14)])
silhouette("shirt", [(2,14),(16,3),(26,2),(32,9),(42,1),(50,4),(62,16),
                      (57,28),(50,27),(51,46),(47,50),(16,50),(12,44),(15,28),(8,27)])
for name, bounds in {
    "princess":(29,833,298,1034), "explorer":(333,833,609,1034), "crystal":(640,831,917,1034),
    "night":(29,1207,298,1422), "fairy":(333,1207,609,1422), "royal":(640,1207,917,1422)
}.items():
    crop(name, 3, bounds, 4)
crop("powers", 4, (35, 462, 296, 628), 4)
for name, y in zip(("move", "extra", "hint", "shuffle", "swap", "hammer"), (641, 796, 951, 1106, 1261, 1416)):
    crop(name, 4, (26, y + 5, 239, y + 139), 4)

def surface(name, size, radius, top, bottom, edge, halo, border=4):
    w, h = size
    mask = Image.new("L", size)
    bounds = (8, 8, w - 9, h - 9)
    ImageDraw.Draw(mask).rounded_rectangle(bounds, radius, fill=255)
    tint = Image.new("RGBA", size, halo)
    tint.putalpha(mask.filter(ImageFilter.GaussianBlur(7)).point(lambda n: int(n * .92)))
    base = Image.new("RGBA", size)
    draw = ImageDraw.Draw(base)
    for y in range(h):
        t = max(0, min(1, (y - 8) / (h - 16)))
        color = tuple(round(a * (1 - t) + b * t) for a, b in zip(top, bottom))
        draw.line((0, y, w, y), fill=color)
    draw.rounded_rectangle(bounds, radius, outline=edge, width=border)
    draw.rounded_rectangle((10, 9, w - 11, h - 11), radius-2, outline=(*edge[:3], 255), width=1)
    shine = (255, 250, 203, 255) if name in {"panel", "active", "green"} else edge
    draw.line((radius+13, 9, w-radius-14, 9), fill=shine, width=2)
    draw.line((radius+13, h-11, w-radius-14, h-11), fill=shine, width=2)
    if name in {"panel", "active"}:
        rim = Image.new("L", size)
        ImageDraw.Draw(rim).rounded_rectangle((12, 12, w-13, h-13), radius-4, outline=200, width=5)
        reflected = Image.new("RGBA", size, (255, 140, 207))
        reflected.putalpha(rim.filter(ImageFilter.GaussianBlur(4)))
        base.alpha_composite(reflected)
    base.putalpha(mask)
    tint.alpha_composite(base)
    pieces[name] = tint

surface("panel", (176,176), 48, (75,23,114,255), (137,58,158,255), (255,220,52,255), (255,153,237,255))
surface("tab", (104,104), 26, (229,217,252,255), (253,213,220,255), (255,234,249,255), (247,185,242,255), 2)
surface("active", (104,104), 26, (151,6,202,255), (75,0,106,255), (255,224,43,255), (255,106,206,255))
surface("cream", (104,104), 43, (255,237,239,255), (255,214,125,255), (255,230,183,255), (255,169,81,255), 2)
# Reuse the approved pill edges. Fill only the separate UI label's former rectangle
# with a clean sample from the same row before slicing the sprite.
green = references[3].crop((643, 706, 900, 777))
for y in range(14, 51):
    green.paste(green.getpixel((49, y)), (62, y, 202, y + 1))
mask = Image.new("L", green.size)
ImageDraw.Draw(mask).rounded_rectangle((3, 4, 253, 67), radius=31, fill=255)
green.putalpha(mask.filter(ImageFilter.GaussianBlur(1.3)))
pieces["green"] = green
surface("ribbon", (104,104), 16, (251,13,137,255), (153,0,43,255), (255,136,195,255), (255,72,144,255), 2)
ribbon = Image.new("RGBA", (200, 80))
d = ImageDraw.Draw(ribbon)
for polygon in (((8,17),(52,18),(49,74),(8,71),(22,45)),
                ((192,17),(148,18),(151,74),(192,71),(178,45))):
    d.polygon(polygon, fill=(172,3,55), outline=(255,123,165), width=2)
d.polygon(((29,14),(57,20),(48,70),(32,66)), fill=(113,1,46))
d.polygon(((171,14),(143,20),(152,70),(168,66)), fill=(113,1,46))
ribbon.alpha_composite(pieces["ribbon"].resize((157,73), Image.Resampling.LANCZOS), (21,0))
pieces["ribbon"] = ribbon
surface("badge", (104,104), 43, (249,216,255,255), (228,178,250,255), (255,230,255,255), (219,115,219,255), 2)
# Warm reflected light follows the long edge of the reference cards.
glow = Image.new("RGBA", (400, 64))
pixels = glow.load()
for y in range(glow.height):
    for x in range(glow.width):
        u = (x - 199.5) / 200
        v = (y - 25) / 13
        alpha = math.exp(-2.9 * u * u - v * v) * max(0, 1 - u*u)
        pixels[x, y] = (255, 195 + round(42 * alpha), 235, round(235 * alpha))
pieces["edgeglow"] = glow
point = Image.new("RGBA", (46, 26))
mask = Image.new("L", point.size)
diamond_tip = [(7,5), (39,5), (23,20)]
ImageDraw.Draw(mask).polygon(diamond_tip, fill=255)
point.paste((255,143,67), (0,0,46,26))
point.putalpha(mask.filter(ImageFilter.GaussianBlur(4)))
ImageDraw.Draw(point).polygon(diamond_tip, fill=(255,236,107))
ImageDraw.Draw(point).polygon([(10,5),(36,5),(23,14)], fill=(111,0,146))
pieces["tabPoint"] = point

discount = Image.new("RGBA", (168, 124))
shape = []
for i in range(160):
    angle = math.tau * i / 160
    scale = 1 + .045 * math.cos(angle * 14)
    shape.append((84 + math.cos(angle)*75*scale, 62 + math.sin(angle)*53*scale))
mask = Image.new("L", discount.size)
ImageDraw.Draw(mask).polygon(shape, fill=255)
halo = Image.new("RGBA", discount.size, (255,96,68))
halo.putalpha(mask.filter(ImageFilter.GaussianBlur(6)))
d = ImageDraw.Draw(discount)
for y in range(124):
    t = y / 123
    d.line((0,y,168,y), fill=(round(252-73*t), round(16-9*t), round(127-79*t)))
d.line(shape+[shape[0]], fill=(255,193,100), width=3)
discount.putalpha(mask)
halo.alpha_composite(discount)
pieces["discount"] = halo

# Decorative lower fills are translucent so the sky continues around the cards.
back = Image.new("RGBA", (32, 256))
draw = ImageDraw.Draw(back)
for y in range(256):
    t = y / 255
    draw.line((0, y, 32, y), fill=(round(18 + 119*t), round(67 + 55*t), round(174 + 70*t), 255))
pieces["back"] = back

atlas = Image.new("RGBA", (2048, 2048))
regions = []
x = y = row = 0
for name, image in sorted(pieces.items(), key=lambda pair: -pair[1].height):
    if x + image.width + 4 > atlas.width:
        x, y, row = 0, y + row + 4, 0
    if y + image.height > atlas.height:
        raise ValueError(name)
    atlas.paste(image, (x, y))
    regions.append(dict(name=name, x=x, y=y, width=image.width, height=image.height,
                        border={"panel":60,"active":34,"tab":34,"green":48,"cream":48,"ribbon":34,"badge":48}.get(name,0)))
    x += image.width + 4
    row = max(row, image.height)
atlas.save(target / "Art.png", optimize=True)
(target / "ArtRegions.json").write_text(json.dumps(dict(regions=regions), indent=2) + "\n", encoding="utf-8")
