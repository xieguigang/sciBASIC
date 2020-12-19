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

    constructor(nodeFrom, nodeTo, data) {
        this.nodeFrom = nodeFrom;
        this.nodeTo = nodeTo;
        this.data = data || {};
    };

    fromJSON(graph, edgeJSON) {
        return new Graph.Edge(graph.get(edgeJSON.nodeFrom),
            graph.get(edgeJSON.nodeTo),
            edgeJSON.data);
    };

    toJSON() {
        return {
            nodeFrom: this.nodeFrom.id,
            nodeTo: this.nodeTo.id,
            data: this.data
        };
    }
}