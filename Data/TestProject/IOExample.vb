#Region "Microsoft.VisualBasic::08dd7fc7a8bbc90b5606302675e3dfd3, ..\visualbasic_App\Data\TestProject\IOExample.vb"

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

Imports Microsoft.VisualBasic.Data.csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.Text

Public Module IOExample

    Sub Main()
        Call csvIO()
        Call Reflection()
        Call Linq()
        Call LargeData()
    End Sub

    Public Sub csvIO()
        Dim csv As File = File.Load("G:\GCModeller\src\runtime\visualbasic_App\Data\Example\visitors.csv")

        ' access row data
        For Each row As RowObject In csv
            Call row.__DEBUG_ECHO

            ' access columns in a row
            For Each col As String In row
                ' blablabla
            Next
        Next

        ' set row data
        csv(5) = New RowObject({"string", "data"})
        ' set column data in a row
        csv(5)(5) = "yes!"
        ' or 
        Dim r12345 As RowObject = csv(5)
        r12345(6) = "no?"

        Call csv.Save("X:\visitors.csv", Encodings.ASCII)
    End Sub

    Public Sub Reflection()

    End Sub

    Public Sub Linq()

    End Sub

    Public Sub LargeData()

    End Sub
End Module

Public Class visitor

    Public Property uid As Integer
    Public Property time As Date
    Public Property ip As String
    Public Property url As String
    Public Property success As Integer
    Public Property method As String
    Public Property ua As String
    Public Property ref As String
    Public Property data As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
