#include "stdlib.dll"
#include "stdio.dll"

imports stdlib
imports stdio

' ---------------------------------------------------------------------------
' HOLA orthogonal layout demo (standard library edition)
'
'   Build a deliberately messy network (grid + chain + star + an isolated ring)
'   with net.addNode / net.addEdge, run the HOLA layout and render it to a PNG.
'
'   The starting positions are computed arithmetically (not randomly), so the
'   layout — and therefore the rendered image — is identical on every engine.
' ---------------------------------------------------------------------------

dim g = net.create()
dim node_id = 0

' deterministic "random" scatter points used as the HOLA starting positions
public function startX(i as integer) as double
    return ((i * 7919) mod 1000) + 0.5
end function

public function startY(i as integer) as double
    return ((i * 104729) mod 1000) + 0.5
end function

public sub addNode(label as string)
    dim x = startX(node_id)
    dim y = startY(node_id)

    call net.addNode(g, label, x, y)
    node_id += 1
end sub

' === 1) Grid block: 5 x 4 = 20 nodes ===
dim rows = 5, cols = 4
dim grid(rows - 1, cols - 1) as string

for r = 0 to rows - 1
    for c = 0 to cols - 1
        dim id = "n" & node_id

        grid(r, c) = id
        call addNode(id)
    next
next

' row / column adjacency edges
for r = 0 to rows - 1
    for c = 0 to cols - 1
        if c + 1 < cols then call net.addEdge(g, grid(r, c), grid(r, c + 1))
        if r + 1 < rows then call net.addEdge(g, grid(r, c), grid(r + 1, c))
    next
next

' cross-row / cross-column edges, deliberately creating crossings
call net.addEdge(g, grid(0, 0), grid(4, 3))
call net.addEdge(g, grid(0, 3), grid(4, 0))
call net.addEdge(g, grid(1, 1), grid(3, 2))
call net.addEdge(g, grid(2, 0), grid(2, 3))

' === 2) Chain structure with 12 nodes; both ends attach to the grid block ===
dim chain(11) as string

for i = 0 to 11
    chain(i) = "c" & i
    call addNode(chain(i))
next

for i = 0 to 10
    call net.addEdge(g, chain(i), chain(i + 1))
next

call net.addEdge(g, chain(0), grid(0, 0))
call net.addEdge(g, chain(11), grid(4, 3))

' === 3) Star structure: 1 hub + 7 leaves ===
dim hub = "hub"

call addNode(hub)
call net.addEdge(g, hub, grid(2, 1))

for i = 0 to 6
    dim leaf = "leaf" & i

    call addNode(leaf)
    call net.addEdge(g, hub, leaf)
next

' === 4) Independent 5-node ring component ===
dim ring = {"r0", "r1", "r2", "r3", "r4"}

for each rn in ring
    call addNode(rn)
next

for i = 0 to ring.length - 1
    call net.addEdge(g, ring(i), ring((i + 1) mod ring.length))
next

' === Run the HOLA layout ===
call net.hola(g)

call console.WriteLine($"=== HOLA complex network: {net.nodeCount(g)} nodes, {net.edgeCount(g)} edges, {net.bendCount(g)} edges with bends ===")

' === Render to PNG ===
call console.WriteLine("plot: " & net.draw(g, here("HOLA_complex_layout.png"), "1400,1400", true, 8, false, -1, 100))

' === Export the final node coordinates ===
dim coordinates = net.coordinates(g)

call console.WriteLine("csv : " & write.csv(coordinates, here("HOLA_node_positions.csv")))

call console.WriteLine("[HOLA] complex test done. see ./HOLA_complex_layout.png")
