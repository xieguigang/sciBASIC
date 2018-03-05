#Region "Microsoft.VisualBasic::0068bcbbef44fdf5408d25c33b401e7a, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\Protocol\Streams\ArrayAbstract.vb"

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

    '     Class ArrayAbstract
    ' 
    '         Properties: Values
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization

Namespace Net.Protocols.Streams.Array

    Public MustInherit Class ArrayAbstract(Of T) : Inherits RawStream

        <XmlAttribute("T")>
        Public Overridable Property Values As T()

        Protected ReadOnly __serialization As Func(Of T, Byte())
        Protected ReadOnly __deserialization As Func(Of Byte(), T)

        ''' <summary>
        ''' 由于这个模块是专门应用于服务器端的数据交换的模块，所以稳定性优先，
        ''' 这里面的函数都是安全的数组访问方法
        ''' </summary>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Default Public Property value(index As Integer) As T
            Get
                Return Values.ElementAtOrDefault(index)
            End Get
            Set(value As T)
                Call Values.Set(index, value)
            End Set
        End Property

        Sub New(serialize As Func(Of T, Byte()), load As Func(Of Byte(), T))
            __serialization = serialize
            __deserialization = load
        End Sub
    End Class
End Namespace
