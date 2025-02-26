#Region "Microsoft.VisualBasic::42ac364cedbe1537c9ae1cdd182702fb, Data_science\Graph\EMD\Signature.vb"

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

    '   Total Lines: 24
    '    Code Lines: 11 (45.83%)
    ' Comment Lines: 8 (33.33%)
    '    - Xml Docs: 75.00%
    ' 
    '   Blank Lines: 5 (20.83%)
    '     File Size: 807 B


    '     Class Signature
    ' 
    '         Properties: Features, NumberOfFeatures, Weights
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace EMD

    ''' <summary>
    ''' Signatures can be used to represent sparse n-dimensional matrices. They are a collection of 
    ''' features and their respective weights. Feature is an interface that you must implement for
    ''' your specific application.
    ''' 
    ''' @author Telmo Menezes (telmo@telmomenezes.com)
    ''' 
    ''' </summary>
    Public Class Signature

        Public Overridable Property NumberOfFeatures As Integer
        Public Overridable Property Features As Feature()
        Public Overridable Property Weights As Double()

        Public Overrides Function ToString() As String
            Return $"[{NumberOfFeatures}] {Weights.GetJson}"
        End Function

    End Class
End Namespace

