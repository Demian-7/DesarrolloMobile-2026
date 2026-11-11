#ifndef ZONE_BACKGROUND_INCLUDED
#define ZONE_BACKGROUND_INCLUDED

// 4x4 ordered-dither threshold (0..1) for a pixel cell
float ZB_Bayer4(float2 cell)
{
    int x = ((int)cell.x) & 3;
    int y = ((int)cell.y) & 3;
    int v = (((x ^ y) & 1) << 3) | ((y & 1) << 2) | ((((x ^ y) >> 1) & 1) << 1) | ((y >> 1) & 1);
    return ((float)v + 0.5) / 16.0;
}

// Pixelated, dithered, palette-locked swirl.
void ZoneBackground_float(
    float2 uv,          // 0..1 across the quad
    float aspect,       // quad width / height
    float time,
    float pixelRows,    // background pixels from top to bottom of the screen
    float3 dark,
    float3 mid,
    float3 accent,
    float swirl,        // 0..1
    float pulse,        // 0..1, kicked by chain steps
    float2 parallax,    // shifts the swirl when the camera pans
    float brightness,   // overall cap, keeps the background below the cards
    out float3 color)
{
    // 1. Snap to a coarse pixel grid
    float2 grid = float2(pixelRows * aspect, pixelRows);
    float2 cell = floor(uv * grid);
    float2 puv = (cell + 0.5) / grid;

    // 2. Centered, aspect-corrected coordinates
    float2 p0 = (puv - 0.5) * float2(aspect, 1.0);
    float2 p = p0 + parallax;

    // 3. Spiral twist
    float r = length(p);
    float a = atan2(p.y, p.x) + time * 0.06 - swirl * 4.0 * r;
    float2 q = float2(cos(a), sin(a)) * r * 11.0;

    // 4. Domain warp: this is what makes it marbled
    float tt = time * 0.35;
    [unroll]
    for (int i = 0; i < 6; i++)
    {
        float fi = (float)i;
        q += 0.75 * float2(cos(1.3 * q.y + tt + fi * 1.7), sin(1.1 * q.x - tt * 0.8 + fi * 2.3));
        q += 0.40 * sin(q.yx * 0.9 + tt * 0.5);
    }

    // 5. Scalar field, dithered, then posterized into 4 palette bands
    float f = 0.5 + 0.5 * sin(q.x * 0.8 + q.y * 0.55 + length(q) * 0.30);
    float fd = f + (ZB_Bayer4(cell) - 0.5) * 0.14;
    float lvl = clamp(floor(fd * 4.0), 0.0, 3.0);

    float3 c0 = dark * 0.55;
    float3 c1 = dark;
    float3 c2 = dark * 0.45 + mid * 0.55;
    float3 c3 = mid;
    float3 col = lvl < 0.5 ? c0 : (lvl < 1.5 ? c1 : (lvl < 2.5 ? c2 : c3));

    // 6. Accent highlights on the brightest ridges, flaring on pulse
    float flare = saturate((fd - (0.96 - pulse * 0.10)) * 25.0);
    col += accent * flare * (0.18 + 0.22 * pulse);
    col += mid * 0.10 * pulse;

    // 7. Soft vignette, then the brightness cap
    float vig = 1.0 - 0.35 * pow(saturate(length(p0) * 1.1), 2.0);
    color = col * vig * brightness;
}

#endif
