#version 330 core

// Interpolated values from the vertex shaders
in vec4 matColor;
in vec2 velementPos;
flat in vec2 velementSize;
in float vborderWidth;

// Ouput data
out vec4 color;

void main() {
	//color = matColor;
	
	if (velementPos.x < vborderWidth
	    || velementPos.y < vborderWidth
	    || abs(velementPos.x - velementSize.x) < vborderWidth
		|| abs(velementPos.y - velementSize.y) < vborderWidth) {
		color = vec4(1, 1, 1, 1);
	}
	else {
		color = matColor;
	}
}