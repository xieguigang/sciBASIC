function $lerpPoint(from: number[][], to: number[][], delta: number) {
    return [$lerp(from[0], to[0], delta), $lerp(from[1], to[1], delta)];
}

function cloneJSON(json) {
    return JSON.parse(JSON.stringify(json));
}

function cloneEdge(json) {
    var i, l = json.length, ans = Array(json.length);
    for (i = 0; i < l; ++i) {
        ans[i] = {
            node: json[i].node,
            pos: json[i].pos,
            normal: json[i].normal && json[i].normal.slice()
        };
    }
    return ans;
}

//Extend generic Graph class with bundle methods and rendering options
function expandEdgesHelper(node: Node, array: any[], collect: any[]) {
    var coords = node.data.coords, i, l, p, ps;

    if (!array.length) {
        array.push([(coords[0] + coords[2]) / 2,
        (coords[1] + coords[3]) / 2]);
    }

    array.unshift([coords[0], coords[1]]);
    array.push([coords[2], coords[3]]);
    ps = node.data.parents;
    if (ps) {
        for (i = 0, l = ps.length; i < l; ++i) {
            expandEdgesHelper(ps[i], array.slice(), collect);
        }
    } else {
        collect.push(array);
    }
}

function setNormalVector(nodeFrom: Node, nodeTo: Node) {
    let node = nodeFrom || nodeTo, dir, coords;
    let normal: number[];

    if (!nodeFrom || !nodeTo) {
        coords = node.data.coords;
        dir = [coords[2] - coords[0], coords[3] - coords[1]];
        normal = [-dir[1], dir[0]];
        normal = $mult(normal, 1 / $norm(normal));
    }
    return normal;
}

function createPosItem(node: Node, pos, index, total): PosItem {
    return {
        node: node,//.toJSON(),
        pos: pos,
        normal: null
    };
}

interface PosItem {
    node: Node, pos: any, normal: number[]
}

//Extend generic Graph class with bundle methods and rendering options
function expandEdgesRichHelper(node: Node, array: any[], collect: any[]) {
    var coords = node.data.coords, i, l, p, ps, a, posItem;
    ps = node.data.parents;
    if (ps) {
        for (i = 0, l = ps.length; i < l; ++i) {
            a = array.slice();
            if (!a.length) {
                p = [(coords[0] + coords[2]) / 2, (coords[1] + coords[3]) / 2];
                posItem = createPosItem(node, p, i, l);
                a.push(posItem);
            }

            posItem = createPosItem(node, [coords[0], coords[1]], i, l);
            a.unshift(posItem);
            posItem = createPosItem(node, [coords[2], coords[3]], i, l);
            a.push(posItem);

            expandEdgesRichHelper(ps[i], a, collect);
        }
    } else {
        a = array.slice();
        if (!a.length) {
            p = [(coords[0] + coords[2]) / 2, (coords[1] + coords[3]) / 2];
            posItem = createPosItem(node, p, 0, 1);
            a.push(posItem);
        }

        posItem = createPosItem(node, [coords[0], coords[1]], 0, 1);
        a.unshift(posItem);
        posItem = createPosItem(node, [coords[2], coords[3]], 0, 1);
        a.push(posItem);

        collect.push(a);
    }
}