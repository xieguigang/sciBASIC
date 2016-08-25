#Region "Microsoft.VisualBasic::379cd29b00463926c59ca7e942bd3b55, ..\visualbasic_App\DocumentFormats\VB_DataFrame\TestProject\ResultData.vb"

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

Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic


Namespace RTools

    Public Class DESeq

        Public Property eeeeeee As ResultData()

        Public Class ResultData

            Public Property [enum] As Microsoft.VisualBasic.AppWinStyle
            Public Property Gene As String
            Public Property baseMean As Double
            Public Property log2FoldChange As Double
            Public Property lfcSE As Double
            Public Property stat As Double
            Public Property pvalue As Double
            Public Property padj As Double
            Public Property attr As KeyValuePair(Of String, Double)
            Public Property attr2 As Microsoft.VisualBasic.ComponentModel.Collection.Generic.KeyValuePairObject(Of String, Boolean)
            Public Property array As Double()
            Public Property list As List(Of Long)

            <MetaAttribute(GetType(Double))>
            Public Property ExperimentValues As Dictionary(Of String, Double)

            Public Overrides Function ToString() As String
                Return $"{Gene} ---> log2FoldChange  {log2FoldChange}"
            End Function
        End Class
    End Class
End Namespace
