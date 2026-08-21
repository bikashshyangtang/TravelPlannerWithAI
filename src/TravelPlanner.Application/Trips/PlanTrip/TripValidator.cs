using FluentValidation;

namespace TravelPlanner.Application.Trips.PlanTrip;

public class TripValidator : AbstractValidator<TripCriteria>
{
    public TripValidator()
    {
        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required.")
            .MaximumLength(200);
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Start date must be before or equal to end date.");
        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be after or equal to start date.");
        RuleFor(x => x.Budget)
            .GreaterThan(0).WithMessage("Budget must be greater than zero.");
        RuleFor(x => x.Interests)
            .NotNull().WithMessage("Interests must be provoded (empty List is allowed).");
        RuleFor(x => x)
            .Must(x => x.EndDate.DayNumber - x.StartDate.DayNumber + 1 <= 14).WithMessage("Trip longer than 14 days is not allowed.");
    }
}