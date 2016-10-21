Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language

Namespace Plot3D

    Public Module Data

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="f"></param>
        ''' <param name="x">x取值范围</param>
        ''' <param name="y">y取值范围</param>
        ''' <param name="xsteps!"></param>
        ''' <param name="ysteps!"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Evaluate(f As Func(Of Double, Double, Double),
                                 x As DoubleRange,
                                 y As DoubleRange,
                                 Optional xsteps! = 0.01,
                                 Optional ysteps! = 0.01) As Point3D()
            Dim out As New List(Of Point3D)

            For xi# = x.Min To x.Max Step xsteps!
                For yi# = y.Min To y.Max Step ysteps!
                    out += New Point3D With {
                        .X = xi#,
                        .Y = yi#,
                        .Z# = f(xi, yi)
                    }
                Next
            Next

            Return out
        End Function
    End Module
End Namespace