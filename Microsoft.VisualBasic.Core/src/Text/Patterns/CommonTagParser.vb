#Region "Microsoft.VisualBasic::a429e057377336a53e590c16bb738180, Microsoft.VisualBasic.Core\src\Text\Patterns\CommonTagParser.vb"

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

    '   Total Lines: 75
    '    Code Lines: 52
    ' Comment Lines: 12
    '   Blank Lines: 11
    '     File Size: 2.58 KB


    '     Class CommonTagParser
    ' 
    '         Properties: LabelPrefix, MaxColumnIndex, maxLen, nameMatrix
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: walkLabels
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Text.Patterns

    Public Class CommonTagParser

        Public ReadOnly Property nameMatrix As Char()()
        Public ReadOnly Property maxLen As Integer

        ''' <summary>
        ''' the max length of the labels common parts
        ''' </summary>
        Public ReadOnly Property MaxColumnIndex As Integer

        ''' <summary>
        ''' get the max common prefixed label value
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LabelPrefix As String
            Get
                Return nameMatrix(Scan0).Take(MaxColumnIndex).CharString
            End Get
        End Property

        Sub New(allSampleNames As IEnumerable(Of String), Optional maxDepth As Boolean = False)
            nameMatrix = allSampleNames.Select(Function(name) name.ToArray).ToArray
            maxLen% = Aggregate name As Char()
                      In nameMatrix
                      Into Max(name.Length)

            Call walkLabels(maxDepth)
        End Sub

        Private Sub walkLabels(maxDepth As Boolean)
            Dim col As Char()
            Dim nameMatrix = Me.nameMatrix _
                .Where(Function(chrs) chrs.Length > 0) _
                .ToArray

            For i As Integer = 0 To maxLen - 1
                _MaxColumnIndex = i
                col = nameMatrix _
                    .Select(Function(name)
                                Return name.ElementAtOrNull(MaxColumnIndex)
                            End Function) _
                    .ToArray

                '      colIndex
                '      |
                ' iBAQ-AA-1
                ' iBAQ-BB-2
                If maxDepth Then
                    If Not nameMatrix _
                        .GroupBy(Function(cs)
                                     Return cs.Take(MaxColumnIndex + 1).CharString
                                 End Function) _
                        .All(Function(c)
                                 Return c.Count > 1
                             End Function) Then

                        _MaxColumnIndex -= 1
                        Exit For
                    End If
                Else
                    If col.Distinct.Count > 1 Then
                        ' group label at here
                        Exit For
                    End If
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return LabelPrefix
        End Function
    End Class
End Namespace
