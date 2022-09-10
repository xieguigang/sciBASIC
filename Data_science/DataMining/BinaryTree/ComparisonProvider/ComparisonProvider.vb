#Region "Microsoft.VisualBasic::d93f8025b524b76742e09d5c105d4001, sciBASIC#\Data_science\DataMining\BinaryTree\ComparisonProvider\ComparisonProvider.vb"

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

    '   Total Lines: 36
    '    Code Lines: 28
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.07 KB


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

    Sub New(equals#, gt#)
        Me.equalsDbl = equals
        Me.gt = gt
    End Sub

    Public MustOverride Function GetSimilarity(x As String, y As String) As Double

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
