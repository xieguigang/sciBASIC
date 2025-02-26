#Region "Microsoft.VisualBasic::bf9d50cd8ddc663f3c16ce62c08f15b7, Data_science\Graph\EMD\Run.vb"

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

    '   Total Lines: 82
    '    Code Lines: 42 (51.22%)
    ' Comment Lines: 31 (37.80%)
    '    - Xml Docs: 93.55%
    ' 
    '   Blank Lines: 9 (10.98%)
    '     File Size: 3.15 KB


    '     Module Run
    ' 
    '         Function: emdDist, getSignature, getValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace EMD

    Public Module Run

        Friend Function getValue(map As Double(), x As Integer, y As Integer, bins As Integer) As Double
            Return map(y * bins + x)
        End Function

        Friend Function getSignature(map As Double(), bins As Integer) As Signature
            ' find number of entries in the sparse matrix
            Dim n = 0
            For x = 0 To bins - 1
                For y = 0 To bins - 1
                    If getValue(map, x, y, bins) > 0 Then
                        n += 1
                    End If
                Next
            Next


            ' compute features and weights
            Dim features = New Feature2D(n - 1) {}
            Dim weights = New Double(n - 1) {}
            Dim i = 0
            For x = 0 To bins - 1
                For y = 0 To bins - 1
                    Dim val = getValue(map, x, y, bins)
                    If val > 0 Then
                        Dim f As Feature2D = New Feature2D(x, y)
                        features(i) = f
                        weights(i) = val
                        i += 1
                    End If
                Next
            Next

            Dim signature As Signature = New Signature()
            signature.NumberOfFeatures = n
            signature.Features = features
            signature.Weights = weights

            Return signature
        End Function

        ''' <summary>
        ''' A Linear Time Histogram Metric for Improved SIFT Matching
        ''' Ofir Pele, Michael Werman
        ''' ECCV 2008
        ''' bibTex:
        ''' @INPROCEEDINGS{Pele-eccv2008,
        ''' author = {Ofir Pele and Michael Werman},
        ''' title = {A Linear Time Histogram Metric for Improved SIFT Matching},
        ''' booktitle = {ECCV},
        ''' year = {2008}
        ''' }
        ''' Fast and Robust Earth Mover's Distances
        ''' Ofir Pele, Michael Werman
        ''' ICCV 2009
        ''' @INPROCEEDINGS{Pele-iccv2009,
        ''' author = {Ofir Pele and Michael Werman},
        ''' title = {Fast and Robust Earth Mover's Distances},
        ''' booktitle = {ICCV},
        ''' year = {2009}
        ''' }
        ''' </summary>
        ''' <param name="map1">vector 1</param>
        ''' <param name="map2">vector 2</param>
        ''' <param name="bins">any positive integer number</param>
        ''' <param name="extraMassPenalty">
        ''' penalty for extra mass. 0 for no penalty, -1 for the default, other positive values to specify the penalty;
        ''' An extraMassPenalty of -1 means that the extra mass penalty is the maximum distance found between two features.
        ''' </param>
        ''' <returns></returns>
        Public Function emdDist(map1 As Double(), map2 As Double(), bins As Integer, Optional extraMassPenalty As Double = -1) As Double
            Dim vec1 = getSignature(map1, bins)
            Dim vec2 = getSignature(map2, bins)
            Dim dist = JFastEMD.distance(vec1, vec2, extraMassPenalty)

            Return dist
        End Function
    End Module
End Namespace
