Namespace dbn
    ''' 
    ''' <summary>
    ''' An attribute is a representation of a random variable in a DBN. It can be
    ''' numeric (discrete valued) or nominal and takes a finite number of different
    ''' values. Values are indexed by sequential integers, which are used for
    ''' representing them.
    ''' 
    ''' @author josemonteiro
    ''' 
    ''' </summary>

    Public Interface Attribute

        ''' <summary>
        ''' Function that says if an attribute is Numeric. </summary>
        ''' <returns> boolean Returns true if the attribute is numeric. </returns>
        ReadOnly Property Numeric As Boolean

        ''' <summary>
        ''' Function that says if an attribute is Nominal. </summary>
        ''' <returns> boolean Returns true if the attribute is nominal. </returns>
        ReadOnly Property Nominal As Boolean

        ''' <summary>
        ''' Function that returns the number of possible values that an attribute can assume. </summary>
        ''' <returns> int the number of possible values that an attribute can assume. </returns>
        Function size() As Integer

        ''' <summary>
        ''' Function that returns the corresponding value of an attribute. </summary>
        ''' <param name="index"> The index of the corresponding value. </param>
        ''' <returns> String Corresponding value that the attribute assume. </returns>
        Function [get](index As Integer) As String

        ''' <summary>
        ''' Function that returns the corresponding index of an value that the attribute assume. </summary>
        ''' <param name="value"> Value that the attribute. </param>
        ''' <returns> int Corresponding index of the assumed value. </returns>
        Function getIndex(value As String) As Integer

        ''' <summary>
        ''' Adds a new value that the attribute assumes. </summary>
        ''' <param name="value"> Corresponding value that the attribute assumes. </param>
        ''' <returns> boolean Returns true if the values is added and false if the attribute already has this value. </returns>
        Function add(value As String) As Boolean

        Function ToString() As String

        ''' <summary>
        ''' Setter for the name of the attribute. </summary>
        ''' <param name="name"> Name of the attribute. </param>
        Property Name As String


    End Interface

End Namespace
