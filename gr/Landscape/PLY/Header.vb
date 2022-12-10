Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Ply

    Public Class Header

        Public Property comment As String
        Public Property element_vertex As Integer
        Public Property element_face As Integer
        Public Property properties As NamedValue(Of String)()

        Friend Sub WriteAsciiText(file As StreamWriter)
            Call file.WriteLine($"ply")
            Call file.WriteLine($"format ascii 1.0")
            Call file.WriteLine($"comment {comment}")
            Call file.WriteLine($"element vertex {element_vertex}")
            Call file.WriteLine($"element face {element_face}")

            For Each field As NamedValue(Of String) In properties
                Call file.WriteLine($"property {field.Value} {field.Name}")
            Next

            Call file.WriteLine($"end_header")
        End Sub

    End Class

    Public Module SimplePlyWriter

        Public Function WriteAsciiText(pointCloud As IEnumerable(Of PointCloud), buffer As Stream) As Boolean
            Using file As New StreamWriter(buffer, Encoding.ASCII) With {
                .AutoFlush = True,
                .NewLine = vbLf
            }
                Dim vertex As PointCloud() = pointCloud.ToArray
                Dim header As New Header With {
                    .comment = "Point Cloud Model",
                    .element_face = -1,
                    .element_vertex = vertex.Length,
                    .properties = {
                        New NamedValue(Of String)("x", "float"),
                        New NamedValue(Of String)("y", "float"),
                        New NamedValue(Of String)("z", "float"),
                        New NamedValue(Of String)("color", "string")
                    }
                }

                Call header.WriteAsciiText(file)
                Call file.Flush()

                For Each point As PointCloud In vertex
                    Call file.WriteLine($"{point.x.ToString("F3")} {point.y.ToString("F3")} {point.z.ToString("F3")} {point.color}")
                Next
            End Using

            Return True
        End Function
    End Module

    Public Class PointCloud

        Public Property x As Double
        Public Property y As Double
        Public Property z As Double
        Public Property color As String

    End Class
End Namespace