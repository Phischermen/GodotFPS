shader_type canvas_item;

void fragment(){
	float alpha = texture(TEXTURE, UV).a;
	vec3 col = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;
	COLOR = vec4(1f - col.r, 1f - col.g, 1f - col.b, alpha);
}