#Region "Microsoft.VisualBasic::923d8969616415108d9842f4375482e3, Microsoft.VisualBasic.Core\ComponentModel\SingletonHolder.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class SingletonHolder
    ' 
    '         Properties: Instance
    ' 
    '         Constructor: (+1 Overloads) Sub New
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

Namespace ComponentModel

    ''' <summary>
    ''' An Interface for the SingletonHolder Class.(存储单体模式的对象实例)
    ''' </summary>
    ''' <typeparam name="T">泛型T必须是含有一个无参数的构造函数的</typeparam>
    Public NotInheritable Class SingletonHolder(Of T As New)

        Shared _instance As T

        Private Sub New()
        End Sub

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
    End Class
End Namespace
