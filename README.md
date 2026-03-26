# ShadersFinalProject

| Participant | Nickname       | Role                                   |
|-------------|----------------|----------------------------------------|
| Maayan      | Maayana24      | Scene Setup, Comparison Compute Shader |
| Eden        | TheAmazingGssi | VFX                                    |
| Shlomo      | cN3rd          | Shaders (Tessellation, Painting, etc)  |

## Implementation Working Assumptions

All shaders share a single wool RenderTexture with this channel layout:
- **RGB** — wool color
- **Alpha** — wool growth/displacement amount (0 = no wool, 1 = full growth)

Brush interaction works through UV-space raycasting — the player clicks the sheep mesh, the hit point is converted to UVs, and those UVs are passed to whichever shader needs them.

The project runs on URP (Universal Render Pipeline).

---

## Shader Breakdown

### 1. WoolInit.shader — Procedural Wool Initialization

This shader generates the starting wool texture using Perlin noise so the sheep doesn't start with a flat, uniform look. It uses a hash-based gradient function to get smooth, natural-looking variation.

There's an edge fade near the UV boundaries (`_EdgeFadeStart`) so you don't get harsh cutoffs at the mesh seams. We randomize the noise each session with `_OffsetX`/`_OffsetY` offsets, and `_NoiseMin`/`_NoiseMax` keep the growth values in a reasonable range (0.4–1.0 by default).

### 2. WoolTessellation.shader + Tessellation.hlsl — Tessellation & Displacement

This is the main rendering shader — it takes the wool texture and turns it into actual 3D geometry using hardware tessellation.

The tessellation level adjusts based on camera distance (`CalcTessFactor()` lerps between `_TessMin` and `_TessMax`), so we get more detail up close without wasting triangles in the distance. Each tessellated vertex gets pushed out along its normal by `growth * _MaxDisplacement` (0.4 in the scene).

There's also a brush preview — when the player hovers over the sheep, a white rim shows where their brush will paint. The shader has two passes: ForwardLit for the actual PBR shading and ShadowCaster so the displaced wool casts correct shadows.

### 3. BrushPaint.shader — Brush Tool

Handles painting onto the wool RenderTexture (applied through `Graphics.Blit`). The brush uses quadratic distance falloff so the edges are soft and feathered. Default brush radius in the scene is 0.15 (UV space) with strength 15.

There are three paint modes set by `_PaintMode`:
- **0 (Growth Add)** — increases alpha, making wool grow
- **1 (Growth Subtract)** — decreases alpha, shaving the wool
- **2 (Color)** — blends `_BrushColor` into RGB

### 4. PhotoCompareShader.compute — Scoring

This compute shader (dispatched in 8x8 thread groups) does the actual image comparison between the player's wool and the reference.

It checks the growth and color channels separately for each pixel — growth difference vs `_AlphaTolerance`, average color difference vs `_ColorTolerance`. These get combined with configurable weights (in the scene: 0.3 for growth, 0.7 for color), and if the weighted sum exceeds `_WrongThreshold` (0.7), that pixel counts as wrong.

The shader outputs wrong pixel count and total pixels through a StructuredBuffer. Final score is `1 - (wrong / total)`, converted to percent (e.g. `0.5` is 50%).

## Difficulty Scaling

There's a toggle in the UI for easy/hard mode. It changes two things:

| | Easy | Hard |
|--|------|------|
| Comparison resolution | 64x64 | 512x512 |
| Tolerance multiplier | 1.0 | 0.5 |

The base tolerances (in the scene: displacement 0.587, color 0.351) get multiplied by the difficulty scale before being passed to the compute shader:

```
alphaTolerance = displacementTolerance * difficultyScale
colorTolerance = colorTolerance * difficultyScale
```

So on easy mode, the comparison uses a small texture and full tolerance — small mistakes get averaged away and larger ones are forgiven. On hard mode, tolerances are cut in half and the comparison runs at 8x the resolution, so both fine details and color accuracy matter a lot more.

This logic lives in `PhotoCompare.cs`, which listens for difficulty change events from `UIManager`.