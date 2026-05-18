using SolidFullstackTemplate.Domain.Constants;
using SolidFullstackTemplate.Domain.Entities;

namespace SolidFullstackTemplate.Domain.Interfaces;

public interface IRestaurantAuthorizationService
{
    Task EnsureAuthorizedAsync(
        int restaurantId,
        ResourceOperation resourceOperation,
        CancellationToken cancellationToken = default);

    void EnsureAuthorized(Restaurant restaurant, ResourceOperation resourceOperation);
}
