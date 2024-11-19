#Region "Microsoft.VisualBasic::bb12e61682e08fe0add8fb9407a9e167, Microsoft.VisualBasic.Core\src\ComponentModel\ValuePair\TagData\FactorValue.vb"

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

'   Total Lines: 42
'    Code Lines: 26 (61.90%)
' Comment Lines: 8 (19.05%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 8 (19.05%)
'     File Size: 1.45 KB


'     Class FactorValue
' 
'         Properties: factor, result
' 
'         Function: Create
' 
'     Class FactorString
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.TagData

    Public Class FactorValue(Of T As {Structure, IComparable(Of T)}, V)

        ''' <summary>
        ''' should be a numeric factor value
        ''' </summary>
        ''' <returns></returns>
        Public Property factor As T
        Public Property result As V

#If NET_48 Or NETCOREAPP Then

        ''' <summary>
        ''' Make conversion from a tuple object
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(value As (factor As T, result As V)) As FactorValue(Of T, V)
            Return New FactorValue(Of T, V) With {
                .factor = value.factor,
                .result = value.result
            }
        End Operator
#End If

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(value As FactorValue(Of T, V)) As KeyValuePair(Of T, V)
            Return New KeyValuePair(Of T, V)(value.factor, value.result)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Create(factor As T, result As V) As FactorValue(Of T, V)
            Return New FactorValue(Of T, V) With {
                .factor = factor,
                .result = result
            }
        End Function
    End Class

    ''' <summary>
    ''' target string label tagged with a numeric factor
    ''' </summary>
    ''' <typeparam name="T">should be a numeric factor type, example as double, single, etc</typeparam>
    Public Class FactorString(Of T As {Structure, IComparable(Of T)}) : Inherits FactorValue(Of T, String)

        Public Overrides Function ToString() As String
            Return $"Dim {result} As {GetType(T).FullName} = {factor.GetJson}"
        End Function
    End Class
End Namespace
