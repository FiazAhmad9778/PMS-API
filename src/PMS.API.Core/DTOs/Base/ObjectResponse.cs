using PMS.API.Core.DTOs.Base.Response;

namespace PMS.API.Core.DTOs.Base;


/// <summary>
/// Object response base class.
/// </summary>
public class ObjectResponse : ErrorResponse
{
  public ObjectResponse()
  {

  }

  /// <summary>
  /// Gets or sets the type of the result.
  /// </summary>
  /// <value>
  /// The type of the result.
  /// </value>
  public ApplicationResult ResultType { get; set; }

  /// <summary>
  /// Gets or sets the metadata.
  /// </summary>
  /// <value>
  /// The metadata.
  /// </value>
  public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

  /// <summary>
  /// Adds meta data.
  /// </summary>
  /// <param name="key">The key.</param>
  /// <param name="value">The value.</param>
  public void AddMetadata(string key, object value) => Metadata.Add(key, value);
}


/// <inheritdoc />
/// <summary>
/// Object response wrapper.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectResponse<T> : ObjectResponse
{

  /// <summary>
  /// Gets or sets the result.
  /// </summary>
  /// <value>
  /// The result.
  /// </value>
  public ResponseObject<T>? Result { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="ObjectResponse{aqaT}"/> class.
  /// </summary>
  /// <param name="result">The result.</param>
  public ObjectResponse(ResponseObject<T> result)
  {
    Result = result;
  }
  public ObjectResponse()
  {

  }
  /// <summary>
  /// Initializes a new instance of the <see cref="ObjectResponse{T}" /> class.
  /// </summary>
  /// <param name="result">The result.</param>
  /// <param name="resultType">Type of the result.</param>
  /// <param name="callback">A callback that can be run against the internally constructed <see cref="ResponseObject{T}" />The type of the result</param>
  public ObjectResponse(T result, ApplicationResult resultType = ApplicationResult.Ok,
      Action<ResponseObject<T>>? callback = null)
  {
    ResultType = resultType;
    Result = new ResponseObject<T>(result);
    callback?.Invoke(Result);
  }
}

