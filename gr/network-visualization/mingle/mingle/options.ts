interface options {
    sort: boolean;
    lineWidth: number;
    fillStyle: string;
    margin: number;
    delta: number;
    scale: number;
    curviness: number;
    angleStrength: number;
}

interface context {

    fillStyle: string;
    lineWidth: number;
    strokeStyle: string | Gradient;
    globalAlpha: number;

    beginPath();
    createLinearGradient(a: number, b: number, c: number, d: number): Gradient;
    moveTo(a: number, b: number);
    quadraticCurveTo(a: number, b: number, c: number, d: number);
    lineTo(a: number, b: number);
    stroke();
    closePath();
    bezierCurveTo(a: number, b: number, c: number, d: number, end1: number, end2: number);
}

interface Gradient {
    addColorStop(a: number, color: string);
}