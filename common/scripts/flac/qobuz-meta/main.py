#!/usr/bin/env python3

import os
import shutil
import argparse
from mutagen.flac import FLAC, Picture

def find_cover_image(folder):
    for file in os.listdir(folder):
        if file.lower().endswith((".jpg", ".jpeg", ".png")):
            return os.path.join(folder, file)
    return None


def embed_cover(audio, image_path):
    audio.clear_pictures()
    with open(image_path, "rb") as img:
        pic = Picture()
        pic.data = img.read()
        pic.type = 3  # front cover
        pic.mime = "image/jpeg" if image_path.lower().endswith(("jpg", "jpeg")) else "image/png"
        audio.add_picture(pic)


def safe_filename(text):
    return "".join(c for c in text if c not in r'\/:*?"<>|').strip()


def get_tag(audio, tag):
    return audio.tags.get(tag, ["Unknown"])[0]


def process(input_root, output_folder, dry_run=False):
    os.makedirs(output_folder, exist_ok=True)

    for root, dirs, files in os.walk(input_root):
        flac_files = [f for f in files if f.lower().endswith(".flac")]
        if not flac_files:
            continue

        cover = find_cover_image(root)

        for flac in flac_files:
            full_path = os.path.join(root, flac)

            try:
                audio = FLAC(full_path)

                if cover:
                    embed_cover(audio, cover)

                artist = safe_filename(get_tag(audio, "artist"))
                title = safe_filename(get_tag(audio, "title"))
                track = get_tag(audio, "tracknumber").split("/")[0].zfill(2)

                new_name = f"{artist} - {track} - {title}.flac"
                output_path = os.path.join(output_folder, new_name)

                if os.path.exists(output_path):
                    print(f"[skip] {new_name} already exists")
                    continue

                print(f"[process] {new_name}")

                if not dry_run:
                    audio.save()
                    shutil.copy2(full_path, output_path)

            except Exception as e:
                print(f"[error] {flac}: {e}")


def main():
    parser = argparse.ArgumentParser(description="Qobuz FLAC metadata processor")
    parser.add_argument("input", help="Input folder")
    parser.add_argument("output", help="Output folder")
    parser.add_argument("--dry-run", action="store_true", help="Preview without writing files")

    args = parser.parse_args()
    
    args.input = os.path.expanduser(args.input)
    args.output = os.path.expanduser(args.output)

    process(args.input, args.output, args.dry_run)


if __name__ == "__main__":
    main()