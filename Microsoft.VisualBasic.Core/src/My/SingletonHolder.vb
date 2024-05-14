#Region "Microsoft.VisualBasic::de045038e7dbdd5390bce387149e0ccc, Microsoft.VisualBasic.Core\src\My\SingletonHolder.vb"

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

    '   Total Lines: 121
    '    Code Lines: 55
    ' Comment Lines: 45
    '   Blank Lines: 21
    '     File Size: 3.96 KB


    '     Class SingletonHolder
    ' 
    '         Properties: Instance
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class SingletonList
    ' 
    '         Function: ForEach
    ' 
    '         Sub: (+2 Overloads) Add, Clear
    ' 
    '     Class SharedObject
    ' 
    '         Properties: Instance
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class SharedObject
    ' 
    '         Properties: GetObject
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: SetObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'! 
'@file SingletonHolder.cs
'@author Woong Gyu La a.k.a Chris. <juhgiyo@gmail.com>
'		<http://github.com/juhgiyo/epForceDirectedGraph.cs>
'@date September 27, 2013
'@brief SingletonHolder Interface
'@version 2.0
'
'@section LICENSE
'
'The MIT License (MIT)
'
'Copyright (c) 2013 Woong Gyu La <juhgiyo@gmail.com>
'
'Permission is hereby granted, free of charge, to any person obtaining a copy
'of this software and associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
'copies of the Software, and to permit persons to whom the Software is
'furnished to do so, subject to the following conditions:
'
'The above copyright notice and this permission notice shall be included in
'all copies or substantial portions of the Software.
'
'THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
'IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
'OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
'THE SOFTWARE.
'
'@section DESCRIPTION
'
'An Interface for the SingletonHolder Class.
'
'

Imports System.Runtime.CompilerServices

Namespace My

    ''' <summary>
    ''' An Interface for the SingletonHolder Class.(存储单体模式的对象实例)
    ''' </summary>
    ''' <typeparam name="T">泛型T必须是含有一个无参数的构造函数的</typeparam>
    Public NotInheritable Class SingletonHolder(Of T As New)

        Shared _instance As T

        ''' <summary>
        ''' 目标类型的唯一单个实例
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Instance() As T
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                If _instance Is Nothing Then
                    _instance = New T()
                End If
                Return _instance
            End Get
        End Property

        Private Sub New()
        End Sub
    End Class

    Public NotInheritable Class SingletonList(Of T)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub Add(item As T)
            Call SingletonHolder(Of List(Of T)).Instance.Add(item)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Sub Add(items As IEnumerable(Of T))
            Call SingletonHolder(Of List(Of T)).Instance.AddRange(items)
        End Sub

        Public Shared Sub Clear()
            Call SingletonHolder(Of List(Of T)).Instance.Clear()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ForEach() As IEnumerable(Of T)
            Return SingletonHolder(Of List(Of T)).Instance
        End Function
    End Class

    Public NotInheritable Class SharedObject(Of T)

        Public Shared Property Instance As T

        Private Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return Instance.ToString
        End Function

    End Class

    Public NotInheritable Class SharedObject

        Shared ReadOnly instances As New Dictionary(Of String, Object)

        Public Shared ReadOnly Property GetObject(reference As String) As Object
            Get
                Return instances.TryGetValue(reference)
            End Get
        End Property

        Private Sub New()
        End Sub

        Public Shared Sub SetObject(guid As String, obj As Object)
            instances(guid) = obj
        End Sub
    End Class
End Namespace
