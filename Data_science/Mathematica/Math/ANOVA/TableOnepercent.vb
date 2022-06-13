﻿Friend Class TableOnepercent : Inherits DistributionTable

    Public Sub New()
        MyBase.New(AnovaTest.P_ONE_PERCENT)
    End Sub

    Protected Overrides Iterator Function loadMatrix() As IEnumerable(Of Double())
        Yield New Double() {4052.181, 4999.5, 5403.352, 5624.583, 5763.65, 5858.986, 5928.356, 5981.07, 6022.473, 6055.847, 6106.321, 6157.285, 6208.73, 6234.631, 6260.649, 6286.782, 6313.03, 6339.391, 6365.864}
        Yield New Double() {98.503, 99, 99.166, 99.249, 99.299, 99.333, 99.356, 99.374, 99.388, 99.399, 99.416, 99.433, 99.449, 99.458, 99.466, 99.474, 99.482, 99.491, 99.499}
        Yield New Double() {34.116, 30.817, 29.457, 28.71, 28.237, 27.911, 27.672, 27.489, 27.345, 27.229, 27.052, 26.872, 26.69, 26.598, 26.505, 26.411, 26.316, 26.221, 26.125}
        Yield New Double() {21.198, 18, 16.694, 15.977, 15.522, 15.207, 14.976, 14.799, 14.659, 14.546, 14.374, 14.198, 14.02, 13.929, 13.838, 13.745, 13.652, 13.558, 13.463}
        Yield New Double() {16.258, 13.274, 12.06, 11.392, 10.967, 10.672, 10.456, 10.289, 10.158, 10.051, 9.888, 9.722, 9.553, 9.466, 9.379, 9.291, 9.202, 9.112, 9.02}
        Yield New Double() {13.745, 10.925, 9.78, 9.148, 8.746, 8.466, 8.26, 8.102, 7.976, 7.874, 7.718, 7.559, 7.396, 7.313, 7.229, 7.143, 7.057, 6.969, 6.88}
        Yield New Double() {12.246, 9.547, 8.451, 7.847, 7.46, 7.191, 6.993, 6.84, 6.719, 6.62, 6.469, 6.314, 6.155, 6.074, 5.992, 5.908, 5.824, 5.737, 5.65}
        Yield New Double() {11.259, 8.649, 7.591, 7.006, 6.632, 6.371, 6.178, 6.029, 5.911, 5.814, 5.667, 5.515, 5.359, 5.279, 5.198, 5.116, 5.032, 4.946, 4.859}
        Yield New Double() {10.561, 8.022, 6.992, 6.422, 6.057, 5.802, 5.613, 5.467, 5.351, 5.257, 5.111, 4.962, 4.808, 4.729, 4.649, 4.567, 4.483, 4.398, 4.311}
        Yield New Double() {10.044, 7.559, 6.552, 5.994, 5.636, 5.386, 5.2, 5.057, 4.942, 4.849, 4.706, 4.558, 4.405, 4.327, 4.247, 4.165, 4.082, 3.996, 3.909}
        Yield New Double() {9.646, 7.206, 6.217, 5.668, 5.316, 5.069, 4.886, 4.744, 4.632, 4.539, 4.397, 4.251, 4.099, 4.021, 3.941, 3.86, 3.776, 3.69, 3.602}
        Yield New Double() {9.33, 6.927, 5.953, 5.412, 5.064, 4.821, 4.64, 4.499, 4.388, 4.296, 4.155, 4.01, 3.858, 3.78, 3.701, 3.619, 3.535, 3.449, 3.361}
        Yield New Double() {9.074, 6.701, 5.739, 5.205, 4.862, 4.62, 4.441, 4.302, 4.191, 4.1, 3.96, 3.815, 3.665, 3.587, 3.507, 3.425, 3.341, 3.255, 3.165}
        Yield New Double() {8.862, 6.515, 5.564, 5.035, 4.695, 4.456, 4.278, 4.14, 4.03, 3.939, 3.8, 3.656, 3.505, 3.427, 3.348, 3.266, 3.181, 3.094, 3.004}
        Yield New Double() {8.683, 6.359, 5.417, 4.893, 4.556, 4.318, 4.142, 4.004, 3.895, 3.805, 3.666, 3.522, 3.372, 3.294, 3.214, 3.132, 3.047, 2.959, 2.868}
        Yield New Double() {8.531, 6.226, 5.292, 4.773, 4.437, 4.202, 4.026, 3.89, 3.78, 3.691, 3.553, 3.409, 3.259, 3.181, 3.101, 3.018, 2.933, 2.845, 2.753}
        Yield New Double() {8.4, 6.112, 5.185, 4.669, 4.336, 4.102, 3.927, 3.791, 3.682, 3.593, 3.455, 3.312, 3.162, 3.084, 3.003, 2.92, 2.835, 2.746, 2.653}
        Yield New Double() {8.285, 6.013, 5.092, 4.579, 4.248, 4.015, 3.841, 3.705, 3.597, 3.508, 3.371, 3.227, 3.077, 2.999, 2.919, 2.835, 2.749, 2.66, 2.566}
        Yield New Double() {8.185, 5.926, 5.01, 4.5, 4.171, 3.939, 3.765, 3.631, 3.523, 3.434, 3.297, 3.153, 3.003, 2.925, 2.844, 2.761, 2.674, 2.584, 2.489}
        Yield New Double() {8.096, 5.849, 4.938, 4.431, 4.103, 3.871, 3.699, 3.564, 3.457, 3.368, 3.231, 3.088, 2.938, 2.859, 2.778, 2.695, 2.608, 2.517, 2.421}
        Yield New Double() {8.017, 5.78, 4.874, 4.369, 4.042, 3.812, 3.64, 3.506, 3.398, 3.31, 3.173, 3.03, 2.88, 2.801, 2.72, 2.636, 2.548, 2.457, 2.36}
        Yield New Double() {7.945, 5.719, 4.817, 4.313, 3.988, 3.758, 3.587, 3.453, 3.346, 3.258, 3.121, 2.978, 2.827, 2.749, 2.667, 2.583, 2.495, 2.403, 2.305}
        Yield New Double() {7.881, 5.664, 4.765, 4.264, 3.939, 3.71, 3.539, 3.406, 3.299, 3.211, 3.074, 2.931, 2.781, 2.702, 2.62, 2.535, 2.447, 2.354, 2.256}
        Yield New Double() {7.823, 5.614, 4.718, 4.218, 3.895, 3.667, 3.496, 3.363, 3.256, 3.168, 3.032, 2.889, 2.738, 2.659, 2.577, 2.492, 2.403, 2.31, 2.211}
        Yield New Double() {7.77, 5.568, 4.675, 4.177, 3.855, 3.627, 3.457, 3.324, 3.217, 3.129, 2.993, 2.85, 2.699, 2.62, 2.538, 2.453, 2.364, 2.27, 2.169}
        Yield New Double() {7.721, 5.526, 4.637, 4.14, 3.818, 3.591, 3.421, 3.288, 3.182, 3.094, 2.958, 2.815, 2.664, 2.585, 2.503, 2.417, 2.327, 2.233, 2.131}
        Yield New Double() {7.677, 5.488, 4.601, 4.106, 3.785, 3.558, 3.388, 3.256, 3.149, 3.062, 2.926, 2.783, 2.632, 2.552, 2.47, 2.384, 2.294, 2.198, 2.097}
        Yield New Double() {7.636, 5.453, 4.568, 4.074, 3.754, 3.528, 3.358, 3.226, 3.12, 3.032, 2.896, 2.753, 2.602, 2.522, 2.44, 2.354, 2.263, 2.167, 2.064}
        Yield New Double() {7.598, 5.42, 4.538, 4.045, 3.725, 3.499, 3.33, 3.198, 3.092, 3.005, 2.868, 2.726, 2.574, 2.495, 2.412, 2.325, 2.234, 2.138, 2.034}
        Yield New Double() {7.562, 5.39, 4.51, 4.018, 3.699, 3.473, 3.304, 3.173, 3.067, 2.979, 2.843, 2.7, 2.549, 2.469, 2.386, 2.299, 2.208, 2.111, 2.006}
        Yield New Double() {7.314, 5.179, 4.313, 3.828, 3.514, 3.291, 3.124, 2.993, 2.888, 2.801, 2.665, 2.522, 2.369, 2.288, 2.203, 2.114, 2.019, 1.917, 1.805}
        Yield New Double() {7.077, 4.977, 4.126, 3.649, 3.339, 3.119, 2.953, 2.823, 2.718, 2.632, 2.496, 2.352, 2.198, 2.115, 2.028, 1.936, 1.836, 1.726, 1.601}
        Yield New Double() {6.851, 4.787, 3.949, 3.48, 3.174, 2.956, 2.792, 2.663, 2.559, 2.472, 2.336, 2.192, 2.035, 1.95, 1.86, 1.763, 1.656, 1.533, 1.381}
        Yield New Double() {6.635, 4.605, 3.782, 3.319, 3.017, 2.802, 2.639, 2.511, 2.407, 2.321, 2.185, 2.039, 1.878, 1.791, 1.696, 1.592, 1.473, 1.325, 1}
    End Function
End Class