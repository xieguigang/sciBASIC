#Region "Microsoft.VisualBasic::ea27d8328b34b141abf2d54101625d9e, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Extensions.vb"

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

    '     Module Extensions
    ' 
    '         Function: (+2 Overloads) Model3D, TupleZ
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric

Namespace Drawing3D.Models

    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Model3D(path As Path3D, color As Color) As Surface
            Return New Surface With {
                .brush = New SolidBrush(color),
                .vertices = path.Points.ToArray
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Model3D(shape As Shape3D, color As Color) As Surface()
            Return shape _
                .paths _
                .Select(Function(path) path.Model3D(color)) _
                .ToArray
        End Function

        <Extension>
        Public Function TupleZ(p As PointF, z#) As Point3D
            Return New Point3D(p, z)
        End Function
    End Module
End Namespace
