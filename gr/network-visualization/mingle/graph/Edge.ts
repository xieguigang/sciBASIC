/*
         Class: Graph.Edge
    
         A <Graph> adjacence (or edge) connecting two <Graph.Nodes>.
    
         Implements:
    
         <Accessors> methods.
    
         See also:
    
         <Graph>, <Graph.Node>
    
         Properties:
    
          nodeFrom - A <Graph.Node> connected by this edge.
          nodeTo - Another  <Graph.Node> connected by this edge.
          data - Node data property containing a hash (i.e {}) with custom options.
    */
class Edge {

    constructor(public nodeFrom: Node, public nodeTo: Node, public data: edgeData = {}) { };

    public static fromJSON(graph: Graph, edgeJSON) {
        return new Edge(<Node>graph.get(edgeJSON.nodeFrom), <Node>graph.get(edgeJSON.nodeTo), edgeJSON.data);
    };

    toJSON() {
        return {
            nodeFrom: this.nodeFrom.id,
            nodeTo: this.nodeTo.id,
            data: this.data
        };
    }
}

interface edgeData {

}