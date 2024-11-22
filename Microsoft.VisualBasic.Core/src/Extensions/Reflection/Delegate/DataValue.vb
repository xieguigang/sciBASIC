#Region "Microsoft.VisualBasic::f5bbeca7cf365c7a2b26a1faa896bdb3, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Delegate\DataValue.vb"

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

    '   Total Lines: 208
    '    Code Lines: 111 (53.37%)
    ' Comment Lines: 63 (30.29%)
    '    - Xml Docs: 49.21%
    ' 
    '   Blank Lines: 34 (16.35%)
    '     File Size: 7.88 KB


    '     Class DataObjectVector
    ' 
    '         Properties: PropertyNames, RawArray
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetProperty, GetSubVector, inspectType
    ' 
    '         Sub: SetProperty
    ' 
    '     Class DataValue
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Emit.Delegates

    ''' <summary>
    ''' .NET object collection data property value ``get/set`` helper.
    ''' </summary>
    ''' <remarks>
    ''' (将属性的<see cref="PropertyInfo.SetValue(Object, Object)"/>编译为方法调用)
    ''' </remarks>
    Public Class DataValue(Of T) : Inherits DataObjectVector

        'Public Property Evaluate(Of V)(name$) As V()
        '    Get
        '        Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))
        '        Return data _
        '            .Select(Function(x) DirectCast([property].__getValue(x), V)) _
        '            .ToArray
        '    End Get
        '    Set(ParamArray value As V())
        '        Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))

        '        If value.IsNullorEmpty Then  
        '            ' value array is nothing or have no data, 
        '            ' then means set all property value to nothing 
        '            For Each x In data
        '                Call [property].__setValue(x, Nothing)
        '            Next
        '        ElseIf value.Length = 1 Then 
        '            ' value array only have one element, 
        '            ' then means set all property value to a specific value
        '            Dim v As Object = value(Scan0)
        '            For Each x In data
        '                Call [property].__setValue(x, v)
        '            Next
        '        Else
        '            If value.Length <> data.Length Then
        '                Throw New InvalidExpressionException(DimNotAgree$)
        '            End If

        '            For i As Integer = 0 To data.Length - 1
        '                Call [property].__setValue(data(i), value(i))
        '            Next
        '        End If
        '    End Set
        'End Property

        Sub New(src As IEnumerable(Of T))
            Call MyBase.New(src.ToArray)
        End Sub

        Public Overrides Function ToString() As String
            Return type.FullName
        End Function
    End Class
End Namespace
