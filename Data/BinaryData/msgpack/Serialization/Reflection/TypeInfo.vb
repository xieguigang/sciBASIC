#Region "Microsoft.VisualBasic::b7bb24287b25a0327421f5ac2b11bff1, Data\BinaryData\msgpack\Serialization\Reflection\TypeInfo.vb"

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

    '   Total Lines: 29
    '    Code Lines: 22
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 900 B


    '     Class TypeInfo
    ' 
    '         Properties: IsGenericDictionary, IsGenericList, IsSerializableGenericCollection, Schema
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace Serialization.Reflection

    Public Class TypeInfo

        Public ReadOnly Property IsGenericList As Boolean
        Public ReadOnly Property IsGenericDictionary As Boolean
        Public ReadOnly Property Schema As Type

        Public ReadOnly Property IsSerializableGenericCollection As Boolean
            Get
                Return IsGenericList OrElse IsGenericDictionary
            End Get
        End Property

        Public Sub New(type As Type)
            IsGenericList = type.ImplementInterface(GetType(IList))
            IsGenericDictionary = type.ImplementInterface(GetType(IDictionary))
            Schema = type
        End Sub

        Public Overrides Function ToString() As String
            Return Schema.FullName
        End Function

    End Class
End Namespace
