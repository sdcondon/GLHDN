#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_viewspace;
layout(location = 1) in vec4 vertexColor;
layout(location = 2) in vec2 elementPos;
layout(location = 3) in vec2 elementSize;
layout(location = 4) in float borderWidth;
// corner rounding?

// Output data ; will be interpolated for each fragment.
out vec4 matColor;
out vec2 velementPos;
flat out vec2 velementSize;
out float vborderWidth;

// Values that stay constant for the whole mesh.
uniform mat4 P;

void main() {

	gl_Position =  P * vec4(vertexPosition_viewspace, 1);
		
	matColor = vertexColor;
	velementPos = elementPos;
	velementSize = elementSize;
	vborderWidth = borderWidth;
}

