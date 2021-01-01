#Region "Microsoft.VisualBasic::4c129a449557dee3f9aea51c4e14c1d3, Microsoft.VisualBasic.Core\Data\TypeCaster.vb"

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

    '     Module Extensions
    ' 
    '         Function: GetBytes, GetString, ParseObject, ToObject
    ' 
    '         Sub: Add
    ' 
    '     Interface ITypeCaster
    ' 
    '         Properties: type
    ' 
    '         Function: GetBytes, GetString, ParseObject, ToObject
    ' 
    '     Class TypeCaster
    ' 
    '         Properties: sizeOf, type
    ' 
    '     Class StringCaster
    ' 
    '         Function: GetBytes, GetString, ParseObject, ToObject
    ' 
    '     Class IntegerCaster
    ' 
    '         Function: GetBytes, GetString, ParseObject, ToObject
    ' 
    '     Class DoubleCaster
    ' 
    '         Function: GetBytes, GetString, ParseObject, ToObject
    ' 
    '     Class DateCaster
    ' 
    '         Function: GetBytes, GetString, ParseObject, ToObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.Text

Namespace ComponentModel.DataSourceModel.Caster

    <HideModuleName> Public Module Extensions

        ReadOnly typeCaster As New Dictionary(Of Type, ITypeCaster) From {
            New StringCaster, New IntegerCaster, New DoubleCaster, New DateCaster
        }

        <Extension>
        Private Sub Add(table As Dictionary(Of Type, ITypeCaster), caster As ITypeCaster)
            Call table.Add(caster.type, caster)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetBytes(type As Type) As Func(Of Object, Byte())
            Return AddressOf typeCaster(type).GetBytes
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function GetString(type As Type) As Func(Of Object, String)
            Return AddressOf typeCaster(type).GetString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ToObject(type As Type) As Func(Of Byte(), Object)
            Return AddressOf typeCaster(type).ToObject
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function ParseObject(type As Type) As Func(Of String, Object)
            Return AddressOf typeCaster(type).ParseObject
        End Function
    End Module

    Public Interface ITypeCaster

        ReadOnly Property type As Type

        Function GetBytes(value As Object) As Byte()
        Function GetString(value As Object) As String
        Function ToObject(bytes As Byte()) As Object
        Function ParseObject(str As String) As Object
    End Interface

    Public MustInherit Class TypeCaster(Of T) : Implements ITypeCaster

        Public ReadOnly Property sizeOf As Integer = Marshal.SizeOf(type)
        Public ReadOnly Property type As Type = GetType(T) Implements ITypeCaster.type

        Public MustOverride Function GetBytes(value As Object) As Byte() Implements ITypeCaster.GetBytes
        Public MustOverride Function GetString(value As Object) As String Implements ITypeCaster.GetString
        Public MustOverride Function ToObject(bytes As Byte()) As Object Implements ITypeCaster.ToObject
        Public MustOverride Function ParseObject(str As String) As Object Implements ITypeCaster.ParseObject

    End Class

    Public Class StringCaster : Inherits TypeCaster(Of String)

        ReadOnly utf8 As Encoding = Encodings.UTF8WithoutBOM.CodePage

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return utf8.GetBytes(DirectCast(value, String))
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return value
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return utf8.GetString(bytes, Scan0, bytes.Length)
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return str
        End Function
    End Class

    Public Class IntegerCaster : Inherits TypeCaster(Of Integer)

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return BitConverter.GetBytes(DirectCast(value, Integer))
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return DirectCast(value, Integer).ToString
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return BitConverter.ToInt32(bytes, Scan0)
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return Integer.Parse(str)
        End Function
    End Class

    Public Class DoubleCaster : Inherits TypeCaster(Of Double)

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return BitConverter.GetBytes(DirectCast(value, Double))
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return DirectCast(value, Double).ToString
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return BitConverter.ToDouble(bytes, Scan0)
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return Double.Parse(str)
        End Function
    End Class

    Public Class DateCaster : Inherits TypeCaster(Of Date)

        Public Overrides Function GetBytes(value As Object) As Byte()
            Return BitConverter.GetBytes(DirectCast(value, Date).ToBinary)
        End Function

        Public Overrides Function GetString(value As Object) As String
            Return DirectCast(value, Date).ToString
        End Function

        Public Overrides Function ToObject(bytes() As Byte) As Object
            Return Date.FromBinary(BitConverter.ToInt64(bytes, Scan0))
        End Function

        Public Overrides Function ParseObject(str As String) As Object
            Return Date.Parse(str)
        End Function
    End Class
End Namespace
