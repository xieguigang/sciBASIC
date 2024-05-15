#Region "Microsoft.VisualBasic::518006cb9e8ac0f628c76857d1b680e4, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\NetCoreApp\runtime.vb"

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

    '   Total Lines: 39
    '    Code Lines: 26
    ' Comment Lines: 0
    '   Blank Lines: 13
    '     File Size: 926 B


    '     Class runtime
    ' 
    '         Properties: assemblyVersion, fileVersion
    ' 
    '     Class runtimeTarget
    ' 
    '         Properties: assetType, rid
    ' 
    '     Class library
    ' 
    '         Properties: hashPath, path, serviceable, sha512, type
    ' 
    '     Class frameworkTarget
    ' 
    '         Properties: name, signature
    ' 
    '         Function: ToString
    ' 
    '     Class compilationOptions
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Development.NetCoreApp

    Public Class runtime

        Public Property assemblyVersion As String
        Public Property fileVersion As String

    End Class

    Public Class runtimeTarget : Inherits runtime

        Public Property rid As String
        Public Property assetType As String

    End Class

    Public Class library
        Public Property type As String
        Public Property serviceable As Boolean
        Public Property sha512 As String
        Public Property path As String
        Public Property hashPath As String
    End Class

    Public Class frameworkTarget

        Public Property name As String
        Public Property signature As String

        Public Overrides Function ToString() As String
            Return name
        End Function

    End Class

    Public Class compilationOptions

    End Class
End Namespace
