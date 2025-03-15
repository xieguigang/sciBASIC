
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
    Public Module GraphMorganFingerprint

    End Module
End Namespace