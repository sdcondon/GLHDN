#version 330 core

// Input
layout(location = 0) in vec3 vertexPosition_viewspace;
layout(location = 1) in vec4 vertexColor;
layout(location = 2) in float texZ;
layout(location = 3) in vec2 texXY;

// Output
out vec4 vColor;
flat out float iTexZ;
out vec2 vTexXY;

// Uniforms
uniform mat4 P;

void main() {
	gl_Position =  P * vec4(vertexPosition_viewspace, 1);
	vColor = vertexColor;
	iTexZ = texZ;
	vTexXY = texXY;
}

