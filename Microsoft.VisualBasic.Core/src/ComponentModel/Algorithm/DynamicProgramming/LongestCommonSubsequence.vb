#Region "Microsoft.VisualBasic::1dd182cc126f1ec9add3568244450e3c, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\DynamicProgramming\LongestCommonSubsequence.vb"

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

    '   Total Lines: 82
    '    Code Lines: 59 (71.95%)
    ' Comment Lines: 10 (12.20%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 13 (15.85%)
    '     File Size: 2.51 KB


    '     Class LongestCommonSubsequence
    ' 
    '         Properties: length, reversed, Sequence, si, ti
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: findMatch
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Python

Namespace ComponentModel.Algorithm.DynamicProgramming

    ''' <summary>
    ''' Longest Common Subsequence
    ''' </summary>
    Public Class LongestCommonSubsequence(Of T)

        Public Property length As Integer
        Public Property si As Integer
        Public Property ti As Integer
        Public Property reversed As Boolean

        Dim a As T()
        Dim b As T()

        Public ReadOnly Property Sequence() As T()
            Get
                If length >= 0 Then
                    Return a.slice(si, si + length)
                Else
                    Return {}
                End If
            End Get
        End Property

        Sub New(s As T(), t As T(), equals As IEquals(Of T))
            Dim mf = findMatch(s, t, equals)
            Dim tr = t.slice(0).Reverse().ToArray
            Dim mr = findMatch(s, tr, equals)

            If (mf(0) >= mr(0)) Then
                length = mf(0)
                si = mf(1)
                ti = mf(2)
                reversed = False
            Else
                length = mr(0)
                si = mr(1)
                ti = t.Length - mr(2) - mr(0)
                reversed = True
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="t"></param>
        ''' <param name="equals"></param>
        ''' <returns>``{length, si, ti}``</returns>
        Private Shared Function findMatch(s As T(), t As T(), equals As IEquals(Of T)) As Integer()
            Dim m = s.Length
            Dim n = t.Length
            Dim match = {0, -1, -1}
            Dim l = New Integer(m - 1)() {}

            For i As Integer = 0 To m - 1
                l(i) = New Integer(n - 1) {}

                For j As Integer = 0 To n - 1
                    If equals(s(i), t(j)) Then
                        Dim v = If(i = 0 OrElse j = 0, 1, l(i - 1)(j - 1) + 1)

                        l(i)(j) = v

                        If (v > match.Length) Then
                            match(0) = v
                            match(1) = i - v + 1
                            match(2) = j - v + 1
                        End If
                    Else
                        l(i)(j) = 0
                    End If
                Next
            Next

            Return match
        End Function
    End Class
End Namespace
