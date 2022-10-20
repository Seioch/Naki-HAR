import cv2
import os, shutil
import argparse
from pathlib import Path

parser = argparse.ArgumentParser(description="Add Male Tag to textures")
parser.add_argument('--dir', dest='dir', type=str, required=True, help='Path to a directory of png images')
args = parser.parse_args()

textures = Path(args.dir).rglob('*.png')

# Replace the head texture names with ones for Male and Female
for path in textures:
    base, filename = os.path.split(path)
    print(base)
    print(filename)
    if ("Female" in filename):
        new_filename_male = filename.replace("Female", "Male")
        shutil.copyfile(path, os.path.join(base, new_filename_male)) # clone all the existing non Male_ prefixed sprites and prefix them
        # break