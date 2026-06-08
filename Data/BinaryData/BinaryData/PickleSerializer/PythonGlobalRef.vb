#Region "Microsoft.VisualBasic::f1dee2aa2ce2c6177643e1cc9d47fde5, Data\BinaryData\BinaryData\PickleSerializer\PythonGlobalRef.vb"

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
    '    Code Lines: 18 (64.29%)
    ' Comment Lines: 5 (17.86%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (17.86%)
    '     File Size: 888 B


    '     Class PythonGlobalRef
    ' 
    '         Properties: ClassName, FullName, ModuleName
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Pickle

    ''' <summary>
    ''' 内部类型：表示 Python 全局引用（module.classname）。
    ''' 由 GLOBAL 操作码产生，供 REDUCE / NEWOBJ 消费。
    ''' 不对外公开，因为用户应通过 PythonObject 访问反序列化结果。
    ''' </summary>
    Friend Class PythonGlobalRef
        Public Property ModuleName As String
        Public Property ClassName As String

        Public Sub New(moduleName As String, className As String)
            Me.ModuleName = moduleName
            Me.ClassName = className
        End Sub

        Public ReadOnly Property FullName As String
            Get
                Return $"{ModuleName}.{ClassName}"
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return FullName
        End Function
    End Class

End Namespace
