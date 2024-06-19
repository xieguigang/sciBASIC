#Region "Microsoft.VisualBasic::01cf1e22edce052ce6778ff0ff15ca07, Microsoft.VisualBasic.Core\src\Extensions\Math\NumberEqualityComparer.vb"

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

'   Total Lines: 50
'    Code Lines: 29 (58.00%)
' Comment Lines: 11 (22.00%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 10 (20.00%)
'     File Size: 1.70 KB


'     Class NumberEqualityComparer
' 
'         Properties: DeltaTolerance
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: (+2 Overloads) Equals, GetHashCode, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports std = System.Math

Namespace Math

    ''' <summary>
    ''' 当两个数的误差值的绝对值小于阈值的时候认为两个数字相等
    ''' </summary>
    Public Class NumberEqualityComparer : Implements IEqualityComparer(Of Double)

        ''' <summary>
        ''' the threshold value
        ''' </summary>
        ''' <returns></returns>
        Public Property DeltaTolerance As Double

        ''' <summary>
        ''' the threshold value
        ''' </summary>
        ''' <param name="tolerance"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(tolerance As Double)
            DeltaTolerance = tolerance
        End Sub

        Sub New()
            Call Me.New(0.00001)
        End Sub

        Public Overrides Function ToString() As String
            Return $"|a-b| <= {DeltaTolerance}"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Equals() As GenericLambda(Of Double).IEquals
            Return AddressOf Equals
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Overloads Function Equals(x As Double, y As Double) As Boolean Implements IEqualityComparer(Of Double).Equals
            Return std.Abs(x - y) <= _DeltaTolerance
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function GetHashCode(obj As Double) As Integer Implements IEqualityComparer(Of Double).GetHashCode
            Return obj.GetHashCode
        End Function
    End Class

End Namespace
