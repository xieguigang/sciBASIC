#Region "Microsoft.VisualBasic::8101eec6b31ddf403ae9db1b4516c322, Microsoft.VisualBasic.Core\src\Data\TypeCast\Converts.vb"

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

    '   Total Lines: 79
    '    Code Lines: 68 (86.08%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (13.92%)
    '     File Size: 3.06 KB


    '     Module Converts
    ' 
    '         Function: CastFromDouble, CastFromInteger, CTypeDynamics
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.ValueTypes

Namespace ComponentModel.DataSourceModel.TypeCast

    Public Module Converts

        Public Function CTypeDynamics(from As Type, [to] As TypeCode) As Func(Of Object, Object)
            If from Is [to].CreatePrimitiveType Then
                Return Function(o) o
            End If

            Select Case from
                Case GetType(Double) : Return CastFromDouble([to])
                Case GetType(Integer) : Return CastFromInteger([to])
                Case Else
                    Throw New NotImplementedException(from.ToString)
            End Select
        End Function

        Private Function CastFromInteger([to] As TypeCode) As Func(Of Object, Object)
            Select Case [to]
                Case TypeCode.Boolean
                    Return Function(d) CInt(d) <> 0
                Case TypeCode.Byte,
                     TypeCode.Decimal,
                     TypeCode.Int16,
                     TypeCode.Int64,
                     TypeCode.SByte,
                     TypeCode.Single,
                     TypeCode.UInt16,
                     TypeCode.UInt32,
                     TypeCode.UInt64

                    Dim to_type As Type = [to].CreatePrimitiveType

                    Return Function(d) Conversion.CTypeDynamic(d, to_type)
                Case TypeCode.Char
                    Return Function(d) ChrW(CInt(d))
                Case TypeCode.DateTime
                    Return Function(d) DateTimeHelper.FromUnixTimeStamp(CLng(d))
                Case TypeCode.String
                    Return Function(d) d.ToString
                Case Else
                    Throw New NotImplementedException([to].ToString)
            End Select
        End Function

        Private Function CastFromDouble([to] As TypeCode) As Func(Of Object, Object)
            Select Case [to]
                Case TypeCode.Boolean
                    Return Function(d) CDbl(d) <> 0
                Case TypeCode.Byte,
                     TypeCode.Decimal,
                     TypeCode.Int16,
                     TypeCode.Int32,
                     TypeCode.Int64,
                     TypeCode.SByte,
                     TypeCode.Single,
                     TypeCode.UInt16,
                     TypeCode.UInt32,
                     TypeCode.UInt64

                    Dim to_type As Type = [to].CreatePrimitiveType

                    Return Function(d) Conversion.CTypeDynamic(d, to_type)
                Case TypeCode.Char
                    Return Function(d) ChrW(CInt(d))
                Case TypeCode.DateTime
                    Return Function(d) DateTimeHelper.FromUnixTimeStamp(CLng(d))
                Case TypeCode.String
                    Return Function(d) d.ToString
                Case Else
                    Throw New NotImplementedException([to].ToString)
            End Select
        End Function
    End Module
End Namespace
