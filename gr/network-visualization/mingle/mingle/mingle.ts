
class Render {

    renderLine(ctx, edges, options) {
        options = options || {};
        var lineWidth = options.lineWidth || 1,
            fillStyle = options.fillStyle || 'gray',
            i, l, j, n, e, pos;

        ctx.fillStyle = fillStyle;
        ctx.lineWidth = lineWidth;
        for (i = 0, l = edges.length; i < l; ++i) {
            e = edges[i];
            ctx.beginPath();
            for (j = 0, n = e.length; j < n; ++j) {
                pos = e[j].unbundledPos;
                if (j == 0) {
                    ctx.moveTo(pos[0], pos[1]);
                } else {
                    ctx.lineTo(pos[0], pos[1]);
                }
            }
            ctx.stroke();
            ctx.closePath();
        }
    }

    renderQuadratic(ctx, edges, options) {
        options = options || {};
        var lineWidth = options.lineWidth || 1,
            fillStyle = options.fillStyle || 'gray',
            margin = (options.margin || 0) * (options.delta || 0),
            lengthBefore, lengthAfter,
            index, i, l, j, k, n, e, node, pos, pos0, pos1, pos2, pos3, pos01, pos02, pos03, pos04, colorFrom, colorTo, grd,
            midPos, quadStart, weightStart, posStart, nodeStart, posItem, posItemStart,
            dist, distMin, nodeArray, nodeLength;

        ctx.fillStyle = fillStyle;
        ctx.lineWidth = lineWidth;

        for (i = 0, l = edges.length; i < l; ++i) {
            e = edges[i];
            quadStart = null;
            posStart = null;
            nodeStart = e[0].node;
            ctx.lineWidth = (Math.max(1, nodeStart.data.weight) || 1) * (options.scale || 1);
            if (nodeStart.data.color && Array.isArray(nodeStart.data.color)) {
                colorFrom = nodeStart.data.color[0];
                colorTo = nodeStart.data.color[1];
                grd = ctx.createLinearGradient(nodeStart.data.coords[0],
                    nodeStart.data.coords[1],
                    nodeStart.data.coords[2],
                    nodeStart.data.coords[3]);
                grd.addColorStop(0, colorFrom);
                grd.addColorStop(0.4, colorFrom);
                grd.addColorStop(0.6, colorTo);
                grd.addColorStop(1, colorTo);
                ctx.strokeStyle = grd;
            } else {
                ctx.strokeStyle = nodeStart.data.color || ctx.strokeStyle;
            }
            ctx.globalAlpha = nodeStart.data.alpha == undefined ? 1 : nodeStart.data.alpha;
            ctx.beginPath();
            for (j = 0, n = e.length; j < n; ++j) {
                posItem = e[j];
                pos = posItem.unbundledPos;
                if (j !== 0) {
                    pos0 = posStart || e[j - 1].unbundledPos;
                    pos = this.adjustPosition(nodeStart.id, posItem, pos, margin, options.delta || 0);
                    midPos = $lerp(pos0, pos, 0.5);
                    pos1 = $lerp(pos0, midPos, j === 1 ? 0 : options.curviness || 0);
                    pos3 = pos;
                    pos2 = $lerp(midPos, pos3, j === n - 1 ? 1 : (1 - (options.curviness || 0)));
                    //ctx.lineCap = 'butt';//'round';
                    //ctx.beginPath();
                    if (quadStart) {
                        //ctx.strokeStyle = 'black';
                        ctx.moveTo(quadStart[0], quadStart[1]);
                        ctx.quadraticCurveTo(pos0[0], pos0[1], pos1[0], pos1[1]);
                        //ctx.stroke();
                        //ctx.closePath();
                    }
                    //ctx.beginPath();
                    //ctx.strokeStyle = 'red';
                    ctx.moveTo(pos1[0], pos1[1]);
                    ctx.lineTo(pos2[0], pos2[1]);
                    //ctx.stroke();
                    //ctx.closePath();
                    quadStart = pos2;
                    posStart = pos;
                }
            }
            ctx.stroke();
            ctx.closePath();
        }
    }

    adjustPosition(id, posItem, pos, margin, delta) {
        var nodeArray = posItem.node.data.nodeArray,
            epsilon = 1,
            nodeLength, index, lengthBefore,
            lengthAfter, k, node;

        if (nodeArray) {
            nodeLength = nodeArray.length;
            index = Infinity;
            lengthBefore = 0;
            lengthAfter = 0;
            for (k = 0; k < nodeLength; ++k) {
                node = nodeArray[k];
                if (node.id == id) {
                    index = k;
                }
                if (k < index) {
                    lengthBefore += (node.data.weight || 0) + margin;
                } else if (k > index) {
                    lengthAfter += (node.data.weight || 0) + margin;
                }
            }
            //remove -margin to get the line weight into account.
            //pos = $add(pos, $mult((lengthBefore - (lengthBefore + lengthAfter) / 2) * -margin, posItem.normal));
            pos = $add(pos, $mult((lengthBefore - (lengthBefore + lengthAfter) / 2) * Math.min(epsilon, delta), posItem.normal));
        }

        return pos;
    }

    renderBezier(ctx, edges, options) {
        options = options || {};
        var pct = options.curviness || 0,
            i, l, j, n, e, pos, midpoint, c1, c2, start, end;

        for (i = 0, l = edges.length; i < l; ++i) {
            e = edges[i];
            start = e[0].unbundledPos;
            ctx.strokeStyle = e[0].node.data.color || ctx.strokeStyle;
            ctx.lineWidth = e[0].node.data.weight || 1;
            midpoint = e[(e.length - 1) / 2].unbundledPos;
            if (e.length > 3) {
                c1 = e[1].unbundledPos;
                c2 = e[(e.length - 1) / 2 - 1].unbundledPos;
                end = $lerp(midpoint, c2, 1 - pct);
                ctx.beginPath();
                ctx.moveTo(start[0], start[1]);
                ctx.bezierCurveTo(c1[0], c1[1], c2[0], c2[1], end[0], end[1]);
                c1 = e[(e.length - 1) / 2 + 1].unbundledPos;
                c2 = e[e.length - 2].unbundledPos;
                end = e[e.length - 1].unbundledPos;
                if (1 - pct) {
                    //line to midpoint + pct of something
                    start = $lerp(midpoint, c1, 1 - pct);
                    ctx.lineTo(start[0], start[1]);
                }
                ctx.bezierCurveTo(c1[0], c1[1], c2[0], c2[1], end[0], end[1]);
                ctx.stroke();
                ctx.closePath();
            } else {
                ctx.beginPath();
                ctx.moveTo(start[0], start[1]);
                end = e[e.length - 1].unbundledPos;
                ctx.lineTo(end[0], end[1]);
            }
        }
    }
};

