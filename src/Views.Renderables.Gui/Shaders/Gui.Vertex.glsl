#version 330 core

// Input
layout(location = 0) in vec3 vertexPosition_viewspace;
layout(location = 1) in vec4 vertexColor;
layout(location = 2) in vec2 elementPos;
layout(location = 3) in vec2 elementSize;
layout(location = 4) in float borderWidth;

// Output
out vec4 matColor;
out float vborderWidth;
out vec2 velementPos;
flat out vec2 velementSize;
flat out float elementLayer;

// Uniforms
uniform mat4 P;

void main() {
	gl_Position =  P * vec4(vertexPosition_viewspace, 1);
	matColor = vertexColor;

	velementPos = elementPos;
	velementSize = elementSize;
	if (borderWidth < 0)
	{
		vborderWidth = 1;
		elementLayer = -borderWidth;
	}
	else
	{
		vborderWidth = borderWidth;
		elementLayer = -1;
	}
}

