#Region "Microsoft.VisualBasic::5f7806e3be017574758ccbbca8142932, ..\sciBASIC#\tutorials\core.test\ParallelTemplate.vb"

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

Imports Microsoft.VisualBasic.MMFProtocol

Module ParallelTemplate

    Sub Main(argv As String())
        Dim File As String = argv(Scan0)
        Dim Port As Integer = CInt(Val(argv(1)))
        Dim LoadResult = ParallelLoadingTest.Load(File)  '数据加载
        Dim host As String = Microsoft.VisualBasic.Parallel.ParallelLoading.SendMessageAPI(Port)  '返回消息
        Dim Socket As New MMFSocket(host) '打开映射的端口
        '   Call Socket.SendMessage(LoadResult.GetSerializeBuffer) '返回内存数据
    End Sub
End Module
