#Region "Microsoft.VisualBasic::5d5b1554a63235e0a3c776496a93c3e5, gr\FluidSim3D\Program.vb"

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
    '    Code Lines: 11 (40.74%)
    ' Comment Lines: 10 (37.04%)
    '    - Xml Docs: 30.00%
    ' 
    '   Blank Lines: 6 (22.22%)
    '     File Size: 818 B


    '     Module Program
    ' 
    '         Sub: Main
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
'     FluidSim3D - a 3D water simulator driven by the 3D SPH fluid engine.
' 
'     This is the custom application entry point (MyType = WindowsFormsWithCustomSubMain).
'
' /********************************************************************************/

Imports System.Windows.Forms

Namespace FluidSim3D

    ''' <summary>
    ''' custom application entry point for the WinForm 3D water simulator.
    ''' </summary>
    Public Module Program

        <System.STAThread()>
        Public Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New MainForm())
        End Sub

    End Module

End Namespace

