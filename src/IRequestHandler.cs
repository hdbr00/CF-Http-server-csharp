using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_http_server.src
{
    interface IRequestHandler
    {
        HttpResponse Handle(HttpRequest request, string directory);
    }
}
