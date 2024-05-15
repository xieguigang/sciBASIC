#Region "Microsoft.VisualBasic::730427f4a9abf5b32dc2a672b92d2ec2, Microsoft.VisualBasic.Core\src\Extensions\Collection\Linq\VectorAssertor.vb"

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

    '   Total Lines: 52
    '    Code Lines: 38
    ' Comment Lines: 6
    '   Blank Lines: 8
    '     File Size: 1.80 KB


    '     Structure VectorAssertor
    ' 
    '         Function: ToString
    '         Operators: <>, =, (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Linq

    Public Structure VectorAssertor(Of T)

        Dim ALL As T
        Dim equal As Func(Of T, T, Boolean)
        Dim likes#

        Public Overrides Function ToString() As String
            Return ALL.ToString
        End Function

        Public Shared Operator Like(list As IEnumerable(Of T), assert As VectorAssertor(Of T)) As Boolean
            Dim array As T() = list.ToArray
            Dim n#

            With assert
                If .equal Is Nothing Then
                    n = array.Where(Function(x) x.Equals(.ALL)).Count
                Else
                    n = array.Where(Function(x) .equal(x, .ALL)).Count
                End If

                If .likes = 0R Then
                    Return (n / array.Length) >= 0.65
                Else
                    Return (n / array.Length) >= .likes
                End If
            End With
        End Operator

        ''' <summary>
        ''' ALL elements in target <paramref name="list"/> equals to <paramref name="assert"/> value
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="assert"></param>
        ''' <returns></returns>
        Public Shared Operator =(list As IEnumerable(Of T), assert As VectorAssertor(Of T)) As Boolean
            With assert
                If .equal Is Nothing Then
                    Return list.All(Function(x) x.Equals(.ALL))
                Else
                    Return list.All(Function(x) .equal(x, .ALL))
                End If
            End With
        End Operator

        Public Shared Operator <>(list As IEnumerable(Of T), assert As VectorAssertor(Of T)) As Boolean
            Return Not list = assert
        End Operator
    End Structure
End Namespace
