#Region "Microsoft.VisualBasic::af936f19966010367e5bdb369ca9bd9c, vs_solutions\JavaScript\test\AssemblyInfo.vb"

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

    '   Total Lines: 6
    '    Code Lines: 2 (33.33%)
    ' Comment Lines: 3 (50.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 1 (16.67%)
    '     File Size: 296 B


    ' 
    ' /********************************************************************************/

#End Region

Imports Xunit

' the engine binds console output through a per-thread static sink; running test
' classes in parallel would still race on the interpreter's global environment,
' so the suite runs serially and deterministically
<Assembly: CollectionBehavior(DisableTestParallelization:=True)>
