#!/usr/bin/env python3
"""
Pixel Art Generator
Creates pixel art collages, wallpapers, and custom designs from prompts and source images.

Usage:
  python pixel_art.py "a knight in a forest" --mode scene
  python pixel_art.py "playing guitar on stage" --source photo.jpg
  python pixel_art.py "epic sunset city" --mode wallpaper --output wall.png
  python pixel_art.py "hiking, cooking, reading" --source family.jpg --mode collage --count 3
"""

import os
import sys
import base64
import io
import argparse
from pathlib import Path
from typing import Optional

# --- Dependency checks with friendly errors ---
try:
    import anthropic
except ImportError:
    sys.exit("Missing dependency: pip install anthropic")

try:
    from PIL import Image, ImageDraw
except ImportError:
    sys.exit("Missing dependency: pip install Pillow")

try:
    import requests
except ImportError:
    sys.exit("Missing dependency: pip install requests")


# ---------------------------------------------------------------------------
# Image helpers
# ---------------------------------------------------------------------------

def encode_image_base64(image_path: str) -> tuple[str, str]:
    ext = Path(image_path).suffix.lower()
    media_types = {
        ".jpg": "image/jpeg", ".jpeg": "image/jpeg",
        ".png": "image/png", ".gif": "image/gif", ".webp": "image/webp",
    }
    media_type = media_types.get(ext, "image/jpeg")
    with open(image_path, "rb") as f:
        data = base64.standard_b64encode(f.read()).decode("utf-8")
    return data, media_type


# ---------------------------------------------------------------------------
# Step 1: Analyze source image with Claude Vision
# ---------------------------------------------------------------------------

def analyze_source_image(image_path: str, context: str = "") -> str:
    """Ask Claude to describe the subject so we can recreate them in pixel art."""
    client = anthropic.Anthropic()
    img_data, media_type = encode_image_base64(image_path)

    prompt = f"""Analyze this image in detail for pixel art recreation.

Describe:
1. Main subject(s) — appearance, age range, distinctive features, clothing style
2. Hair color/style, skin tone, eye color if visible
3. Any accessories, props, or notable visual elements
4. Overall mood and setting

Additional context from user: {context or "None provided."}

Give a clear 2–3 paragraph description focused on visual details that translate well to pixel art.
Do NOT include advice or commentary — just the visual description."""

    response = client.messages.create(
        model="claude-sonnet-4-6",
        max_tokens=1024,
        messages=[{
            "role": "user",
            "content": [
                {"type": "image", "source": {"type": "base64", "media_type": media_type, "data": img_data}},
                {"type": "text", "text": prompt},
            ],
        }],
    )
    return response.content[0].text


# ---------------------------------------------------------------------------
# Step 2: Build DALL-E prompt
# ---------------------------------------------------------------------------

STYLE_NOTES = {
    "scene":     "Create a detailed scene composition.",
    "portrait":  "Create a centered portrait composition with a decorative pixel art background.",
    "wallpaper": "Create a wide panoramic scene suitable for a desktop wallpaper (16:9 aspect ratio).",
    "collage":   "Create a single pixel art panel. It will be combined with others into a collage.",
}

def build_prompt(subject_desc: str, creative_prompt: str, mode: str) -> str:
    style_note = STYLE_NOTES.get(mode, STYLE_NOTES["scene"])
    return f"""Pixel art digital artwork. {style_note}

Subject: {subject_desc}

Creative direction: {creative_prompt}

Visual style:
- Classic 16-bit or 32-bit pixel art aesthetic
- Clearly visible pixel blocks, no anti-aliasing, no blur
- Bold vibrant colors with a limited palette (16–32 colors)
- Retro video game character art quality
- Crisp clean outlines, charming and expressive

Make it colorful, lively, and unmistakably pixel art."""


# ---------------------------------------------------------------------------
# Step 3: Generate with DALL-E 3
# ---------------------------------------------------------------------------

def generate_with_dalle(prompt: str, size: str = "1024x1024") -> bytes:
    try:
        from openai import OpenAI
    except ImportError:
        sys.exit("Missing dependency for DALL-E backend: pip install openai")

    client = OpenAI()
    response = client.images.generate(
        model="dall-e-3",
        prompt=prompt,
        size=size,
        quality="hd",
        n=1,
    )
    url = response.data[0].url
    r = requests.get(url, timeout=60)
    r.raise_for_status()
    return r.content


def generate_with_replicate(prompt: str) -> bytes:
    try:
        import replicate
    except ImportError:
        sys.exit("Missing dependency for Replicate backend: pip install replicate")

    output = replicate.run(
        "stability-ai/sdxl:39ed52f2a78e934b3ba6e2a89f5b1c712de7dfea535525255b1aa35c5565e08b",
        input={
            "prompt": f"pixel art style, {prompt}",
            "negative_prompt": "blurry, smooth, photorealistic, 3d, anti-aliased, oil painting",
            "num_inference_steps": 40,
            "guidance_scale": 7.5,
        },
    )
    url = output[0] if isinstance(output, list) else output
    r = requests.get(url, timeout=60)
    r.raise_for_status()
    return r.content


# ---------------------------------------------------------------------------
# Step 4: Post-process — enforce pixelated look
# ---------------------------------------------------------------------------

def apply_pixel_effect(image_bytes: bytes, pixel_size: int = 4, num_colors: int = 32) -> Image.Image:
    """Downscale → quantize colors → upscale with NEAREST for crisp pixel blocks."""
    img = Image.open(io.BytesIO(image_bytes)).convert("RGB")
    orig_w, orig_h = img.size

    small_w = max(1, orig_w // pixel_size)
    small_h = max(1, orig_h // pixel_size)

    small = img.resize((small_w, small_h), Image.NEAREST)
    small = small.quantize(colors=num_colors, method=Image.Quantize.FASTOCTREE).convert("RGB")
    return small.resize((orig_w, orig_h), Image.NEAREST)


def add_grid_overlay(img: Image.Image, pixel_size: int = 4, alpha: int = 25) -> Image.Image:
    """Overlay a subtle grid to emphasize the pixel art blocks."""
    base = img.convert("RGBA")
    overlay = Image.new("RGBA", img.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(overlay)
    w, h = img.size
    line_color = (0, 0, 0, alpha)
    for x in range(0, w, pixel_size):
        draw.line([(x, 0), (x, h)], fill=line_color)
    for y in range(0, h, pixel_size):
        draw.line([(0, y), (w, y)], fill=line_color)
    return Image.alpha_composite(base, overlay).convert("RGB")


# ---------------------------------------------------------------------------
# Collage + wallpaper layout
# ---------------------------------------------------------------------------

def make_collage(images: list[Image.Image], padding: int = 12, bg_color=(18, 18, 28)) -> Image.Image:
    if len(images) == 1:
        return images[0]

    target = images[0].size
    images = [img.resize(target, Image.NEAREST) for img in images]

    n = len(images)
    cols = 2 if n <= 4 else 3
    rows = (n + cols - 1) // cols

    w, h = target
    canvas_w = cols * w + (cols + 1) * padding
    canvas_h = rows * h + (rows + 1) * padding
    canvas = Image.new("RGB", (canvas_w, canvas_h), bg_color)

    for i, img in enumerate(images):
        col = i % cols
        row = i // cols
        x = padding + col * (w + padding)
        y = padding + row * (h + padding)
        canvas.paste(img, (x, y))

    return canvas


def make_wallpaper(img: Image.Image, resolution=(2560, 1440), bg_color=(12, 12, 22)) -> Image.Image:
    rw, rh = resolution
    ratio = img.width / img.height
    wall_ratio = rw / rh

    if ratio > wall_ratio:
        new_w, new_h = rw, int(rw / ratio)
    else:
        new_w, new_h = int(rh * ratio), rh

    scaled = img.resize((new_w, new_h), Image.NEAREST)
    canvas = Image.new("RGB", resolution, bg_color)
    canvas.paste(scaled, ((rw - new_w) // 2, (rh - new_h) // 2))
    return canvas


# ---------------------------------------------------------------------------
# Save helper
# ---------------------------------------------------------------------------

def save_image(img: Image.Image, path: str) -> str:
    p = Path(path)
    p.parent.mkdir(parents=True, exist_ok=True)
    if p.suffix.lower() not in (".png", ".jpg", ".jpeg"):
        path = str(p.with_suffix(".png"))
    save_kwargs = {"quality": 95} if path.endswith((".jpg", ".jpeg")) else {}
    img.save(path, **save_kwargs)
    return path


# ---------------------------------------------------------------------------
# Main generation pipeline
# ---------------------------------------------------------------------------

def run(
    prompt: str,
    source: Optional[str],
    output: str,
    mode: str,
    backend: str,
    pixel_size: int,
    num_colors: int,
    resolution: str,
    collage_count: int,
    grid: bool,
    context: str,
    verbose: bool,
):
    print(f"  Mode: {mode}  |  Backend: {backend}  |  Pixel size: {pixel_size}  |  Colors: {num_colors}")

    # 1. Analyze source image
    subject_desc = ""
    if source:
        print(f"\n[1/3] Analyzing source image: {source}")
        subject_desc = analyze_source_image(source, context)
        if verbose:
            print(f"\n--- Subject Description ---\n{subject_desc}\n---\n")
        else:
            print("      Done.")

    # 2. Build prompt
    full_prompt = build_prompt(subject_desc, prompt, mode)
    if verbose:
        print(f"\n--- Generation Prompt ---\n{full_prompt}\n---\n")

    # 3. Generate image(s)
    dalle_size = "1792x1024" if mode == "wallpaper" else "1024x1024"

    def _generate(extra_note="") -> Image.Image:
        p = full_prompt + (f"\n\n{extra_note}" if extra_note else "")
        if backend == "dalle":
            raw = generate_with_dalle(p, size=dalle_size)
        else:
            raw = generate_with_replicate(p)
        img = apply_pixel_effect(raw, pixel_size, num_colors)
        if grid:
            img = add_grid_overlay(img, pixel_size)
        return img

    step = "2" if not source else "3"

    if mode == "collage":
        # Split the prompt on commas to give each panel its own creative note
        themes = [t.strip() for t in prompt.split(",") if t.strip()]
        count = max(collage_count, len(themes)) if themes else collage_count
        panels = []
        print(f"\n[{step}/{step}] Generating {count} panel(s) for collage...")
        for i in range(count):
            theme = themes[i] if i < len(themes) else f"scene {i+1}"
            note = f"Panel {i+1} of {count}: focus on {theme}."
            print(f"      Panel {i+1}/{count}: {theme}")
            panels.append(_generate(note))
        final = make_collage(panels)
    else:
        print(f"\n[{step}/{step}] Generating image...")
        generated = _generate()
        if mode == "wallpaper":
            rw, rh = map(int, resolution.split("x"))
            final = make_wallpaper(generated, (rw, rh))
        else:
            final = generated

    # 4. Save
    saved = save_image(final, output)
    print(f"\n[✓] Saved → {saved}")
    print(f"    Size: {final.width} x {final.height} px")


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(
        prog="pixel_art.py",
        description="Pixel Art Generator — turn prompts and photos into pixel art",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
EXAMPLES
  # Basic pixel art scene
  python pixel_art.py "a wizard in a dark forest"

  # From a family photo — scene of a hobby
  python pixel_art.py "playing piano in a cozy room" --source grandma.jpg

  # Multiple hobbies as a collage from a photo (comma-separated themes)
  python pixel_art.py "hiking, cooking, painting" --source dad.jpg --mode collage

  # Desktop wallpaper from a photo
  python pixel_art.py "riding a dragon over a fantasy city" --source me.jpg --mode wallpaper

  # Stronger pixel effect with fewer colors
  python pixel_art.py "retro dungeon crawler scene" --pixel-size 8 --colors 16

  # Using Replicate instead of DALL-E
  python pixel_art.py "space explorer" --backend replicate

ENVIRONMENT VARIABLES
  ANTHROPIC_API_KEY   Required when using --source (Claude Vision)
  OPENAI_API_KEY      Required for --backend dalle (default)
  REPLICATE_API_TOKEN Required for --backend replicate
""",
    )

    parser.add_argument("prompt", help="What to create, or comma-separated themes for collage")
    parser.add_argument("--source", "-s", metavar="IMAGE", help="Source photo to base the art on")
    parser.add_argument("--output", "-o", default="pixel_art_output.png", help="Output file (default: pixel_art_output.png)")
    parser.add_argument(
        "--mode", "-m",
        choices=["scene", "portrait", "wallpaper", "collage"],
        default="scene",
        help="Output type (default: scene)",
    )
    parser.add_argument(
        "--backend", "-b",
        choices=["dalle", "replicate"],
        default="dalle",
        help="Image generation backend (default: dalle)",
    )
    parser.add_argument("--pixel-size", type=int, default=4, metavar="N",
                        help="Pixel block size for post-processing effect (default: 4, higher = chunkier)")
    parser.add_argument("--colors", type=int, default=32, metavar="N",
                        help="Color palette size (default: 32, lower = more retro)")
    parser.add_argument("--resolution", default="2560x1440", metavar="WxH",
                        help="Wallpaper resolution (default: 2560x1440)")
    parser.add_argument("--count", type=int, default=2, metavar="N",
                        help="Number of panels in collage mode (default: 2; overridden by comma count in prompt)")
    parser.add_argument("--grid", action="store_true", help="Add subtle pixel grid overlay")
    parser.add_argument("--context", "-c", default="", metavar="TEXT",
                        help="Extra context about source image (e.g. 'my grandmother, age 70')")
    parser.add_argument("--verbose", "-v", action="store_true", help="Print prompts and full descriptions")

    args = parser.parse_args()

    # API key checks
    if args.source and not os.environ.get("ANTHROPIC_API_KEY"):
        sys.exit("ANTHROPIC_API_KEY not set. Required for source image analysis.\n"
                 "  export ANTHROPIC_API_KEY=sk-ant-...")

    if args.backend == "dalle" and not os.environ.get("OPENAI_API_KEY"):
        sys.exit("OPENAI_API_KEY not set. Required for DALL-E backend.\n"
                 "  export OPENAI_API_KEY=sk-...")

    if args.backend == "replicate" and not os.environ.get("REPLICATE_API_TOKEN"):
        sys.exit("REPLICATE_API_TOKEN not set. Required for Replicate backend.\n"
                 "  export REPLICATE_API_TOKEN=r8_...")

    try:
        run(
            prompt=args.prompt,
            source=args.source,
            output=args.output,
            mode=args.mode,
            backend=args.backend,
            pixel_size=args.pixel_size,
            num_colors=args.colors,
            resolution=args.resolution,
            collage_count=args.count,
            grid=args.grid,
            context=args.context,
            verbose=args.verbose,
        )
    except KeyboardInterrupt:
        print("\nCancelled.")
    except Exception as exc:
        print(f"\n[!] Error: {exc}")
        if args.verbose:
            raise
        sys.exit(1)


if __name__ == "__main__":
    main()
