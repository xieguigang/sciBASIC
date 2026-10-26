import stdlib from '../../../../.nuget/net10.0/stdlib.dll';
import stdio from '../../../../.nuget/net10.0/stdio.dll';

// ---------------------------------------------------------------------------
// HOLA orthogonal layout demo — the njs twin of hola_layout.vb
//
// The starting positions are computed arithmetically, so the layout and the
// rendered image are identical on both engines.
// ---------------------------------------------------------------------------

const g = stdlib.net.create();
let nodeId = 0;

const startX = (i) => ((i * 7919) % 1000) + 0.5;
const startY = (i) => ((i * 104729) % 1000) + 0.5;

const addNode = (label) => {
    stdlib.net.addNode(g, label, startX(nodeId), startY(nodeId));
    nodeId += 1;
};

// === 1) grid block: 5 x 4 = 20 nodes ===
const rows = 5;
const cols = 4;
const grid = [];

for (let r = 0; r < rows; r++) {
    const line = [];

    for (let c = 0; c < cols; c++) {
        const id = 'n' + nodeId;

        line.push(id);
        addNode(id);
    }

    grid.push(line);
}

// row / column adjacency edges
for (let r = 0; r < rows; r++) {
    for (let c = 0; c < cols; c++) {
        if (c + 1 < cols) stdlib.net.addEdge(g, grid[r][c], grid[r][c + 1]);
        if (r + 1 < rows) stdlib.net.addEdge(g, grid[r][c], grid[r + 1][c]);
    }
}

// crossings
stdlib.net.addEdge(g, grid[0][0], grid[4][3]);
stdlib.net.addEdge(g, grid[0][3], grid[4][0]);
stdlib.net.addEdge(g, grid[1][1], grid[3][2]);
stdlib.net.addEdge(g, grid[2][0], grid[2][3]);

// === 2) chain of 12 nodes, both ends attached to the grid ===
const chain = [];

for (let i = 0; i <= 11; i++) {
    chain.push('c' + i);
    addNode(chain[i]);
}

for (let i = 0; i <= 10; i++) {
    stdlib.net.addEdge(g, chain[i], chain[i + 1]);
}

stdlib.net.addEdge(g, chain[0], grid[0][0]);
stdlib.net.addEdge(g, chain[11], grid[4][3]);

// === 3) star: 1 hub + 7 leaves ===
const hub = 'hub';

addNode(hub);
stdlib.net.addEdge(g, hub, grid[2][1]);

for (let i = 0; i <= 6; i++) {
    const leaf = 'leaf' + i;

    addNode(leaf);
    stdlib.net.addEdge(g, hub, leaf);
}

// === 4) independent 5-node ring ===
const ring = ['r0', 'r1', 'r2', 'r3', 'r4'];

for (const rn of ring) {
    addNode(rn);
}

for (let i = 0; i < ring.length; i++) {
    stdlib.net.addEdge(g, ring[i], ring[(i + 1) % ring.length]);
}

// === run the HOLA layout ===
stdlib.net.hola(g);

console.log(`=== HOLA complex network: ${stdlib.net.nodeCount(g)} nodes, ` +
            `${stdlib.net.edgeCount(g)} edges, ${stdlib.net.bendCount(g)} edges with bends ===`);

// === render to png ===
console.log('plot: ' + stdlib.net.draw(g, here('HOLA_complex_layout.png'), '1400,1400', true, 8, false, -1, 100));

// === export the final node coordinates ===
const coordinates = stdlib.net.coordinates(g);

console.log('csv : ' + stdio.write.csv(coordinates, here('HOLA_node_positions.csv')));

console.log('[HOLA] complex test done. see ./HOLA_complex_layout.png');
