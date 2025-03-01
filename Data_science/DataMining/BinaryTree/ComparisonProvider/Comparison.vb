#Region "Microsoft.VisualBasic::9f1923d445010df36473aecff91ad3eb, Data_science\DataMining\BinaryTree\ComparisonProvider\Comparison.vb"

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

'   Total Lines: 25
'    Code Lines: 17 (68.00%)
' Comment Lines: 3 (12.00%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 5 (20.00%)
'     File Size: 805 B


' Class Comparison
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: GetObject, GetSimilarity
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Matrix

''' <summary>
''' Data adapter for get comparision score from a pre-computed <see cref="DistanceMatrix"/>.
''' </summary>
Public Class Comparison : Inherits ComparisonProvider

    ReadOnly d As DistanceMatrix

    Sub New(d As DistanceMatrix, equals As Double, gt As Double)
        MyBase.New(equals, gt)
        Me.d = d
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function GetObject(id As String) As Object
        Return d.GetVector(name:=id)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function GetSimilarity(x As String, y As String) As Double
        Return d(x, y)
    End Function
End Class
