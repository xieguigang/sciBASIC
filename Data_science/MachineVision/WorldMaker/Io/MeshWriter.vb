' ---------------------------------------------------------------------------
' 网格导出：STL（主） + 网格 PLY（带顶点色）
'
' 为什么 STL 走 Landscape、PLY 自己写：
'   * Landscape 的 StlWriter 已经处理好了二进制 STL 的头/面法向/字节序，
'     没必要重复实现（MeshBuilder.AddTriangle 内部会自动算面法向）；
'   * Landscape 的 PlyWriter 只支持点云，不支持 face，而我们需要一个
'     “能在 MeshLab 里直接看到颜色”的网格文件，因此这里补一个几十行的
'     ASCII PLY 网格写入器。
' ---------------------------------------------------------------------------

Imports System.IO
Imports System.Text
Imports CC = Microsoft.VisualBasic.MachineVision.Cloudy.Core
Imports CR = Microsoft.VisualBasic.MachineVision.Cloudy.Reconstruction
Imports LS = Microsoft.VisualBasic.Imaging.Landscape

Namespace Io

    ''' <summary>
    ''' 三角网格写出。
    ''' </summary>
    Public NotInheritable Class MeshWriter

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 写出二进制 STL。
        ''' </summary>
        Public Shared Function SaveStl(mesh As CR.MeshData, path As String, header As String) As Integer
            If mesh Is Nothing OrElse mesh.TriangleCount = 0 Then
                Throw New InvalidOperationException($"网格为空，无法写出 STL: {path}")
            End If

            Dim builder As New LS.Stl.MeshBuilder()

            For i As Integer = 0 To mesh.TriangleCount - 1
                Dim triangle = mesh.Triangle(i)

                Call builder.AddTriangle(
                    triangle.a.X, triangle.a.Y, triangle.a.Z,
                    triangle.b.X, triangle.b.Y, triangle.b.Z,
                    triangle.c.X, triangle.c.Y, triangle.c.Z)
            Next

            Call LS.Stl.StlWriter.Write(builder, path, header)

            Return builder.TriangleCount
        End Function

        ''' <summary>
        ''' 写出 ASCII PLY 网格（顶点 + 可选顶点色 + 三角面）。
        ''' </summary>
        Public Shared Function SavePly(mesh As CR.MeshData, path As String, Optional includeColor As Boolean = True) As Integer
            If mesh Is Nothing OrElse mesh.TriangleCount = 0 Then
                Throw New InvalidOperationException($"网格为空，无法写出 PLY: {path}")
            End If

            Dim colors As Integer() = mesh.Colors
            Dim withColor As Boolean = includeColor AndAlso colors IsNot Nothing AndAlso colors.Length = mesh.VertexCount

            Using writer As New StreamWriter(path, False, New UTF8Encoding(False))
                Call writer.WriteLine("ply")
                Call writer.WriteLine("format ascii 1.0")
                Call writer.WriteLine("comment reconstructed by WorldMaker (3DGS surface cloud + Cloudy)")
                Call writer.WriteLine($"element vertex {mesh.VertexCount}")
                Call writer.WriteLine("property float x")
                Call writer.WriteLine("property float y")
                Call writer.WriteLine("property float z")

                If withColor Then
                    Call writer.WriteLine("property uchar red")
                    Call writer.WriteLine("property uchar green")
                    Call writer.WriteLine("property uchar blue")
                End If

                Call writer.WriteLine($"element face {mesh.TriangleCount}")
                Call writer.WriteLine("property list uchar int vertex_indices")
                Call writer.WriteLine("end_header")

                Dim vertices As CC.Vector3d() = mesh.Vertices

                For i As Integer = 0 To mesh.VertexCount - 1
                    Dim v As CC.Vector3d = vertices(i)
                    Dim sb As New StringBuilder()

                    Call sb.Append($"{v.X:F6} {v.Y:F6} {v.Z:F6}")

                    If withColor Then
                        Dim argb As Integer = colors(i)

                        Call sb.Append($" {(argb >> 16) And 255} {(argb >> 8) And 255} {argb And 255}")
                    End If

                    Call writer.WriteLine(sb.ToString())
                Next

                Dim triangles As Integer() = mesh.Triangles

                For i As Integer = 0 To mesh.TriangleCount - 1
                    Call writer.WriteLine($"3 {triangles(i * 3)} {triangles(i * 3 + 1)} {triangles(i * 3 + 2)}")
                Next
            End Using

            Return mesh.TriangleCount
        End Function

    End Class

End Namespace
