using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nstech.Challenge.OrderServices.AppCore.UseCases.Shared_
{
    public enum UseCaseResultType
    {
        NoContent,
        Created,
        Accepted,
        Success,
        Unprocessable,
        NotFound,
        Failure
    }
}
