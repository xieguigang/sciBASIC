#Region "Microsoft.VisualBasic::72a6dcacf718ed181ca3abb4dae1c5e3, Microsoft.VisualBasic.Core\Language\BaseClass.vb"

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

    '     Interface IClassObject
    ' 
    '         Properties: Extension
    ' 
    '     Class BaseClass
    ' 
    '         Properties: Extension
    ' 
    '         Function: __toString, ReadProperty, ToString
    '         Operators: (+2 Overloads) IsFalse, (+2 Overloads) IsTrue, (+2 Overloads) Not
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Web.Script.Serialization
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language

    'Public Module ClassAPI

    '    ''' <summary>
    '    ''' Example of the extension property in VisualBasic
    '    ''' </summary>
    '    ''' <typeparam name="T"></typeparam>
    '    ''' <param name="x"></param>
    '    ''' <returns></returns>
    '    <Extension>
    '    Public Function Uid(Of T As ClassObject)(x As T) As PropertyValue(Of Long)
    '        Return PropertyValue(Of Long).Read(Of T)(x, NameOf(Uid))

    '        ' Just copy this statement without any big modification. just modify the generics type constraint.
    '        Return PropertyValue(Of Long).Read(Of T)(x, MethodBase.GetCurrentMethod)
    '    End Function
    'End Module

    ''' <summary>
    ''' The base class object in VisualBasic
    ''' </summary>
    Public Interface IClassObject

        ''' <summary>
        ''' The extension property.(为了节省内存的需要，这个附加属性尽量不要被自动初始化)
        ''' </summary>
        ''' <returns></returns>
        Property Extension As ExtendedProps
    End Interface

    ''' <summary>
    ''' The base class object in VisualBasic language
    ''' </summary>
    Public Class BaseClass : Implements IClassObject

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
            Return [GetType].GetObjectJson(Me, False)
        End Function

        ''' <summary>
        ''' String source for operator <see cref="BaseClass.Operator &(BaseClass, String)"/>
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
        Public Shared Operator &(x As BaseClass, s As String) As String
            Return x.__toString & s
        End Operator

        ''' <summary>
        ''' Contact this class object with other string value
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator &(s As String, x As BaseClass) As String
            Return s & x.__toString
        End Operator

        Public Shared Operator IsTrue(x As BaseClass) As Boolean
            Return Not x Is Nothing
        End Operator

        Public Shared Operator IsFalse(x As BaseClass) As Boolean
            Return x Is Nothing
        End Operator

        Public Shared Operator Not(x As BaseClass) As Boolean
            If x Is Nothing Then
                Return True
            Else
                Return False
            End If
        End Operator
    End Class
End Namespace
