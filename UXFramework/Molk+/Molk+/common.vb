#Region "Microsoft.VisualBasic::4f1564cdf5c6cdd28be20cdd15ad49a3, ..\visualbasic_App\UXFramework\Molk+\Molk+\common.vb"

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

Module Common

    ''' <summary>
    ''' Microsoft YaHei font.(微软雅黑字体)
    ''' </summary>
    ''' <remarks></remarks>
    Public Const YaHei As String = "Microsoft YaHei"

    Public Function Format(s As Xml.Linq.XElement, ParamArray args As String())
        Return String.Format(s.Value, args)
    End Function
End Module
