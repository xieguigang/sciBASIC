//General convenience functions and constants
const PHI = (1 + Math.sqrt(5)) / 2;

function $dist(a, b) {
    var diffX = a[0] - b[0],
        diffY = a[1] - b[1];
    return Math.sqrt(diffX * diffX + diffY * diffY);
}

function $norm(a) {
    return Math.sqrt(a[0] * a[0] + a[1] * a[1]);
}

function $normalize(a) {
    var n = $norm(a);
    return $mult(1 / n, a);
}

function $lerp(a, b, delta) {
    return [a[0] * (1 - delta) + b[0] * delta,
    a[1] * (1 - delta) + b[1] * delta];
}

function $add(a, b) {
    return [a[0] + b[0], a[1] + b[1]];
}

function $sub(a, b) {
    return [a[0] - b[0], a[1] - b[1]];
}

function $dot(a, b) {
    return a[0] * b[0] + a[1] * b[1];
}

function $mult(k, a) {
    return [a[0] * k, a[1] * k];
}