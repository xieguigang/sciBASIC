#Region "Microsoft.VisualBasic::5ea030c011d797daf6d7f98a1956f414, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataSource\Property\DynamicProperty.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Reflection
Imports System.Web.Script.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.DataSourceModel

    ''' <summary>
    ''' The <see cref="PropertyInfo"/> like definition of the extension property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class PropertyValue(Of T) : Inherits Value(Of T)

        ReadOnly __get As Func(Of T)
        ReadOnly __set As Action(Of T)

        ''' <summary>
        ''' The Extension property value.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Property value As T
            Get
                Return __get()
            End Get
            Set(value As T)
                MyBase.value = value

                If Not __set Is Nothing Then
                    Call __set(value)  ' 因为在初始化的时候会对这个属性赋值，但是set没有被初始化，所以会出错，在这里加了一个if判断来避免空引用的错误
                End If
            End Set
        End Property

        ''' <summary>
        ''' The instance object for this extension property
        ''' </summary>
        ''' <returns></returns>
        Public Property obj As IClassObject

        ''' <summary>
        ''' Custom property value.(value generated based on the extension property host <see cref="obj"/>)
        ''' </summary>
        ''' <param name="[get]">请勿使用<see cref="GetValue"/></param>函数，否则会出现栈空间溢出
        ''' <param name="[set]">请勿使用<see cref="SetValue"/></param>方法，否则会出现栈空间溢出
        Sub New([get] As Func(Of T), [set] As Action(Of T))
            __get = [get]
            __set = [set]
        End Sub

        ''' <summary>
        ''' Tag property value.(默认是将数据写入到基本类型的值之中)
        ''' </summary>
        Sub New()
            __get = Function() MyBase.Value
            __set = Sub(v) MyBase.Value = v
        End Sub

        ''' <summary>
        ''' 这个主要是应用于Linq表达式之中，将属性值设置之后返回宿主对象实例
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        Public Function SetValue(value As T) As IClassObject
            Call __set(value)
            Return obj
        End Function

        ''' <summary>
        ''' Property Get Value
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Overloads Shared Narrowing Operator CType(x As PropertyValue(Of T)) As T
            Return x.Value
        End Operator

        ''' <summary>
        ''' <see cref="Value"/> -> <see cref="GetObjectJson"/>
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return Value.GetJson
        End Function

        Public Shared Function GetValue(Of Cls As IClassObject)(x As Cls, name As String) As PropertyValue(Of T)
            Dim value As Object = x.Extension.DynamicHash(name)
            Dim pv As PropertyValue(Of T) = DirectCast(value, PropertyValue(Of T))
            Return pv
        End Function

        Public Shared Sub SetValue(Of Cls As IClassObject)(x As Cls, name As String, value As T)
            Dim pvo As Object = x.Extension.DynamicHash(name)
            Dim pv As PropertyValue(Of T) = DirectCast(pvo, PropertyValue(Of T))
            pv.Value = value
        End Sub

        ''' <summary>
        ''' Creates a new extension property for the target <see cref="ClassObject"/>
        ''' </summary>
        ''' <typeparam name="Cls"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Shared Function [New](Of Cls As IClassObject)(x As Cls, name As String) As PropertyValue(Of T)
            Dim value As New PropertyValue(Of T)()
            x.Extension.DynamicHash.Value(name) = value
            value.obj = x
            Return value
        End Function

        ''' <summary>
        ''' Gets the tag property value from the <see cref="ClassObject"/>.(读取<see cref="ClassObject"/>对象之中的一个拓展属性)
        ''' </summary>
        ''' <typeparam name="Cls"></typeparam>
        ''' <param name="x"></param>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Shared Function Read(Of Cls As IClassObject)(x As Cls, name As String) As PropertyValue(Of T)
            If x.Extension Is Nothing Then
                x.Extension = New ExtendedProps
            End If
            Dim prop As Object = x.Extension.DynamicHash(name)
            If prop Is Nothing Then
                prop = PropertyValue(Of T).[New](Of Cls)(x, name)
            End If
            Return DirectCast(prop, PropertyValue(Of T))
        End Function

        Public Shared Function Read(Of Cls As IClassObject)(x As Cls, pm As MethodBase) As PropertyValue(Of T)
            Return Read(Of Cls)(x, pm.Name)
        End Function
    End Class

    ''' <summary>
    ''' Abstracts for the dynamics property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface IDynamicMeta(Of T)

        ''' <summary>
        ''' Properties
        ''' </summary>
        ''' <returns></returns>
        Property Properties As Dictionary(Of String, T)
    End Interface

    ''' <summary>
    ''' Has a dictionary as a dynamics property.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public MustInherit Class DynamicPropertyBase(Of T)
        Implements IDynamicMeta(Of T)

        ''' <summary>
        ''' The dynamics property object with specific type of value.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Can not serialize the dictionary object in to xml document.</remarks>
        <XmlIgnore> Public Overridable Property Properties As Dictionary(Of String, T) Implements IDynamicMeta(Of T).Properties
            Get
                If _propHash Is Nothing Then
                    _propHash = New Dictionary(Of String, T)
                End If
                Return _propHash
            End Get
            Set(value As Dictionary(Of String, T))
                _propHash = value
            End Set
        End Property

        Dim _propHash As Dictionary(Of String, T)

        ''' <summary>
        ''' Get value by property name.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public Property Value(name$) As T
            Get
                If Properties.ContainsKey(name) Then
                    Return Properties(name)
                Else
                    Return Nothing
                End If
            End Get
            Set(value As T)
                Properties(name) = value
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return $"{Properties.Count} Property(s)."
        End Function
    End Class

    ''' <summary>
    ''' Dictionary for [<see cref="String"/>, <typeparamref name="T"/>]
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class [Property](Of T) : Inherits DynamicPropertyBase(Of T)

        Sub New()
        End Sub

        ''' <summary>
        ''' New with a init property value
        ''' </summary>
        ''' <param name="initKey"></param>
        ''' <param name="initValue"></param>
        Sub New(initKey$, initValue As T)
            Call Properties.Add(initKey, initValue)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        <ScriptIgnore> Public Iterator Property source As IEnumerable(Of NamedValue(Of T))
            Get
                For Each x In Properties
                    Yield New NamedValue(Of T) With {
                        .Name = x.Key,
                        .Value = x.Value
                    }
                Next
            End Get
            Set(value As IEnumerable(Of NamedValue(Of T)))
                Properties = value.ToDictionary(Function(x) x.Name, Function(x) x.Value)
            End Set
        End Property
    End Class
End Namespace
