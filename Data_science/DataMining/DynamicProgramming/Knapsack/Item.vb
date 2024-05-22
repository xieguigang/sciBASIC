#Region "Microsoft.VisualBasic::de902ae2bbe00312b9c74e03b83a78d3, Data_science\DataMining\DynamicProgramming\Knapsack\Item.vb"

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

    '   Total Lines: 22
    '    Code Lines: 17 (77.27%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (22.73%)
    '     File Size: 668 B


    '     Class Item
    ' 
    '         Properties: Name, Value, Weight
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Knapsack

    Public Class Item

        Public Property Name As String
        Public Property Value As Integer
        Public Property Weight As Integer

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(name As String, value As Integer, weight As Integer)
            Me.Name = name
            Me.Value = value
            Me.Weight = weight
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format(vbLf & "    {0} - weight: {1}, value: {2}", Name, Weight, Value)
        End Function
    End Class
End Namespace
