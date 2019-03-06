#Region "Microsoft.VisualBasic::41b6d68e200cab5fd1d9577b1261732a, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\Model\Directory\docProps.vb"

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

    '     Class docProps
    ' 
    '         Properties: app, core, custom
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: _name
    ' 
    '         Sub: _loadContents
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.docProps

Namespace Model.Directory

    Public Class docProps : Inherits Directory

        Public Property core As core
        Public Property app As XML.docProps.app
        Public Property custom As custom

        Sub New(ROOT$)
            Call MyBase.New(ROOT)
        End Sub

        ''' <summary>
        ''' 有些文件可能是会不存在的，所以在这里就不抛出错误了，直接返回Nothing
        ''' </summary>
        Protected Overrides Sub _loadContents()
            core = (Folder & "/core.xml").LoadXml(Of core)(throwEx:=False)
            custom = (Folder & "/custom.xml").LoadXml(Of custom)(throwEx:=False)
            app = (Folder & "/app.xml").LoadXml(Of XML.docProps.app)(throwEx:=False)
        End Sub

        Protected Overrides Function _name() As String
            Return NameOf(docProps)
        End Function
    End Class
End Namespace
