#Region "Microsoft.VisualBasic::bc7a481cef6f09241ec37d49eea1fa90, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\ValuePair\Binding.vb"

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

Namespace ComponentModel

    ''' <summary>
    ''' 作用与<see cref="KeyValuePair(Of T, K)"/>类似，只不过类型的名称更加符合绑定的描述
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="K"></typeparam>
    Public Structure Binding(Of T, K)

        Dim Bind As T
        Dim Target As K

        ''' <summary>
        ''' 当<see cref="Bind"/>以及<see cref="Target"/>都同时为空值的时候这个参数才会为真
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Bind Is Nothing AndAlso Target Is Nothing
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Bind.ToString & " --> " & Target.ToString
        End Function
    End Structure
End Namespace
