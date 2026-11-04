"""Compose store assets from approved mascot art and archived Unity Game View captures."""
from pathlib import Path
import json
import re
import math
from PIL import Image, ImageChops, ImageDraw, ImageFont, ImageOps

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Store Assets"
ART = ROOT / "Assets/Art"
REPORTS = ROOT / "docs/ProductionReports"
RESAMPLE = Image.Resampling.LANCZOS
CAT = ART / "Characters/WizardCat/MageCat_NoWand_v001.png"
LOGO = ART / "UI/Logo/Mewgic_Logo_Main.png"
TITLE = ART / "Typography/MewgicTitle.ttf"
BODY = ART / "Typography/MewgicBody.ttf"

CARDS = [
    ("01_Puzzles", "Pour, sort, solve", "Colorful liquid puzzles", "Part18/en/Gameplay.jpg"),
    ("02_Mascot", "Meet your wizard cat", "Your companion in a world of magic", "Part18/en/Home.jpg"),
    ("03_World", "Follow the magic", "Explore a fantasy island map", "Part18/en/Map.jpg"),
    ("04_Progress", "One stone at a time", "Unlock levels and improve your stars", "Part18/en/Island.jpg"),
    ("05_Rewards", "Earn magical rewards", "Collect coins, stars and daily gifts", "Part18/en/Victory.jpg"),
    ("06_Islands", "Discover 12 islands", "Distinct landscapes along your journey", None),
]


def rgb(path):
    return Image.open(path).convert("RGB")


def space(size):
    w, h = size
    img = Image.new("RGB", size)
    pixels = []
    for y in range(h):
        for x in range(w):
            radius = min(1, math.hypot((x/w-.52)*1.55, (y/h-.37)*1.55))
            glow = (1-radius)**1.8
            pixels.append((int(14+64*glow), int(10+29*glow), int(37+92*glow)))
    img.putdata(pixels)
    return img


def stars(img):
    w, h = img.size
    draw = ImageDraw.Draw(img)
    for x, y, r in [(0.13,.27,.017),(.87,.24,.013),(.91,.47,.008),(.08,.54,.007),(.19,.73,.012),(.82,.74,.01)]:
        x, y, r = x*w, y*h, r*min(w,h)
        draw.polygon([(x,y-r),(x+r*.23,y-r*.23),(x+r,y),(x+r*.23,y+r*.23),
                      (x,y+r),(x-r*.23,y+r*.23),(x-r,y),(x-r*.23,y-r*.23)], fill="#e7bd77")


def place_art(canvas, path, box):
    art = Image.open(path).convert("RGBA")
    art = art.crop(art.getbbox())
    art.thumbnail(box[2:], RESAMPLE)
    canvas.paste(art, (box[0]+(box[2]-art.width)//2, box[1]+(box[3]-art.height)//2), art)


def icon():
    folder = ART / "Store"
    folder.mkdir(parents=True, exist_ok=True)
    bg = space((1024,1024))
    stars(bg)
    foreground = Image.new("RGBA",bg.size)
    cat = Image.open(CAT).convert("RGBA").crop((15,25,1100,930))
    cat.thumbnail((880,810),RESAMPLE)
    alpha=cat.getchannel("A")
    fade=Image.new("L",cat.size)
    fade.putdata([min(255, round((cat.height-1-y)*255/max(1,cat.height*.13)))
                  for y in range(cat.height) for _ in range(cat.width)])
    cat.putalpha(ImageChops.multiply(alpha,fade))
    foreground.paste(cat,((1024-cat.width)//2,132))
    adaptive = Image.new("RGBA",bg.size)
    safe = foreground.resize((760,760),RESAMPLE)
    adaptive.paste(safe,(132,132))
    adaptive.save(folder / "Mewgic_AdaptiveForeground.png", optimize=True)
    bg.save(folder / "Mewgic_AdaptiveBackground.png", optimize=True)
    full = bg.copy()
    full.paste(foreground,(0,0),foreground)
    full.save(folder / "Mewgic_Icon1024.png", optimize=True)
    (OUT/"AppStore").mkdir(parents=True,exist_ok=True)
    (OUT/"GooglePlay").mkdir(parents=True,exist_ok=True)
    full.save(OUT/"AppStore/AppIcon-1024.png", optimize=True)
    full.resize((512,512),RESAMPLE).convert("RGBA").save(OUT/"GooglePlay/Icon-512.png",optimize=True)


def centered(draw, text, font_file, font_size, center, fill, width):
    font = ImageFont.truetype(str(font_file),font_size)
    while draw.textlength(text,font=font)>width:
        font_size-=1
        font=ImageFont.truetype(str(font_file),font_size)
    draw.text(center,text,font=font,fill=fill,anchor="mm")


def shot(canvas, path, box, crop=False):
    x,y,w,h=box
    img=rgb(path)
    img=ImageOps.fit(img,(w,h),RESAMPLE,centering=(.5,.43)) if crop else ImageOps.contain(img,(w,h),RESAMPLE)
    x+=(w-img.width)//2
    y+=(h-img.height)//2
    ImageDraw.Draw(canvas).rounded_rectangle((x-3,y-3,x+img.width+3,y+img.height+3),radius=8,fill="#ce9a50")
    canvas.paste(img,(x,y))


def gallery(folder, size):
    folder.mkdir(parents=True,exist_ok=True)
    w,h=size
    template=space(size)
    stars(template)
    for index,(name, headline, subtitle, source) in enumerate(CARDS,1):
        canvas=template.copy()
        draw=ImageDraw.Draw(canvas)
        draw.line((w*.36,h*.047,w*.64,h*.047),fill="#cf9e58",width=max(2,w//400))
        centered(draw,headline,TITLE,round(w*.087),(w//2,round(h*.09)),"#fff1cd",w*.89)
        centered(draw,subtitle,BODY,round(w*.033),(w//2,round(h*.136)),"#eadbf7",w*.90)
        if source:
            shot(canvas,REPORTS/source,(round(w*.06),round(h*.173),round(w*.88),round(h*.755)))
        else:
            for i in range(12):
                col,row=i%3,i//3
                box=(round(w*(.065+.294*col)),round(h*(.177+.185*row)),round(w*.275),round(h*.171))
                shot(canvas,REPORTS/"Part15F"/f"Ada_{i+1:02}.png",box,True)
        centered(draw,f"MEWGIC   /   {index:02}",BODY,round(w*.021),(w//2,round(h*.964)),"#dcc08c",w*.9)
        canvas.save(folder/(name+".jpg"),quality=92,subsampling=0,optimize=True)


def feature():
    size=(1024,500)
    canvas=space(size)
    landscape=ImageOps.fit(rgb(REPORTS/"Part18/en/Map.jpg"),(520,500),RESAMPLE,centering=(.5,.42))
    fade=Image.new("L",landscape.size)
    fade.putdata([min(255,int(x*255/170)) for y in range(500) for x in range(520)])
    canvas.paste(landscape,(504,0),fade)
    stars(canvas)
    place_art(canvas,LOGO,(80,83,450,220))
    place_art(canvas,CAT,(480,64,410,430))
    centered(ImageDraw.Draw(canvas),"A magical sorting adventure",BODY,28,(310,330),"#fff1cd",435)
    canvas.save(OUT/"GooglePlay/Feature-1024x500.jpg",quality=95,subsampling=0)


def verify():
    targets={
        "AppStore/iPhone-Large": (1320,2868),
        "AppStore/iPhone-Medium": (1179,2556),
        "AppStore/iPad-Editorial-Draft": (2064,2752),
        "GooglePlay/Phone": (1080,1920),
    }
    manifest={}
    for folder,size in targets.items():
        files=list((OUT/folder).glob("*.jpg"))
        assert len(files)==6
        for file in files:
            with Image.open(file) as img:
                assert img.size==size and img.mode=="RGB", file
        manifest[folder]={"count":len(files),"size":size}
    for relative,size in [("AppStore/AppIcon-1024.png",(1024,1024)),("GooglePlay/Icon-512.png",(512,512)),("GooglePlay/Feature-1024x500.jpg",(1024,500))]:
        file=OUT/relative
        with Image.open(file) as img:
            assert img.size==size
            assert img.mode==("RGBA" if "GooglePlay/Icon" in relative else "RGB")
        manifest[relative]={"size":size,"bytes":file.stat().st_size}
    assert (OUT/"GooglePlay/Icon-512.png").stat().st_size<=1024*1024
    metadata={}
    for code in ("en","tr","de","fr","es"):
        text=(OUT/"Metadata"/(code+".md")).read_text(encoding="utf-8")
        fields=dict((k,v.strip()) for k,v in re.findall(r"(?ms)^## ([^\n]+)\n(.*?)(?=^## |\Z)",text))
        counts={"title": len(text.splitlines()[0][2:]),
                "subtitle": len(fields["App Store subtitle"]),
                "short": len(fields["Google Play short description"]),
                "description": len(fields["Full description"]),
                "keywords_utf8": len(fields["App Store keywords"].encode("utf-8")),
                "notes": len(fields["Release notes"])}
        limits={"title":30,"subtitle":30,"short":80,"description":4000,"keywords_utf8":100,"notes":500}
        assert all(0<counts[k]<=limits[k] for k in limits), (code,counts)
        assert all(len(word.strip())>2 for word in fields["App Store keywords"].split(",")), code
        metadata[code]=counts
    manifest["metadata"]=metadata
    (OUT/"IMAGE_VALIDATION.json").write_text(json.dumps(manifest,indent=2)+"\n",encoding="utf-8")


if __name__=="__main__":
    icon()
    gallery(OUT/"AppStore/iPhone-Large",(1320,2868))
    gallery(OUT/"AppStore/iPhone-Medium",(1179,2556))
    gallery(OUT/"AppStore/iPad-Editorial-Draft",(2064,2752))
    gallery(OUT/"GooglePlay/Phone",(1080,1920))
    feature()
    verify()
