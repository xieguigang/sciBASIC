#Region "Microsoft.VisualBasic::1554b6bdaa5a8f8f0f733a8ab6163986, Data\DataFrame\StorageProvider\ComponntModels\ProviderBase.vb"

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

    '     Class StorageProvider
    ' 
    '         Properties: BindProperty, CanReadDataFromObject, CanWriteDataToObject, IsMetaField, LoadMethod
    '                     Ordinal
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: GetValue, ToString
    ' 
    '         Sub: SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Scripting

Namespace StorageProvider.ComponentModels

    ''' <summary>
    ''' The base type of the data I/O object schema.
    ''' (数据读写对象的基本类型)
    ''' </summary>
    Public MustInherit Class StorageProvider

        ''' <summary>
        ''' The column index of this column in the csv table.
        ''' (这个属性值在Csv文件的第几列？)
        ''' </summary>
        ''' <returns></returns>
        Public Property Ordinal As Integer
        ''' <summary>
        ''' The bind property in the reflected class object.
        ''' (在反射的类型定义之中所绑定的属性入口定义)
        ''' </summary>
        ''' <returns></returns>
        Public Property BindProperty As PropertyInfo

        ''' <summary>
        ''' If the target property didn't provides the column name by 
        ''' using custom attribute, then this property will returns 
        ''' the Class propertyName from <see cref="PropertyInfo"/>.
        ''' (假若目标属性之中没有提供名称的话，则会使用属性名称来代替)
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property Name As String

        ''' <summary>
        ''' 从目标类型对象之中可以读取这个属性的值将数据写入到文件之中
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CanReadDataFromObject As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return BindProperty.CanRead
            End Get
        End Property

        ''' <summary>
        ''' 可以在读取Csv文件之中的数据之后将数据写入到这个属性之中从而将数据加载进入内存之中
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CanWriteDataToObject As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return BindProperty.CanWrite
            End Get
        End Property

        Public MustOverride ReadOnly Property ProviderId As Reflection.ProviderIds

        ''' <summary>
        ''' 解析字符串为.NET对象的方法
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LoadMethod As Func(Of String, Object)

        Public ReadOnly Property IsMetaField As Boolean
            Get
                Dim hasAttribute As Boolean = Not BindProperty _
                    .GetCustomAttributes(GetType(Reflection.MetaAttribute), inherit:=True) _
                    .IsNullOrEmpty

                Return hasAttribute OrElse Not BindProperty.PropertyType.GetMetaAttribute Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' By using this function that save the property value as a cell value string.
        ''' (将.NET对象序列化为csv单元格之中的一个字符串值的方法) 
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public MustOverride Overloads Function ToString(obj As Object) As String

        ''' <summary>
        ''' 从目标实例之中读取属性数据然后转换为字符串
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function GetValue(obj As Object) As String
            Dim value As Object = BindProperty.GetValue(obj)
            Return ToString(value)
        End Function

        ''' <summary>
        ''' <see cref="PropertyInfo.SetValue(Object, Object)"/>
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="value"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetValue(obj As Object, value As Object)
            Call _BindProperty.SetValue(obj, value, Nothing)
        End Sub

        ''' <summary>
        ''' Creates the object model from target property definition.
        ''' </summary>
        ''' <param name="BindProperty"></param>
        Sub New(BindProperty As PropertyInfo)
            Call Me.New(BindProperty, BindProperty.PropertyType)
        End Sub

        Sub New(BindProperty As PropertyInfo, ElementType As Type)
            Me.BindProperty = BindProperty

            If CasterString.ContainsKey(ElementType) Then
                Me.LoadMethod = AddressOf CasterString(ElementType).Invoke
            Else
                ' Meta 字典类型，则忽略掉
            End If
        End Sub

        Sub New(bindProperty As PropertyInfo, loadMethod As Func(Of String, Object))
            Me.BindProperty = bindProperty
            Me.LoadMethod = loadMethod
        End Sub

        ''' <summary>
        ''' VB style definition string
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Dim vbDim$ = $"[Dim {Name} As {BindProperty.PropertyType.FullName}]"
            Return vbDim & $" //{Me.GetType.Name} --> {BindProperty.Name}"
        End Function
    End Class
End Namespace
