using System;

namespace PMS.API.Web.Common;

public enum FailureType
{
  Unspecified,
  EntityNotFound,
  ValidationFailure,
  ExternalServiceFailure,
  UnexpectedFailure,
  BusinessRuleViolation,
  BadConfiguration,
  NotImplemented
}
