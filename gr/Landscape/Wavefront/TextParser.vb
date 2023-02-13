Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Values
Imports Microsoft.VisualBasic.Linq

Namespace Wavefront

    Public Module TextParser

        <Extension>
        Public Function ParseFile(buf As StreamReader) As OBJ
            Dim line As New Value(Of String)
            Dim comments As New StringBuilder
            Dim mtllib As String = Nothing
            Dim parts As New List(Of ObjectPart)
            Dim vertex As New List(Of Point3D)
            Dim vn As New List(Of Point3D)
            Dim g As String = Nothing
            Dim usemtl As String = Nothing
            Dim f As New List(Of Triangle)

            Do While Not (line = buf.ReadLine) Is Nothing
                If line.First = "#"c Then
                    comments.AppendLine(line.Trim("#"c, " "c))
                ElseIf Not line.Value.StringEmpty Then
                    If line.StartsWith("mtllib") Then
                        mtllib = Mid(line.Value, "mtllib".Length + 1).Trim
                    Else
                        Dim tokens As String() = line.Split

                        Select Case tokens(Scan0)
                            Case "v"
                                Call vertex.Add(tokens.Skip(1).Select(AddressOf Val).ToArray.DoCall(Function(v) New Point3D(v)))
                            Case "vn"
                                Call vn.Add(tokens.Skip(1).Select(AddressOf Val).ToArray.DoCall(Function(v) New Point3D(v)))
                            Case "g"
                                g = tokens.Skip(1).JoinBy(" ")
                            Case "usemtl"
                                usemtl = tokens.Skip(1).JoinBy(" ")
                            Case "f"
                                Dim ints As Integer()() = tokens _
                                    .Skip(1) _
                                    .Take(3) _
                                    .Select(Function(str)
                                                Return str.Split("/"c).Select(Function(c) CInt(Val(c))).ToArray
                                            End Function) _
                                    .ToArray

                                f.Add(New Triangle With {
                                    .v3 = ints.Select(Function(v) v(Scan0)).ToArray,
                                    .vn3 = ints.Select(Function(v) v(2)).ToArray,
                                    .comment = tokens(4)
                                })
                            Case Else
                                Throw New NotImplementedException(line)
                        End Select
                    End If
                Else
                    Dim objPart As New ObjectPart With {
                        .vertex = vertex.ToArray,
                        .vn = vn.ToArray,
                        .g = g,
                        .usemtl = usemtl,
                        .f = f.ToArray
                    }

                    If Not objPart.IsEmpty Then
                        ' create new part
                        Call parts.Add(objPart)
                    End If

                    g = Nothing
                    usemtl = Nothing
                    f.Clear()
                    vertex.Clear()
                    vn.Clear()
                End If
            Loop

            If vertex.Count > 0 Then
                Dim objPart As New ObjectPart With {
                    .vertex = vertex.ToArray,
                    .vn = vn.ToArray,
                    .g = g,
                    .usemtl = usemtl,
                    .f = f.ToArray
                }

                ' create new part
                parts.Add(objPart)
            End If

            Return New OBJ With {
                .comment = comments.ToString,
                .mtllib = mtllib,
                .parts = parts.ToArray
            }
        End Function
    End Module
End Namespace