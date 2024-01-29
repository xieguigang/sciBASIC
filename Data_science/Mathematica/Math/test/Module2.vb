Imports Microsoft.VisualBasic.Math.LinearAlgebra.Solvers
Imports np = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.Numpy

Module Module2

    Sub Main()
        Call test2()
    End Sub


    Sub test2()
        Dim identity = np.eye(13)
        Dim pm As PowerMethod = New PowerMethod(identity)
        pm.powerMethod()
        '  pm.printDiscrepancy()
        pm.printVectorDiscrepancy()

        Pause()
    End Sub

    Sub test1()
        Dim matrA = New Double()() {
     New Double() {0.5757, -0.0758, 0.0152, 0.0303, 0.1061, 0.0001445},
     New Double() {0.0788, 0.9014, 0.0000, -0.0606, 0.0606, 0.05699},
     New Double() {0.0455, 0.0000, 0.7242, -0.2121, 0.1212, -0.3365},
     New Double() {-0.0909, 0.1909, 0.0000, 0.7121, -0.0303, 0.1222},
     New Double() {0.3788, 0.0000, 0.1364, 0.0152, 0.8484, 0.03369},
     New Double() {0.3457, 0.345893, 0.007489, 0.027489, -0.002374, 0.8923}
 }

        Dim pm As PowerMethod = New PowerMethod(matrA)
        pm.powerMethod()
        '  pm.printDiscrepancy()
        pm.printVectorDiscrepancy()

        Pause()
    End Sub
End Module
