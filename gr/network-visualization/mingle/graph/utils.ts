
function descriptor(object) {
    var desc = {}, p;
    for (p in object) {
        desc[p] = {
            value: object[p],
            writable: true,
            enumerable: true,
            configurable: true
        };
    }
    return desc;
}

function merge(proto, object) {
    return Object.create(proto, descriptor(object));
}

function extend(proto, object) {
    var p;
    for (p in object) {
        proto[p] = object[p];
    }
}

/*
      Object: Graph.Util
   
      <Graph> traversal and processing utility object.
   
      Note:
   
      For your convenience some of these methods have also been appended to <Graph> and <Graph.Node> classes.
   */
class Util {
    /*
       filter
 
       For internal use only. Provides a filtering function based on flags.
    */
    filter(param) {
        if (!param || !(typeof param == 'string')) return function () { return true; };
        var props = param.split(" ");
        return function (elem) {
            for (var i = 0; i < props.length; i++) {
                if (elem[props[i]]) {
                    return false;
                }
            }
            return true;
        };
    }
    /*
       Method: getNode
     
       Returns a <Graph.Node> by *id*.
     
       Also implemented by:
     
       <Graph>
     
       Parameters:
     
       graph - (object) A <Graph> instance.
       id - (string) A <Graph.Node> id.
     
       Example:
     
       (start code js)
         $jit.Graph.Util.getNode(graph, 'nodeid');
         //or...
         graph.getNode('nodeid');
       (end code)
    */
    getNode(graph, id) {
        return graph.nodes[id];
    }

    /*
       Method: eachNode
     
       Iterates over <Graph> nodes performing an *action*.
     
       Also implemented by:
     
       <Graph>.
     
       Parameters:
     
       graph - (object) A <Graph> instance.
       action - (function) A callback function having a <Graph.Node> as first formal parameter.
     
       Example:
       (start code js)
         $jit.Graph.Util.eachNode(graph, function(node) {
          alert(node.name);
         });
         //or...
         graph.eachNode(function(node) {
           alert(node.name);
         });
       (end code)
    */
    eachNode(graph, action, flags) {
        var filter = this.filter(flags);
        for (var i in graph.nodes) {
            if (filter(graph.nodes[i])) action(graph.nodes[i]);
        }
    }

    /*
      Method: each
     
      Iterates over <Graph> nodes performing an *action*. It's an alias for <Graph.Util.eachNode>.
     
      Also implemented by:
     
      <Graph>.
     
      Parameters:
     
      graph - (object) A <Graph> instance.
      action - (function) A callback function having a <Graph.Node> as first formal parameter.
     
      Example:
      (start code js)
        $jit.Graph.Util.each(graph, function(node) {
         alert(node.name);
        });
        //or...
        graph.each(function(node) {
          alert(node.name);
        });
      (end code)
    */
    each(graph, action, flags) {
        this.eachNode(graph, action, flags);
    }

    /*
          Method: eachEdge
     
          Iterates over <Graph.Node> adjacencies applying the *action* function.
     
          Also implemented by:
     
          <Graph.Node>.
     
          Parameters:
     
          node - (object) A <Graph.Node>.
          action - (function) A callback function having <Graph.Edge> as first formal parameter.
     
          Example:
          (start code js)
            $jit.Graph.Util.eachEdge(node, function(adj) {
             alert(adj.nodeTo.name);
            });
            //or...
            node.eachEdge(function(adj) {
              alert(adj.nodeTo.name);
            });
          (end code)
       */
    eachEdge(node, action, flags) {
        var adj = node.adjacencies, filter = this.filter(flags);
        for (var id in adj) {
            var a = adj[id];
            if (filter(a)) {
                if (a.nodeFrom != node) {
                    var tmp = a.nodeFrom;
                    a.nodeFrom = a.nodeTo;
                    a.nodeTo = tmp;
                }
                action(a, id);
            }
        }
    }

    /*
      Method: computeLevels
     
      Performs a BFS traversal setting the correct depth for each node.
     
      Also implemented by:
     
      <Graph>.
     
      Note:
     
      The depth of each node can then be accessed by
      >node.depth
     
      Parameters:
     
      graph - (object) A <Graph>.
      id - (string) A starting node id for the BFS traversal.
      startDepth - (optional|number) A minimum depth value. Default's 0.
     
    */
    computeLevels(graph, id, startDepth, flags) {
        startDepth = startDepth || 0;
        var filter = this.filter(flags);
        this.eachNode(graph, function (elem) {
            elem._flag = false;
            elem.depth = -1;
        }, flags);
        var root = graph.getNode(id);
        root.depth = startDepth;
        var queue = [root];
        while (queue.length != 0) {
            var node = queue.pop();
            node._flag = true;
            this.eachEdge(node, function (adj) {
                var n = adj.nodeTo;
                if (n._flag == false && filter(n) && !adj._hiding) {
                    if (n.depth < 0) n.depth = node.depth + 1 + startDepth;
                    queue.unshift(n);
                }
            }, flags);
        }
    }

    /*
       Method: eachBFS
     
       Performs a BFS traversal applying *action* to each <Graph.Node>.
     
       Also implemented by:
     
       <Graph>.
     
       Parameters:
     
       graph - (object) A <Graph>.
       id - (string) A starting node id for the BFS traversal.
       action - (function) A callback function having a <Graph.Node> as first formal parameter.
     
       Example:
       (start code js)
         $jit.Graph.Util.eachBFS(graph, 'mynodeid', function(node) {
          alert(node.name);
         });
         //or...
         graph.eachBFS('mynodeid', function(node) {
           alert(node.name);
         });
       (end code)
    */
    eachBFS(graph, id, action, flags) {
        var filter = this.filter(flags);
        this.clean(graph);
        var queue = [graph.getNode(id)];
        while (queue.length != 0) {
            var node = queue.pop();
            if (!node) return;
            node._flag = true;
            action(node, node.depth);
            this.eachEdge(node, function (adj) {
                var n = adj.nodeTo;
                if (n._flag == false && filter(n) && !adj._hiding) {
                    n._flag = true;
                    queue.unshift(n);
                }
            }, flags);
        }
    }

    /*
       Method: eachLevel
     
       Iterates over a node's subgraph applying *action* to the nodes of relative depth between *levelBegin* and *levelEnd*.
       In case you need to break the iteration, *action* should return false.
     
       Also implemented by:
     
       <Graph.Node>.
     
       Parameters:
     
       node - (object) A <Graph.Node>.
       levelBegin - (number) A relative level value.
       levelEnd - (number) A relative level value.
       action - (function) A callback function having a <Graph.Node> as first formal parameter.
     
    */
    eachLevel(node, levelBegin, levelEnd, action, flags) {
        var d = node.depth, filter = this.filter(flags), that = this, shouldContinue = true;
        levelEnd = levelEnd === false ? Number.MAX_VALUE - d : levelEnd;
        (function loopLevel(node, levelBegin, levelEnd) {
            if (!shouldContinue) return;
            var d = node.depth, ret;
            if (d >= levelBegin && d <= levelEnd && filter(node)) ret = action(node, d);
            if (typeof ret !== "undefined") shouldContinue = ret;
            if (shouldContinue && d < levelEnd) {
                that.eachEdge(node, function (adj) {
                    var n = adj.nodeTo;
                    if (n.depth > d) loopLevel(n, levelBegin, levelEnd);
                });
            }
        })(node, levelBegin + d, levelEnd + d);
    }

    /*
       Method: eachSubgraph
     
       Iterates over a node's children recursively.
     
       Also implemented by:
     
       <Graph.Node>.
     
       Parameters:
       node - (object) A <Graph.Node>.
       action - (function) A callback function having a <Graph.Node> as first formal parameter.
     
       Example:
       (start code js)
         $jit.Graph.Util.eachSubgraph(node, function(node) {
           alert(node.name);
         });
         //or...
         node.eachSubgraph(function(node) {
           alert(node.name);
         });
       (end code)
    */
    eachSubgraph(node, action, flags) {
        this.eachLevel(node, 0, false, action, flags);
    }

    /*
       Method: eachSubnode
     
       Iterates over a node's children (without deeper recursion).
     
       Also implemented by:
     
       <Graph.Node>.
     
       Parameters:
       node - (object) A <Graph.Node>.
       action - (function) A callback function having a <Graph.Node> as first formal parameter.
     
       Example:
       (start code js)
         $jit.Graph.Util.eachSubnode(node, function(node) {
          alert(node.name);
         });
         //or...
         node.eachSubnode(function(node) {
           alert(node.name);
         });
       (end code)
    */
    eachSubnode(node, action, flags) {
        this.eachLevel(node, 1, 1, action, flags);
    }

    /*
       Method: anySubnode
     
       Returns *true* if any subnode matches the given condition.
     
       Also implemented by:
     
       <Graph.Node>.
     
       Parameters:
       node - (object) A <Graph.Node>.
       cond - (function) A callback function returning a Boolean instance. This function has as first formal parameter a <Graph.Node>.
     
       Example:
       (start code js)
         $jit.Graph.Util.anySubnode(node, function(node) { return node.name == "mynodename"; });
         //or...
         node.anySubnode(function(node) { return node.name == 'mynodename'; });
       (end code)
    */
    anySubnode(node, cond, flags) {
        var flag = false;
        cond = cond || function () { return true; };
        var c = typeof cond == 'string' ? function (n) { return n[cond]; } : cond;
        this.eachSubnode(node, function (elem) {
            if (c(elem)) flag = true;
        }, flags);
        return flag;
    }

    /*
       Method: getSubnodes
     
       Collects all subnodes for a specified node.
       The *level* parameter filters nodes having relative depth of *level* from the root node.
     
       Also implemented by:
     
       <Graph.Node>.
     
       Parameters:
       node - (object) A <Graph.Node>.
       level - (optional|number) Default's *0*. A starting relative depth for collecting nodes.
     
       Returns:
       An array of nodes.
     
    */
    getSubnodes(node, level, flags) {
        var ans = [], that = this;
        level = level || 0;
        var levelStart, levelEnd;
        if (Array.isArray(level) == 'array') {
            levelStart = level[0];
            levelEnd = level[1];
        } else {
            levelStart = level;
            levelEnd = Number.MAX_VALUE - node.depth;
        }
        this.eachLevel(node, levelStart, levelEnd, function (n) {
            ans.push(n);
        }, flags);
        return ans;
    }


    /*
       Method: getParents
     
       Returns an Array of <Graph.Nodes> which are parents of the given node.
     
       Also implemented by:
     
       <Graph.Node>.
     
       Parameters:
       node - (object) A <Graph.Node>.
     
       Returns:
       An Array of <Graph.Nodes>.
     
       Example:
       (start code js)
         var pars = $jit.Graph.Util.getParents(node);
         //or...
         var pars = node.getParents();
     
         if(pars.length > 0) {
           //do stuff with parents
         }
       (end code)
    */
    getParents(node) {
        var ans = [];
        this.eachEdge(node, function (adj) {
            var n = adj.nodeTo;
            if (n.depth < node.depth) ans.push(n);
        });
        return ans;
    }

    /*
    Method: isDescendantOf
     
    Returns a boolean indicating if some node is descendant of the node with the given id.
     
    Also implemented by:
     
    <Graph.Node>.
     
     
    Parameters:
    node - (object) A <Graph.Node>.
    id - (string) A <Graph.Node> id.
     
    Example:
    (start code js)
      $jit.Graph.Util.isDescendantOf(node, "nodeid"); //true|false
      //or...
      node.isDescendantOf('nodeid');//true|false
    (end code)
    */
    isDescendantOf(node, id) {
        if (node.id == id) return true;
        var pars = this.getParents(node), ans = false;
        for (var i = 0; !ans && i < pars.length; i++) {
            ans = ans || this.isDescendantOf(pars[i], id);
        }
        return ans;
    }

    /*
        Method: clean
     
        Cleans flags from nodes.
     
        Also implemented by:
     
        <Graph>.
     
        Parameters:
        graph - A <Graph> instance.
     */
    clean(graph) {
        this.eachNode(graph, function (elem) { elem._flag = false; });
    }
};