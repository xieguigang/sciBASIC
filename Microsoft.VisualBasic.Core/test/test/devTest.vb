#Region "Microsoft.VisualBasic::251530e0496d7bdb9e8c8b09a40ed98a, Microsoft.VisualBasic.Core\test\test\devTest.vb"

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

    '   Total Lines: 31
    '    Code Lines: 22
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 859 B


    ' Module devTest
    ' 
    '     Sub: logfileTest, Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCoreApp
Imports Microsoft.VisualBasic.Serialization.JSON

Module devTest

    Sub Main1()
        Call logfileTest()

        Dim deps = "D:\GCModeller\src\R-sharp\App\net5.0\base.deps.json".LoadJsonFile(Of deps)
        Dim ref = deps.GetReferenceProject.ToArray

        Pause()
    End Sub

    Sub logfileTest()
        Dim log = App.RedirectLogging("E:\VB_GamePads\src\framework\Microsoft.VisualBasic.Core\test\bin\Debug.txt")

        Call Console.WriteLine("sdfsdfsdfsdf")
        Call Console.Write("adfasdas")
        Call Console.Write("  ")
        Call Console.Write("!!!!!")
        Call Console.WriteLine("--------")

        Call log.Flush()
        Call log.Close()

        Call Console.OpenStandardOutput()

        Pause()
    End Sub
End Module
