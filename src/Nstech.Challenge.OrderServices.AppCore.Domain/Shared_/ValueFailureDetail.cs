using Nstech.Challenge.OrderServices.AppCore.Domain.Shared_;
using System.Collections.Generic;

namespace Nstech.Challenge.OrderServices.Http.Bff.Shared_;

public sealed record ValueFailureDetail(IReadOnlyList<FailureDetail> Failures);
