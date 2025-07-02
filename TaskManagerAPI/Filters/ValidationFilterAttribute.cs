using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskManagerAPI.Filters;

public class ValidationFilterAttribute : ActionFilterAttribute
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilterAttribute(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var parameter in context.ActionArguments)
        {
            if (parameter.Value == null) continue;

            var parameterType = parameter.Value.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(parameterType);
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator != null)
            {
                var validationResult = await validator.ValidateAsync(new ValidationContext<object>(parameter.Value));
                
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.ErrorMessage).ToArray()
                        );
                    
                    context.Result = new BadRequestObjectResult(new { errors });
                    return;
                }
            }
        }

        await next();
    }
}