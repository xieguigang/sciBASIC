/**
 * Edge bundling algorithm class.
*/
class Bundler {

    constructor(
        public options: options = <options>{ sort: true },
        public graph = new Graph(),
        public kdTree = null) {
    }

    setNodes(nodes) {
        var i, l, graph = this.graph;
        graph.clear();
        for (i = 0, l = nodes.length; i < l; ++i) {
            graph.addNode(nodes[i]);
        }
    }

    buildKdTree() {
        var nodeArray = [];
        this.graph.each(function (n) {
            var coords = n.data.coords;
            n.x = coords[0];
            n.y = coords[1];
            n.z = coords[2];
            n.w = coords[3];
            nodeArray.push(n);
        });

        this.kdTree = new KdTree(nodeArray, function (a, b) {
            var diff0 = a.x - b.x,
                diff1 = a.y - b.y,
                diff2 = a.z - b.z,
                diff3 = a.w - b.w;

            return Math.sqrt(diff0 * diff0 + diff1 * diff1 + diff2 * diff2 + diff3 * diff3);
        }, ['x', 'y', 'z', 'w']);
    }

    buildNearestNeighborGraph(k) {
        k = k || 10;
        var graph = this.graph, node, dist, kdTree;
        this.buildKdTree();
        kdTree = this.kdTree;
        graph.each(function (n) {
            var nodes = kdTree.nearest(n, k), i, l;
            for (i = 0, l = nodes.length; i < l; ++i) {
                node = nodes[i][0];
                dist = nodes[i][1];
                if (node.id != n.id) {
                    graph.addEdge(n, node);
                }
            }
        });
    }

    computeIntermediateNodePositions(node: Node) {
        var m1, m2, centroids: number[][]
        let a: number, b: number, c: number, tau: number, f, res;
        if (!node.data.nodes) {
            return;
        }
        centroids = this.getCentroids(node.data.nodes);
        f = this.costFunction.bind(this, node, centroids);
        a = 0;
        b = 1;
        c = 0.72; //because computers
        tau = 0.1;
        res = this.goldenSectionSearch(a, b, c, tau, f);
        f(res); //set m1 and m2;
    }

    costFunction(node: Node, centroids: number[][], x: number) {
        var top, bottom, m1, m2, ink, alpha, p;
        x /= 2;
        top = centroids[0];
        bottom = centroids[1];
        m1 = $lerp(top, bottom, x);
        m2 = $lerp(top, bottom, 1 - x);
        node.data.m1 = m1;
        node.data.m2 = m2;
        delete node.data.ink;
        ink = this.getInkValue(node);
        alpha = this.getMaxTurningAngleValue(node, m1, m2);
        p = this.options.angleStrength || 1.2;
        return ink * (1 + Math.sin(alpha) / p);
    }

    goldenSectionSearch(a: number, b: number, c: number, tau: number, f): number {
        var phi = PHI,
            resphi = 2 - PHI,
            abs = Math.abs, x: number;

        if (c - b > b - a) {
            x = b + resphi * (c - b);
        } else {
            x = b - resphi * (b - a);
        }
        if (abs(c - a) < tau * (abs(b) + abs(x))) {
            return (c + a) / 2;
        }
        if (f(x) < f(b)) {
            if (c - b > b - a) {
                return this.goldenSectionSearch(b, x, c, tau, f);
            }
            return this.goldenSectionSearch(a, x, b, tau, f);
        }
        if (c - b > b - a) {
            return this.goldenSectionSearch(a, b, x, tau, f);
        }
        return this.goldenSectionSearch(x, b, c, tau, f);
    }

    getCentroids(nodes: Node[]): number[][] {
        var topCentroid = [0, 0],
            bottomCentroid = [0, 0],
            coords: number[], i, l;

        for (i = 0, l = nodes.length; i < l; ++i) {
            coords = nodes[i].data.coords;
            topCentroid[0] += coords[0];
            topCentroid[1] += coords[1];
            bottomCentroid[0] += coords[2];
            bottomCentroid[1] += coords[3];
        }

        topCentroid[0] /= l;
        topCentroid[1] /= l;
        bottomCentroid[0] /= l;
        bottomCentroid[1] /= l;

        return [topCentroid, bottomCentroid];
    }

    getInkValue(node: Node, depth: number = 0) {
        var data = node.data,
            sqrt = Math.sqrt,
            coords, diffX, diffY,
            m1, m2, acum: number, i, l, nodes: Node[],
            ni: Node;

        //bundled node
        if (!depth && (data.bundle || data.nodes)) {
            nodes = data.bundle ? data.bundle.data.nodes : data.nodes;
            m1 = data.m1;
            m2 = data.m2;
            acum = 0;
            for (i = 0, l = nodes.length; i < l; ++i) {
                ni = nodes[i];
                coords = ni.data.coords;
                diffX = m1[0] - coords[0];
                diffY = m1[1] - coords[1];
                acum += $norm([diffX, diffY]);
                diffX = m2[0] - coords[2];
                diffY = m2[1] - coords[3];
                acum += $norm([diffX, diffY]);
                acum += this.getInkValue(ni, depth + 1);
            }
            if (!depth) {
                acum += $dist(m1, m2);
            }
            return (node.data.ink = acum);
        }

        //coalesced node
        if (data.parents) {
            nodes = data.parents;
            m1 = [data.coords[0], data.coords[1]];
            m2 = [data.coords[2], data.coords[3]];
            acum = 0;
            for (i = 0, l = nodes.length; i < l; ++i) {
                ni = nodes[i];
                coords = ni.data.coords;
                diffX = m1[0] - coords[0];
                diffY = m1[1] - coords[1];
                acum += $norm([diffX, diffY]);
                diffX = m2[0] - coords[2];
                diffY = m2[1] - coords[3];
                acum += $norm([diffX, diffY]);
                acum += this.getInkValue(ni, depth + 1);
            }
            //only add the distance if this is the first recursion
            if (!depth) {
                acum += $dist(m1, m2);
            }
            return (node.data.ink = acum);
        }

        //simple node
        if (depth) {
            return (node.data.ink = 0);
        }
        coords = node.data.coords;
        diffX = coords[0] - coords[2];
        diffY = coords[1] - coords[3];
        return (node.data.ink = $norm([diffX, diffY]));

    }

    getMaxTurningAngleValue(node: Node, m1: number[], m2: number[]) {
        var sqrt = Math.sqrt,
            abs = Math.abs,
            acos = Math.acos,
            m2Tom1 = [m1[0] - m2[0], m1[1] - m2[1]],
            m1Tom2 = [-m2Tom1[0], -m2Tom1[1]],
            m1m2Norm = $norm(m2Tom1),
            angle = 0, nodes, vec: number[], norm: number, dot, angleValue,
            x, y, coords: number[], i, l, n;

        if (node.data.bundle || node.data.nodes) {
            nodes = node.data.bundle ? (<Node>node.data.bundle).data.nodes : node.data.nodes;
            for (i = 0, l = nodes.length; i < l; ++i) {
                coords = nodes[i].data.coords;
                vec = [coords[0] - m1[0], coords[1] - m1[1]];
                norm = $norm(vec);
                dot = vec[0] * m2Tom1[0] + vec[1] * m2Tom1[1];
                angleValue = abs(acos(dot / norm / m1m2Norm));
                angle = angle < angleValue ? angleValue : angle;

                vec = [coords[2] - m2[0], coords[3] - m2[1]];
                norm = $norm(vec);
                dot = vec[0] * m1Tom2[0] + vec[1] * m1Tom2[1];
                angleValue = abs(acos(dot / norm / m1m2Norm));
                angle = angle < angleValue ? angleValue : angle;
            }

            return angle;
        }

        return -1;
    }

    getCombinedNode(node1: Node, node2: Node, data: nodeData = <any>{}): Node {
        node1 = <Node>node1.data.bundle || node1;
        node2 = <Node>node2.data.bundle || node2;

        var id = node1.id + '-' + node2.id,
            name = node1.name + '-' + node2.name,
            nodes1 = node1.data.nodes || [node1],
            nodes2 = node2.data.nodes || [node2],
            weight1 = node1.data.weight || 0,
            weight2 = node2.data.weight || 0,
            nodes: Node[] = [], ans: Node;

        if (node1.id == node2.id) {
            return node1;
        }
        nodes.push.apply(nodes, nodes1);
        nodes.push.apply(nodes, nodes2);

        data.nodes = nodes;
        data.nodeArray = (node1.data.nodeArray || []).concat(node2.data.nodeArray || []);
        data.weight = weight1 + weight2;
        ans = new Node({
            id: id,
            name: name,
            data: data
        });

        this.computeIntermediateNodePositions(ans);

        return ans;
    }

    coalesceNodes(nodes: Node[]): Node {
        var node = nodes[0],
            data = node.data,
            m1 = data.m1,
            m2 = data.m2,
            weight = nodes.reduce(function (acum, n) { return acum + (n.data.weight || 0); }, 0),
            coords = data.coords,
            bundle: Node = <Node>data.bundle,
            nodeArray: Node[] = [],
            i, l;

        if (m1) {
            coords = [m1[0], m1[1], m2[0], m2[1]];

            //flattened nodes for cluster.
            for (i = 0, l = nodes.length; i < l; ++i) {
                nodeArray.push.apply(nodeArray, nodes[i].data.nodeArray || (nodes[i].data.parents ? [] : [nodes[i]]));
            }

            if (this.options.sort) {
                nodeArray.sort(this.options.sort);
            }

            //if (!nodeArray.length || (typeof nodeArray[0].id == 'string')) {
            //debugger;
            //}

            return <any>{
                id: bundle.id,
                name: bundle.id,
                data: {
                    nodeArray: nodeArray,
                    parents: nodes,
                    coords: coords,
                    weight: weight,
                    parentsInk: bundle.data.ink
                }
            };
        }

        return nodes[0];
    }

    bundle(combinedNode, node1, node2) {
        var graph = this.graph;

        node1.data.bundle = combinedNode;
        node2.data.bundle = combinedNode;

        node1.data.ink = combinedNode.data.ink;
        node1.data.m1 = combinedNode.data.m1;
        node1.data.m2 = combinedNode.data.m2;
        //node1.data.nodeArray = combinedNode.data.nodeArray;

        node2.data.ink = combinedNode.data.ink;
        node2.data.m1 = combinedNode.data.m1;
        node2.data.m2 = combinedNode.data.m2;
        //node2.data.nodeArray = combinedNode.data.nodeArray;
    }

    updateGraph(graph, groupedNode, nodes, ids) {
        var i, l, n, connections,
            checkConnection = function (e) {
                var nodeToId = e.nodeTo.id;
                if (!ids[nodeToId]) {
                    connections.push(e.nodeTo);
                }
            };
        for (i = 0, l = nodes.length; i < l; ++i) {
            n = nodes[i];
            connections = [];
            n.eachEdge(checkConnection);
            graph.removeNode(n.id);
        }
        graph.addNode(groupedNode);
        for (i = 0, l = connections.length; i < l; ++i) {
            graph.addEdge(groupedNode, connections[i]);
        }
    }

    coalesceGraph() {
        var graph = this.graph,
            newGraph = new Graph(),
            groupsIds = {},
            maxGroup = -Infinity,
            nodes, i, l, ids, groupedNode, connections,
            updateGraph = this.updateGraph,
            coalesceNodes = this.coalesceNodes.bind(this);

        graph.each(function (node) {
            var group = node.data.group;
            if (maxGroup < group) {
                maxGroup = group;
            }
            if (!groupsIds[group]) {
                groupsIds[group] = {};
            }
            groupsIds[group][node.id] = node;
        });

        maxGroup++;
        while (maxGroup--) {
            ids = groupsIds[maxGroup];
            nodes = [];
            for (i in ids) {
                nodes.push(ids[i]);
            }
            if (nodes.length) {
                groupedNode = coalesceNodes(nodes);
                updateGraph(graph, groupedNode, nodes, ids);
            }
        }
    }

    getMaximumInkSavingNeighbor(n) {
        var nodeFrom = n,
            getInkValue = this.getInkValue.bind(this),
            inkFrom = getInkValue(nodeFrom),
            combineNodes = this.getCombinedNode.bind(this),
            inkTotal = Infinity,
            bundle: Node[] = Array(2),
            combinedBundle: Node;

        n.eachEdge(function (e) {
            var nodeTo = e.nodeTo,
                inkTo = getInkValue(nodeTo),
                combined: Node = combineNodes(nodeFrom, nodeTo),
                inkUnion = getInkValue(combined),
                inkValue = inkUnion - (inkFrom + inkTo);

            if (inkTotal > inkValue) {
                inkTotal = inkValue;
                bundle[0] = nodeFrom;
                bundle[1] = nodeTo;
                combinedBundle = combined;
            }
        });

        return {
            bundle: bundle,
            inkTotal: inkTotal,
            combined: combinedBundle
        };
    }

    MINGLE() {
        var edgeProximityGraph: Graph = this.graph,
            that = this,
            totalGain = 0,
            ungrouped = -1,
            gain = 0,
            k = 0,
            clean = function (n) { n.data.group = ungrouped; },
            nodeMingle = function (node: Node) {
                if (node.data.group == ungrouped) {
                    var ans = that.getMaximumInkSavingNeighbor(node),
                        bundle = ans.bundle,
                        u = bundle[0],
                        v = bundle[1],
                        combined = ans.combined,
                        gainUV = -ans.inkTotal;

                    //graph has been collapsed and is now only one node
                    if (!u && !v) {
                        gain = -Infinity;
                        return;
                    }

                    if (gainUV > 0) {
                        that.bundle(combined, u, v);
                        gain += gainUV;
                        if (v.data.group != ungrouped) {
                            u.data.group = v.data.group;
                        } else {
                            u.data.group = v.data.group = k;
                        }
                    } else {
                        u.data.group = k;
                    }
                    k++;
                }
            };

        do {
            gain = 0;
            k = 0;
            edgeProximityGraph.each(clean);
            edgeProximityGraph.each(nodeMingle);
            this.coalesceGraph();
            totalGain += gain;
        } while (gain > 0);
    }
}