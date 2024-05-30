#Region "Microsoft.VisualBasic::403988e5bf8bbabafb9d4f03d3e3d9b7, Data\FullTextSearch\InvertedIndex.vb"

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

    '   Total Lines: 153
    '    Code Lines: 109 (71.24%)
    ' Comment Lines: 15 (9.80%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 29 (18.95%)
    '     File Size: 4.37 KB


    ' Class InvertedIndex
    ' 
    '     Properties: lastId, size
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: Add, GenericEnumerator, intersection, Search, split
    ' 
    '     Sub: Add
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

''' <summary>
''' The core of FTS is a data structure called Inverted Index. 
''' The Inverted Index associates every word in documents with documents that contain the word.
''' </summary>
''' <remarks>
''' https://artem.krylysov.com/blog/2020/07/28/lets-build-a-full-text-search-engine/
''' </remarks>
Public Class InvertedIndex : Implements Enumeration(Of NamedCollection(Of Integer))

    ReadOnly index As New Dictionary(Of String, List(Of Integer))

    Dim id As i32 = 0

    Public ReadOnly Property lastId As Integer
        Get
            Return CInt(id)
        End Get
    End Property

    Public ReadOnly Property size As Integer
        Get
            Return index.Count
        End Get
    End Property

    Sub New()
    End Sub

    Sub New(index As Dictionary(Of String, List(Of Integer)), lastId As Integer)
        Me.id = lastId
        Me.index = index
    End Sub

    Public Sub Add(docs As IEnumerable(Of String))
        For Each doc As String In docs
            Call Add(doc)
        Next
    End Sub

    Public Function Add(doc As String) As Boolean
        Dim id As Integer
        Dim tokens As String() = split(doc)

        If tokens.IsNullOrEmpty Then
            Return False
        Else
            id = ++Me.id
        End If

        Dim append As Boolean = False

        For Each str As String In tokens
            If str = "" Then
                Continue For
            Else
                ' tokens may be all empty string
                '
                append = True
            End If

            If Not index.ContainsKey(str) Then
                Call index.Add(str, New List(Of Integer))
            End If

            If index(str).Count = 0 OrElse index(str).Last <> id Then
                Call index(str).Add(id)
            End If
        Next

        Return append
    End Function

    Private Function split(doc As String) As String()
        doc = Strings.Trim(doc).ToLower

        If doc.StringEmpty Then
            Return Nothing
        Else
            Return doc.Split({"!"c, "?"c, "+"c, "-"c, "*"c, "/"c, "."c, ","c, " "c, ASCII.TAB, "'"c, """"c, "����"c, "����"c})
        End If
    End Function

    ''' <summary>
    ''' Boolean queries
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    Public Function Search(text As String) As IReadOnlyCollection(Of Integer)
        Dim tokens As String() = split(text)

        If tokens.IsNullOrEmpty Then
            Return Nothing
        End If

        Dim r As List(Of Integer) = Nothing

        For Each token As String In tokens
            If token = "" Then
                Continue For
            End If

            If index.ContainsKey(token) Then
                If r Is Nothing Then
                    r = New List(Of Integer)(index(token))
                Else
                    r = intersection(r, index(token))
                End If
            Else
                ' Token doesn't exist.
                Return Nothing
            End If
        Next

        Return r
    End Function

    Private Shared Function intersection(a As List(Of Integer), b As List(Of Integer)) As List(Of Integer)
        Dim maxLen = a.Count

        If b.Count > maxLen Then
            maxLen = b.Count
        End If

        Dim r As New List(Of Integer)
        Dim i, j As Integer

        Do While i < a.Count AndAlso j < b.Count
            If a(i) < b(j) Then
                i += 1
            ElseIf a(i) > b(j) Then
                j += 1
            Else
                r.Add(a(i))
                i += 1
                j += 1
            End If
        Loop

        Return r
    End Function

    Public Iterator Function GenericEnumerator() As IEnumerator(Of NamedCollection(Of Integer)) Implements Enumeration(Of NamedCollection(Of Integer)).GenericEnumerator
        For Each token In index
            Yield New NamedCollection(Of Integer)(token.Key, token.Value)
        Next
    End Function
End Class
