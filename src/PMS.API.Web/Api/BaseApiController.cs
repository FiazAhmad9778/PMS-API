using System.Net;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PMS.API.Core.DTOs.Base;
using PMS.API.Web.Common;

namespace PMS.API.Web.Api;

/// <summary>
/// If your API controllers will use a consistent route convention and the [ApiController] attribute (they should)
/// then it's a good idea to define and use a common base controller class like this one.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public abstract class BaseApiController : Controller
{
  private readonly IServiceProvider _serviceProvider;

  /// <summary>
  /// Initializes a new instance of the <see cref="ApiController"/> class.
  /// </summary>
  /// <param name="serviceProvider">The service provider.</param>
  protected BaseApiController(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  /// <summary>
  /// Gets the mediator.
  /// </summary>
  /// <value>
  /// The mediator.
  /// </value>
  private ISender? _mediator = null;
  protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

  /// <summary>
  /// Gets the mapper.
  /// </summary>
  /// <value>
  /// The mapper.
  /// </value>
  protected IMapper Mapper => _serviceProvider.GetRequiredService<IMapper>();

  /// <summary>
  /// Gets the feature flags.
  /// </summary>
  /// <value>
  /// The feature flags.
  /// If we decide to start using FeatureFlags.
  /// </value>
  //protected IFeatureFlags FeatureFlags => _serviceProvider.GetRequiredService<IFeatureFlags>();

  /// <summary>
  /// Unwraps the application response model and creates a uniform API response model.
  /// </summary>
  /// <typeparam name="TInput">The result type from the application response <see cref="ObjectResponse{T}"/></typeparam>
  /// <typeparam name="TOutput">The view model type to be returned to the client.</typeparam>
  /// <param name="applicationResult">The application response.</param>
  /// <returns></returns>
  protected IActionResult PrepareResponse<TInput, TOutput>(ObjectResponse<TInput> applicationResult)
      where TOutput : ViewModel
  {
    var result = GetModelResult<TInput, TOutput>(applicationResult.Result!);
    result.Metadata = applicationResult.Metadata;

    return GetResponse(applicationResult.ResultType, result);
  }

  /// <summary>
  /// Unwraps the application response model and creates a uniform API response model.
  /// </summary>
  /// <typeparam name="TInput">The result type from the application response <see cref="ObjectResponse{T}"/></typeparam>
  /// <typeparam name="TOutput">The view model type to be returned to the client.</typeparam>
  /// <param name="applicationResult">The application response.</param>
  /// <returns></returns>
  protected IActionResult PrepareResponse<TInput, TOutput>(ObjectSetResponse<TInput> applicationResult)
      where TOutput : ViewModel
  {
    var viewModelResults = applicationResult.Results!
        .Select(GetModelResult<TInput, TOutput>);

    var set = new ViewModelResultSet<TOutput>(viewModelResults) { Metadata = applicationResult.Metadata };
    return GetResponse(applicationResult.ResultType, set);
  }

  /// <summary>
  /// Unwraps the application response model and creates a uniform API response model.
  /// </summary>
  /// <typeparam name="TInput">The result type from the application response <see cref="ObjectResponse{T}"/></typeparam>
  /// <typeparam name="TOutput">The view model type to be returned to the client.</typeparam>
  /// <param name="applicationResult">The application response.</param>
  /// <returns></returns>
  protected IActionResult PrepareResponse<TInput, TOutput>(PagedObjectSetResponse<TInput> applicationResult)
      where TOutput : ViewModel
  {
    var viewModelResults = applicationResult.Results!
        .Select(GetModelResult<TInput, TOutput>);

    var set = new PagedViewModelResultSet<TOutput>(
        viewModelResults,
        applicationResult.Page,
        applicationResult.PageSize,
        applicationResult.Total);

    return GetResponse(applicationResult.ResultType, set);
  }

  private ViewModelResult<TOutput> GetModelResult<TInput, TOutput>(ResponseObject<TInput> applicationResult)
      where TOutput : ViewModel
  {
    if (applicationResult.Value == null)
      return null!;

    var mapped = Mapper.Map<TInput, TOutput>(applicationResult.Value);
    mapped.Metadata = new Metadata(applicationResult.Metadata);
    var vmResult = new ViewModelResult<TOutput>(mapped);

    return vmResult;
  }

  private IActionResult GetResponse(ApplicationResult resultType, ViewModelResult result = null!)
  {
    HttpStatusCode statusCode;

    switch (resultType)
    {
      case ApplicationResult.Unspecified:
        statusCode = HttpStatusCode.OK;
        break;
      case ApplicationResult.Found:
        statusCode = HttpStatusCode.OK;
        break;
      case ApplicationResult.NotFound:
        statusCode = HttpStatusCode.NotFound;
        break;
      case ApplicationResult.Created:
        statusCode = HttpStatusCode.Created;
        break;
      case ApplicationResult.Ok:
        statusCode = HttpStatusCode.OK;
        break;
      case ApplicationResult.NoAction:
        statusCode = HttpStatusCode.NotAcceptable;
        break;
      case ApplicationResult.Queued:
        statusCode = HttpStatusCode.Accepted;
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(resultType), resultType, null);
    }

    return result == null
        ? (IActionResult)new StatusCodeResult((int)statusCode)
        : new StatusCodeObjectResult((int)statusCode, result);
  }

  #region -- Validation --


  public override void OnActionExecuting(ActionExecutingContext context)
  {
    if (!ModelState.IsValid)
    {
      var errors = new List<ServiceError>();
      foreach (var entry in ModelState.Where(k => !string.IsNullOrEmpty(k.Key)))
      {
        errors.AddRange(entry.Value!.Errors.Select(e => new ServiceError
        {
          Code = 400,
          Message = e.ErrorMessage
        }));
      }

      var response = new ServiceError
      {
        Code = 400,
        Message = "Bad Request",
        Trace = errors
      };

      context.Result = BadRequest(response);
    }
  }

  #endregion
}
