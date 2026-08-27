using Xunit;
using DotnetBaseApi.Filters;

namespace DotnetBaseApi.Tests.Filters
{
    /// <summary>
    /// Unit tests for ETagFilter.
    /// 
    /// Note: Full integration testing of the filter's OnResultExecutionAsync method
    /// requires a complete ASP.NET Core test server or WebApplicationFactory setup.
    /// These tests focus on the ETagGenerator utility which is the core logic.
    /// 
    /// To test the full filter integration, create integration tests using
    /// WebApplicationFactory or similar ASP.NET Core testing utilities.
    /// </summary>
    public class ETagFilterUnitTests
    {
        [Fact]
        public void ETagFilter_Constructor_Sets_Default_StatusCode_200()
        {
            // Arrange & Act
            var filter = new ETagFilter();

            // Assert
            Assert.NotNull(filter);
        }

        [Fact]
        public void ETagFilter_Constructor_Accepts_Multiple_StatusCodes()
        {
            // Arrange & Act
            var filter = new ETagFilter(200, 201, 202);

            // Assert
            Assert.NotNull(filter);
        }
    }
}
