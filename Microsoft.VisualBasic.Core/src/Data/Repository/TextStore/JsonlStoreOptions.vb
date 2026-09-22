#Region "Microsoft.VisualBasic::7b7c06119a091022890b793f87a5ea52, Microsoft.VisualBasic.Core\src\Data\Repository\TextStore\JsonlStoreOptions.vb"

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

    '   Total Lines: 12
    '    Code Lines: 5 (41.67%)
    ' Comment Lines: 4 (33.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (25.00%)
    '     File Size: 342 B


    '     Class JsonlStoreOptions
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Data.Repository

    ''' <summary>
    ''' <see cref="TextStoreOptions"/> 的兼容别名。
    ''' 引擎已泛化为与行格式无关，本类型仅为兼容既有调用方而保留。
    ''' </summary>
    Public NotInheritable Class JsonlStoreOptions
        Inherits TextStoreOptions

    End Class

End Namespace
