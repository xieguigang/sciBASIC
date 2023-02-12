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
            Dim mtllib As String
            Dim parts As New List(Of ObjectPart)
            Dim objPart As New ObjectPart
            Dim vertex As New List(Of Point3D)

            Do While Not (line = buf.ReadLine) Is Nothing
                If line.First = "#"c Then
                    comments.AppendLine(line.Trim("#"c, " "c))
                ElseIf Not line.Value.StringEmpty Then
                    If line.StartsWith("mtllib") Then
                        mtllib = Mid(line.Value, 1, "mtllib".Length).Trim
                    Else
                        Select Case line.First
                            Case "v"c
                                Call vertex.Add(line.Split.Skip(1).Select(AddressOf Val).ToArray.DoCall(Function(v) New Point3D(v)))
                            Case Else
                                Throw New NotImplementedException(line)
                        End Select
                    End If
                End If
            Loop
        End Function
    End Module
End Namespace