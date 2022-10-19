import cv2
from pathlib import Path

# texture_paths = glob.glob("C:\\Program Files (x86)\\Steam\\steamapps\\common\\RimWorld\\Mods\\Naki-HAR\\Textures\\Naki\\**.png", recursive = True)
texture_paths = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\RimWorld\\Mods\\Naki-HAR\\Textures\\Naki"
textures = Path(texture_paths).rglob('*.png')

for tex in textures:
    # print(tex)
    im = cv2.imread(str(tex))
    # print(im.shape)
    if(im.shape[0] > 512 or im.shape[1] > 512):
        print(f"Holo you dum dum image {tex.name} is bigger than 512x, resizing")
        resized = cv2.resize(im, (512, 512), interpolation = cv2.INTER_AREA)
        # print(f"writing new file to {str(tex)}")
        cv2.imwrite(str(tex), resized)