using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.VisualBasic.Extensions;
using static Microsoft.VisualBasic.Webservices.Github.API.Search;
using Microsoft.VisualBasic.Webservices.Github;
using Microsoft.VisualBasic.Webservices.Github.API;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

            var result = Microsoft.VisualBasic.Webservices.Bing.SearchEngineProvider.Search("GCModeller");
            // Microsoft.VisualBasic.Webservices.Bing.SearchResult nextr = result.NextPage();

            var query = new UsersQuery { term ="guigang" };
            var queryResult = Search.Users(query.Build(), UserSorts.joined);

            Pause();
        }
    }
}
