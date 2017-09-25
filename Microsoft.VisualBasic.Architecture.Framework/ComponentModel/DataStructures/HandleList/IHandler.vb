#Region "Microsoft.VisualBasic::fe2154878959cdc255860f3cb7337cd6, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\DataStructures\HandleList\IHandler.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    ''' <summary>
    ''' This object gets a object handle value to indicated that the position this object exists 
    ''' in the list collection structure. 
    ''' (这个对象具有一个用于指明该对象在列表对象中的位置的对象句柄值)
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IAddressOf : Inherits IAddress(Of Integer)
    End Interface

    ''' <summary>
    ''' This object gets a object handle value to indicated that the position this object exists 
    ''' in the list collection structure. 
    ''' (这个对象具有一个用于指明该对象在列表对象中的位置的对象句柄值)
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IAddress(Of T As IComparable)

        ''' <summary>
        ''' The ID that this object in a list instance.
        ''' (本对象在一个列表对象中的位置索引号) 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Address As T
    End Interface

    Public Interface IHashHandle : Inherits IAddressOf, INamedValue
    End Interface

    Public Class IHashValue(Of T As INamedValue) : Inherits BaseClass
        Implements IHashHandle

        Public Property obj As T
        Public Property Address As Integer Implements IAddressOf.Address
        Public Property ID As String Implements INamedValue.Key
            Get
                Return obj.Key
            End Get
            Set(value As String)
                obj.Key = value
            End Set
        End Property
    End Class

    Public Module AddressedValueExtensions

        <Extension>
        Public Function Vector(Of T As IAddress(Of Integer), TOut)(source As IEnumerable(Of T), length As Integer, getValue As Func(Of T, TOut)) As TOut()
            Dim chunk As TOut() = New TOut(length - 1) {}

            For Each x As T In source
                chunk(x.Address) = getValue(x)
            Next

            Return chunk
        End Function
    End Module
End Namespace
