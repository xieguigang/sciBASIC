#Region "Microsoft.VisualBasic::2ae8a93a96a05ced9dd712d5238cadcc, gr\Landscape\Wavefront\TextParser.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 103
    '    Code Lines: 90
    ' Comment Lines: 2
    '   Blank Lines: 11
    '     File Size: 4.14 KB


    '     Module TextParser
    ' 
    '         Function: ParseFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
