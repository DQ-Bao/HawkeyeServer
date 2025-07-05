using System.ComponentModel.DataAnnotations;

namespace HawkeyeServer.Api.Endpoints;

public class ValidationFilter<T> : IEndpointFilter
    where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
            return Results.BadRequest("Invalid payload");

        var validationContext = new ValidationContext(model);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(model, validationContext, results, true))
        {
            var errors = results
                .SelectMany(r => r.MemberNames.Select(name => new { name, r.ErrorMessage }))
                .GroupBy(x => x.name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage ?? "Invalid value").ToArray()
                );
            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}
