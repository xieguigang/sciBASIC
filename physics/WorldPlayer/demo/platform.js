// ============================================================================
//  WorldPlayer 演示场景：带斜面的平台 + 鼠标左键生成随机三维刚体
//
//  本文件是"场景定义"，全部由 JavaScript 编写；VB.NET 宿主只提供引擎与 API。
//  可用 API：
//    world  : gravity / ground / addBox / addCuboid / addSphere / addCone /
//             addCylinder / addTetra / addStaticBox / addStaticRamp /
//             remove / clear / count / get / list / applyImpulse / ...
//    camera : orbit / zoom / angles / distance / focus / reset /
//             screenToRay / screenToPlane / info
//    input  : mouse / isDown
//    sim    : pause / resume / isPaused / step / time / contacts /
//             warmStarting / allowSleep / iterations / substeps
//    render : wireframe / grid / axes / background / hud
//    on(name, fn) 注册事件：mousedown / mousemove / mouseup / wheel / keydown
//
//  操作：左键在鼠标处生成随机物体；右键拖拽旋转视角；滚轮缩放；
//        C 清空、P 暂停、G 网格、W 线框、R 重置视角。
// ============================================================================

var PALETTE = ["#22D3EE", "#0EA5E9", "#6366F1", "#34D399",
               "#FBBF24", "#F87171", "#A78BFA", "#E2E8F0"];

var SHAPES = ["cube", "cuboid", "tetra", "sphere", "cone", "cylinder"];

var SPAWN_HEIGHT = 7.0;      // 生成平面高度
var GROUND_Y = 0.0;
var PLATFORM_TOP = 2.0;
var RAMP_RISE = 3.0;
var SPAWN_RANGE = 9.0;       // 生成点的水平范围限制

var spawned = 0;
var wireframe = false;
var gridVisible = true;

// ---------------------------------------------------------------- 工具函数

function clamp(v, lo, hi) {
    if (v < lo) { return lo; }
    if (v > hi) { return hi; }
    return v;
}

function pick(list) {
    return list[Math.floor(Math.random() * list.length)];
}

function rand(lo, hi) {
    return lo + Math.random() * (hi - lo);
}

function randomUnitVector() {
    var x = Math.random() * 2 - 1;
    var y = Math.random() * 2 - 1;
    var z = Math.random() * 2 - 1;
    var len = Math.sqrt(x * x + y * y + z * z);

    if (len < 0.001) { return [0, 1, 0]; }

    return [x / len, y / len, z / len];
}

// ---------------------------------------------------------------- 场景搭建

function setup() {
    world.gravity(0, -9.81, 0);

    // 地面：无限平面 y = 0
    world.ground(GROUND_Y);

    // 平台：一块静态长方体，顶面在 y = PLATFORM_TOP
    world.addStaticBox({
        x: 0, y: 1.0, z: 0,
        w: 16, h: 2, d: 16,
        color: "#1E293B",
        friction: 0.55
    });

    // 斜面：静态楔形斜坡架在平台顶面上，沿 +X 方向抬升
    //       物体落在斜面上会因重力沿斜面下滑，撞到低端边缘后落到平台顶面
    world.addStaticRamp({
        x: 0, y: PLATFORM_TOP, z: 0,
        w: 12, h: RAMP_RISE, d: 8,
        color: "#334155",
        friction: 0.3
    });

    // 高低两根立柱，用来观察物体与竖直面的碰撞
    world.addStaticBox({ x: -6.5, y: PLATFORM_TOP + 0.9, z: -5, w: 1, h: 1.8, d: 1, color: "#475569", friction: 0.5 });
    world.addStaticBox({ x: 6.5, y: PLATFORM_TOP + 0.9, z: 5, w: 1, h: 1.8, d: 1, color: "#475569", friction: 0.5 });

    render.background("#0B1220");
    render.grid(gridVisible);
    render.axes(true);
    render.wireframe(wireframe);
    render.hud("左键生成随机物体 · 右键拖拽旋转 · 滚轮缩放 · C 清空 · P 暂停 · W 线框");

    camera.angles(24, -38);
    camera.distance(46);
    camera.focus(0, 2.5, 0);

    print("场景已就绪：左键点击视口即可生成随机刚体");
}

// ---------------------------------------------------------------- 随机生成

function spawnRandom(sx, sy) {
    // 把屏幕点投射到高度为 SPAWN_HEIGHT 的水平面上，得到三维生成位置
    var p = camera.screenToPlane(sx, sy, 0, 1, 0, SPAWN_HEIGHT);

    if (!p) {
        print("无法把鼠标点投射到生成平面");
        return;
    }

    var kind = pick(SHAPES);
    var axis = randomUnitVector();
    var angle = rand(0, Math.PI * 2);
    var color = pick(PALETTE);
    var mass = rand(0.8, 3.0);
    var size = rand(0.85, 1.8);

    var cfg = {
        x: clamp(p.x, -SPAWN_RANGE, SPAWN_RANGE),
        y: SPAWN_HEIGHT,
        z: clamp(p.z, -SPAWN_RANGE, SPAWN_RANGE),
        mass: mass,
        friction: rand(0.2, 0.6),
        restitution: rand(0.0, 0.25),
        color: color,
        rot: { axis: axis, angle: angle }
    };

    var id = "";

    if (kind === "cube") {
        cfg.w = size;
        cfg.h = size;
        cfg.d = size;
        id = world.addBox(cfg);
    } else if (kind === "cuboid") {
        cfg.w = size;
        cfg.h = rand(0.4, 1.4);
        cfg.d = rand(0.8, 1.8);
        id = world.addCuboid(cfg);
    } else if (kind === "tetra") {
        cfg.edge = size * 1.5;
        id = world.addTetra(cfg);
    } else if (kind === "sphere") {
        cfg.r = size * 0.55;
        id = world.addSphere(cfg);
    } else if (kind === "cone") {
        cfg.r = size * 0.55;
        cfg.h = size * 1.3;
        cfg.seg = 12;
        id = world.addCone(cfg);
    } else {
        cfg.r = size * 0.5;
        cfg.h = size * 1.5;
        cfg.seg = 12;
        id = world.addCylinder(cfg);
    }

    spawned = spawned + 1;
    print("生成 " + kind + " -> " + id + "  位置 (" +
          Math.round(cfg.x * 100) / 100 + ", " +
          Math.round(cfg.y * 100) / 100 + ", " +
          Math.round(cfg.z * 100) / 100 + ")  总数 " + spawned);
}

// ---------------------------------------------------------------- 事件钩子

on("init", function () {
    setup();
});

on("mousedown", function (x, y, button) {
    // button: 1 = 左键, 2 = 右键, 3 = 中键
    if (button === 1) {
        spawnRandom(x, y);
    }
});

on("mousemove", function (x, y, dx, dy, button) {
    // 按住中键拖拽也可以平移焦点（可选交互）
});

on("wheel", function (delta) {
    // 宿主持有相机缩放，这里只做记录
});

on("keydown", function (key) {
    if (key === "C") {
        // 只清掉玩家生成的物体，保留地面 / 平台 / 斜坡等关卡几何
        world.clearDynamic();
        spawned = 0;
        print("已清空全部动态刚体（关卡几何保留）");
    } else if (key === "P") {
        if (sim.isPaused()) {
            sim.resume();
            print("继续仿真");
        } else {
            sim.pause();
            print("暂停仿真");
        }
    } else if (key === "G") {
        gridVisible = !gridVisible;
        render.grid(gridVisible);
        print(gridVisible ? "显示地面网格" : "隐藏地面网格");
    } else if (key === "W") {
        wireframe = !wireframe;
        render.wireframe(wireframe);
        print(wireframe ? "线框模式" : "实体模式");
    } else if (key === "R") {
        camera.reset();
        print("视角已重置");
    }
});
