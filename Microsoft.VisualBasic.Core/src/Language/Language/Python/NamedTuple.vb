#Region "Microsoft.VisualBasic::6cd06372a0ab61fdc6935b640899b086, Microsoft.VisualBasic.Core\src\Language\Language\Python\NamedTuple.vb"

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

    '   Total Lines: 19
    '    Code Lines: 12
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 621 B


    '     Class NamedTuple
    ' 
    '         Properties: Type
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language.Python

    ''' <summary>
    ''' ``namedtuple()`` Factory Function for Tuples with Named Fields
    ''' </summary>
    Public Class NamedTuple : Inherits [Property](Of Object)
        Implements INamedValue

        Public Property Type As String Implements INamedValue.Key

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace
