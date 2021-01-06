#Region "Microsoft.VisualBasic::5e61b127bee3782c78f65f984f0df102, Microsoft.VisualBasic.Core\src\Scripting\Runtime\Extensions.vb"

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
    '         Properties: Numerics
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateArray, CreatePrimitiveType, OverloadsBinaryOperator, PrimitiveTypeCode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Scripting.Runtime

    Public Module Extensions

        Public ReadOnly Property Numerics As Index(Of TypeCode)

        Sub New()
            Numerics = {
                TypeCode.Byte,
                TypeCode.Decimal,
                TypeCode.Double,
                TypeCode.Int16,
                TypeCode.Int32,
                TypeCode.Int64,
                TypeCode.SByte,
                TypeCode.Single,
                TypeCode.UInt16,
                TypeCode.UInt32,
                TypeCode.UInt64
            }.Indexing
        End Sub

        ''' <summary>
        ''' ``<see cref="TypeCode"/> -> <see cref="Type"/>``
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CreatePrimitiveType(type As TypeCode) As Type
            Select Case type
                Case TypeCode.Boolean : Return GetType(Boolean)
                Case TypeCode.Byte : Return GetType(Byte)
                Case TypeCode.Char : Return GetType(Char)
                Case TypeCode.DateTime : Return GetType(DateTime)
                Case TypeCode.DBNull : Return GetType(Void)
                Case TypeCode.Decimal : Return GetType(Decimal)
                Case TypeCode.Double : Return GetType(Double)
                Case TypeCode.Int16 : Return GetType(Int16)
                Case TypeCode.Int32 : Return GetType(Int32)
                Case TypeCode.Int64 : Return GetType(Int64)
                Case TypeCode.Object : Return GetType(Object)
                Case TypeCode.SByte : Return GetType(SByte)
                Case TypeCode.Single : Return GetType(Single)
                Case TypeCode.String : Return GetType(String)
                Case TypeCode.UInt16 : Return GetType(UInt16)
                Case TypeCode.UInt32 : Return GetType(UInt32)
                Case TypeCode.UInt64 : Return GetType(UInt64)
                Case Else
                    Return Nothing
            End Select
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function PrimitiveTypeCode(type As Type) As TypeCode
            Return Type.GetTypeCode(type)
        End Function

        <Extension>
        Public Function OverloadsBinaryOperator(methods As IEnumerable(Of MethodInfo)) As BinaryOperator
            Return BinaryOperator.CreateOperator(methods?.ToArray)
        End Function

        <Extension>
        Public Function CreateArray(data As IEnumerable, type As Type) As Object
            Dim src = data.Cast(Of Object).ToArray
            Dim array As Array = Array.CreateInstance(type, src.Length)

            For i As Integer = 0 To src.Length - 1
                array.SetValue(src(i), i)
            Next

            Return array
        End Function
    End Module
End Namespace
