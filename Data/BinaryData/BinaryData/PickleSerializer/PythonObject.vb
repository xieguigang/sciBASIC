#Region "Microsoft.VisualBasic::e1f6c95e86b4af34ca63f27909211c6d, Data\BinaryData\BinaryData\PickleSerializer\PythonObject.vb"

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

    '   Total Lines: 41
    '    Code Lines: 22 (53.66%)
    ' Comment Lines: 11 (26.83%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (19.51%)
    '     File Size: 1.55 KB


    '     Class PythonObject
    ' 
    '         Properties: ClassName, ConstructorArgs, FullName, ModuleName, State
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Pickle

    ''' <summary>
    ''' 表示从 Pickle 反序列化的未知 Python 对象。
    ''' 当遇到无法映射到 .NET 类型的 Python 类实例时，
    ''' 使用此包装器保存模块名、类名、构造参数和对象状态，
    ''' 以便后续代码能够识别和处理这些对象。
    ''' </summary>
    Public Class PythonObject
        ''' <summary>Python 模块名（如 "collections"）</summary>
        Public Property ModuleName As String

        ''' <summary>Python 类名（如 "OrderedDict"）</summary>
        Public Property ClassName As String

        ''' <summary>传递给 __init__ 的构造参数</summary>
        Public Property ConstructorArgs As Object()

        ''' <summary>对象的 __dict__ 状态（由 BUILD 操作码设置）</summary>
        Public Property State As Dictionary(Of Object, Object)

        Public Sub New(moduleName As String, className As String, args As Object())
            Me.ModuleName = moduleName
            Me.ClassName = className
            Me.ConstructorArgs = args
            Me.State = New Dictionary(Of Object, Object)()
        End Sub

        ''' <summary>获取状态的完整限定类名</summary>
        Public ReadOnly Property FullName As String
            Get
                Return $"{ModuleName}.{ClassName}"
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"<{FullName}>"
        End Function
    End Class

End Namespace
