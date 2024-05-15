#Region "Microsoft.VisualBasic::ebdf46fff322a8fbaf22989b286305b2, Microsoft.VisualBasic.Core\src\ApplicationServices\Tools\Zip\Options.vb"

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

    '   Total Lines: 24
    '    Code Lines: 13
    ' Comment Lines: 9
    '   Blank Lines: 2
    '     File Size: 542 B


    '     Enum Overwrite
    ' 
    '         Always, IfNewer, Never
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum ArchiveAction
    ' 
    '         [Error], Ignore, Merge, Replace
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Zip

    ''' <summary>
    ''' Used to specify what our overwrite policy
    ''' is for files we are extracting.
    ''' </summary>
    Public Enum Overwrite
        Always
        IfNewer
        Never
    End Enum

    ''' <summary>
    ''' Used to identify what we will do if we are
    ''' trying to create a zip file and it already
    ''' exists.
    ''' </summary>
    Public Enum ArchiveAction
        Merge
        Replace
        [Error]
        Ignore
    End Enum
End Namespace
