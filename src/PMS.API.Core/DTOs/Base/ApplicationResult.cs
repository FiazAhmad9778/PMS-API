using System;

namespace PMS.API.Core.DTOs.Base;

public enum ApplicationResult
{
  /// <summary>
  /// The application forgot to set the result type.
  /// </summary>
  Unspecified,

  /// <summary>
  /// The resource was found.
  /// </summary>
  Found,

  /// <summary>
  /// The resource could not be found.
  /// </summary>
  NotFound,

  /// <summary>
  /// The resource was created.
  /// </summary>
  Created,

  /// <summary>
  /// The operation was completed.
  /// </summary>
  Ok,

  /// <summary>
  /// The application did nothing.
  /// </summary>
  NoAction,

  /// <summary>
  /// The operation was deferred to the bus.
  /// </summary>
  Queued
}
