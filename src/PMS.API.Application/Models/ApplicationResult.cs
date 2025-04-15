using PMS.API.Application.Common.Enums;
using PMS.API.Application.DTOs.Common.Base.Response;

namespace PMS.API.Application.Common.Models;

public class ApplicationResult : ErrorResponse
{
  public ApplicationResult()
  {

  }
  protected ApplicationResult(bool succeed, IEnumerable<string> errors)
  {
    Success = succeed;
    IsSuccess = succeed;
    Errors = errors;
  }
  protected ApplicationResult(bool succeed, IEnumerable<string> errors, int totalCount)
  {
    Success = succeed;
    Errors = errors;
  }
  protected ApplicationResult(bool succeed, string message)
  {
    Success = succeed;
    IsSuccess = succeed;
    Message = message;
  }

  protected ApplicationResult(bool succeed, IEnumerable<ValidationError> validationErrors)
  {
    Success = succeed;
    IsSuccess = succeed;
    ValidationErrors = validationErrors;
  }
  protected ApplicationResult(bool succeed, IEnumerable<string> errors, Exception ex)
  {
    Success = succeed;
    IsSuccess = succeed;
    Errors = errors;
  }
  public bool Success { get; }



  public static ApplicationResult Error(IEnumerable<string> errors)
  {
    return new ApplicationResult(false, errors);
  }

  public static ApplicationResult Error(IEnumerable<string> errors, Exception ex)
  {
    return new ApplicationResult(false, errors, ex);
  }
  public static ApplicationResult SuccessResult()
  {
    return new ApplicationResult(true, new string[] { });
  }
}

public class ApplicationResult<T> : ApplicationResult
{
  public ApplicationResult()
  {

  }
  private ApplicationResult(bool succeed, IEnumerable<string> errors, T? data) : base(succeed, errors)
  {
    Data = data;
  }
  private ApplicationResult(bool succeed, IEnumerable<string> errors, T? data, int totalCount) : base(succeed, errors, totalCount)
  {
    Data = data;
    TotalCount = totalCount;
  }
  private ApplicationResult(bool succeed, IEnumerable<string> errors, Exception ex, T? data) : base(succeed, errors, ex)
  {
    Data = data;
  }

  private ApplicationResult(bool succeed, string message, T? data) : base(succeed, message)
  {
    Data = data;
  }

  private ApplicationResult(bool succeed, string message, PMSErrorEnums? errorId = null) : base(succeed, message)
  {
    ErrorId = errorId;
  }
  private ApplicationResult(bool succeed, IEnumerable<ValidationError> ValidationErrors, T? data) : base(succeed, ValidationErrors)
  {
    Data = data;
  }

  public T? Data { get; }
  public int TotalCount { get; }

  public static ApplicationResult<T> Error(IEnumerable<string> errors, T? data = default)
  {
    return new ApplicationResult<T>(false, errors, data);
  }

  public static ApplicationResult<T> Error(string message, T? data = default)
  {
    return new ApplicationResult<T>(false, message, data);
  }
  public static ApplicationResult<T> Error(string message, PMSErrorEnums? errorId)
  {
    return new ApplicationResult<T>(false, message, errorId);
  }

  //List of Validation Errors
  public static ApplicationResult<T> Error(IEnumerable<ValidationError> validationErrors, T? data = default)
  {
    return new ApplicationResult<T>(false, validationErrors, data);
  }

  //single Validation Error param
  public static ApplicationResult<T> Error(ValidationError validationErrors, T? data = default)
  {

    List<ValidationError> listOfErrors = new List<ValidationError> {
    validationErrors,
    };

    return new ApplicationResult<T>(false, listOfErrors, data);
  }

  public static ApplicationResult<T> Error(IEnumerable<string> errors, Exception ex, T? data = default)
  {
    return new ApplicationResult<T>(false, errors, ex, data);
  }

  public static ApplicationResult<T> SuccessResult(T data)
  {
    return new ApplicationResult<T>(true, new string[] { }, data);
  }
  public static ApplicationResult<T> SuccessResult(T data, int totalCount)
  {
    return new ApplicationResult<T>(true, new string[] { }, data, totalCount);
  }
  public static ApplicationResult<T> SuccessResult(T data, string message)
  {
    return new ApplicationResult<T>(true, message, data);
  }
}
