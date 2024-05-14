#Region "Microsoft.VisualBasic::4e460ae58064f52b4031d38acbaac7c1, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\DataModels\ModelBase.vb"

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

    '   Total Lines: 13
    '    Code Lines: 6
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 377 B


    '     Interface IProfile
    ' 
    '         Function: Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace ComponentModel.Settings

    ''' <summary>
    ''' 具备有保存数据功能的可配置数据文件的基本定义
    ''' </summary>
    Public Interface IProfile : Inherits IFileReference

        Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean

    End Interface
End Namespace
