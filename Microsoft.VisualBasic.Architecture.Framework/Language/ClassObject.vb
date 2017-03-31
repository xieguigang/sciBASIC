#Region "Microsoft.VisualBasic::8aa77d16354696a534331686c7ae3023, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\ClassObject.vb"

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
Imports System.Runtime.CompilerServices
Imports System.Web.Script.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language

    Public Module ClassAPI

        ''' <summary>
        ''' Example of the extension property in VisualBasic
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Uid(Of T As ClassObject)(x As T) As PropertyValue(Of Long)
            Return PropertyValue(Of Long).Read(Of T)(x, NameOf(Uid))

            ' Just copy this statement without any big modification. just modify the generics type constraint.
            Return PropertyValue(Of Long).Read(Of T)(x, MethodBase.GetCurrentMethod)
        End Function
    End Module

    Public Interface IMySelf(Of T As IMySelf(Of T))

        ''' <summary>
        ''' With keyword in VisualBasic
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property MySelf As T
    End Interface

    ''' <summary>
    ''' The base class object in VisualBasic
    ''' </summary>
    Public Class ClassObject
        Implements IClassObject, IMySelf(Of ClassObject)

        ''' <summary>
        ''' The extension property.(为了节省内存的需要，这个附加属性尽量不要被自动初始化)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' Dummy field for solve the problem of xml serialization >>>simpleContent&lt;&lt;&lt;   
        ''' 
        ''' http://stackoverflow.com/questions/2501466/xmltext-attribute-in-base-class-breakes-serialization
        ''' 
        ''' So I think you could make it work by adding a dummy property or field that you never 
        ''' use in the ``LookupItem`` class. 
        ''' If you're never assign a value to it, it will remain null and will not be serialized, 
        ''' but it will prevent your class from being treated as simpleContent. I know it's a 
        ''' dirty workaround, but I see no other easy way...
        ''' </remarks>
        <XmlIgnore>
        <ScriptIgnore>
        <SoapIgnore>
        Public Overridable Property Extension As ExtendedProps Implements IClassObject.Extension

        Public Overridable ReadOnly Property this As ClassObject Implements IMySelf(Of ClassObject).MySelf
            Get
                Return Me
            End Get
        End Property

        ''' <summary>
        ''' Get dynamics property value.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function ReadProperty(Of T)(name As String) As PropertyValue(Of T)
            Return PropertyValue(Of T).Read(Me, name)
        End Function

        ''' <summary>
        ''' Default is display the json value of the object class
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return JsonContract.GetObjectJson(Me, [GetType], False)
        End Function

        ''' <summary>
        ''' String source for operator <see cref="ClassObject.Operator &(ClassObject, String)"/>
        ''' </summary>
        ''' <returns>Default is using <see cref="ToString"/> method as provider</returns>
        Protected Friend Overridable Function __toString() As String
            Return ToString()
        End Function

        ''' <summary>
        ''' Contact this class object with other string value
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Shared Operator &(x As ClassObject, s As String) As String
            Return x.__toString & s
        End Operator

        ''' <summary>
        ''' Contact this class object with other string value
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator &(s As String, x As ClassObject) As String
            Return s & x.__toString
        End Operator

        Protected Sub Copy(ByRef x As ClassObject)
            x = Me
        End Sub
    End Class
End Namespace
