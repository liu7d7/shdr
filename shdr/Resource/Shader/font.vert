#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 color;

out vec4 ourColor;
out vec2 ourTexCoord;

uniform mat4 _proj;

void main() {
    gl_Position = vec4(aPos, 1.0) * _proj;
    ourColor = color;
    ourTexCoord = aTexCoord;
}