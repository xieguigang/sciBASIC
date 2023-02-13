"""Simple point-in-polygon algorithm based on winding number, with robustness
depending only on the underlying arithmetic.

We've got a closed, possibly non-simple polygon described as a list of vertices
in R^2, and we're given a point that doesn't lie directly on the path of the
polygon.  We'd like to compute the winding number of the polygon around the
point.

There are two sources of difficulty: (1) dealing with numerical errors that
might result in an incorrect answer, and (2) dealing with degenerate cases.
We'll ignore the numerical issues for the moment.

Strategy: without loss of generality, let's place the point at the origin.
Divide the remainder of the plane (i.e., R^2 minus the origin) into two
halves, L and R, defined as follows:

    L = {(x, y) | x < 0 or x == 0 and y < 0}

    R = {(x, y) | x > 0 or x == 0 and y > 0}

That is, R contains all points with argument in the half-closed interval
(-pi/2, pi/2], and L contains all others.  Note that with these definitions, L
and R are both convex: a line segment between two points in R lies entirely in
R, and similarly for L.  In particular, a line segment between two points can
only pass through the origin if one of those points is in L and the other in R.

Now the idea is that we follow the edges of the polygon, keeping track of how
many times we move between L and R.  For each move from L to R (or vice versa),
we also need to compute whether the edge passes *above* or *below* the origin,
to compute its contribution to the total winding number.  From the comment
above, we can safely ignore all edges that lie entirely within either L or R.

"""


def sign(x):
    """
    Return 1 if x is positive, -1 if it's negative, and 0 if it's zero.

    """
    if x > 0:
        return 1
    elif x < 0:
        return -1
    else:
        return 0


def vertex_sign(P, O):
    result = sign(P[0] - O[0]) or sign(P[1] - O[1])
    if not result:
        raise ValueError("vertex coincides with origin")
    return result


def edge_sign(P, Q, O):
    result = sign(
        (P[0] - O[0]) * (Q[1] - O[1]) - (P[1] - O[1]) * (Q[0] - O[0]))
    if not result:
        raise ValueError("vertices collinear with origin")
    return result


def half_turn(point1, point2, origin):
    """
    Return the contribution to the total winding number about 'origin' from a
    single edge, from point1 to point2.

    """
    edge_boundary = vertex_sign(point2, origin) - vertex_sign(point1, origin)
    return 0 if not edge_boundary else edge_sign(point1, point2, origin)


class Polygon(object):
    def __init__(self, vertex_positions):
        """
        Initialize from list of vertex positions.

        """
        self.vertex_positions = vertex_positions

    def edge_positions(self):
        """
        Pairs of vertex positions corresponding to the polygon edges.

        """
        points = self.vertex_positions
        first_point = previous_point = points[0]
        for point in points[1:]:
            yield previous_point, point
            previous_point = point
        yield previous_point, first_point

    def area(self):
        """
        Area enclosed by this polygon.

        For simple counterclockwise-oriented polygons, return the area
        enclosed by the polygon.  More generally, return the integral
        of the winding number of the polygon over R^2.

        """
        acc = 0.0
        for p1, p2 in self.edge_positions():
            acc += (p1[0] - p2[0]) * (p1[1] + p2[1])
        return acc / 2.0

    def winding_number(self, origin):
        """
        Compute the (counterclockwise) winding number of a polygon around a
        point.

        Raise ValueError if the point lies directly on the path
        of the polygon.

        """
        return sum(
            half_turn(point1, point2, origin)
            for point1, point2 in self.edge_positions()
        ) // 2
