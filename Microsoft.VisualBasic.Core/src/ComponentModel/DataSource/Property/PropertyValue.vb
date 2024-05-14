#Region "Microsoft.VisualBasic::b5834ea2d7b39560dd5d476170c1be22, Microsoft.VisualBasic.Core\src\ComponentModel\DataSource\Property\PropertyValue.vb"

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

    '   Total Lines: 142
    '    Code Lines: 74
    ' Comment Lines: 51
    '   Blank Lines: 17
    '     File Size: 5.67 KB


    '     Class PropertyValue
    ' 
    '         Properties: obj, value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [New], GetValue, (+3 Overloads) Read, SetValue, ToString
    ' 
    '         Sub: SetValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' The <see cref="PropertyInfo"/> like definition of the extension property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class PropertyValue(Of T) : Inherits Value(Of T)

        ReadOnly handleGet As Func(Of T)
        ReadOnly handleSet As Action(Of T)

        ''' <summary>
        ''' The Extension property value.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Property value As T
            Get
                Return handleGet()
            End Get
            Set(value As T)
                MyBase.Value = value

                If Not handleSet Is Nothing Then
                    ' 因为在初始化的时候会对这个属性赋值，但是set没有被初始化，
                    ' 所以会出错， 在这里加了一个if判断来避免空引用的错误
                    Call handleSet(value)
                End If
            End Set
        End Property

        ''' <summary>
        ''' The instance object for this extension property
        ''' </summary>
        ''' <returns></returns>
        Public Property obj As DynamicPropertyBase(Of T)

        ''' <summary>
        ''' Custom property value.(value generated based on the extension property host <see cref="obj"/>)
        ''' </summary>
        ''' <param name="[get]">请勿使用<see cref="GetValue"/></param>函数，否则会出现栈空间溢出
        ''' <param name="[set]">请勿使用<see cref="SetValue"/></param>方法，否则会出现栈空间溢出
        Sub New([get] As Func(Of T), [set] As Action(Of T))
            handleGet = [get]
            handleSet = [set]
        End Sub

        ''' <summary>
        ''' Tag property value.(默认是将数据写入到基本类型的值之中)
        ''' </summary>
        Sub New()
            handleGet = Function() MyBase.Value
            handleSet = Sub(v) MyBase.Value = v
        End Sub

        ''' <summary>
        ''' 这个主要是应用于Linq表达式之中，将属性值设置之后返回宿主对象实例
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function SetValue(value As T) As DynamicPropertyBase(Of T)
            Call handleSet(value)
            Return obj
        End Function

        ''' <summary>
        ''' Property Get Value
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overloads Shared Narrowing Operator CType(x As PropertyValue(Of T)) As T
            Return x.value
        End Operator

        ''' <summary>
        ''' <see cref="Value"/> -> <see cref="GetObjectJson"/>
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return value.GetJson
        End Function

        Public Shared Function GetValue(Of Cls As DynamicPropertyBase(Of T))(x As Cls, name As String) As PropertyValue(Of T)
            Dim value As Object = x(name)
            Dim pv As PropertyValue(Of T) = DirectCast(value, PropertyValue(Of T))
            Return pv
        End Function

        Public Shared Sub SetValue(Of Cls As DynamicPropertyBase(Of T))(x As Cls, name As String, value As T)
            Dim pvo As Object = x(name)
            Dim pv As PropertyValue(Of T) = DirectCast(pvo, PropertyValue(Of T))
            pv.value = value
        End Sub

        ''' <summary>
        ''' Creates a new extension property for the target <see cref="DynamicPropertyBase(Of T)"/>
        ''' </summary>
        ''' <typeparam name="Cls"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Shared Function [New](Of Cls As DynamicPropertyBase(Of T))(x As Cls, name As String) As PropertyValue(Of T)
            Dim value As New PropertyValue(Of T)()
            x(name) = value
            value.obj = x
            Return value
        End Function

        ''' <summary>
        ''' Gets the tag property value from the <see cref="DynamicPropertyBase(Of T)"/>.
        ''' (读取<see cref="DynamicPropertyBase(Of T)"/>对象之中的一个拓展属性)
        ''' </summary>
        ''' <typeparam name="Cls"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Shared Function Read(Of Cls As DynamicPropertyBase(Of T))(x As Cls, name As String) As PropertyValue(Of T)
            Dim prop As Object = x(name)
            If prop Is Nothing Then
                prop = PropertyValue(Of T).[New](Of Cls)(x, name)
            End If
            Return DirectCast(prop, PropertyValue(Of T))
        End Function

        Public Shared Function Read(Of Cls As DynamicPropertyBase(Of T), V)(x As Cls, name As String) As PropertyValue(Of V)
            Dim prop As Object = x(name)
            If prop Is Nothing Then
                prop = New [PropertyValue](Of V)
                x(name) = prop
                prop.obj = x
            End If
            Return DirectCast(prop, PropertyValue(Of V))
        End Function

        Public Shared Function Read(Of Cls As DynamicPropertyBase(Of T))(x As Cls, pm As MethodBase) As PropertyValue(Of T)
            Return Read(Of Cls)(x, pm.Name)
        End Function
    End Class
End Namespace
