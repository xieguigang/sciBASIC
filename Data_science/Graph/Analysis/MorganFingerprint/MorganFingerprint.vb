#Region "Microsoft.VisualBasic::03f5b851cae2f0fa752904a95c00a0a3, Data_science\Graph\Analysis\MorganFingerprint\MorganFingerprint.vb"

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

'   Total Lines: 104
'    Code Lines: 39 (37.50%)
' Comment Lines: 50 (48.08%)
'    - Xml Docs: 76.00%
' 
'   Blank Lines: 15 (14.42%)
'     File Size: 5.22 KB


'     Class GraphMorganFingerprint
' 
'         Properties: FingerprintLength
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: CalculateFingerprint, CalculateFingerprintCheckSum
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Math.HashMaps

Namespace Analysis.MorganFingerprint

    ''' <summary>
    ''' Morgan fingerprints, also known as circular fingerprints, are a type of molecular fingerprint 
    ''' used in cheminformatics to represent the structure of chemical compounds. The algorithm steps 
    ''' for generating Morgan fingerprints are as follows:
    ''' 
    ''' 1. **Initialization**:
    '''  - Start with the initial set of atoms in the molecule.
    '''  - Assign a unique identifier (e.g., integer) to each atom.
    '''  
    ''' 2. **Atom Environment Encoding**:
    '''  - For each atom, encode its immediate environment, which includes the atom type and the types of its directly connected neighbors.
    '''  - This information can be represented as a string or a hash.
    '''  
    ''' 3. **Iterative Expansion**:
    '''  - Expand the environment encoding iteratively to include atoms further away from the starting atom.
    '''  - In each iteration, update the encoding to include the types of atoms that are two, three, etc., bonds away from the starting atom.
    '''  
    ''' 4. **Hashing**:
    '''  - Convert the environment encoding into a fixed-size integer using a hashing function. This integer represents the fingerprint of the atom's environment.
    '''  - Different atoms in the molecule will have different fingerprints based on their environments.
    '''  
    ''' 5. **Circular Fingerprint Generation**:
    '''  - For each atom, generate a series of fingerprints that represent its environment at different radii (number of bonds away).
    '''  - The final fingerprint for an atom is a combination of these series of fingerprints.
    '''  
    ''' 6. **Molecular Fingerprint**:
    '''  - Combine the fingerprints of all atoms in the molecule to create the final molecular fingerprint.
    '''  - This can be done by taking the bitwise OR of all atom fingerprints, resulting in a single fingerprint that represents the entire molecule.
    '''  
    ''' 7. **Optional Folding**:
    '''  - To reduce the size of the fingerprint, an optional folding step can be applied. This involves 
    '''    dividing the fingerprint into chunks and performing a bitwise XOR operation within each chunk.
    '''    
    ''' 8. **Result**:
    '''  - The final result is a binary vector (or a list of integers) that represents the Morgan fingerprint 
    '''    of the molecule. This fingerprint can be used for similarity searching, clustering, and other 
    '''    cheminformatics tasks.
    '''    
    ''' Morgan fingerprints are particularly useful because they capture the circular nature of molecular
    ''' environments, meaning that the path taken to reach an atom is not as important as the environment 
    ''' around it. This makes them effective for comparing the similarity of molecules based on their 
    ''' structural features.
    ''' </summary>
    Public MustInherit Class GraphMorganFingerprint(Of V As IMorganAtom, E As IndexEdge)

        ''' <summary>
        ''' the size of the fingerprint data
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property FingerprintLength As Integer = 4096

        Sub New(size As Integer)
            FingerprintLength = size
        End Sub

        Public Function CalculateFingerprintCheckSum(Of G As MorganGraph(Of V, E))(struct As G, Optional radius As Integer = 3) As Byte()
            Dim bits As BitArray = CalculateFingerprint(struct, radius)
            Dim bytes = New Byte(FingerprintLength / 8 - 1) {}
            bits.CopyTo(bytes, 0)
            Return bytes
        End Function

        Public Function CalculateFingerprint(Of G As MorganGraph(Of V, E))(struct As G, Optional radius As Integer = 3) As BitArray
            Dim atoms As V() = struct.Atoms

            ' Initialize atom codes based on atom type
            For i As Integer = 0 To struct.Atoms.Length - 1
                atoms(i).Code = CULng(HashAtom(struct.Atoms(i)))
                atoms(i).Index = i
            Next

            ' Perform iterations to expand the atom codes
            For r As Integer = 0 To radius - 1
                Dim newCodes As ULong() = New ULong(struct.Atoms.Length - 1) {}

                For Each bound As E In struct.Graph
                    newCodes(bound.U) = HashEdge(atoms, bound, flip:=False)
                    newCodes(bound.V) = HashEdge(atoms, bound, flip:=True)
                Next

                For i As Integer = 0 To struct.Atoms.Length - 1
                    atoms(i).Code = newCodes(i)
                Next
            Next

            ' Generate the final fingerprint
            Dim fingerprint As New BitArray(FingerprintLength)

            For Each atom As IMorganAtom In atoms
                Call fingerprint.Xor(position:=atom.Code Mod FingerprintLength)
            Next

            Return fingerprint
        End Function

        Protected MustOverride Function HashAtom(v As V) As ULong
        Protected MustOverride Function HashEdge(atoms As V(), e As E, flip As Boolean) As ULong

        Public Shared Function HashLabelKey(key As String) As ULong
            Static hashcodes As New Dictionary(Of String, ULong)

            Return hashcodes.ComputeIfAbsent(
                key,
                lazyValue:=Function(k)
                               Dim hashcode As ULong = 0

                               For Each c As Char In k
                                   hashcode = HashMap.HashCodePair(hashcode, CULng(Asc(c)))
                               Next

                               Return hashcode
                           End Function)
        End Function
    End Class
End Namespace
