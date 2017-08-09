#Region "Microsoft.VisualBasic::76baf6366154f50c01f84f1cd42bded5, ..\sciBASIC#\Data_science\DataMining\Microsoft.VisualBasic.DataMining.Framework\ComponentModel\Entity.vb"

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

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    ''' <summary>
    '''
    ''' </summary>
    ''' <typeparam name="T">只允许数值类型</typeparam>
    Public MustInherit Class EntityBase(Of T) : Inherits ClassObject

        <XmlAttribute("T")>
        Public Overridable Property Properties As T()

        Default Public Property Item(i%) As T
            Get
                Return Properties(i)
            End Get
            Set(value As T)
                Properties(i) = value
            End Set
        End Property

        Public Overridable ReadOnly Property Length As Integer
            Get
                Return Properties.Length
            End Get
        End Property
    End Class

    ''' <summary>
    ''' {Properties} -> Class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Entity : Inherits EntityBase(Of Integer)

        <XmlAttribute> Public Property [Class] As Integer

        Public Overrides Function ToString() As String
            Return $"<{String.Join("; ", Properties)}> --> {[Class]}"
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="row">第一个元素为分类，其余元素为属性</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CastTo(row As RowObject) As Entity
            Dim LQuery = From s As String
                         In row.Skip(1)
                         Select CType(Val(s), Integer) '

            Return New Entity With {
                .Class = Val(row.First),
                .Properties = LQuery.ToArray
            }
        End Function

        Default Public Overloads ReadOnly Property Item(Index As Integer) As Integer
            Get
                Return Properties(Index)
            End Get
        End Property

        Public Shared Widening Operator CType(properties As Double()) As Entity
            Return New Entity With {
                .Properties = (From x In properties Select CType(x, Integer)).ToArray
            }
        End Operator

        Public Shared Widening Operator CType(properties As Integer()) As Entity
            Return New Entity With {
                .Properties = properties
            }
        End Operator
    End Class
End Namespace
