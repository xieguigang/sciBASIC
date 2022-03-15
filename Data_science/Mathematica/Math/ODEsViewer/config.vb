#Region "Microsoft.VisualBasic::0b028dda780d08ac268c2cbf6d8636cf, sciBASIC#\Data_science\Mathematica\Math\ODEsViewer\config.vb"

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
    '    Code Lines: 26
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 922.00 B


    ' Class config
    ' 
    '     Properties: DefaultFile, models, references
    ' 
    '     Function: Load
    ' 
    '     Sub: Save
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Public Class config

    Public Property models As List(Of String)
    Public Property references As List(Of String)

    Public Shared ReadOnly Property DefaultFile As String = App.LocalData & "/config.json"

    Public Shared Function Load() As config
        If DefaultFile.FileExists Then
            Try
                Return DefaultFile.ReadAllText.LoadObject(Of config)
            Catch ex As Exception
                GoTo NEW_CONFIG
            End Try
        Else
NEW_CONFIG:
            Dim [new] As New config With {
                  .models = New List(Of String),
                  .references = New List(Of String)
              }
            Call [new].GetJson.SaveTo(DefaultFile)
            Return [new]
        End If
    End Function

    Public Sub Save()
        Call GetJson.SaveTo(DefaultFile)
    End Sub
End Class
