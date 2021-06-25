# https://www.codeproject.com/Articles/1264613/GraphQuery-Powerful-Text-Query-Language-3

graphquery
{
    # parser function pipeline can be 
    # in different line,
    # this will let you write graphquery
    # code in a more graceful style when
    # you needs a lot of pipeline function
    # for parse value data.
    bookID    css("book") 
            | attr("id")

    title     css("title")
    isbn      xpath("//isbn")
    quote     css("quote")
    language  css("title") | attr("lang")

    # another sub query in current graph query
    author css("author") {
        name css("name")
        born css("born")
        dead css("dead")
    }

    # this is a array of type character
    character xpath("//character") [{
        name          css("name")
        born          css("born")
        qualification xpath("qualification")
    }]
}