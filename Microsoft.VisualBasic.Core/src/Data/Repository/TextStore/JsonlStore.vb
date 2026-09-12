#Region "Microsoft.VisualBasic::8b439a65270e117d9bb2bd3cb2beeef0, Microsoft.VisualBasic.Core\src\Data\Repository\TextStore\JsonlStore.vb"

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

    '   Total Lines: 23
    '    Code Lines: 8 (34.78%)
    ' Comment Lines: 10 (43.48%)
    '    - Xml Docs: 40.00%
    ' 
    '   Blank Lines: 5 (21.74%)
    '     File Size: 1.07 KB


    '     Class JsonlStore
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Data.Repository

    ' ============================================================================
    '  JsonlStore.vb —— 纯文本行存储引擎的兼容包装
    '  历史上本引擎只用于 JSONL（每行一个 JSON 对象），因此命名为 JsonlStore。
    '  引擎本身与行格式无关，现已泛化为 <see cref="TextLineStore"/>。
    '  这里保留 JsonlStore 作为 Thin Wrapper，以兼容既有调用方与测试代码。
    ' ============================================================================

    ''' <summary>
    ''' <see cref="TextLineStore"/> 的兼容别名。行为与泛化后的行存储引擎完全一致。
    ''' </summary>
    Public NotInheritable Class JsonlStore
        Inherits TextLineStore

        ''' <summary>打开（或创建）指定数据文件的存储引擎。</summary>
        Public Sub New(dataFilePath As String, Optional options As JsonlStoreOptions = Nothing)
            MyBase.New(dataFilePath, options)
        End Sub

    End Class

End Namespace

