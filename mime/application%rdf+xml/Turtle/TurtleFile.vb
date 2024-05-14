#Region "Microsoft.VisualBasic::a16b8e8de19d24a4082dde85353994ab, mime\application%rdf+xml\Turtle\TurtleFile.vb"

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

    '   Total Lines: 143
    '    Code Lines: 93
    ' Comment Lines: 27
    '   Blank Lines: 23
    '     File Size: 5.21 KB


    '     Class TurtleFile
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ParseObject, ParseTriple, ReadObjects
    ' 
    '         Sub: (+2 Overloads) Dispose, ParsePrefix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace Turtle

    ''' <summary>
    ''' RDF 1.1 Turtle
    ''' 
    ''' Terse RDF Triple Language
    ''' </summary>
    Public Class TurtleFile : Implements IDisposable

        Dim prefix As New Dictionary(Of String, String)
        Dim stream As StreamReader

        Private disposedValue As Boolean

        Sub New(ttlfile As String)
            Call Me.New(ttlfile.OpenReadonly)
        End Sub

        Sub New(file As Stream)
            stream = New StreamReader(file)
        End Sub

        Public Iterator Function ReadObjects() As IEnumerable(Of Triple)
            Dim line As Value(Of String) = ""
            Dim cache As New List(Of String)
            Dim ttl As Triple = Nothing

            Do While (line = stream.ReadLine) IsNot Nothing
                If line.Value.StringEmpty Then
                    Continue Do
                End If

                If line.Value.Last = "."c Then
                    If cache = 0 Then
                        ttl = ParseObject(line)
                    Else
                        cache.Add(line)
                        ttl = ParseObject(cache.PopAll.JoinBy(" "))
                    End If

                    If Not ttl Is Nothing Then
                        Yield ttl
                    End If
                Else
                    cache.Add(line)
                End If
            Loop

            If cache > 0 Then
                ttl = ParseObject(cache.PopAll.JoinBy(" "))

                If Not ttl Is Nothing Then
                    Yield ttl
                End If
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="line"></param>
        ''' <returns>
        ''' this function returns nothing if the line data is a prefix
        ''' </returns>
        Private Function ParseObject(line As String) As Triple
            If line.StartsWith("@prefix") Then
                Call ParsePrefix(line)
                Return Nothing
            Else
                Return ParseTriple(line)
            End If
        End Function

        Private Function ParseTriple(line As String) As Triple
            Dim tuple = line.GetTagValue(vbTab, trim:=True)
            Dim tokens As String() = tuple.Value.Trim("."c, " "c).StringSplit("\s;\s")
            Dim subj As String = tuple.Name
            Dim rels As Relation() = New Relation(tokens.Length - 1) {}

            For i As Integer = 0 To tokens.Length - 1
                Dim t = tokens(i).StringSplit("(\t)|(\s,\s)", trimTrailingEmptyStrings:=True)
                Dim predicate As String = t(0)
                Dim objs As String() = t.Skip(1) _
                    .Where(Function(si) Not si.IsPattern("\s+,\s+")) _
                    .Select(Function(si) si.Trim(""""c)) _
                    .ToArray

                rels(i) = New Relation With {
                    .predicate = predicate,
                    .objs = objs
                }
            Next

            Return New Triple With {
                .subject = subj,
                .relations = rels
            }
        End Function

        Private Sub ParsePrefix(line As String)
            Dim data = line.GetTagValue(":", trim:=True)
            Dim prefix As String = data.Name.Substring(8).Trim
            Dim url As String = data.Value.Trim("."c, "<"c, ">"c, " "c, ASCII.TAB)

            ' The Turtle language originally permitted only the syntax including the '@'
            ' character for writing prefix and base directives. The case-insensitive 
            ' PREFIX' and 'BASE' forms were added to align Turtle's syntax with that of
            ' SPARQL. It is advisable to serialize RDF using the '@prefix' and '@base' forms
            ' until RDF 1.1 Turtle parsers are widely deployed.
            Me.prefix.Add(prefix, url)
        End Sub

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)
                    Call stream.Dispose()
                End If

                ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
                ' TODO: 将大型字段设置为 null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
        ' Protected Overrides Sub Finalize()
        '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
