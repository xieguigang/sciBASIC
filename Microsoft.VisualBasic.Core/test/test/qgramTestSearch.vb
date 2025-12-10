#Region "Microsoft.VisualBasic::be86ebb43cca1cc032796451150506e6, Microsoft.VisualBasic.Core\test\test\qgramTestSearch.vb"

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
    '    Code Lines: 23 (82.14%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (17.86%)
    '     File Size: 834 B


    ' Module qgramTestSearch
    ' 
    '     Sub: Run
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language

Module qgramTestSearch

    Sub Run()
        Dim q As New QGramIndex(3)
        Dim i As i32 = 0

        Call q.AddString("", ++i)
        Call q.AddString("", ++i)
        Call q.AddString("", ++i)
        Call q.AddString("test ATP token", ++i)
        Call q.AddString("", ++i)
        Call q.AddString("", ++i)
        Call q.AddString("ATP", ++i)
        Call q.AddString("test acid", ++i)
        Call q.AddString("ATP", ++i)
        Call q.AddString("ATP+", ++i)
        Call q.AddString("Hello world", ++i)
        Call q.AddString("test", ++i)

        Dim find1 = q.FindSimilar("ATP").ToArray
        Dim find2 = q.FindSimilar("atp").ToArray

        Pause()
    End Sub
End Module

