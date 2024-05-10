using MediatR;
using Rumassa.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rumassa.Application.UseCases.UserCases.Queries
{
    public class GetUserByIdQuery : IRequest<User>
    {

        public Guid Id { get; set; }

    }
}
