import cv2
import os, shutil
import argparse
from pathlib import Path

parser = argparse.ArgumentParser(description="Downsample or Upsample images")
parser.add_argument('--dir', dest='dir', type=str, required=True, help='Path to a directory of png images')
parser.add_argument('--output', dest='output', type=str, required=True, help='Where to put the resized images')
parser.add_argument('--width', type=int, default=512, help='Width of outputted images')
parser.add_argument('--height', type=int, default=512, help='Height of outputted images')
args = parser.parse_args()

# If the output directory is not empty, clear it out and make a new one
if os.path.exists(args.output):
    print("Clearing output directory and making a new one")
    shutil.rmtree(args.output)
    os.mkdir(args.output)
else:
    os.mkdir(args.output)

textures = Path(args.dir).rglob('*.png')

for tex in textures:
    im = cv2.imread(str(tex), cv2.IMREAD_UNCHANGED)
    resized = cv2.resize(im, (args.height, args.width), interpolation = cv2.INTER_AREA)
    print(f"writing new file to {os.path.join(os.path.abspath(args.output), os.path.basename(str(tex)))}")
    cv2.imwrite(os.path.join(os.path.abspath(args.output), os.path.basename(str(tex))), resized)