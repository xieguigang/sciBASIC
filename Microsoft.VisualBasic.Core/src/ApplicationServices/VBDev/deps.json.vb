#Region "Microsoft.VisualBasic::e907db9dccd723690dcf9bf8583b1c28, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\deps.json.vb"

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

    '     Class deps
    ' 
    '         Properties: compilationOptions, libraries, runtimeTarget, targets
    ' 
    '         Function: GetReferenceProject
    ' 
    '     Class target
    ' 
    '         Properties: dependencies
    ' 
    '     Class library
    ' 
    '         Properties: hashPath, path, serviceable, sha512, type
    ' 
    '     Class runtimeTarget
    ' 
    '         Properties: name, signature
    ' 
    '     Class compilationOptions
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Development.NetCore5

    ''' <summary>
    ''' read deps.json for .net 5 assembly
    ''' </summary>
    Public Class deps

        Public Property runtimeTarget As runtimeTarget
        Public Property compilationOptions As compilationOptions
        Public Property targets As Dictionary(Of String, target)
        Public Property libraries As Dictionary(Of String, library)

        ''' <summary>
        ''' get list of project reference name
        ''' </summary>
        ''' <returns></returns>
        Public Function GetReferenceProject() As IEnumerable(Of String)
            Return From entry As KeyValuePair(Of String, library)
                   In libraries
                   Let ref As library = entry.Value
                   Where ref.type = "project"
                   Select entry.Key.StringReplace("/\d+(\.\d+)+", "")
        End Function

    End Class

    Public Class target
        Public Property dependencies As Dictionary(Of String, String)
    End Class

    Public Class library
        Public Property type As String
        Public Property serviceable As Boolean
        Public Property sha512 As String
        Public Property path As String
        Public Property hashPath As String
    End Class

    Public Class runtimeTarget

        Public Property name As String
        Public Property signature As String

    End Class

    Public Class compilationOptions

    End Class
End Namespace
