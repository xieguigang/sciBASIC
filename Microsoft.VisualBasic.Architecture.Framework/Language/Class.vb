#Region "Microsoft.VisualBasic::237ec1c8eb0d39cddaf73766a216d1c7, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Language\Class.vb"

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

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem

Namespace Language

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
    ''' <see cref="System.Type"/>
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class [Class](Of T) : Inherits BaseClass

        Public ReadOnly Property Type As Type
        Public ReadOnly Property Schema As Dictionary(Of BindProperty(Of Field))

        ReadOnly __enumsHandler As Func(Of IEnumerable(Of T), T())

        Default Public ReadOnly Property Field(name$) As BindProperty(Of Field)
            Get
                Return Schema(name)
            End Get
        End Property

        Sub New()
            _Type = GetType(T)
            __enumsHandler = AddressOf Enumerable.ToArray
        End Sub

        Sub New(array As Func(Of IEnumerable(Of T), T()))
            _Type = GetType(T)
            __enumsHandler = array
        End Sub

        Public Overrides Function ToString() As String
            Return Type.FullName
        End Function

        Public Shared Operator <=(cls As [Class](Of T), path As String) As List(Of T)
            Dim source As IEnumerable = CollectionIO.DefaultLoadHandle(cls.Type, path, Encoding.Default)
            Return (From x In source Select DirectCast(x, T)).AsList
        End Operator

        Public Shared Operator >=(cls As [Class](Of T), path As String) As List(Of T)
            Throw New NotSupportedException
        End Operator

        Public Shared Operator <<(cls As [Class](Of T), path As Integer) As List(Of T)
            Dim file As FileHandle = FileHandles.__getHandle(path)
            Return cls <= file.FileName
        End Operator

        ''' <summary>
        ''' ToArray
        ''' </summary>
        ''' <param name="cls"></param>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Shared Operator <=(cls As [Class](Of T), source As IEnumerable(Of T)) As T()
            Return cls.__enumsHandler(source)
        End Operator

        Public Shared Operator <=(cls As [Class](Of T), source As IEnumerable(Of IEnumerable(Of T))) As T()
            Return source.ToVector
        End Operator

        Public Shared Operator >=(cls As [Class](Of T), source As IEnumerable(Of IEnumerable(Of T))) As T()
            Throw New NotImplementedException
        End Operator

        Public Shared Operator >=(cls As [Class](Of T), source As IEnumerable(Of T)) As T()
            Throw New NotSupportedException
        End Operator

        Public Shared Function IsNullOrEmpty() As [Class](Of T)
            Return New [Class](Of T)
        End Function

        ''' <summary>
        ''' IsNullOrEmpty
        ''' </summary>
        ''' <param name="cls"></param>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Shared Operator Like(cls As [Class](Of T), source As IEnumerable(Of T)) As Boolean
            Return source.IsNullOrEmpty
        End Operator
    End Class
End Namespace
