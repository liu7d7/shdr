#version 410 core

vec2 uv;
out vec4 color;

uniform vec2 _resolution;
uniform float _time;
uniform sampler2D _prevFrame;

void main() {
    uv = gl_FragCoord.xy / _resolution.xy;
    color = vec4(uv.r, 0., uv.g, 1.);
}