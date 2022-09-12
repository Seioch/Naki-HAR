import glob as glob
import os, shutil


texture_paths = glob.glob("C:\\Program Files (x86)\\Steam\\steamapps\\common\\RimWorld\\Mods\\Naki-HAR\\Textures\\Naki\\Apparel\\**\\*.png", recursive=True)

print(len(texture_paths))

for path in texture_paths:
    base, filename = os.path.split(path)
    print(base)
    print(filename)
    if("Female" in filename):
        new_filename = filename
        # replace the Female with Male and resave the file
        new_filename = new_filename.replace("Female", "Male")
        print(new_filename)
        shutil.copyfile(path, os.path.join(base, new_filename))
    # break