#Region "Microsoft.VisualBasic::3905fd2d12302516e706bd7a632aafa5, Data_science\DataMining\BinaryTree\ComparisonProvider\ComparisonProvider.vb"

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

    '   Total Lines: 48
    '    Code Lines: 29 (60.42%)
    ' Comment Lines: 11 (22.92%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (16.67%)
    '     File Size: 1.54 KB


    ' Class ComparisonProvider
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: Compares, GetComparer
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Public MustInherit Class ComparisonProvider

    Protected ReadOnly equalsDbl As Double
    Protected ReadOnly gt As Double

    ''' <summary>
    ''' create a new score generator
    ''' </summary>
    ''' <param name="equals">score level for construct a binary tree cluster</param>
    ''' <param name="gt">score level for create a binary tree branch</param>
    Sub New(equals#, gt#)
        Me.equalsDbl = equals
        Me.gt = gt
    End Sub

    Public MustOverride Function GetSimilarity(x As String, y As String) As Double
    Public MustOverride Function GetObject(id As String) As Object

    ''' <summary>
    ''' binary tree generator
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <returns></returns>
    Public Function Compares(x As String, y As String) As Integer
        Dim similarity As Double = GetSimilarity(x, y)

        If similarity >= equalsDbl Then
            Return 0
        ElseIf similarity >= gt Then
            Return 1
        Else
            Return -1
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetComparer() As Comparison(Of String)
        Return Me
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Narrowing Operator CType(c As ComparisonProvider) As Comparison(Of String)
        Return New Comparison(Of String)(AddressOf c.Compares)
    End Operator
End Class
