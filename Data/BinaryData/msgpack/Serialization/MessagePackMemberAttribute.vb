#Region "Microsoft.VisualBasic::262244559ec047469eb2645a199eb3b8, Data\BinaryData\msgpack\Serialization\MessagePackMemberAttribute.vb"

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

    '   Total Lines: 28
    '    Code Lines: 19 (67.86%)
    ' Comment Lines: 3 (10.71%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (21.43%)
    '     File Size: 859 B


    '     Class MessagePackMemberAttribute
    ' 
    '         Properties: Id, NilImplication
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization

    ''' <summary>
    ''' 必须要使用这个自定义属性标记在对象属性上才会被加入序列化之中
    ''' </summary>
    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class MessagePackMemberAttribute : Inherits Attribute

        ReadOnly m_id As Integer

        Public Property NilImplication As NilImplication

        Public ReadOnly Property Id As Integer
            Get
                Return m_id
            End Get
        End Property

        Public Sub New(id As Integer)
            m_id = id
            NilImplication = NilImplication.MemberDefault
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{Id}] {NilImplication.ToString}"
        End Function
    End Class
End Namespace
