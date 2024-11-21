using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.Core;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Repository.Repositories;
using Talabat.Respository;
using Talabat.Service;

namespace Talabat.APIs.Extensions
{
    public static class ApplicationServicesExtensions
    {

        public static IServiceCollection AddApplicationService(this IServiceCollection services) // Container Services
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddScoped<IPaymentService, PaymentService>();

            //webApplicationBuilder.Services.AddAutoMapper(M => M.AddProfile(new MappingProfile()));




            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(P => P.Value.Errors.Count() > 0)
                                              .SelectMany(P => P.Value.Errors)
                                              .Select(E => E.ErrorMessage)
                                              .ToArray();

                    var validationErrorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };
                    return new BadRequestObjectResult(validationErrorResponse);
                };
            });



            return services;
        }

    }
}
