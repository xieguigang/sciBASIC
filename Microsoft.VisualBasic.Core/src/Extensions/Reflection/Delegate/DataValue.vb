#Region "Microsoft.VisualBasic::a83a684941618983dd929c05f90f9428, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Delegate\DataValue.vb"

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

    '   Total Lines: 66
    '    Code Lines: 43 (65.15%)
    ' Comment Lines: 11 (16.67%)
    '    - Xml Docs: 54.55%
    ' 
    '   Blank Lines: 12 (18.18%)
    '     File Size: 2.63 KB


    '     Class DataValue
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetVector, ToString
    ' 
    '         Sub: (+2 Overloads) SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Namespace Emit.Delegates

    ''' <summary>
    ''' .NET object collection data property value ``get/set`` helper.
    ''' </summary>
    ''' <remarks>
    ''' (将属性的<see cref="PropertyInfo.SetValue(Object, Object)"/>编译为方法调用)
    ''' </remarks>
    Public Class DataValue(Of T) : Inherits DataObjectVector

        Sub New(src As IEnumerable(Of T))
            Call MyBase.New(src.ToArray)
        End Sub

        Public Function GetVector(Of V)(name As String) As V()
            Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))
            Dim pull As V() = New V(data.Length - 1) {}

            For i As Integer = 0 To pull.Length - 1
                pull(i) = DirectCast([property].GetValue(data(i)), V)
            Next

            Return pull
        End Function

        Public Sub SetValue(Of V)(name As String, value As V)
            Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))

            For i As Integer = 0 To data.Length - 1
                Call [property].SetValue(data(i), value)
            Next
        End Sub

        Public Sub SetValue(Of V)(name$, vector As V())
            If vector.IsNullOrEmpty Then
                ' 20241125
                ' value array is nothing or have no data, 
                ' then means set all property value to nothing 
                Call SetValue(Of V)(name, value:=Nothing)
            ElseIf vector.Length = 1 Then
                ' value array only have one element, 
                ' then means set all property value to a specific value
                Call SetValue(name, value:=vector(Scan0))
            Else
                Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))

                If vector.Length <> data.Length Then
                    Throw New InvalidExpressionException($"the dimension size of the given vector value({vector.Length}) is not equals to the dimension size of the base data vector({data.Length})!")
                End If

                For i As Integer = 0 To data.Length - 1
                    Call [property].SetValue(data(i), vector(i))
                Next
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return type.FullName
        End Function
    End Class
End Namespace
