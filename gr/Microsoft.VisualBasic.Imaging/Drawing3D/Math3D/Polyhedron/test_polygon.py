"""
Tests for Polygon.winding_number.

"""
import unittest

try:
    import numpy
except ImportError:
    NUMPY_AVAILABLE = False
else:
    NUMPY_AVAILABLE = True

from polygon import Polygon


class TestPolygon(unittest.TestCase):
    def test_simple_square(self):
        square = Polygon(
            vertex_positions=[
                (1.0, -1.0),
                (1.0, 1.0),
                (-1.0, 1.0),
                (-1.0, -1.0),
            ]
        )
        origin = (0.0, 0.0)
        self.assertEqual(square.winding_number(origin), 1)
        self.assertEqual(square.area(), 4.0)

    def test_double_square(self):
        square = Polygon(
            vertex_positions=[
                (1.0, -1.0),
                (1.0, 1.0),
                (-1.0, 1.0),
                (-1.0, -1.0),
            ] * 2
        )
        origin = (0.0, 0.0)
        self.assertEqual(square.winding_number(origin), 2)
        self.assertEqual(square.area(), 8.0)

    def test_clockwise_square(self):
        square = Polygon(
            vertex_positions=[
                (1.0, -1.0),
                (1.0, 1.0),
                (-1.0, 1.0),
                (-1.0, -1.0),
            ][::-1]
        )
        origin = (0.0, 0.0)
        self.assertEqual(square.winding_number(origin), -1)
        self.assertEqual(square.area(), -4.0)

    def test_various_points_in_square(self):
        square = Polygon(
            vertex_positions=[
                (1.0, -1.0),
                (1.0, 1.0),
                (-1.0, 1.0),
                (-1.0, -1.0),
            ]
        )
        test_points = []
        for x in range(-3, 4):
            for y in range(-3, 4):
                test_points.append((0.5*x, 0.5*y))

        for point in test_points:
            x, y = point
            if -1 < x < 1 and -1 < y < 1:
                # Point is inside.
                self.assertEqual(square.winding_number(point), 1)
            elif x < -1 or x > 1 or y < -1 or y > 1:
                # Point outside.
                self.assertEqual(square.winding_number(point), 0)
            else:
                with self.assertRaises(ValueError):
                    square.winding_number(point)

    def test_aitch(self):
        aitch = Polygon(
            vertex_positions=[
                (0, 0),
                (1, 0),
                (1, 1),
                (2, 1),
                (2, 0),
                (3, 0),
                (3, 3),
                (2, 3),
                (2, 2),
                (1, 2),
                (1, 3),
                (0, 3),
            ]
        )

        test_points = [
            (0.5*x, 0.5*y)
            for y in range(-1, 8)
            for x in range(-1, 8)
        ]

        # * for boundary, '.' for outside, 'o' for inside.
        template = """\
.........
.***.***.
.*o*.*o*.
.*o***o*.
.*ooooo*.
.*o***o*.
.*o*.*o*.
.***.***.
.........
"""
        expected = ''.join(template.strip().split())

        assert len(expected) == len(test_points)
        for point, point_type in zip(test_points, expected):
            if point_type == '.':
                self.assertEqual(aitch.winding_number(point), 0)
            elif point_type == 'o':
                self.assertEqual(aitch.winding_number(point), 1)
            else:
                with self.assertRaises(ValueError):
                    aitch.winding_number(point)

    @unittest.skipUnless(NUMPY_AVAILABLE, "Test requires NumPy")
    def test_numpy_compatibility(self):
        square = Polygon(
            vertex_positions=numpy.array(
                [
                    [1.0, -1.0],
                    [1.0, 1.0],
                    [-1.0, 1.0],
                    [-1.0, -1.0],
                ],
                dtype=numpy.float64,
            )
        )
        origin = numpy.array([0.0, 0.0], dtype=numpy.float64)
        self.assertEqual(square.winding_number(origin), 1)
        self.assertEqual(square.area(), 4.0)
