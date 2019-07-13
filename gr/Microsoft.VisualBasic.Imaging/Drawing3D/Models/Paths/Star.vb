#Region "Microsoft.VisualBasic::ca441c0f62de937f1ba7b9de97a7f2ee, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Paths\Star.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Star
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math

Namespace Drawing3D.Models.Isometric.Paths

    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>
    Public Class Star : Inherits Path3D

        Public Sub New(origin As Point3D, outerRadius#, innerRadius#, points%)
            MyBase.New()

            For i As Integer = 0 To points * 2 - 1
                Dim r As Double = If(i Mod 2 = 0, outerRadius, innerRadius)
                Dim p As New Point3D(
                    (r * Cos(i * Math.PI / points)) + origin.X,
                    (r * Sin(i * Math.PI / points)) + origin.Y,
                    origin.Z)

                Call Push(p)
            Next
        End Sub
    End Class
End Namespace
