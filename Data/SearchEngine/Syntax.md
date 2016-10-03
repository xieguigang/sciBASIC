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
   + If not, then will be treated as plant text, but wildcards will works
     + allowed wildcards includes: 
       + ``*`` for any characters
       + ``%`` for any single characters 
