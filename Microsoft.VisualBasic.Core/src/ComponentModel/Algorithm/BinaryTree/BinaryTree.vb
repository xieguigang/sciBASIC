#Region "Microsoft.VisualBasic::5f33772aefaeaaf7974a62a9f108616d, Microsoft.VisualBasic.Core\src\ComponentModel\Algorithm\BinaryTree\BinaryTree.vb"

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

    '   Total Lines: 101
    '    Code Lines: 77 (76.24%)
    ' Comment Lines: 9 (8.91%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (14.85%)
    '     File Size: 3.56 KB


    '     Module Extensions
    ' 
    '         Function: Find, HasKey, Max, MaxKey, Min
    '                   MinKey, TakeRange
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ComponentModel.Algorithm.BinaryTree

    <HideModuleName>
    Public Module Extensions

        ''' <summary>
        ''' 查找失败会返回空值
        ''' </summary>
        ''' <typeparam name="K"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="tree"></param>
        ''' <param name="key"></param>
        ''' <param name="compares"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Find(Of K, V)(tree As BinaryTree(Of K, V), key As K, compares As Comparison(Of K)) As BinaryTree(Of K, V)
            Do While Not tree Is Nothing
                Select Case compares(key, tree.Key)
                    Case < 0 : tree = tree.Left
                    Case > 0 : tree = tree.Right
                    Case = 0
                        Return tree
                End Select
            Loop

            Return Nothing
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function HasKey(Of K, V)(tree As BinaryTree(Of K, V), key As K, compares As Comparison(Of K)) As Boolean
            Return Not tree.Find(key, compares) Is Nothing
        End Function

        <Extension>
        Public Iterator Function TakeRange(Of K, V)(tree As BinaryTree(Of K, V), min As K, max As K, compares As Comparison(Of K)) As IEnumerable(Of Map(Of K, V))
            Do While Not tree Is Nothing
                Dim compare = (
                    min:=compares(min, tree.Key),
                    max:=compares(max, tree.Key)
                )

                If compare.min < 0 Then
                    tree = tree.Left
                ElseIf compare.min <= 0 AndAlso compare.max >= 0 Then
                    Yield New Map(Of K, V)(tree.Key, tree.Value)

                    For Each map In tree.Left.TakeRange(min, max, compares)
                        Yield map
                    Next
                    For Each map In tree.Right.TakeRange(min, max, compares)
                        Yield map
                    Next

                ElseIf compare.min > 0 OrElse compare.max > 0 Then
                    tree = tree.Right
                End If
            Loop
        End Function

        <Extension>
        Public Function Min(Of K, V)(tree As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            Do While Not tree Is Nothing
                If tree.Left Is Nothing Then
                    Return tree
                Else
                    tree = tree.Left
                End If
            Loop

            Return Nothing
        End Function

        <Extension>
        Public Function Max(Of K, V)(tree As BinaryTree(Of K, V)) As BinaryTree(Of K, V)
            Do While Not tree Is Nothing
                If tree.Right Is Nothing Then
                    Return tree
                Else
                    tree = tree.Right
                End If
            Loop

            Return Nothing
        End Function

        <Extension>
        Public Function MinKey(Of K, V)(tree As BinaryTree(Of K, V)) As K
            Dim min = tree.Min
            Return If(min Is Nothing, Nothing, min.Key)
        End Function

        <Extension>
        Public Function MaxKey(Of K, V)(tree As BinaryTree(Of K, V)) As K
            Dim max = tree.Max
            Return If(max Is Nothing, Nothing, max.Key)
        End Function
    End Module
End Namespace
