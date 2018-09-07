+ Normal search, **contains any token**: ``term``
+ The result **MUST** contains this term: ``"this term"``
+ **AND**: ``term1 AND term2``
+ **OR**: ``term1 OR term2``
+ **NOT**: ``NOT term3``
+ Search **in Field**: ``<field>: expression``

Please note that:

1. the operator is case sensitive, all of them should be **UPCASE**
2. ``term`` can be regexp, but **MUST** term just a plant text
   + If ``term`` start with character **#**, then ``term`` will translate as regexp
   + If ``term`` start with character **~**, then ``term`` will using **Levenshtein** method to search match
   + If not, then will be treated as plant text, but wildcards will works
   + allowed wildcards includes: 
      + ``*`` for any characters
      + ``%`` for any single characters 
3. If you are using **Normal search**, please note that the ASCII symbol will be using as the delimiter for tokenlize the alphabet words, then if you want these symbol is also be contains in result, please using **MUST** contains method: adding quot on your query term.

   > Here listing the delimiter symbols:
   > 
   > ```json
   > ["!","\"","#","$","%","&","'","(",")","*","+",",","-",".","\/",":",";","<","=",">","?","@","[","\\","]","^","_","`","{","|","}","~"]
   > ```
   >
   > ``*`` and ``%`` was using as wildcards symbol in normal search.
