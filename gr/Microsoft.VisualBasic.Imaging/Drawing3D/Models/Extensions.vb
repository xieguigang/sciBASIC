Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric

Namespace Drawing3D.Models

    Public Module Extensions

        <Extension> Public Function Model3D(path As Path3D, color As Color) As Surface
            Return New Surface With {
                .brush = New SolidBrush(color),
                .vertices = path.Points.ToArray
            }
        End Function

        <Extension> Public Function Model3D(shape As Shape3D, color As Color) As Surface()
            Return shape _
                .paths _
                .Select(Function(path) path.Model3D(color)) _
                .ToArray
        End Function
    End Module
End Namespace