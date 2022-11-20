#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec4 aColor;

out vec4 ourColor;

uniform mat4 _proj;

void main() {
    gl_Position = vec4(aPos, 1.0) * _proj;
    ourColor = aColor;
}