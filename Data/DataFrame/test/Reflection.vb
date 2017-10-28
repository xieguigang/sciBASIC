#Region "Microsoft.VisualBasic::733af266d2da7f75214650da38e712ee, ..\sciBASIC#\Data\DataFrame\test\Reflection.vb"

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

Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO

Module Reflection

    Sub Main()

        Dim visitors As Visitor() = "../../../../Example/visitors.csv".LoadCsv(Of Visitor)
        Dim dynamics As EntityObject() = EntityObject.LoadDataSet("../../../../Example/visitors.csv").ToArray

        Call visitors.SaveTo("./test.csv")
        Call dynamics.SaveTo("./test2.csv")

        For Each visit In dynamics
            With visit
                Call println("%s visit %s at %s", !ip, !url, !time)
            End With
        Next

        Pause()
    End Sub
End Module

Public Class Visitor

    Public Property uid As String
    Public Property time As String
    Public Property ip As String
    Public Property url As String
    Public Property success As String
    Public Property method As String
    Public Property ua As String
    Public Property ref As String
    Public Property data As String

    Public Overrides Function ToString() As String
        Return $"{ip}@{time}"
    End Function

End Class
