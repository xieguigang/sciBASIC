Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports RealMatrix = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.NumericMatrix
Imports std = System.Math

Namespace GMM.EMGaussianMixtureModel

    Public Class GaussianMixtureComponent

        Const PI As Double = std.PI
        Const e As Double = std.E

        Public ReadOnly Property Position As Integer
        Public Overridable ReadOnly Property Mean As RealMatrix
        Public Overridable ReadOnly Property CovMatrix As RealMatrix
        Public Overridable ReadOnly Property Weight As Double

        Public Sub New(position As Integer, mean As RealMatrix, covMatrix As RealMatrix, weight As Double)
            Me.Position = position
            Me.Mean = mean
            Me.CovMatrix = covMatrix
            Me.Weight = weight
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function componentPDFandProb(x As Double()) As Double
            Return _Weight * multiVariateGaussianPDF(x, _Mean, _CovMatrix)
        End Function
    End Class
End Namespace
