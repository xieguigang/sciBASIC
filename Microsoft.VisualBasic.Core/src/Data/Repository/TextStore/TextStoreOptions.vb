#Region "Microsoft.VisualBasic::9e9eb2db8f1e3d5d014b42fc0ba706bc, Microsoft.VisualBasic.Core\src\Data\Repository\TextStore\TextStoreOptions.vb"

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

    '   Total Lines: 27
    '    Code Lines: 13 (48.15%)
    ' Comment Lines: 10 (37.04%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (14.81%)
    '     File Size: 1.43 KB


    '     Class TextStoreOptions
    ' 
    '         Properties: Encoding, FsyncEachWrite, IndexGranularity, LogBufferBytes, MergeBufferBytes
    '                     NewLine, ReadBufferBytes, RepairTornTail
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Data.Repository

    ''' <summary>
    ''' 数据文件的进程级锁模式。
    ''' </summary>
    Public Enum TextStoreLockMode
        ''' <summary>
        ''' 独占锁（默认）：同一数据文件同一时刻只允许一个实例打开。
        ''' 与旧版行为完全一致。
        ''' </summary>
        Exclusive = 0
        ''' <summary>
        ''' 共享读：多个只读实例可以同时持有该锁；写实例（<see cref="Exclusive"/>）
        ''' 与所有读者互斥。仅限只读使用，调用方必须保证不写入。
        ''' </summary>
        SharedRead = 1
        ''' <summary>
        ''' 不使用进程级锁：调用方需自行保证互斥（例如已在外层持锁）。
        ''' </summary>
        None = 2
    End Enum

    ''' <summary>
    ''' 与具体行格式无关的纯文本行存储引擎配置。
    ''' 该引擎把每一行视为不透明文本（JSONL / CSV / TSV 等均可复用），
    ''' 因此这里的配置项不包含任何格式相关语义。
    ''' </summary>
    Public Class TextStoreOptions

        ''' <summary>数据文件编码。Nothing = UTF-8（推荐）。不支持 UTF-16（行切分依赖 0x0A 字节）。</summary>
        Public Property Encoding As Encoding
        ''' <summary>写入使用的换行符。Nothing = 自动检测，缺省 LF。</summary>
        Public Property NewLine As String
        ''' <summary>稀疏索引粒度（每多少行记一个字节偏移）。</summary>
        Public Property IndexGranularity As Integer = 1024
        ''' <summary>每次写操作后是否 fsync 日志（断电级安全；False 仅防进程崩溃）。</summary>
        Public Property FsyncEachWrite As Boolean = True
        ''' <summary>打开时截断数据文件尾部不完整行。注意：会删除“无结尾换行符”的最后一行。</summary>
        Public Property RepairTornTail As Boolean = True
        Public Property ReadBufferBytes As Integer = 1 << 20
        Public Property MergeBufferBytes As Integer = 1 << 20
        Public Property LogBufferBytes As Integer = 1 << 16

        ''' <summary>
        ''' 进程级锁模式。默认 <see cref="TextStoreLockMode.Exclusive"/>，
        ''' 与旧版行为一致。
        ''' </summary>
        Public Property LockMode As TextStoreLockMode = TextStoreLockMode.Exclusive
        ''' <summary>
        ''' 获取锁的等待超时（毫秒）。0（默认）= 冲突时立即抛出异常，与旧版行为一致；
        ''' 大于 0 时按 <see cref="LockRetryIntervalMs"/> 重试直到超时。
        ''' </summary>
        Public Property LockWaitTimeoutMs As Integer = 0
        ''' <summary>锁冲突后的重试间隔（毫秒），仅在 <see cref="LockWaitTimeoutMs"/> &gt; 0 时生效。</summary>
        Public Property LockRetryIntervalMs As Integer = 50

    End Class
End Namespace

