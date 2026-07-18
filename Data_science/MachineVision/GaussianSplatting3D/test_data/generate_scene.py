"""
generate_scene.py
=================
Generate multi-view PNG images of a synthetic 3D scene for testing the
VB.NET 3D Gaussian Splatting algorithm.

The scene is composed of:
  * A ground plane (checkerboard)
  * A central colored cube
  * A sphere on top of the cube
  * A torus floating beside the cube
  * A small cone

For each camera view we save:
  * <view_NN>.png          - the rendered RGB image (H x W x 3)
  * cameras.json           - camera intrinsics + extrinsics for every view
  * ground_truth.ply       - the ground-truth 3D point cloud (for validation)
  * scene_info.json        - scene metadata (image size, num views, etc.)

The renderer is a simple software rasterizer:
  1. Each triangle of the scene is sampled into a dense 3D point cloud
     (one point per ~0.02 world units, with the surface color).
  2. For each camera, points are projected to image space using a pinhole
     camera model and z-buffered to produce a clean RGB image.

This produces clean, occlusion-correct images that are perfect for
validating a 3D-Gaussian-Splatting reconstruction pipeline.
"""

import os
import json
import math
import numpy as np
from PIL import Image

# ---------------------------------------------------------------------------
# Output configuration
# ---------------------------------------------------------------------------
OUT_DIR = "/home/z/my-project/download/test_data"
os.makedirs(OUT_DIR, exist_ok=True)

IMG_W = 320
IMG_H = 240
NUM_VIEWS = 12            # number of camera views around the scene
CAM_RADIUS = 6.0          # camera distance from scene origin
CAM_HEIGHT = 2.0          # camera height above the ground plane
FOV_DEG = 50.0            # vertical field of view in degrees

# ---------------------------------------------------------------------------
# 3D scene definition
# ---------------------------------------------------------------------------
def make_cube(center, size, color):
    """Return vertices and triangle indices of an axis-aligned cube."""
    cx, cy, cz = center
    s = size * 0.5
    v = np.array([
        [cx - s, cy - s, cz - s], [cx + s, cy - s, cz - s],
        [cx + s, cy + s, cz - s], [cx - s, cy + s, cz - s],
        [cx - s, cy - s, cz + s], [cx + s, cy - s, cz + s],
        [cx + s, cy + s, cz + s], [cx - s, cy + s, cz + s],
    ], dtype=np.float64)
    f = np.array([
        [0, 1, 2], [0, 2, 3],   # back
        [4, 5, 6], [4, 6, 7],   # front
        [0, 1, 5], [0, 5, 4],   # bottom
        [3, 2, 6], [3, 6, 7],   # top
        [0, 3, 7], [0, 7, 4],   # left
        [1, 2, 6], [1, 6, 5],   # right
    ], dtype=np.int32)
    return v, f, np.array(color, dtype=np.float32)


def make_sphere(center, radius, color, segments=16, rings=12):
    cx, cy, cz = center
    verts = []
    for r in range(rings + 1):
        theta = math.pi * r / rings
        for s in range(segments):
            phi = 2 * math.pi * s / segments
            verts.append([
                cx + radius * math.sin(theta) * math.cos(phi),
                cy + radius * math.cos(theta),
                cz + radius * math.sin(theta) * math.sin(phi),
            ])
    verts = np.array(verts, dtype=np.float64)
    faces = []
    for r in range(rings):
        for s in range(segments):
            i00 = r * segments + s
            i01 = r * segments + (s + 1) % segments
            i10 = (r + 1) * segments + s
            i11 = (r + 1) * segments + (s + 1) % segments
            faces.append([i00, i10, i11])
            faces.append([i00, i11, i01])
    return verts, np.array(faces, dtype=np.int32), np.array(color, dtype=np.float32)


def make_torus(center, R, r, color, segments=24, rings=16):
    cx, cy, cz = center
    verts = []
    for i in range(segments):
        u = 2 * math.pi * i / segments
        for j in range(rings):
            v = 2 * math.pi * j / rings
            x = (R + r * math.cos(v)) * math.cos(u)
            y = r * math.sin(v)
            z = (R + r * math.cos(v)) * math.sin(u)
            verts.append([cx + x, cy + y, cz + z])
    verts = np.array(verts, dtype=np.float64)
    faces = []
    for i in range(segments):
        for j in range(rings):
            i00 = i * rings + j
            i01 = i * rings + (j + 1) % rings
            i10 = ((i + 1) % segments) * rings + j
            i11 = ((i + 1) % segments) * rings + (j + 1) % rings
            faces.append([i00, i10, i11])
            faces.append([i00, i11, i01])
    return verts, np.array(faces, dtype=np.int32), np.array(color, dtype=np.float32)


def make_cone(center, radius, height, color, segments=20):
    cx, cy, cz = center
    base_y = cy - height * 0.5
    tip_y = cy + height * 0.5
    verts = [[cx, tip_y, cz]]
    for i in range(segments):
        a = 2 * math.pi * i / segments
        verts.append([cx + radius * math.cos(a), base_y, cz + radius * math.sin(a)])
    verts.append([cx, base_y, cz])  # center for fan triangulation of base
    verts = np.array(verts, dtype=np.float64)
    faces = []
    # side faces
    for i in range(segments):
        faces.append([0, 1 + i, 1 + (i + 1) % segments])
    # base faces (fan)
    center_idx = len(verts) - 1
    for i in range(segments):
        faces.append([center_idx, 1 + (i + 1) % segments, 1 + i])
    return verts, np.array(faces, dtype=np.int32), np.array(color, dtype=np.float32)


def make_ground_plane(y, size, color1, color2, tiles=8):
    """Checkerboard ground plane made of two triangles per tile."""
    verts = []
    faces = []
    colors = []
    half = size * 0.5
    step = size / tiles
    for i in range(tiles):
        for j in range(tiles):
            x0 = -half + i * step
            z0 = -half + j * step
            x1 = x0 + step
            z1 = z0 + step
            base = len(verts)
            verts.extend([
                [x0, y, z0], [x1, y, z0], [x1, y, z1], [x0, y, z1]
            ])
            faces.extend([[base, base + 1, base + 2], [base, base + 2, base + 3]])
            c = color1 if (i + j) % 2 == 0 else color2
            colors.extend([c, c, c, c])
    return (np.array(verts, dtype=np.float64),
            np.array(faces, dtype=np.int32),
            np.array(colors, dtype=np.float32))


# ---------------------------------------------------------------------------
# Build the scene
# ---------------------------------------------------------------------------
def build_scene():
    meshes = []
    # Ground plane (checkerboard)
    g_v, g_f, g_c = make_ground_plane(
        y=-1.5, size=8.0,
        color1=np.array([0.85, 0.82, 0.78], dtype=np.float32),
        color2=np.array([0.35, 0.32, 0.30], dtype=np.float32),
        tiles=8,
    )
    meshes.append((g_v, g_f, g_c))
    # Central cube
    v, f, c = make_cube(center=[0.0, -0.5, 0.0], size=1.4, color=[0.85, 0.25, 0.20])
    meshes.append((v, f, c))
    # Sphere on top of cube
    v, f, c = make_sphere(center=[0.0, 0.7, 0.0], radius=0.55, color=[0.20, 0.55, 0.90])
    meshes.append((v, f, c))
    # Torus floating beside the cube
    v, f, c = make_torus(center=[1.8, 0.0, 0.3], R=0.55, r=0.18, color=[0.95, 0.75, 0.15])
    meshes.append((v, f, c))
    # Small cone on the other side
    v, f, c = make_cone(center=[-1.8, -0.6, 0.2], radius=0.45, height=1.0, color=[0.30, 0.80, 0.40])
    meshes.append((v, f, c))
    return meshes


# ---------------------------------------------------------------------------
# Convert meshes to a dense 3D point cloud (one point per surface sample)
# ---------------------------------------------------------------------------
def sample_points_from_meshes(meshes, density=40.0):
    """
    Sample a dense point cloud from triangle meshes.
    density: number of samples per world unit area.
    """
    all_pts = []
    all_cols = []
    for verts, faces, colors in meshes:
        # If colors has one row per vertex use it, else broadcast
        if colors.ndim == 1:
            colors = np.tile(colors, (len(verts), 1))
        for f in faces:
            v0, v1, v2 = verts[f[0]], verts[f[1]], verts[f[2]]
            c0, c1, c2 = colors[f[0]], colors[f[1]], colors[f[2]]
            # Triangle area
            area = 0.5 * np.linalg.norm(np.cross(v1 - v0, v2 - v0))
            n_samples = max(1, int(area * density))
            r1 = np.random.rand(n_samples, 1)
            r2 = np.random.rand(n_samples, 1)
            sqrt_r1 = np.sqrt(r1)
            u = 1.0 - sqrt_r1
            v = sqrt_r1 * (1.0 - r2)
            w = sqrt_r1 * r2
            pts = u * v0 + v * v1 + w * v2
            cols = u * c0 + v * c1 + w * c2
            all_pts.append(pts)
            all_cols.append(cols)
    pts = np.concatenate(all_pts, axis=0)
    cols = np.concatenate(all_cols, axis=0)
    return pts, cols


# ---------------------------------------------------------------------------
# Camera utilities
# ---------------------------------------------------------------------------
def look_at(eye, target, up):
    """
    Return a 4x4 world-to-camera transform using OpenCV convention:
      * +X right, +Y down, +Z forward (camera looks along +Z).
    Points in front of the camera have positive Z in camera space.
    """
    eye = np.asarray(eye, dtype=np.float64)
    target = np.asarray(target, dtype=np.float64)
    up = np.asarray(up, dtype=np.float64)
    forward = target - eye
    forward = forward / np.linalg.norm(forward)
    # right = cross(forward, up) gives a left-handed basis; for OpenCV
    # (right-handed with +Y down) we use right = cross(up, forward) then
    # y_cam = cross(forward, right) which yields the "down" vector.
    right = np.cross(up, forward)
    right = right / np.linalg.norm(right)
    down = np.cross(forward, right)
    R = np.stack([right, down, forward], axis=0)  # 3x3, rows: right/down/forward
    t = -R @ eye
    M = np.eye(4)
    M[:3, :3] = R
    M[:3, 3] = t
    return M, R, t


def build_camera_pose(angle_deg):
    """Camera orbits the scene on a circle of radius CAM_RADIUS."""
    a = math.radians(angle_deg)
    eye = [CAM_RADIUS * math.cos(a), CAM_HEIGHT, CAM_RADIUS * math.sin(a)]
    target = [0.0, 0.0, 0.0]
    up = [0.0, 1.0, 0.0]
    M, R, t = look_at(eye, target, up)
    return {
        "eye": eye,
        "target": target,
        "up": up,
        "world_to_camera": M.tolist(),
        "R": R.tolist(),
        "t": t.tolist(),
    }


def build_intrinsics():
    fovy = math.radians(FOV_DEG)
    fy = (IMG_H * 0.5) / math.tan(fovy * 0.5)
    fx = fy  # square pixels
    cx = IMG_W * 0.5
    cy = IMG_H * 0.5
    return {"fx": fx, "fy": fy, "cx": cx, "cy": cy, "width": IMG_W, "height": IMG_H}


# ---------------------------------------------------------------------------
# Software rasterizer (z-buffered point splat)
# ---------------------------------------------------------------------------
def render_view(pts, cols, cam, intr):
    """
    Render the point cloud from a given camera pose using a z-buffer.
    Returns an (H, W, 3) uint8 image.
    """
    M = np.array(cam["world_to_camera"], dtype=np.float64)
    R = np.array(cam["R"], dtype=np.float64)
    t = np.array(cam["t"], dtype=np.float64)

    # World -> camera
    pts_cam = (R @ pts.T).T + t  # (N, 3)

    # Cull points behind the camera
    in_front = pts_cam[:, 2] > 0.1
    pts_cam = pts_cam[in_front]
    cols = cols[in_front]

    if len(pts_cam) == 0:
        return np.zeros((IMG_H, IMG_W, 3), dtype=np.uint8)

    # Project to pixel coordinates
    fx, fy = intr["fx"], intr["fy"]
    cx, cy = intr["cx"], intr["cy"]
    z = pts_cam[:, 2]
    u = (fx * pts_cam[:, 0] / z) + cx
    v = (fy * pts_cam[:, 1] / z) + cy

    # Sort by depth (far -> near) so nearer points overwrite farther ones
    order = np.argsort(-z)
    u = u[order]
    v = v[order]
    z = z[order]
    cols = cols[order]

    ui = np.round(u).astype(np.int32)
    vi = np.round(v).astype(np.int32)
    valid = (ui >= 0) & (ui < IMG_W) & (vi >= 0) & (vi < IMG_H)
    ui, vi, z, cols = ui[valid], vi[valid], z[valid], cols[valid]

    img = np.zeros((IMG_H, IMG_W, 3), dtype=np.float32)
    zbuf = np.full((IMG_H, IMG_W), np.inf, dtype=np.float32)

    # Splat each point as a small disk (radius 1 pixel) for nicer images
    for i in range(len(ui)):
        x, y = ui[i], vi[i]
        d = z[i]
        if d < zbuf[y, x]:
            zbuf[y, x] = d
            img[y, x] = cols[i]
        # 4-neighbors for slightly thicker splats
        for dx, dy in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
            xx, yy = x + dx, y + dy
            if 0 <= xx < IMG_W and 0 <= yy < IMG_H and d < zbuf[yy, xx]:
                zbuf[yy, xx] = d
                img[yy, xx] = cols[i]

    # Background: light gray gradient (sky)
    bg = np.zeros((IMG_H, IMG_W, 3), dtype=np.float32)
    for y in range(IMG_H):
        t = y / max(1, IMG_H - 1)
        bg[y, :, 0] = 0.85 - 0.4 * t
        bg[y, :, 1] = 0.90 - 0.5 * t
        bg[y, :, 2] = 1.00 - 0.4 * t
    mask = np.isinf(zbuf)
    img[mask] = bg[mask]

    return (np.clip(img, 0, 1) * 255).astype(np.uint8)


# ---------------------------------------------------------------------------
# PLY writer (ASCII) for ground-truth point cloud
# ---------------------------------------------------------------------------
def write_ply(path, pts, cols):
    with open(path, "w") as f:
        f.write("ply\n")
        f.write("format ascii 1.0\n")
        f.write(f"element vertex {len(pts)}\n")
        f.write("property float x\n")
        f.write("property float y\n")
        f.write("property float z\n")
        f.write("property uchar red\n")
        f.write("property uchar green\n")
        f.write("property uchar blue\n")
        f.write("end_header\n")
        for p, c in zip(pts, cols):
            f.write(f"{p[0]:.6f} {p[1]:.6f} {p[2]:.6f} "
                    f"{int(c[0]*255)} {int(c[1]*255)} {int(c[2]*255)}\n")


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
def main():
    np.random.seed(42)
    print("[1/4] Building scene meshes ...")
    meshes = build_scene()

    print("[2/4] Sampling dense point cloud ...")
    pts, cols = sample_points_from_meshes(meshes, density=80.0)
    print(f"      -> {len(pts)} points sampled")

    print("[3/4] Writing ground truth PLY ...")
    write_ply(os.path.join(OUT_DIR, "ground_truth.ply"), pts, cols)

    intr = build_intrinsics()
    cameras = []
    print(f"[4/4] Rendering {NUM_VIEWS} views ...")
    for i in range(NUM_VIEWS):
        angle = 360.0 * i / NUM_VIEWS
        cam = build_camera_pose(angle)
        cam["view_id"] = i
        cam["angle_deg"] = angle
        img = render_view(pts, cols, cam, intr)
        fname = f"view_{i:02d}.png"
        Image.fromarray(img).save(os.path.join(OUT_DIR, fname))
        cameras.append(cam)
        print(f"      view_{i:02d}.png  angle={angle:6.1f} deg  "
              f"eye=({cam['eye'][0]:+.2f},{cam['eye'][1]:+.2f},{cam['eye'][2]:+.2f})")

    # Save camera parameters
    with open(os.path.join(OUT_DIR, "cameras.json"), "w") as f:
        json.dump({"intrinsics": intr, "cameras": cameras}, f, indent=2)

    # Save scene info
    scene_info = {
        "image_width": IMG_W,
        "image_height": IMG_H,
        "num_views": NUM_VIEWS,
        "cam_radius": CAM_RADIUS,
        "cam_height": CAM_HEIGHT,
        "fov_deg": FOV_DEG,
        "ground_truth_points": len(pts),
        "scene_objects": [
            "ground_plane (checkerboard)",
            "cube (red)",
            "sphere (blue)",
            "torus (yellow)",
            "cone (green)",
        ],
    }
    with open(os.path.join(OUT_DIR, "scene_info.json"), "w") as f:
        json.dump(scene_info, f, indent=2)

    print("\nDone. Output written to:", OUT_DIR)
    print(f"  - {NUM_VIEWS} PNG images (view_00.png ... view_{NUM_VIEWS-1:02d}.png)")
    print("  - cameras.json (intrinsics + extrinsics)")
    print("  - ground_truth.ply (ground-truth 3D point cloud)")
    print("  - scene_info.json (scene metadata)")


if __name__ == "__main__":
    main()
