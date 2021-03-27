#Region "Microsoft.VisualBasic::4ada7b8a809323e160746d9883d0d5ec, Data\BinaryData\BinaryData\XDR\EnumHelper.vb"

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

    '     Class EnumHelper
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EnumToInt, IntToEnum
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Xdr

    Public Class EnumHelper(Of T As Structure)

        Private Shared ReadOnly _enumMap As Dictionary(Of T, Integer)
        Private Shared ReadOnly _intMap As Dictionary(Of Integer, T)

        Shared Sub New()
            Dim underType As Type = GetType(T).GetEnumUnderlyingType()
            Dim conv As Func(Of T, Integer)

            If underType Is GetType(Byte) Then
                conv = Function(item) CByte(CType(item, ValueType))
            ElseIf underType Is GetType(SByte) Then
                conv = Function(item) CSByte(CType(item, ValueType))
            ElseIf underType Is GetType(Short) Then
                conv = Function(item) CShort(CType(item, ValueType))
            ElseIf underType Is GetType(UShort) Then
                conv = Function(item) CUShort(CType(item, ValueType))
            ElseIf underType Is GetType(Integer) Then
                conv = Function(item) CType(item, ValueType)
            Else
                Throw New NotSupportedException(String.Format("unsupported type {0}", GetType(T).FullName))
            End If

            _intMap = New Dictionary(Of Integer, T)()
            _enumMap = New Dictionary(Of T, Integer)()

            For Each item In [Enum].GetValues(GetType(T)).Cast(Of T)()
                Dim exist As T = Nothing
                Dim key = conv(item)
                If Not _intMap.TryGetValue(key, exist) Then _intMap.Add(key, item)
                If Not _enumMap.TryGetValue(item, key) Then _enumMap.Add(item, conv(item))
            Next
        End Sub

        Public Shared Function IntToEnum(val As Integer) As T
            Dim exist As T = Nothing
            If _intMap.TryGetValue(val, exist) Then Return exist
            Throw New InvalidCastException(String.Format("type `{0}' not contain {1}", GetType(T).FullName, val))
        End Function

        Public Shared Function EnumToInt(item As T) As Integer
            Dim val As Integer
            If _enumMap.TryGetValue(item, val) Then Return val
            Throw New InvalidCastException(String.Format("enum {0} not contain value {1}", GetType(T).FullName, item))
        End Function
    End Class
End Namespace

