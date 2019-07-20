#Region "Microsoft.VisualBasic::9cbaca763259aab24b01598384794733, Scripting\Runtime\Extensions.vb"

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
    '         Function: CreateArray, OverloadsBinaryOperator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Scripting.Runtime

    Module Extensions

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
