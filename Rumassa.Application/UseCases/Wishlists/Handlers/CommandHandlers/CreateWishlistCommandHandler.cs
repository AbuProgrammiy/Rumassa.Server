using MediatR;
using Rumassa.Application.Abstractions;
using Rumassa.Application.UseCases.Wishlists.Commands;
using Rumassa.Domain.Entities;
using Rumassa.Domain.Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rumassa.Application.UseCases.Wishlists.Handlers.CommandHandlers
{
    public class CreateWishlistCommandHandler : IRequestHandler<CreateWishlistCommand, ResponseModel>
    {
        private readonly IRumassaDbContext _rumassaDbContext;

        public CreateWishlistCommandHandler(IRumassaDbContext rumassaDbContext)
        {
            _rumassaDbContext = rumassaDbContext;
        }

        public async Task<ResponseModel> Handle(CreateWishlistCommand request, CancellationToken cancellationToken)
        {
            try
            {

                var wishlist = new Wishlist()
                {
                    UserId = request.UserId
                };
                //await _rumassaDbContext.Wishlists.AddAsync(wishlist);

                await _rumassaDbContext.SaveChangesAsync(cancellationToken);

                return new ResponseModel()
                {
                    Message = "Created",
                    StatusCode = 500,
                    IsSuccess = false
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel()
                {
                    Message = ex.Message,
                    StatusCode = 500,
                    IsSuccess = false
                };
            }
        }
    }
}