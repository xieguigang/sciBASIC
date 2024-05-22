#Region "Microsoft.VisualBasic::b46378c746dbd6227879d3b902f4a669, Data_science\MachineLearning\Bootstrapping\GraphEmbedding\Arguments.vb"

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

    '   Total Lines: 29
    '    Code Lines: 23 (79.31%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 6 (20.69%)
    '     File Size: 1.35 KB


    '     Class Arguments
    ' 
    '         Properties: gamma, iterations, k, lmbda, neg
    '                     skip
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace GraphEmbedding

    Public Class Arguments

        Const help =
            " Usagelala: java ComplEx -train train_triples -valid valid_triples -test test_triples -all all_triples -m number_of_relations -n number_of_entities [options]" & vbLf & vbLf &
            " Options: " & vbLf &
            "   -k        -> number of latent factors (default 50)" & vbLf &
            "   -lmbda    -> regularization parameter (default 0.001)" & vbLf &
            "   -gamma    -> initial learning rate (default 0.1)" & vbLf &
            "   -neg      -> number of negative instances (default 2)" & vbLf &
            "   -#        -> number of iterations (default 1000)" & vbLf &
            "   -skip     -> number of skipped iterations (default 50)"

        Public Property k As Integer = 50
        Public Property lmbda As Double = 0.001
        Public Property gamma As Double = 0.1
        Public Property neg As Integer = 2
        Public Property iterations As Integer = 1000
        Public Property skip As Integer = 1

        Public fnTrainTriples As String, fnValidTriples As String, fnTestTriples As String, fnAllTriples As String
        Public strNumRelation As String = 470
        Public strNumEntity As String = 99604

        Public other As Dictionary(Of String, String)

    End Class
End Namespace
