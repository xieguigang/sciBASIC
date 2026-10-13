#Region "Microsoft.VisualBasic::b22bdca1d5f260422b8b36249f2c5d90, vs_solutions\PkgVersionUpgrade\ProjectResult.vb"

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

    '   Total Lines: 34
    '    Code Lines: 27 (79.41%)
    ' Comment Lines: 2 (5.88%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (14.71%)
    '     File Size: 1.02 KB


    ' Class ProjectResult
    ' 
    '     Properties: [Error], Changed, Changes, FilePath, OutputPath
    '                 OutputPathChanged, RemovedConditions, Skipped, Warnings
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>单个工程的处理结果</summary>
Public Class ProjectResult

    Public Property FilePath As String
    Public Property Changes As VersionChange()
    Public Property RemovedConditions As Integer
    Public Property Warnings As Integer
    Public Property OutputPath As OutputPathResult
    Public Property [Error] As String
    Public Property Skipped As Boolean

    ''' <summary>输出路径修正是否产生了改动</summary>
    Public ReadOnly Property OutputPathChanged As Boolean
        Get
            Return OutputPath IsNot Nothing AndAlso OutputPath.Changed
        End Get
    End Property

    Public ReadOnly Property Changed As Boolean
        Get
            If Changes Is Nothing Then
                Return False
            End If

            For Each change In Changes
                If change.Changed Then
                    Return True
                End If
            Next

            Return False
        End Get
    End Property
End Class
