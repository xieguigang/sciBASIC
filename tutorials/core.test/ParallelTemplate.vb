#Region "Microsoft.VisualBasic::5385add417dafd50030cea3ee7590cd5, sciBASIC#\tutorials\core.test\ParallelTemplate.vb"

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

    '   Total Lines: 99
    '    Code Lines: 0
    ' Comment Lines: 77
    '   Blank Lines: 22
    '     File Size: 2.49 KB


    ' 
    ' /********************************************************************************/

#End Region

'#Region "Microsoft.VisualBasic::167f8c6516031eed8598779f56d18be4, vs_solutions\tutorials\core.test\ParallelTemplate.vb"

'' Author:
'' 
''       asuka (amethyst.asuka@gcmodeller.org)
''       xie (genetics@smrucc.org)
''       xieguigang (xie.guigang@live.com)
'' 
'' Copyright (c) 2018 GPL3 Licensed
'' 
'' 
'' GNU GENERAL PUBLIC LICENSE (GPL3)
'' 
'' 
'' This program is free software: you can redistribute it and/or modify
'' it under the terms of the GNU General Public License as published by
'' the Free Software Foundation, either version 3 of the License, or
'' (at your option) any later version.
'' 
'' This program is distributed in the hope that it will be useful,
'' but WITHOUT ANY WARRANTY; without even the implied warranty of
'' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'' GNU General Public License for more details.
'' 
'' You should have received a copy of the GNU General Public License
'' along with this program. If not, see <http://www.gnu.org/licenses/>.



'' /********************************************************************************/

'' Summaries:

'' Module ParallelTemplate
'' 
''     Sub: Main
'' 
'' /********************************************************************************/

'#End Region

'#Region "Microsoft.VisualBasic::079c39c18aa05ac55970509e9ad8cfc3, core.test"

'' Author:
'' 
'' 
'' Copyright (c) 2018 GPL3 Licensed
'' 
'' 
'' GNU GENERAL PUBLIC LICENSE (GPL3)
'' 


'' Source file summaries:

'' Module ParallelTemplate
'' 
''     Sub: Main
'' 
'' 

'#End Region

'#Region "Microsoft.VisualBasic::5f7806e3be017574758ccbbca8142932, core.test"

'' Author:
'' 
'' 
'' Copyright (c) 2018 GPL3 Licensed
'' 
'' 
'' GNU GENERAL PUBLIC LICENSE (GPL3)
'' 


'' Source file summaries:

'' Module ParallelTemplate
'' 
''     Sub: Main
'' 
'' 
'' 

'#End Region

'Imports Microsoft.VisualBasic.MMFProtocol

'Module ParallelTemplate

'    Sub Main(argv As String())
'        Dim File As String = argv(Scan0)
'        Dim Port As Integer = CInt(Val(argv(1)))
'        Dim LoadResult = ParallelLoadingTest.Load(File)  '数据加载
'        Dim host As String = Microsoft.VisualBasic.Parallel.ParallelLoading.SendMessageAPI(Port)  '返回消息
'        Dim Socket As New MMFSocket(host) '打开映射的端口
'        '   Call Socket.SendMessage(LoadResult.GetSerializeBuffer) '返回内存数据
'    End Sub
'End Module
