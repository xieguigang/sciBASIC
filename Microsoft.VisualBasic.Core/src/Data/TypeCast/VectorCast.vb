#Region "Microsoft.VisualBasic::a400a2012c4b1abd7d1c2ed6aa11d717, Microsoft.VisualBasic.Core\src\Data\TypeCast\VectorCast.vb"

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

    '   Total Lines: 89
    '    Code Lines: 73 (82.02%)
    ' Comment Lines: 1 (1.12%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (16.85%)
    '     File Size: 3.60 KB


    '     Module VectorCast
    ' 
    '         Function: [CType], Allocate, CastToGeneral, CastToGeneric
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace ComponentModel.DataSourceModel.TypeCast

    Public Module VectorCast

        <Extension>
        Public Function [CType](col As IEnumerable(Of Object), type As TypeCode) As Array
            Dim pull As Object() = col.SafeQuery.ToArray

            If pull.Length = 0 Then
                Return VectorCast.Allocate(type, len:=0)
            End If

            Dim schema As Type() = pull _
                .Select(Function(o) If(o Is Nothing, Nothing, o.GetType)) _
                .Where(Function(t) Not t Is Nothing) _
                .Distinct _
                .ToArray
            Dim vec As Array = VectorCast.Allocate(type, len:=pull.Length)
            Dim castTo As Func(Of Object, Object)

            If schema.Length = 1 Then
                ' is generic
                castTo = CastToGeneric(schema(0), type)
            Else
                castTo = CastToGeneral(type)
            End If

            For i As Integer = 0 To pull.Length - 1
                If Not pull(i) Is Nothing Then
                    Call vec.SetValue(castTo(pull(i)), i)
                End If
            Next

            Return vec
        End Function

        Public Function Allocate(type As TypeCode, len As Integer) As Array
            Select Case type
                Case TypeCode.Boolean : Return New Boolean(len - 1) {}
                Case TypeCode.Byte : Return New Byte(len - 1) {}
                Case TypeCode.Char : Return New Char(len - 1) {}
                Case TypeCode.DateTime : Return New Date(len - 1) {}
                Case TypeCode.DBNull, TypeCode.Empty, TypeCode.Object : Return New Object(len - 1) {}
                Case TypeCode.Decimal : Return New Decimal(len - 1) {}
                Case TypeCode.Double : Return New Double(len - 1) {}
                Case TypeCode.Int16 : Return New Int16(len - 1) {}
                Case TypeCode.Int32 : Return New Int32(len - 1) {}
                Case TypeCode.Int64 : Return New Int64(len - 1) {}
                Case TypeCode.SByte : Return New SByte(len - 1) {}
                Case TypeCode.Single : Return New Single(len - 1) {}
                Case TypeCode.String : Return New String(len - 1) {}
                Case TypeCode.UInt16 : Return New UInt16(len - 1) {}
                Case TypeCode.UInt32 : Return New UInt32(len - 1) {}
                Case TypeCode.UInt64 : Return New UInt64(len - 1) {}
                Case Else
                    Throw New NotImplementedException(type.ToString)
            End Select
        End Function

        Private Function CastToGeneric(from As Type, [to] As TypeCode) As Func(Of Object, Object)
            If [to] = TypeCode.Object OrElse
                [to] = TypeCode.Empty OrElse
                [to] = TypeCode.DBNull Then

                Return Function(o) o
            Else
                Return Converts.CTypeDynamics(from, [to])
            End If
        End Function

        Public Function CastToGeneral([to] As TypeCode) As Func(Of Object, Object)
            Dim to_type As Type = [to].CreatePrimitiveType

            If [to] = TypeCode.Object OrElse
               [to] = TypeCode.Empty OrElse
               [to] = TypeCode.DBNull Then

                Return Function(o) o
            Else
                Return Function(o) Conversion.CTypeDynamic(o, to_type)
            End If
        End Function
    End Module
End Namespace
