<p align="center">
  <img width="160px" src="https://raw.githubusercontent.com/pleasegivesource/glsland/master/shdr/code-edit.svg"/>
</p>

# GLSL Editor

This is a GLSL editor for the desktop written using OpenTK and C#. It's a work in progress, but it's already pretty usable.

## Features

- Syntax highlighting.
- Hot-reloading of shader code.
- Weird text editor with 2 cursors‽
- DiscordRPC. Why? No one knows.

## Installation

- Just download the release from the releases page and run `shdr.exe`.
- You can also build it yourself by cloning the repo and running `dotnet build` in the `shdr` directory.

## Usage
- After you run `shdr.exe`, just press enter at the prompt to start editing. 
This will create a file called `shaderx.frag` where x is the number of unnamed shaders created before it.
- Alternatively, you can type in a file name at the prompt. If it exists, you'll be editing that file. 
If it doesn't exist, it will be created and a template will be generated.
- After you're happy with your code, press `Ctrl+S` or `F5` to compile and run the shader.
- The code window can be toggled with `F1` .
- The font can be changed with `F2`.
    - If you don't like any of the fonts that are pre-supplied, you may install your own fonts by putting them in the `Resource/Font/` directory.
- DiscordRPC can be toggled with `F10`.