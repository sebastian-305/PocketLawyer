import struct, zlib

WIDTH, HEIGHT = 800, 400
bg_color = (255, 255, 255)
fg_color = (0, 0, 0)

canvas = bytearray([bg_color[0], bg_color[1], bg_color[2]] * WIDTH * HEIGHT)

def set_pixel(x, y, color):
    if 0 <= x < WIDTH and 0 <= y < HEIGHT:
        i = (y * WIDTH + x) * 3
        canvas[i:i+3] = bytes(color)

def draw_rect(x, y, w, h, color=fg_color):
    for i in range(x, x+w):
        set_pixel(i, y, color)
        set_pixel(i, y+h-1, color)
    for j in range(y, y+h):
        set_pixel(x, j, color)
        set_pixel(x+w-1, j, color)

def draw_line(x1, y1, x2, y2, color=fg_color):
    dx = abs(x2 - x1)
    dy = -abs(y2 - y1)
    sx = 1 if x1 < x2 else -1
    sy = 1 if y1 < y2 else -1
    err = dx + dy
    while True:
        set_pixel(x1, y1, color)
        if x1 == x2 and y1 == y2:
            break
        e2 = 2 * err
        if e2 >= dy:
            err += dy
            x1 += sx
        if e2 <= dx:
            err += dx
            y1 += sy

# 5x7 font
data_font = {
    'A': [' 1 ','1 1','111','1 1','1 1'],
    'C': [' 11','1  ','1  ','1  ',' 11'],
    'D': ['11 ','1 1','1 1','1 1','11 '],
    'E': ['111','1  ','11 ','1  ','111'],
    'F': ['111','1  ','11 ','1  ','1  '],
    'G': [' 11','1  ','1 1','1 1',' 11'],
    'I': ['111',' 1 ',' 1 ',' 1 ','111'],
    'K': ['1 1','1 1','11 ','1 1','1 1'],
    'L': ['1  ','1  ','1  ','1  ','111'],
    'N': ['1 1','111','111','111','1 1'],
    'O': ['111','1 1','1 1','1 1','111'],
    'P': ['11 ','1 1','11 ','1  ','1  '],
    'R': ['11 ','1 1','11 ','1 1','1 1'],
    'S': [' 11','1  ',' 1 ','  1','11 '],
    'T': ['111',' 1 ',' 1 ',' 1 ',' 1 '],
    'V': ['1 1','1 1','1 1','1 1',' 1 '],
    'c': ['   ','  ',' 11','1  ',' 11'],
    'o': ['   ','  ','11 ','1 1','11 '],
    'n': ['   ','   ','11 ','1 1','1 1'],
    't': [' 1 ','111',' 1 ',' 1 ',' 1 '],
    'r': ['   ','   ','11 ','1  ','1  '],
    'l': ['1  ','1  ','1  ','1  ','11 '],
    'e': ['   ','  ','11 ','111','11 '],
    'd': ['  ','  ',' 11','1 1',' 11'],
    'v': ['   ','   ','1 1','1 1',' 1 '],
    'i': ['  ','1 ','1 ','1 ','1 '],
    'a': ['   ','  ',' 1 ','111','1 1'],
}

CHAR_W = 3
CHAR_H = 5

def draw_char(x, y, ch, color=fg_color):
    pattern = data_font.get(ch, ['']*CHAR_H)
    for dy,row in enumerate(pattern):
        for dx,c in enumerate(row):
            if c == '1':
                set_pixel(x+dx, y+dy, color)

def draw_text(x, y, text, color=fg_color):
    for idx,ch in enumerate(text):
        draw_char(x + idx*(CHAR_W+1), y, ch, color)

def save_png(filename):
    def chunk(tag, data):
        return struct.pack('>I', len(data)) + tag + data + struct.pack('>I', zlib.crc32(tag+data)&0xffffffff)
    png = b'\x89PNG\r\n\x1a\n'
    ihdr = struct.pack('>IIBBBBB', WIDTH, HEIGHT, 8, 2, 0, 0, 0)
    png += chunk(b'IHDR', ihdr)
    raw = b''
    for y in range(HEIGHT):
        raw += b'\x00' + canvas[y*WIDTH*3:(y+1)*WIDTH*3]
    png += chunk(b'IDAT', zlib.compress(raw,9))
    png += chunk(b'IEND', b'')
    with open(filename, 'wb') as f:
        f.write(png)

# layout

def box(x,y,w,h,label):
    draw_rect(x,y,w,h)
    draw_text(x+3,y+2,label)

box(20,50,150,30,'PocketController')
box(300,30,150,30,'GeminiService')
box(300,70,150,30,'KvAiDetection')
box(300,110,150,30,'KvFileService')
box(300,150,150,30,'FileDecoder')
box(580,30,190,30,'ContractAnalysis')
box(580,70,190,30,'ContractValidation')
box(580,110,190,30,'FileUploadDto')
box(580,150,190,30,'FileContentDto')
box(580,190,190,30,'KvDetectionRes')

# lines controller->services
for y in (45,85,125,165):
    draw_line(170,65,y, y,)

draw_line(170,65,300,45)

save_png('docs/UML.png')
