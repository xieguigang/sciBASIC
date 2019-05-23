Namespace HDF5.device

    ''' <summary>
    ''' <para>
    ''' This class handles converting the <seealso cref="ByteBuffer"/> obtained from the file
    ''' into a Java array containing the data. It makes use of Java NIO ByteBuffers
    ''' bulk read methods where possible to enable high performance IO.
    ''' </para>
    ''' Some useful information about HDF5 → Java type mappings see:
    ''' <ul>
    ''' <li><a href=
    ''' "https://support.hdfgroup.org/ftp/HDF5/prev-releases/HDF-JAVA/hdfjni-3.2.1/hdf5_java_doc/hdf/hdf5lib/H5.html">HDF5
    ''' Java wrapper H5.java</a></li>
    ''' <li><a href="http://docs.h5py.org/en/stable/faq.html">h5py FAQ</a></li>
    ''' <li><a href=
    ''' "https://docs.oracle.com/javase/tutorial/java/nutsandbolts/datatypes.html">Java
    ''' primitive types</a></li>
    ''' </ul>
    ''' 
    ''' @author James Mudd
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/jamesmudd/jhdf/blob/master/jhdf/src/main/java/io/jhdf/dataset/DatasetReader.java
    ''' </remarks>
    Module DatasetReader

    End Module
End Namespace