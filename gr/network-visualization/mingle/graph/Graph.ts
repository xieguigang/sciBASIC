/*
 * File: Graph.js
 *
*/

/*
 Class: Graph
 
 A Graph Class that provides useful manipulation functions. You can find more manipulation methods in the <Graph.Util> object.
 
 An instance of this class can be accessed by using the *graph* parameter of any tree or graph visualization.
 
 Example:
 
 (start code js)
   //create new visualization
   var viz = new $jit.Viz(options);
   //load JSON data
   viz.loadJSON(json);
   //access model
   viz.graph; //<Graph> instance
 (end code)
 
 Implements:
 
 The following <Graph.Util> methods are implemented in <Graph>
 
  - <Graph.Util.getNode>
  - <Graph.Util.eachNode>
  - <Graph.Util.computeLevels>
  - <Graph.Util.eachBFS>
  - <Graph.Util.clean>
  - <Graph.Util.getClosestNodeToPos>
  - <Graph.Util.getClosestNodeToOrigin>
 
*/

class Graph {

    private nodes: {} = {};
    private edges: {} = {};

    constructor(public opt = null) {
        this.opt = merge({
            node: {}
        }, opt || {});
    }

    static fromJSON(json) {
        var nodes = json.nodes,
            edges = json.edges,
            graph = new Graph(),
            k;

        for (k in nodes) {
            nodes[k] = Node.fromJSON(nodes[k]);
        }

        graph.nodes = nodes;

        for (k in edges) {
            edges[k] = Edge.fromJSON(graph, edges[k]);
        }

        graph.edges = edges;

        return graph;
    }

    public each(action: (n: Node) => void) {
        for (let id in this.nodes) {
            action(this.nodes[id]);
        }
    }

    clear() {
        this.nodes = {};
        this.edges = {};
    }

    //serialize
    toJSON() {
        var nodes = [],
            edges = [],
            gNodes = this.nodes,
            gEdges = this.edges,
            k, from, to;

        for (k in gNodes) {
            nodes.push(gNodes[k].toJSON());
        }

        for (from in gEdges) {
            for (to in gEdges[from]) {
                edges.push(gEdges[from][to].toJSON());
            }
        }

        return { nodes: nodes, edges: edges };
    }

    /*
         Method: getNode
    
         Returns a <Graph.Node> by *id*.
    
         Parameters:
    
         id - (string) A <Graph.Node> id.
    
         Example:
    
         (start code js)
           var node = graph.getNode('nodeId');
         (end code)
    */
    getNode(id: string): Node | boolean {
        if (this.hasNode(id)) return this.nodes[id];
        return false;
    }

    /*
        Method: get
   
        An alias for <Graph.Util.getNode>. Returns a node by *id*.
   
        Parameters:
   
        id - (string) A <Graph.Node> id.
   
        Example:
   
        (start code js)
          var node = graph.get('nodeId');
        (end code)
   */
    get(id: string) {
        return this.getNode(id);
    }

    /*
      Method: getByName
   
      Returns a <Graph.Node> by *name*.
   
      Parameters:
   
      name - (string) A <Graph.Node> name.
   
      Example:
   
      (start code js)
        var node = graph.getByName('someName');
      (end code)
     */
    getByName(name: string) {
        for (var id in this.nodes) {
            var n: Node = this.nodes[id];
            if (n.name == name) return n;
        }
        return false;
    }

    /*
       Method: getEdge
    
       Returns a <Graph.Edge> object connecting nodes with ids *id* and *id2*.
    
       Parameters:
    
       id - (string) A <Graph.Node> id.
       id2 - (string) A <Graph.Node> id.
    */
    getEdge(id: string, id2: string) {
        if (id in this.edges) {
            return this.edges[id][id2];
        }
        return false;
    }

    /*
     Method: addNode
 
     Adds a node.
 
     Parameters:
 
      obj - An object with the properties described below
 
      id - (string) A node id
      name - (string) A node's name
      data - (object) A node's data hash
 
    See also:
    <Graph.Node>
 
  */
    addNode(obj) {
        if (!this.nodes[obj.id]) {
            var edges = this.edges[obj.id] = {};
            this.nodes[obj.id] = new Node(merge({
                'id': obj.id,
                'name': obj.name,
                'data': merge(obj.data || {}, {}),
                'adjacencies': edges
            }, this.opt.node));
        }
        return this.nodes[obj.id];
    }

    /*
     Method: addEdge
 
     Connects nodes specified by *obj* and *obj2*. If not found, nodes are created.
 
     Parameters:
 
      obj - (object) A <Graph.Node> object.
      obj2 - (object) Another <Graph.Node> object.
      data - (object) A data object. Used to store some extra information in the <Graph.Edge> object created.
 
    See also:
 
    <Graph.Node>, <Graph.Edge>
    */
    addEdge(obj, obj2, data = null) {
        if (!this.hasNode(obj.id)) { this.addNode(obj); }
        if (!this.hasNode(obj2.id)) { this.addNode(obj2); }
        obj = this.nodes[obj.id]; obj2 = this.nodes[obj2.id];
        if (!obj.adjacentTo(obj2)) {
            var adjsObj = this.edges[obj.id] = this.edges[obj.id] || {};
            var adjsObj2 = this.edges[obj2.id] = this.edges[obj2.id] || {};
            adjsObj[obj2.id] = adjsObj2[obj.id] = new Edge(obj, obj2, data);
            return adjsObj[obj2.id];
        }
        return this.edges[obj.id][obj2.id];
    }

    /*
     Method: removeNode
 
     Removes a <Graph.Node> matching the specified *id*.
 
     Parameters:
 
     id - (string) A node's id.
 
    */
    removeNode(id: string) {
        if (this.hasNode(id)) {
            delete this.nodes[id];
            var adjs = this.edges[id];
            for (var to in adjs) {
                delete this.edges[to][id];
            }
            delete this.edges[id];
        }
    }

    /*
         Method: removeEdge
    
         Removes a <Graph.Edge> matching *id1* and *id2*.
    
         Parameters:
    
         id1 - (string) A <Graph.Node> id.
         id2 - (string) A <Graph.Node> id.
    */
    removeEdge(id1: string, id2: string) {
        delete this.edges[id1][id2];
        delete this.edges[id2][id1];
    }

    /*
      Method: hasNode
 
      Returns a boolean indicating if the node belongs to the <Graph> or not.
 
      Parameters:
 
         id - (string) Node id.
    */
    hasNode(id: string) {
        return id in this.nodes;
    }

    /*
      Method: empty
  
      Empties the Graph
  
    */
    empty() {
        this.nodes = {};
        this.edges = {};
    }
}

//    //Append graph methods to <Graph>
//    ['get', 'getNode', 'each', 'eachNode', 'computeLevels', 'eachBFS', 'clean'].forEach(function (m) {
//        Graph.prototype[m] = function () {
//            return Graph.Util[m].apply(Graph.Util, [this].concat(Array.prototype.slice.call(arguments)));
//        };
//    });

//    //Append node methods to <Graph.Node>
//    ['eachEdge', 'eachLevel', 'eachSubgraph', 'eachSubnode', 'anySubnode', 'getSubnodes', 'getParents', 'isDescendantOf'].forEach(function (m) {
//        Graph.Node.prototype[m] = function () {
//            return Graph.Util[m].apply(Graph.Util, [this].concat(Array.prototype.slice.call(arguments)));
//        };
//    });

//    this.Graph = Graph;
//})();

