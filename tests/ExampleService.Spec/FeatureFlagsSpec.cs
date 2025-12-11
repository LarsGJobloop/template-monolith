using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ExampleService.Domain.FeatureFlags;

namespace ExampleService.Spec.FeatureFlags;

public class CreateFeatureFlag : TestEnvironment
{
    [Fact]
    public async Task GivenValidFeatureFlagData_WhenCreatingFeatureFlag_ThenFeatureFlagIsCreated()
    {
        // Given valid feature flag data
        await InitializeAsync();
        var featureFlag = new
        {
            Key = "test_feature",
            Description = "A test feature flag",
            Enabled = true,
            RolloutPercentage = 50
        };

        // When creating feature flag
        var response = await Client.PostAsJsonAsync("/api/feature-flags", featureFlag);

        // Then feature flag is created
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<FeatureFlag>();
        Assert.NotNull(created);
        Assert.Equal("test_feature", created.Key);
        Assert.Equal("A test feature flag", created.Description);
        Assert.True(created.Enabled);
        Assert.Equal(50, created.RolloutPercentage);
        Assert.NotEqual(Guid.Empty, created.Id);
    }

    [Fact]
    public async Task GivenDuplicateKey_WhenCreatingFeatureFlag_ThenConflictIsReturned()
    {
        // Given a feature flag already exists
        await InitializeAsync();
        var featureFlag = new { Key = "duplicate_feature", Description = "First flag" };
        await Client.PostAsJsonAsync("/api/feature-flags", featureFlag);

        // When creating another with same key
        var duplicate = new { Key = "duplicate_feature", Description = "Duplicate flag" };
        var response = await Client.PostAsJsonAsync("/api/feature-flags", duplicate);

        // Then conflict is returned
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GivenInvalidRolloutPercentage_WhenCreatingFeatureFlag_ThenBadRequestIsReturned()
    {
        // Given invalid rollout percentage
        await InitializeAsync();
        var featureFlag = new
        {
            Key = "invalid_percentage",
            Description = "Invalid percentage",
            Enabled = true,
            RolloutPercentage = 150 // Invalid: over 100
        };

        // When creating feature flag
        var response = await Client.PostAsJsonAsync("/api/feature-flags", featureFlag);

        // Then bad request is returned
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public class GetFeatureFlags : TestEnvironment
{
    [Fact]
    public async Task GivenNoFeatureFlagsExist_WhenGettingAllFeatureFlags_ThenEmptyListIsReturned()
    {
        // Given no feature flags exist
        await InitializeAsync();

        // When getting all feature flags
        var response = await Client.GetAsync("/api/feature-flags");

        // Then empty list is returned
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var featureFlags = await response.Content.ReadFromJsonAsync<List<FeatureFlag>>();
        Assert.NotNull(featureFlags);
        Assert.Empty(featureFlags);
    }

    [Fact]
    public async Task GivenFeatureFlagsExist_WhenGettingAllFeatureFlags_ThenAllFeatureFlagsAreReturned()
    {
        // Given feature flags exist
        await InitializeAsync();
        var flag1 = new { Key = "feature_1", Description = "First feature" };
        var flag2 = new { Key = "feature_2", Description = "Second feature" };
        await Client.PostAsJsonAsync("/api/feature-flags", flag1);
        await Client.PostAsJsonAsync("/api/feature-flags", flag2);

        // When getting all feature flags
        var response = await Client.GetAsync("/api/feature-flags");

        // Then all feature flags are returned
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var featureFlags = await response.Content.ReadFromJsonAsync<List<FeatureFlag>>();
        Assert.NotNull(featureFlags);
        Assert.Equal(2, featureFlags.Count);
        Assert.Contains(featureFlags, f => f.Key == "feature_1");
        Assert.Contains(featureFlags, f => f.Key == "feature_2");
    }

    [Fact]
    public async Task GivenFeatureFlagExists_WhenGettingFeatureFlagById_ThenFeatureFlagIsReturned()
    {
        // Given a feature flag exists
        await InitializeAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/feature-flags",
            new { Key = "specific_feature", Description = "Specific feature" });
        var created = await createResponse.Content.ReadFromJsonAsync<FeatureFlag>();

        // When getting feature flag by id
        var response = await Client.GetAsync($"/api/feature-flags/{created.Id}");

        // Then feature flag is returned
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var retrieved = await response.Content.ReadFromJsonAsync<FeatureFlag>();
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal("specific_feature", retrieved.Key);
    }

    [Fact]
    public async Task GivenFeatureFlagDoesNotExist_WhenGettingFeatureFlagById_ThenNotFoundIsReturned()
    {
        // Given feature flag does not exist
        await InitializeAsync();
        var nonExistentId = Guid.NewGuid();

        // When getting feature flag by id
        var response = await Client.GetAsync($"/api/feature-flags/{nonExistentId}");

        // Then not found is returned
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public class UpdateFeatureFlag : TestEnvironment
{
    [Fact]
    public async Task GivenFeatureFlagExists_WhenUpdatingFeatureFlag_ThenFeatureFlagIsUpdated()
    {
        // Given a feature flag exists
        await InitializeAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/feature-flags",
            new { Key = "update_me", Description = "Original description", Enabled = false });
        var created = await createResponse.Content.ReadFromJsonAsync<FeatureFlag>();

        // When updating feature flag
        var update = new
        {
            Key = "update_me",
            Description = "Updated description",
            Enabled = true,
            RolloutPercentage = 75
        };
        var response = await Client.PutAsJsonAsync($"/api/feature-flags/{created.Id}", update);

        // Then feature flag is updated
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<FeatureFlag>();
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("Updated description", updated.Description);
        Assert.True(updated.Enabled);
        Assert.Equal(75, updated.RolloutPercentage);
    }

    [Fact]
    public async Task GivenFeatureFlagDoesNotExist_WhenUpdatingFeatureFlag_ThenNotFoundIsReturned()
    {
        // Given feature flag does not exist
        await InitializeAsync();
        var nonExistentId = Guid.NewGuid();
        var update = new { Key = "nonexistent", Description = "Updated" };

        // When updating feature flag
        var response = await Client.PutAsJsonAsync($"/api/feature-flags/{nonExistentId}", update);

        // Then not found is returned
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public class DeleteFeatureFlag : TestEnvironment
{
    [Fact]
    public async Task GivenFeatureFlagExists_WhenDeletingFeatureFlag_ThenFeatureFlagIsDeleted()
    {
        // Given a feature flag exists
        await InitializeAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/feature-flags",
            new { Key = "delete_me", Description = "To be deleted" });
        var created = await createResponse.Content.ReadFromJsonAsync<FeatureFlag>();

        // When deleting feature flag
        var deleteResponse = await Client.DeleteAsync($"/api/feature-flags/{created.Id}");

        // Then feature flag is deleted
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // And feature flag no longer exists
        var getResponse = await Client.GetAsync($"/api/feature-flags/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GivenFeatureFlagDoesNotExist_WhenDeletingFeatureFlag_ThenNotFoundIsReturned()
    {
        // Given feature flag does not exist
        await InitializeAsync();
        var nonExistentId = Guid.NewGuid();

        // When deleting feature flag
        var response = await Client.DeleteAsync($"/api/feature-flags/{nonExistentId}");

        // Then not found is returned
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
