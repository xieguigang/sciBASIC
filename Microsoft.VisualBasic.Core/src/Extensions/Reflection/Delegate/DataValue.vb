#Region "Microsoft.VisualBasic::1ee88c0b02c6e652e4309a5a25d16309, Microsoft.VisualBasic.Core\src\Extensions\Reflection\Delegate\DataValue.vb"

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

    '   Total Lines: 185
    '    Code Lines: 96 (51.89%)
    ' Comment Lines: 60 (32.43%)
    '    - Xml Docs: 46.67%
    ' 
    '   Blank Lines: 29 (15.68%)
    '     File Size: 7.10 KB


    '     Class DataObjectVector
    ' 
    '         Properties: PropertyNames, RawArray
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetProperty, inspectType
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
    ''' .net clr object vector data property ``get/set`` helper.
    ''' </summary>
    Public Class DataObjectVector

        Protected ReadOnly type As Type
        Protected ReadOnly data As Array

        ''' <summary>
        ''' Using for expression tree compile to delegate by using <see cref="BindProperty(Of T)"/>, 
        ''' to makes the get/set invoke faster
        ''' </summary>
        Protected ReadOnly properties As Dictionary(Of String, PropertyInfo)

        ''' <summary>
        ''' expose the internal clr array object to public directly.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RawArray As Array
            Get
                Return data
            End Get
        End Property

        ''' <summary>
        ''' get array of the clr property name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property PropertyNames As String()
            Get
                Return properties.Values _
                    .Select(Function(x) x.Name) _
                    .ToArray
            End Get
        End Property

        Const DimNotAgree$ = "Value array should have the same length as the target data array"

        ''' <summary>
        ''' Evaluate the clr object property, and get vector value exports in batch 
        ''' </summary>
        ''' <param name="name$">The property name, using the ``nameof`` operator to get the property name!</param>
        ''' <returns>
        ''' get an array of the property value. the array is generic.
        ''' </returns>
        Default Public Property Evaluate(name As String) As Object
            Get
                Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))
                Dim vector As Array = Array.CreateInstance([property].Type, data.Length)

                For i As Integer = 0 To data.Length - 1
                    Call vector.SetValue([property].handleGetValue(data(i)), i)
                Next

                Return vector
            End Get
            Set(value As Object)
                Call SetProperty(name, value)
            End Set
        End Property

        Private Sub SetProperty(name$, value As Object)
            Dim [property] As New BindProperty(Of DataFrameColumnAttribute)(properties(name))
            Dim array As IEnumerable

            If [property].Type Is GetType(String) AndAlso value.GetType Is GetType(String) Then
                array = Nothing
            Else
                array = TryCast(value, IEnumerable)
            End If

            If value Is Nothing Then
                For Each x In data
                    Call [property].handleSetValue(x, Nothing)
                Next
            ElseIf array Is Nothing Then  ' 不是一个集合
                Dim v As Object = value

                For Each x As Object In data
                    Call [property].handleSetValue(x, v)
                Next
            Else
                Dim vector = array.As(Of Object).ToArray

                If vector.Length <> data.Length Then
                    Throw New InvalidExpressionException(DimNotAgree$)
                End If
                For i As Integer = 0 To data.Length - 1
                    Call [property].handleSetValue(data(i), vector(i))
                Next
            End If
        End Sub

        Sub New(array As Array)
            If array Is Nothing Then
                Throw New InvalidProgramException("the input clr object vector is nothing!")
            End If

            type = array.GetType.GetElementType

            If type Is Nothing OrElse type Is GetType(Object) Then
                Throw New InvalidProgramException("the general object type array is not accepted!")
            End If

            data = array
            properties = inspectType(type)
        End Sub

        Public Function GetProperty(property$) As PropertyInfo
            Return properties([property])
        End Function

        Private Shared Function inspectType(type As Type) As Dictionary(Of String, PropertyInfo)
            Static typeCache As New Dictionary(Of Type, Dictionary(Of String, PropertyInfo))

            If Not typeCache.ContainsKey(type) Then
                SyncLock typeCache
                    typeCache(type) = type.Schema(PropertyAccess.NotSure, PublicProperty, True)
                End SyncLock
            End If

            Return typeCache(type)
        End Function
    End Class

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
