#Region "Microsoft.VisualBasic::6a44cbc100c8c4c03681b2218f3f9d30, Data_science\MachineLearning\CellularAutomaton\GameOfLife.WinForms\Program.vb"

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

    '   Total Lines: 16
    '    Code Lines: 10 (62.50%)
    ' Comment Lines: 3 (18.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (18.75%)
    '     File Size: 461 B


    '     Module Program
    ' 
    '         Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Windows.Forms

    ''' <summary>
    ''' 应用程序入口。禁用应用框架（UseApplicationFramework=false），以 Sub Main 显式启动窗体。
    ''' </summary>
    Module Program

        <STAThread>
        Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New MainForm())
        End Sub

    End Module
