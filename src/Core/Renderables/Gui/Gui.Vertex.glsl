#version 330 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 vertexPosition_viewspace;
layout(location = 1) in vec3 vertexColor;

// Output data ; will be interpolated for each fragment.
out vec3 matColor;

// Values that stay constant for the whole mesh.
uniform mat4 P;

void main(){

	// Output position of the vertex, in clip space : MVP * position
	gl_Position =  P * vec4(vertexPosition_viewspace,1);
		
	// Color of the vertex. No special space for this one.
	matColor = vertexColor;
}

