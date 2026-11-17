"""Compile exported Vehicle shaders on the local SDL2/OpenGL driver, without running the game.

Usage: python3 compile-vehicle-gl.py artifacts/validation/vehicle/vehicle-shaders.json
Requires the original failing effect: it must fail before repair and pass afterwards.
"""
import ctypes as c
import json
import sys

sdl = c.CDLL('libSDL2-2.0.so.0')
sdl.SDL_Init.argtypes = [c.c_uint]
sdl.SDL_CreateWindow.argtypes = [c.c_char_p, c.c_int, c.c_int, c.c_int, c.c_int, c.c_uint]
sdl.SDL_CreateWindow.restype = c.c_void_p
sdl.SDL_GL_CreateContext.argtypes = [c.c_void_p]
sdl.SDL_GL_CreateContext.restype = c.c_void_p
sdl.SDL_GL_GetProcAddress.argtypes = [c.c_char_p]
sdl.SDL_GL_GetProcAddress.restype = c.c_void_p
sdl.SDL_GL_DeleteContext.argtypes = [c.c_void_p]
sdl.SDL_DestroyWindow.argtypes = [c.c_void_p]
sdl.SDL_GetError.restype = c.c_char_p

def gl(name, result, *args):
    address = sdl.SDL_GL_GetProcAddress(name.encode())
    if not address:
        raise RuntimeError(name + ' unavailable')
    return c.CFUNCTYPE(result, *args)(address)

window = context = None
try:
    if sdl.SDL_Init(0x20) != 0:
        raise RuntimeError(sdl.SDL_GetError())
    # Hidden OpenGL window. Default SDL context uses desktop compatibility GLSL.
    window = sdl.SDL_CreateWindow(b'Vehicle shader validation', 0, 0, 32, 32, 0x2 | 0x8)
    if not window:
        raise RuntimeError(sdl.SDL_GetError())
    context = sdl.SDL_GL_CreateContext(window)
    if not context:
        raise RuntimeError(sdl.SDL_GetError())
    create = gl('glCreateShader', c.c_uint, c.c_uint)
    source = gl('glShaderSource', None, c.c_uint, c.c_int, c.POINTER(c.c_char_p), c.POINTER(c.c_int))
    compile_shader = gl('glCompileShader', None, c.c_uint)
    status = gl('glGetShaderiv', None, c.c_uint, c.c_uint, c.POINTER(c.c_int))
    log = gl('glGetShaderInfoLog', None, c.c_uint, c.c_int, c.POINTER(c.c_int), c.c_char_p)
    delete = gl('glDeleteShader', None, c.c_uint)
    get_string = gl('glGetString', c.c_char_p, c.c_uint)
    print('Renderer:', get_string(0x1F01).decode())
    with open(sys.argv[1]) as report:
        stages = json.load(report)
    counts = {}
    for label in ('before', 'after'):
        failures = 0
        for index, stage in enumerate(stages[label]):
            shader = create(0x8B31 if stage['vertex'] else 0x8B30)
            text = stage['source'].rstrip('\0').encode('ascii')
            text_pointer = c.c_char_p(text)
            length = c.c_int(len(text))
            source(shader, 1, c.byref(text_pointer), c.byref(length))
            compile_shader(shader)
            ok = c.c_int()
            status(shader, 0x8B81, c.byref(ok))
            if not ok.value:
                failures += 1
                buffer = c.create_string_buffer(65536)
                log(shader, len(buffer), None, buffer)
                print(label, index, buffer.value.decode(errors='replace'))
            delete(shader)
        counts[label] = failures
        print(label, len(stages[label]), 'shaders;', failures, 'failures')
    if counts['before'] != 3 or counts['after'] != 0:
        raise RuntimeError('Expected three original failures and zero repaired failures')
    print('PASS: reproduced original failure and compiled every repaired Vehicle shader')
finally:
    if context:
        sdl.SDL_GL_DeleteContext(context)
    if window:
        sdl.SDL_DestroyWindow(window)
    sdl.SDL_Quit()
