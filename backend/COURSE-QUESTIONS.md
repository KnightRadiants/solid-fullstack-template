# Course Questions

This file tracks implementation questions spotted during code review.
The goal is to revisit them after finishing the course and decide whether changes are needed.

## Restaurants - delete flow (CQRS)

1. Should `CancellationToken` be propagated through repository methods used by handlers?
   - Current `DeleteRestaurantCommandHandler` accepts `cancellationToken`, but repository methods called from it do not.
   - Question: is this intentionally simplified for the course stage, or should the repo contract include tokens now?

2. Should delete path load full restaurant graph (`Include(Dishes)`) before remove?
   - Current delete command fetches entity via `GetByIdAsync`, which includes `Dishes`.
   - Question: is eager loading intentional (domain rule/side effect), or would a lighter delete-oriented query be preferred?

3. What HTTP behavior should we target for deleting non-existing resources?
   - Current endpoint returns `404 NotFound` when entity does not exist.
   - Question: do we want strict `404`, or idempotent delete semantics (`204 NoContent` also when already absent)?

4. What is the expected cascade/restrict behavior for `Restaurant -> Dishes` on delete?
   - Current implementation relies on configured EF/database behavior.
   - Question: should this be explicitly documented and covered by integration tests?
