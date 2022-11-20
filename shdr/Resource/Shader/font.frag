#version 330 core

in vec4 ourColor;
in vec2 ourTexCoord;

out vec4 color;

uniform sampler2D ourTexture;

void main() {
    color = vec4(vec3(1), texture(ourTexture, ourTexCoord)) * ourColor;
}