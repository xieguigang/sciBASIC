//General convenience functions and constants
const PHI = (1 + Math.sqrt(5)) / 2;

function $dist(a: number[], b: number[]) {
    var diffX = a[0] - b[0],
        diffY = a[1] - b[1];
    return Math.sqrt(diffX * diffX + diffY * diffY);
}

function $norm(a: number[]) {
    return Math.sqrt(a[0] * a[0] + a[1] * a[1]);
}

function $normalize(a: number[]) {
    var n = $norm(a);
    return $mult(a, 1 / n);
}

function $lerp(a: number[], b: number[], delta: number) {
    return [a[0] * (1 - delta) + b[0] * delta,
    a[1] * (1 - delta) + b[1] * delta];
}

function $add(a: number[], b: number[]) {
    return [a[0] + b[0], a[1] + b[1]];
}

function $sub(a: number[], b: number[]) {
    return [a[0] - b[0], a[1] - b[1]];
}

function $dot(a: number[], b: number[]) {
    return a[0] * b[0] + a[1] * b[1];
}

function $mult(a: number[], k: number) {
    return [a[0] * k, a[1] * k];
}