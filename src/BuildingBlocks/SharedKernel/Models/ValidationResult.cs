using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Models;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    public ValidationResult()
    {
        IsValid = true;
    }

    public ValidationResult(bool isValid, IEnumerable<string> errors)
    {
        IsValid = isValid;
        Errors = errors ?? Enumerable.Empty<string>();
    }

    public static ValidationResult Success()
    {
        return new ValidationResult(true, Enumerable.Empty<string>());
    }

    public static ValidationResult Failure(string error)
    {
        return new ValidationResult(false, new[] { error });
    }

    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        return new ValidationResult(false, errors);
    }

    public static ValidationResult FromDataAnnotations(object model)
    {
        var validationContext = new ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        
        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);
        
        var errors = validationResults.SelectMany(vr => vr.MemberNames.Select(mn => $"{mn}: {vr.ErrorMessage}"));
        
        return new ValidationResult(isValid, errors);
    }

    public void AddError(string error)
    {
        var errors = Errors.ToList();
        errors.Add(error);
        Errors = errors;
        IsValid = false;
    }

    public void AddErrors(IEnumerable<string> errors)
    {
        var errorList = Errors.ToList();
        errorList.AddRange(errors);
        Errors = errorList;
        IsValid = false;
    }
}
