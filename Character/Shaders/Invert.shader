shader_type canvas_item;

uniform float blend;

void fragment(){
	float alpha = texture(TEXTURE, UV).a;
	vec3 screen_col = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;
	vec3 screen_col_inv = vec3(1f - screen_col.r, 1f - screen_col.g, 1f - screen_col.b);

	COLOR.r = mix(screen_col_inv.r, COLOR.r, blend);
	COLOR.g = mix(screen_col_inv.g, COLOR.g, blend);
	COLOR.b = mix(screen_col_inv.b, COLOR.b, blend);
	COLOR.a = alpha;
}