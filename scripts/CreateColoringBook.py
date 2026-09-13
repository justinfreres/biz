from pathlib import Path

from PIL import Image, ImageDraw, ImageFont, ImageOps
from reportlab.lib.pagesizes import letter
from reportlab.lib.utils import ImageReader
from reportlab.pdfgen import canvas


ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "assets" / "coloring-book"
OUTPUT = ROOT / "output" / "pdf" / "flowbridge-funny-tech-coloring-book-series-1.pdf"
THUMBNAIL_DESTINATIONS = [
    ROOT / "dist" / "assets" / "products" / "flowbridge-funny-tech-coloring-book-series-1-cover.png",
    ROOT / "src" / "FlowBridge.Web" / "wwwroot" / "assets" / "products" / "flowbridge-funny-tech-coloring-book-series-1-cover.png",
]
RENDERED_PAGES = ROOT / "tmp" / "pdfs" / "coloring-book-rendered-pages"

COVER = ASSETS / "exec-fcdc7463-4de4-4e5f-b9ab-79b47769bab1.png"
SHEETS = [
    ASSETS / "exec-c633b4b8-f9be-4ab3-be84-2cbab30eb5a3.png",
    ASSETS / "exec-3c0be020-0c42-4c4a-bfdb-0230c33a5c47.png",
    ASSETS / "exec-5fb9ee78-9af3-444a-8146-55ff5346f8d4.png",
    ASSETS / "exec-cb703c85-c1a0-4219-8038-0ae3552314f4.png",
    ASSETS / "exec-61206c22-3369-4654-a391-9106a4bf1568.png",
    ASSETS / "exec-09223523-3a52-4a27-b109-fab6d16ac7a9.png",
]


def make_print_page(image, footer):
    page = Image.new("RGB", (1275, 1650), "white")
    draw = ImageDraw.Draw(page)
    draw.rounded_rectangle((55, 55, 1220, 1560), radius=18, outline="black", width=4)
    fitted = ImageOps.contain(image.convert("RGB"), (1085, 1360), Image.Resampling.LANCZOS)
    x = (page.width - fitted.width) // 2
    y = 110 + (1360 - fitted.height) // 2
    page.paste(fitted, (x, y))
    font = ImageFont.load_default()
    footer_width = draw.textbbox((0, 0), footer, font=font)[2]
    draw.text(((page.width - footer_width) // 2, 1595), footer, fill="black", font=font)
    return page


def main():
    if not COVER.exists() or any(not sheet.exists() for sheet in SHEETS):
        raise FileNotFoundError("All source cover and panel sheets must exist before creating the book.")

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    for destination in THUMBNAIL_DESTINATIONS:
        destination.parent.mkdir(parents=True, exist_ok=True)

    with Image.open(COVER) as cover_source:
        cover = cover_source.convert("RGB")
        thumbnail = ImageOps.contain(cover, (1200, 1600), Image.Resampling.LANCZOS)
        for destination in THUMBNAIL_DESTINATIONS:
            thumbnail.save(destination, "PNG", optimize=True)

    RENDERED_PAGES.mkdir(parents=True, exist_ok=True)
    rendered_paths = []
    cover_page = make_print_page(cover, "FLOWBRIDGE SYSTEMS LLC - DIGITAL EDITION")
    cover_path = RENDERED_PAGES / "page-01-cover.jpg"
    cover_page.save(cover_path, "JPEG", quality=88, optimize=True)
    rendered_paths.append(cover_path)

    page_number = 1
    for sheet_path in SHEETS:
        with Image.open(sheet_path) as sheet_source:
            sheet = sheet_source.convert("RGB")
            mid_x, mid_y = sheet.width // 2, sheet.height // 2
            boxes = [
                (0, 0, mid_x, mid_y),
                (mid_x, 0, sheet.width, mid_y),
                (0, mid_y, mid_x, sheet.height),
                (mid_x, mid_y, sheet.width, sheet.height),
            ]
            for box in boxes:
                page_number += 1
                footer = f"FLOWBRIDGE FUNNY TECH COLORING BOOK - SERIES 1 - PAGE {page_number - 1}"
                rendered = make_print_page(sheet.crop(box), footer)
                rendered_path = RENDERED_PAGES / f"page-{page_number:02d}.jpg"
                rendered.save(rendered_path, "JPEG", quality=88, optimize=True)
                rendered_paths.append(rendered_path)

    if len(rendered_paths) != 25:
        raise ValueError("Series 1 requires exactly 24 interior coloring pages.")

    page_width, page_height = letter
    pdf = canvas.Canvas(str(OUTPUT), pagesize=letter, pageCompression=1)
    pdf.setTitle("FlowBridge Funny Tech Coloring Book - Series 1")
    pdf.setAuthor("FlowBridge Systems LLC")
    pdf.setSubject("Original funny technology coloring pages")

    for rendered_path in rendered_paths:
        pdf.drawImage(str(rendered_path), 0, 0, page_width, page_height)
        pdf.showPage()

    pdf.save()
    print(f"Created {OUTPUT}")


if __name__ == "__main__":
    main()
