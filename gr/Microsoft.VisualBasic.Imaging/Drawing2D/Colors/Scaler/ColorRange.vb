Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors.Scaler

    Public Structure ColorRange : Implements INamedValue

        Public Property Level$ Implements INamedValue.Key
        Public Property Points As Color()

        ''' <summary>
        ''' 返回和最近的一个颜色点的距离值
        ''' </summary>
        ''' <param name="color"></param>
        ''' <returns></returns>
        Public Function GetMinDistance(color As Color) As Double
            With color
                Dim array As Double() = { .R, .G, .B}
                Return Points.Min(
                    Function(x)
                        Return DistanceMethods.EuclideanDistance(array, New Double() {x.R, x.G, x.B})
                    End Function)
            End With
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace