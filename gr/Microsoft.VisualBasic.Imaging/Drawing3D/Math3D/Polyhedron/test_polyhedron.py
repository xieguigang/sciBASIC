"""
Tests for Polyhedron winding number calculation.

"""
import unittest

try:
    import numpy
except ImportError:
    NUMPY_AVAILABLE = False
else:
    NUMPY_AVAILABLE = True

from polyhedron import Polyhedron


# Sample polyhedra ############################################################

# Regular tetrahedron.
tetrahedron = Polyhedron(
    vertex_positions=[
        (0, 0, 0),
        (0, 1, 1),
        (1, 0, 1),
        (1, 1, 0),
    ],
    triangles=[
        [0, 1, 3],
        [0, 2, 1],
        [0, 3, 2],
        [1, 2, 3],
    ],
)


def tetrahedron_classify(point):
    x, y, z = point
    if x + y + z < 2 and x + y > z and x + z > y and y + z > x:
        return "inside"
    if x + y + z <= 2 and x + y >= z and x + z >= y and y + z >= x:
        return "boundary"
    return "outside"


# Regular octahedron, with vertices on the axes.
octahedron = Polyhedron(
    vertex_positions=[
        (-1, 0, 0), (0, -1, 0), (0, 0, -1),
        (1, 0, 0), (0, 1, 0), (0, 0, 1),
    ],
    triangles=[
        [0, 2, 1], [0, 4, 2], [0, 1, 5], [0, 5, 4],
        [3, 1, 2], [3, 5, 1], [3, 2, 4], [3, 4, 5],
    ],
)

# Cube with vertices at (+-1, +-1, +-1).
cube = Polyhedron(
    vertex_positions=[
        (-1, -1, -1), (-1, -1, +1), (-1, +1, -1), (-1, +1, +1),
        (+1, -1, -1), (+1, -1, +1), (+1, +1, -1), (+1, +1, +1),
    ],
    triangles=[
        [1, 3, 2], [1, 0, 4], [1, 5, 7],
        [2, 0, 1], [2, 6, 4], [2, 3, 7],
        [4, 5, 1], [4, 0, 2], [4, 6, 7],
        [7, 3, 1], [7, 6, 2], [7, 5, 4],
    ],
)

# Pair of cubes, sharing a common vertex at the origin.
pair_of_cubes = Polyhedron(
    vertex_positions=[
        (-1, -1, -1), (-1, -1, 0), (-1, 0, -1), (-1, 0, 0),
        (0, -1, -1), (0, -1, 0), (0, 0, -1), (0, 0, 0),
        (0, 0, 1), (0, 1, 0), (0, 1, 1),
        (1, 0, 0), (1, 0, 1), (1, 1, 0), (1, 1, 1),
    ],
    triangles=[
        [1, 3, 2], [1, 0, 4], [1, 5, 7],
        [2, 0, 1], [2, 6, 4], [2, 3, 7],
        [4, 5, 1], [4, 0, 2], [4, 6, 7],
        [7, 3, 1], [7, 6, 2], [7, 5, 4],
        [8, 10, 9], [8, 7, 11], [8, 12, 14],
        [9, 7, 8], [9, 13, 11], [9, 10, 14],
        [11, 12, 8], [11, 7, 9], [11, 13, 14],
        [14, 10, 8], [14, 13, 9], [14, 12, 11],
    ],
)

# Two stacked cuboids, one directly above the other.
aligned_stacked_cuboids = Polyhedron(
    vertex_positions=[
        (0, 0, 0), (0, 0, 1), (0, 3, 0), (0, 3, 1),
        (3, 0, 0), (3, 0, 1), (3, 3, 0), (3, 3, 1),
        (0, 0, 2), (0, 0, 3), (0, 3, 2), (0, 3, 3),
        (3, 0, 2), (3, 0, 3), (3, 3, 2), (3, 3, 3),
    ],
    triangles=(
        cube.triangles +
        [[x+8, y+8, z+8] for x, y, z in cube.triangles]
    ),
)

# Similar, but with the cuboids misaligned.

misaligned_stacked_cuboids = Polyhedron(
    vertex_positions=[
        (0, 0, 0), (0, 0, 1), (0, 2, 0), (0, 2, 1),
        (2, 0, 0), (2, 0, 1), (2, 2, 0), (2, 2, 1),
        (1, 1, 2), (1, 1, 3), (1, 3, 2), (1, 3, 3),
        (3, 1, 2), (3, 1, 3), (3, 3, 2), (3, 3, 3),
    ],
    triangles=(
        cube.triangles +
        [[x+8, y+8, z+8] for x, y, z in cube.triangles]
    ),
)

# Hollow cube: surface consists of two cubes, one facing outwards
# and one facing inwards.
hollow_cube = Polyhedron(
    vertex_positions=[
        (0, 0, 0), (0, 0, 3), (0, 3, 0), (0, 3, 3),
        (3, 0, 0), (3, 0, 3), (3, 3, 0), (3, 3, 3),
        (1, 1, 1), (1, 1, 2), (1, 2, 1), (1, 2, 2),
        (2, 1, 1), (2, 1, 2), (2, 2, 1), (2, 2, 2),
    ],
    triangles=(
        # Outer cube.
        cube.triangles +
        # Inner cube: reverse orientation.
        [[z+8, y+8, x+8] for x, y, z in cube.triangles]
    ),
)

# Nested cubes: surface consists of two cubes, both facing
# outwards.  Points inside the inner cube should have a winding
# number of 2.

nested_cube = Polyhedron(
    vertex_positions=[
        (0, 0, 0), (0, 0, 3), (0, 3, 0), (0, 3, 3),
        (3, 0, 0), (3, 0, 3), (3, 3, 0), (3, 3, 3),
        (1, 1, 1), (1, 1, 2), (1, 2, 1), (1, 2, 2),
        (2, 1, 1), (2, 1, 2), (2, 2, 1), (2, 2, 2),
    ],
    triangles=(
        # Outer cube.
        cube.triangles +
        # Inner cube: same orientation as outer cube.
        [[x+8, y+8, z+8] for x, y, z in cube.triangles]
    ),
)

# Torus.
torus = Polyhedron(
    vertex_positions=[
        # Outer square, bottom face.
        (0, 0, 0), (0, 3, 0), (3, 0, 0), (3, 3, 0),
        # Inner square, bottom face.
        (1, 1, 0), (1, 2, 0), (2, 1, 0), (2, 2, 0),
        # Outer square, top face.
        (0, 0, 1), (0, 3, 1), (3, 0, 1), (3, 3, 1),
        # Inner square, top face.
        (1, 1, 1), (1, 2, 1), (2, 1, 1), (2, 2, 1),
    ],
    triangles=[
        # Top face.
        [8, 14, 12], [14, 8, 10], [10, 15, 14], [15, 10, 11],
        [11, 13, 15], [13, 11, 9], [9, 12, 13], [12, 9, 8],
        # Bottom face.
        [4, 6, 0], [2, 0, 6], [6, 7, 2], [3, 2, 7],
        [7, 5, 3], [1, 3, 5], [5, 4, 1], [0, 1, 4],
        # Outer faces.
        [0, 2, 10], [10, 8, 0], [2, 3, 11], [11, 10, 2],
        [3, 1, 9], [9, 11, 3], [1, 0, 8], [8, 9, 1],
        # Inner faces.
        [4, 12, 14], [14, 6, 4], [6, 14, 15], [15, 7, 6],
        [7, 15, 13], [13, 5, 7], [5, 13, 12], [12, 4, 5],
    ],
)


# Empty surface
empty = Polyhedron(
    vertex_positions=[],
    triangles=[],
)


# Double-sided triangle.  This is the simplest valid nontrivial
# surface.
triangle = Polyhedron(
    triangles=[
        [0, 1, 2],
        [2, 1, 0],
    ],
    vertex_positions=[
        [1.0, 0.0, 0.0],
        [0.0, 1.0, 0.0],
        [0.0, 0.0, 1.0],
    ],
)


# A hexadecahedron that's topologically equivalent to two octagonal pyramids
# glued base-to-base, but with a mapping into R^3 that wraps the surface twice
# around the origin, so that its image in R^3 looks like the surface of an
# octahedron.  All points inside the surface have winding number 2.
twice_wrapped_octahedron = Polyhedron(
    triangles=[
        (0, 1, 2),
        (0, 2, 3),
        (0, 3, 4),
        (0, 4, 5),
        (0, 5, 6),
        (0, 6, 7),
        (0, 7, 8),
        (0, 8, 1),
        (9, 2, 1),
        (9, 3, 2),
        (9, 4, 3),
        (9, 5, 4),
        (9, 6, 5),
        (9, 7, 6),
        (9, 8, 7),
        (9, 1, 8),
    ],
    vertex_positions=[
        (0.0, 0.0, 1.0),
        (1.0, 0.0, 0.0),
        (0.0, 1.0, 0.0),
        (-1.0, 0.0, 0.0),
        (0.0, -1.0, 0.0),
        (1.0, 0.0, 0.0),
        (0.0, 1.0, 0.0),
        (-1.0, 0.0, 0.0),
        (0.0, -1.0, 0.0),
        (0.0, 0.0, -1.0),
    ],
)


class TestPolyhedron(unittest.TestCase):
    def test_invalid_polyhedra(self):
        with self.assertRaises(ValueError):
            # Tetrahedron with one face incorrectly oriented.
            Polyhedron(
                vertex_positions=[
                    (0, 0, 0),
                    (0, 1, 1),
                    (1, 0, 1),
                    (1, 1, 0),
                ],
                triangles=[
                    [0, 1, 3],
                    [0, 1, 2],
                    [0, 3, 2],
                    [1, 2, 3],
                ],
            )

        with self.assertRaises(ValueError):
            # Tetrahedron with a duplicated face.
            Polyhedron(
                vertex_positions=[
                    (0, 0, 0),
                    (0, 1, 1),
                    (1, 0, 1),
                    (1, 1, 0),
                ],
                triangles=[
                    [0, 1, 3],
                    [0, 2, 1],
                    [0, 3, 2],
                    [1, 2, 3],
                    [1, 2, 3],
                ],
            )

        with self.assertRaises(ValueError):
            # Tetrahedron with a missing face.
            Polyhedron(
                vertex_positions=[
                    (0, 0, 0),
                    (0, 1, 1),
                    (1, 0, 1),
                    (1, 1, 0),
                ],
                triangles=[
                    [0, 1, 3],
                    [0, 2, 1],
                    [0, 3, 2],
                ],
            )

    def test_tetrahedron(self):
        xs = ys = zs = [0.25 * v for v in range(-1, 6)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = tetrahedron_classify(point)
            if class_ == "inside":
                self.assertEqual(tetrahedron.winding_number(point), 1)
            elif class_ == "outside":
                self.assertEqual(tetrahedron.winding_number(point), 0)
            elif class_ == "boundary":
                # Point is on the boundary.
                with self.assertRaises(ValueError):
                    tetrahedron.winding_number(point)
            else:
                assert False, "should never get here"

    def test_cube(self):
        # Check volume
        self.assertEqual(cube.volume(), 8)

        def classify(point):
            x, y, z = point
            if -1 < x < 1 and -1 < y < 1 and -1 < z < 1:
                return "inside"
            if -1 <= x <= 1 and -1 <= y <= 1 and -1 <= z <= 1:
                return "boundary"
            return "outside"

        # Quarter-integer boundaries from -1.25 to 1.25 inclusive.
        xs = ys = zs = [0.25 * v for v in range(-5, 6)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = classify(point)
            if class_ == "inside":
                self.assertEqual(cube.winding_number(point), 1)
            elif class_ == "outside":
                self.assertEqual(cube.winding_number(point), 0)
            elif class_ == "boundary":
                # Point is on the boundary.
                with self.assertRaises(ValueError):
                    cube.winding_number(point)
            else:
                assert False, "should never get here"

    def test_octahedron(self):
        self.assertEqual(octahedron.volume(), 4.0 / 3.0)

        def classify(point):
            x, y, z = point
            s = abs(x) + abs(y) + abs(z)
            if s < 1:
                return "inside"
            elif s == 1:
                return "boundary"
            else:
                return "outside"

        # Quarter-integer boundaries from -1.25 to 1.25 inclusive.
        xs = ys = zs = [0.25 * v for v in range(-5, 6)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = classify(point)
            if class_ == "inside":
                self.assertEqual(octahedron.winding_number(point), 1)
            elif class_ == "outside":
                self.assertEqual(octahedron.winding_number(point), 0)
            elif class_ == "boundary":
                # Point is on the boundary.
                with self.assertRaises(ValueError):
                    octahedron.winding_number(point)
            else:
                assert False, "never get here"

    def test_twice_wrapped_octahedron(self):
        poly = twice_wrapped_octahedron

        self.assertEqual(poly.volume(), 8.0 / 3.0)

        def winding_number(point):
            x, y, z = point
            s = abs(x) + abs(y) + abs(z)
            if s == 1:
                raise ValueError("boundary")
            return 2 if s < 1 else 0

        xs = ys = zs = [0.25 * v for v in range(-5, 6)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            try:
                expected = winding_number(point)
            except ValueError:
                with self.assertRaises(ValueError):
                    poly.winding_number(point)
            else:
                self.assertEqual(poly.winding_number(point), expected)

    def test_pair_of_cubes(self):
        self.assertEqual(pair_of_cubes.volume(), 2.0)

        def classify(point):
            x, y, z = point
            if -1 < x < 0 and -1 < y < 0 and -1 < z < 0:
                return "inside"
            if 0 < x < 1 and 0 < y < 1 and 0 < z < 1:
                return "inside"
            if -1 <= x <= 0 and -1 <= y <= 0 and -1 <= z <= 0:
                return "boundary"
            if 0 <= x <= 1 and 0 <= y <= 1 and 0 <= z <= 1:
                return "boundary"
            return "outside"

        # Quarter-integer boundaries from -1.25 to 1.25 inclusive.
        xs = ys = zs = [0.25 * v for v in range(-5, 6)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = classify(point)
            if class_ == "inside":
                self.assertEqual(pair_of_cubes.winding_number(point), 1)
            elif class_ == "boundary":
                with self.assertRaises(ValueError):
                    pair_of_cubes.winding_number(point)
            elif class_ == "outside":
                self.assertEqual(pair_of_cubes.winding_number(point), 0)
            else:
                assert False, "never get here"

    def test_hollow_cube(self):
        self.assertEqual(hollow_cube.volume(), 26.0)

        def classify(point):
            x, y, z = point
            if 1 < x < 2 and 1 < y < 2 and 1 < z < 2:
                return "outside"
            if 1 <= x <= 2 and 1 <= y <= 2 and 1 <= z <= 2:
                return "boundary"
            if 0 < x < 3 and 0 < y < 3 and 0 < z < 3:
                return "inside"
            if 0 <= x <= 3 and 0 <= y <= 3 and 0 <= z <= 3:
                return "boundary"
            return "outside"

        xs = ys = zs = [0.25 * v for v in range(-1, 14)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = classify(point)
            if class_ == "inside":
                self.assertEqual(hollow_cube.winding_number(point), 1)
            elif class_ == "boundary":
                with self.assertRaises(ValueError):
                    hollow_cube.winding_number(point)
            elif class_ == "outside":
                self.assertEqual(hollow_cube.winding_number(point), 0)
            else:
                assert False, "never get here"

    def test_nested_cube(self):
        # To make sense of the volume, think of it as the integral
        # of the winding number:  in effect, points inside the inner cube
        # contribute to the volume *twice*.
        self.assertEqual(nested_cube.volume(), 28.0)

        def classify(point):
            x, y, z = point
            if 1 < x < 2 and 1 < y < 2 and 1 < z < 2:
                return "doubled"
            if 1 <= x <= 2 and 1 <= y <= 2 and 1 <= z <= 2:
                return "boundary"
            if 0 < x < 3 and 0 < y < 3 and 0 < z < 3:
                return "inside"
            if 0 <= x <= 3 and 0 <= y <= 3 and 0 <= z <= 3:
                return "boundary"
            return "outside"

        xs = ys = zs = [0.25 * v for v in range(-1, 14)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = classify(point)
            if class_ == "inside":
                self.assertEqual(nested_cube.winding_number(point), 1)
            elif class_ == "boundary":
                with self.assertRaises(ValueError):
                    nested_cube.winding_number(point)
            elif class_ == "outside":
                self.assertEqual(nested_cube.winding_number(point), 0)
            elif class_ == "doubled":
                self.assertEqual(nested_cube.winding_number(point), 2)
            else:
                assert False, "never get here"

    def test_torus(self):
        self.assertEqual(torus.volume(), 8.0)

        def classify(point):
            x, y, z = point
            if 0 < z < 1 and (0 < x < 1 or 2 < x < 3) and 0 < y < 3:
                return "inside"
            if 0 < z < 1 and (0 < y < 1 or 2 < y < 3) and 0 < x < 3:
                return "inside"
            if 0 <= z <= 1 and (0 <= x <= 1 or 2 <= x <= 3) and 0 <= y <= 3:
                return "boundary"
            if 0 <= z <= 1 and (0 <= y <= 1 or 2 <= y <= 3) and 0 <= x <= 3:
                return "boundary"
            return "outside"

        xs = ys = [0.25 * v for v in range(-1, 14)]
        zs = [0.25 * v for v in range(-1, 6)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = classify(point)
            if class_ == "inside":
                self.assertEqual(torus.winding_number(point), 1)
            elif class_ == "boundary":
                with self.assertRaises(ValueError):
                    torus.winding_number(point)
            elif class_ == "outside":
                self.assertEqual(torus.winding_number(point), 0)
            else:
                assert False, "never get here"

    def test_aligned_stacked_cuboids(self):
        self.assertEqual(aligned_stacked_cuboids.volume(), 18.0)

        def classify(point):
            x, y, z = point
            if 0 < x < 3 and 0 < y < 3 and 0 < z < 1:
                return "inside"
            if 0 < x < 3 and 0 < y < 3 and 2 < z < 3:
                return "inside"
            if 0 <= x <= 3 and 0 <= y <= 3 and 0 <= z <= 1:
                return "boundary"
            if 0 <= x <= 3 and 0 <= y <= 3 and 2 <= z <= 3:
                return "boundary"
            return "outside"

        xs = ys = zs = [0.25 * v for v in range(-1, 14)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = classify(point)
            if class_ == "inside":
                self.assertEqual(
                    aligned_stacked_cuboids.winding_number(point), 1)
            elif class_ == "boundary":
                with self.assertRaises(ValueError):
                    aligned_stacked_cuboids.winding_number(point)
            elif class_ == "outside":
                self.assertEqual(
                    aligned_stacked_cuboids.winding_number(point), 0)
            else:
                assert False, "never get here"

    def test_misaligned_stacked_cuboids(self):
        self.assertEqual(misaligned_stacked_cuboids.volume(), 8.0)

        def classify(point):
            x, y, z = point
            if 0 < x < 2 and 0 < y < 2 and 0 < z < 1:
                return "inside"
            if 1 < x < 3 and 1 < y < 3 and 2 < z < 3:
                return "inside"
            if 0 <= x <= 2 and 0 <= y <= 2 and 0 <= z <= 1:
                return "boundary"
            if 1 <= x <= 3 and 1 <= y <= 3 and 2 <= z <= 3:
                return "boundary"
            return "outside"

        xs = ys = zs = [0.25 * v for v in range(-1, 14)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = classify(point)
            if class_ == "inside":
                self.assertEqual(
                    misaligned_stacked_cuboids.winding_number(point), 1)
            elif class_ == "boundary":
                with self.assertRaises(ValueError):
                    misaligned_stacked_cuboids.winding_number(point)
            elif class_ == "outside":
                self.assertEqual(
                    misaligned_stacked_cuboids.winding_number(point), 0)
            else:
                assert False, "never get here"

    def test_empty(self):
        self.assertEqual(empty.volume(), 0.0)
        xs = ys = zs = [0.25 * v for v in range(-1, 14)]
        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            self.assertEqual(empty.winding_number(point), 0)

    @unittest.skipUnless(NUMPY_AVAILABLE, "Test requires NumPy")
    def test_numpy_float64_compatibility(self):
        # This is a repetition of test_cube, but using NumPy float64
        # and int64 values in place of Python ints for vertices and triangles
        # (respectively), and ndarrays in place of lists or tuples.
        numpy_cube = Polyhedron(
            triangles=numpy.array(cube.triangles, dtype=numpy.int64),
            vertex_positions=numpy.array(
                cube.vertex_positions, dtype=numpy.float64),
        )

        # Check volume
        self.assertEqual(numpy_cube.volume(), 8)

        def classify(point):
            x, y, z = point
            if -1 < x < 1 and -1 < y < 1 and -1 < z < 1:
                return "inside"
            if -1 <= x <= 1 and -1 <= y <= 1 and -1 <= z <= 1:
                return "boundary"
            return "outside"

        # Quarter-integer boundaries from -1.25 to 1.25 inclusive.
        xs = ys = zs = numpy.linspace(-1.25, 1.25, 11)

        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = classify(point)
            if class_ == "inside":
                self.assertEqual(numpy_cube.winding_number(point), 1)
            elif class_ == "outside":
                self.assertEqual(numpy_cube.winding_number(point), 0)
            elif class_ == "boundary":
                # Point is on the boundary.
                with self.assertRaises(ValueError):
                    numpy_cube.winding_number(point)
            else:
                assert False, "should never get here"

    @unittest.skipUnless(NUMPY_AVAILABLE, "Test requires NumPy")
    def test_numpy_int64_compatibility(self):
        # This is a repetition of test_cube, but using NumPy int64
        # values in place of Python ints, and ndarrays in place
        # of lists or tuples.
        numpy_cube = Polyhedron(
            triangles=numpy.array(cube.triangles, dtype=numpy.int64),
            vertex_positions=numpy.array(
                cube.vertex_positions, dtype=numpy.int64),
        )

        # Check volume
        self.assertEqual(numpy_cube.volume(), 8)

        def classify(point):
            x, y, z = point
            if -1 < x < 1 and -1 < y < 1 and -1 < z < 1:
                return "inside"
            if -1 <= x <= 1 and -1 <= y <= 1 and -1 <= z <= 1:
                return "boundary"
            return "outside"

        # Quarter-integer boundaries from -1.25 to 1.25 inclusive.
        xs = ys = zs = numpy.linspace(-1.25, 1.25, 11)

        points = [(x, y, z) for x in xs for y in ys for z in zs]
        for point in points:
            class_ = classify(point)
            if class_ == "inside":
                self.assertEqual(numpy_cube.winding_number(point), 1)
            elif class_ == "outside":
                self.assertEqual(numpy_cube.winding_number(point), 0)
            elif class_ == "boundary":
                # Point is on the boundary.
                with self.assertRaises(ValueError):
                    numpy_cube.winding_number(point)
            else:
                assert False, "should never get here"


if __name__ == '__main__':
    unittest.main()
