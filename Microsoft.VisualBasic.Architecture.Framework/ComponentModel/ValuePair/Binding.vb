#Region "Microsoft.VisualBasic::db6af0fd295df87d465dddf59e6b4d78, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\ValuePair\Binding.vb"

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
    ''' Functioning the same as the <see cref="KeyValuePair(Of T, K)"/>, but with more specific on the name. 
    ''' <see cref="KeyValuePair(Of T, K)"/> its name is too generic.
    ''' (作用与<see cref="KeyValuePair(Of T, K)"/>类似，只不过类型的名称更加符合绑定的描述)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="K"></typeparam>
    Public Structure Binding(Of T, K)

        Dim Bind As T
        Dim Target As K

        ''' <summary>
        ''' If the field <see cref="Bind"/> and <see cref="Target"/> are both nothing, then this binding is empty.
        ''' (当<see cref="Bind"/>以及<see cref="Target"/>都同时为空值的时候这个参数才会为真)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return Bind Is Nothing AndAlso Target Is Nothing
            End Get
        End Property

        Sub New(source As T, target As K)
            Me.Bind = source
            Me.Target = target
        End Sub

        Public Overrides Function ToString() As String
            If IsEmpty Then
                Return "No binding"
            Else
                Return Bind.ToString & " --> " & Target.ToString
            End If
        End Function

        ''' <summary>
        ''' Convert this binding to tuple
        ''' </summary>
        ''' <returns></returns>
        Public Function Tuple() As Tuple(Of T, K)
            Return Me
        End Function

        ''' <summary>
        ''' Implicit convert this binding as the <see cref="System.Tuple(Of T, K)"/>
        ''' </summary>
        ''' <param name="b"></param>
        ''' <returns></returns>
        Public Shared Narrowing Operator CType(b As Binding(Of T, K)) As Tuple(Of T, K)
            Return New Tuple(Of T, K)(b.Bind, b.Target)
        End Operator
    End Structure
End Namespace
