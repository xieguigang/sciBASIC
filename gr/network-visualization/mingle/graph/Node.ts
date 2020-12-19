/*
       Class: Graph.Node
  
       A <Graph> node.
  
       Implements:
  
       <Accessors> methods.
  
       The following <Graph.Util> methods are implemented by <Graph.Node>
  
      - <Graph.Util.eachEdge>
      - <Graph.Util.eachLevel>
      - <Graph.Util.eachSubgraph>
      - <Graph.Util.eachSubnode>
      - <Graph.Util.anySubnode>
      - <Graph.Util.getSubnodes>
      - <Graph.Util.getParents>
      - <Graph.Util.isDescendantOf>
  */
class Node {
    constructor(opt) {
        var innerOptions = {
            'id': '',
            'name': '',
            'data': {},
            'adjacencies': {}
        };
        extend(this, merge(innerOptions, opt));
    };

    fromJSON(json) {
        return new Graph.Node(json);
    };


    toJSON() {
        return {
            id: this.id,
            name: this.name,
            data: this.serializeData(this.data)
        };
    }

    serializeData(data) {
        var serializedData = {},
            parents = data.parents,
            parentsCopy, i, l;

        if (parents) {
            parentsCopy = Array(parents.length);
            for (i = 0, l = parents.length; i < l; ++i) {
                parentsCopy[i] = parents[i].toJSON();
            }
        }

        for (i in data) {
            serializedData[i] = data[i];
        }

        delete serializedData.parents;
        delete serializedData.bundle;
        serializedData = JSON.parse(JSON.stringify(serializedData));

        if (parentsCopy) {
            serializedData.parents = parentsCopy;
        }

        return serializedData;
    }

    /*
       Method: adjacentTo
 
       Indicates if the node is adjacent to the node specified by id
 
       Parameters:
 
          id - (string) A node id.
 
       Example:
       (start code js)
        node.adjacentTo('nodeId') == true;
       (end code)
    */
    adjacentTo(node) {
        return node.id in this.adjacencies;
    }

    /*
       Method: getAdjacency
 
       Returns a <Graph.Edge> object connecting the current <Graph.Node> and the node having *id* as id.
 
       Parameters:
 
          id - (string) A node id.
    */
    getEdge(id) {
        return this.adjacencies[id];
    }

    /*
       Method: toString
 
       Returns a String with information on the Node.
 
    */
    toString() {
        return 'Node(' + JSON.stringify([this.id, this.name, this.data, this.adjacencies]) + ')';
    }

    expandEdges() {
        if (this.expandedEdges) {
            return this.expandedEdges;
        }
        var ans = [];
        expandEdgesRichHelper(this, [], ans);
        this.expandedEdges = ans;
        return ans;
    };

    unbundleEdges(delta) {
        var expandedEdges = this.expandEdges(),
            ans = Array(expandedEdges.length),
            min = Math.min,
            i, l, j, n, edge, edgeCopy, normal, x0, xk, xk_x0, xi, xi_x0, xi_bar, dot, norm, norm2, c, last;

        delta = delta || 0;
        this.unbundledEdges = this.unbundledEdges || {};

        if ((delta === 0 || delta === 1) &&
            this.unbundledEdges[delta]) {
            return this.unbundledEdges[delta];
        }

        for (i = 0, l = expandedEdges.length; i < l; ++i) {
            edge = expandedEdges[i];
            last = edge.length - 1;
            edgeCopy = cloneEdge(edge);
            //edgeCopy = cloneJSON(edge);
            x0 = edge[0].pos;
            xk = edge[last].pos;
            xk_x0 = $sub(xk, x0);

            edgeCopy[0].unbundledPos = edgeCopy[0].pos.slice();
            normal = $sub(edgeCopy[1].pos, edgeCopy[0].pos);
            normal = $normalize([-normal[1], normal[0]]);
            edgeCopy[0].normal = normal;

            edgeCopy[last].unbundledPos = edgeCopy[edge.length - 1].pos.slice();
            normal = $sub(edgeCopy[last].pos, edgeCopy[last - 1].pos);
            normal = $normalize([-normal[1], normal[0]]);
            edgeCopy[last].normal = normal;

            for (j = 1, n = edge.length - 1; j < n; ++j) {
                xi = edge[j].pos;
                xi_x0 = $sub(xi, x0);
                dot = $dot(xi_x0, xk_x0);
                norm = $dist(xk, x0);
                norm2 = norm * norm;
                c = dot / norm2;
                xi_bar = $add(x0, $mult(c, xk_x0));
                edgeCopy[j].unbundledPos = $lerp(xi_bar, xi, delta);
                normal = $sub(edgeCopy[j + 1].pos, edgeCopy[j - 1].pos);
                normal = $normalize([-normal[1], normal[0]]);
                edgeCopy[j].normal = normal;
            }
            ans[i] = edgeCopy;
        }

        if (delta === 0 || delta === 1) {
            this.unbundledEdges[delta] = ans;
        }

        return ans;
    };
};