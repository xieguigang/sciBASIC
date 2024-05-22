#Region "Microsoft.VisualBasic::27579ccb20e6051398794740ba47a9a7, Microsoft.VisualBasic.Core\src\Language\Value\var.vb"

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

    '   Total Lines: 61
    '    Code Lines: 42 (68.85%)
    ' Comment Lines: 11 (18.03%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (13.11%)
    '     File Size: 2.05 KB


    '     Class Value
    ' 
    '         Properties: IsNumeric, IsString, Name, Trace, Type
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository

Namespace Language

    ''' <summary>
    ''' Variable model in VisualBasic
    ''' </summary>
    Public Class Value : Inherits Value(Of Object)
        Implements INamedValue

        Public Property Name As String Implements IKeyedEntity(Of String).Key
        Public Property Type As Type
        ''' <summary>
        ''' 这个变量所在的函数的位置记录
        ''' </summary>
        ''' <returns></returns>
        Public Property Trace As NamedValue(Of MethodBase)

        ''' <summary>
        ''' Is a numeric type?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsNumeric As Boolean
            Get
                Dim tcode As TypeCode = Type.GetTypeCode(Type)

                If tcode = TypeCode.Byte OrElse
                    tcode = TypeCode.Decimal OrElse
                    tcode = TypeCode.Double OrElse
                    tcode = TypeCode.Int16 OrElse
                    tcode = TypeCode.Int32 OrElse
                    tcode = TypeCode.Int64 OrElse
                    tcode = TypeCode.Single OrElse
                    tcode = TypeCode.UInt16 OrElse
                    tcode = TypeCode.UInt32 OrElse
                    tcode = TypeCode.UInt64 Then

                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Public ReadOnly Property IsString As Boolean
            Get
                Return Type.Equals(GetType(String))
            End Get
        End Property

        Public Overrides Function ToString() As String
            If value Is Nothing Then
                Return Nothing
            End If
            Return value.ToString
        End Function
    End Class
End Namespace
