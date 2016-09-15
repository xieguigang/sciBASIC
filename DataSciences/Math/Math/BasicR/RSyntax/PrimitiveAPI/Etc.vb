#Region "Microsoft.VisualBasic::3a082e2c5d1cc1dedc8a8c71892f3ba8, ..\R.Bioconductor\RDotNET.Extensions.VisualBasic\RSyntax\PrimitiveAPI\Etc.vb"

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

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Mathematical.SyntaxAPI.Vectors

Namespace SyntaxAPI

    <PackageNamespace("RBase.Is")>
    Public Module [Is]

        ''' <summary>
        ''' is.finite and is.infinite return a vector of the same length as x, indicating which elements are finite (not infinite and not missing) or infinite.
        ''' </summary>
        ''' <param name="x">R object to be tested: the default methods handle atomic vectors.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("is.finite")>
        Public Function Finite(x As Vector) As BooleanVector
            Return New BooleanVector((From n In x Select Not Double.IsInfinity(n)).ToArray)
        End Function

        <ExportAPI("is.NAN")>
        Public Function NAN(x As Vector) As BooleanVector
            Return New BooleanVector((From n In x Select Double.IsNaN(x)).ToArray)
        End Function

    End Module

    Namespace Machine

        <PackageNamespace("RBase.Machine.Double")>
        Public Module [Double]

            Public ReadOnly Property Eps As Double
                Get
                    Return Double.Epsilon
                End Get
            End Property
        End Module
    End Namespace
End Namespace
