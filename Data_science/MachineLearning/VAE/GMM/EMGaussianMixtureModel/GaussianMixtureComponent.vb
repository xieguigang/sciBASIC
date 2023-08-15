Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports RealMatrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix
Imports std = System.Math

Namespace GMM.EMGaussianMixtureModel

    Public Class GaussianMixtureComponent

        Const PI As Double = std.PI
        Const e As Double = std.E

        Private position As Integer
        Private meanField As RealMatrix
        Private covMatrixField As RealMatrix
        Private weightField As Double

        Public Sub New(position As Integer, mean As RealMatrix, covMatrix As RealMatrix, weight As Double)
            Me.position = position
            meanField = mean
            covMatrixField = covMatrix
            weightField = weight
        End Sub

        Public Overridable Function multiVariateGaussianPDF(x As Double(), means As RealMatrix, CovMatrix As RealMatrix) As Double
            Dim d = x.Length
            Dim xMatrix As RealMatrix = New RealMatrix(x)
            Dim xMinusMeansMatrix = xMatrix - means
            Dim lu As LUDecomposition = New LUDecomposition(CovMatrix)
            Dim CovInverse As RealMatrix = CType(CType(lu.Solve(CType(RealMatrix.Identity(lu.Pivot.Length), RealMatrix)), RealMatrix).Inverse(), RealMatrix)
            Dim eExponent As Double = -0.5 * (CType((CType(xMinusMeansMatrix.Transpose(), RealMatrix) * CovInverse), RealMatrix) * xMinusMeansMatrix)(0, 0)

            Dim CovMatrixDeterminant As Double = (New LUDecomposition(CovMatrix)).Determinant()
            Return 1 / (std.Pow(2 * PI, d / 2) * std.Sqrt(CovMatrixDeterminant)) * std.Pow(e, eExponent)
        End Function


        Public Sub New(position As Integer, mean As Double(), variance As Double()(), weight As Double)
            Me.New(position, New RealMatrix(mean), New RealMatrix(variance), weight)
        End Sub

        Public Overridable ReadOnly Property Mean As RealMatrix
            Get
                Return meanField
            End Get
        End Property

        Public Overridable ReadOnly Property CovMatrix As RealMatrix
            Get
                Return covMatrixField
            End Get
        End Property

        Public Overridable ReadOnly Property Weight As Double
            Get
                Return weightField
            End Get
        End Property

        Public Overridable Function componentPDFandProb(x As Double()) As Double
            Return weightField * multiVariateGaussianPDF(x, meanField, covMatrixField)
        End Function
    End Class

End Namespace
