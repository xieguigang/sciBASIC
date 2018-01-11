

Imports Microsoft.VisualBasic.Math.Algebra.LinearProgramming

Public Class LPPExamples

    Public Shared Function minimizeExample() As LPP
        Return New LPP("Min", New String() {}, New Double() {1, -1, 2, -5}, New Double()() {New Double() {1, 1, 2, 4}, New Double() {0, 3, 1, 8}}, New String() {"=", "="}, New Double() {6, 3}, 0)
    End Function

    Public Shared Function smallMinimizeExample() As LPP
        Return New LPP("Min", New String() {"a", "b"}, New Double() {0.6, 0.8}, New Double()() {New Double() {0.6, 0.2}, New Double() {0.1, 0.5}}, New String() {"³", "³"}, New Double() {30, 26}, 0)

    End Function

    Public Shared Function maximizeExample() As LPP
        Return New LPP("Max", New String() {}, New Double() {2, 3, 3}, New Double()() {New Double() {3, 2, 0}, New Double() {-1, 1, 4}, New Double() {2, -2, 5}}, New String() {"²", "²", "²"}, New Double() {60, 10, 50}, 0)
    End Function

    Public Shared Function transshipment() As LPP
        Return New LPP("Min", New String() {}, New Double() {16, 21, 18, 16, 22, 25, 23, 15, 29, 20, 17, 24}, New Double()() {New Double() {1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, New Double() {0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0}, New Double() {0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0}, New Double() {1, 0, 1, 0, 1, 0, -1, -1, -1, 0, 0, 0}, New Double() {0, 1, 0, 1, 0, 1, 0, 0, 0, -1, -1, -1}, New Double() {0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0}, New Double() {0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0}, New Double() {0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1}}, New String() {"=", "=", "=", "=", "=", "²", "²", "²"}, New Double() {72, 105, 83, 0, 0, 90, 80, 120}, 0)
    End Function

    Public Shared Function strictEquality() As LPP
        Return New LPP("Min", New String() {}, New Double() {3, 4, 5, 2, 7, 8}, New Double()() {New Double() {1, 1, 1, 0, 0, 0}, New Double() {0, 0, 0, 0, 1, 1}, New Double() {1, 0, 0, -1, 0, 0}, New Double() {0, 1, 0, 1, -1, 0}, New Double() {0, 0, 1, 0, 0, -1}}, New String() {"=", "=", "=", "=", "="}, New Double() {5, 5, 0, 0, 0}, 0)
    End Function
End Class

