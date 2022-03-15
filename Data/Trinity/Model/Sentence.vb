#Region "Microsoft.VisualBasic::d41bab3b0b274a8f99bef08afae5c602, sciBASIC#\Data\Trinity\Model\Sentence.vb"

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

    '   Total Lines: 100
    '    Code Lines: 70
    ' Comment Lines: 10
    '   Blank Lines: 20
    '     File Size: 2.89 KB


    '     Class Sentence
    ' 
    '         Properties: IsEmpty, segments
    ' 
    '         Function: matchIndex, Parse, searchIndex, ToString, Trim
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Model

    Public Class Sentence

        Public Property segments As Segment()

        Public ReadOnly Property IsEmpty As Boolean
            Get
                If segments.IsNullOrEmpty Then
                    Return True
                End If

                If segments.All(Function(s) s.IsEmpty) Then
                    Return True
                End If

                Return False
            End Get
        End Property

        ''' <summary>
        ''' exactly token matched
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        Public Function matchIndex(token As String) As Integer
            Dim index As Integer = -1

            For i As Integer = 0 To segments.Length - 1
                index = segments(i).matchIndex(token)

                If index > -1 Then
                    Return i * 1000 + index
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' search for starts with [prefix]
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        Public Function searchIndex(token As String) As Integer
            Dim index As Integer = -1

            For i As Integer = 0 To segments.Length - 1
                index = segments(i).searchIndex(token)

                If index > -1 Then
                    Return i * 1000 + index
                End If
            Next

            Return -1
        End Function

        Public Overrides Function ToString() As String
            Return segments.JoinBy("; ")
        End Function

        Friend Shared Function Parse(line As String) As Sentence
            Return New Sentence With {
               .segments = line _
                   .Split(","c, ";"c, """"c, "`"c, "~"c) _
                   .Select(Function(str)
                               Return New Segment With {
                                   .tokens = str.Trim.StringSplit("\s+")
                               }
                           End Function) _
                   .ToArray
           }
        End Function

        Friend Function Trim() As Sentence
            Dim list As New List(Of Segment)
            Dim data As Segment

            For Each block As Segment In segments
                data = New Segment With {
                    .tokens = block.tokens _
                        .Where(Function(str)
                                   Return Not TextRank.IsEmpty(str)
                               End Function) _
                        .ToArray
                }

                If Not data.tokens.IsNullOrEmpty Then
                    list.Add(data)
                End If
            Next

            Return New Sentence With {
               .segments = list.ToArray
            }
        End Function

    End Class
End Namespace
