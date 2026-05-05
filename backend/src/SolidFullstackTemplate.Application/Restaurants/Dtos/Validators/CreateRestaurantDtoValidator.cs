using FluentValidation;

namespace SolidFullstackTemplate.Application.Restaurants.Dtos.Validators;

public class CreateRestaurantDtoValidator : AbstractValidator<CreateRestaurantDto>
{
    private readonly List<string> validCategories = [ "Italian", "Mexican", "Chinese", "Fast Food" ];

    public CreateRestaurantDtoValidator(/*ICategoryValidationService categoryValidationService*/)
    {
        RuleFor(dto => dto.Name)
            .MinimumLength(3)
            .MaximumLength(100);

        var categories = string.Join(", ", validCategories);

        RuleFor(dto => dto.Category)
            .Must(validCategories.Contains)
            .WithMessage($"Please provide a valid category: {categories}");

        // Advanced async category validation example:
        // If category validation had to use cache/database state, inject
        // ICategoryValidationService through this validator constructor and use CustomAsync.
        //
        // RuleFor(dto => dto.Category)
        //     .NotEmpty()
        //     .CustomAsync(async (category, context, cancellationToken) =>
        //     {
        //         var status = await categoryValidationService
        //             .ValidateCategoryAsync(category, cancellationToken);
        //
        //         if (status == CategoryValidationStatus.MissingInCache)
        //         {
        //             context.AddFailure(
        //                 nameof(CreateRestaurantDto.Category),
        //                 "Category was not found in cache.");
        //         }
        //
        //         if (status == CategoryValidationStatus.MissingInDatabase)
        //         {
        //             context.AddFailure(
        //                 nameof(CreateRestaurantDto.Category),
        //                 "Category does not exist in database.");
        //         }
        //     });

        // Those are redundant because fields are not nullable
        // so they are required by default
        //
        // RuleFor(dto => dto.Description)
        //     .NotEmpty()
        //     .WithMessage("Description is required");
        //
        // RuleFor(dto => dto.Category)
        //     .NotEmpty()
        //     .WithMessage("Please provide a valid category");

        RuleFor(dto => dto.ContactEmail)
            .EmailAddress()
            .WithMessage("Please provide a valid email address");

        RuleFor(dto => dto.PostalCode)
            .Matches(@"^\d{2}-\d{3}$")
            .WithMessage("Please provide a valid postal code (XX-XXX)");
    }
}

/*
Advanced category validation example:

If we wanted a more advanced case where category validation depends on cache/database state,
it could look like this. The real implementation should live in dedicated application services,
not inside the validator file.

public enum CategoryValidationStatus
{
    Valid,
    MissingInCache,
    MissingInDatabase
}

public enum CategoryCacheStatus
{
    Valid,
    FailedAttempt,
    MissingInDatabase
}

public interface ICategoryValidationService
{
    Task<CategoryValidationStatus> ValidateCategoryAsync(
        string? category,
        CancellationToken cancellationToken);
}

public interface ICategoryCache
{
    Task<CategoryCacheStatus?> GetCategoryStatusAsync(
        string category,
        CancellationToken cancellationToken);

    Task MarkCategoryAsFailedAttemptAsync(
        string category,
        CancellationToken cancellationToken);

    Task MarkCategoryAsValidAsync(
        string category,
        CancellationToken cancellationToken);

    Task MarkCategoryAsMissingInDatabaseAsync(
        string category,
        CancellationToken cancellationToken);
}

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(
        string category,
        CancellationToken cancellationToken);
}

public class CategoryValidationService(
    ICategoryCache cache,
    ICategoryRepository categoryRepository) : ICategoryValidationService
{
    public async Task<CategoryValidationStatus> ValidateCategoryAsync(
        string? category,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return CategoryValidationStatus.MissingInCache;
        }

        var normalizedCategory = category.Trim().ToLowerInvariant();

        var cachedStatus = await cache.GetCategoryStatusAsync(
            normalizedCategory,
            cancellationToken);

        if (cachedStatus is null)
        {
            await cache.MarkCategoryAsFailedAttemptAsync(
                normalizedCategory,
                cancellationToken);

            return CategoryValidationStatus.MissingInCache;
        }

        switch (cachedStatus)
        {
            case CategoryCacheStatus.Valid:
                return CategoryValidationStatus.Valid;

            case CategoryCacheStatus.FailedAttempt:
                var existsInDatabase = await categoryRepository.ExistsAsync(
                    normalizedCategory,
                    cancellationToken);

                if (existsInDatabase)
                {
                    await cache.MarkCategoryAsValidAsync(
                        normalizedCategory,
                        cancellationToken);

                    return CategoryValidationStatus.Valid;
                }

                await cache.MarkCategoryAsMissingInDatabaseAsync(
                    normalizedCategory,
                    cancellationToken);

                return CategoryValidationStatus.MissingInDatabase;

            case CategoryCacheStatus.MissingInDatabase:
            default:
                return CategoryValidationStatus.MissingInDatabase;
        }
    }
}
*/
