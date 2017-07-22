Imports Microsoft.VisualBasic.Math.Matrix
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace PCA

    Public Class PCA

        ''' <summary>
        ''' 使用<see cref="CenterNormalize(GeneralMatrix)"/>特征中心化之后的结果矩阵
        ''' </summary>
        ''' <returns></returns>
        Public Property B As GeneralMatrix
        ''' <summary>
        ''' B的协方差矩阵C
        ''' </summary>
        ''' <returns></returns>
        Public Property C As GeneralMatrix

        ''' <summary>
        ''' SVD(<see cref="C"/>)
        ''' </summary>
        ''' <returns></returns>
        Public Property SVD As SingularValueDecomposition

        Public ReadOnly Property S As Vector
            Get
                Return SVD.SingularValues
            End Get
        End Property

        Public ReadOnly Property V As GeneralMatrix
            Get
                Return SVD.V
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return S.ToString
        End Function
    End Class
End Namespace